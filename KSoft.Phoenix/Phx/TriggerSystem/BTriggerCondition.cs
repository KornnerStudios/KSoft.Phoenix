
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerCondition
		: TriggerScriptObjectWithArgs
	{
		#region Xml constants
		public const string kXmlRootName = "TriggerConditions";

		public static readonly XML.BListXmlParams kBListXmlParams_And = new XML.BListXmlParams
		{
			RootName = "And",
			ElementName = "Condition",
			DataName = kXmlAttrType,
		};
		public static readonly XML.BListXmlParams kBListXmlParams_Or = new XML.BListXmlParams
		{
			RootName = "Or",
			ElementName = "Condition",
			DataName = kXmlAttrType,
		};

		const string kXmlAttrInvert = "Invert";
		const string kXmlAttrAsync = "Async"; // engine treats this as optional, but not the key
		const string kXmlAttrAsyncParameterKey = "AsyncParameterKey"; // really a sbyte
		#endregion

		bool mInvert;

		bool mAsync;
		public bool Async { get { return mAsync; } }

		int mAsyncParameterKey; // References a Parameter (via SigID). Runtime then takes that parameter's BTriggerVarID
		public int AsyncParameterKey { get { return mAsyncParameterKey; } }

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(kXmlAttrInvert, ref mInvert);
			s.StreamAttribute(kXmlAttrAsync, ref mAsync);
			s.StreamAttribute(kXmlAttrAsyncParameterKey, ref mAsyncParameterKey);
		}
	};
}