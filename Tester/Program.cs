using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using Dapper;
using ProductiveRage.SqlProxyAndReplay.DataProviderClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderService;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.Example;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.PassThrough;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			var proxyEndPoint = new Uri("net.tcp://localhost:5000/SqlProxy");
			var replayEndPoint = new Uri("net.tcp://localhost:5001/SqlProxy");

			var terminationIndicator = new ManualResetEvent(initialState: false);
			ConfigureHost(proxyEndPoint, replayEndPoint, terminationIndicator);
			ExecuteClientCalls(proxyEndPoint, replayEndPoint);
			terminationIndicator.Set();
		}

		private static void ConfigureHost(Uri proxyEndPoint, Uri replayEndPoint, EventWaitHandle terminationIndicator)
		{
			if (proxyEndPoint == null)
				throw new ArgumentNullException(nameof(proxyEndPoint));
			if (replayEndPoint == null)
				throw new ArgumentNullException(nameof(replayEndPoint));
			if (terminationIndicator == null)
				throw new ArgumentNullException(nameof(terminationIndicator));

			new Thread(() =>
			{
				// In this example, a Sqlite database is being used since it means that there is no dependency of this project on a SQL Server
				// installation somewhere. But it would be easy for the host to connect to a SQL Server database, SqlRunner.Instance would be
				// specified (referencing a class in the DataProviderService.Example project) rather than SqliteRunner.Instance for the
				// DiskCache and the first argument of the Host constructor would return a new SqlConnection, instead of a SQLiteConnection.
				var cache = new DiskCache(SqliteRunner.Instance, cacheFolder: new DirectoryInfo("Cache"), infoLogger: Console.WriteLine);
				using (var proxyHost = new Host(new SqlProxy(() => new SQLiteConnection(), cache.QueryRecorder, cache.ScalarQueryRecorder, cache.NonQueryRowCountRecorder), proxyEndPoint))
				{
					using (var replayHost = new Host(new SqlReplayer(cache.DataRetriever, cache.ScalarDataRetriever, cache.NonQueryRowCountRetriever), replayEndPoint))
					{
						terminationIndicator.WaitOne();
					}
				}
			}).Start();
		}

		private static void ExecuteClientCalls(Uri proxyEndPoint, Uri replayEndPoint)
		{
			if (proxyEndPoint == null)
				throw new ArgumentNullException(nameof(proxyEndPoint));
			if (replayEndPoint == null)
				throw new ArgumentNullException(nameof(replayEndPoint));

			// A Sqlite database is being used here to avoid this project depending upon being able to access SQL Server installation (or
			// other database) somewhere, so the connection string and query are in Sqlite formats
			var connectionString = "data source=Blog.sqlite; Version=3;";
			var sql = "SELECT * FROM Posts WHERE Title LIKE '%' || @title || '%'";

			using (var conn = new SQLiteConnection(connectionString))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					using (var cmd = new SQLiteCommand(sql, conn, transaction))
					{
						cmd.Parameters.AddWithValue("title", "C#");
						using (var rdr = cmd.ExecuteReader())
						{
							Console.WriteLine("From direct SQL call..");
							while (rdr.Read())
							{
								Console.WriteLine(rdr.GetString(rdr.GetOrdinal("Title")));
							}
							Console.WriteLine();
						}
					}
				}
			}
			using (var conn = new RemoteSqlClient(connectionString, proxyEndPoint))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					using (var cmd = conn.CreateCommand(sql, transaction: transaction))
					{
						cmd.Parameters.AddWithValue("title", "C#");
						using (var rdr = cmd.ExecuteReader())
						{
							Console.WriteLine("Raw SQL via proxy..");
							while (rdr.Read())
							{
								Console.WriteLine(rdr.GetString(rdr.GetOrdinal("Title")));
							}
							Console.WriteLine();
						}
					}
				}
			}
			using (var conn = new RemoteSqlClient(connectionString, proxyEndPoint))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					Console.WriteLine("Dapper via proxy..");
					var posts = conn.Query<Post>(sql, new { title = "C#" }, transaction: transaction);
					foreach (var post in posts)
						Console.WriteLine(post.Title);
					Console.WriteLine();
				}
			}
			using (var conn = new RemoteSqlClient(connectionString, replayEndPoint))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					using (var cmd = conn.CreateCommand(sql, transaction))
					{
						cmd.Parameters.AddWithValue("title", "C#");
						using (var rdr = cmd.ExecuteReader())
						{
							Console.WriteLine("Raw SQL via replay proxy..");
							while (rdr.Read())
							{
								Console.WriteLine(rdr.GetString(rdr.GetOrdinal("Title")));
							}
							Console.WriteLine();
						}
					}
				}
			}
			using (var conn = new RemoteSqlClient(connectionString, replayEndPoint))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					Console.WriteLine("Dapper via replay proxy..");
					var posts = conn.Query<Post>(sql, new { title = "C#" }, transaction: transaction);
					foreach (var post in posts)
						Console.WriteLine(post.Title);
					Console.WriteLine();
				}
			}
			Console.WriteLine("Press [Enter] to terminate..");
			Console.ReadLine();
		}
	}
}
