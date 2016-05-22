using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class SqlReplayerParameterCollection : IDataParameterCollection, IEnumerable<IDbDataParameter>
	{
		private readonly List<IDbDataParameter> _parameters;
		public SqlReplayerParameterCollection()
		{
			_parameters = new List<IDbDataParameter>();
		}

		public object this[int index]
		{
			get
			{
				lock (_parameters)
				{
					if ((index >= 0) && (index < _parameters.Count))
						return _parameters[index];
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				var parameter = value as IDbDataParameter;
				if (parameter == null)
					throw new InvalidCastException($"The Parameter Collection only accepts non-null IDbDataParameter type objects, not {value.GetType()} objects");
				lock (_parameters)
				{
					if ((index >= 0) && (index < _parameters.Count))
					{
						_parameters[index] = parameter;
						return;
					}
				}
				throw new IndexOutOfRangeException();
			}
		}

		public object this[string parameterName]
		{
			get
			{
				lock (_parameters)
				{
					var parameter = _parameters.FirstOrDefault(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
					if (parameter != null)
						return parameter;
				}
				throw new IndexOutOfRangeException();
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				if (string.IsNullOrWhiteSpace(parameterName))
					throw new ArgumentException($"Null/blank {nameof(parameterName)} specified");
				var parameter = value as IDbDataParameter;
				if (parameter == null)
					throw new InvalidCastException($"The Parameter Collection only accepts non-null IDbDataParameter type objects, not {value.GetType()} objects");
				lock (_parameters)
				{
					var indexedParameterToSet = _parameters
						.Select((p, i) => new { Index = i, Parameter = p })
						.FirstOrDefault(p => p.Parameter.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
					if (indexedParameterToSet != null)
					{
						_parameters[indexedParameterToSet.Index] = parameter;
						return;
					}
				}
				throw new IndexOutOfRangeException();
			}
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
			var parameter = value as IDbDataParameter;
			if (parameter == null)
				throw new InvalidCastException($"The Parameter Collection only accepts non-null IDbDataParameter type objects, not {value.GetType()} objects");
			lock (_parameters)
			{
				if (!_parameters.Contains(parameter))
				{
					_parameters.Add(parameter);
					return _parameters.Count;
				}
			}
			throw new ArgumentException("The parameter is already present in the Parameters collection");
		}

		public void Clear()
		{
			lock (_parameters)
			{
				_parameters.Clear();
			}
		}

		public bool Contains(object value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			var parameter = value as IDbDataParameter;
			if (parameter == null)
				throw new InvalidCastException($"The Parameter Collection only accepts non-null IDbDataParameter type objects, not {value.GetType()} objects");
			lock (_parameters)
			{
				return _parameters.Contains(parameter);
			}
		}

		public bool Contains(string parameterName)
		{
			if (parameterName == null)
				throw new ArgumentNullException(nameof(parameterName));
			lock (_parameters)
			{
				return _parameters.Any(p => p.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
			}
		}

		public int IndexOf(object value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			var parameter = value as IDbDataParameter;
			if (parameter == null)
				throw new InvalidCastException($"The Parameter Collection only accepts non-null IDbDataParameter type objects, not {value.GetType()} objects");
			lock (_parameters)
			{
				return _parameters.IndexOf(parameter);
			}
		}

		public int IndexOf(string parameterName)
		{
			if (parameterName == null)
				throw new ArgumentNullException(nameof(parameterName));
			lock (_parameters)
			{
				var parameter = _parameters
					.Select((p, i) => new { Index = i, Parameter = p })
					.FirstOrDefault(p => p.Parameter.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
				return (parameter == null) ? -1 : parameter.Index;
			}
		}

		public void Insert(int index, object value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			var parameter = value as IDbDataParameter;
			if (parameter == null)
				throw new InvalidCastException($"The Parameter Collection only accepts non-null IDbDataParameter type objects, not {value.GetType()} objects");
			bool indexOutOfRange;
			lock (_parameters)
			{
				indexOutOfRange = (index < 0) || (index > _parameters.Count);
				if (!indexOutOfRange && !_parameters.Contains(parameter))
				{
					_parameters.Insert(index, parameter);
					return;
				}
			}
			if (indexOutOfRange)
				throw new IndexOutOfRangeException();
			throw new ArgumentException("The parameter is already present in the Parameters collection");
		}

		public void Remove(object value)
		{
			var parameter = value as IDbDataParameter;
			if (parameter == null)
				throw new InvalidCastException($"The Parameter Collection only accepts non-null IDbDataParameter type objects, not {value.GetType()} objects");

			lock (_parameters)
			{
				var indexedParameterToRemove = _parameters
					.Select((p, i) => new { Index = i, Parameter = p })
					.FirstOrDefault(p => p.Parameter == value);
				if (indexedParameterToRemove != null)
				{
					_parameters.RemoveAt(indexedParameterToRemove.Index);
					return;
				}
			}
			throw new ArgumentException("Attempted to remove a parameter that is not contained by this  Parameter Collection");
		}

		public void RemoveAt(int index)
		{
			lock (_parameters)
			{
				if ((index >= 0) && (index < _parameters.Count))
				{
					_parameters.RemoveAt(index);
					return;
				}
			}
			throw new IndexOutOfRangeException();
		}

		public void RemoveAt(string parameterName)
		{
			lock (_parameters)
			{
				var indexedParameterToRemove = _parameters
					.Select((p, i) => new { Index = i, Parameter = p })
					.FirstOrDefault(p => p.Parameter.ParameterName.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
				if (indexedParameterToRemove != null)
				{
					_parameters.RemoveAt(indexedParameterToRemove.Index);
					return;
				}
			}
			throw new IndexOutOfRangeException();
		}

		public IEnumerator<IDbDataParameter> GetEnumerator() { return _parameters.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}
