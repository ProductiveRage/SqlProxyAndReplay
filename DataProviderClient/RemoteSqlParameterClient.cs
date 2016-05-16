using System;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	internal sealed class RemoteSqlParameterClient : IDbDataParameter
	{
		private readonly IRemoteSqlParameter _parameter;
		public RemoteSqlParameterClient(IRemoteSqlParameter parameter, ParameterId parameterId, CommandId commandId)
		{
			if (parameter == null)
				throw new ArgumentNullException(nameof(parameter));

			_parameter = parameter;
			ParameterId = parameterId;
			CommandId = commandId;
		}

		public ParameterId ParameterId { get; }
		public CommandId CommandId { get; }

		public DbType DbType
		{
			get { return _parameter.GetDbType(ParameterId); }
			set { _parameter.SetDbType(ParameterId, value); }
		}

		public ParameterDirection Direction
		{
			get { return _parameter.GetDirection(ParameterId); }
			set { _parameter.SetDirection(ParameterId, value); }
		}

		public bool IsNullable
		{
			get { return _parameter.GetIsNullable(ParameterId); }
		}

		public string ParameterName
		{
			get { return _parameter.GetParameterName(ParameterId); }
			set { _parameter.SetParameterName(ParameterId, value); }
		}
		public byte Precision
		{
			get { return _parameter.GetPrecision(ParameterId); }
			set { _parameter.SetPrecision(ParameterId, value); }
		}

		public byte Scale
		{
			get { return _parameter.GetScale(ParameterId); }
			set { _parameter.SetScale(ParameterId, value); }
		}

		public int Size
		{
			get { return _parameter.GetSize(ParameterId); }
			set { _parameter.SetSize(ParameterId, value); }
		}


		public string SourceColumn
		{
			get { return _parameter.GetSourceColumn(ParameterId); }
			set { _parameter.SetSourceColumn(ParameterId, value); }
		}

		public DataRowVersion SourceVersion
		{
			get { return _parameter.GetSourceVersion(ParameterId); }
			set { _parameter.SetSourceVersion(ParameterId, value); }
		}

		public object Value
		{
			get { return _parameter.GetValue(ParameterId); }
			set { _parameter.SetValue(ParameterId, value); }
		}
	}
}
