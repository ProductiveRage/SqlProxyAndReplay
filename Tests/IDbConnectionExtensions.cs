using System;
using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.Tests
{
	public static class IDbConnectionExtensions
	{
		public static IDbCommand CreateCommand(this IDbConnection connection, string sql)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (string.IsNullOrWhiteSpace(sql))
				throw new ArgumentException($"Null/blank {nameof(sql)} specified");

			var command = connection.CreateCommand();
			command.CommandText = sql;
			return command;
		}

		public static void Execute(this IDbConnection connection, string sql)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (string.IsNullOrWhiteSpace(sql))
				throw new ArgumentException($"Null/blank {nameof(sql)} specified");

			using (var command = connection.CreateCommand(sql))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}
