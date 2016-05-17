using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.Implementations.Replay
{
	public sealed class QueryCriteria
	{
		public QueryCriteria(string connectionString, string commandText, CommandType commandType, IEnumerable<ParameterInformation> parameters)
		{
			if (connectionString == null)
				throw new ArgumentNullException(nameof(connectionString));
			if (commandText == null)
				throw new ArgumentNullException(nameof(commandText));
			if (parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			ConnectionString = connectionString;
			CommandText = commandText;
			CommandType = commandType;
			Parameters = parameters.ToList().AsReadOnly();
			if (Parameters.Any(p => p == null))
				throw new ArgumentException("null reference encountered in set", nameof(parameters));
		}

		public string ConnectionString { get; }
		public string CommandText { get; }
		public CommandType CommandType { get; }
		public IEnumerable<ParameterInformation> Parameters { get; }

		public override bool Equals(object obj)
		{
			var otherQuery = obj as QueryCriteria;
			if (otherQuery == null)
				return false;
			return
				(otherQuery.ConnectionString == ConnectionString) &&
				(otherQuery.CommandText == CommandText) &&
				(otherQuery.CommandType == CommandType) &&
				(otherQuery.Parameters.Count() == Parameters.Count()) &&
				otherQuery.Parameters.Zip(Parameters, (x, y) => x.Equals(y)).All(parameterMatches => parameterMatches == true);
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = (int)2166136261;
				hash = (hash * 16777619) ^ ConnectionString.GetHashCode();
				hash = (hash * 16777619) ^ CommandText.GetHashCode();
				hash = (hash * 16777619) ^ CommandType.GetHashCode();
				foreach (var parameter in Parameters)
					hash = (hash * 16777619) ^ parameter.GetHashCode();
				return hash;
			}
		}

		public sealed class ParameterInformation
		{
			public ParameterInformation(string parameterName, object value, DbType dbType, bool isNullable, ParameterDirection direction, byte scale, int size)
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));
				
				ParameterName = parameterName;
				Value = value;
				DbType = dbType;
				IsNullable = IsNullable;
				Direction = direction;
				Scale = scale;
				Size = size;
			}

			public string ParameterName { get; }
			public object Value { get; }
			public DbType DbType { get; }
			public bool IsNullable { get; }
			public ParameterDirection Direction { get; }
			public byte Scale { get; }
			public int Size { get; }

			public override bool Equals(object obj)
			{
				var otherParameter = obj as ParameterInformation;
				if (otherParameter == null)
					return false;
				return
					(otherParameter.ParameterName == ParameterName) &&
					AreValuesEqual(otherParameter.Value, Value) && // TODO: Explain
					(otherParameter.DbType == DbType) &&
					(otherParameter.IsNullable == IsNullable) &&
					(otherParameter.Direction == Direction) &&
					(otherParameter.Scale == Scale) &&
					(otherParameter.Size == Size);
			}

			private static bool AreValuesEqual(object x, object y)
			{
				if ((x == null) && (y == null))
					return true;
				else if ((x == null) || (y == null))
					return false;
				else
					return x.Equals(y);
			}

			public override int GetHashCode()
			{
				// Courtesy of http://stackoverflow.com/a/263416
				unchecked // Overflow is fine, just wrap
				{
					int hash = (int)2166136261;
					hash = (hash * 16777619) ^ ParameterName.GetHashCode();
					hash = (hash * 16777619) ^ ((Value == null) ? 0 : Value.GetHashCode());
					hash = (hash * 16777619) ^ DbType.GetHashCode();
					hash = (hash * 16777619) ^ IsNullable.GetHashCode();
					hash = (hash * 16777619) ^ Direction.GetHashCode();
					hash = (hash * 16777619) ^ Scale.GetHashCode();
					hash = (hash * 16777619) ^ Size.GetHashCode();
					return hash;
				}
			}
		}
	}
}
