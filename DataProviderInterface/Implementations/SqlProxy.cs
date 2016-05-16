using System;
using System.Data;
using System.Data.SqlClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		private readonly Store<ConnectionId, SqlConnection> _connectionStore;
		private readonly Store<CommandId, IDbCommand> _commandStore;
		private readonly Store<TransactionId, IDbTransaction> _transactionStore;
		private readonly Store<ParameterId, IDbDataParameter> _parameterStore;
		private readonly Store<DataReaderId, IDataReader> _readerStore;
		private readonly ConcurrentParameterToCommandLookup _parametersToTidy;
		public SqlProxy(
			Store<ConnectionId, SqlConnection> connectionStore,
			Store<CommandId, IDbCommand> commandStore,
			Store<TransactionId, IDbTransaction> transactionStore,
			Store<ParameterId, IDbDataParameter> parameterStore,
			Store<DataReaderId, IDataReader> readerStore)
		{
			if (connectionStore == null)
				throw new ArgumentNullException(nameof(connectionStore));
			if (commandStore == null)
				throw new ArgumentNullException(nameof(commandStore));
			if (transactionStore == null)
				throw new ArgumentNullException(nameof(transactionStore));
			if (parameterStore == null)
				throw new ArgumentNullException(nameof(parameterStore));
			if (readerStore == null)
				throw new ArgumentNullException(nameof(readerStore));

			_connectionStore = connectionStore;
			_commandStore = commandStore;
			_transactionStore = transactionStore;
			_parameterStore = parameterStore;
			_readerStore = readerStore;

			// Parameters are not disposed of individually (unlike connections, commands, transactions and readers) - instead, the parameters in
			// the parameter store must be removed when the command that created them is disposed. The information to do that is recorded here.
		}
		public SqlProxy() : this(
			DefaultStores.ConnectionStore,
			DefaultStores.CommandStore,
			DefaultStores.TransactionStore,
			DefaultStores.ParameterStore,
			DefaultStores.ReaderStore) { }
	}
}
