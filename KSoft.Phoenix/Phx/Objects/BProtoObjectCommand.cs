using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectCommand
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		const string kXmlAttrPosition = "Position";
		const string kXmlAttrType = "Type";
		#endregion

		#region Properties
		int mPosition;
		public int Position { get { return mPosition; } }

		BProtoObjectCommandType mCommandType = BProtoObjectCommandType.Invalid;
		public BProtoObjectCommandType CommandType { get { return mCommandType; } }

		int mID = TypeExtensions.kNone;
		public int ID { get { return mID; } }

		BSquadMode mSquadMode = BSquadMode.Invalid;
		public BSquadMode SquadMode { get { return mSquadMode; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute(kXmlAttrPosition, ref mPosition);

			s.StreamAttributeEnum(kXmlAttrType, ref mCommandType);
			switch (mCommandType)
			{
			case BProtoObjectCommandType.Research: // proto tech
				xs.StreamDBID(s, null, ref mID, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.TrainUnit: // proto object
			case BProtoObjectCommandType.Build:
			case BProtoObjectCommandType.BuildOther:
				xs.StreamDBID(s, null, ref mID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.TrainSquad: // proto squad
				xs.StreamDBID(s, null, ref mID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
				break;

			case BProtoObjectCommandType.ChangeMode: // unused
				s.StreamCursorEnum(ref mSquadMode);
				break;

			case BProtoObjectCommandType.Ability:
				xs.StreamDBID(s, null, ref mID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceCursor);
				break;
			case BProtoObjectCommandType.Power:
				xs.StreamDBID(s, null, ref mID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceCursor);
				break;
			}
		}
		#endregion
	};
}