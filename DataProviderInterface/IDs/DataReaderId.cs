using System;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs
{
	[Serializable]
	public struct DataReaderId
	{
		public DataReaderId(Guid value)
		{
			Value = value;
		}
		public Guid Value { get; }
	}
}
