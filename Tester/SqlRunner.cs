using System;
using System.Data;
using System.Data.SqlClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderServiceProductiveRage.SqlProxyAndReplay.Tester
{
	public sealed class SqlRunner : ISqlRunner
	{
		private static SqlRunner _instance = new SqlRunner();
		public static SqlRunner Instance => _instance;
		private SqlRunner() { }

		public DataSet Execute(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = CreateCommand(connection, query))
				{
					connection.Open();
					using (var dataAdapter = new SqlDataAdapter(command))
					{
						var dataSet = new DataSet();
						dataAdapter.Fill(dataSet);
						return dataSet;
					}
				}
			}
		}

		public object ExecuteScalar(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = CreateCommand(connection, query))
				{
					connection.Open();
					return command.ExecuteScalar();
				}
			}
		}

		public int ExecuteNonQuery(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = CreateCommand(connection, query))
				{
					connection.Open();
					return command.ExecuteNonQuery();
				}
			}
		}

		private static SqlCommand CreateCommand(SqlConnection connection, QueryCriteria query)
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