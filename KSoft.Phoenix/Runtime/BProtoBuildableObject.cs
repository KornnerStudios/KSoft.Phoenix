
namespace KSoft.Phoenix.Runtime
{
	abstract class BProtoBuildableObject
		: IO.IEndianStreamSerializable
	{
		public BCostDatum[] Cost;
		public float BuildPoints;

		public bool Forbid;

		public abstract void Serialize(IO.EndianStream s);
	};
}