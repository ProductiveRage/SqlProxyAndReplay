using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.PassThrough
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public ParameterDirection GetDirection(ParameterId parameterId) { return _parameterStore.Get(parameterId).Direction; }
		public void SetDirection(ParameterId parameterId, ParameterDirection value) { _parameterStore.Get(parameterId).Direction = value; }

		public DbType GetDbType(ParameterId parameterId) { return _parameterStore.Get(parameterId).DbType; }
		public void SetDbType(ParameterId parameterId, DbType value) { _parameterStore.Get(parameterId).DbType = value; }

		public bool GetIsNullable(ParameterId parameterId) { return _parameterStore.Get(parameterId).IsNullable; }

		public byte GetPrecision(ParameterId parameterId) { return _parameterStore.Get(parameterId).Precision; }
		public void SetPrecision(ParameterId parameterId, byte value) { _parameterStore.Get(parameterId).Precision = value; }

		public string GetParameterName(ParameterId parameterId) { return _parameterStore.Get(parameterId).ParameterName; }
		public void SetParameterName(ParameterId parameterId, string value) { _parameterStore.Get(parameterId).ParameterName = value; }

		public byte GetScale(ParameterId parameterId) { return _parameterStore.Get(parameterId).Scale; }
		public void SetScale(ParameterId parameterId, byte value) { _parameterStore.Get(parameterId).Scale = value; }

		public int GetSize(ParameterId parameterId) { return _parameterStore.Get(parameterId).Size; }
		public void SetSize(ParameterId parameterId, int value) { _parameterStore.Get(parameterId).Size = value; }

		public string GetSourceColumn(ParameterId parameterId) { return _parameterStore.Get(parameterId).SourceColumn; }
		public void SetSourceColumn(ParameterId parameterId, string value) { _parameterStore.Get(parameterId).SourceColumn = value; }

		public DataRowVersion GetSourceVersion(ParameterId parameterId) { return _parameterStore.Get(parameterId).SourceVersion; }
		public void SetSourceVersion(ParameterId parameterId, DataRowVersion value) { _parameterStore.Get(parameterId).SourceVersion = value; }

		public object GetValue(ParameterId parameterId) { return _parameterStore.Get(parameterId).Value; }
		public void SetValue(ParameterId parameterId, object value) { _parameterStore.Get(parameterId).Value = value; }
	}
}