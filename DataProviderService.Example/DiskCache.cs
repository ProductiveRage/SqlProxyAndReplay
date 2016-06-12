using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.Example
{
	public sealed class DiskCache : SerialisingCache
	{
		private const string PREFIX_DATASET = "DataSet";
		private const string PREFIX_SCALAR = "Scalar";
		private const string PREFIX_ROWCOUNT = "RowCount";

		private readonly DirectoryInfo _cacheFolder;
		private readonly int _folderDepth;
		private readonly Action<Exception> _errorLogger;
		public DiskCache(ISqlRunner sqlRunner, DirectoryInfo cacheFolder, int folderDepth, Action<string> infoLogger, Action<Exception> errorLogger)
			: base(sqlRunner, infoLogger)
		{
			if (cacheFolder == null)
				throw new ArgumentNullException(nameof(cacheFolder));
			if (folderDepth < 1)
				throw new ArgumentException("Must be one or greater", nameof(folderDepth));
			if (errorLogger == null)
				throw new ArgumentNullException(nameof(errorLogger));

			_cacheFolder = cacheFolder;
			_folderDepth = folderDepth;
			_errorLogger = errorLogger;
		}
		public DiskCache(ISqlRunner sqlRunner, DirectoryInfo cacheFolder, Action<string> infoLogger, Action<Exception> errorLogger)
			: this(sqlRunner, cacheFolder, folderDepth: 1, infoLogger: infoLogger, errorLogger: errorLogger) { }
		public DiskCache(ISqlRunner sqlRunner, DirectoryInfo cacheFolder, Action<string> infoLogger)
			: this(sqlRunner, cacheFolder, folderDepth: 1, infoLogger: infoLogger, errorLogger: e => infoLogger(e.Message)) { }

		/// <summary>
		/// This should never have to deal with a null query or null data reference
		/// </summary>
		protected override void DataSetRecorder(QueryCriteria query, byte[] data)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));
			if (data == null)
				throw new ArgumentNullException(nameof(data));

			WriteResultsIgnoringError(PREFIX_DATASET, query, SetInCachedResults(PREFIX_DATASET, query, data));
		}

		/// <summary>
		/// This should never have to deal with a null query reference (though it is feasible that the value reference may be null)
		/// </summary>
		protected override void ScalarRecorder(QueryCriteria query, object value)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			WriteResultsIgnoringError(PREFIX_SCALAR, query, SetInCachedResults(PREFIX_SCALAR, query, value));
		}

		/// <summary>
		/// This should never have to deal with a null query reference
		/// </summary>
		protected override void RowCountRecorder(QueryCriteria query, int rowCount)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			WriteResultsIgnoringError(PREFIX_ROWCOUNT, query, SetInCachedResults(PREFIX_ROWCOUNT, query, rowCount));
		}

		protected override byte[] DataSetRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			var result = GetCachedResultsIgnoringError<byte[]>(PREFIX_DATASET, query.GetHashCode())
				.FirstOrDefault(cachedResult => cachedResult.Query.Equals(query));
			return (result == null) ? null : result.Value;
		}

		protected override Tuple<object> ScalarRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			var result = GetCachedResultsIgnoringError<object>(PREFIX_SCALAR, query.GetHashCode())
				.FirstOrDefault(cachedResult => cachedResult.Query.Equals(query));
			return (result == null) ? null : Tuple.Create(result.Value);
		}

		protected override int? RowCountRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			var result = GetCachedResultsIgnoringError<int>(PREFIX_ROWCOUNT, query.GetHashCode())
				.FirstOrDefault(cachedResult => cachedResult.Query.Equals(query));
			return (result == null) ? (int?)null : result.Value;
		}

		private void WriteResultsIgnoringError<T>(string prefix, QueryCriteria query, CachedResult<T>[] results)
		{
			if (prefix == null)
				throw new ArgumentNullException(nameof(prefix));
			if (query == null)
				throw new ArgumentNullException(nameof(query));
			if (results == null)
				throw new ArgumentNullException(nameof(results));

			try
			{
				var cacheFile = GetCacheFile(prefix, query.GetHashCode());
				cacheFile.Directory.Create();
				using (var stream = cacheFile.OpenWrite())
				{
					(new BinaryFormatter()).Serialize(stream, results);
				}
			}
			catch (Exception e)
			{
				_errorLogger(e);
			}
		}

		private CachedResult<T>[] SetInCachedResults<T>(string prefix, QueryCriteria query, T data)
		{
			if (prefix == null)
				throw new ArgumentNullException(nameof(prefix));
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			var hashCode = query.GetHashCode();
			return GetCachedResultsIgnoringError<T>(prefix, hashCode)
				.Where(result => !result.Query.Equals(query))
				.Concat(new[] { new CachedResult<T>(query, data) })
				.ToArray();
		}

		/// <summary>
		/// This will record any exception but not allow it to bubble up. It will never return null (nor will the array contain any null references), if
		/// there was no cache file or if the cache file could not be read, then an empty array will be returned. 
		/// </summary>
		private CachedResult<T>[] GetCachedResultsIgnoringError<T>(string prefix, int hashCode)
		{
			if (prefix == null)
				throw new ArgumentNullException(nameof(prefix));

			var cacheFile = GetCacheFile(prefix, hashCode);
			if (!cacheFile.Exists)
				return new CachedResult<T>[0];
			try
			{
				CachedResult<T>[] results;
				using (var stream = GetCacheFile(prefix, hashCode).OpenRead())
				{
					results = (CachedResult<T>[])(new BinaryFormatter()).Deserialize(stream);
				}
				return results.Where(result => result != null).ToArray(); // There shouldn't be any nulls.. but make triple-sure since callers depend on it
			}
			catch (Exception e)
			{
				_errorLogger(e);
				return new CachedResult<T>[0];
			}
		}

		private FileInfo GetCacheFile(string prefix, int hashCode)
		{
			if (prefix == null)
				throw new ArgumentNullException(nameof(prefix));

			var hashCodeString = Math.Abs(hashCode).ToString();
			var subFolderNames = Enumerable.Range(0, _folderDepth - 1).Select(i => (i >= hashCodeString.Length) ? "0" : hashCodeString.Substring(i, 1));
			var subFolderPath = Path.Combine(subFolderNames.ToArray());
			return new FileInfo(Path.Combine(_cacheFolder.FullName, subFolderPath, prefix + hashCodeString + ".dat"));
		}

		[Serializable]
		private sealed class CachedResult<T>
		{
			public CachedResult(QueryCriteria query, T value)
			{
				if (query == null)
					throw new ArgumentNullException(nameof(query));

				Query = query;
				Value = value;
			}

			/// <summary>
			/// This will never be null
			/// </summary>
			public QueryCriteria Query { get; }
			public T Value { get; }
		}
	}
}