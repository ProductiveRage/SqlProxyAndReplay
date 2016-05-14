using System.ServiceModel;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	[ServiceContract]
	public interface ISqlProxy : IRemoteSqlConnection, IRemoteSqlCommand, IRemoteSqlTransaction, IRemoteSqlDataReader { }
}