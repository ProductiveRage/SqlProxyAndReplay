using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public string GetCommandText(CommandId commandId) { return _commandStore.Get(commandId).CommandText; }
		public void SetCommandText(CommandId commandId, string value) { _commandStore.Get(commandId).CommandText = value; }

		public int GetCommandTimeout(CommandId commandId) { return _commandStore.Get(commandId).CommandTimeout; }
		public void SetCommandTimeout(CommandId commandId, int value) { _commandStore.Get(commandId).CommandTimeout = value; }

		public CommandType GetCommandType(CommandId commandId) { return _commandStore.Get(commandId).CommandType; }
		public void SetCommandType(CommandId commandId, CommandType value) { _commandStore.Get(commandId).CommandType = value; }

		public ConnectionId? GetConnection(CommandId commandId)
		{
			var command = _commandStore.Get(commandId);
			if (command.Connection == null)
				return null;
			var sqlConnection = command.Connection as SqlConnection;
			if (sqlConnection == null)
				throw new Exception("All connnections should be of type SqlConnection, but this one is \"" + command.Connection.GetType() + "\")");
			return _connectionStore.GetIdFor(sqlConnection);
		}
		public void SetConnection(CommandId commandId, ConnectionId? connectionId)
		{
			var command = _commandStore.Get(commandId);
			if (connectionId == null)
				command.Connection = null;
			else
				command.Connection = _connectionStore.Get(connectionId.Value);
		}

		public ParameterId CreateParameter(CommandId commandId)
		{
			// Note: We need to track created parameters and tidy them up when the command is disposed (because parameters are not disposable and so
			// we won't be told via a Dispose call when parameters are no longer required; we know that they're no longer required when the command
			// that owns them is disposed, though)
			var parameterId = _parameterStore.Add(_commandStore.Get(commandId).CreateParameter());
			_parametersToTidy.Record(commandId, parameterId);
			return parameterId;
		}

		public TransactionId? GetTransaction(CommandId commandId)
		{
			var command = _commandStore.Get(commandId);
			if (command.Transaction == null)
				return null;
			var sqlTransaction = command.Transaction as SqlTransaction;
			if (sqlTransaction == null)
				throw new Exception("All connnections should be of type SqlTransaction, but this one is \"" + command.Transaction.GetType() + "\")");
			return _transactionStore.GetIdFor(sqlTransaction);
		}

		public void SetTransaction(CommandId commandId, TransactionId? transactionId)
		{
			var command = _commandStore.Get(commandId);
			if (transactionId == null)
				command.Transaction = null;
			else
				command.Transaction = _transactionStore.Get(transactionId.Value);
		}

		public UpdateRowSource GetUpdatedRowSource(CommandId commandId) { return _commandStore.Get(commandId).UpdatedRowSource; }
		public void SetUpdatedRowSource(CommandId commandId, UpdateRowSource value) { _commandStore.Get(commandId).UpdatedRowSource = value; }

		public void Prepare(CommandId commandId)
		{
			_commandStore.Get(commandId).Prepare();
		}
		public void Cancel(CommandId commandId)
		{
			_commandStore.Get(commandId).Cancel();
		}
		public void Dispose(CommandId commandId)
		{
			// Parameters are not individually disposed of when use of them is complete - instead, the entries from the parameter store must
			// be removed when the command that created them is disposed of
			_commandStore.Get(commandId).Dispose();
			_parametersToTidy.RemoveAnyParametersFor(
				commandId,
				parameterId => _parameterStore.Remove(parameterId)
			);
			_commandStore.Remove(commandId);
		}

		public int ExecuteNonQuery(CommandId commandId)
		{
			ThrowForAnyNonInputOnlyParameters(commandId);
			var command = _commandStore.Get(commandId);
			return command.ExecuteNonQuery();
		}
		public object ExecuteScalar(CommandId commandId)
		{
			ThrowForAnyNonInputOnlyParameters(commandId);
			var queryCriteria = TryToGetQueryCriteria(commandId);
			if (queryCriteria != null)
				_queryRecorder(queryCriteria);
			return _commandStore.Get(commandId).ExecuteScalar();
		}
		public DataReaderId ExecuteReader(CommandId commandId, CommandBehavior behavior = CommandBehavior.Default)
		{
			ThrowForAnyNonInputOnlyParameters(commandId);
			var queryCriteria = TryToGetQueryCriteria(commandId);
			if (queryCriteria != null)
				_queryRecorder(queryCriteria);
			var reader = _commandStore.Get(commandId).ExecuteReader(behavior);
			try
			{
				return _readerStore.Add(reader);
			}
			catch
			{
				reader.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Since that aim of this library is to capture SQL statements (and their connection strings, parameters, etc..) and cache their results against
		/// these query criteria so that they can be replayed from this cache (removing the database from the equation), it is important that it is possible
		/// to emulate the behaviour of the database. Parameters that may be changed by the database are complicated since they may be changed at any point
		/// during execution - after one record has been returned, for example, or after two or after all data has been returned (or a single parameter's
		/// value may change multiple times during execution). This is difficult to simulate if the "replay" stand-in just records the data that should be
		/// returned. As such, at this time, only Input parameters are supported.
		/// </summary>
		private void ThrowForAnyNonInputOnlyParameters(CommandId commandId)
		{
			var command = _commandStore.Get(commandId);
			if (command == null)
			{
				// As with TryToGetQueryCriteria, ignore this invalid state for now and let things blow up when the execute-SQL attempt is made (there
				// is an outside chance that the command and/or its parameters could be changed on a different thread between this point and the point
				// at which the execute-SQL call is attempted but there's little that can done about that - the SQL connection / command / etc.. classes
				// are not thread-safe and so there should be no expectation that all will go well if this is not respected)
				return;
			}

			var nonInputParameterNames = ToEnumerable(command.Parameters)
				.Where(p => p.Direction != ParameterDirection.Input)
				.Select(p => p.ParameterName);
			if (nonInputParameterNames.Any())
				throw new Exception($"Only Input parameters are supported (the following do not meet this criteria: {string.Join(", ", nonInputParameterNames)})");
		}

		/// <summary>
		/// This will return null if the commandId can not be mapped to a command or if that command has no connection - rather than throw an exception
		/// in this method, it's better to leave the real exception that will follow to blow up (this method will be called before an ExecuteReader or
		/// ExecuteScalar call and they will fail if there is no connection)
		/// </summary>
		private QueryCriteria TryToGetQueryCriteria(CommandId commandId)
		{
			var command = _commandStore.Get(commandId);
			if (command == null)
				return null;

			var connection = command.Connection;
			if (connection == null)
				return null;

			return new QueryCriteria(
				connection.ConnectionString,
				command.CommandText,
				command.CommandType,
				ToEnumerable(command.Parameters).Select(p => new QueryCriteria.ParameterInformation(p.ParameterName, p.Value, p.DbType, p.IsNullable, p.Direction, p.Scale, p.Size))
			);
		}

		private static IEnumerable<IDbDataParameter> ToEnumerable(IDataParameterCollection parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			return Enumerable.Range(0, parameters.Count)
				.Select(i => parameters[i])
				.Cast<IDbDataParameter>();
		}
	}
}