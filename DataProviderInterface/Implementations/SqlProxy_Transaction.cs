using System;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public ConnectionId GetConnection(TransactionId transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public IsolationLevel GetIsolationLevel(TransactionId transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Commit(TransactionId transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
		public void Rollback(TransactionId transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
		public void Dispose(TransactionId transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
	}
}