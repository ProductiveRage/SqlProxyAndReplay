using System;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Description;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService
{
	public sealed class Host : IDisposable
	{
		private ServiceHost _host;
		private bool _disposed;
		public Host(Uri endPoint)
		{
			if (endPoint == null)
				throw new ArgumentNullException(nameof(endPoint));

			try
			{
				//_host = new ServiceHost(new SqlProxy());
				_host = new ServiceHost(new SqlReplayer(DatRetriever, ScalarDataRetriever));
				_host.AddServiceEndpoint(typeof(ISqlProxy), new NetTcpBinding(), endPoint);
				_host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
				_host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
				_host.Open();
			}
			catch
			{
				Dispose();
				throw;
			}
			_disposed = false;
		}

		~Host()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				// Note: This waits until clients have disconnect
				if (_host != null)
					((IDisposable)_host).Dispose();
			}

			_disposed = true;
		}

		private static IDataReader DatRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			// The idea behind the "data retriever" delegate that the SqlReplayer takes as a constructor argument is that there will be a cache of results that data
			// is returned from but none of that has been fleshed out yet, so this data retriever just takes the data from each query and makes a real database call,
			// pulling back all of the data into a disconnected DataReader (so that the connection, command, etc.. initialised here can be immediately disposed of so
			// that only the returned data reader needs to be disposed by the caller).
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
							return dataSet.CreateDataReader();
						}
					}
				}
			}
		}

		private static Tuple<object> ScalarDataRetriever(QueryCriteria query)
		{
			if (query == null)
				throw new ArgumentNullException(nameof(query));

			// Note: The same applies as to DatRetriever - this should use a memory or disk cache rather than hitting the database
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