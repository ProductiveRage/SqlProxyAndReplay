using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface IRemoteSqlParameterSet
	{
		[OperationContract]
		int Add(CommandId commandId, ParameterId parameterId);

		[OperationContract]
		ParameterId GetParameterByIndex(CommandId commandId, int index);
		[OperationContract]
		void SetParameterByIndex(CommandId commandId, int index, ParameterId parameterId);

		[OperationContract]
		ParameterId GetParameterByName(CommandId commandId, string parameterName);

		[OperationContract]
		void SetParameterByName(CommandId commandId, string parameterName, ParameterId parameterId);

		[OperationContract]
		int GetCount(CommandId commandId);

		[OperationContract]
		void Clear(CommandId commandId);

		[OperationContract(Name = "ContainsParameterWithId")]
		bool Contains(CommandId commandId, ParameterId parameterId);
		[OperationContract(Name = "ContainsParameterWithName")]
		bool Contains(CommandId commandId, string parameterName);

		[OperationContract(Name = "IndexOfParameterById")]
		int IndexOf(CommandId commandId, ParameterId parameterId);
		[OperationContract(Name = "IndexOfParameterByName")]
		int IndexOf(CommandId commandId, string parameterName);

		[OperationContract]
		void Insert(CommandId commandId, int index, ParameterId parameterId);

		[OperationContract]
		void Remove(CommandId commandId, ParameterId parameterId);
		[OperationContract(Name = "RemoveParameterByIndex")]
		void RemoveAt(CommandId commandId, int index);
		[OperationContract(Name = "RemoveParameterByName")]
		void RemoveAt(CommandId commandId, string parameterName);
	}
}
