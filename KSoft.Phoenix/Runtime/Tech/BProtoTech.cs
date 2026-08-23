
namespace KSoft.Phoenix.Runtime
{
	sealed class BProtoTech
		: BProtoBuildableObject
	{
		public float ResearchPoints => BuildPoints;

		public bool OwnStaticData, Unobtainable, Unique,
			Shadow, OrPrereqs, Perpetual,
			NoSound, Instant;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			var owner = s.Owner;
			System.ArgumentNullException.ThrowIfNull(owner);
			var sg = KSoft.Debug.TypeCheck.CastReference<BSaveGame>(owner);

			BSaveGameNullableSerialization.StreamBCost(sg, s, ref Cost);
			s.Stream(ref BuildPoints);
			s.Stream(ref OwnStaticData); s.Stream(ref Unobtainable); s.Stream(ref Unique);
			s.Stream(ref Shadow); s.Stream(ref OrPrereqs); s.Stream(ref Perpetual);
			s.Stream(ref Forbid);
			s.Stream(ref NoSound); s.Stream(ref Instant);
		}
		#endregion
	};
}
