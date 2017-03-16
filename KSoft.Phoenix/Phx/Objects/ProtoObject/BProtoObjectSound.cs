
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectSound
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Sound",
		};
		#endregion

		#region Sound
		string mSound;
		public string Sound
		{
			get { return mSound; }
			set { mSound = value; }
		}
		#endregion

		#region Type
		BObjectSoundType mType = BObjectSoundType.None;
		public BObjectSoundType Type
		{
			get { return mType; }
			set { mType = value; }
		}
		#endregion

		#region SquadID
		int mSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int SquadID
		{
			get { return mSquadID; }
			set { mSquadID = value; }
		}
		#endregion

		#region Action
		string mAction;
		[Meta.BProtoActionReference]
		public string Action
		{
			get { return mAction; }
			set { mAction = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamCursor(ref mSound);

			if (s.StreamAttributeEnumOpt("Type", ref mType, e => e != BObjectSoundType.None))
			{
				xs.StreamDBID(s, "Squad", ref mSquadID, DatabaseObjectKind.Squad, xmlSource: XML.XmlUtil.kSourceAttr);
				s.StreamAttributeOpt("Action", ref mAction, Predicates.IsNotNullOrEmpty);
			}
		}
		#endregion
	};
}