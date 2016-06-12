using System;
using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay
{
	public sealed class SqlReplayerTransaction : IDbTransaction
	{
		public SqlReplayerTransaction(SqlReplayerConnection connection, IsolationLevel isolationLevel)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			Connection = connection;
			IsolationLevel = IsolationLevel;
		}

		public IDbConnection Connection { get; }
		public IsolationLevel IsolationLevel { get; }

		public void Commit() { }
		public void Rollback() { }
		public void Dispose() { }
	}
}
