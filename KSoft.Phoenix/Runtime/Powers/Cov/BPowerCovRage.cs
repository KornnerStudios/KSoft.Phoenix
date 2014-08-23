
using BVector = SlimMath.Vector4;
using BCost = System.Single;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovRage
		: BPower
	{
		public sealed class BInterpTable
			: IO.IEndianStreamSerializable
		{
			public float[] Keys;
			public uint[] Values; // Type

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				BSaveGame.StreamArray16(s, ref Keys);
				BSaveGame.StreamArray16(s, ref Values);
			}
			#endregion
		};

		public sealed class BCameraEffectData
			: IO.IEndianStreamSerializable
		{
			public string Name;
			public BInterpTable ColorTransformRTable = new BInterpTable(), ColorTransformGTable = new BInterpTable(), 
				ColorTransformBTable = new BInterpTable();
			public BInterpTable ColorTransformFactorTable = new BInterpTable(), 
				BlurFactorTable = new BInterpTable(), // same data gets written 3x :s
				FOVTable = new BInterpTable(), ZoomTable = new BInterpTable(), YawTable = new BInterpTable(),
				PitchTable = new BInterpTable();
			public bool RadialBlur, Use3DPosition, ModeCameraEffect,
				UserHoverPointAs3DPosition;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref Name);
				s.Stream(ColorTransformRTable); s.Stream(ColorTransformGTable); s.Stream(ColorTransformBTable);
				s.Stream(ColorTransformFactorTable);
				s.Stream(BlurFactorTable); s.Stream(BlurFactorTable); s.Stream(BlurFactorTable); // yes, 3x
				s.Stream(FOVTable); s.Stream(ZoomTable); s.Stream(YawTable);
				s.Stream(PitchTable);
				s.Stream(ref RadialBlur); s.Stream(ref Use3DPosition); s.Stream(ref ModeCameraEffect);
				s.Stream(ref UserHoverPointAs3DPosition);
			}
			#endregion
		};

		public double NextTickTime;
		public BEntityID TargettedSquad;
		public BVector LastDirectionInput, TeleportDestination, PositionInput;
		public float TimeUntilTeleport, TimeUntilRetarget;
		public BCueIndex AttackSound;
		public BParametricSplineCurve JumpSplineCurve = new BParametricSplineCurve();
		public BCameraEffectData CameraEffectData = new BCameraEffectData();
		public BCost[] CostPerTick, CostPerTickAttacking, CostPerJump;
		public float TickLength, DamageMultiplier, DamageTakenMultiplier, 
			SpeedMultiplier, NudgeMultiplier, ScanRadius;
		public BProtoObjectID ProjectileObject, HandAttachObject, TeleportAttachObject;
		public float AudioReactionTimer, TeleportTime,
			TeleportLateralDistance, TeleportJumpDistance, TimeBetweenRetarget, 
			MotionBlurAmount, MotionBlurDistance, MotionBlurTime,
			DistanceVsAngleWeight, HealPerKillCombatValue, AuraRadius, AuraDamageBonus;
		public BProtoObjectID AuraAttachObjectSmall, AuraAttachObjectMedium, AuraAttachObjectLarge, 
			HealAttachObject;
		public BEntityID[] SquadsInAura;
		public BObjectTypeID FilterTypeID;
		public bool CompletedInitialization, HasSuccessfullyAttacked, UsePather;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref NextTickTime);
			s.Stream(ref TargettedSquad);
			s.StreamV(ref LastDirectionInput); s.StreamV(ref TeleportDestination); s.StreamV(ref PositionInput);
			s.Stream(ref TimeUntilTeleport); s.Stream(ref TimeUntilRetarget);
			s.Stream(ref AttackSound);
			s.Stream(JumpSplineCurve);
			s.Stream(CameraEffectData);
			sg.StreamBCost(s, ref CostPerTick); sg.StreamBCost(s, ref CostPerTickAttacking); sg.StreamBCost(s, ref CostPerJump);
			s.Stream(ref TickLength); s.Stream(ref DamageMultiplier); s.Stream(ref DamageTakenMultiplier);
			s.Stream(ref SpeedMultiplier); s.Stream(ref NudgeMultiplier); s.Stream(ref ScanRadius);
			s.Stream(ref ProjectileObject); s.Stream(ref HandAttachObject); s.Stream(ref TeleportAttachObject);
			s.Stream(ref AudioReactionTimer); s.Stream(ref TeleportTime);
			s.Stream(ref TeleportLateralDistance); s.Stream(ref TeleportJumpDistance); s.Stream(ref TimeBetweenRetarget);
			s.Stream(ref MotionBlurAmount); s.Stream(ref MotionBlurDistance); s.Stream(ref MotionBlurTime);
			s.Stream(ref DistanceVsAngleWeight); s.Stream(ref HealPerKillCombatValue); s.Stream(ref AuraRadius); s.Stream(ref AuraDamageBonus);
			s.Stream(ref AuraAttachObjectSmall); s.Stream(ref AuraAttachObjectMedium); s.Stream(ref AuraAttachObjectLarge);
			s.Stream(ref HealAttachObject);
			BSaveGame.StreamArray(s, ref SquadsInAura);
			s.Stream(ref FilterTypeID);
			s.Stream(ref CompletedInitialization); s.Stream(ref HasSuccessfullyAttacked); s.Stream(ref UsePather);
		}
		#endregion
	};
}