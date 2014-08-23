
using BVector = SlimMath.Vector4;
using BEntityID = System.Int32;
using BEntityRef = System.UInt64; // idk, 8 bytes
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			Entity1 = 0x2710
			;
	};

	public abstract class BEntity
		: IO.IEndianStreamSerializable
	{
		const int cMaximumEntityRefs = 0x3E8;

		public BVector Position, Up, Forward, Velocity;
		public ActionListEntry[] Actions;
		public BEntityRef[] EntityRefs;
		public BEntityID ID;
		public BPlayerID PlayerID;
		public float YDisplacement,
			ObstructionRadiusX, ObstructionRadiusY, ObstructionRadiusZ;
		#region Flags
		public bool FlagCollidable	// 0x9C, 7
			, FlagMoving			// 0x9C, 5
			, FlagDestroy			// 0x9C, 6
			, FlagFirstUpdate		// 0x9C, 4
			, FlagTiesToGround		// 0x9C, 3
			, FlagUseMaxHeight		// 0x9C, 2
			, FlagPhysicsControl	// 0x9C, 1
			, FlagRotateObstruction	// 0x9C, 0
			, FlagFlying			// 0x9D, 7
			, FlagValid				// 0x9D, 6
			, FlagNonMobile			// 0x9D, 5
			, FlagLockedDown		// 0x9D, 4
			, FlagEntityRefsLocked	// 0x9D, 3
			, FlagFlyingHeightFixup	// 0x9D, 2
			, FlagGarrisoned		// 0x9D, 1
			, FlagPassiveGarrisoned	// 0x9D, 0
			, FlagMoved				// 0x9E, 6
			, FlagTeleported		// 0x9E, 5
			, FlagInSniper			// 0x9E, 4
			, FlagIsBuilt			// 0x9E, 3
			, FlagHasSounds			// 0x9E, 2
			, FlagHitched			// 0x9E, 1
			, FlagSprinting			// 0x9E, 0
			, FlagRecovering		// 0x9E, 7
			, FlagInCover			// 0x9F, 6
			, FlagSelectable		// 0x9F, 5
			, FlagUngarrisonValid	// 0x9F, 4
			, FlagGarrisonValid		// 0x9F, 3
			, FlagIsPhysicsReplacement	// 0x9F, 2
			, FlagIsDoneBuilding	// 0x9F, 1
			;
		#endregion

		public bool HasRefs { get { return EntityRefs != null; } }

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref Position); s.StreamV(ref Up); s.StreamV(ref Forward); s.StreamV(ref Velocity);
			BActionManager.StreamActionList(s, ref Actions);

			bool has_refs = s.IsReading ? false : HasRefs;
			s.Stream(ref has_refs);
			if (has_refs)
				BSaveGame.StreamArray16(s, ref EntityRefs, cMaximumEntityRefs);

			s.Stream(ref ID);
			s.Stream(ref PlayerID);
			s.Stream(ref YDisplacement);
			s.Stream(ref ObstructionRadiusX); s.Stream(ref ObstructionRadiusY); s.Stream(ref ObstructionRadiusZ);

			#region Flags
			s.Stream(ref FlagCollidable);		s.Stream(ref FlagMoving);			s.Stream(ref FlagDestroy);
			s.Stream(ref FlagFirstUpdate);		s.Stream(ref FlagTiesToGround);		s.Stream(ref FlagUseMaxHeight);
			s.Stream(ref FlagPhysicsControl);	s.Stream(ref FlagRotateObstruction);s.Stream(ref FlagFlying);
			s.Stream(ref FlagValid);			s.Stream(ref FlagNonMobile);		s.Stream(ref FlagLockedDown);
			s.Stream(ref FlagEntityRefsLocked);	s.Stream(ref FlagFlyingHeightFixup);s.Stream(ref FlagGarrisoned);
			s.Stream(ref FlagPassiveGarrisoned);s.Stream(ref FlagMoved);			s.Stream(ref FlagTeleported);
			s.Stream(ref FlagInSniper);			s.Stream(ref FlagIsBuilt);			s.Stream(ref FlagHasSounds);
			s.Stream(ref FlagHitched);			s.Stream(ref FlagSprinting);		s.Stream(ref FlagRecovering);
			s.Stream(ref FlagInCover);			s.Stream(ref FlagSelectable);		s.Stream(ref FlagUngarrisonValid);
			s.Stream(ref FlagGarrisonValid);	s.Stream(ref FlagIsPhysicsReplacement);	s.Stream(ref FlagIsDoneBuilding);
			#endregion

			s.StreamSignature(cSaveMarker.Entity1);
		}
		#endregion
	};
}