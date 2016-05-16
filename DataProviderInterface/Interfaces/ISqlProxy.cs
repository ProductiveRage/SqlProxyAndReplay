using System.ServiceModel;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces
{
	[ServiceContract]
	public interface ISqlProxy : IRemoteSqlConnection, IRemoteSqlCommand, IRemoteSqlParameterSet, IRemoteSqlParameter, IRemoteSqlTransaction, IRemoteSqlDataReader { }
}