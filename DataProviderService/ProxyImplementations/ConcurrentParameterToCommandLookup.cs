using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations
{
	/// <summary>
	/// This class is used to try to ensure that multi-threaded access to the command-to-parameters-created-by-that-command lookup is supported - primarily to
	/// ensure that not all actions or lookups require the entire collection to be locked. I'm still a little concerned about the updateValueFactory used when
	/// adding a ParameterId to a set associated with a CommandId since that delegate's  is invoked outside of the dictionary's internal lock" (according to
	/// the documentation at https://msdn.microsoft.com/en-us/library/dd997369(v=vs.110).aspx), though the use of an immutable type (for the set of ParameterId
	/// values) has been suggested for other projects (see https://github.com/EasyNetQ/EasyNetQ/issues/281). In normal use, there should not be any race conditions
	/// around adding or removing parameters for a given command since that sort of work is commonly done on a single thread and commands not shared between
	/// threads.
	/// </summary>
	internal sealed class ConcurrentParameterToCommandLookup
	{
		private readonly ConcurrentDictionary<CommandId, SimpleImmutableList<ParameterId>> _data;
		public ConcurrentParameterToCommandLookup()
		{
			_data = new ConcurrentDictionary<CommandId, SimpleImmutableList<ParameterId>>();
		}

		public bool IsRecordedForCommand(ParameterId parameterId, CommandId commandId)
		{
			SimpleImmutableList<ParameterId> parameters;
			if (!_data.TryGetValue(commandId, out parameters))
				return false;
			return parameters.Enumerate().Any(value => value.Equals(parameterId));
		}

		/// <summary>
		/// This will never return null, it will return an empty set if there are no parameters recorded against the command
		/// </summary>
		public IEnumerable<ParameterId> GetParameters(CommandId commandId)
		{
			SimpleImmutableList<ParameterId> parameters;
			if (!_data.TryGetValue(commandId, out parameters))
				return Enumerable.Empty<ParameterId>();
			return parameters.Enumerate();
		}

		public void Record(CommandId commandId, ParameterId parameterId)
		{
			_data.AddOrUpdate(
				commandId,
				addValueFactory: id => SimpleImmutableList<ParameterId>.Empty.Add(parameterId),
				updateValueFactory: (id, value) => value.Add(parameterId)
			);
		}

		public void Remove(CommandId commandId, ParameterId parameterId)
		{
			while (true)
			{
				SimpleImmutableList<ParameterId> parameters;
				if (!_data.TryGetValue(commandId, out parameters))
					return;
				if (_data.TryUpdate(commandId, newValue: parameters.Remove(parameterId), comparisonValue: parameters))
					return;
			}
		}

		public void RemoveAnyParametersFor(CommandId commandId, Action<ParameterId> optionalWhenRemoved = null)
		{
			SimpleImmutableList<ParameterId> parameters;
			if (!_data.TryRemove(commandId, out parameters))
				return;
			if (optionalWhenRemoved != null)
			{
				foreach (var parameter in parameters.Enumerate())
					optionalWhenRemoved(parameter);
			}
		}

		private sealed class SimpleImmutableList<T>
		{
			private static readonly SimpleImmutableList<T> _empty = new SimpleImmutableList<T>(null);
			public static SimpleImmutableList<T> Empty => _empty;

			private readonly Node _nodeIfAny;
			private SimpleImmutableList(Node nodeIfAny)
			{
				_nodeIfAny = nodeIfAny;
			}

			public SimpleImmutableList<T> Add(T value)
			{
				return new SimpleImmutableList<T>(new Node(value, _nodeIfAny));
			}

			public SimpleImmutableList<T> Remove(T value)
			{
				// Could probably do this more efficiently (share more of a node chain in some cases) but this will do the job
				var removedValue = false;
				var newList = Empty;
				var node = _nodeIfAny;
				while (node != null)
				{
					if (AreValuesEqual(node.Value, value))
						removedValue = true;
					else
						newList = newList.Add(value);
					node = node.NextIfAny;
				}
				return removedValue ? newList : this;
			}

			private static bool AreValuesEqual(T x, T y)
			{
				if ((x == null) && (y == null))
					return true;
				else if ((x == null) || (y == null))
					return false;
				return x.Equals(y);
			}

			public IEnumerable<T> Enumerate()
			{
				var node = _nodeIfAny;
				while (node != null)
				{
					yield return node.Value;
					node = node.NextIfAny;
				}
			}

			private sealed class Node
			{
				public Node(T value, Node nextIfAny)
				{
					Value = value;
					NextIfAny = nextIfAny;
				}
				public T Value { get; }
				public Node NextIfAny { get; }
			}
		}
	}
}