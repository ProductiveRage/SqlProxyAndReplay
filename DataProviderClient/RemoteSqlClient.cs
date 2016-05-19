using System;
using System.Data;
using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	public sealed class RemoteSqlClient : IDbConnection, IDisposable
	{
		private ChannelFactory<ISqlProxy> _proxyChannelFactory;
		private ISqlProxy _proxy;
		private RemoteSqlConnectionClient _connection;
		private bool _faulted, _disposed;
		public RemoteSqlClient(string connectionString, Uri endPoint)
		{
			if (connectionString == null)
				throw new ArgumentNullException(nameof(connectionString));
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			try
			{
				_proxyChannelFactory = new ChannelFactory<ISqlProxy>(new NetTcpBinding(), new EndpointAddress(endPoint));
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
			if (!_faulted && (_proxyChannelFactory != null))
				((IDisposable)_proxyChannelFactory).Dispose();

			_disposed = true;
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
			command.CommandText = commandText;
			command.CommandType = commandType;
			if (remoteSqlTransaction != null)
				command.Transaction = remoteSqlTransaction;
			return command;
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("client");
		}
	}
}
