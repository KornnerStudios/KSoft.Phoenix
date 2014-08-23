
using BCost = System.Single;

namespace KSoft.Phoenix.Runtime
{
	abstract class BProtoBuildableObject
		: IO.IEndianStreamSerializable
	{
		public BCost[] Cost;
		public float BuildPoints;

		public bool Forbid;

		public abstract void Serialize(IO.EndianStream s);
	};
}