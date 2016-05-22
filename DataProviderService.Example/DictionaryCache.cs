using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.Example
{
	public sealed class DictionaryCache
	{
		private readonly ISqlRunner _sqlRunner;
		private readonly Action<string> _infoLogger;
		private readonly ConcurrentDictionary<QueryCriteria, byte[]> _serialisedQueryAndResultsCache;
		private readonly ConcurrentDictionary<QueryCriteria, object> _serialisedQueryAndScalarResultsCache;
		private readonly ConcurrentDictionary<QueryCriteria, int> _serialisedQueryAndNonQueryRowCountCache;
		public DictionaryCache(ISqlRunner sqlRunner, Action<string> infoLogger)
		{
			if (sqlRunner == null)
				throw new ArgumentNullException(nameof(sqlRunner));
			if (infoLogger == null)
				throw new ArgumentNullException(nameof(infoLogger));

			_sqlRunner = sqlRunner;
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
			using (var dataSet = _sqlRunner.Execute(query))
			{
				dataSet.RemotingFormat = SerializationFormat.Binary;
				using (var stream = new MemoryStream())
				{
					(new BinaryFormatter()).Serialize(stream, dataSet);
					_serialisedQueryAndResultsCache.TryAdd(query, stream.ToArray());
				}
			}
		}

		public void ScalarQueryRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("LIVE[ExecuteScalar]: " + query.CommandText);
			_serialisedQueryAndScalarResultsCache.TryAdd(query, _sqlRunner.ExecuteScalar(query));
		}

		public void NonQueryRowCountRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("LIVE[ExecuteNonQuery]: " + query.CommandText);
			_serialisedQueryAndNonQueryRowCountCache.TryAdd(query, _sqlRunner.ExecuteNonQuery(query));
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
	}
}