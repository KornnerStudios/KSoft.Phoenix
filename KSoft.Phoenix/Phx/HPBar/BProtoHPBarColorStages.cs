
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoHPBarColorStages
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "ColorStages",
			DataName = "name",
		};
		#endregion

		public BProtoHPBarColorStages()
		{
		}

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
		}
		#endregion
	};
}