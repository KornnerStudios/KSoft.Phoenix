
namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovRage
		: BPower
	{
		public sealed class BInterpTable
			: IO.IEndianStreamSerializable
		{
			public float[]? Keys;
			public uint[]? Values; // Type

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				float[] keys = s.IsReading
					? System.Array.Empty<float>()
					: Keys ?? throw new System.InvalidOperationException("Interpolation keys must be initialized before writing.");
				uint[] values = s.IsReading
					? System.Array.Empty<uint>()
					: Values ?? throw new System.InvalidOperationException("Interpolation values must be initialized before writing.");
				BSaveGame.StreamArray16(s, ref keys);
				BSaveGame.StreamArray16(s, ref values);
				Keys = keys;
				Values = values;
			}
			#endregion
		};

		public sealed class BCameraEffectData
			: IO.IEndianStreamSerializable
		{
			public string? Name;
			public BInterpTable ColorTransformRTable = new(), ColorTransformGTable = new(),
				ColorTransformBTable = new();
			public BInterpTable ColorTransformFactorTable = new(),
				BlurFactorTable = new(), // same data gets written 3x :s
				FOVTable = new(), ZoomTable = new(), YawTable = new(),
				PitchTable = new();
			public bool RadialBlur, Use3DPosition, ModeCameraEffect,
				UserHoverPointAs3DPosition;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				string name = s.IsReading
					? string.Empty
					: Name ?? throw new System.InvalidOperationException("Camera effect name must be initialized before writing.");
				s.StreamPascalString32(ref name);
				Name = name;
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
		public BParametricSplineCurve JumpSplineCurve = new();
		public BCameraEffectData CameraEffectData = new();
		public BCostDatum[]? CostPerTick, CostPerTickAttacking, CostPerJump;
		public float TickLength, DamageMultiplier, DamageTakenMultiplier,
			SpeedMultiplier, NudgeMultiplier, ScanRadius;
		public BProtoObjectID ProjectileObject, HandAttachObject, TeleportAttachObject;
		public float AudioReactionTimer, TeleportTime,
			TeleportLateralDistance, TeleportJumpDistance, TimeBetweenRetarget,
			MotionBlurAmount, MotionBlurDistance, MotionBlurTime,
			DistanceVsAngleWeight, HealPerKillCombatValue, AuraRadius, AuraDamageBonus;
		public BProtoObjectID AuraAttachObjectSmall, AuraAttachObjectMedium, AuraAttachObjectLarge,
			HealAttachObject;
		public BEntityID[]? SquadsInAura;
		public BObjectTypeID FilterTypeID;
		public bool CompletedInitialization, HasSuccessfullyAttacked, UsePather;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var owner = s.Owner;
			System.ArgumentNullException.ThrowIfNull(owner);
			var sg = KSoft.Debug.TypeCheck.CastReference<BSaveGame>(owner);

			s.Stream(ref NextTickTime);
			s.Stream(ref TargettedSquad);
			s.StreamV(ref LastDirectionInput); s.StreamV(ref TeleportDestination); s.StreamV(ref PositionInput);
			s.Stream(ref TimeUntilTeleport); s.Stream(ref TimeUntilRetarget);
			s.Stream(ref AttackSound);
			s.Stream(JumpSplineCurve);
			s.Stream(CameraEffectData);
			BCostDatum[] cost_per_tick = s.IsReading
				? System.Array.Empty<BCostDatum>()
				: CostPerTick ?? throw new System.InvalidOperationException("Per-tick cost must be initialized before writing.");
			BCostDatum[] cost_per_tick_attacking = s.IsReading
				? System.Array.Empty<BCostDatum>()
				: CostPerTickAttacking ?? throw new System.InvalidOperationException("Attacking per-tick cost must be initialized before writing.");
			BCostDatum[] cost_per_jump = s.IsReading
				? System.Array.Empty<BCostDatum>()
				: CostPerJump ?? throw new System.InvalidOperationException("Jump cost must be initialized before writing.");
			sg.StreamBCost(s, ref cost_per_tick); sg.StreamBCost(s, ref cost_per_tick_attacking); sg.StreamBCost(s, ref cost_per_jump);
			CostPerTick = cost_per_tick;
			CostPerTickAttacking = cost_per_tick_attacking;
			CostPerJump = cost_per_jump;
			s.Stream(ref TickLength); s.Stream(ref DamageMultiplier); s.Stream(ref DamageTakenMultiplier);
			s.Stream(ref SpeedMultiplier); s.Stream(ref NudgeMultiplier); s.Stream(ref ScanRadius);
			s.Stream(ref ProjectileObject); s.Stream(ref HandAttachObject); s.Stream(ref TeleportAttachObject);
			s.Stream(ref AudioReactionTimer); s.Stream(ref TeleportTime);
			s.Stream(ref TeleportLateralDistance); s.Stream(ref TeleportJumpDistance); s.Stream(ref TimeBetweenRetarget);
			s.Stream(ref MotionBlurAmount); s.Stream(ref MotionBlurDistance); s.Stream(ref MotionBlurTime);
			s.Stream(ref DistanceVsAngleWeight); s.Stream(ref HealPerKillCombatValue); s.Stream(ref AuraRadius); s.Stream(ref AuraDamageBonus);
			s.Stream(ref AuraAttachObjectSmall); s.Stream(ref AuraAttachObjectMedium); s.Stream(ref AuraAttachObjectLarge);
			s.Stream(ref HealAttachObject);
			BEntityID[] squads_in_aura = s.IsReading
				? System.Array.Empty<BEntityID>()
				: SquadsInAura ?? throw new System.InvalidOperationException("Squads in aura must be initialized before writing.");
			BSaveGame.StreamArray(s, ref squads_in_aura);
			SquadsInAura = squads_in_aura;
			s.Stream(ref FilterTypeID);
			s.Stream(ref CompletedInitialization); s.Stream(ref HasSuccessfullyAttacked); s.Stream(ref UsePather);
		}
		#endregion
	};
}
