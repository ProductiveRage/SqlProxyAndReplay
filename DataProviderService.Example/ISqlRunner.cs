using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.Example
{
	public interface ISqlRunner
	{
		DataSet Execute(QueryCriteria query);
		int ExecuteNonQuery(QueryCriteria query);
		object ExecuteScalar(QueryCriteria query);
	}
}