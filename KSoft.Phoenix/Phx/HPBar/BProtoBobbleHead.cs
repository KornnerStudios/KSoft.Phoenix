
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoBobbleHead
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "BobbleHead",
			DataName = "name",
		};
		#endregion

		public BProtoBobbleHead()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}