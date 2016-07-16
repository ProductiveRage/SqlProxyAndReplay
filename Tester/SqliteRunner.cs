using System;
using System.Data;
using System.Data.SQLite;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.Example;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.Tester
{
	public sealed class SqliteRunner : ISqlRunner
	{
		private static SqliteRunner _instance = new SqliteRunner();
		public static SqliteRunner Instance => _instance;
		private SqliteRunner() { }

		public DataSet Execute(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			using (var connection = new SQLiteConnection(query.ConnectionString))
			{
				using (var command = CreateCommand(connection, query))
				{
					connection.Open();
					using (var dataAdapter = new SQLiteDataAdapter(command))
					{
						var dataSet = new DataSet();
						dataAdapter.Fill(dataSet);
						return dataSet;
					}
				}
			}
		}

		public int ExecuteNonQuery(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			using (var connection = new SQLiteConnection(query.ConnectionString))
			{
				using (var command = CreateCommand(connection, query))
				{
					connection.Open();
					return command.ExecuteNonQuery();
				}
			}
		}

		public object ExecuteScalar(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			using (var connection = new SQLiteConnection(query.ConnectionString))
			{
				using (var command = CreateCommand(connection, query))
				{
					connection.Open();
					return command.ExecuteScalar();
				}
			}
		}

		private static SQLiteCommand CreateCommand(SQLiteConnection connection, QueryCriteria query)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			var command = connection.CreateCommand();
			command.CommandText = query.CommandText;
			command.CommandType = query.CommandType;
			foreach (var p in query.Parameters)
			{
				var parameter = command.CreateParameter();
				parameter.ParameterName = p.ParameterName;
				parameter.Value = p.Value;
				parameter.DbType = p.DbType;
				parameter.Direction = p.Direction;
				parameter.IsNullable = p.IsNullable;
				parameter.Scale = p.Scale;
				parameter.Size = p.Size;
				command.Parameters.Add(parameter);
			}
			return command;
		}
	}
}
