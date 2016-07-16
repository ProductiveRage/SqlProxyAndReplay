using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderService.ProxyImplementations.Replay
{
	[Serializable]
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
				var hash = (int)2166136261;
				hash = (hash * 16777619) ^ ConnectionString.GetHashCode();
				hash = (hash * 16777619) ^ CommandText.GetHashCode();
				hash = (hash * 16777619) ^ CommandType.GetHashCode();
				foreach (var parameter in Parameters)
					hash = (hash * 16777619) ^ parameter.GetHashCode();
				return hash;
			}
		}

		[Serializable]
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
					AreValuesEqual(otherParameter.Value, Value) &&
					(otherParameter.DbType == DbType) &&
					(otherParameter.IsNullable == IsNullable) &&
					(otherParameter.Direction == Direction) &&
					(otherParameter.Scale == Scale) &&
					AreSizesEquivalent(DbType, otherParameter.Size, Size, otherParameter.Value, Value);
			}

			/// <summary>
			/// When comparing two object instances, a reference equality will check the references and not any Equals method that the values
			/// may have - if two strings were compared while they were cast as object then the same string that two separate references pointed
			/// at would not be found to match. To avoid this false negative, we need to ensure that either both references are null or that both
			/// are non-null and that the Equals method on one reference returns true when given the other.
			/// </summary>
			private static bool AreValuesEqual(object x, object y)
			{
				if ((x == null) && (y == null))
					return true;
				else if ((x == null) || (y == null))
					return false;
				else
					return x.Equals(y);
			}

			private static bool AreSizesEquivalent(DbType type, int sizeX, int sizeY, object valueX, object valueY)
			{
				// Parameter Size values can be complicated - for example, sometimes string parameters are specified with a zero length, sometimes
				// with a max length (eg. 4000 or 8000, depending upon whether they're ASCII or unicode) and sometimes with a length that corresponds
				// to the parameter's value. When considering strings, so long as both Size values are large enough for the specified values then we
				// can consider them equivalent.
				if ((type == DbType.AnsiString) || (type == DbType.String))
				{
					var stringX = valueX as string;
					if ((sizeX <= 0) || sizeX >= (stringX ?? "").Length)
						sizeX = stringX.Length;
					var stringY = valueY as string;
					if ((sizeY == 0) || sizeY >= (stringY ?? "").Length)
						sizeY = stringY.Length;
				}
				return (sizeX == sizeY);
			}

			public override int GetHashCode()
			{
				// Courtesy of http://stackoverflow.com/a/263416
				unchecked // Overflow is fine, just wrap
				{
					// Note: Don't include the parameter size in the hash code, size is complicated (see the AreSizesEquivalent method for more info)
					var hash = (int)2166136261;
					hash = (hash * 16777619) ^ ParameterName.GetHashCode();
					hash = (hash * 16777619) ^ ((Value == null) ? 0 : Value.GetHashCode());
					hash = (hash * 16777619) ^ DbType.GetHashCode();
					hash = (hash * 16777619) ^ IsNullable.GetHashCode();
					hash = (hash * 16777619) ^ Direction.GetHashCode();
					hash = (hash * 16777619) ^ Scale.GetHashCode();
					return hash;
				}
			}
		}
	}
}
