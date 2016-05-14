using System.Data;
using System.Data.SqlClient;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public static class DefaultStores
	{
		static DefaultStores()
		{
			ConnectionStore = new Store<SqlConnection>();
			CommandStore = new Store<IDbCommand>();
			TransactionStore = new Store<IDbTransaction>();
			ReaderStore = new Store<IDataReader>();
		}

		public static Store<SqlConnection> ConnectionStore { get; }
		public static Store<IDbCommand> CommandStore { get; }
		public static Store<IDbTransaction> TransactionStore { get; }
		public static Store<IDataReader> ReaderStore { get; }
	}
}
