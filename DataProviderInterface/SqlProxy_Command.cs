using System;
using System.Data;
using System.Data.SqlClient;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public Guid GetNewCommandId(Guid connectionId)
		{
			return _commandStore.Add(new SqlCommand
			{
				Connection = _connectionStore.Get(connectionId)
			});
		}

		public string GetCommandText(Guid commandId) { return _commandStore.Get(commandId).CommandText; }
		public void SetCommandText(Guid commandId, string value) { _commandStore.Get(commandId).CommandText = value; }

		public int GetCommandTimeout(Guid commandId) { return _commandStore.Get(commandId).CommandTimeout; }
		public void SetCommandTimeout(Guid commandId, int value) { _commandStore.Get(commandId).CommandTimeout = value; }

		public CommandType GetCommandType(Guid commandId) { return _commandStore.Get(commandId).CommandType; }
		public void SetCommandType(Guid commandId, CommandType value) { _commandStore.Get(commandId).CommandType = value; }

		Guid? IRemoteSqlCommand.GetConnection(Guid commandId) // TODO: Use typed ids to avoid explicitly-implementing interface methods?
		{
			var command = _commandStore.Get(commandId);
			if (command.Connection == null)
				return null;
			var sqlConnection = command.Connection as SqlConnection;
			if (sqlConnection == null)
				throw new Exception("All connnections should be of type SqlConnection, but this one is \"" + command.Connection.GetType() + "\")");
			return _connectionStore.GetIdFor(sqlConnection);
		}
		public void SetConnection(Guid commandId, Guid? connectionId)
		{
			var command = _commandStore.Get(commandId);
			if (connectionId == null)
				command.Connection = null;
			else
				command.Connection = _connectionStore.Get(connectionId.Value);
		}

		public IDbDataParameter CreateParameter()
		{
			throw new NotImplementedException(); // TODO
		}

		public IRemoteDataParameterCollection GetParameters()
		{
			throw new NotImplementedException(); // TODO
		}

		public Guid? GetTransaction(Guid commandId)
		{
			var command = _commandStore.Get(commandId);
			if (command.Transaction == null)
				return null;
			var sqlTransaction = command.Transaction as SqlTransaction;
			if (sqlTransaction == null)
				throw new Exception("All connnections should be of type SqlTransaction, but this one is \"" + command.Transaction.GetType() + "\")");
			return _transactionStore.GetIdFor(sqlTransaction);
		}

		public void SetTransaction(Guid commandId, Guid? transactionId)
		{
			var command = _commandStore.Get(commandId);
			if (transactionId == null)
				command.Transaction = null;
			else
				command.Transaction = _transactionStore.Get(transactionId.Value);
		}

		public UpdateRowSource GetUpdatedRowSource(Guid commandId) { return _commandStore.Get(commandId).UpdatedRowSource; }
		public void SetUpdatedRowSource(Guid commandId, UpdateRowSource value) { _commandStore.Get(commandId).UpdatedRowSource = value; }

		public void Prepare(Guid commandId)
		{
			_commandStore.Get(commandId).Prepare();
		}
		public void Cancel(Guid commandId)
		{
			_commandStore.Get(commandId).Cancel();
		}
		void IRemoteSqlCommand.Dispose(Guid commandId) // TODO: Use typed ids to avoid explicitly-implementing interface methods?
		{
			_commandStore.Get(commandId).Dispose();
			_commandStore.Remove(commandId);

		}

		public int ExecuteNonQuery(Guid commandId)
		{
			return _commandStore.Get(commandId).ExecuteNonQuery();
		}
		public object ExecuteScalar(Guid commandId)
		{
			return _commandStore.Get(commandId).ExecuteScalar();
		}
		public Guid ExecuteReader(Guid commandId, CommandBehavior behavior = CommandBehavior.Default)
		{
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
	}
}