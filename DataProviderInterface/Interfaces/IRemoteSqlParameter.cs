using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface IRemoteSqlParameter
	{
		[OperationContract]
		ParameterDirection GetDirection(ParameterId parameterId);
		[OperationContract]
		void SetDirection(ParameterId parameterId, ParameterDirection value);

		[OperationContract]
		DbType GetDbType(ParameterId parameterId);
		[OperationContract]
		void SetDbType(ParameterId parameterId, DbType value);

		[OperationContract]
		byte GetPrecision(ParameterId parameterId);
		[OperationContract]
		void SetPrecision(ParameterId parameterId, byte value);

		[OperationContract]
		byte GetScale(ParameterId parameterId);
		[OperationContract]
		void SetScale(ParameterId parameterId, byte value);

		[OperationContract]
		int GetSize(ParameterId parameterId);
		[OperationContract]
		void SetSize(ParameterId parameterId, int value);

		[OperationContract]
		bool GetIsNullable(ParameterId parameterId);

		[OperationContract]
		string GetParameterName(ParameterId parameterId);
		[OperationContract]
		void SetParameterName(ParameterId parameterId, string value);

		[OperationContract]
		string GetSourceColumn(ParameterId parameterId);
		[OperationContract]
		void SetSourceColumn(ParameterId parameterId, string value);

		[OperationContract]
		DataRowVersion GetSourceVersion(ParameterId parameterId);
		[OperationContract]
		void SetSourceVersion(ParameterId parameterId, DataRowVersion value);

		[OperationContract(Name = "GetParameterValue")]
		object GetValue(ParameterId parameterId);
		[OperationContract(Name = "SetParameterValue")]
		void SetValue(ParameterId parameterId, object value);
	}
}