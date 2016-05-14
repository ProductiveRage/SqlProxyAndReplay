using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface IRemoteSqlTransaction
	{
		[OperationContract(Name = "GetTransactionConnection")]
		ConnectionId GetConnection(Guid transactionId);
		[OperationContract]
		IsolationLevel GetIsolationLevel(Guid transactionId);

		[OperationContract]
		void Commit(Guid transactionId);
		[OperationContract]
		void Rollback(Guid transactionId);
		[OperationContract(Name = "DisposeTransaction")]
		void Dispose(Guid transactionId);
	}
}