using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.Example
{
	public abstract class SerialisingCache
	{
		private readonly ISqlRunner _sqlRunner;
		private readonly Action<string> _infoLogger;
		public SerialisingCache(ISqlRunner sqlRunner, Action<string> infoLogger)
		{
			if (sqlRunner == null)
				throw new ArgumentNullException(nameof(sqlRunner));
			if (infoLogger == null)
				throw new ArgumentNullException(nameof(infoLogger));

			_sqlRunner = sqlRunner;
			_infoLogger = infoLogger;
		}

		/// <summary>
		/// This should never have to deal with a null query or null data reference
		/// </summary>
		protected abstract void DataSetRecorder(QueryCriteria query, byte[] data);
		/// <summary>
		/// This should never have to deal with a null query reference (though it is feasible that the value reference may be null)
		/// </summary>
		protected abstract void ScalarRecorder(QueryCriteria query, object value);
		/// <summary>
		/// This should never have to deal with a null query reference
		/// </summary>
		protected abstract void RowCountRecorder(QueryCriteria query, int rowCount);

		/// <summary>
		/// This should return null if there is no cache data available for the specified query - a null query reference should never be passed in to it
		/// </summary>
		protected abstract byte[] DataSetRetriever(QueryCriteria query);
		/// <summary>
		/// This should return null if there is no cache data available for the specified query - a null query reference should never be passed in to it
		/// </summary>
		protected abstract Tuple<object> ScalarRetriever(QueryCriteria query);
		/// <summary>
		/// This should return null if there is no cache data available for the specified query - a null query reference should never be passed in to it
		/// </summary>
		protected abstract int? RowCountRetriever(QueryCriteria query);

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
					DataSetRecorder(query, stream.ToArray());
				}
			}
		}

		public void ScalarQueryRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("LIVE[ExecuteScalar]: " + query.CommandText);
			ScalarRecorder(query, _sqlRunner.ExecuteScalar(query));
		}

		public void NonQueryRowCountRecorder(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("LIVE[ExecuteNonQuery]: " + query.CommandText);
			RowCountRecorder(query, _sqlRunner.ExecuteNonQuery(query));
		}

		public IDataReader DataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("REPLAY[ExecuteReader]: " + query.CommandText);
			var cachedSerialisedData = DataSetRetriever(query);
			if (cachedSerialisedData == null)
				return null;

			using (var stream = new MemoryStream(cachedSerialisedData))
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
			return ScalarRetriever(query);
		}

		public int? NonQueryRowCountRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			_infoLogger("REPLAY[ExecuteNonQuery]: " + query.CommandText);
			return RowCountRetriever(query);
		}
	}
}