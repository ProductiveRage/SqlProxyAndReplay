using System;
using System.Runtime.Serialization;

namespace ProductiveRage.SqlProxyAndReplay.DataProviderInterface
{
	public sealed class InvalidIdException<TId> : Exception
	{
		public InvalidIdException(TId id, string idType) : base($"Invalid {idType} id specified: " + id)
		{
			if (string.IsNullOrWhiteSpace(idType))
				throw new ArgumentException($"Null/blank {nameof(idType)} specified");

			Id = id;
		}
		private InvalidIdException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			// TODO info.AddValue("Id", Id.ToString());
		}
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			// TODO Id = Guid.Parse(info.GetString("Id"));
		}

		public TId Id { get; private set; }
	}
}