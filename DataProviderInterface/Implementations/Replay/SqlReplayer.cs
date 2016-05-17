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
		private readonly Store<ConnectionId, SqlReplayerConnection> _connectionStore;
		private readonly Store<CommandId, SqlReplayerCommand> _commandStore;
		private readonly Store<TransactionId, SqlReplayerTransaction> _transactionStore;
		private readonly Store<ParameterId, SqlReplayerParameter> _parameterStore;
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

			_connectionStore = new Store<ConnectionId, SqlReplayerConnection>(() => new ConnectionId(Guid.NewGuid()));
			_commandStore = new Store<CommandId, SqlReplayerCommand>(() => new CommandId(Guid.NewGuid()));
			_transactionStore = new Store<TransactionId, SqlReplayerTransaction>(() => new TransactionId(Guid.NewGuid()));
			_parameterStore = new Store<ParameterId, SqlReplayerParameter>(() => new ParameterId(Guid.NewGuid()));
			_readerStore = new Store<DataReaderId, IDataReader>(() => new DataReaderId(Guid.NewGuid()));

			// Parameters are not disposed of individually (unlike connections, commands, transactions and readers) - instead, the parameters in
			// the parameter store must be removed when the command that created them is disposed. The information to do that is recorded here.
			_parametersToTidy = new ConcurrentParameterToCommandLookup();
		}

		public ConnectionId GetNewConnectionId() { return _connectionStore.Add(new SqlReplayerConnection(_dataRetriever, _scalarDataRetriever, _nonQueryRowCountDataRetriever)); }

		public string GetConnectionString(ConnectionId connectionId) { return _connectionStore.Get(connectionId).ConnectionString; }
		public void SetConnectionString(ConnectionId connectionId, string value) { _connectionStore.Get(connectionId).ConnectionString = value; }

		public int GetConnectionTimeout(ConnectionId connectionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public string GetDatabase(ConnectionId connectionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public ConnectionState GetState(ConnectionId connectionId) { return _connectionStore.Get(connectionId).State; }

		public void ChangeDatabase(ConnectionId connectionId, string databaseName)
		{
			throw new NotImplementedException(); // TODO
		}

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

		public int GetCommandTimeout(CommandId commandId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetCommandTimeout(CommandId commandId, int value)
		{
			throw new NotImplementedException(); // TODO
		}

		public CommandType GetCommandType(CommandId commandId) { return _commandStore.Get(commandId).CommandType; }
		public void SetCommandType(CommandId commandId, CommandType value) { _commandStore.Get(commandId).CommandType = value; }

		public ConnectionId? GetConnection(CommandId commandId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetConnection(CommandId commandId, ConnectionId? connectionId)
		{
			throw new NotImplementedException(); // TODO
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
			throw new NotImplementedException(); // TODO
		}
		public void SetTransaction(CommandId commandId, TransactionId? transactionId) { _commandStore.Get(commandId).Transaction = (transactionId == null) ? null : _transactionStore.Get(transactionId.Value); }

		public UpdateRowSource GetUpdatedRowSource(CommandId commandId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetUpdatedRowSource(CommandId commandId, UpdateRowSource value)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Prepare(CommandId commandId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Cancel(CommandId commandId)
		{
			throw new NotImplementedException(); // TODO
		}

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
			throw new NotImplementedException(); // TODO
		}

		public void SetParameterByIndex(CommandId commandId, int index, ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public ParameterId GetParameterByName(CommandId commandId, string parameterName)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetParameterByName(CommandId commandId, string parameterName, ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public int GetCount(CommandId commandId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Clear(CommandId commandId)
		{
			throw new NotImplementedException(); // TODO
		}

		public bool Contains(CommandId commandId, ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public bool Contains(CommandId commandId, string parameterName)
		{
			throw new NotImplementedException(); // TODO
		}

		public ParameterDirection GetDirection(ParameterId parameterId) { return _parameterStore.Get(parameterId).Direction; }
		public void SetDirection(ParameterId parameterId, ParameterDirection value) { _parameterStore.Get(parameterId).Direction = value; }

		public DbType GetDbType(ParameterId parameterId) { return _parameterStore.Get(parameterId).DbType; }
		public void SetDbType(ParameterId parameterId, DbType value) { _parameterStore.Get(parameterId).DbType = value; }

		public byte GetPrecision(ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetPrecision(ParameterId parameterId, byte value)
		{
			throw new NotImplementedException(); // TODO
		}

		public byte GetScale(ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetScale(ParameterId parameterId, byte value)
		{
			throw new NotImplementedException(); // TODO
		}

		public int GetSize(ParameterId parameterId) { return _parameterStore.Get(parameterId).Size; }
		public void SetSize(ParameterId parameterId, int value) { _parameterStore.Get(parameterId).Size = value; }

		public bool GetIsNullable(ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public string GetParameterName(ParameterId parameterId) { return _parameterStore.Get(parameterId).ParameterName; }
		public void SetParameterName(ParameterId parameterId, string value) { _parameterStore.Get(parameterId).ParameterName = value; }

		public string GetSourceColumn(ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetSourceColumn(ParameterId parameterId, string value)
		{
			throw new NotImplementedException(); // TODO
		}

		public DataRowVersion GetSourceVersion(ParameterId parameterId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void SetSourceVersion(ParameterId parameterId, DataRowVersion value)
		{
			throw new NotImplementedException(); // TODO
		}

		public object GetValue(ParameterId parameterId) { return _parameterStore.Get(parameterId).Value; }
		public void SetValue(ParameterId parameterId, object value) { _parameterStore.Get(parameterId).Value = value; }

		public ConnectionId GetConnection(TransactionId transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public IsolationLevel GetIsolationLevel(TransactionId transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Commit(TransactionId transactionId) { _transactionStore.Get(transactionId).Commit(); }
		public void Rollback(TransactionId transactionId) { _transactionStore.Get(transactionId).Rollback(); }

		public void Dispose(TransactionId transactionId)
		{
			_transactionStore.Get(transactionId).Dispose();
			_transactionStore.Remove(transactionId);
		}

		public bool Read(DataReaderId readerId) { return _readerStore.Get(readerId).Read(); }
		public bool NextResult(DataReaderId readerId) { return _readerStore.Get(readerId).NextResult(); }

		public void Close(DataReaderId readerId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Dispose(DataReaderId readerId)
		{
			_readerStore.Get(readerId).Dispose();
			_readerStore.Remove(readerId);
		}

		public int GetDepth(DataReaderId readerId)
		{
			throw new NotImplementedException(); // TODO
		}

		public int GetFieldCount(DataReaderId readerId) { return _readerStore.Get(readerId).FieldCount; }

		public bool GetIsClosed(DataReaderId readerId)
		{
			throw new NotImplementedException(); // TODO
		}

		public int GetRecordsAffected(DataReaderId readerId)
		{
			throw new NotImplementedException(); // TODO
		}

		public bool IsDBNull(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public bool GetBoolean(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public byte GetByte(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public Tuple<long, byte[]> GetBytes(DataReaderId readerId, int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException(); // TODO
		}

		public char GetChar(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public Tuple<long, char[]> GetChars(DataReaderId readerId, int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			throw new NotImplementedException(); // TODO
		}

		public DataReaderId GetData(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public string GetDataTypeName(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public DateTime GetDateTime(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public decimal GetDecimal(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public double GetDouble(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public string GetFieldType(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetFieldType(i).FullName; }

		public float GetFloat(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public Guid GetGuid(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public short GetInt16(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public int GetInt32(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public long GetInt64(DataReaderId readerId, int i)
		{
			throw new NotImplementedException(); // TODO
		}

		public string GetName(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetName(i); }

		public int GetOrdinal(DataReaderId readerId, string name) { return _readerStore.Get(readerId).GetOrdinal(name); }

		public DataTable GetSchemaTable(DataReaderId readerId)
		{
			throw new NotImplementedException(); // TODO
		}

		public string GetString(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetString(i); }

		public object GetValue(DataReaderId readerId, int i) { return _readerStore.Get(readerId).GetValue(i); }

		public int GetValues(DataReaderId readerId, object[] values)
		{
			throw new NotImplementedException(); // TODO
		}
	}
}
