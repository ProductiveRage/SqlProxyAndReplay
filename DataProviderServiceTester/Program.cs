using System;
using System.Data.SqlClient;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;
using ProductiveRage.SqlProxyAndReplay.DataProviderService;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderServiceProductiveRage.SqlProxyAndReplay.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			var cache = new DictionaryCache(infoLogger: Console.WriteLine);
			var proxyEndPoint = new Uri("net.tcp://localhost:5000/SqlProxy");
			var replayEndPoint = new Uri("net.tcp://localhost:5001/SqlProxy");
			using (var proxyHost = new Host(new SqlProxy(() => new SqlConnection(), cache.QueryRecorder, cache.ScalarQueryRecorder), proxyEndPoint))
			{
				using (var replayHost = new Host(new SqlReplayer(cache.DataRetriever, cache.ScalarDataRetriever, cache.NonQueryRowCountDataRetriever), replayEndPoint))
				{
					Console.WriteLine("Started..");
					Console.WriteLine("Press [Enter] to end..");
					Console.ReadLine();
				}
			}
			Console.WriteLine("Done");
			Console.ReadLine();
		}
	}
}