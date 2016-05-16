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
			CommandStore = new Store<CommandId, IDbCommand>(() => new CommandId(Guid.NewGuid()));
			TransactionStore = new Store<TransactionId, IDbTransaction>(() => new TransactionId(Guid.NewGuid()));
			ParameterStore = new Store<ParameterId, IDbDataParameter>(() => new ParameterId(Guid.NewGuid()));
			ReaderStore = new Store<DataReaderId, IDataReader>(() => new DataReaderId(Guid.NewGuid()));
		}

		public static Store<ConnectionId, SqlConnection> ConnectionStore { get; }
		public static Store<CommandId, IDbCommand> CommandStore { get; }
		public static Store<TransactionId, IDbTransaction> TransactionStore { get; }
		public static Store<ParameterId, IDbDataParameter> ParameterStore { get; }
		public static Store<DataReaderId, IDataReader> ReaderStore { get; }
	}
}
