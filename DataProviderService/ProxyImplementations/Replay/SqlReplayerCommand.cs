using System;
using System.Data;
using System.Linq;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay
{
	public sealed class SqlReplayerCommand : IDbCommand
	{
		private readonly SqlReplayerConnection _connection;
		private readonly Func<QueryCriteria, IDataReader> _dataRetriever;
		private readonly Func<QueryCriteria, Tuple<object>> _scalarDataRetriever;
		private readonly Func<QueryCriteria, int?> _nonQueryRowCountDataRetriever;
		public SqlReplayerCommand(
			SqlReplayerConnection connection,
			Func<QueryCriteria, IDataReader> dataRetriever,
			Func<QueryCriteria, Tuple<object>> scalarDataRetriever,
			Func<QueryCriteria, int?> nonQueryRowCountDataRetriever)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (dataRetriever == null)
				throw new ArgumentNullException(nameof(dataRetriever));
			if (scalarDataRetriever == null)
				throw new ArgumentNullException(nameof(scalarDataRetriever));
			if (nonQueryRowCountDataRetriever == null)
				throw new ArgumentNullException(nameof(nonQueryRowCountDataRetriever));

			_connection = connection;
			_dataRetriever = dataRetriever;
			_scalarDataRetriever = scalarDataRetriever;
			_nonQueryRowCountDataRetriever = nonQueryRowCountDataRetriever;
			Parameters = new SqlReplayerParameterCollection();

			// Apply some sensible defaults - important for properties whose default state is invalid (eg. there is no zero value for the CommandType
			// enum) but more important that the property defaults here match those on SqlParameter for when the replayer aspect of the services comes
			// to look in its cache (it will check that the connection, command and parameters are consistent between the current request and any cache
			// entry, the type of command - eg. Text or StoredProcedure - is important)
			CommandType = CommandType.Text;
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
			var rowCount = _nonQueryRowCountDataRetriever(new QueryCriteria(
				_connection.ConnectionString,
				CommandText,
				CommandType,
				Parameters.Select(p => new QueryCriteria.ParameterInformation(p.ParameterName, p.Value, p.DbType, p.IsNullable, p.Direction, p.Scale, p.Size))
			));
			if (rowCount == null)
				throw new Exception("Data is not available for this query");
			return rowCount.Value;
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
			var data = _scalarDataRetriever(new QueryCriteria(
				_connection.ConnectionString,
				CommandText,
				CommandType,
				Parameters.Select(p => new QueryCriteria.ParameterInformation(p.ParameterName, p.Value, p.DbType, p.IsNullable, p.Direction, p.Scale, p.Size))
			));
			if (data == null)
				throw new Exception("Data is not available for this query");
			return data.Item1;
		}

		public void Prepare() { }
		public void Cancel() { }
		public void Dispose() { }
	}
}
