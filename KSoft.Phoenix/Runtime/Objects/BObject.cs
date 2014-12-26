using Contract = System.Diagnostics.Contracts.Contract;

using BVector = SlimMath.Vector4;
using BBitVector32 = System.UInt32;
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			Object1 = 0x2710
			;
	};

	public struct BObjectAnimationState
		: IO.IEndianStreamSerializable
	{
		public short AnimType;
		public short TweenToAnimation;
		public float TweenTime, MoveSpeed;
		public int ForceAnimID;
		public sbyte State;
		public sbyte ExitAction;
		public bool
			FlagDirty, FlagMoving, FlagTurning,
			FlagReset, FlagApplyInstantly, FlagLock,
			FlagOverrideExitAction
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref AnimType);
			s.Stream(ref TweenToAnimation);
			s.Stream(ref TweenTime); s.Stream(ref MoveSpeed);
			s.Stream(ref ForceAnimID);
			s.Stream(ref State);
			s.Stream(ref ExitAction);
			s.Stream(ref FlagDirty);	s.Stream(ref FlagMoving);			s.Stream(ref FlagTurning);
			s.Stream(ref FlagReset);	s.Stream(ref FlagApplyInstantly);	s.Stream(ref FlagLock);
			s.Stream(ref FlagOverrideExitAction);
		}
		#endregion
	};

	public sealed class BObject
		: BEntity
	{
		const int kUVOffsetsSize = 0x18;
		const int cMaximumObjectAttachments = 0xFA;
		const int cMaximumAdditionalTextures = 0xFA;
		const int cMaximumHardpoints = 0x64;

		public BVector CenterOffset, IconColorSize;
		public byte[] UVOffsets = new byte[kUVOffsetsSize];
		public uint MultiframeTextureIndex;
		public int VisualVariationIndex;
		public BVisual Visual;
		public float AnimationRate, Radius, MoveAnimationPosition, HighlightIntensity;
		public uint SubUpdateNumber;
		public BBitVector32 PlayerVisibility, DoppleBits;
		public int SimX, SimZ;
		public float LOSScalar;
		public int LastSimLOS;
		public BObjectAttachments[] ObjectAttachments;
		public BAdditionalTextures[] AdditionalTextures;
		public BHardpointState[] HardpointState;
		public BObjectAnimationState AnimationState;
		public uint AnimationLockEnds;
		public int ProtoID;
		public BPlayerID ColorPlayerID;
		public uint OverrideTint,
			OverrideFlashInterval, OverrideFlashIntervalTimer, OverrideFlashDuration,
			LifespanExpiration;
		public float CurrentAlphaTime, AlphaFadeDuration,
			SelectionPulseTime, SelectionPulsePercent, SelectionFlashTime, SelectionPulseSpeed,
			LastRealtime;
		public byte AOTintValue, TeamSelectionMask;
		public float LOSRevealTime;

		#region Flags
		public bool
			FlagVisibility, FlagLOS, FlagHasLifespan, FlagDopples,
			FlagIsFading, FlagAnimationDisabled, FlagIsRevealer, FlagDontInterpolate,
			FlagBlockLOS, FlagCloaked, FlagCloakDetected, FlagGrayMapDopples,
			FlagLOSMarked, FlagUseLOSScalar, FlagLOSDirty, FlagAnimationLocked,
			FlagUpdateSquadPositionOnAnimationUnlock, FlagExistSoundPlaying, FlagNoUpdate, FlagSensorLockTagged,
			FlagNoReveal, FlagBuilt, FlagBeingCaptured, FlagInvulnerable,
			FlagVisibleForOwnerOnly, FlagVisibleToAll, FlagNearLayer, FlagIKDisabled,
			FlagHasTrackMask, FlagLODFading, FlagOccluded, FlagFadeOnDeath,
			FlagObscurable, FlagNoRender, FlagTurning, FlagAppearsBelowDecals,
			FlagSkipMotionExtraction, FlagOverrideTint, FlagMotionCollisionChecked,
			FlagIsDopple, FlagIsImpactEffect, FlagDebugRenderAreaAttackRange, FlagDontLockMovementAnimation,
			FlagRemainVisible, FlagVisibleForTeamOnly, FlagDontAutoAttackMe, FlagAlwaysAttackReviveUnits,
			FlagNoRenderForOwner, FlagNoRenderDuringCinematic, FlagUseCenterOffset, FlagNotDoppleFriendly,
			FlagForceVisibilityUpdateNextFrame, FlagTurningRight, FlagIsUnderCinematicControl, FlagNoWorldUpdate;
		#endregion

		public bool IsObstruction; // "mpObstructionNode != NULL"

		public bool HasObjectAttachments { get { return ObjectAttachments != null; } }
		public bool HasAdditionalTextures { get { return AdditionalTextures != null; } }

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref CenterOffset); s.StreamV(ref IconColorSize);
			s.Stream(UVOffsets);
			s.Stream(ref MultiframeTextureIndex);
			s.Stream(ref VisualVariationIndex);
			BVisualManager.Stream(s, ref Visual);
			s.Stream(ref AnimationRate); s.Stream(ref Radius); s.Stream(ref MoveAnimationPosition); s.Stream(ref HighlightIntensity);
			s.Stream(ref SubUpdateNumber);
			s.Stream(ref PlayerVisibility); s.Stream(ref DoppleBits);
			s.Stream(ref SimX); s.Stream(ref SimZ);
			s.Stream(ref LOSScalar);
			s.Stream(ref LastSimLOS);

			if (s.StreamCond(this, me => me.HasObjectAttachments))
				BSaveGame.StreamArray(s, ref ObjectAttachments, cMaximumObjectAttachments);

			if (s.StreamCond(this, me => me.HasAdditionalTextures))
				BSaveGame.StreamArray(s, ref AdditionalTextures, cMaximumAdditionalTextures);

			BSaveGame.StreamArray(s, ref HardpointState, cMaximumHardpoints);
			s.Stream(ref AnimationState);
			s.Stream(ref AnimationLockEnds);
			s.Stream(ref ProtoID);
			s.Stream(ref ColorPlayerID);
			s.Stream(ref OverrideTint);
			s.Stream(ref OverrideFlashInterval); s.Stream(ref OverrideFlashIntervalTimer); s.Stream(ref OverrideFlashDuration);
			s.Stream(ref LifespanExpiration);
			s.Stream(ref CurrentAlphaTime); s.Stream(ref AlphaFadeDuration);
			s.Stream(ref SelectionPulseTime); s.Stream(ref SelectionPulsePercent); s.Stream(ref SelectionFlashTime); s.Stream(ref SelectionPulseSpeed);
			s.Stream(ref LastRealtime);
			s.Stream(ref AOTintValue);
			s.Stream(ref TeamSelectionMask);
			s.Stream(ref LOSRevealTime);

			#region Flags
			s.Stream(ref FlagVisibility); s.Stream(ref FlagLOS); s.Stream(ref FlagHasLifespan); s.Stream(ref FlagDopples);
			s.Stream(ref FlagIsFading); s.Stream(ref FlagAnimationDisabled); s.Stream(ref FlagIsRevealer); s.Stream(ref FlagDontInterpolate);
			s.Stream(ref FlagBlockLOS); s.Stream(ref FlagCloaked); s.Stream(ref FlagCloakDetected); s.Stream(ref FlagGrayMapDopples);
			s.Stream(ref FlagLOSMarked); s.Stream(ref FlagUseLOSScalar); s.Stream(ref FlagLOSDirty); s.Stream(ref FlagAnimationLocked);
			s.Stream(ref FlagUpdateSquadPositionOnAnimationUnlock); s.Stream(ref FlagExistSoundPlaying); s.Stream(ref FlagNoUpdate); s.Stream(ref FlagSensorLockTagged);
			s.Stream(ref FlagNoReveal); s.Stream(ref FlagBuilt); s.Stream(ref FlagBeingCaptured); s.Stream(ref FlagInvulnerable);
			s.Stream(ref FlagVisibleForOwnerOnly); s.Stream(ref FlagVisibleToAll); s.Stream(ref FlagNearLayer); s.Stream(ref FlagIKDisabled);
			s.Stream(ref FlagHasTrackMask); s.Stream(ref FlagLODFading); s.Stream(ref FlagOccluded); s.Stream(ref FlagFadeOnDeath);
			s.Stream(ref FlagObscurable); s.Stream(ref FlagNoRender); s.Stream(ref FlagTurning); s.Stream(ref FlagAppearsBelowDecals);
			s.Stream(ref FlagSkipMotionExtraction); s.Stream(ref FlagOverrideTint); s.Stream(ref FlagMotionCollisionChecked); s.Stream(ref FlagIsDopple);
			s.Stream(ref FlagIsImpactEffect); s.Stream(ref FlagDebugRenderAreaAttackRange); s.Stream(ref FlagDontLockMovementAnimation); s.Stream(ref FlagRemainVisible);
			s.Stream(ref FlagVisibleForTeamOnly); s.Stream(ref FlagDontAutoAttackMe); s.Stream(ref FlagAlwaysAttackReviveUnits); s.Stream(ref FlagNoRenderForOwner);
			s.Stream(ref FlagNoRenderDuringCinematic); s.Stream(ref FlagUseCenterOffset); s.Stream(ref FlagNotDoppleFriendly); s.Stream(ref FlagForceVisibilityUpdateNextFrame);
			s.Stream(ref FlagTurningRight); s.Stream(ref FlagIsUnderCinematicControl); s.Stream(ref FlagNoWorldUpdate);
			#endregion

			s.Stream(ref IsObstruction);

			Contract.Assert(false);// mpPhysicsObject

			s.StreamSignature(cSaveMarker.Object1);
		}
		#endregion
	};
}