using System;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	public sealed class RemoteSqlClient : IDbConnection, IDisposable
	{
		private ChannelFactory<ISqlProxy> _proxyChannelFactory;
		private ISqlProxy _proxy;
		private RemoteSqlConnectionClient _connection;
		private bool _faulted, _disposed;
		private readonly bool _disposeChannelFactory;
		public RemoteSqlClient(string connectionString, Uri endPoint) : this(connectionString, endPoint, GetDefaultBinding()) { }
		public RemoteSqlClient(string connectionString, Uri endPoint, Binding binding)
			: this(connectionString, new ChannelFactory<ISqlProxy>(binding, new EndpointAddress(endPoint, GetDefaultIdentity())), disposeChannelFactory: true) { }
		public RemoteSqlClient(string connectionString, ChannelFactory<ISqlProxy> proxyChannelFactory)
			: this(connectionString, proxyChannelFactory, disposeChannelFactory: false) { }
		private RemoteSqlClient(string connectionString, ChannelFactory<ISqlProxy> proxyChannelFactory, bool disposeChannelFactory)
		{
			if (connectionString == null)
				throw new ArgumentNullException(nameof(connectionString));
			if (proxyChannelFactory == null)
				throw new ArgumentNullException(nameof(proxyChannelFactory));

			try
			{
				_disposeChannelFactory = disposeChannelFactory;
				_proxyChannelFactory = proxyChannelFactory;
				_proxy = _proxyChannelFactory.CreateChannel();
				((ICommunicationObject)_proxy).Faulted += SetFaulted;
				_connection = new RemoteSqlConnectionClient(_proxy, connectionId: _proxy.GetNewConnectionId());
				_connection.ConnectionString = connectionString;
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
			{
				_connection.Dispose();
				proxyCommunicationObject.Close();
			}
			if (_disposeChannelFactory && !_faulted && (_proxyChannelFactory != null))
				((IDisposable)_proxyChannelFactory).Dispose();

			_disposed = true;
		}

		private static NetTcpBinding GetDefaultBinding()
		{
			return new NetTcpBinding
			{
				MaxBufferPoolSize = 2147483647,
				MaxReceivedMessageSize = 2147483647,
				MaxBufferSize = 2147483647
			};
		}

		private static EndpointIdentity GetDefaultIdentity()
		{
			// See http://stackoverflow.com/a/19849782/3813189 or http://inaspiralarray.blogspot.co.uk/2013/05/wcf-security-issue-target-principal.html
			// "if a dummy SPN is used, Kerberos authentication will fail, however in this case authentication will fall back to NTLM and succeed."
			// This prevents the error "A call to SSPI failed, see inner exception." (inner: "The target principal name is incorrect")/
			return EndpointIdentity.CreateSpnIdentity("");
		}

		private void SetFaulted(object sender, EventArgs e)
		{
			_faulted = true;
		}

		public string ConnectionString
		{
			get { ThrowIfDisposed(); return _connection.ConnectionString; }
			set { ThrowIfDisposed(); _connection.ConnectionString = value; }
		}
		public int ConnectionTimeout { get { ThrowIfDisposed(); return _connection.ConnectionTimeout; } }
		public string Database { get { ThrowIfDisposed(); return _connection.Database; } }
		public ConnectionState State { get { ThrowIfDisposed(); return _connection.State; } }
		public void ChangeDatabase(string databaseName) { ThrowIfDisposed(); _connection.BeginTransaction(); }
		public void Open() { ThrowIfDisposed(); _connection.Open(); }
		public void Close() { ThrowIfDisposed(); _connection.Close(); }
		public IDbTransaction BeginTransaction() { ThrowIfDisposed(); return _connection.BeginTransaction(); }
		public IDbTransaction BeginTransaction(IsolationLevel il) { ThrowIfDisposed(); return _connection.BeginTransaction(il); }
		public RemoteSqlCommandClient CreateCommand() { ThrowIfDisposed(); return _connection.CreateCommand(); }
		IDbCommand IDbConnection.CreateCommand() { return CreateCommand(); }

		// This method isn't part of IDbConnection but it's very convenient, so I'm including it here
		public RemoteSqlCommandClient CreateCommand(string commandText, IDbTransaction transaction = null, CommandType commandType = CommandType.Text)
		{
			if (string.IsNullOrWhiteSpace(commandText))
				throw new ArgumentException("Null/blank " + nameof(commandText) + " specified");
			RemoteSqlTransactionClient remoteSqlTransaction;
			if (transaction == null)
				remoteSqlTransaction = null;
			else
			{
				remoteSqlTransaction = transaction as RemoteSqlTransactionClient;
				if (remoteSqlTransaction == null)
					throw new ArgumentException($"Transaction must be a {typeof(RemoteSqlTransactionClient)}");
			}
			var command = CreateCommand();
			try
			{
				command.CommandText = commandText;
				command.CommandType = commandType;
				if (remoteSqlTransaction != null)
					command.Transaction = remoteSqlTransaction;
				return command;
			}
			catch
			{
				// If the configuration above fails then this method won't return a command reference and so the caller won't be able
				// to call Dispose on it - so we'll have to do it here, before allowing the exception to blow up
				command.Dispose();
				throw;
			}
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("client");
		}
	}
}
