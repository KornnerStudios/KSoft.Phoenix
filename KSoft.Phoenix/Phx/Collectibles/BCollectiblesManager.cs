
namespace KSoft.Phoenix.Phx
{
	public sealed class BCollectiblesManager
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string kXmlRootName = "CollectiblesDefinitions";

		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Skulls.xml",
			RootName = kXmlRootName
		};

		const string kXmlElementXMLVersion = "CollectiblesXMLVersion";
		#endregion

		int mXmlVersion = TypeExtensions.kNone;
		public BCollectiblesSkullManager SkullManager { get; private set; }

		public BCollectiblesManager()
		{
			SkullManager = new BCollectiblesSkullManager();
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamElementOpt(kXmlElementXMLVersion, ref mXmlVersion, Predicates.IsNotNone);
			SkullManager.Serialize(s);
		}
		#endregion
	};
}