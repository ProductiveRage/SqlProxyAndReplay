using System;
using System.Collections.Concurrent;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.Example
{
	public sealed class DictionaryCache : SerialisingCache
	{
		private readonly ConcurrentDictionary<QueryCriteria, byte[]> _serialisedQueryAndResultsCache;
		private readonly ConcurrentDictionary<QueryCriteria, object> _serialisedQueryAndScalarResultsCache;
		private readonly ConcurrentDictionary<QueryCriteria, int> _serialisedQueryAndNonQueryRowCountCache;
		public DictionaryCache(ISqlRunner sqlRunner, Action<string> infoLogger) : base(sqlRunner, infoLogger)
		{
			_serialisedQueryAndResultsCache = new ConcurrentDictionary<QueryCriteria, byte[]>();
			_serialisedQueryAndScalarResultsCache = new ConcurrentDictionary<QueryCriteria, object>();
			_serialisedQueryAndNonQueryRowCountCache = new ConcurrentDictionary<QueryCriteria, int>();
		}

		/// <summary>
		/// This should never have to deal with a null query or null data reference
		/// </summary>
		protected override void DataSetRecorder(QueryCriteria query, byte[] data)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			_serialisedQueryAndResultsCache.TryAdd(query, data);
		}

		/// <summary>
		/// This should never have to deal with a null query reference (though it is feasible that the value reference may be null)
		/// </summary>
		protected override void ScalarRecorder(QueryCriteria query, object value)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_serialisedQueryAndScalarResultsCache.TryAdd(query, value);
		}

		/// <summary>
		/// This should never have to deal with a null query reference
		/// </summary>
		protected override void RowCountRecorder(QueryCriteria query, int rowCount)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_serialisedQueryAndNonQueryRowCountCache.TryAdd(query, rowCount);
		}

		protected override byte[] DataSetRetriever(QueryCriteria query)
		{
			byte[] serialisedData;
			return _serialisedQueryAndResultsCache.TryGetValue(query, out serialisedData) ? serialisedData : null;
		}

		protected override Tuple<object> ScalarRetriever(QueryCriteria query)
		{
			object value;
			return _serialisedQueryAndScalarResultsCache.TryGetValue(query, out value) ? Tuple.Create(value) : null;
		}

		protected override int? RowCountRetriever(QueryCriteria query)
		{
			int rowCount;
			return _serialisedQueryAndNonQueryRowCountCache.TryGetValue(query, out rowCount) ? rowCount : (int?)null;
		}
	}
}