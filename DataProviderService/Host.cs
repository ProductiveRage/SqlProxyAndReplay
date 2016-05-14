using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService
{
	public sealed class Host : IDisposable
	{
		private ServiceHost _host;
		private bool _disposed;
		public Host(Uri endPoint)
		{
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			try
			{
				_host = new ServiceHost(typeof(SqlProxy));
				_host.AddServiceEndpoint(typeof(ISqlProxy), new NetTcpBinding(), endPoint);
				_host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
				_host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
				_host.Open();
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
				if (_host != null)
					((IDisposable)_host).Dispose();
			}

			_disposed = true;
		}
	}
}
