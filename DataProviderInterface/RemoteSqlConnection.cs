using System;
using System.Data;
using System.Data.SqlClient;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed class RemoteSqlConnection : IRemoteSqlConnection
	{
		private readonly Store<SqlConnection> _connectionStore;
		private readonly Store<IDbTransaction> _transactionStore;
		public RemoteSqlConnection(Store<SqlConnection> connectionStore, Store<IDbTransaction> transactionStore)
		{
			if (connectionStore == null)
				throw new ArgumentNullException(nameof(connectionStore));
			if (transactionStore == null)
				throw new ArgumentNullException(nameof(transactionStore));

			_connectionStore = connectionStore;
			_transactionStore = transactionStore;
		}
		public RemoteSqlConnection() : this(DefaultStores.ConnectionStore, DefaultStores.TransactionStore) { }

		public Guid GetNewId() { return _connectionStore.Add(new SqlConnection()); }

		public string GetConnectionString(Guid connectionId) { return _connectionStore.Get(connectionId).ConnectionString; }
		public void SetConnectionString(Guid connectionId, string value) { _connectionStore.Get(connectionId).ConnectionString = value; }
		public int GetConnectionTimeout(Guid connectionId) { return _connectionStore.Get(connectionId).ConnectionTimeout; }
		public string GetDatabase(Guid connectionId) { return _connectionStore.Get(connectionId).Database; }
		public ConnectionState GetState(Guid connectionId) { return _connectionStore.Get(connectionId).State; }

		public void ChangeDatabase(Guid connectionId, string databaseName) { _connectionStore.Get(connectionId).ChangeDatabase(databaseName); }

		public void Open(Guid connectionId) { _connectionStore.Get(connectionId).Open(); }
		public void Close(Guid connectionId) { _connectionStore.Get(connectionId).Close(); }
		public void Dispose(Guid connectionId)
		{
			_connectionStore.Get(connectionId).Dispose();
			_connectionStore.Remove(connectionId);
		}

		public Guid BeginTransaction(Guid connectionId)
		{
			var transaction = _connectionStore.Get(connectionId).BeginTransaction();
			try
			{
				return _transactionStore.Add(transaction);
			}
			catch
			{
				transaction.Dispose();
				throw;
			}
		}
		public Guid BeginTransaction(Guid connectionId, IsolationLevel il)
		{
			var transaction = _connectionStore.Get(connectionId).BeginTransaction(il);
			try
			{
				return _transactionStore.Add(transaction);
			}
			catch
			{
				transaction.Dispose();
				throw;
			}
		}

	}
}
