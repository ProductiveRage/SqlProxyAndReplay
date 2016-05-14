using System;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	internal sealed class RemoteSqlDataReaderClient : IDataReader
	{
		private readonly IRemoteSqlDataReader _reader;
		private readonly DataReaderId _readerId;
		private bool _disposed;
		public RemoteSqlDataReaderClient(IRemoteSqlDataReader reader, DataReaderId readerId)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			
			_reader = reader;
			_readerId = readerId;
			_disposed = false;
		}
		~RemoteSqlDataReaderClient()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
				_reader.Dispose(_readerId); // Tell the service that the current Reader is finished with

			_disposed = true;
		}

		public bool Read() { ThrowIfDisposed(); return _reader.Read(_readerId); }
		public bool NextResult() { ThrowIfDisposed(); return _reader.NextResult(_readerId); }
		public void Close() { ThrowIfDisposed(); _reader.Close(_readerId); }

		public object this[int i] { get { ThrowIfDisposed(); return _reader.GetValue(_readerId, i); } }
		public object this[string name] { get { ThrowIfDisposed(); return GetValue(GetOrdinal(name)); } }

		public int Depth { get { ThrowIfDisposed(); return _reader.GetDepth(_readerId); } }
		public int FieldCount { get { ThrowIfDisposed(); return _reader.GetFieldCount(_readerId); } }
		public bool IsClosed { get { ThrowIfDisposed(); return _reader.GetIsClosed(_readerId); } }
		public int RecordsAffected { get { ThrowIfDisposed(); return _reader.GetRecordsAffected(_readerId); } }

		public bool IsDBNull(int i) { ThrowIfDisposed(); return _reader.IsDBNull(_readerId, i); }

		public bool GetBoolean(int i) { ThrowIfDisposed(); return _reader.GetBoolean(_readerId, i); }
		public byte GetByte(int i) { ThrowIfDisposed(); return _reader.GetByte(_readerId, i); }
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			// When messages are passed over the wire, the data is serialised here then deserialised on the other end and then the response
			// is serialised and returned and deserialised here. That means that the "buffer" reference that is populated on the other end
			// is not the same as the reference here - so we need to overwrite the input buffer with the data from the response message.
			ThrowIfDisposed();
			var result = _reader.GetBytes(_readerId, i, fieldOffset, buffer, bufferoffset, length);
			var numberOfBytesRead = result.Item1;
			var bytesRead = result.Item2;
			Array.Copy(bytesRead, buffer, numberOfBytesRead);
			return numberOfBytesRead;
		}
		public char GetChar(int i) { ThrowIfDisposed(); return _reader.GetChar(_readerId, i); }
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			// When messages are passed over the wire, the data is serialised here then deserialised on the other end and then the response
			// is serialised and returned and deserialised here. That means that the "buffer" reference that is populated on the other end
			// is not the same as the reference here - so we need to overwrite the input buffer with the data from the response message.
			ThrowIfDisposed();
			var result = _reader.GetChars(_readerId, i, fieldoffset, buffer, bufferoffset, length);
			var numberOfBytesRead = result.Item1;
			var bytesRead = result.Item2;
			Array.Copy(bytesRead, buffer, numberOfBytesRead);
			return numberOfBytesRead;
		}
		public IDataReader GetData(int i)
		{
			ThrowIfDisposed();
			return new RemoteSqlDataReaderClient(_reader, _reader.GetData(_readerId, i));
		}
		public string GetDataTypeName(int i) { ThrowIfDisposed(); return _reader.GetDataTypeName(_readerId, i); }
		public DateTime GetDateTime(int i) { ThrowIfDisposed(); return _reader.GetDateTime(_readerId, i); }
		public decimal GetDecimal(int i) { ThrowIfDisposed(); return _reader.GetDecimal(_readerId, i); }
		public double GetDouble(int i) { ThrowIfDisposed(); return _reader.GetDouble(_readerId, i); }
		public Type GetFieldType(int i)
		{
			// "Type" is not serialisable, so send its name down the wire and translate back into a Type instance on the other end
			ThrowIfDisposed();
			var typeFullName = _reader.GetFieldType(_readerId, i);
			return Type.GetType(typeFullName, throwOnError: true);
		}
		public float GetFloat(int i) { ThrowIfDisposed(); return _reader.GetFloat(_readerId, i); }
		public Guid GetGuid(int i) { ThrowIfDisposed(); return _reader.GetGuid(_readerId, i); }
		public short GetInt16(int i) { ThrowIfDisposed(); return _reader.GetInt16(_readerId, i); }
		public int GetInt32(int i) { ThrowIfDisposed(); return _reader.GetInt32(_readerId, i); }
		public long GetInt64(int i) { ThrowIfDisposed(); return _reader.GetInt64(_readerId, i); }
		public string GetName(int i) { ThrowIfDisposed(); return _reader.GetName(_readerId, i); }
		public int GetOrdinal(string name) { ThrowIfDisposed(); return _reader.GetOrdinal(_readerId, name); }
		public DataTable GetSchemaTable() { ThrowIfDisposed(); throw new NotImplementedException(); }
		public string GetString(int i) { ThrowIfDisposed(); return _reader.GetString(_readerId, i); }
		public object GetValue(int i) { ThrowIfDisposed(); return _reader.GetValue(_readerId, i); }
		public int GetValues(object[] values) { ThrowIfDisposed(); return _reader.GetValues(_readerId, values); }

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("reader");
		}
	}
}
