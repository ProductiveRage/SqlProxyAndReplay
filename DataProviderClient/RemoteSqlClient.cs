using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	public sealed class RemoteSqlClient : IDisposable
	{
		private ChannelFactory<ISqlProxy> _proxyChannelFactory;
		private ISqlProxy _proxy;
		private bool _disposed;
		public RemoteSqlClient(Uri endPoint)
		{
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			try
			{
				_proxyChannelFactory = new ChannelFactory<ISqlProxy>(new NetTcpBinding(), new EndpointAddress(endPoint));
				_proxy = _proxyChannelFactory.CreateChannel();
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
				if (_proxyChannelFactory != null)
					((IDisposable)_proxyChannelFactory).Dispose();
			}

			_disposed = true;
		}

		public IDbConnection GetConnection()
		{
			ThrowIfDisposed();
			return new RemoteSqlConnectionClient(
				connection: _proxy,
				command: _proxy,
				transaction: _proxy,
				reader: _proxy,
				connectionId: _proxy.GetNewConnectionId()
			);
		}

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
