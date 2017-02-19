
namespace KSoft.Phoenix.Phx
{
	public sealed class BDamageType
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("DamageType",
			XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading);
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "DamageTypes.xml",
			RootName = kBListXmlParams.RootName
		};
		#endregion

		bool mAttackRating;
		public bool AttackRating { get { return mAttackRating; } }

		bool mBaseType;
		public bool BaseType { get { return mBaseType; } }

		bool mShielded;
		public bool Shielded { get { return mShielded; } }

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("AttackRating", ref mAttackRating, Predicates.IsTrue);
			s.StreamAttributeOpt("BaseType", ref mBaseType, Predicates.IsTrue);
			s.StreamAttributeOpt("Shielded", ref mShielded, Predicates.IsTrue);
		}
		#endregion
	};
}