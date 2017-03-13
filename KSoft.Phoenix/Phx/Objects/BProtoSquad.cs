
using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoSquad
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Squad")
		{
			DataName = DatabaseNamedObject.kXmlAttrName,
			Flags = //XML.BCollectionXmlParamsFlags.ToLowerDataNames |
				XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading |
				XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Game,
			Directory = Engine.GameDirectory.Data,
			FileName = "Squads.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Squads_Update.xml",
			RootName = kBListXmlParams.RootName
		};

		static readonly Collections.CodeEnum<BProtoSquadFlags> kFlagsProtoEnum = new Collections.CodeEnum<BProtoSquadFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);
		#endregion

		#region FormationType
		BProtoSquadFormationType mFormationType = BProtoSquadFormationType.Standard;
		public BProtoSquadFormationType FormationType
		{
			get { return mFormationType; }
			set { mFormationType = value; }
		}
		#endregion

		#region PortraitIcon
		string mPortraitIcon;
		public string PortraitIcon
		{
			get { return mPortraitIcon; }
			set { mPortraitIcon = value; }
		}
		#endregion
		#region AltIcon
		string mAltIcon;
		public string AltIcon
		{
			get { return mAltIcon; }
			set { mAltIcon = value; }
		}
		#endregion
		#region Stance
		BSquadStance mStance = BSquadStance.Defensive;
		public BSquadStance Stance
		{
			get { return mStance; }
			set { mStance = value; }
		}
		#endregion
		#region TrainAnim
		string mTrainAnim;
		[Meta.BAnimTypeReference]
		public string TrainAnim
		{
			get { return mTrainAnim; }
			set { mTrainAnim = value; }
		}
		#endregion
		#region Selection
		static BVector cDefaultSelectionRadius { get { return new BVector(1.0f, 0.0f, 1.0f, 0.0f); } }

		BVector mSelectionRadius = cDefaultSelectionRadius;
		public BVector SelectionRadius
		{
			get { return mSelectionRadius; }
			set { mSelectionRadius = value; }
		}

		BVector mSelectionOffset;
		public BVector SelectionOffset
		{
			get { return mSelectionOffset; }
			set { mSelectionOffset = value; }
		}

		bool mSelectionConformToTerrain;
		public bool SelectionConformToTerrain
		{
			get { return mSelectionConformToTerrain; }
			set { mSelectionConformToTerrain = value; }
		}

		bool mSelectionAllowOrientation = true;
		public bool SelectionAllowOrientation
		{
			get { return mSelectionAllowOrientation; }
			set { mSelectionAllowOrientation = value; }
		}

		bool HasSelectionData { get {
			return SelectionRadius != cDefaultSelectionRadius
				|| PhxPredicates.IsNotZero(SelectionOffset)
				|| SelectionConformToTerrain
				|| SelectionAllowOrientation==false;
		} }
		#endregion
		#region HPBar

		int mHPBarID = TypeExtensions.kNone;
		[Meta.BProtoHPBarReference]
		public int HPBarID
		{
			get { return mHPBarID; }
			set { mHPBarID = value; }
		}

		BVector mHPBarSize;
		public BVector HPBarSize
		{
			get { return mHPBarSize; }
			set { mHPBarSize = value; }
		}

		BVector mHPBarOffset;
		public BVector HPBarOffset
		{
			get { return mHPBarOffset; }
			set { mHPBarOffset = value; }
		}

		bool HasHPBarData { get {
			return HPBarID.IsNotNone()
				|| PhxPredicates.IsNotZero(HPBarSize)
				|| PhxPredicates.IsNotZero(HPBarOffset);
		} }
		#endregion
		#region VeterancyBar
		static BVector cDefaultVeterancyBarSize { get { return new BVector(1.0f, 1.0f, 0.0f, 0.0f); } }

		int mVeterancyBarID = TypeExtensions.kNone;
		[Meta.BProtoVeterancyBarReference]
		public int VeterancyBarID
		{
			get { return mVeterancyBarID; }
			set { mVeterancyBarID = value; }
		}

		int mVeterancyCenteredBarID = TypeExtensions.kNone;
		[Meta.BProtoVeterancyBarReference]
		public int VeterancyCenteredBarID
		{
			get { return mVeterancyCenteredBarID; }
			set { mVeterancyCenteredBarID = value; }
		}

		BVector mVeterancyBarSize = cDefaultVeterancyBarSize;
		public BVector VeterancyBarSize
		{
			get { return mVeterancyBarSize; }
			set { mVeterancyBarSize = value; }
		}

		BVector mVeterancyBarOffset;
		public BVector VeterancyBarOffset
		{
			get { return mVeterancyBarOffset; }
			set { mVeterancyBarOffset = value; }
		}

		bool HasVeterancyBarData { get {
			return VeterancyBarID.IsNotNone()
				|| VeterancyCenteredBarID.IsNotNone()
				|| VeterancyBarSize != cDefaultVeterancyBarSize
				|| PhxPredicates.IsNotZero(VeterancyBarOffset);
		} }
		#endregion
		#region AbilityRecoveryBar
		static BVector cDefaultAbilityRecoveryBarSize { get { return new BVector(1.0f, 1.0f, 0.0f, 0.0f); } }

		int mAbilityRecoveryBarID = TypeExtensions.kNone;
		[Meta.BProtoPieProgressReference]
		public int AbilityRecoveryBarID
		{
			get { return mAbilityRecoveryBarID; }
			set { mAbilityRecoveryBarID = value; }
		}

		int mAbilityRecoveryCenteredBarID = TypeExtensions.kNone;
		[Meta.BProtoPieProgressReference]
		public int AbilityRecoveryCenteredBarID
		{
			get { return mAbilityRecoveryCenteredBarID; }
			set { mAbilityRecoveryCenteredBarID = value; }
		}

		BVector mAbilityRecoveryBarSize = cDefaultAbilityRecoveryBarSize;
		public BVector AbilityRecoveryBarSize
		{
			get { return mAbilityRecoveryBarSize; }
			set { mAbilityRecoveryBarSize = value; }
		}

		BVector mAbilityRecoveryBarOffset;
		public BVector AbilityRecoveryBarOffset
		{
			get { return mAbilityRecoveryBarOffset; }
			set { mAbilityRecoveryBarOffset = value; }
		}

		bool HasAbilityRecoveryBarData { get {
			return AbilityRecoveryBarID.IsNotNone()
				|| AbilityRecoveryCenteredBarID.IsNotNone()
				|| AbilityRecoveryBarSize != cDefaultAbilityRecoveryBarSize
				|| PhxPredicates.IsNotZero(AbilityRecoveryBarOffset);
		} }
		#endregion
		#region BobbleHeadID
		int mBobbleHeadID = TypeExtensions.kNone;
		[Meta.BProtoBobbleHeadReference]
		public int BobbleHeadID
		{
			get { return mBobbleHeadID; }
			set { mBobbleHeadID = value; }
		}
		#endregion
		#region BuildingStrengthDisplayID
		int mBuildingStrengthDisplayID = TypeExtensions.kNone;
		[Meta.BProtoBuildingStrengthReference]
		public int BuildingStrengthDisplayID
		{
			get { return mBuildingStrengthDisplayID; }
			set { mBuildingStrengthDisplayID = value; }
		}
		#endregion
		#region CryoPoints
		float mCryoPoints = PhxUtil.kInvalidSingle;
		/// <remarks>Actually defaults to the default value in GameData</remarks>
		public float CryoPoints
		{
			get { return mCryoPoints; }
			set { mCryoPoints = value; }
		}
		#endregion
		#region DazeResist
		float mDazeResist = 1.0f;
		public float DazeResist
		{
			get { return mDazeResist; }
			set { mDazeResist = value; }
		}
		#endregion
		#region Birth

		BSquadBirthType mBirthType = BSquadBirthType.Invalid;
		public BSquadBirthType BirthType
		{
			get { return mBirthType; }
			set { mBirthType = value; }
		}

		string mBirthBone;
		public string BirthBone
		{
			get { return mBirthBone; }
			set { mBirthBone = value; }
		}

		string mBirthEndBone;
		public string BirthEndBone
		{
			get { return mBirthEndBone; }
			set { mBirthEndBone = value; }
		}

		string mBirthAnim0;
		[Meta.BAnimTypeReference]
		public string BirthAnim0
		{
			get { return mBirthAnim0; }
			set { mBirthAnim0 = value; }
		}

		string mBirthAnim1;
		[Meta.BAnimTypeReference]
		public string BirthAnim1
		{
			get { return mBirthAnim1; }
			set { mBirthAnim1 = value; }
		}

		string mBirthAnim2;
		[Meta.BAnimTypeReference]
		public string BirthAnim2
		{
			get { return mBirthAnim2; }
			set { mBirthAnim2 = value; }
		}

		string mBirthAnim3;
		[Meta.BAnimTypeReference]
		public string BirthAnim3
		{
			get { return mBirthAnim3; }
			set { mBirthAnim3 = value; }
		}

		string mBirthTrainerAnim;
		[Meta.BAnimTypeReference]
		public string BirthTrainerAnim
		{
			get { return mBirthTrainerAnim; }
			set { mBirthTrainerAnim = value; }
		}

		bool HasBirthData { get {
			return BirthType != BSquadBirthType.Invalid
				|| BirthBone.IsNotNullOrEmpty()
				|| BirthEndBone.IsNotNullOrEmpty()
				|| BirthAnim0.IsNotNullOrEmpty()
				|| BirthAnim1.IsNotNullOrEmpty()
				|| BirthAnim2.IsNotNullOrEmpty()
				|| BirthAnim3.IsNotNullOrEmpty()
				|| BirthTrainerAnim.IsNotNullOrEmpty();
		} }
		#endregion
		public Collections.BListArray<BProtoSquadUnit> Units { get; private set; }
		#region SubSelectSort
		int mSubSelectSort = int.MaxValue;
		public int SubSelectSort
		{
			get { return mSubSelectSort; }
			set { mSubSelectSort = value; }
		}
		#endregion
		#region TurnRadius

		float mTurnRadiusMin = PhxUtil.kInvalidSingle;
		public float TurnRadiusMin
		{
			get { return mTurnRadiusMin; }
			set { mTurnRadiusMin = value; }
		}

		float mTurnRadiusMax = PhxUtil.kInvalidSingle;
		public float TurnRadiusMax
		{
			get { return mTurnRadiusMax; }
			set { mTurnRadiusMax = value; }
		}

		bool HasTurnRadiusData { get {
			return PhxPredicates.IsNotInvalid(TurnRadiusMin)
				|| PhxPredicates.IsNotInvalid(TurnRadiusMax);
		} }
		#endregion
		#region LeashDistance
		float mLeashDistance;
		public float LeashDistance
		{
			get { return mLeashDistance; }
			set { mLeashDistance = value; }
		}
		#endregion
		#region LeashDeadzone
		const float cDefaultLeashDeadzone = 10.0f;

		float mLeashDeadzone = cDefaultLeashDeadzone;
		public float LeashDeadzone
		{
			get { return mLeashDeadzone; }
			set { mLeashDeadzone = value; }
		}
		#endregion
		#region LeashRecallDelay
		const int cDefaultLeashRecallDelay = 2 * 1000;

		int mLeashRecallDelay = cDefaultLeashRecallDelay;
		/// <remarks>in milliseconds</remarks>
		public int LeashRecallDelay
		{
			get { return mLeashRecallDelay; }
			set { mLeashRecallDelay = value; }
		}
		#endregion
		#region AggroDistance
		float mAggroDistance;
		public float AggroDistance
		{
			get { return mAggroDistance; }
			set { mAggroDistance = value; }
		}
		#endregion
		#region MinimapScale
		float mMinimapScale;
		public float MinimapScale
		{
			get { return mMinimapScale; }
			set { mMinimapScale = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
		#region Level
		int mLevel;
		public int Level
		{
			get { return mLevel; }
			set { mLevel = value; }
		}
		#endregion
		#region TechLevel
		int mTechLevel;
		public int TechLevel
		{
			get { return mTechLevel; }
			set { mTechLevel = value; }
		}
		#endregion
		public Collections.BListArray<BProtoSquadSound> Sounds { get; private set; }
		#region RecoveringEffectID
		int mRecoveringEffectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int RecoveringEffectID
		{
			get { return mRecoveringEffectID; }
			set { mRecoveringEffectID = value; }
		}
		#endregion
		#region CanAttackWhileMoving
		bool mCanAttackWhileMoving = true;
		public bool CanAttackWhileMoving
		{
			get { return mCanAttackWhileMoving; }
			set { mCanAttackWhileMoving = value; }
		}
		#endregion

		/// <summary>Is this Squad just made up of a single Unit?</summary>
		public bool SquadIsUnit { get {
			return Units.Count == 1 && Units[0].Count == 1;
		}}

		public BProtoSquad() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_CostLowercaseType)
		{
			var textData = base.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasRolloverTextID = true;
			textData.HasStatsNameID = true;
			textData.HasPrereqTextID = true;
			textData.HasRoleTextID = true;

			Units = new Collections.BListArray<BProtoSquadUnit>();

			Flags = new Collections.BBitSet(kFlagsParams);

			Sounds = new Collections.BListArray<BProtoSquadSound>();
		}

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("formationType", ref mFormationType, e => e != BProtoSquadFormationType.Standard);

			s.StreamElementOpt("PortraitIcon", ref mPortraitIcon, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("AltIcon", ref mAltIcon, Predicates.IsNotNullOrEmpty);
			s.StreamElementEnumOpt("Stance", ref mStance, e => e != BSquadStance.Defensive);
			s.StreamElementOpt("TrainAnim", ref mTrainAnim, Predicates.IsNotNullOrEmpty);
			#region Selection
			using (var bm = s.EnterCursorBookmarkOpt("Selection", this, v => v.HasSelectionData)) if (bm.IsNotNull)
			{
				// #NOTE assumes cDefaultSelectionRadius's XZ values are 1.0
				s.StreamElementOpt("RadiusX", ref mSelectionRadius.X, PhxPredicates.IsNotOne);
				s.StreamElementOpt("RadiusZ", ref mSelectionRadius.Z, PhxPredicates.IsNotOne);
				s.StreamElementOpt("YOffset", ref mSelectionOffset.Y, Predicates.IsNotZero);
				s.StreamElementOpt("ZOffset", ref mSelectionOffset.Z, Predicates.IsNotZero);
				s.StreamElementOpt("ConformToTerrain", ref mSelectionConformToTerrain, Predicates.IsTrue);
				s.StreamElementOpt("AllowOrientation", ref mSelectionAllowOrientation, Predicates.IsFalse);
			}
			#endregion
			#region HPBar
			using (var bm = s.EnterCursorBookmarkOpt("HPBar", this, v => v.HasHPBarData)) if (bm.IsNotNull)
			{
				xs.StreamHPBarName(s, XML.XmlUtil.kNoXmlName, ref mHPBarID, HPBarDataObjectKind.HPBar, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				s.StreamAttributeOpt("sizeX", ref mHPBarSize.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("sizeY", ref mHPBarSize.Y, Predicates.IsNotZero);
				s.StreamBVector("offset", ref mHPBarOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			}
			#endregion
			#region VeterancyBar
			using (var bm = s.EnterCursorBookmarkOpt("VeterancyBar", this, v => v.HasVeterancyBarData)) if (bm.IsNotNull)
			{
				xs.StreamHPBarName(s, XML.XmlUtil.kNoXmlName, ref mVeterancyBarID, HPBarDataObjectKind.VeterancyBar, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				xs.StreamHPBarName(s, "Centered", ref mVeterancyCenteredBarID, HPBarDataObjectKind.VeterancyBar, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				// #NOTE assumes cDefaultVeterancyBarSize's XY values are 1.0
				s.StreamAttributeOpt("sizeX", ref mVeterancyBarSize.X, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("sizeY", ref mVeterancyBarSize.Y, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("offsetX", ref mVeterancyBarOffset.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("offsetY", ref mVeterancyBarOffset.Y, Predicates.IsNotZero);
			}
			#endregion
			#region AbilityRecoveryBar
			using (var bm = s.EnterCursorBookmarkOpt("AbilityRecoveryBar", this, v => v.HasAbilityRecoveryBarData)) if (bm.IsNotNull)
			{
				xs.StreamHPBarName(s, XML.XmlUtil.kNoXmlName, ref mAbilityRecoveryBarID, HPBarDataObjectKind.PieProgress, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				xs.StreamHPBarName(s, "Centered", ref mAbilityRecoveryCenteredBarID, HPBarDataObjectKind.PieProgress, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
				// #NOTE assumes cDefaultAbilityRecoveryBarSize's XY values are 1.0
				s.StreamAttributeOpt("sizeX", ref mAbilityRecoveryBarSize.X, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("sizeY", ref mAbilityRecoveryBarSize.Y, PhxPredicates.IsNotOne);
				s.StreamAttributeOpt("offsetX", ref mAbilityRecoveryBarOffset.X, Predicates.IsNotZero);
				s.StreamAttributeOpt("offsetY", ref mAbilityRecoveryBarOffset.Y, Predicates.IsNotZero);
			}
			#endregion
			xs.StreamHPBarName(s, "BobbleHead", ref mBobbleHeadID, HPBarDataObjectKind.BobbleHead);
			xs.StreamHPBarName(s, "BuildingStrengthDisplay", ref mBuildingStrengthDisplayID, HPBarDataObjectKind.BuildingStrength);
			s.StreamElementOpt("CryoPoints", ref mCryoPoints, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("DazeResist", ref mDazeResist, PhxPredicates.IsNotOne);
			#region Birth
			using (var bm = s.EnterCursorBookmarkOpt("Birth", this, v => v.HasBirthData)) if (bm.IsNotNull)
			{
				s.StreamCursorEnum(ref mBirthType);
				s.StreamAttributeOpt("spawnPoint", ref mBirthBone, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("endPoint", ref mBirthEndBone, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim0", ref mBirthAnim0, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim1", ref mBirthAnim1, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim2", ref mBirthAnim2, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("anim3", ref mBirthAnim3, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("trainerAnim", ref mBirthTrainerAnim, Predicates.IsNotNullOrEmpty);
			}
			#endregion
			XML.XmlUtil.Serialize(s, Units, BProtoSquadUnit.kBListXmlParams);
			s.StreamElementOpt("SubSelectSort", ref mSubSelectSort, v => v != int.MaxValue);
			#region TurnRadius
			using (var bm = s.EnterCursorBookmarkOpt("TurnRadius", this, v => v.HasTurnRadiusData)) if (bm.IsNotNull)
			{
				s.StreamAttributeOpt("min", ref mTurnRadiusMin, PhxPredicates.IsNotInvalid);
				s.StreamAttributeOpt("max", ref mTurnRadiusMax, PhxPredicates.IsNotInvalid);
			}
			#endregion
			s.StreamElementOpt("LeashDistance", ref mLeashDistance, Predicates.IsNotZero);
			s.StreamElementOpt("LeashDeadzone", ref mLeashDeadzone, v => v != cDefaultLeashDeadzone);
			s.StreamElementOpt("LeashRecallDelay", ref mLeashRecallDelay, v => v != cDefaultLeashRecallDelay);
			s.StreamElementOpt("AggroDistance", ref mAggroDistance, Predicates.IsNotZero);
			s.StreamElementOpt("MinimapScale", ref mMinimapScale, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, Flags, XML.BBitSetXmlParams.kFlagsSansRoot);
			s.StreamElementOpt("Level", ref mLevel, Predicates.IsNotZero);
			s.StreamElementOpt("TechLevel", ref mTechLevel, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, Sounds, BProtoSquadSound.kBListXmlParams);
			xs.StreamDBID(s, "RecoveringEffect", ref mRecoveringEffectID, DatabaseObjectKind.Object);
			s.StreamElementOpt("CanAttackWhileMoving", ref mCanAttackWhileMoving, Predicates.IsFalse);
		}
		#endregion
	};
}