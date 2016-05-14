using System;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs
{
	[Serializable]
	public sealed class CommandId
	{
		public CommandId(Guid value)
		{
			Value = value;
		}
		public Guid Value { get; }

		public override bool Equals(object obj)
		{
			var otherId = obj as CommandId;
			return (otherId != null) && otherId.Value.Equals(Value);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
	}
}
