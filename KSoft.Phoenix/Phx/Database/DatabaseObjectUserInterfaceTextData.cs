using System;

namespace KSoft.Phoenix.Phx
{
	// #TODO BGameMode also needs this

	/// <summary>
	/// UX string references common to many objects. Up to implementing objects to declare
	/// which strings they use.
	/// </summary>
	public sealed class DatabaseObjectUserInterfaceTextData
		: IO.ITagElementStringNameStreamable
	{
		#region Flags
		private enum Flags
		{
			HasNameID,
			HasDisplayNameID,
			HasDisplayName2ID, // BAbility only
			HasDescriptionID,
			HasLongDescriptionID,
			HasPrereqTextID,
			HasStatsNameID,
			HasRoleTextID,
			HasRolloverTextID,

			// ProtoObject specific:
			HasEnemyRolloverTextID,
			HasGaiaRolloverTextID,

			// ProtoPower specific:
			HasChooseTextID,

			kNumberOf
		};
		private Collections.BitVector32 mFlags;

		static void ThrowIfTextFlagUnset(bool hasFlag, string propertyName)
		{
			if (!hasFlag)
			{
				throw new InvalidOperationException(string.Format(
					"{0} cannot be set because its matching Has* flag is false.",
					propertyName));
			}
		}

		public bool HasNameID
		{
			get { return mFlags.Test(Flags.HasNameID); }
			set { mFlags.Set(Flags.HasNameID, value); }
		}

		public bool HasDisplayNameID
		{
			get { return mFlags.Test(Flags.HasDisplayNameID); }
			set { mFlags.Set(Flags.HasDisplayNameID, value); }
		}

		public bool HasDisplayName2ID
		{
			get { return mFlags.Test(Flags.HasDisplayName2ID); }
			set { mFlags.Set(Flags.HasDisplayName2ID, value); }
		}

		public bool HasDescriptionID
		{
			get { return mFlags.Test(Flags.HasDescriptionID); }
			set { mFlags.Set(Flags.HasDescriptionID, value); }
		}

		public bool HasLongDescriptionID
		{
			get { return mFlags.Test(Flags.HasLongDescriptionID); }
			set { mFlags.Set(Flags.HasLongDescriptionID, value); }
		}

		public bool HasPrereqTextID
		{
			get { return mFlags.Test(Flags.HasPrereqTextID); }
			set { mFlags.Set(Flags.HasPrereqTextID, value); }
		}

		public bool HasStatsNameID
		{
			get { return mFlags.Test(Flags.HasStatsNameID); }
			set { mFlags.Set(Flags.HasStatsNameID, value); }
		}

		public bool HasRoleTextID
		{
			get { return mFlags.Test(Flags.HasRoleTextID); }
			set { mFlags.Set(Flags.HasRoleTextID, value); }
		}

		public bool HasRolloverTextID
		{
			get { return mFlags.Test(Flags.HasRolloverTextID); }
			set { mFlags.Set(Flags.HasRolloverTextID, value); }
		}

		public bool HasEnemyRolloverTextID
		{
			get { return mFlags.Test(Flags.HasEnemyRolloverTextID); }
			set { mFlags.Set(Flags.HasEnemyRolloverTextID, value); }
		}

		public bool HasGaiaRolloverTextID
		{
			get { return mFlags.Test(Flags.HasGaiaRolloverTextID); }
			set {
				mFlags.Set(Flags.HasGaiaRolloverTextID, value);

				if (value)
				{
					GaiaRolloverText = new Collections.BListArray<GaiaRolloverTextData>();
				}
				else
				{
					GaiaRolloverText = null;
				}
			}
		}

		public bool HasChooseTextID
		{
			get { return mFlags.Test(Flags.HasChooseTextID); }
			set { mFlags.Set(Flags.HasChooseTextID, value); }
		}
		#endregion

		#region NameID
		int mNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int NameID
		{
			get {
				if (!HasNameID)
				{
					return TypeExtensions.kNone;
				}

				return mNameID;
			}
			set {
				ThrowIfTextFlagUnset(HasNameID, nameof(NameID));
				mNameID = value;
			}
		}
		#endregion

		#region DisplayNameID
		int mDisplayNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int DisplayNameID
		{
			get {
				if (!HasDisplayNameID)
				{
					return TypeExtensions.kNone;
				}

				return mDisplayNameID;
			}
			set {
				ThrowIfTextFlagUnset(HasDisplayNameID, nameof(DisplayNameID));
				mDisplayNameID = value;
			}
		}
		#endregion

		#region DisplayName2ID
		int mDisplayName2ID = TypeExtensions.kNone;
		[Meta.UnusedData("unused in code")]
		[Meta.LocStringReference]
		public int DisplayName2ID
		{
			get {
				if (!HasDisplayName2ID)
				{
					return TypeExtensions.kNone;
				}

				return mDisplayName2ID;
			}
			set {
				ThrowIfTextFlagUnset(HasDisplayName2ID, nameof(DisplayName2ID));
				mDisplayName2ID = value;
			}
		}
		#endregion

		#region DescriptionID
		int mDescriptionID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int DescriptionID
		{
			get {
				if (!HasDescriptionID)
				{
					return TypeExtensions.kNone;
				}

				return mDescriptionID;
			}
			set {
				ThrowIfTextFlagUnset(HasDescriptionID, nameof(DescriptionID));
				mDescriptionID = value;
			}
		}
		#endregion

		#region LongDescriptionID
		int mLongDescriptionID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int LongDescriptionID
		{
			get {
				if (!HasLongDescriptionID)
				{
					return TypeExtensions.kNone;
				}

				return mLongDescriptionID;
			}
			set {
				ThrowIfTextFlagUnset(HasLongDescriptionID, nameof(LongDescriptionID));
				mLongDescriptionID = value;
			}
		}
		#endregion

		#region PrereqTextID
		int mPrereqTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int PrereqTextID
		{
			get {
				if (!HasPrereqTextID)
				{
					return TypeExtensions.kNone;
				}

				return mPrereqTextID;
			}
			set {
				ThrowIfTextFlagUnset(HasPrereqTextID, nameof(PrereqTextID));
				mPrereqTextID = value;
			}
		}
		#endregion

		#region StatsNameID
		int mStatsNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int StatsNameID
		{
			get {
				if (!HasStatsNameID)
				{
					return TypeExtensions.kNone;
				}

				return mStatsNameID;
			}
			set {
				ThrowIfTextFlagUnset(HasStatsNameID, nameof(StatsNameID));
				mStatsNameID = value;
			}
		}
		#endregion

		#region RoleTextID
		int mRoleTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int RoleTextID
		{
			get {
				if (!HasRoleTextID)
				{
					return TypeExtensions.kNone;
				}

				return mRoleTextID;
			}
			set {
				ThrowIfTextFlagUnset(HasRoleTextID, nameof(RoleTextID));
				mRoleTextID = value;
			}
		}
		#endregion

		#region RolloverTextID
		int mRolloverTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int RolloverTextID
		{
			get {
				if (!HasRolloverTextID)
				{
					return TypeExtensions.kNone;
				}

				return mRolloverTextID;
			}
			set {
				ThrowIfTextFlagUnset(HasRolloverTextID, nameof(RolloverTextID));
				mRolloverTextID = value;
			}
		}
		#endregion

		#region EnemyRolloverTextID
		int mEnemyRolloverTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int EnemyRolloverTextID
		{
			get {
				if (!HasEnemyRolloverTextID)
				{
					return TypeExtensions.kNone;
				}

				return mEnemyRolloverTextID;
			}
			set {
				ThrowIfTextFlagUnset(HasEnemyRolloverTextID, nameof(EnemyRolloverTextID));
				mEnemyRolloverTextID = value;
			}
		}
		#endregion

		public Collections.BListArray<GaiaRolloverTextData>? GaiaRolloverText { get; private set; }

		#region ChooseTextID
		int mChooseTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int ChooseTextID
		{
			get {
				if (!HasChooseTextID)
				{
					return TypeExtensions.kNone;
				}

				return mChooseTextID;
			}
			set {
				ThrowIfTextFlagUnset(HasChooseTextID, nameof(ChooseTextID));
				mChooseTextID = value;
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			if (HasNameID)
			{
				xs.StreamStringID(s, "NameID", ref mNameID);
			}

			if (HasDisplayNameID)
			{
				xs.StreamStringID(s, "DisplayNameID", ref mDisplayNameID);
			}

			if (HasDisplayName2ID)
			{
				xs.StreamStringID(s, "DisplayName2ID", ref mDisplayName2ID);
			}

			if (HasDescriptionID)
			{
				xs.StreamStringID(s, "DescriptionID", ref mDescriptionID);
			}

			if (HasLongDescriptionID)
			{
				xs.StreamStringID(s, "LongDescriptionID", ref mLongDescriptionID);
			}

			if (HasPrereqTextID)
			{
				xs.StreamStringID(s, "PrereqTextID", ref mPrereqTextID);
			}

			if (HasRoleTextID)
			{
				xs.StreamStringID(s, "RoleTextID", ref mRoleTextID);
			}

			if (HasRolloverTextID)
			{
				xs.StreamStringID(s, "RolloverTextID", ref mRolloverTextID);
			}

			if (HasEnemyRolloverTextID)
			{
				xs.StreamStringID(s, "EnemyRolloverTextID", ref mEnemyRolloverTextID);
			}

			if (HasGaiaRolloverTextID)
			{
				var gaia_rollover_text = GaiaRolloverText
					?? throw new InvalidOperationException("Gaia rollover text must be initialized when its flag is set.");
				XML.XmlUtil.Serialize(s, gaia_rollover_text, GaiaRolloverTextData.kBListXmlParams);
			}

			if (HasStatsNameID)
			{
				xs.StreamStringID(s, "StatsNameID", ref mStatsNameID);
			}

			if (HasChooseTextID)
			{
				xs.StreamStringID(s, "ChooseTextID", ref mChooseTextID);
			}
		}
		#endregion
	};

	public sealed class GaiaRolloverTextData
		: IO.ITagElementStringNameStreamable
		, IComparable<GaiaRolloverTextData>
		, IEquatable<GaiaRolloverTextData>
	{
		/// <summary>HW1 is hard coded to only support 4 (for the first four Civs)</summary>
		public const int cMaxGaiaRolloverTextIndices = 4;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "GaiaRolloverTextID",
		};
		#endregion

		#region CivID
		int mCivID = TypeExtensions.kNone;
		/// <summary>No CivID means it applies to all Civs</summary>
		[Meta.BCivReference]
		public int CivID
		{
			get { return mCivID; }
			set { mCivID = value; }
		}
		#endregion

		#region TextID
		int mTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int TextID
		{
			get { return mTextID; }
			set { mTextID = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "civ", ref mCivID, DatabaseObjectKind.Civ, true, XML.XmlUtil.kSourceAttr);
			s.StreamCursor(ref mTextID);
			if (s.IsReading && mTextID.IsNotNone())
			{
				xs.Database.AddStringIDReference(mTextID);
			}
		}
		#endregion

		#region IComparable Members
		public int CompareTo(GaiaRolloverTextData? other)
		{
			if (other is null)
			{
				return 1;
			}

			if (CivID != other.CivID)
			{
				CivID.CompareTo(other.CivID);
			}

			return TextID.CompareTo(other.TextID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(GaiaRolloverTextData? other)
		{
			return other is not null
				&& CivID == other.CivID
				&& TextID == other.TextID;
		}

		public override bool Equals(object? obj) => Equals(obj as GaiaRolloverTextData);

		public override int GetHashCode()
			=> HashCode.Combine(CivID, TextID);
		#endregion
	};
}