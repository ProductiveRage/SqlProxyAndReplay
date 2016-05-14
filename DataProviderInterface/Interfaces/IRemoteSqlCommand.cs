using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface IRemoteSqlCommand
	{
		[OperationContract]
		CommandId GetNewCommandId(ConnectionId connectionId);

		[OperationContract]
		string GetCommandText(CommandId commandId);

		[OperationContract]
		void SetCommandText(CommandId commandId, string value);

		[OperationContract]
		int GetCommandTimeout(CommandId commandId);
		[OperationContract]
		void SetCommandTimeout(CommandId commandId, int value);

		[OperationContract]
		CommandType GetCommandType(CommandId commandId);
		[OperationContract]
		void SetCommandType(CommandId commandId, CommandType value);

		[OperationContract(Name = "GetCommandConnection")]
		ConnectionId GetConnection(CommandId commandId);
		[OperationContract(Name = "SetCommandConnection")]
		void SetConnection(CommandId commandId, ConnectionId optionalConnectionId);

		/* TODO
		[OperationContract]
		IDbDataParameter CreateParameter(); // TODO: Wrap
		[OperationContract]
		IRemoteDataParameterCollection GetParameters();
		*/

		[OperationContract]
		TransactionId GetTransaction(CommandId commandId);
		[OperationContract]
		void SetTransaction(CommandId commandId, TransactionId optionalTransactionId);
		[OperationContract]
		UpdateRowSource GetUpdatedRowSource(CommandId commandId);
		[OperationContract]
		void SetUpdatedRowSource(CommandId commandId, UpdateRowSource value);

		[OperationContract]
		void Prepare(CommandId commandId);
		[OperationContract]
		void Cancel(CommandId commandId);
		[OperationContract(Name = "DisposeCommand")]
		void Dispose(CommandId commandId);

		[OperationContract]
		int ExecuteNonQuery(CommandId commandId);
		[OperationContract]
		object ExecuteScalar(CommandId commandId);
		[OperationContract]
		DataReaderId ExecuteReader(CommandId commandId, CommandBehavior behavior = CommandBehavior.Default);
	}
}