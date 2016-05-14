using System;
using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.Tester
{
	public static class IDbConnectionExtensions
	{
		public static IDbCommand CreateCommand(this IDbConnection conn, string commandText, CommandType commandType = CommandType.Text)
		{
			if (conn == null)
				throw new ArgumentNullException(nameof(conn));
			if (string.IsNullOrWhiteSpace("commandText"))
				throw new ArgumentException($"Null/blank/whitespace-only {nameof(commandText)} specified");

			var command = conn.CreateCommand();
			command.CommandText = commandText;
			command.CommandType = commandType;
			return command;
		}
	}
}
