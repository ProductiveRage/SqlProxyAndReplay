using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderServiceProductiveRage.SqlProxyAndReplay.Tester
{
	public interface ISqlRunner
	{
		DataSet Execute(QueryCriteria query);
		int ExecuteNonQuery(QueryCriteria query);
		object ExecuteScalar(QueryCriteria query);
	}
}