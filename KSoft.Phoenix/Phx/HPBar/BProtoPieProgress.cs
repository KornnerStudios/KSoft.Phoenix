
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoPieProgress
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "PieProgress",
			DataName = "name",
		};
		#endregion

		public BProtoPieProgress()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}