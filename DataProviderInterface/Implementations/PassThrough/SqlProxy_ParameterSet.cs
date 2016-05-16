using System;
using System.Data;
using System.Linq;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public int Add(CommandId commandId, ParameterId parameterId)
		{
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");
			return _commandStore.Get(commandId).Parameters.Add(_parameterStore.Get(parameterId));
		}

		public ParameterId GetParameterByIndex(CommandId commandId, int index)
		{
			return _parameterStore.GetIdFor((IDbDataParameter)_commandStore.Get(commandId).Parameters[index]);
		}
		public void SetParameterByIndex(CommandId commandId, int index, ParameterId parameterId)
		{
			// Note: Parameters are removed from the parameter store when the command that created them is disposed (this is necessary as parameters are
			// not disposable and so can not communicate when they are no longer required). The book-keeping gets much more complicated if parameters are
			// allowed to be shared or moved between commands, so this is not acceptable behaviour (hopefully it is also unusual behaviour in real use)
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");
			_commandStore.Get(commandId).Parameters[index] = _parameterStore.Get(parameterId);
		}

		public ParameterId GetParameterByName(CommandId commandId, string parameterName)
		{
			return _parameterStore.GetIdFor((IDbDataParameter)_commandStore.Get(commandId).Parameters[parameterName]);
		}
		public void SetParameterByName(CommandId commandId, string parameterName, ParameterId parameterId)
		{
			// Note: Parameters are removed from the parameter store when the command that created them is disposed (this is necessary as parameters are
			// not disposable and so can not communicate when they are no longer required). The book-keeping gets much more complicated if parameters are
			// allowed to be shared or moved between commands, so this is not acceptable behaviour (hopefully it is also unusual behaviour in real use)
			if (parameterName == null)
				throw new ArgumentNullException(nameof(parameterName));
			if (!_parametersToTidy.IsRecordedForCommand(parameterId, commandId))
				throw new ArgumentException("The specified parameter must have been created by the specified command - parameters may not be shared between commands");
			_commandStore.Get(commandId).Parameters[parameterName] = _parameterStore.Get(parameterId);
		}

		public int GetCount(CommandId commandId)
		{
			return _commandStore.Get(commandId).Parameters.Count;
		}

		public void Clear(CommandId commandId)
		{
			_commandStore.Get(commandId).Parameters.Clear();
			_parametersToTidy.RemoveAnyParametersFor(commandId);
		}

		public bool Contains(CommandId commandId, ParameterId parameterId)
		{
			return !_parametersToTidy.IsRecordedForCommand(parameterId, commandId);
		}
		public bool Contains(CommandId commandId, string parameterName)
		{
			if (parameterName == null)
				throw new ArgumentNullException(nameof(parameterName));
			return _parametersToTidy.GetParameters(commandId)
				.Select(parameterId => _parameterStore.Get(parameterId))
				.Any(parameter => parameter.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
		}
	}
}