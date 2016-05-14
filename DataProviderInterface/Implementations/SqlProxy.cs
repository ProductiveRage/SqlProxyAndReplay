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
		private readonly Store<Guid, IDbCommand> _commandStore;
		private readonly Store<Guid, IDataReader> _readerStore;
		private readonly Store<Guid, IDbTransaction> _transactionStore;
		public SqlProxy(
			Store<ConnectionId, SqlConnection> connectionStore,
			Store<Guid, IDbCommand> commandStore,
			Store<Guid, IDbTransaction> transactionStore,
			Store<Guid, IDataReader> readerStore)
		{
			if (connectionStore == null)
				throw new ArgumentNullException(nameof(connectionStore));
			if (commandStore == null)
				throw new ArgumentNullException(nameof(commandStore));
			if (transactionStore == null)
				throw new ArgumentNullException(nameof(transactionStore));
			if (readerStore == null)
				throw new ArgumentNullException(nameof(readerStore));

			_connectionStore = connectionStore;
			_commandStore = commandStore;
			_transactionStore = transactionStore;
			_readerStore = readerStore;
		}
		public SqlProxy() : this(DefaultStores.ConnectionStore, DefaultStores.CommandStore, DefaultStores.TransactionStore, DefaultStores.ReaderStore) { }
	}
}
