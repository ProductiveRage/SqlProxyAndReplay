using System;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	internal sealed class RemoteSqlTransactionClient : IDbTransaction
	{
		private readonly IRemoteSqlConnection _connection;
		private readonly IRemoteSqlCommand _command;
		private readonly IRemoteSqlTransaction _transaction;
		private readonly IRemoteSqlDataReader _reader;
		private bool _disposed;
		public RemoteSqlTransactionClient(
			IRemoteSqlConnection connection,
			IRemoteSqlCommand command,
			IRemoteSqlTransaction transaction,
			IRemoteSqlDataReader reader,
			TransactionId transactionId)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (command == null)
				throw new ArgumentNullException(nameof(command));
			if (transaction == null)
				throw new ArgumentNullException(nameof(transaction));
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			_connection = connection;
			_command = command;
			_transaction = transaction;
			_reader = reader;
			TransactionId = transactionId;
			_disposed = false;
		}
		~RemoteSqlTransactionClient()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
				_transaction.Dispose(TransactionId); // Tell the service that the current Transaction is finished with

			_disposed = true;
		}

		public TransactionId TransactionId { get; }

		public IDbConnection Connection
		{
			get { ThrowIfDisposed(); return new RemoteSqlConnectionClient(_connection, _command, _transaction, _reader, _transaction.GetConnection(TransactionId)); }
		}
		public IsolationLevel IsolationLevel
		{
			get { ThrowIfDisposed(); return _transaction.GetIsolationLevel(TransactionId); }
		}

		public void Commit() { ThrowIfDisposed(); _transaction.Commit(TransactionId); }
		public void Rollback() { ThrowIfDisposed(); _transaction.Rollback(TransactionId); }

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("transaction");
		}
	}
}
