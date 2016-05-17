using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.PassThrough;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;
using ProductiveRage.SqlProxyAndReplay.DataProviderService;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderServiceProductiveRage.SqlProxyAndReplay.Tester
{
	class Program
	{
		private readonly static ConcurrentDictionary<QueryCriteria, byte[]> _serialisedQueryAndResultsCache = new ConcurrentDictionary<QueryCriteria, byte[]>();

		static void Main(string[] args)
		{
			var proxyEndPoint = new Uri("net.tcp://localhost:5000/SqlProxy");
			var replayEndPoint = new Uri("net.tcp://localhost:5001/SqlProxy");
			using (var proxyHost = new Host(new SqlProxy(QueryRecorder, ScalarQueryRecorder), proxyEndPoint))
			{
				using (var replayHost = new Host(new SqlReplayer(DataRetriever, ScalarDataRetriever, NonQueryRowCountDataRetriever), replayEndPoint))
				{
					Console.WriteLine("Started..");
					Console.WriteLine("Press [Enter] to end..");
					Console.ReadLine();
				}
			}
			Console.WriteLine("Done");
			Console.ReadLine();
		}

		private static void QueryRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			Console.WriteLine("LIVE[ExecuteReader]: " + query.CommandText);
			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = GetCommand(connection, query))
				{
					using (var dataSet = new DataSet())
					{
						connection.Open();
						using (var dataAdapter = new SqlDataAdapter(command))
						{
							dataAdapter.Fill(dataSet);
							dataSet.RemotingFormat = SerializationFormat.Binary;
							using (var stream = new MemoryStream())
							{
								(new BinaryFormatter()).Serialize(stream, dataSet);
								_serialisedQueryAndResultsCache.TryAdd(query, stream.ToArray());
								var hashes = _serialisedQueryAndResultsCache.Keys.Select(x => x.GetHashCode()).ToArray(); // TODO: Remove
							}
						}
					}
				}
			}
		}

		private static void ScalarQueryRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			Console.WriteLine("LIVE[ExecuteScalar]: " + query.CommandText); // TODO
		}

		private static IDataReader DataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			Console.WriteLine("REPLAY[ExecuteReader]: " + query.CommandText);
			byte[] serialisedData;
			if (!_serialisedQueryAndResultsCache.TryGetValue(query, out serialisedData))
				return null;

			using (var stream = new MemoryStream(serialisedData))
			{
				var deserialisedData = (DataSet)(new BinaryFormatter()).Deserialize(stream);
				return deserialisedData.CreateDataReader();
			}

		}

		private static Tuple<object> ScalarDataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			// Note: The same applies as to DatRetriever - this should use a memory or disk cache rather than hitting the database
			Console.WriteLine("REPLAY: " + query.CommandText); // TODO
			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = GetCommand(connection, query))
				{
					using (var dataSet = new DataSet())
					{
						connection.Open();
						return Tuple.Create(command.ExecuteScalar());
					}
				}
			}
		}

		private static int? NonQueryRowCountDataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			// Note: The same applies as to DatRetriever - this should use a memory or disk cache rather than hitting the database
			Console.WriteLine("REPLAY: " + query.CommandText); // TODO
			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = GetCommand(connection, query))
				{
					using (var dataSet = new DataSet())
					{
						connection.Open();
						return command.ExecuteNonQuery();
					}
				}
			}
		}

		private static SqlCommand GetCommand(SqlConnection connection, QueryCriteria query)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			var command = connection.CreateCommand();
			command.CommandText = query.CommandText;
			command.CommandType = query.CommandType;
			foreach (var p in query.Parameters)
			{
				var parameter = command.CreateParameter();
				parameter.ParameterName = p.ParameterName;
				parameter.Value = p.Value;
				parameter.DbType = p.DbType;
				parameter.IsNullable = p.IsNullable;
				parameter.Direction = p.Direction;
				parameter.Scale = p.Scale;
				parameter.Size = p.Size;
				command.Parameters.Add(parameter);
			}
			return command;
		}
	}
}