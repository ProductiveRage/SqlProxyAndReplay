using System;
using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class SqlReplayerConnection : IDbConnection
	{
		private readonly Func<QueryCriteria, IDataReader> _dataRetriever;
		private readonly Func<QueryCriteria, Tuple<object>> _scalarDataRetriever;
		public SqlReplayerConnection(Func<QueryCriteria, IDataReader> dataRetriever, Func<QueryCriteria, Tuple<object>> scalarDataRetriever)
		{
			if (dataRetriever == null)
				throw new ArgumentNullException(nameof(dataRetriever));
			if (scalarDataRetriever == null)
				throw new ArgumentNullException(nameof(scalarDataRetriever));

			_dataRetriever = dataRetriever;
			_scalarDataRetriever = scalarDataRetriever;
		}

		public string ConnectionString { get; set; }

		public int ConnectionTimeout
		{
			get
			{
				throw new NotImplementedException(); // TODO
			}
		}

		public string Database
		{
			get
			{
				throw new NotImplementedException(); // TODO
			}
		}

		public ConnectionState State { get { return ConnectionState.Open; } } // TODO: This should be alright..(?)

		public SqlReplayerTransaction BeginTransaction(IsolationLevel il = IsolationLevel.ReadCommitted) { return new SqlReplayerTransaction(this, il); }
		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il) { return BeginTransaction(il); }
		IDbTransaction IDbConnection.BeginTransaction() { return BeginTransaction(); }

		public void ChangeDatabase(string databaseName)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Open() { }
		public void Close() { }
		public void Dispose() { }

		public SqlReplayerCommand CreateCommand() { return new SqlReplayerCommand(this, _dataRetriever, _scalarDataRetriever); }
		IDbCommand IDbConnection.CreateCommand() { return CreateCommand(); }
	}
}