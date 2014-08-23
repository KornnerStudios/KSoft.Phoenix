
namespace KSoft.Phoenix.Phx
{
	public sealed class BCollectibleSkullEffect
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Effect",
		};

		const string kXmlAttrValue = "value";
		const string kXmlAttrTarget = "target";
		#endregion

		BCollectibleSkullEffectType mType = BCollectibleSkullEffectType.Invalid;
		BCollectibleSkullTarget mTarget = BCollectibleSkullTarget.None;
		float mValue = PhxUtil.kInvalidSingle;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursorEnum(ref mType);
			s.StreamAttributeEnumOpt(kXmlAttrTarget, ref mTarget, e => e != BCollectibleSkullTarget.None);
			s.StreamAttributeOpt    (kXmlAttrValue, ref mValue, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}