using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public ConnectionId GetConnection(TransactionId transactionId) { return _connectionStore.GetIdFor(_transactionStore.Get(transactionId).Connection); }

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