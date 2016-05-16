using System;
using System.Collections;
using System.Data;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Interfaces;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderClient
{
	public sealed class RemoteSqlParameterSetClient : IDataParameterCollection
	{
		private readonly IRemoteSqlCommand _command;
		private readonly IRemoteSqlParameterSet _parameters;
		private readonly IRemoteSqlParameter _parameter;
		private readonly CommandId _commandId;
		public RemoteSqlParameterSetClient(IRemoteSqlCommand command, IRemoteSqlParameterSet parameters, IRemoteSqlParameter parameter, CommandId commandId)
		{
			if (command == null)
				throw new ArgumentNullException(nameof(command));
			if (parameters == null)
				throw new ArgumentNullException(nameof(parameters));
			if (parameter == null)
				throw new ArgumentNullException(nameof(parameter));

			_command = command;
			_parameters = parameters;
			_parameter = parameter;
			_commandId = commandId;

			SyncRoot = new object();
		}

		// TODO: Implement all these..
		public object this[int index]
		{
			get
			{
				throw new NotImplementedException();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public object this[string parameterName]
		{
			get
			{
				throw new NotImplementedException();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public int Count
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsFixedSize
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsReadOnly
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsSynchronized
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public object SyncRoot { get; }

		public int Add(object value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			var parameter = value as RemoteSqlParameterClient;
			if (parameter == null)
				throw new ArgumentException($"must be an RemoteSqlParameterClient implementation, not a {value.GetType()}", nameof(value));
			return _parameters.Add(_commandId, parameter.ParameterId);
		}

		// This method isn't part of IDataParameterCollection but it's on the SqlParameterCollection and it's very convenient, so I'm including it here
		public IDbDataParameter AddWithValue(string parameterName, object value)
		{
			if (string.IsNullOrWhiteSpace(parameterName))
				throw new ArgumentException($"Null/blank {nameof(parameterName)} specified");

			var parameter = new RemoteSqlParameterClient(_parameter, _command.CreateParameter(_commandId));
			parameter.ParameterName = parameterName;
			parameter.Value = value;
			Add(parameter);
			return parameter;
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(object value)
		{
			throw new NotImplementedException();
		}

		public bool Contains(string parameterName)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public int IndexOf(object value)
		{
			throw new NotImplementedException();
		}

		public int IndexOf(string parameterName)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		public void Remove(object value)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(string parameterName)
		{
			throw new NotImplementedException();
		}
	}
}
