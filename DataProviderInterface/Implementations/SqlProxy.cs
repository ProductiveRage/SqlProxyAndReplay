using System;
using System.Data;
using System.Data.SqlClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		private readonly Store<SqlConnection> _connectionStore;
		private readonly Store<IDbCommand> _commandStore;
		private readonly Store<IDataReader> _readerStore;
		private readonly Store<IDbTransaction> _transactionStore;
		public SqlProxy(
			Store<SqlConnection> connectionStore,
			Store<IDbCommand> commandStore,
			Store<IDbTransaction> transactionStore,
			Store<IDataReader> readerStore)
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
