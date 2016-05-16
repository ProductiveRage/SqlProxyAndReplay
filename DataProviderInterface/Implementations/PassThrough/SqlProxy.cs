using System;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough
{
	// This class should have no problems dealing with multiple requests interacting with it conccurently, so it makes sense to configure it for "Single"
	// instance context mode; meaning that a single instance will be shared across all calls. (It would make no sense for a new instance to be created
	// for each call to the service, since data must be persisted across calls for each operation on the client - eg. create connection, create command,
	// execute command, read data - it could make sense for a single instance to be used per session but there is little advantage to doing this rather
	// than sharing a single instance across all calls and all sessions).
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public sealed partial class SqlProxy : ISqlProxy
	{
		private readonly Store<ConnectionId, SqlConnection> _connectionStore;
		private readonly Store<CommandId, IDbCommand> _commandStore;
		private readonly Store<TransactionId, IDbTransaction> _transactionStore;
		private readonly Store<ParameterId, IDbDataParameter> _parameterStore;
		private readonly Store<DataReaderId, IDataReader> _readerStore;
		private readonly ConcurrentParameterToCommandLookup _parametersToTidy;
		public SqlProxy()
		{
			_connectionStore = new Store<ConnectionId, SqlConnection>(() => new ConnectionId(Guid.NewGuid()));
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
