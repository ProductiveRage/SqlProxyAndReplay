using System;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough
{
	// InstanceContextMode.Single is required in order to initialise a ServiceHost with a singleton reference, which is the easiest way to instantiate
	// a service class without having to use a parameterless-constructor (since this class is designed to deal with all connections - we don't need one
	// instance per request, for example - a singleton instance is what we want)
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public sealed partial class SqlProxy : ISqlProxy
	{
		private readonly Action<QueryCriteria> _queryRecorder;
		private readonly Action<QueryCriteria> _scalarQueryRecorder;
		private readonly Store<ConnectionId, SqlConnection> _connectionStore;
		private readonly Store<CommandId, SqlCommand> _commandStore;
		private readonly Store<TransactionId, SqlTransaction> _transactionStore;
		private readonly Store<ParameterId, SqlParameter> _parameterStore;
		private readonly Store<DataReaderId, IDataReader> _readerStore;
		private readonly ConcurrentParameterToCommandLookup _parametersToTidy;
		public SqlProxy(Action<QueryCriteria> queryRecorder, Action<QueryCriteria> scalarQueryRecorder)
		{
			if (queryRecorder == null)
				throw new ArgumentNullException(nameof(queryRecorder));
			if (scalarQueryRecorder == null)
				throw new ArgumentNullException(nameof(scalarQueryRecorder));

			_queryRecorder = queryRecorder;
			_scalarQueryRecorder = scalarQueryRecorder;

			_connectionStore = new Store<ConnectionId, SqlConnection>(() => new ConnectionId(Guid.NewGuid()));
			_commandStore = new Store<CommandId, SqlCommand>(() => new CommandId(Guid.NewGuid()));
			_transactionStore = new Store<TransactionId, SqlTransaction>(() => new TransactionId(Guid.NewGuid()));
			_parameterStore = new Store<ParameterId, SqlParameter>(() => new ParameterId(Guid.NewGuid()));
			_readerStore = new Store<DataReaderId, IDataReader>(() => new DataReaderId(Guid.NewGuid()));

			// Parameters are not disposed of individually (unlike connections, commands, transactions and readers) - instead, the parameters in
			// the parameter store must be removed when the command that created them is disposed. The information to do that is recorded here.
			_parametersToTidy = new ConcurrentParameterToCommandLookup();
		}
	}
}
