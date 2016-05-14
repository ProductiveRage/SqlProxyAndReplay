using System;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface.IDs
{
	[Serializable]
	public struct TransactionId
	{
		public TransactionId(Guid value)
		{
			Value = value;
		}
		public Guid Value { get; }
	}
}
