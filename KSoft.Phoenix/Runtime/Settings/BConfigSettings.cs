
namespace KSoft.Phoenix.Runtime
{
	sealed class BConfigSettings
		: IO.IEndianStreamSerializable
	{
		const uint kVersion = 0;

		public bool 
			NoFogMask,			AIDisable,			AIShadow,			NoVismap,					NoRandomPlayerPlacement, 
			DisableOneBuilding,	BuildingQueue,		UseTestLeaders,		EnableFlight,				NoBirthAnims, 
			Veterancy,			TrueLOS,			NoDestruction,		CoopSharedResources,		MaxProjectileHeightForDecal,
			EnableSubbreakage,	EnableThrowPart,	AllowAnimIsDirty,	NoVictoryCondition,			AIAutoDifficulty,
			Demo,				AsyncWorldUpdate,	EnableHintSystem,	PercentFadeTimeCorpseSink,	CorpseSinkSpeed,
			CorpseMinScale,		BlockOutsideBounds,	AINoAttack,			PassThroughOwnVehicles,		EnableCapturePointResourceSharing,
			NoUpdatePathingQuad,SlaveUnitPosition,	Turning,			HumanAttackMove,			MoreNewMovement3,
			OverrideGroundIK,	DriveWarthog,		EnableCorpses,		DisablePathingLimits,		DisableVelocityMatchingBySquadType,
			ActiveAbilities,	AlphaTest,			NoDamage,			IgnoreAllPlatoonmates,		ClassicPlatoonGrouping,
			NoShieldDamage,		EnableSubUpdating,	MPSubUpdating,		AlternateSubUpdating,		DynamicSubUpdateTime,
			DecoupledUpdate;

		public float
			PlatoonRadius,	ProjectionTime,	OverrideGroundIKRange,	OverrideGroundIKTiltFactor,	GameSpeed;

		#region IEndianStreamSerializable Members
		void Read(IO.EndianReader s, out float value)
		{
			if (s.ReadBoolean())
				value = s.ReadSingle();
			else
				value = 0f;
		}
		void Write(IO.EndianWriter s, float value)
		{
			bool not_zero = value != 0f;
			s.Write(not_zero);

			if (not_zero) s.Write(value);
		}
		void ReadFloats(IO.EndianReader s)
		{
			Read(s, out PlatoonRadius);
			Read(s, out ProjectionTime);
			Read(s, out OverrideGroundIKRange);
			Read(s, out OverrideGroundIKTiltFactor);
			Read(s, out GameSpeed);
		}
		void WriteFloats(IO.EndianWriter s)
		{
			Write(s, PlatoonRadius);
			Write(s, ProjectionTime);
			Write(s, OverrideGroundIKRange);
			Write(s, OverrideGroundIKTiltFactor);
			Write(s, GameSpeed);
		}

		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersion);

			s.Stream(ref NoFogMask);			s.Stream(ref AIDisable);			s.Stream(ref AIShadow);			s.Stream(ref NoVismap);					s.Stream(ref NoRandomPlayerPlacement);
			s.Stream(ref DisableOneBuilding);	s.Stream(ref BuildingQueue);		s.Stream(ref UseTestLeaders);	s.Stream(ref EnableFlight);				s.Stream(ref NoBirthAnims);
			s.Stream(ref Veterancy);			s.Stream(ref TrueLOS);				s.Stream(ref NoDestruction);	s.Stream(ref CoopSharedResources);		s.Stream(ref MaxProjectileHeightForDecal);
			s.Stream(ref EnableSubbreakage);	s.Stream(ref EnableThrowPart);		s.Stream(ref AllowAnimIsDirty);	s.Stream(ref NoVictoryCondition);		s.Stream(ref AIAutoDifficulty);
			s.Stream(ref Demo);					s.Stream(ref AsyncWorldUpdate);		s.Stream(ref EnableHintSystem);	s.Stream(ref PercentFadeTimeCorpseSink);s.Stream(ref CorpseSinkSpeed);
			s.Stream(ref CorpseMinScale);		s.Stream(ref BlockOutsideBounds);	s.Stream(ref AINoAttack);		s.Stream(ref PassThroughOwnVehicles);	s.Stream(ref EnableCapturePointResourceSharing);
			s.Stream(ref NoUpdatePathingQuad);	s.Stream(ref SlaveUnitPosition);	s.Stream(ref Turning);			s.Stream(ref HumanAttackMove);			s.Stream(ref MoreNewMovement3);
			s.Stream(ref OverrideGroundIK);		s.Stream(ref DriveWarthog);			s.Stream(ref EnableCorpses);	s.Stream(ref DisablePathingLimits);		s.Stream(ref DisableVelocityMatchingBySquadType);
			s.Stream(ref ActiveAbilities);		s.Stream(ref AlphaTest);			s.Stream(ref NoDamage);			s.Stream(ref IgnoreAllPlatoonmates);	s.Stream(ref ClassicPlatoonGrouping);
			s.Stream(ref NoShieldDamage);		s.Stream(ref EnableSubUpdating);	s.Stream(ref MPSubUpdating);	s.Stream(ref AlternateSubUpdating);		s.Stream(ref DynamicSubUpdateTime);
			s.Stream(ref DecoupledUpdate);

			s.StreamMethods(ReadFloats, WriteFloats);
		}
		#endregion
	};
}