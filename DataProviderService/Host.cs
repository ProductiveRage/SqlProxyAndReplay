using System;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService
{
	public sealed class Host : IDisposable
	{
		private ServiceHost _connectionHost, _commandHost, _readerHost;
		private bool _disposed;
		public Host(Uri connectionServerEndPoint, Uri commandServerEndPoint, Uri readerServerEndPoint)
		{
			if (connectionServerEndPoint == null)
				throw new ArgumentNullException(nameof(connectionServerEndPoint));
			if (commandServerEndPoint == null)
				throw new ArgumentNullException(nameof(commandServerEndPoint));
			if (readerServerEndPoint == null)
				throw new ArgumentNullException(nameof(readerServerEndPoint));

			try
			{
				_connectionHost = new ServiceHost(typeof(RemoteSqlConnection));
				_connectionHost.AddServiceEndpoint(typeof(IRemoteSqlConnection), new NetTcpBinding(), connectionServerEndPoint);
				_connectionHost.Open();

				_commandHost = new ServiceHost(typeof(RemoteSqlCommand));
				_commandHost.AddServiceEndpoint(typeof(IRemoteSqlCommand), new NetTcpBinding(), commandServerEndPoint);
				_commandHost.Open();

				_readerHost = new ServiceHost(typeof(RemoteSqlDataReader));
				_readerHost.AddServiceEndpoint(typeof(IRemoteSqlDataReader), new NetTcpBinding(), readerServerEndPoint);
				_readerHost.Open();
			}
			catch
			{
				Dispose();
				throw;
			}
			_disposed = false;
		}

		~Host()
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
				// TODO: Note: This waits until clients have disconnect
				if (_connectionHost != null)
					((IDisposable)_connectionHost).Dispose();
				if (_commandHost != null)
					((IDisposable)_commandHost).Dispose();
				if (_readerHost != null)
					((IDisposable)_readerHost).Dispose();
			}

			_disposed = true;
		}
	}
}
