using System.Data;
using System.Data.SqlClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public ConnectionId GetNewConnectionId() { return _connectionStore.Add(new SqlConnection()); }

		public string GetConnectionString(ConnectionId connectionId) { return _connectionStore.Get(connectionId).ConnectionString; }
		public void SetConnectionString(ConnectionId connectionId, string value) { _connectionStore.Get(connectionId).ConnectionString = value; }
		public int GetConnectionTimeout(ConnectionId connectionId) { return _connectionStore.Get(connectionId).ConnectionTimeout; }
		public string GetDatabase(ConnectionId connectionId) { return _connectionStore.Get(connectionId).Database; }
		public ConnectionState GetState(ConnectionId connectionId) { return _connectionStore.Get(connectionId).State; }

		public void ChangeDatabase(ConnectionId connectionId, string databaseName) { _connectionStore.Get(connectionId).ChangeDatabase(databaseName); }

		public void Open(ConnectionId connectionId) { _connectionStore.Get(connectionId).Open(); }
		public void Close(ConnectionId connectionId) { _connectionStore.Get(connectionId).Close(); }
		public void Dispose(ConnectionId connectionId)
		{
			_connectionStore.Get(connectionId).Dispose();
			_connectionStore.Remove(connectionId);
		}

		public TransactionId BeginTransaction(ConnectionId connectionId)
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
		public TransactionId BeginTransaction(ConnectionId connectionId, IsolationLevel il)
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

		public CommandId CreateCommand(ConnectionId connectionId)
		{
			return _commandStore.Add(new SqlCommand
			{
				Connection = _connectionStore.Get(connectionId)
			});
		}
	}
}
