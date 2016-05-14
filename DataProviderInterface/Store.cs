using System;
using System.Collections.Concurrent;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed class Store<T> where T : class
	{
		private readonly ConcurrentDictionary<Guid, T> _data;
		public Store()
		{
			_data = new ConcurrentDictionary<Guid, T>();
		}

		/// <summary>
		/// This will throw an exception for a null value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Guid Add(T value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			var id = Guid.NewGuid();
			if (!_data.TryAdd(id, value))
				throw new Exception("Guid.NewGuid generated duplicate values - didn't expect this!");
			return id;
		}

		/// <summary>
		/// This will throw an exception for an invalid id
		/// </summary>
		public T Get(Guid id)
		{
			T value;
			if (!_data.TryGetValue(id, out value))
				throw new InvalidIdException(id, typeof(T).Name);
			return value;
		}

		/// <summary>
		/// This will throw an exception for an invalid value. Each value should be unique because they should never be shared (whenever a new SqlConnection
		/// is created, for example, it should be given a unique id and so the id-to-connection ratio should be one-to-one; while a connection may be shared
		/// between multiple commands, those commands should all be linked with the same connection id)
		/// </summary>
		public Guid GetIdFor(T value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			foreach (var keyValuePair in _data) // TODO: Maintain a reverse lookup store to avoid this enumeration?
			{
				if (keyValuePair.Value == value)
					return keyValuePair.Key;
			}
			throw new ArgumentException("Unable to locate specified value");
		}

		/// <summary>
		/// This will throw an exception for an invalid id
		/// </summary>
		public void Remove(Guid id)
		{
			T value;
			if (!_data.TryRemove(id, out value))
				throw new InvalidIdException(id, typeof(T).Name);
		}
	}
}