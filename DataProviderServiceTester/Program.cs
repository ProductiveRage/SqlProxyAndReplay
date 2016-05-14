using System;
using ProductiveRage.SqlProxyAndReplay.DataProviderService;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderServiceProductiveRage.SqlProxyAndReplay.Tester
{
	class Program
	{
		static void Main(string[] args)
		{
			var endPoint = new Uri("net.tcp://localhost:5000/SqlProxy");
			using (var host = new Host(endPoint))
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
