using System;
using System.Data;
using System.ServiceModel;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	[ServiceContract]
	public interface IRemoteSqlCommand
	{
		[OperationContract]
		Guid GetNewCommandId(Guid connectionId);

		[OperationContract]
		string GetCommandText(Guid commandId);

		[OperationContract]
		void SetCommandText(Guid commandId, string value);

		[OperationContract]
		int GetCommandTimeout(Guid commandId);
		[OperationContract]
		void SetCommandTimeout(Guid commandId, int value);

		[OperationContract]
		CommandType GetCommandType(Guid commandId);
		[OperationContract]
		void SetCommandType(Guid commandId, CommandType value);

		[OperationContract(Name = "GetCommandConnection")]
		Guid? GetConnection(Guid commandId);
		[OperationContract(Name = "SetCommandConnection")]
		void SetConnection(Guid commandId, Guid? connectionId);

		/* TODO
		[OperationContract]
		IDbDataParameter CreateParameter(); // TODO: Wrap
		[OperationContract]
		IRemoteDataParameterCollection GetParameters();
		*/

		[OperationContract]
		Guid? GetTransaction(Guid commandId);
		[OperationContract]
		void SetTransaction(Guid commandId, Guid? transactionId);
		[OperationContract]
		UpdateRowSource GetUpdatedRowSource(Guid commandId);
		[OperationContract]
		void SetUpdatedRowSource(Guid commandId, UpdateRowSource value);

		[OperationContract]
		void Prepare(Guid commandId);
		[OperationContract]
		void Cancel(Guid commandId);
		[OperationContract(Name = "DisposeCommand")]
		void Dispose(Guid commandId);

		[OperationContract]
		int ExecuteNonQuery(Guid commandId);
		[OperationContract]
		object ExecuteScalar(Guid commandId);
		[OperationContract]
		Guid ExecuteReader(Guid commandId, CommandBehavior behavior = CommandBehavior.Default);
	}
}