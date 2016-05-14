using System;
using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed class RemoteSqlTransaction : IRemoteSqlTransaction
	{
		private readonly IRemoteSqlConnection _connection;

		public RemoteSqlTransaction(RemoteSqlConnection connection, IsolationLevel? isolationLevel = null)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			_connection = connection;
			/* TODO
			Transaction = (isolationLevel == null)
				? connection.Connection.BeginTransaction()
				: connection.Connection.BeginTransaction(isolationLevel.Value);
				*/
		}

		public Guid TransactionId { get; }

		public Guid GetConnection(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public IsolationLevel GetIsolationLevel(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Commit(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
		public void Rollback(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
		public void Dispose(Guid transactionId)
		{
			throw new NotImplementedException(); // TODO
		}
	}
}