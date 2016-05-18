using System.Data;
using System.Data.SqlClient;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class SqlReplayerParameter : IDbDataParameter
	{
		private readonly SqlParameter _parameter;
		public SqlReplayerParameter()
		{
			// The SqlParameter has all kinds of not-immediately-obvious behaviour that I don't want to replicate here, so I'm just going to
			// use one behind the scenes. For example, if nothing is set other than ParameterName and Value and the Value is a string then
			// an appropriate Size will be set on the parameter.
			_parameter = new SqlParameter();
		}

		public DbType DbType
		{
			get { return _parameter.DbType; }
			set { _parameter.DbType = value; }
		}
		public ParameterDirection Direction
		{
			get { return _parameter.Direction; }
			set { _parameter.Direction = value; }
		}
		public bool IsNullable
		{
			get { return _parameter.IsNullable; }
		}
		public string ParameterName
		{
			get { return _parameter.ParameterName; }
			set { _parameter.ParameterName = value; }
		}
		public byte Precision
		{
			get { return _parameter.Precision; }
			set { _parameter.Precision = value; }
		}
		public byte Scale
		{
			get { return _parameter.Scale; }
			set { _parameter.Scale = value; }
		}
		public int Size
		{
			get { return _parameter.Size; }
			set { _parameter.Size = value; }
		}
		public string SourceColumn
		{
			get { return _parameter.SourceColumn; }
			set { _parameter.SourceColumn = value; }
		}
		public DataRowVersion SourceVersion
		{
			get { return _parameter.SourceVersion; }
			set { _parameter.SourceVersion = value; }
		}
		public object Value
		{
			get { return _parameter.Value; }
			set { _parameter.Value = value; }
		}
	}
}
