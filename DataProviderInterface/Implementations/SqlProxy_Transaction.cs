using System;
using System.Data;
using System.Data.SqlClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public ConnectionId GetConnection(TransactionId transactionId)
		{
			var transaction = _transactionStore.Get(transactionId);
			var sqlConnection = transaction.Connection as SqlConnection;
			if (sqlConnection == null)
				throw new Exception("All connnections should be of type SqlConnection, but this one is \"" + transaction.Connection.GetType() + "\")");
			return _connectionStore.GetIdFor(sqlConnection);
		}

		public IsolationLevel GetIsolationLevel(TransactionId transactionId) { return _transactionStore.Get(transactionId).IsolationLevel; }

		public void Commit(TransactionId transactionId) { _transactionStore.Get(transactionId).Commit(); }
		public void Rollback(TransactionId transactionId) { _transactionStore.Get(transactionId).Commit(); }

		public void Dispose(TransactionId transactionId)
		{
			_transactionStore.Get(transactionId).Commit();
			_transactionStore.Remove(transactionId);
		}
	}
}