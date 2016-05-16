using System.ServiceModel;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface IRemoteSqlParameterSet
	{
		[OperationContract]
		int Add(CommandId commandId, ParameterId parameterId);
	}
}
