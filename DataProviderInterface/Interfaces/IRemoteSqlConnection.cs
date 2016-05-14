using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface IRemoteSqlConnection
	{
		[OperationContract]
		ConnectionId GetNewConnectionId();

		[OperationContract]
		string GetConnectionString(ConnectionId connectionId);
		[OperationContract]
		void SetConnectionString(ConnectionId connectionId, string value);
		[OperationContract]
		int GetConnectionTimeout(ConnectionId connectionId);
		[OperationContract]
		string GetDatabase(ConnectionId connectionId);
		[OperationContract]
		ConnectionState GetState(ConnectionId connectionId);

		[OperationContract]
		void ChangeDatabase(ConnectionId connectionId, string databaseName);

		[OperationContract]
		void Open(ConnectionId connectionId);
		[OperationContract(Name = "CloseConnection")]
		void Close(ConnectionId connectionId);
		[OperationContract(Name = "DisposeConnection")]
		void Dispose(ConnectionId connectionId);

		[OperationContract(Name = "BeginTransactionWithDefaultIsolationLevel")]
		TransactionId BeginTransaction(ConnectionId connectionId);
		[OperationContract]
		TransactionId BeginTransaction(ConnectionId connectionId, IsolationLevel il);
	}
}