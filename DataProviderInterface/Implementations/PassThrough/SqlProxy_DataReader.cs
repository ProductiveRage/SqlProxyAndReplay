using System;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public bool Read(DataReaderId readerId) { return _readerStore.Get(readerId).Read(); }
		public bool NextResult(DataReaderId readerId) { return _readerStore.Get(readerId).NextResult(); }
		public void Close(DataReaderId readerId) { _readerStore.Get(readerId).Close(); }
		public void Dispose(DataReaderId readerId)
		{
			_readerStore.Get(readerId).Dispose();
			_readerStore.Remove(readerId);
		}

		public int GetDepth(DataReaderId readerId) { return _readerStore.Get(readerId).Depth; }
		public int GetFieldCount(DataReaderId readerId) { return _readerStore.Get(readerId).FieldCount; }
		public bool GetIsClosed(DataReaderId readerId) { return _readerStore.Get(readerId).IsClosed; }
		public int GetRecordsAffected(DataReaderId readerId) { return _readerStore.Get(readerId).RecordsAffected; }

		public bool IsDBNull(DataReaderId readerId, int i) { return _readerStore.Get(readerId).IsDBNull(i); }

		public bool GetBoolean(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetBoolean(i); }
		public byte GetByte(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetByte(i); }
		public Tuple<long, byte[]> GetBytes(DataReaderId readerId, int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			// When messages are passed over the write, the "buffer" reference on the client is serialised and then deserialised here, so
			// it's not the same array. With an IDataReader, buffer WOULD be populated - to approximate this, we have to return a new array
			// and the client has to write its contents over the original array's contents.
			var lengthRead = _readerStore.Get(readerId).GetBytes(i, fieldOffset, buffer, bufferoffset, length);
			return Tuple.Create(lengthRead, buffer);
		}
		public char GetChar(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetChar(i); }
		public Tuple<long, char[]> GetChars(DataReaderId readerId, int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			// When messages are passed over the write, the "buffer" reference on the client is serialised and then deserialised here, so
			// it's not the same array. With an IDataReader, buffer WOULD be populated - to approximate this, we have to return a new array
			// and the client has to write its contents over the original array's contents.
			var lengthRead = _readerStore.Get(readerId).GetChars(i, fieldoffset, buffer, bufferoffset, length);
			return Tuple.Create(lengthRead, buffer);
		}
		public DataReaderId GetData(DataReaderId readerId, int i)
		{
			var reader = _readerStore.Get(readerId).GetData(i);
			try
			{
				return _readerStore.Add(reader);
			}
			catch
			{
				reader.Dispose();
				throw;
			}
		}
		public string GetDataTypeName(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetDataTypeName(i); }
		public DateTime GetDateTime(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetDateTime(i); }
		public decimal GetDecimal(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetDecimal(i); }
		public double GetDouble(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetDouble(i); }
		public string GetFieldType(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetFieldType(i).FullName; }
		public float GetFloat(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetFloat(i); }
		public Guid GetGuid(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetGuid(i); }
		public short GetInt16(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetInt16(i); }
		public int GetInt32(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetInt32(i); }
		public long GetInt64(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetInt64(i); }
		public string GetName(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetName(i); }
		public int GetOrdinal(DataReaderId readerId, string name) { return _readerStore.Get(readerId).GetOrdinal(name); }
		public DataTable GetSchemaTable(DataReaderId readerId) { return _readerStore.Get(readerId).GetSchemaTable(); }
		public string GetString(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetString(i); }
		public object GetValue(DataReaderId readerId, int i)
		{
			// If the value is null in the database then we'll get DBNull.Value here, which the WCF host won't like (it will say, if you know
			// how to trap the correct exception, something about using [KnownType] - but that can only be applied to types, not members). I'm
			// not sure how to solve this in a perfect manner, so I'm going to replace DBNull.Value with null and then do the opposite on the
			// client.
			var value = _readerStore.Get(readerId).GetValue(i);
			return (value == DBNull.Value) ? null : value;
		}
		public Tuple<int, object[]> GetValues(DataReaderId readerId, object[] values)
		{
			// When messages are passed over the write, the "buffer" reference on the client is serialised and then deserialised here, so
			// it's not the same array. With an IDataReader, buffer WOULD be populated - to approximate this, we have to return a new array
			// and the client has to write its contents over the original array's contents.
			var lengthRead = _readerStore.Get(readerId).GetValues(values);
			for (var i = 0; i < lengthRead; i++)
			{
				if (values[i] == DBNull.Value)
					values[i] = null; // Replace DBNull.Value with null for the same reason as in GetValue
			}
			return Tuple.Create(lengthRead, values);
		}
	}
}