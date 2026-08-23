
namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			ProtoSquad1 = 0x2710
			;
	};

	sealed class BProtoSquad
		: BProtoObjectBase
	{
		public BProtoSquadID ProtoID;
		public float MaxHP, MaxSP, MaxAmmo; // SP=shield points?
		public int Level, TechLevel, DisplayNameIndex,
			CircleMenuIconID, AltCircleMenuIconID, HPBar;
		public bool OneTimeSpawnUsed, KBAware;
		public bool HasOverrideNodes;
		public BProtoSquadNodeOverride[] OverrideNodes = null!;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var owner = s.Owner;
			System.ArgumentNullException.ThrowIfNull(owner);
			var sg = KSoft.Debug.TypeCheck.CastReference<BSaveGame>(owner);

			s.Stream(ref ProtoID);
			s.Stream(ref BuildPoints);
			BSaveGameNullableSerialization.StreamBCost(sg, s, ref Cost);
			s.Stream(ref MaxHP); s.Stream(ref MaxSP); s.Stream(ref MaxAmmo);
			s.Stream(ref Level); s.Stream(ref TechLevel); s.Stream(ref DisplayNameIndex);
			s.Stream(ref CircleMenuIconID); s.Stream(ref AltCircleMenuIconID); s.Stream(ref HPBar);
			s.Stream(ref Available); s.Stream(ref Forbid);
			s.Stream(ref OneTimeSpawnUsed); s.Stream(ref KBAware);
			s.Stream(ref HasOverrideNodes);
			if (HasOverrideNodes)
			{
				BSaveGame.StreamArray(s, ref OverrideNodes);
			}

			s.StreamSignature(cSaveMarker.ProtoSquad1);
		}
		#endregion
	};
}
