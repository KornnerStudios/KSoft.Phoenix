
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	public struct BObjectAttachments
		: IO.IEndianStreamSerializable
	{
		public BVector Position, Up, Forward;
		public BEntityID AttachmentObjectID;
		public int ToBoneHandle, FromBoneHandle;
		public bool IsUnitAttachment
			, UseOffset
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref Position); s.StreamV(ref Up); s.StreamV(ref Forward);
			s.Stream(ref AttachmentObjectID);
			s.Stream(ref ToBoneHandle); s.Stream(ref FromBoneHandle);
			s.Stream(ref IsUnitAttachment); s.Stream(ref UseOffset);
		}
		#endregion
	};
}