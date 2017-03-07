
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoImpactEffect
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("TerrainEffect")
		{
			DataName = "name",
			Flags = XML.BCollectionXmlParamsFlags.ToLowerDataNames
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "ImpactEffects.xml",
			RootName = "ImpactEffects"//kBListXmlParams.RootName
		};
		#endregion

		#region Limit
		int mLimit = 2;
		public int Limit
		{
			get { return mLimit; }
			set { mLimit = value; }
		}
		#endregion

		#region Lifespan
		float mLifespan = 3.0f;
		public float Lifespan
		{
			get { return mLifespan; }
			set { mLifespan = value; }
		}
		#endregion

		#region FileName
		string mFileName;
		public string FileName
		{
			get { return mFileName; }
			set { mFileName = value; }
		}
		#endregion

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("limit", ref mLimit, x => x != 2);
			s.StreamAttributeOpt("lifespan", ref mLifespan, x => x != 3.0f);
			s.StreamCursor(ref mFileName);
		}
		#endregion
	};
}