using System;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs
{
	[Serializable]
	public struct ConnectionId
	{
		public ConnectionId(Guid value)
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
