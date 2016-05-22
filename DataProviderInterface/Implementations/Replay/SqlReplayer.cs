using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	// InstanceContextMode.Single is required in order to initialise a ServiceHost with a singleton reference, which is the easiest way to instantiate
	// a service class without having to use a parameterless-constructor (since this class is designed to deal with all connections - we don't need one
	// instance per request, for example - a singleton instance is what we want)
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public sealed partial class SqlReplayer : ISqlProxy
	{
		private readonly Func<QueryCriteria, IDataReader> _dataRetriever;
		private readonly Func<QueryCriteria, Tuple<object>> _scalarDataRetriever;
		private readonly Func<QueryCriteria, int?> _nonQueryRowCountDataRetriever;
		private readonly Store<ConnectionId, IDbConnection> _connectionStore;
		private readonly Store<CommandId, IDbCommand> _commandStore;
		private readonly Store<TransactionId, IDbTransaction> _transactionStore;
		private readonly Store<ParameterId, IDbDataParameter> _parameterStore;
		private readonly Store<DataReaderId, IDataReader> _readerStore;
		private readonly ConcurrentParameterToCommandLookup _parametersToTidy;
		public SqlReplayer(
			Func<QueryCriteria, IDataReader> dataRetriever,
			Func<QueryCriteria, Tuple<object>> scalarDataRetriever,
			Func<QueryCriteria, int?> nonQueryRowCountDataRetriever)
		{
			if (dataRetriever == null)
				throw new ArgumentNullException(nameof(dataRetriever));
			if (scalarDataRetriever == null)
				throw new ArgumentNullException(nameof(scalarDataRetriever));
			if (nonQueryRowCountDataRetriever == null)
				throw new ArgumentNullException(nameof(nonQueryRowCountDataRetriever));

			_dataRetriever = dataRetriever;
			_scalarDataRetriever = scalarDataRetriever;
			_nonQueryRowCountDataRetriever = nonQueryRowCountDataRetriever;

			_connectionStore = new Store<ConnectionId, IDbConnection>(() => new ConnectionId(Guid.NewGuid()));
			_commandStore = new Store<CommandId, IDbCommand>(() => new CommandId(Guid.NewGuid()));
			_transactionStore = new Store<TransactionId, IDbTransaction>(() => new TransactionId(Guid.NewGuid()));
			_parameterStore = new Store<ParameterId, IDbDataParameter>(() => new ParameterId(Guid.NewGuid()));
			_readerStore = new Store<DataReaderId, IDataReader>(() => new DataReaderId(Guid.NewGuid()));

			// Parameters are not disposed of individually (unlike connections, commands, transactions and readers) - instead, the parameters in
			// the parameter store must be removed when the command that created them is disposed. The information to do that is recorded here.
			_parametersToTidy = new ConcurrentParameterToCommandLookup();
		}

		public ConnectionId GetNewConnectionId() { return _connectionStore.Add(new SqlReplayerConnection(_dataRetriever, _scalarDataRetriever, _nonQueryRowCountDataRetriever)); }

		public string GetConnectionString(ConnectionId connectionId) { return _connectionStore.Get(connectionId).ConnectionString; }
		public void SetConnectionString(ConnectionId connectionId, string value) { _connectionStore.Get(connectionId).ConnectionString = value; }

		public int GetConnectionTimeout(ConnectionId connectionId) { return _connectionStore.Get(connectionId).ConnectionTimeout; }

		public string GetDatabase(ConnectionId connectionId) { return _connectionStore.Get(connectionId).Database; }

		public ConnectionState GetState(ConnectionId connectionId) { return _connectionStore.Get(connectionId).State; }

		public void ChangeDatabase(ConnectionId connectionId, string databaseName) { _connectionStore.Get(connectionId).ChangeDatabase(databaseName); }

		public void Open(ConnectionId connectionId) { _connectionStore.Get(connectionId).Open(); }
		public void Close(ConnectionId connectionId) { _connectionStore.Get(connectionId).Close(); }

		public void Dispose(ConnectionId connectionId)
		{
			_connectionStore.Get(connectionId).Dispose();
			_connectionStore.Remove(connectionId);
		}

		public TransactionId BeginTransaction(ConnectionId connectionId) { return _transactionStore.Add(_connectionStore.Get(connectionId).BeginTransaction()); }
		public TransactionId BeginTransaction(ConnectionId connectionId, IsolationLevel il) { return _transactionStore.Add(_connectionStore.Get(connectionId).BeginTransaction(il)); }

		public CommandId CreateCommand(ConnectionId connectionId) { return _commandStore.Add(_connectionStore.Get(connectionId).CreateCommand()); }

		public string GetCommandText(CommandId commandId) { return _commandStore.Get(commandId).CommandText; }
		public void SetCommandText(CommandId commandId, string value) { _commandStore.Get(commandId).CommandText = value; }

		public int GetCommandTimeout(CommandId commandId) { return _commandStore.Get(commandId).CommandTimeout; }
		public void SetCommandTimeout(CommandId commandId, int value) { _commandStore.Get(commandId).CommandTimeout = value; }

		public CommandType GetCommandType(CommandId commandId) { return _commandStore.Get(commandId).CommandType; }
		public void SetCommandType(CommandId commandId, CommandType value) { _commandStore.Get(commandId).CommandType = value; }

		public ConnectionId? GetConnection(CommandId commandId)
		{
			var command = _commandStore.Get(commandId);
			if (command.Connection == null)
				return null;
			return _connectionStore.GetIdFor(command.Connection);
		}
		public void SetConnection(CommandId commandId, ConnectionId? connectionId)
		{
			var command = _commandStore.Get(commandId);
			if (connectionId == null)
				command.Connection = null;
			else
				command.Connection = _connectionStore.Get(connectionId.Value);
		}

		public ParameterId CreateParameter(CommandId commandId)
		{
			// Note: We need to track created parameters and tidy them up when the command is disposed (because parameters are not disposable and so
			// we won't be told via a Dispose call when parameters are no longer required; we know that they're no longer required when the command
			// that owns them is disposed, though)
			var parameterId = _parameterStore.Add(_commandStore.Get(commandId).CreateParameter());
			_parametersToTidy.Record(commandId, parameterId);
			return parameterId;
		}

		public TransactionId? GetTransaction(CommandId commandId)
		{
			var command = _commandStore.Get(commandId);
			if (command.Transaction == null)
				return null;
			return _transactionStore.GetIdFor(command.Transaction);
		}
		public void SetTransaction(CommandId commandId, TransactionId? transactionId) { _commandStore.Get(commandId).Transaction = (transactionId == null) ? null : _transactionStore.Get(transactionId.Value); }

		public UpdateRowSource GetUpdatedRowSource(CommandId commandId) { return _commandStore.Get(commandId).UpdatedRowSource; }
		public void SetUpdatedRowSource(CommandId commandId, UpdateRowSource value) { _commandStore.Get(commandId).UpdatedRowSource = value; }

		public void Prepare(CommandId commandId) { _commandStore.Get(commandId).Prepare(); }
		public void Cancel(CommandId commandId) { _commandStore.Get(commandId).Cancel(); }
		public void Dispose(CommandId commandId)
		{
			_commandStore.Get(commandId).Dispose();
			_commandStore.Remove(commandId);
		}

		public int ExecuteNonQuery(CommandId commandId) { return _commandStore.Get(commandId).ExecuteNonQuery(); }
		public object ExecuteScalar(CommandId commandId) { return _commandStore.Get(commandId).ExecuteScalar(); }
		public DataReaderId ExecuteReader(CommandId commandId, CommandBehavior behavior = CommandBehavior.Default) { return _readerStore.Add(_commandStore.Get(commandId).ExecuteReader(behavior)); }

		public int Add(CommandId commandId, ParameterId parameterId)
		{
			return _commandStore.Get(commandId).Parameters.Add(_parameterStore.Get(parameterId));
		}

		public ParameterId GetParameterByIndex(CommandId commandId, int index)
		{
			return _parameterStore.GetIdFor((IDbDataParameter)_commandStore.Get(commandId).Parameters[index]);
		}
		public void SetParameterByIndex(CommandId commandId, int index, ParameterId parameterId)
		{
			// Note: Parameters are removed from the parameter store when the command that created them is disposed (this is necessary as parameters are
			// not disposable and so can not communicate when they are no longer required). The book-keeping gets much more complicated if parameters are
			// allowed to be shared or moved between commands, so this is not acceptable behaviour (hopefully it is also unusual behaviour in real use)
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");

			// This will throw an IndexOutOfRangeException if index is not valid - that is the correct behaviour, so let it do so
			_commandStore.Get(commandId).Parameters[index] = _parameterStore.Get(parameterId);
		}

		public ParameterId GetParameterByName(CommandId commandId, string parameterName)
		{
			return _parameterStore.GetIdFor((IDbDataParameter)_commandStore.Get(commandId).Parameters[parameterName]);
		}
		public void SetParameterByName(CommandId commandId, string parameterName, ParameterId parameterId)
		{
			// Note: Parameters are removed from the parameter store when the command that created them is disposed (this is necessary as parameters are
			// not disposable and so can not communicate when they are no longer required). The book-keeping gets much more complicated if parameters are
			// allowed to be shared or moved between commands, so this is not acceptable behaviour (hopefully it is also unusual behaviour in real use)
			if (parameterName == null)
				throw new ArgumentNullException(nameof(parameterName));
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");

			// This will throw an IndexOutOfRangeException if parameterName is not valid - that is the correct behaviour, so let it do so
			_commandStore.Get(commandId).Parameters[parameterName] = _parameterStore.Get(parameterId);
		}

		public int GetCount(CommandId commandId) { return _commandStore.Get(commandId).Parameters.Count; }
		public void Clear(CommandId commandId) { _commandStore.Get(commandId).Parameters.Clear(); }
		public bool Contains(CommandId commandId, ParameterId parameterId) { return _parametersToTidy.IsRecordedForCommand(parameterId, commandId); }
		public bool Contains(CommandId commandId, string parameterName) { return _commandStore.Get(commandId).Parameters.Contains(parameterName); }

		public ParameterDirection GetDirection(ParameterId parameterId) { return _parameterStore.Get(parameterId).Direction; }
		public void SetDirection(ParameterId parameterId, ParameterDirection value) { _parameterStore.Get(parameterId).Direction = value; }

		public DbType GetDbType(ParameterId parameterId) { return _parameterStore.Get(parameterId).DbType; }
		public void SetDbType(ParameterId parameterId, DbType value) { _parameterStore.Get(parameterId).DbType = value; }

		public byte GetPrecision(ParameterId parameterId) { return _parameterStore.Get(parameterId).Precision; }
		public void SetPrecision(ParameterId parameterId, byte value) { _parameterStore.Get(parameterId).Precision = value; }

		public byte GetScale(ParameterId parameterId) { return _parameterStore.Get(parameterId).Scale; }
		public void SetScale(ParameterId parameterId, byte value) { _parameterStore.Get(parameterId).Scale = value; }
		
		public int GetSize(ParameterId parameterId) { return _parameterStore.Get(parameterId).Size; }
		public void SetSize(ParameterId parameterId, int value) { _parameterStore.Get(parameterId).Size = value; }

		public bool GetIsNullable(ParameterId parameterId) { return _parameterStore.Get(parameterId).IsNullable; }

		public string GetParameterName(ParameterId parameterId) { return _parameterStore.Get(parameterId).ParameterName; }
		public void SetParameterName(ParameterId parameterId, string value) { _parameterStore.Get(parameterId).ParameterName = value; }

		public string GetSourceColumn(ParameterId parameterId) { return _parameterStore.Get(parameterId).SourceColumn; }
		public void SetSourceColumn(ParameterId parameterId, string value) { _parameterStore.Get(parameterId).SourceColumn = value; }

		public DataRowVersion GetSourceVersion(ParameterId parameterId) { return _parameterStore.Get(parameterId).SourceVersion; }
		public void SetSourceVersion(ParameterId parameterId, DataRowVersion value) { _parameterStore.Get(parameterId).SourceVersion = value; }

		public object GetValue(ParameterId parameterId) { return _parameterStore.Get(parameterId).Value; }
		public void SetValue(ParameterId parameterId, object value) { _parameterStore.Get(parameterId).Value = value; }

		public int IndexOf(CommandId commandId, ParameterId parameterId)
		{
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");
			return _commandStore.Get(commandId).Parameters.IndexOf(_parameterStore.Get(parameterId));
		}
		public int IndexOf(CommandId commandId, string parameterName) { return _commandStore.Get(commandId).Parameters.IndexOf(parameterName); }

		public void Insert(CommandId commandId, int index, ParameterId parameterId)
		{
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");
			_commandStore.Get(commandId).Parameters.Insert(index, _parameterStore.Get(parameterId));
		}

		public void Remove(CommandId commandId, ParameterId parameterId)
		{
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");
			_commandStore.Get(commandId).Parameters.Remove(_parameterStore.Get(parameterId));
		}
		public void RemoveAt(CommandId commandId, int index) { _commandStore.Get(commandId).Parameters.RemoveAt(index); }
		public void RemoveAt(CommandId commandId, string parameterName) { _commandStore.Get(commandId).Parameters.RemoveAt(parameterName); }

		public ConnectionId GetConnection(TransactionId transactionId) { return _connectionStore.GetIdFor(_transactionStore.Get(transactionId).Connection); }

		public IsolationLevel GetIsolationLevel(TransactionId transactionId) { return _transactionStore.Get(transactionId).IsolationLevel; }

		public void Commit(TransactionId transactionId) { _transactionStore.Get(transactionId).Commit(); }
		public void Rollback(TransactionId transactionId) { _transactionStore.Get(transactionId).Rollback(); }

		public void Dispose(TransactionId transactionId)
		{
			_transactionStore.Get(transactionId).Dispose();
			_transactionStore.Remove(transactionId);
		}

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
		public object GetValue(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetValue(i); }
		public Tuple<int, object[]> GetValues(DataReaderId readerId, object[] values)
		{
			// When messages are passed over the write, the "buffer" reference on the client is serialised and then deserialised here, so
			// it's not the same array. With an IDataReader, buffer WOULD be populated - to approximate this, we have to return a new array
			// and the client has to write its contents over the original array's contents.
			var lengthRead = _readerStore.Get(readerId).GetValues(values);
			return Tuple.Create(lengthRead, values);
		}
	}
}
