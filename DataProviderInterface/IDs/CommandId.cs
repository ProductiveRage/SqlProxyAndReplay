using System;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs
{
	[Serializable]
	public struct CommandId
	{
		public CommandId(Guid value)
		{
			Value = value;
		}
		public Guid Value { get; }
		public override string ToString()
		{
			return Value.ToString();
		}
	}
}
