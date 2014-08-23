
namespace KSoft.Phoenix.Runtime
{
	sealed class BProtoAction
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 0xC8;

		public float WorkRate, WorkRateVariance;
		public int AnimType;
		public /*float*/uint DamagePerAttack;
		public int MaxNumAttacksPerAnim;
		public float StrafingTurnRate, JoinBoardTime;
		public bool Disabled;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref WorkRate); s.Stream(ref WorkRateVariance);
			s.Stream(ref AnimType);
			s.Stream(ref DamagePerAttack);
			s.Stream(ref MaxNumAttacksPerAnim);
			s.Stream(ref StrafingTurnRate); s.Stream(ref JoinBoardTime);
			s.Stream(ref Disabled);
			s.StreamSignature(cSaveMarker.ProtoAction);
		}
		#endregion
	};
}