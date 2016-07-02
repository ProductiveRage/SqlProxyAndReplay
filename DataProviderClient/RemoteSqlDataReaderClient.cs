using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
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
					{
						if (!_currentColumnNamesLookupIfKnown.ContainsKey(fieldNames[i]))
							_currentColumnNamesLookupIfKnown.Add(fieldNames[i], i);
					}
				}
				_valuesInCurrentRowIfKnown = _reader.GetValues(_readerId, _currentColumnNamesLookupIfKnown.Count);
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
						return _valuesInCurrentRowIfKnown[fieldIndex] ?? DBNull.Value; // Replace null with DBNull.Value for the same reason as in GetValue
				}
				return GetValue(GetOrdinal(name));
			}
		}

		public int Depth { get { ThrowIfDisposed(); return _reader.GetDepth(_readerId); } }
		public int FieldCount
		{
			get
			{
				ThrowIfDisposed();
				if (_valuesInCurrentRowIfKnown != null)
					return _valuesInCurrentRowIfKnown.Length;
				return _reader.GetFieldCount(_readerId);
			}
		}
		public bool IsClosed { get { ThrowIfDisposed(); return _reader.GetIsClosed(_readerId); } }
		public int RecordsAffected { get { ThrowIfDisposed(); return _reader.GetRecordsAffected(_readerId); } }

		public bool IsDBNull(int i)
		{
			ThrowIfDisposed();
			if ((_valuesInCurrentRowIfKnown != null) && (i >= 0) && (i < _valuesInCurrentRowIfKnown.Length))
				return _valuesInCurrentRowIfKnown[i] == null;
			return _reader.IsDBNull(_readerId, i);
		}

		public bool GetBoolean(int i) { return GetAs<bool>(i, _reader.GetBoolean); }
		public byte GetByte(int i) { return GetAs<byte>(i, _reader.GetByte); }
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
		public char GetChar(int i) { return GetAs<char>(i, _reader.GetChar); }
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
		public DateTime GetDateTime(int i) { return GetAs<DateTime>(i, _reader.GetDateTime); }
		public decimal GetDecimal(int i) { return GetAs<decimal>(i, _reader.GetDecimal); }
		public double GetDouble(int i) { return GetAs<double>(i, _reader.GetDouble); }
		public Type GetFieldType(int i)
		{
			// "Type" is not serialisable, so send its name down the wire and translate back into a Type instance on the other end
			ThrowIfDisposed();
			var typeFullName = _reader.GetFieldType(_readerId, i);
			return Type.GetType(typeFullName, throwOnError: true);
		}
		public float GetFloat(int i) { return GetAs<float>(i, _reader.GetFloat); }
		public Guid GetGuid(int i) { return GetAs<Guid>(i, _reader.GetGuid); }
		public short GetInt16(int i) { return GetAs<short>(i, _reader.GetInt16); }
		public int GetInt32(int i) { return GetAs<int>(i, _reader.GetInt32); }
		public long GetInt64(int i) { return GetAs<long>(i, _reader.GetInt64); }
		public string GetName(int i)
		{
			ThrowIfDisposed();
			if (_currentColumnNamesLookupIfKnown != null)
			{
				foreach (var nameAndIndex in _currentColumnNamesLookupIfKnown)
				{
					if (nameAndIndex.Value == i)
						return nameAndIndex.Key;
				}
			}
			return _reader.GetName(_readerId, i);
		}
		public int GetOrdinal(string name)
		{
			ThrowIfDisposed();
			int fieldIndex;
			if ((_currentColumnNamesLookupIfKnown != null) && (name != null) && _currentColumnNamesLookupIfKnown.TryGetValue(name, out fieldIndex))
				return fieldIndex;
			return _reader.GetOrdinal(_readerId, name);
		}
		public DataTable GetSchemaTable() { ThrowIfDisposed(); return _reader.GetSchemaTable(_readerId); }
		public string GetString(int i) { return GetAs<string>(i, _reader.GetString); }
		public object GetValue(int i)
		{
			// I've had difficulty transmitting DBNull.Value down the wire for GetValue, so I've resorted to replacing DBNull.Value with null
			// on the host and then having to replace it back again here (I don't believe that there should be any time that null is a valid
			// value - if the database returned null for the column value then it will be returned DBNull.Value)
			if ((_valuesInCurrentRowIfKnown != null) && (i >= 0) && (i < _valuesInCurrentRowIfKnown.Length))
				return _valuesInCurrentRowIfKnown[i] ?? DBNull.Value;
			return GetAs<object>(i, _reader.GetValue) ?? DBNull.Value;
		}
		public int GetValues(object[] values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			ThrowIfDisposed();
			if (values.Length == 0)
				return 0;

			// We couldn't pass the values array directly to the host to be populated because it would be serialised in order to pass it over
			// the wire, so any manipulations (such as pushing data into it) would not occur on the array reference that we have here. Instead,
			// we get a new array from the host and then copy the values back on top of the input array, ensuring that it gets populated.
			var valuesRead = _reader.GetValues(_readerId, maximumNumberOfValuesToRead: values.Length);
			for (var i = 0; i < valuesRead.Length; i++)
				values[i] = valuesRead[i] ?? DBNull.Value; // Replace null with DBNull.Value for the same reason as in GetValue
			return valuesRead.Length;
		}

		/// <summary>
		/// If there is data for the row in memory then try to return the requested value from that data. The data should be in memory if a row
		/// has been read. If there isn't any data or if there is another error condition (such as an invalid field index or an inconsistent type)
		/// then make the remote call, so that the exception that is returned is consistent with the real exception that would happen (the only
		/// error case that IS handled here is the return of DBNull.Value, which is not acceptable here - this method should be used to make it
		/// easier to implement GetString, GetInt32, etc.. and they will never return null / DBNull.Value).
		/// </summary>
		private T GetAs<T>(int i, Func<DataReaderId, int, T> fallback)
		{
			if (fallback == null)
				throw new ArgumentNullException(nameof(fallback));

			ThrowIfDisposed();
			if ((_valuesInCurrentRowIfKnown != null) && (i >= 0) && (i < _valuesInCurrentRowIfKnown.Length))
			{
				var value = _valuesInCurrentRowIfKnown[i];
				if (value == null)
					throw new SqlNullValueException();
				if (value is T)
					return (T)value;
			}
			return fallback(_readerId, i);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("reader");
		}
	}
}
