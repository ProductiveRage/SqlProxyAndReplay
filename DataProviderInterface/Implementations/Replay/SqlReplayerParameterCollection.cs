using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class SqlReplayerParameterCollection : IDataParameterCollection, IEnumerable<SqlReplayerParameter>
	{
		private readonly List<SqlReplayerParameter> _parameters;
		public SqlReplayerParameterCollection()
		{
			_parameters = new List<SqlReplayerParameter>();
		}

		public object this[int index]
		{
			get { return _parameters[index]; }
			set { throw new NotImplementedException(); } // TODO
		}

		public object this[string parameterName]
		{
			get { return _parameters.FirstOrDefault(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase)); } // TODO: Is this correct? What happens for invalid parameterName?
			set { throw new NotImplementedException(); } // TODO: What should this do? What happens for invalid parameterName?
		}

		public int Count { get { return _parameters.Count; } }

		public bool IsFixedSize { get { return ((IList)_parameters).IsFixedSize; } }
		public bool IsReadOnly { get { return ((IList)_parameters).IsReadOnly; } }
		public bool IsSynchronized { get { return ((ICollection)_parameters).IsSynchronized; } }
		public object SyncRoot { get { return ((ICollection)_parameters).SyncRoot; } }

		public int Add(object value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			var parameter = value as SqlReplayerParameter;
			if (parameter == null)
				throw new ArgumentException($"value must be a {typeof(SqlReplayerParameter)}");
			_parameters.Add(parameter);
			return _parameters.Count;
		}

		public void Clear()
		{
			throw new NotImplementedException(); // TODO
		}

		public bool Contains(object value)
		{
			throw new NotImplementedException(); // TODO
		}

		public bool Contains(string parameterName)
		{
			throw new NotImplementedException(); // TODO
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException(); // TODO
		}

		public IEnumerator<SqlReplayerParameter> GetEnumerator() { return _parameters.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public int IndexOf(object value)
		{
			throw new NotImplementedException(); // TODO
		}

		public int IndexOf(string parameterName)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Insert(int index, object value)
		{
			throw new NotImplementedException(); // TODO
		}

		public void Remove(object value)
		{
			throw new NotImplementedException(); // TODO
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException(); // TODO
		}

		public void RemoveAt(string parameterName)
		{
			throw new NotImplementedException(); // TODO
		}
	}
}
