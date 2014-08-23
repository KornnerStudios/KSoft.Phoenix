
using BVec2 = SlimMath.Vector2;

namespace KSoft.Phoenix.Runtime
{
	public struct BAdditionalTextures
		: IO.IEndianStreamSerializable
	{
		public int RenderType, Texture;
		public BVec2 TexUVOfs;
		public float TexUVScale, TexInten, TexScrollSpeed;
		// relative in gamefiles, TexTimeout - gWorld->getGametimeFloat()
		public float TexTimeoutOffset;
		public bool ModulateOffset
			, ModulateIntensity
			, ShouldBeCopied
			, TexClamp
			, TexScrollLoop
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref RenderType);
			s.Stream(ref Texture);
			s.StreamV(ref TexUVOfs);
			s.Stream(ref TexUVScale);
			s.Stream(ref TexInten);
			s.Stream(ref TexScrollSpeed);
			s.Stream(ref TexTimeoutOffset);
			s.Stream(ref ModulateOffset);
			s.Stream(ref ModulateIntensity);
			s.Stream(ref ShouldBeCopied);
			s.Stream(ref TexClamp);
			s.Stream(ref TexScrollLoop);
		}
		#endregion
	};
}