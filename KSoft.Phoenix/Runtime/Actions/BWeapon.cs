
namespace KSoft.Phoenix.Runtime
{
	// WTF: Some player's cov_bldg_turret_01's ProtoAction's DamagePerAttack
	// and some other object's (same player) Weapon's DamagePerSecond where
	// being read and somehow getting at least .125 to .25 added to them 
	// So I'm reading them as raw instead

	sealed class BWeapon
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 0xC8;

		public /*float*/uint DamagePerSecond;
		public float DOTrate, DOTduration, 
			MaxRange, MinRange, AOERadius, 
			AOEPrimaryTargetFactor, AOEDistanceFactor, AOEDamageFactor, 
			Accuracy, MovingAccuracy, MaxDeviation,
			MovingMaxDeviation, AccuracyDistanceFactor, AccuracyDeviationFactor,
			MaxVelocityLead, MaxDamagePerRam, ReflectDamageFactor, 
			AirBurstSpan;
		public int ProjectileObjectID, ImpactEffectProtoID;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref DamagePerSecond); s.Stream(ref DOTrate); s.Stream(ref DOTduration); 
			s.Stream(ref MaxRange); s.Stream(ref MinRange); s.Stream(ref AOERadius); 
			s.Stream(ref AOEPrimaryTargetFactor); s.Stream(ref AOEDistanceFactor); s.Stream(ref AOEDamageFactor); 
			s.Stream(ref Accuracy); s.Stream(ref MovingAccuracy); s.Stream(ref MaxDeviation);
			s.Stream(ref MovingMaxDeviation); s.Stream(ref AccuracyDistanceFactor); s.Stream(ref AccuracyDeviationFactor);
			s.Stream(ref MaxVelocityLead); s.Stream(ref MaxDamagePerRam); s.Stream(ref ReflectDamageFactor); 
			s.Stream(ref AirBurstSpan);
			s.Stream(ref ProjectileObjectID); s.Stream(ref ImpactEffectProtoID);
			s.StreamSignature(cSaveMarker.Weapon);
		}
		#endregion
	};
}