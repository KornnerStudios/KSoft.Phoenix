
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerVar
		: TriggerScriptIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("TriggerVar")
		{
			DataName = DatabaseNamedObject.kXmlAttrNameN,
		};

		const string kXmlAttrType = "Type";
		const string kXmlAttrIsNull = "IsNull";
		#endregion

		BTriggerVarType mType = BTriggerVarType.None;
		public BTriggerVarType Type { get { return mType; } }

		bool mIsNull;
		public bool IsNull { get { return mIsNull; } }

		//string mValue;

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttributeEnum(kXmlAttrType, ref mType);
			s.StreamAttribute    (kXmlAttrIsNull, ref mIsNull);
			//s.StreamCursor(ref mValue);
		}
	};
}