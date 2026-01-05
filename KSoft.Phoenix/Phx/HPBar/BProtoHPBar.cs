
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoHPBar
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "HPBar",
			DataName = "name",
		};
		#endregion

		public BProtoHPBar()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}