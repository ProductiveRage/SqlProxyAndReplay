﻿using System;
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
			var proxyEndPoint = new Uri("net.tcp://localhost:5000/SqlProxy");
			var replayEndPoint = new Uri("net.tcp://localhost:5001/SqlProxy");
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
