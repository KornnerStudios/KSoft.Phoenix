
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoBuildingStrength
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "BuildingStrength",
			DataName = "name",
		};
		#endregion

		public BProtoBuildingStrength()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}