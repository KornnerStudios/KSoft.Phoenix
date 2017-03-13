
using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectChildObject
		: IO.ITagElementStringNameStreamable
	{
		public enum ChildObjectType
		{
			Object,
			ParkingLot,
			Socket,
			Rally,
			OneTimeSpawnSquad,
			Unit,
			Foundation,

			kNumberOf
		};

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			RootName = "ChildObjects",
			ElementName = "Object",
		};
		#endregion

		#region Type
		ChildObjectType mType = ChildObjectType.Object;
		public ChildObjectType Type
		{
			get { return mType; }
			set { mType = value; }
		}
		#endregion

		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return mID; }
			set { mID = value; }
		}
		#endregion

		#region AttachBone
		string mAttachBone;
		public string AttachBone
		{
			get { return mAttachBone; }
			set { mAttachBone = value; }
		}
		#endregion

		#region Offset
		BVector mOffset;
		public BVector Offset
		{
			get { return mOffset; }
			set { mOffset = value; }
		}
		#endregion

		#region Rotation
		float mRotation;
		public float Rotation
		{
			get { return mRotation; }
			set { mRotation = value; }
		}
		#endregion

		#region UserCivID
		int mUserCivID = TypeExtensions.kNone;
		[Meta.BCivReference]
		public int UserCivID
		{
			get { return mUserCivID; }
			set { mUserCivID = value; }
		}
		#endregion

		public DatabaseObjectKind TypeObjectKind { get {
			if (Type == ChildObjectType.OneTimeSpawnSquad)
				return DatabaseObjectKind.Squad;

			return DatabaseObjectKind.Object;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnumOpt("Type", ref mType, e => e != ChildObjectType.Object);

			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mID, TypeObjectKind, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
			s.StreamAttributeOpt("AttachBone", ref mAttachBone, Predicates.IsNotNullOrEmpty);
			s.StreamBVector("Offset", ref mOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamAttributeOpt("Rotation", ref mRotation, Predicates.IsNotZero);
			xs.StreamDBID(s, "UserCiv", ref mUserCivID, DatabaseObjectKind.Civ, xmlSource: XML.XmlUtil.kSourceAttr);
		}
		#endregion
	};
}