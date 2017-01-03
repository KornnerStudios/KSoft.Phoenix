using System;

namespace KSoft.Phoenix.Phx
{
	/// <remarks>
	/// The engine's parsing logic for this duplicates the IsNull and InnerText.IsNullOrEmpty checks for all
	/// the related cases. Would be more efficient to split these up into diff groups to avoid code dupe
	/// </remarks>
	public enum BTriggerVarType : byte
	{
		#region 0x0
		None,

		Tech,
		/// <see cref="BProtoTechStatus"/>
		TechStatus,
		/// <see cref="BOperatorType"/>
		Operator,
		ProtoObject,
		ObjectType,
		ProtoSquad,
		Sound,
		Entity,
		EntityList,
		Trigger, // int
		Time, // int
		Player,
		[Obsolete] UILocation,
		[Obsolete] UIEntity,
		Cost, // TODO: special KV parsing...
		#endregion

		#region 0x10
		/// <remarks>Engine defined</remarks>
		/// <see cref="HaloWars.BAnimType"/>
		AnimType,
		/// <see cref="BActionStatus"/>
		ActionStatus,
		Power,
		Bool,
		Float,
		[Obsolete] Iterator,
		Team, // int
		PlayerList,
		TeamList,
		/// <see cref="BGameData.PlayerStates"/>
		PlayerState,
		Objective, // int
		Unit,
		UnitList,
		Squad,
		SquadList,
		[Obsolete] UIUnit,
		#endregion

		#region 0x20
		[Obsolete] UISquad,
		[Obsolete] UISquadList,
		String,
		MessageIndex, // int
		MessageJustify, // int
		MessagePoint, // float
		Color,
		ProtoObjectList,
		ObjectTypeList,
		ProtoSquadList,
		TechList,
		/// <see cref="BMathOperatorType"/>
		MathOperator,
		/// <see cref="BObjectDataType"/>
		ObjectDataType,
		/// <see cref="BObjectDataRelative"/>
		ObjectDataRelative,
		Civ,
		ProtoObjectCollection,
		#endregion

		#region 0x30
		Object,
		ObjectList,
		Group, // int
		/// <see cref="BGameData.RefCounts"/>
		RefCountType,
		/// <see cref="BGameData.UnitFlags"/>
		UnitFlag,
		/// <see cref="BLOSType"/>
		LOSType,
		[Obsolete] EntityFilterSet,
		/// <remarks>Population Bucket</remarks>
		PopBucket,
		/// <see cref="BListPosition"/>
		ListPosition,
		/// <see cref="BDiplomacy"/>
		Diplomacy,
		/// <see cref="BExposedAction"/>
		ExposedAction,
		/// <see cref="BSquadMode"/>
		SquadMode,
		ExposedScript, // int
		[Obsolete] KBBase,
		[Obsolete] KBBaseList, // engine still initializes the list
		/// <see cref="BDataScalar"/>
		DataScalar,
		#endregion

		#region 0x40
		[Obsolete] KBBaseQuery, // Obsolete? engine sets a flag in the BTriggerVar
		DesignLine, // int
		LocStringID, // int
		Leader,
		Cinematic, // int
		/// <see cref="BFlareType"/>
		FlareType,
		CinematicTag, // int
		IconType, // parses as ProtoObject...
		Difficulty, // int
		Integer, // int (XMB specialized)
		/// <remarks>Engine defined</remarks>
		/// <see cref="HaloWars.BHUDItem"/>
		HUDItem,
		/// <see cref="BUIControlType"/>
		ControlType,
		[Obsolete] UIButton,
		/// <see cref="BMissionType"/>
		MissionType,
		/// <see cref="BMissionState"/>
		MissionState,
		/// <see cref="BMissionTargetType"/>
		MissionTargetType,
		#endregion

		#region 0x50
		IntegerList,
		/// <see cref="BBidType"/>
		BidType,
		/// <see cref="BBidState"/>
		BidState,
		[Obsolete] BuildingCommandState,
		Vector,
		VectorList,
		PlacementRule,
		[Obsolete] KBSquad,
		[Obsolete] KBSquadList, // engine still initializes the list
		KBSquadQuery, // Obsolete? engine sets a flag in the BTriggerVar
		[Obsolete] AISquadAnalysis,
		/// <see cref="BAISquadAnalysisComponent"/>
		AISquadAnalysisComponent,
		[Obsolete] KBSquadFilterSet,
		/// <remarks>Engine defined</remarks>
		/// <see cref="HaloWars.BChatSpeaker"/>
		ChatSpeaker,
		/// <see cref="BRumbleType"/>
		RumbleType,
		/// <see cref="BRumbleMotor"/>
		RumbleMotor,
		#endregion

		#region 0x60
		/// <see cref="BProtoObjectCommandType"/>
		CommandType,
		/// <see cref="BObjectDataType"/>
		SquadDataType,
		/// <see cref="BEventType"/>
		EventType,
		TimeList, // int[]
		DesignLineList, // int[]
		/// <see cref="BGameStatePredicate"/>
		GameStatePredicate,
		FloatList,
		[Obsolete] UILocationMinigame,
		/// <see cref="BGameData.SquadFlags"/>
		SquadFlag,
		/// <remarks>Engine defined</remarks>
		/// <see cref="HaloWars.BFlashableUIItem"/>
		FlashableUIItem, // aka FlashableItems
		TalkingHead, // int (XMB specialized)
		Concept, // int
		ConceptList, // int[]
		UserClassType, // int (XMB specialized)


		#endregion

		// "s/w b/w": sandwiched between

		Distance = Float, // s/w b/w Trigger & Time
		Percent = Float, // s/w b/w ActionStatus & Hitpoints
		Hitpoints = Float, // s/w b/w Percent & Power

		Count = Integer, // s/w b/w Player & Location

		Location = Vector, // s/w b/w Count & UILocation
		Direction = Vector, // s/w b/w TalkingHead & FlareType

		LocationList = VectorList, // s/w b/w Group & RefCountType
	};
}