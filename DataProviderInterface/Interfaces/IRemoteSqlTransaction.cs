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
		ConnectionId GetConnection(TransactionId transactionId);
		[OperationContract]
		IsolationLevel GetIsolationLevel(TransactionId transactionId);

		[OperationContract]
		void Commit(TransactionId transactionId);
		[OperationContract]
		void Rollback(TransactionId transactionId);
		[OperationContract(Name = "DisposeTransaction")]
		void Dispose(TransactionId transactionId);
	}
}