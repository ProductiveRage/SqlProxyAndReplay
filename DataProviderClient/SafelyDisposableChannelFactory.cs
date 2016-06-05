using System;
using System.ServiceModel;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	public static class ChannelFactoryExtensions
	{
		/// <summary>
		/// Wrap a ChannelFactory in a SafelyDisposableChannelFactory so that it may be used within a using block without having to worry about dealing
		/// with the case where the channel factory may have faulted (in which case Dispose may not be called, Abort must be called instead)
		/// </summary>
		public static SafelyDisposableChannelFactory<T> AsSafelyDisposable<T>(this ChannelFactory<T> channelFactory)
		{
			if (channelFactory == null)
				throw new ArgumentNullException(nameof(channelFactory));
			return new SafelyDisposableChannelFactory<T>(channelFactory);
		}
	}

	/// <summary>
	/// If a ChannelFactory enters a faulted state then the only method that may be called on it is Abort - not even Dispose is acceptable. So, if a
	/// ChannelFactory is in a faulted state then Dispose must not be called when the reference is finished with, which is a problem if you want to
	/// wrap it in a using statement. This class acts as a wrapper that ensures that the appropriate Abort-or-Dispose method be called.
	/// </summary>
	public sealed class SafelyDisposableChannelFactory<T> : IDisposable
	{
		private ChannelFactory<T> _channelFactory;
		private bool _faulted, _disposed;
		internal SafelyDisposableChannelFactory(ChannelFactory<T> channelFactory)
		{
			if (channelFactory == null)
				throw new ArgumentNullException(nameof(channelFactory));

			_channelFactory = channelFactory;
			_channelFactory.Faulted += SetFaulted;
			_faulted = false;
			_disposed = false;
		}
		~SafelyDisposableChannelFactory()
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

			// Note should only tidy up managed objects if disposing is true - but the channel factory wraps an unmanaged resources (so we're only tidying
			// up unmanaged resources and so we don't need to check whether disposing is true or not)

			// There is an oddity to be aware of - if the channel factory entered a "faulted state" (meaning that an exception was thrown by the service)
			// then any subsequent method calls will fail, including any attempt to Close or Dispose the channel factory. The way to avoid this is to call
			// Abort on the proxy when disposing if a fault occurred (see https://msdn.microsoft.com/en-us/library/aa355056.aspx for more information).
			if (_channelFactory != null)
				_channelFactory.Faulted -= SetFaulted;
			if (_faulted)
				_channelFactory.Abort();
			else if (_channelFactory != null)
				((IDisposable)_channelFactory).Dispose();

			_disposed = true;
		}

		public ChannelFactory<T> ChannelFactory
		{
			get
			{
				if (_disposed)
					throw new ObjectDisposedException("client");
				return _channelFactory;
			}
		}

		private void SetFaulted(object sender, EventArgs e)
		{
			_faulted = true;
		}
	}
}
