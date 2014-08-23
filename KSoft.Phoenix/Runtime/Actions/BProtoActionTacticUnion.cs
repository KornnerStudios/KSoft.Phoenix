
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	// lol idk
	struct BProtoActionTacticUnion
		: IO.IEndianStreamSerializable
	{
		public bool ProtoAction;
		public sbyte PlayerID;
		public BProtoObjectID ProtoObjectID;
		public sbyte ProtoActionIndex;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref ProtoAction);
			s.Stream(ref PlayerID);
			s.Stream(ref ProtoObjectID);
			s.Stream(ref ProtoActionIndex);
		}
		#endregion
	};
}