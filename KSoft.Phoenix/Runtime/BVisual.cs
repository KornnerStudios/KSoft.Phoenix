// int mType - cVisualAsset*
// int mIndex
using BVisualAsset = System.UInt64;

namespace KSoft.Phoenix.Runtime
{
	public sealed class BVisualItem
		: IO.IEndianStreamSerializable
	{
		const int kUVOffsetsSize = 0x18;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains visual-item format attachment-limit documentation.")]
		const int cMaximumAttachments = 0x64;

		public BMatrix Matrix;
		public uint SubUpdateNumber, GrannySubUpdateNumber;
		public BMatrix Matrix1, Matrix2;
		public BVector
			CombinedMinCorner, CombinedMaxCorner,
			MinCorner, MaxCorner;
		public BVisualAsset ModelAsset;
		public byte[] ModelUVOffsets = new byte[kUVOffsetsSize]; // BVisualModelUVOffsets
		public uint Flags;
		public BVisualItem[]? Attachments;

		#region IEndianStreamSerializable Members
		void StreamFlags(IO.EndianStream s)
		{
			const byte k_size_in_bytes = sizeof(uint);

			s.StreamSignature(k_size_in_bytes);
			s.Stream(ref Flags);
		}
		void StreamAttachments(IO.EndianStream /*s*/_)
		{
			System.Diagnostics.Debug.Fail("TODO");
		}
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamMatrix(s, ref Matrix);
			s.Stream(ref SubUpdateNumber);
			s.Stream(ref GrannySubUpdateNumber);
			BSaveGame.StreamMatrix(s, ref Matrix1);
			BSaveGame.StreamMatrix(s, ref Matrix2);
			s.StreamV(ref CombinedMinCorner); s.StreamV(ref CombinedMaxCorner);
			s.StreamV(ref MinCorner); s.StreamV(ref MaxCorner);
			s.Stream(ref ModelAsset);
			if (s.StreamCond(ModelUVOffsets, offsets => !offsets.EqualsZero()))
			{
				s.Stream(ModelUVOffsets);
			}

			StreamFlags(s);
			StreamAttachments(s);
		}
		#endregion
	};

	public sealed class BVisual
		: IO.IEndianStreamSerializable
	{
		public int ProtoId;

		public long UserData;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref UserData);
		}
		#endregion
	};

	public static class BVisualManager
	{
		static BVisual NewVisual()
		{
			return new BVisual();
		}
		static void SetProtoId(BVisual visual, int id)
		{
			visual.ProtoId = id;
		}
		static int GetProtoId(BVisual visual)
		{
			return visual.ProtoId;
		}
		internal static void Stream(IO.EndianStream s, ref BVisual? visual)
		{
			if (BSaveGame.StreamObjectId(s, ref visual, NewVisual, SetProtoId, GetProtoId))
			{
				visual!.Serialize(s);
			}
		}
	};
}