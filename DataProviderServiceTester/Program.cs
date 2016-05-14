using System;
using ProductiveRage.SqlProxyAndReplay.DataProviderService;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderServiceProductiveRage.SqlProxyAndReplay.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			var connectionServerEndPoint = new Uri("net.tcp://localhost:5000/RemoteSqlConnection");
			var commandServerEndPoint = new Uri("net.tcp://localhost:5000/RemoteSqlCommand");
			var readerServerEndPoint = new Uri("net.tcp://localhost:5000/RemoteSqlDataReader");
			using (var host = new Host(connectionServerEndPoint, commandServerEndPoint, readerServerEndPoint))
			{
				Console.WriteLine("Started..");
				Console.WriteLine("Press [Enter] to end..");
				Console.ReadLine();
			}
			Console.WriteLine("Done");
			Console.ReadLine();
		}
	}
}
