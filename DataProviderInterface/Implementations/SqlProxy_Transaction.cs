using System;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public ConnectionId GetConnection(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public IsolationLevel GetIsolationLevel(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Commit(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
		public void Rollback(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
		public void Dispose(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
	}
}