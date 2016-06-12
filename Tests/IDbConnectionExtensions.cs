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
			try
			{
				command.CommandText = sql;
				return command;
			}
			catch
			{
				// If the configuration above fails then this method won't return a command reference and so the caller won't be able
				// to call Dispose on it - so we'll have to do it here, before allowing the exception to blow up
				command.Dispose();
				throw;
			}
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
