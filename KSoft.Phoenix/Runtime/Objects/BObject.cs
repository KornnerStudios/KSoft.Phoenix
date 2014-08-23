using Contract = System.Diagnostics.Contracts.Contract;

using BVector = SlimMath.Vector4;
using BBitVector32 = System.UInt32;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			Object1 = 0x2710
			;
	};

	public sealed class BObject
		: BEntity
	{
		const int cMaximumObjectAttachments = 0xFA;
		const int cMaximumAdditionalTextures = 0xFA;
		const int cMaximumHardpoints = 0x64;

		public BVector CenterOffset, IconColorSize;
		public byte[] UVOffsets = new byte[0x18];
		public uint MultiframeTextureIndex;
		public int VisualVariationIndex;
		//BVisual Visual
		public float AnimationRate, Radius, MoveAnimationPosition, HighlightIntensity;
		public uint SubUpdateNumber;
		public BBitVector32 PlayerVisibility, DoppleBits;
		public int SimX, SimZ;
		public float LOSScalar;
		public int LastSimLOS;
		public BObjectAttachments[] ObjectAttachments;
		public BAdditionalTextures[] AdditionalTextures;
		//BHardpointState[] HardpointState

		public bool HasObjectAttachments { get { return ObjectAttachments != null; } }
		public bool HasAdditionalTextures { get { return AdditionalTextures != null; } }
		//public bool HasHardpointState { get { return HardpointState != null; } }

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref CenterOffset); s.StreamV(ref IconColorSize);
			s.Stream(UVOffsets);
			s.Stream(ref MultiframeTextureIndex);
			s.Stream(ref VisualVariationIndex);
			Contract.Assert(false);//BVisualManager::BVisual here
			s.Stream(ref AnimationRate); s.Stream(ref Radius); s.Stream(ref MoveAnimationPosition); s.Stream(ref HighlightIntensity);
			s.Stream(ref SubUpdateNumber);
			s.Stream(ref PlayerVisibility); s.Stream(ref DoppleBits);
			s.Stream(ref SimX); s.Stream(ref SimZ);
			s.Stream(ref LOSScalar);
			s.Stream(ref LastSimLOS);

			bool has_attachments = s.IsReading ? false : HasObjectAttachments;
			s.Stream(ref has_attachments);
			if (has_attachments)
				BSaveGame.StreamArray(s, ref ObjectAttachments, cMaximumObjectAttachments);

			bool has_additional_textures = s.IsReading ? false : HasAdditionalTextures;
			s.Stream(ref has_additional_textures);
			if (has_additional_textures)
				BSaveGame.StreamArray(s, ref AdditionalTextures, cMaximumAdditionalTextures);

			Contract.Assert(false);// HardpointState
		}
		#endregion
	};
}