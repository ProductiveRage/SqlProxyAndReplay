using System;
using System.ServiceModel;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	[ServiceContract]
	public interface IRemoteSqlDataReader
	{
		[OperationContract]
		bool Read(Guid readerId);
		[OperationContract]
		bool NextResult(Guid readerId);
		[OperationContract(Name = "CloseReader")]
		void Close(Guid readerId);
		[OperationContract(Name = "DisposeReader")]
		void Dispose(Guid readerId);

		[OperationContract]
		int GetDepth(Guid readerId);
		[OperationContract]
		int GetFieldCount(Guid readerId);
		[OperationContract]
		bool GetIsClosed(Guid readerId);
		[OperationContract]
		int GetRecordsAffected(Guid readerId);

		[OperationContract]
		bool IsDBNull(Guid readerId, int i);

		[OperationContract]
		bool GetBoolean(Guid readerId, int i);
		[OperationContract]
		byte GetByte(Guid readerId, int i);
		[OperationContract]
		Tuple<long, byte[]> GetBytes(Guid readerId, int i, long fieldOffset, byte[] buffer, int bufferoffset, int length);
		[OperationContract]
		char GetChar(Guid readerId, int i);
		[OperationContract]
		Tuple<long, char[]> GetChars(Guid readerId, int i, long fieldoffset, char[] buffer, int bufferoffset, int length);
		[OperationContract]
		Guid GetData(Guid readerId, int i);
		[OperationContract]
		string GetDataTypeName(Guid readerId, int i);
		[OperationContract]
		DateTime GetDateTime(Guid readerId, int i);
		[OperationContract]
		decimal GetDecimal(Guid readerId, int i);
		[OperationContract]
		double GetDouble(Guid readerId, int i);
		[OperationContract]
		string GetFieldType(Guid readerId, int i);
		[OperationContract]
		float GetFloat(Guid readerId, int i);
		[OperationContract]
		Guid GetGuid(Guid readerId, int i);
		[OperationContract]
		short GetInt16(Guid readerId, int i);
		[OperationContract]
		int GetInt32(Guid readerId, int i);
		[OperationContract]
		long GetInt64(Guid readerId, int i);
		[OperationContract]
		string GetName(Guid readerId, int i);
		[OperationContract]
		int GetOrdinal(Guid readerId, string name);
		[OperationContract]
		string GetString(Guid readerId, int i);
		[OperationContract]
		object GetValue(Guid readerId, int i);
		[OperationContract]
		int GetValues(Guid readerId, object[] values);
	}
}