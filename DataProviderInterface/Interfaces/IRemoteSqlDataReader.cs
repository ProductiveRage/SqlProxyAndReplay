using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface IRemoteSqlDataReader
	{
		[OperationContract]
		bool Read(DataReaderId readerId);
		[OperationContract]
		bool NextResult(DataReaderId readerId);
		[OperationContract(Name = "CloseReader")]
		void Close(DataReaderId readerId);
		[OperationContract(Name = "DisposeReader")]
		void Dispose(DataReaderId readerId);

		[OperationContract]
		int GetDepth(DataReaderId readerId);
		[OperationContract]
		int GetFieldCount(DataReaderId readerId);
		[OperationContract]
		bool GetIsClosed(DataReaderId readerId);
		[OperationContract]
		int GetRecordsAffected(DataReaderId readerId);

		[OperationContract]
		bool IsDBNull(DataReaderId readerId, int i);

		[OperationContract]
		bool GetBoolean(DataReaderId readerId, int i);
		[OperationContract]
		byte GetByte(DataReaderId readerId, int i);
		[OperationContract]
		Tuple<long, byte[]> GetBytes(DataReaderId readerId, int i, long fieldOffset, byte[] buffer, int bufferoffset, int length);
		[OperationContract]
		char GetChar(DataReaderId readerId, int i);
		[OperationContract]
		Tuple<long, char[]> GetChars(DataReaderId readerId, int i, long fieldoffset, char[] buffer, int bufferoffset, int length);
		[OperationContract]
		DataReaderId GetData(DataReaderId readerId, int i);
		[OperationContract]
		string GetDataTypeName(DataReaderId readerId, int i);
		[OperationContract]
		DateTime GetDateTime(DataReaderId readerId, int i);
		[OperationContract]
		decimal GetDecimal(DataReaderId readerId, int i);
		[OperationContract]
		double GetDouble(DataReaderId readerId, int i);
		[OperationContract]
		string GetFieldType(DataReaderId readerId, int i);
		[OperationContract]
		float GetFloat(DataReaderId readerId, int i);
		[OperationContract]
		Guid GetGuid(DataReaderId readerId, int i);
		[OperationContract]
		short GetInt16(DataReaderId readerId, int i);
		[OperationContract]
		int GetInt32(DataReaderId readerId, int i);
		[OperationContract]
		long GetInt64(DataReaderId readerId, int i);
		[OperationContract]
		string GetName(DataReaderId readerId, int i);
		[OperationContract]
		int GetOrdinal(DataReaderId readerId, string name);
		[OperationContract]
		DataTable GetSchemaTable(DataReaderId readerId);
		[OperationContract]
		string GetString(DataReaderId readerId, int i);
		[OperationContract]
		object GetValue(DataReaderId readerId, int i);
		[OperationContract]
		Tuple<int, object[]> GetValues(DataReaderId readerId, object[] values);
	}
}