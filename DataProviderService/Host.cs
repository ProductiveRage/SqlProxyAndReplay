using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService
{
	public sealed class Host : IDisposable
	{
		private ServiceHost _host;
		private bool _disposed;
		public Host(ISqlProxy singleInstanceContextModeProxy, Uri endPoint)
		{
			if (singleInstanceContextModeProxy == null)
				throw new ArgumentNullException(nameof(singleInstanceContextModeProxy));
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			try
			{
				_host = new ServiceHost(singleInstanceContextModeProxy);
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

			// Note should only tidy up managed objects if disposing is true - but the host reference wraps unmanaged resources (so we're only tidying up unmanaged
			// resources here and so we don't need to check whether disposing is true or not)
			var disposableHost = _host as IDisposable;
			if (disposableHost != null)
				disposableHost.Dispose(); // Note: This waits until clients have disconnect

			_disposed = true;
		}
	}
}