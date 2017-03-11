
using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTerrainImpactDecalHandle
		: IO.ITagElementStringNameStreamable
	{
		public enum OrientationType
		{
			Random,
			Forward,
		};

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "ImpactDecal",
		};
		#endregion

		#region Size
		BVector mSize = new BVector(2.0f, 0, 2.0f, 0);
		public BVector Size
		{
			get { return mSize; }
			set { mSize = value; }
		}
		#endregion

		#region TimeFullyOpaque
		float mTimeFullyOpaque = 5.0f;
		public float TimeFullyOpaque
		{
			get { return mTimeFullyOpaque; }
			set { mTimeFullyOpaque = value; }
		}
		#endregion

		#region FadeOutTime
		float mFadeOutTime = 10.0f;
		public float FadeOutTime
		{
			get { return mFadeOutTime; }
			set { mFadeOutTime = value; }
		}
		#endregion

		#region Orientation
		OrientationType mOrientation = OrientationType.Random;
		public OrientationType Orientation
		{
			get { return mOrientation; }
			set { mOrientation = value; }
		}
		#endregion

		#region TextureName
		string mTextureName;
		[Meta.TextureReference]
		public string TextureName
		{
			get { return mTextureName; }
			set { mTextureName = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("sizeX", ref mSize.X, f => f != 2.0f);
			s.StreamAttributeOpt("sizeZ", ref mSize.Z, f => f != 2.0f);
			s.StreamAttributeOpt("timeFullyOpaque", ref mTimeFullyOpaque, f => f != 5.0f);
			s.StreamAttributeOpt("fadeOutTime", ref mFadeOutTime, f => f != 10.0f);
			s.StreamAttributeEnumOpt("sizeX", ref mOrientation, e => e != OrientationType.Random);
			s.StreamCursor(ref mTextureName);
		}
		#endregion
	};
}