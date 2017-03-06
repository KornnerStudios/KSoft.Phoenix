
namespace KSoft.Phoenix.Phx
{
	public enum ReticleAttackGrade
	{
		NoEffect,
		Weak,
		Fair,
		Good,
		Extreme,

		kNumberOf
	};

	public enum DamageDirection
	{
		Full,
		FrontHalf,
		BackHalf,
		Front,
		Back,
		Left,
		Right,

		kNumberOf
	};

	public sealed class BDamageType
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("DamageType",
			XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading);
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "DamageTypes.xml",
			RootName = kBListXmlParams.RootName
		};
		#endregion

		bool mAttackRating;
		public bool AttackRating
		{
			get { return mAttackRating; }
			set { mAttackRating = value; }
		}

		bool mBaseType;
		public bool BaseType
		{
			get { return mBaseType; }
			set { mBaseType = value; }
		}

		bool mShielded;
		/// <remarks>The last type with this set will be the shielded damage type, or anything named "Shielded" will be</remarks>
		public bool Shielded
		{
			get { return mShielded; }
			set { mShielded = value; }
		}

		#region BListAutoIdObject Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttributeOpt("AttackRating", ref mAttackRating, Predicates.IsTrue);
			s.StreamAttributeOpt("BaseType", ref mBaseType, Predicates.IsTrue);
			s.StreamAttributeOpt("Shielded", ref mShielded, Predicates.IsTrue);
		}
		#endregion
	};
}