using System;
using System.Collections.Generic;
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
		private Dictionary<string, int> _currentColumnNamesLookupIfKnown;
		private object[] _valuesInCurrentRowIfKnown;
		public RemoteSqlDataReaderClient(IRemoteSqlDataReader reader, DataReaderId readerId)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			_reader = reader;
			_readerId = readerId;
			_disposed = false;

			_currentColumnNamesLookupIfKnown = null;
			_valuesInCurrentRowIfKnown = null;
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
			{
				// Dispose managed state if disposing is true
				_currentColumnNamesLookupIfKnown = null;
				_valuesInCurrentRowIfKnown = null;
			}

			// Free unmanaged resources - the remote Dispose call is the same as unmanaged resource because the garbage collector
			// has not idea how to automatically tidy it up
			_reader.Dispose(_readerId); // Tell the service that the current Reader is finished with

			_disposed = true;
		}

		public bool Read()
		{
			ThrowIfDisposed();
			var hasData = _reader.Read(_readerId);
			if (hasData)
			{
				// If there is a row to read data for then get all of the values for it and, if we don't already have them, the names
				// of the fields in the current result set. This will allow any requests for individual fields in the row (whether
				// requested by field name or by index) to be returned from data in memory, rather than having to go back to the
				// remote host every time (chatty WCF services can be slow). This has the disadvantage that every value from
				// current row has been pulled in to memory and we might not need them all, but hopefully that's worth the
				// trade-off (and, where performance is important, queries should only specify fields they want data for).
				if (_currentColumnNamesLookupIfKnown == null)
				{
					var fieldNames = _reader.GetFieldNames(_readerId);
					_currentColumnNamesLookupIfKnown = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
					for (var i = 0; i < fieldNames.Length; i++)
						_currentColumnNamesLookupIfKnown[fieldNames[i]] = i;
				}
				var currentRowValuesReadResult = _reader.GetValues(_readerId, new object[_currentColumnNamesLookupIfKnown.Count]);
				_valuesInCurrentRowIfKnown = currentRowValuesReadResult.Item2;
			}
			else
				_valuesInCurrentRowIfKnown = null; // If there is no more data then there can be no values for the current row
			return hasData;
		}
		public bool NextResult()
		{
			ThrowIfDisposed();
			_currentColumnNamesLookupIfKnown = null; // Moving to a new resultset will invalidate any column names that we have..
			_valuesInCurrentRowIfKnown = null; // .. and any current-row values
			return _reader.NextResult(_readerId);
		}
		public void Close()
		{
			ThrowIfDisposed();
			_currentColumnNamesLookupIfKnown = null;
			_valuesInCurrentRowIfKnown = null;
			_reader.Close(_readerId);
		}

		public object this[int i] { get { return GetValue(i); } }
		public object this[string name]
		{
			get
			{
				ThrowIfDisposed();
				if ((name != null) && (_currentColumnNamesLookupIfKnown != null) && (_valuesInCurrentRowIfKnown != null))
				{
					int fieldIndex;
					if (_currentColumnNamesLookupIfKnown.TryGetValue(name, out fieldIndex))
						return _valuesInCurrentRowIfKnown[fieldIndex];
				}
				return GetValue(GetOrdinal(name));
			}
		}

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
		public DataTable GetSchemaTable() { ThrowIfDisposed(); return _reader.GetSchemaTable(_readerId); }
		public string GetString(int i) { ThrowIfDisposed(); return _reader.GetString(_readerId, i); }
		public object GetValue(int i)
		{
			// I've had difficulty transmitting DBNull.Value down the wire for GetValue, so I've resorted to replacing DBNull.Value with null
			// on the host and then having to replace it back again here (I don't believe that there should be any time that null is a valid
			// value - if the database returned null for the column value then it will be returned DBNull.Value)
			ThrowIfDisposed();
			return _reader.GetValue(_readerId, i) ?? DBNull.Value;
		}
		public int GetValues(object[] values)
		{
			// When messages are passed over the wire, the data is serialised here then deserialised on the other end and then the response
			// is serialised and returned and deserialised here. That means that the "values" reference that is populated on the other end
			// is not the same as the reference here - so we need to overwrite the input array with the data from the response message.
			ThrowIfDisposed();
			var result = _reader.GetValues(_readerId, values);
			var numberOfObjectsPopulatedInArray = result.Item1;
			var valuesRead = result.Item2;
			Array.Copy(valuesRead, values, numberOfObjectsPopulatedInArray);
			for (var i = 0; i < numberOfObjectsPopulatedInArray; i++)
			{
				if (values[i] == null)
					values[i] = DBNull.Value; // Replace null with DBNull.Value for the same reason as in GetValue
			}
			return numberOfObjectsPopulatedInArray;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("reader");
		}
	}
}
