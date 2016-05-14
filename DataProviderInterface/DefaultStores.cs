using System;
using System.Data;
using System.Data.SqlClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public static class DefaultStores
	{
		static DefaultStores()
		{
			ConnectionStore = new Store<ConnectionId, SqlConnection>(() => new ConnectionId(Guid.NewGuid()));
			CommandStore = new Store<Guid, IDbCommand>(() => Guid.NewGuid());
			TransactionStore = new Store<Guid, IDbTransaction>(() => Guid.NewGuid());
			ReaderStore = new Store<Guid, IDataReader>(() => Guid.NewGuid());
		}

		public static Store<ConnectionId, SqlConnection> ConnectionStore { get; }
		public static Store<Guid, IDbCommand> CommandStore { get; }
		public static Store<Guid, IDbTransaction> TransactionStore { get; }
		public static Store<Guid, IDataReader> ReaderStore { get; }
	}
}
