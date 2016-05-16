using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations
{
	public sealed partial class SqlProxy : ISqlProxy
	{
		public int Add(CommandId commandId, ParameterId parameterId)
		{
			return _commandStore.Get(commandId).Parameters.Add(_parameterStore.Get(parameterId));
		}
	}
}