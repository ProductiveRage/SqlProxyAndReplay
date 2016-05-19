using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderServiceProductiveRage.SqlProxyAndReplay.Tester
{
	public sealed class DictionaryCache
	{
		private readonly Action<string> _infoLogger;
		private readonly ConcurrentDictionary<QueryCriteria, byte[]> _serialisedQueryAndResultsCache;
		private readonly ConcurrentDictionary<QueryCriteria, object> _serialisedQueryAndScalarResultsCache;
		private readonly ConcurrentDictionary<QueryCriteria, int> _serialisedQueryAndNonQueryRowCountCache;
		public DictionaryCache(Action<string> infoLogger)
		{
			if (infoLogger == null)
				throw new ArgumentNullException(nameof(infoLogger));

			_infoLogger = infoLogger;
			_serialisedQueryAndResultsCache = new ConcurrentDictionary<QueryCriteria, byte[]>();
			_serialisedQueryAndScalarResultsCache = new ConcurrentDictionary<QueryCriteria, object>();
			_serialisedQueryAndNonQueryRowCountCache = new ConcurrentDictionary<QueryCriteria, int>();
		}

		public void QueryRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("LIVE[ExecuteReader]: " + query.CommandText);
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
							}
						}
					}
				}
			}
		}

		public void ScalarQueryRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("LIVE[ExecuteScalar]: " + query.CommandText);
			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = GetCommand(connection, query))
				{
					connection.Open();
					_serialisedQueryAndScalarResultsCache.TryAdd(query, command.ExecuteScalar());
				}
			}
		}

		public void NonQueryRowCountRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("LIVE[ExecuteNonQuery]: " + query.CommandText);
			using (var connection = new SqlConnection(query.ConnectionString))
			{
				using (var command = GetCommand(connection, query))
				{
					connection.Open();
					_serialisedQueryAndNonQueryRowCountCache.TryAdd(query, command.ExecuteNonQuery());
				}
			}
		}

		public IDataReader DataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("REPLAY[ExecuteReader]: " + query.CommandText);
			byte[] serialisedData;
			if (!_serialisedQueryAndResultsCache.TryGetValue(query, out serialisedData))
				return null;

			using (var stream = new MemoryStream(serialisedData))
			{
				var deserialisedData = (DataSet)(new BinaryFormatter()).Deserialize(stream);
				return deserialisedData.CreateDataReader();
			}

		}

		public Tuple<object> ScalarDataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("REPLAY[ExecuteScalar]: " + query.CommandText);
			object result;
			if (!_serialisedQueryAndScalarResultsCache.TryGetValue(query, out result))
				return null;

			return Tuple.Create(result);
		}

		public int? NonQueryRowCountRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("REPLAY[ExecuteNonQuery]: " + query.CommandText);
			int rowCount;
			if (!_serialisedQueryAndNonQueryRowCountCache.TryGetValue(query, out rowCount))
				return null;

			return rowCount;
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