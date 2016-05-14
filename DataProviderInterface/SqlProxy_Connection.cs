using System;
using System.Data;
using System.Data.SqlClient;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public Guid GetNewConnectionId() { return _connectionStore.Add(new SqlConnection()); }

		public string GetConnectionString(Guid connectionId) { return _connectionStore.Get(connectionId).ConnectionString; }
		public void SetConnectionString(Guid connectionId, string value) { _connectionStore.Get(connectionId).ConnectionString = value; }
		public int GetConnectionTimeout(Guid connectionId) { return _connectionStore.Get(connectionId).ConnectionTimeout; }
		public string GetDatabase(Guid connectionId) { return _connectionStore.Get(connectionId).Database; }
		public ConnectionState GetState(Guid connectionId) { return _connectionStore.Get(connectionId).State; }

		public void ChangeDatabase(Guid connectionId, string databaseName) { _connectionStore.Get(connectionId).ChangeDatabase(databaseName); }

		public void Open(Guid connectionId) { _connectionStore.Get(connectionId).Open(); }
		void IRemoteSqlConnection.Close(Guid connectionId) { _connectionStore.Get(connectionId).Close(); } // TODO: Use typed ids to avoid explicitly-implementing interface methods?
		void IRemoteSqlConnection.Dispose(Guid connectionId) // TODO: Use typed ids to avoid explicitly-implementing interface methods?
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
