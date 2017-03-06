
namespace KSoft.Phoenix.Phx
{
	public enum BProtoTechAlphaMode
	{
		None = -1,
		Excluded = 0,
		AlphaOnly = 1,
	};

	public sealed class BProtoTech
		: DatabaseIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Tech")
		{
			RootName = "TechTree",
			DataName = "name",
			Flags = //XML.BCollectionXmlParamsFlags.ToLowerDataNames |
				XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading |
				XML.BCollectionXmlParamsFlags.SupportsUpdating
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Game,
			Directory = Engine.GameDirectory.Data,
			FileName = "Techs.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfoUpdate = new Engine.XmlFileInfo
		{
			Location = Engine.ContentStorage.Update,
			Directory = Engine.GameDirectory.Data,
			FileName = "Techs_Update.xml",
			RootName = kBListXmlParams.RootName
		};

		static readonly Collections.CodeEnum<BProtoTechFlags> kFlagsProtoEnum = new Collections.CodeEnum<BProtoTechFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);
		#endregion

		#region Alpha
		BProtoTechAlphaMode mAlphaMode = BProtoTechAlphaMode.None;
		public BProtoTechAlphaMode AlphaMode
		{
			get { return mAlphaMode; }
			set { mAlphaMode = value; }
		}
		#endregion

		public Collections.BBitSet Flags { get; private set; }

		#region Icon
		string mIcon;
		[Meta.TextureReference]
		public string Icon
		{
			get { return mIcon; }
			set { mIcon = value; }
		}
		#endregion

		#region ResearchCompleteSound
		string mResearchCompleteSound;
		[Meta.SoundCueReference]
		public string ResearchCompleteSound
		{
			get { return mResearchCompleteSound; }
			set { mResearchCompleteSound = value; }
		}
		#endregion

		#region ResearchAnim
		string mResearchAnim;
		[Meta.BAnimTypeReference]
		public string ResearchAnim
		{
			get { return mResearchAnim; }
			set { mResearchAnim = value; }
		}
		#endregion

		public BProtoTechPrereqs Prereqs { get; private set; }
		public Collections.BListArray<BProtoTechEffect> Effects { get; private set; }

		#region StatsObjectID
		int mStatsObjectID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int StatsObjectID
		{
			get { return mStatsObjectID; }
			set { mStatsObjectID = value; }
		}
		#endregion

		public bool HasPrereqs { get { return Prereqs != null && Prereqs.IsNotEmpty; } }

		public BProtoTech() : base(BResource.kBListTypeValuesParams, BResource.kBListTypeValuesXmlParams_CostLowercaseType)
		{
			Flags = new Collections.BBitSet(kFlagsParams);
			Prereqs = new BProtoTechPrereqs();
			Effects = new Collections.BListArray<BProtoTechEffect>();
		}

		#region IXmlElementStreamable Members
		protected override void StreamDbId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			// This isn't always used, nor unique.
			// In fact, the engine doesn't even use it beyond reading it!
			s.StreamElementOpt("DBID", ref mDbId, Predicates.IsNotNone);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			int alpha = (int)mAlphaMode;
			s.StreamAttributeOpt("Alpha", ref alpha, Predicates.IsNotNone);
			mAlphaMode = (BProtoTechAlphaMode)alpha;

			XML.XmlUtil.Serialize(s, Flags, XML.BBitSetXmlParams.kFlagsSansRoot);

			if (s.IsReading)
			{
				using (var bm = s.EnterCursorBookmarkOpt("Status")) if (bm.IsNotNull)
				{
					string statusValue = null;
					s.ReadCursor(ref statusValue);
					if (string.Equals(statusValue, "Unobtainable", System.StringComparison.OrdinalIgnoreCase))
						Flags.Set((int)BProtoTechFlags.Unobtainable);
				}
			}

			s.StreamStringOpt("Icon", ref mIcon, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("ResearchCompleteSound", ref mIcon, toLower: false, type: XML.XmlUtil.kSourceElement);
			s.StreamStringOpt("ResearchAnim", ref mResearchAnim, toLower: false, type: XML.XmlUtil.kSourceElement);
			using (var bm = s.EnterCursorBookmarkOpt("Prereqs", this, v => v.HasPrereqs)) if (bm.IsNotNull)
			{
				Prereqs.Serialize(s);
			}
			XML.XmlUtil.Serialize(s, Effects, BProtoTechEffect.kBListXmlParams);
			xs.StreamDBID(s, "StatsObject", ref mStatsObjectID, DatabaseObjectKind.Object);
		}
		#endregion
	};
}