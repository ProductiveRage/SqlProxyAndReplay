using System;
using System.Data.SqlClient;
using Dapper;
using ProductiveRage.SqlProxyAndReplay.DataProviderClient;

namespace ProductiveRage.SqlProxyAndReplay.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			// Note: Need the DataProviderServiceTester project to be running in order for these connections to be handled
			var connectionServerEndPoint = new Uri("net.tcp://localhost:5000/RemoteSqlConnection");
			var commandChannelFactory = new Uri("net.tcp://localhost:5000/RemoteSqlCommand");
			var transactionServerEndPoint = new Uri("net.tcp://localhost:5000/RemoteSqlTransaction");
			var readerServerEndPoint = new Uri("net.tcp://localhost:5000/RemoteSqlDataReader");

			var connectionString =
				new SqlConnectionStringBuilder
				{
					DataSource = ".",
					InitialCatalog = "NORTHWND",
					IntegratedSecurity = true
				}
				.ToString();

			var sql = "SELECT TOP 10 * FROM Products";
			using (var conn = new SqlConnection(connectionString))
			{
				conn.Open();
				using (var transaction = conn.BeginTransaction())
				{
					using (var cmd = conn.CreateCommand(sql))
					{
						cmd.Transaction = transaction;
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
			using (var connCreator = new RemoteSqlClient(connectionServerEndPoint, commandChannelFactory, transactionServerEndPoint, readerServerEndPoint))
			{
				using (var conn = connCreator.GetConnection(connectionString))
				{
					using (var cmd = conn.CreateCommand(sql))
					{
						conn.Open();
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
			using (var connCreator = new RemoteSqlClient(connectionServerEndPoint, commandChannelFactory, transactionServerEndPoint, readerServerEndPoint))
			{
				using (var conn = connCreator.GetConnection(connectionString))
				{
					conn.Open();
					Console.WriteLine("Dapper via proxy..");
					var products = conn.Query<Product>(sql);
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
