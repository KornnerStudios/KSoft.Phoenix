
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectCaptureCost
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "CaptureCost",
		};
		#endregion

		#region CivID
		int mCivID = TypeExtensions.kNone;
		[Meta.BCivReference]
		public int CivID
		{
			get { return mCivID; }
			set { mCivID = value; }
		}
		#endregion

		#region ResourceType
		int mResourceType = TypeExtensions.kNone;
		[Meta.ResourceReference]
		public int ResourceType
		{
			get { return mResourceType; }
			set { mResourceType = value; }
		}
		#endregion

		#region Cost
		float mCost;
		public float Cost
		{
			get { return mCost; }
			set { mCost = value; }
		}
		#endregion

		public bool AppliesToAllCivs => CivID.IsNone();
		/// <summary>Does the engine not ignore the XML data of this bit?</summary>
		public bool IsNotIgnored
			=> ResourceType >= 0
				&& Cost != 0.0f;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "Civ", ref mCivID, DatabaseObjectKind.Civ, xmlSource: XML.XmlUtil.kSourceAttr);

			var costDataName = BResource.kBListTypeValuesXmlParams_Cost.DataName
				?? throw new System.InvalidOperationException("Cost data name is required.");

			if (!xs.StreamTypeName(s, costDataName, ref mResourceType, GameDataObjectKind.Cost, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr))
			{
				s.ThrowReadException(new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"ProtoObject's {kBListXmlParams.ElementName} XML doesn't define a {BResource.kBListTypeValuesXmlParams_Cost.DataName}")));
			}

			s.StreamCursor(ref mCost);
		}
		#endregion
	};
}