
namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerProtoCondition
		: TriggerSystemProtoObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Condition")
		{
			DataName = DatabaseNamedObject.kXmlAttrNameN,
			Flags = 0,
		};

		const string kXmlAttrAsync = "Async";
		const string kXmlAttrAsyncParameterKey = "AsyncParameterKey"; // really a sbyte
		#endregion

		bool mAsync;
		public bool Async { get { return mAsync; } }

		int mAsyncParameterKey;
		public int AsyncParameterKey { get { return mAsyncParameterKey; } }

		public BTriggerProtoCondition() { }
		public BTriggerProtoCondition(BTriggerSystem root, BTriggerCondition instance) : base(root, instance)
		{
			mAsync = instance.Async;
			mAsyncParameterKey = instance.AsyncParameterKey;
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			if(s.StreamAttributeOpt(kXmlAttrAsync, ref mAsync, Predicates.IsTrue))
				s.StreamAttribute(kXmlAttrAsyncParameterKey, ref mAsyncParameterKey);
		}
	};
}