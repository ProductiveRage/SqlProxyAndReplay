using System;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.Example;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.Tests
{
	public sealed class ReadOnlyDictionaryCache
	{
		private readonly DictionaryCache _cache;
		public ReadOnlyDictionaryCache(DictionaryCache cache)
		{
			if (cache == null)
				throw new ArgumentNullException(nameof(cache));

			_cache = cache;
		}

		public IDataReader DataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			return _cache.DataRetriever(query);
		}

		public Tuple<object> ScalarDataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			return _cache.ScalarDataRetriever(query);
		}

		public int? NonQueryRowCountRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			return _cache.NonQueryRowCountRetriever(query);
		}
	}
}
