using System;
using System.Data;
using System.Data.SQLite;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.Example;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.Tests
{
	public sealed class SqliteRunner : ISqlRunner
	{
		private readonly StaysOpenSqliteConnection _reusableConnection;
		public SqliteRunner(StaysOpenSqliteConnection reusableConnection)
		{
			if (reusableConnection == null)
				throw new ArgumentNullException(nameof(reusableConnection));

			_reusableConnection = reusableConnection;
		}

		public DataSet Execute(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));
			if (query.ConnectionString != _reusableConnection.ConnectionString)
				throw new ArgumentException("The query's ConnectionString should match the reusableConnection's ConnectionString");

			using (var command = CreateCommand(query))
			{
				_reusableConnection.Open();
				using (var dataAdapter = new SQLiteDataAdapter(command))
				{
					var dataSet = new DataSet();
					dataAdapter.Fill(dataSet);
					return dataSet;
				}
			}
		}

		public int ExecuteNonQuery(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));
			if (query.ConnectionString != _reusableConnection.ConnectionString)
				throw new ArgumentException("The query's ConnectionString should match the reusableConnection's ConnectionString");

			using (var command = CreateCommand(query))
			{
				_reusableConnection.Open();
				return command.ExecuteNonQuery();
			}
		}

		public object ExecuteScalar(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));
			if (query.ConnectionString != _reusableConnection.ConnectionString)
				throw new ArgumentException("The query's ConnectionString should match the reusableConnection's ConnectionString");

			using (var command = CreateCommand(query))
			{
				_reusableConnection.Open();
				return command.ExecuteScalar();
			}
		}

		private SQLiteCommand CreateCommand(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			var command = _reusableConnection.CreateCommand();
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
