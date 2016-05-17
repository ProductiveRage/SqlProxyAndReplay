using System;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	public sealed class RemoteSqlClient : IDisposable
	{
		private ChannelFactory<ISqlProxy> _proxyChannelFactory;
		private ISqlProxy _proxy;
		private bool _faulted, _disposed;
		public RemoteSqlClient(Uri endPoint)
		{
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			try
			{
				_proxyChannelFactory = new ChannelFactory<ISqlProxy>(new NetTcpBinding(), new EndpointAddress(endPoint));
				_proxy = _proxyChannelFactory.CreateChannel();
				((ICommunicationObject)_proxy).Faulted += SetFaulted;

			}
			catch
			{
				Dispose(true);
				throw;
			}
			_faulted = false;
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

			// Note should only tidy up managed objects if disposing is true - but the proxy object and the channel factory wrap unmanaged resources
			// (so we're only tidying up unmanaged resources and so we don't need to check whether disposing is true or not)

			// There is an oddity to be aware of when communicating over a channel - if it enters a "faulted state" (meaning that an exception was thrown)
			// then an subsequent method calls will fail, including any attempt to Close or Dispose the channel factory. The way to avoid this is to call
			// Abort on the proxy when disposing, if a fault occurred (see https://msdn.microsoft.com/en-us/library/aa355056.aspx for more information).
			var proxyCommunicationObject = _proxy as ICommunicationObject;
			if (proxyCommunicationObject != null)
				proxyCommunicationObject.Faulted -= SetFaulted;
			if (_faulted)
			{
				if (proxyCommunicationObject != null)
					proxyCommunicationObject.Abort();
			}
			else
				proxyCommunicationObject.Close();
			if (!_faulted && (_proxyChannelFactory != null))
				((IDisposable)_proxyChannelFactory).Dispose();

			_disposed = true;
		}

		private void SetFaulted(object sender, EventArgs e)
		{
			_faulted = true;
		}

		public RemoteSqlConnectionClient GetConnection()
		{
			ThrowIfDisposed();
			return new RemoteSqlConnectionClient(
				connection: _proxy,
				command: _proxy,
				transaction: _proxy,
				parameters: _proxy,
				parameter: _proxy,
				reader: _proxy,
				connectionId: _proxy.GetNewConnectionId()
			);
		}

		public RemoteSqlConnectionClient GetConnection(string connectionString)
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
