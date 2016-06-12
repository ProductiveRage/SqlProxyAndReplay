using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.PassThrough
{
	// InstanceContextMode.Single is required in order to initialise a ServiceHost with a singleton reference, which is the easiest way to instantiate
	// a service class without having to use a parameterless-constructor (since this class is designed to deal with all connections - we don't need one
	// instance per request, for example - a singleton instance is what we want)
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public sealed partial class SqlProxy : ISqlProxy
	{
		private readonly Func<IDbConnection> _connectionGenerator;
		private readonly Action<QueryCriteria> _queryRecorder, _scalarQueryRecorder, _nonQueryRowCountRecorder;
		private readonly Store<ConnectionId, IDbConnection> _connectionStore;
		private readonly Store<CommandId, IDbCommand> _commandStore;
		private readonly Store<TransactionId, IDbTransaction> _transactionStore;
		private readonly Store<ParameterId, IDbDataParameter> _parameterStore;
		private readonly Store<DataReaderId, IDataReader> _readerStore;
		private readonly ConcurrentParameterToCommandLookup _parametersToTidy;
		public SqlProxy(
			Func<IDbConnection> connectionGenerator,
			Action<QueryCriteria> queryRecorder,
			Action<QueryCriteria> scalarQueryRecorder,
			Action<QueryCriteria> nonQueryRowCountRecorder)
		{
			if (connectionGenerator == null)
				throw new ArgumentNullException(nameof(connectionGenerator));
			if (queryRecorder == null)
				throw new ArgumentNullException(nameof(queryRecorder));
			if (scalarQueryRecorder == null)
				throw new ArgumentNullException(nameof(scalarQueryRecorder));
			if (nonQueryRowCountRecorder == null)
				throw new ArgumentNullException(nameof(nonQueryRowCountRecorder));
			
			_connectionGenerator = connectionGenerator;
			_queryRecorder = queryRecorder;
			_scalarQueryRecorder = scalarQueryRecorder;
			_nonQueryRowCountRecorder = nonQueryRowCountRecorder;

			_connectionStore = new Store<ConnectionId, IDbConnection>(() => new ConnectionId(Guid.NewGuid()));
			_commandStore = new Store<CommandId, IDbCommand>(() => new CommandId(Guid.NewGuid()));
			_transactionStore = new Store<TransactionId, IDbTransaction>(() => new TransactionId(Guid.NewGuid()));
			_parameterStore = new Store<ParameterId, IDbDataParameter>(() => new ParameterId(Guid.NewGuid()));
			_readerStore = new Store<DataReaderId, IDataReader>(() => new DataReaderId(Guid.NewGuid()));

			// Parameters are not disposed of individually (unlike connections, commands, transactions and readers) - instead, the parameters in
			// the parameter store must be removed when the command that created them is disposed. The information to do that is recorded here.
			_parametersToTidy = new ConcurrentParameterToCommandLookup();
		}
	}
}
