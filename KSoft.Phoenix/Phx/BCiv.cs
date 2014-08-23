
namespace KSoft.Phoenix.Phx
{
	public sealed class BCiv
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Civ")
		{
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Civs.xml",
			RootName = kBListXmlParams.RootName
		};

		const string kXmlElementTech = "CivTech";

		const string kXmlElementPowerFromHero = "PowerFromHero";
		#endregion

		int mTechID = TypeExtensions.kNone;
		public int TechID { get { return mTechID; } }

		bool mPowerFromHero;
		public bool PowerFromHero { get { return mPowerFromHero; } }

		// Empty Civs just have a name
		public bool IsEmpty { get { return mTechID.IsNotNone(); } }

		public BCiv()
		{
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, kXmlElementTech, ref mTechID, DatabaseObjectKind.Tech);
			s.StreamElementOpt(kXmlElementPowerFromHero, ref mPowerFromHero, Predicates.IsTrue);
		}
		#endregion
	};
}