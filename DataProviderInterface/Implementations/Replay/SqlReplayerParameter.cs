using System.Data;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class SqlReplayerParameter : IDbDataParameter
	{
		private object _value;
		public SqlReplayerParameter()
		{
			// Apply some sensible defaults - important for properties whose default state is invalid (eg. there is no zero value for the ParameterDirection
			// enum) but more important that the property defaults here match those on SqlParameter for when the replayer aspect of the services comes to
			// look in its cache (it will check that the connection, command and parameters are consistent between the current request and any cache entry,
			// the direction of the parameter - Input, Output, InputOutput, ReturnValue - is important)
			Direction = ParameterDirection.Input;
			_value = null;
		}

		public DbType DbType { get; set; }

		public ParameterDirection Direction { get; set; }

		public bool IsNullable { get; set; } // TODO: Is this correct? What should set this value (it's read-only in the interface)?

		public string ParameterName { get; set; }

		public byte Precision { get; set; }

		public byte Scale { get; set; }

		public int Size { get; set; }

		public string SourceColumn { get; set; }

		public DataRowVersion SourceVersion { get; set; }

		public object Value
		{
			get { return _value; }
			set
			{
				var stringValue = value as string;
				if (stringValue != null)
				{
					DbType = DbType.String; // TODO: Correct?
					if (Size < stringValue.Length)
						 Size = stringValue.Length; // TODO: Correct?
				}
				_value = value;
			}
		}
	}
}
