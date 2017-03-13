
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoVeterancyBar
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "VeterancyBar",
			DataName = "name",
		};
		#endregion

		public BProtoVeterancyBar()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}