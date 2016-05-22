using System;
using System.Collections.Concurrent;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	internal sealed class Store<TId, TValue> where TValue : class
	{
		private readonly ConcurrentDictionary<TId, TValue> _data;
		private readonly Func<TId> _idGenerator;
		public Store(Func<TId> idGenerator)
		{
			if (idGenerator == null)
				throw new ArgumentNullException(nameof(idGenerator));

			_data = new ConcurrentDictionary<TId, TValue>();
			_idGenerator = idGenerator;
		}

		/// <summary>
		/// This will throw an exception for a null value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public TId Add(TValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			var id = _idGenerator();
			if (!_data.TryAdd(id, value))
				throw new Exception("Guid.NewGuid generated duplicate values - didn't expect this!");
			return id;
		}

		/// <summary>
		/// This will throw an exception for an invalid id
		/// </summary>
		public TValue Get(TId id)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));

			TValue value;
			if (!_data.TryGetValue(id, out value))
				throw new InvalidIdException<TId>(id, typeof(TValue).Name);
			return value;
		}

		/// <summary>
		/// This will throw an exception for an invalid value. Each value should be unique because they should never be shared (whenever a new SqlConnection
		/// is created, for example, it should be given a unique id and so the id-to-connection ratio should be one-to-one; while a connection may be shared
		/// between multiple commands, those commands should all be linked with the same connection id)
		/// </summary>
		public TId GetIdFor(TValue value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			foreach (var keyValuePair in _data)
			{
				if (keyValuePair.Value == value)
					return keyValuePair.Key;
			}
			throw new ArgumentException("Unable to locate specified value");
		}

		/// <summary>
		/// This will throw an exception for an invalid id
		/// </summary>
		public void Remove(TId id)
		{
			if (id == null)
				throw new ArgumentNullException(nameof(id));

			TValue value;
			if (!_data.TryRemove(id, out value))
				throw new InvalidIdException<TId>(id, typeof(TValue).Name);
		}
	}
}