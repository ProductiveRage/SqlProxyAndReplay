using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	public sealed class RemoteSqlClient : IDisposable
	{
		private ChannelFactory<IRemoteSqlConnection> _connectionChannelFactory;
		private ChannelFactory<IRemoteSqlCommand> _commandChannelFactory;
		private ChannelFactory<IRemoteSqlTransaction> _translationChannelFactory;
		private ChannelFactory<IRemoteSqlDataReader> _readerChannelFactory;
		private IRemoteSqlConnection _connection;
		private IRemoteSqlCommand _command;
		private IRemoteSqlTransaction _transaction;
		private IRemoteSqlDataReader _reader;
		private bool _disposed;
		public RemoteSqlClient(Uri connectionServerEndPoint, Uri commandServerEndPoint, Uri transactionServerEndPoint, Uri readerServerEndPoint)
		{
			if (connectionServerEndPoint == null)
				throw new ArgumentNullException(nameof(connectionServerEndPoint));
			if (commandServerEndPoint == null)
				throw new ArgumentNullException(nameof(commandServerEndPoint));
			if (transactionServerEndPoint == null)
				throw new ArgumentNullException(nameof(transactionServerEndPoint));
			if (readerServerEndPoint == null)
				throw new ArgumentNullException(nameof(readerServerEndPoint));

			try
			{
				_connectionChannelFactory = new ChannelFactory<IRemoteSqlConnection>(new NetTcpBinding(), new EndpointAddress(connectionServerEndPoint));
				_connection = _connectionChannelFactory.CreateChannel();
				_commandChannelFactory = new ChannelFactory<IRemoteSqlCommand>(new NetTcpBinding(), new EndpointAddress(commandServerEndPoint));
				_command = _commandChannelFactory.CreateChannel();
				_translationChannelFactory = new ChannelFactory<IRemoteSqlTransaction>(new NetTcpBinding(), new EndpointAddress(transactionServerEndPoint));
				_transaction = _translationChannelFactory.CreateChannel();
				_readerChannelFactory = new ChannelFactory<IRemoteSqlDataReader>(new NetTcpBinding(), new EndpointAddress(readerServerEndPoint));
				_reader = _readerChannelFactory.CreateChannel();
			}
			catch
			{
				Dispose(true);
				throw;
			}
			_disposed = false;
		}
		~RemoteSqlClient()
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
			{
				if (_connectionChannelFactory != null)
					((IDisposable)_connectionChannelFactory).Dispose();
				if (_commandChannelFactory != null)
					((IDisposable)_commandChannelFactory).Dispose();
				if (_translationChannelFactory != null)
					((IDisposable)_translationChannelFactory).Dispose();
				if (_readerChannelFactory != null)
					((IDisposable)_readerChannelFactory).Dispose();
			}

			_disposed = true;
		}

		public IDbConnection GetConnection() { ThrowIfDisposed(); return new RemoteSqlConnectionClient(_connection, _command, _transaction, _reader, _connection.GetNewId()); }

		public IDbConnection GetConnection(string connectionString)
		{
			ThrowIfDisposed();
			var connection = GetConnection();
			connection.ConnectionString = connectionString;
			return connection;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("client");
		}
	}
}
