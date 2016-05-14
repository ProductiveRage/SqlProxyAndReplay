using System;
using System.Data;
using System.ServiceModel;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	[ServiceContract]
	public interface IRemoteSqlTransaction
	{
		[OperationContract]
		Guid GetConnection(Guid transactionId);
		[OperationContract]
		IsolationLevel GetIsolationLevel(Guid transactionId);

		[OperationContract]
		void Commit(Guid transactionId);
		[OperationContract]
		void Rollback(Guid transactionId);
		[OperationContract]
		void Dispose(Guid transactionId);
	}
}