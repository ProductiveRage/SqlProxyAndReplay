using System;
using System.Data;
using System.Linq;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class SqlReplayerCommand : IDbCommand
	{
		private readonly Func<QueryCriteria, IDataReader> _dataRetriever;
		private readonly SqlReplayerConnection _connection;
		public SqlReplayerCommand(SqlReplayerConnection connection, Func<QueryCriteria, IDataReader> dataRetriever)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (dataRetriever == null)
				throw new ArgumentNullException(nameof(dataRetriever));

			_connection = connection;
			_dataRetriever = dataRetriever;
			Parameters = new SqlReplayerParameterCollection();
		}

		public string CommandText { get; set; }
		public int CommandTimeout { get; set; }
		public CommandType CommandType { get; set; }
		public IDbConnection Connection { get; set; }
		public SqlReplayerParameterCollection Parameters { get; }
		IDataParameterCollection IDbCommand.Parameters { get { return Parameters; } }

		public IDbTransaction Transaction { get; set; }
		public UpdateRowSource UpdatedRowSource { get; set; }

		public SqlReplayerParameter CreateParameter() { return new SqlReplayerParameter(); }
		IDbDataParameter IDbCommand.CreateParameter() { return CreateParameter(); }

		public int ExecuteNonQuery()
		{
			throw new NotImplementedException(); // TODO
		}

		public IDataReader ExecuteReader() { return ExecuteReader(CommandBehavior.Default); }
		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			// We don't want to hit the database here, this data should be cached somewhere for replay - the _dataRetriever should retrieve data that matches
			// the current query criteria (connection string, sql text, etc..), if it returns null then we can't do anything useful here (so throw)
			var data = _dataRetriever(new QueryCriteria(
				_connection.ConnectionString,
				CommandText,
				CommandType,
				Parameters.Select(p => new QueryCriteria.ParameterInformation(p.ParameterName, p.Value, p.DbType, p.IsNullable, p.Direction, p.Scale, p.Size))
			));
			if (data == null)
				throw new Exception("Data is not available for this query");
			return data;
		}

		public object ExecuteScalar()
		{
			throw new NotImplementedException(); // TODO
		}

		public void Prepare() { }
		public void Cancel() { }
		public void Dispose() { }
	}
}
