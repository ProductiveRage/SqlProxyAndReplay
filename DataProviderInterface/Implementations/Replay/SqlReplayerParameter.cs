using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class SqlReplayerParameter : IDbDataParameter
	{
		public DbType DbType { get; set; }

		public ParameterDirection Direction { get; set; }

		public bool IsNullable { get; set; } // TODO: Is this correct? What should set this value (it's read-only in the interface)?

		public string ParameterName { get; set; }

		public byte Precision { get; set; }

		public byte Scale { get; set; }

		public int Size { get; set; }

		public string SourceColumn { get; set; }

		public DataRowVersion SourceVersion { get; set; }

		public object Value { get; set; }
	}
}
