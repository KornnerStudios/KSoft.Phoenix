
namespace KSoft.Phoenix.Phx
{
	public sealed class BCollectiblesSkullManager
		: IO.ITagElementStringNameStreamable
	{
		public Collections.BListAutoId<BProtoSkull> Skulls { get; private set; }

		// bool RocketAllGrunts, MinimapHidden
		// int BonusSquadLevels, DeathExplodeObjectType, DeathExplodeProtoObject
		// float DeathExplodeChance

		public BCollectiblesSkullManager()
		{
			Skulls = new Collections.BListAutoId<BProtoSkull>();
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.Serialize(s, Skulls, BProtoSkull.kBListXmlParams);
			//TimeLineEvent BProtoTimeLineEvent
		}
		#endregion
	};
}