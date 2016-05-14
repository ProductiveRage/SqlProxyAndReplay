using System;
using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public Guid GetConnection(Guid transactionId)
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