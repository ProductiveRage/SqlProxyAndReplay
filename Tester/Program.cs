using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using Dapper;
using ProductiveRage.SqlProxyAndReplay.DataProviderClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;
using ProductiveRage.SqlProxyAndReplay.DataProviderService;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.Example;

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
				var cache = new DiskCache(SqlRunner.Instance, cacheFolder: new DirectoryInfo("Cache"), infoLogger: Console.WriteLine);
				using (var proxyHost = new Host(new SqlProxy(() => new SqlConnection(), cache.QueryRecorder, cache.ScalarQueryRecorder, cache.NonQueryRowCountRecorder), proxyEndPoint))
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

			// Note: Need the DataProviderServiceTester project to be running in order for these connections to be handled
			var connectionString =
				new SqlConnectionStringBuilder
				{
					DataSource = ".",
					InitialCatalog = "NORTHWND",
					IntegratedSecurity = true
				}
				.ToString();

			var sql = "SELECT TOP 10 * FROM Products WHERE ProductName LIKE '%' + @name + '%'";
			using (var conn = new SqlConnection(connectionString))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					using (var cmd = new SqlCommand(sql, conn, transaction))
					{
						cmd.Parameters.AddWithValue("name", "Bob");
						using (var rdr = cmd.ExecuteReader())
						{
							Console.WriteLine("From direct SQL call..");
							while (rdr.Read())
							{
								Console.WriteLine(rdr.GetString(rdr.GetOrdinal("ProductName")));
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
						cmd.Parameters.AddWithValue("name", "Bob");
						using (var rdr = cmd.ExecuteReader())
						{
							Console.WriteLine("Raw SQL via proxy..");
							while (rdr.Read())
							{
								Console.WriteLine(rdr.GetString(rdr.GetOrdinal("ProductName")));
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
					var products = conn.Query<Product>(sql, new { name = "Bob" }, transaction: transaction);
					foreach (var product in products)
						Console.WriteLine(product.ProductName);
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
						cmd.Parameters.AddWithValue("name", "Bob");
						using (var rdr = cmd.ExecuteReader())
						{
							Console.WriteLine("Raw SQL via replay proxy..");
							while (rdr.Read())
							{
								Console.WriteLine(rdr.GetString(rdr.GetOrdinal("ProductName")));
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
					var products = conn.Query<Product>(sql, new { name = "Bob" }, transaction: transaction);
					foreach (var product in products)
						Console.WriteLine(product.ProductName);
					Console.WriteLine();
				}
			}
			Console.WriteLine("Press [Enter] to terminate..");
			Console.ReadLine();
		}
	}
}
