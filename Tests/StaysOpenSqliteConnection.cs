using System;
using System.Data;
using System.Data.SQLite;

namespace ProductiveRage.SqlProxyAndReplay.Tests
{
	/// <summary>
	/// When using an in-memory Sqlite database, if a connection is opened and database table(s) created, they are lost again when the connection is closed.
	/// To work around that, a connection may be opened and the data populated and then closing of the connection be disabled - any calls to Close will be
	/// ignored while any calls to Open when the connection is already open will be ignored. In fact, this class even goes one further - if an instance of
	/// this class if passed around as an IDbConnection then any Dispose call will be ignored, which allows a connection to be given to a SqlProxy instance
	/// as the result of the connectionGenerator delegate and for the connection to then be re-used (which it couldn't be if the IDbConnection.Dispose call
	/// that the SqlProxy will make actually closed the connection). If the Dispose method is called on a reference that is of type StaysOpenSqliteConnection
	/// then it WILL be disposed correctly - this allows for an instance to be declared within a using and then re-used several times, in ways that would
	/// otherwise close it.
	/// </summary>
	public sealed class StaysOpenSqliteConnection : IDbConnection
	{
		private SQLiteConnection _connection;
		private bool _disposed = false;
		public StaysOpenSqliteConnection()
		{
			_connection = new SQLiteConnection();
			_disposed = false;
		}
		~StaysOpenSqliteConnection()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			// Note: This is only executed when Dispose is called against an instance of this class that is referenced as a StaysOpenSqliteConnection, if
			// something has a reference of type IDisposable then Dispose is treated as a no-op so that the connection can continued to be used in other
			// places (until the point at which it was instantiated, as a a StaysOpenSqliteConnection and within a using statement, has finished with it)
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_connection.Dispose();
				_connection = null;
				_disposed = true;
			}
		}

		public string ConnectionString
		{
			get { ThrowIfDisposed(); return _connection.ConnectionString; }
			set { ThrowIfDisposed(); _connection.ConnectionString = value; }
		}
		public int ConnectionTimeout { get { ThrowIfDisposed(); return _connection.ConnectionTimeout; } }
		public string Database { get { ThrowIfDisposed(); return _connection.Database; } }
		public ConnectionState State { get { ThrowIfDisposed(); return _connection.State; } }
		public IDbTransaction BeginTransaction() { ThrowIfDisposed(); return _connection.BeginTransaction(); }
		public IDbTransaction BeginTransaction(IsolationLevel il) { ThrowIfDisposed(); return _connection.BeginTransaction(il); }
		public void ChangeDatabase(string databaseName) { ThrowIfDisposed(); _connection.ChangeDatabase(databaseName); }
		public SQLiteCommand CreateCommand() { ThrowIfDisposed(); return _connection.CreateCommand(); }
		IDbCommand IDbConnection.CreateCommand() { return CreateCommand(); }

		public void Open()
		{
			// This will only try to open the connection if it's not already open - it was previously opened and then Close was called
			// then the connection will still be open, in which case we don't want to second call to Open to throw
			ThrowIfDisposed();
			if (_connection.State != ConnectionState.Open)
				_connection.Open();
		}
		public void Close()
		{
			// This will NOT close the connection, it will leave it open (if it's been opened by this point)
			ThrowIfDisposed();
		}
		void IDisposable.Dispose()
		{
			// This will NOT dispose of the connection - it is only truly disposed when accessed as a StaysOpenSqliteConnection and not
			// just as an IDisposable reference (this allows an instance of this to be instantiated within a using block but to be used
			// multiple times within that block, even in ways that would ordinarily close it as an IDiposable)
			ThrowIfDisposed();
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException("connection");
		}
	}
}
