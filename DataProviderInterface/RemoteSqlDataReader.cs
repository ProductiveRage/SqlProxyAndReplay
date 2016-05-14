using System;
using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed class RemoteSqlDataReader : IRemoteSqlDataReader
	{
		private readonly Store<IDataReader> _readerStore;
		public RemoteSqlDataReader(Store<IDataReader> readerStore)
		{
			if (readerStore == null)
				throw new ArgumentNullException(nameof(readerStore));

			_readerStore = readerStore;
		}
		public RemoteSqlDataReader() : this(DefaultStores.ReaderStore) { }

		public bool Read(Guid readerId) { return _readerStore.Get(readerId).Read(); }
		public bool NextResult(Guid readerId) { return _readerStore.Get(readerId).NextResult(); }
		public void Close(Guid readerId) { _readerStore.Get(readerId).Close(); }
		public void Dispose(Guid readerId)
		{
			_readerStore.Get(readerId).Dispose();
			_readerStore.Remove(readerId);
		}

		public int GetDepth(Guid readerId) { return _readerStore.Get(readerId).Depth; }
		public int GetFieldCount(Guid readerId) { return _readerStore.Get(readerId).FieldCount; }
		public bool GetIsClosed(Guid readerId) { return _readerStore.Get(readerId).IsClosed; }
		public int GetRecordsAffected(Guid readerId) { return _readerStore.Get(readerId).RecordsAffected; }

		public bool IsDBNull(Guid readerId, int i) { return _readerStore.Get(readerId).IsDBNull(i); }

		public bool GetBoolean(Guid readerId, int i) { return _readerStore.Get(readerId).GetBoolean(i); }
		public byte GetByte(Guid readerId, int i) { return _readerStore.Get(readerId).GetByte(i); }
		public Tuple<long, byte[]> GetBytes(Guid readerId, int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			// When messages are passed over the write, the "buffer" reference on the client is serialised and then deserialised here, so
			// it's not the same array. With an IDataReader, buffer WOULD be populated - to approximate this, we have to return a new array
			// and the client has to write its contents over the original array's contents.
			var lengthRead = _readerStore.Get(readerId).GetBytes(i, fieldOffset, buffer, bufferoffset, length);
			return Tuple.Create(lengthRead, buffer);
		}
		public char GetChar(Guid readerId, int i) { return _readerStore.Get(readerId).GetChar(i); }
		public Tuple<long, char[]> GetChars(Guid readerId, int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			// When messages are passed over the write, the "buffer" reference on the client is serialised and then deserialised here, so
			// it's not the same array. With an IDataReader, buffer WOULD be populated - to approximate this, we have to return a new array
			// and the client has to write its contents over the original array's contents.
			var lengthRead = _readerStore.Get(readerId).GetChars(i, fieldoffset, buffer, bufferoffset, length);
			return Tuple.Create(lengthRead, buffer);
		}
		public Guid GetData(Guid readerId, int i)
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
		public string GetDataTypeName(Guid readerId, int i) { return _readerStore.Get(readerId).GetDataTypeName(i); }
		public DateTime GetDateTime(Guid readerId, int i) { return _readerStore.Get(readerId).GetDateTime(i); }
		public decimal GetDecimal(Guid readerId, int i) { return _readerStore.Get(readerId).GetDecimal(i); }
		public double GetDouble(Guid readerId, int i) { return _readerStore.Get(readerId).GetDouble(i); }
		public string GetFieldType(Guid readerId, int i) { return _readerStore.Get(readerId).GetFieldType(i).FullName; }
		public float GetFloat(Guid readerId, int i) { return _readerStore.Get(readerId).GetFloat(i); }
		public Guid GetGuid(Guid readerId, int i) { return _readerStore.Get(readerId).GetGuid(i); }
		public short GetInt16(Guid readerId, int i) { return _readerStore.Get(readerId).GetInt16(i); }
		public int GetInt32(Guid readerId, int i) { return _readerStore.Get(readerId).GetInt32(i); }
		public long GetInt64(Guid readerId, int i) { return _readerStore.Get(readerId).GetInt64(i); }
		public string GetName(Guid readerId, int i) { return _readerStore.Get(readerId).GetName(i); }
		public int GetOrdinal(Guid readerId, string name) { return _readerStore.Get(readerId).GetOrdinal(name); }
		public string GetString(Guid readerId, int i) { return _readerStore.Get(readerId).GetString(i); }
		public object GetValue(Guid readerId, int i) { return _readerStore.Get(readerId).GetValue(i); }
		public int GetValues(Guid readerId, object[] values) { return _readerStore.Get(readerId).GetValues(values); }
	}
}