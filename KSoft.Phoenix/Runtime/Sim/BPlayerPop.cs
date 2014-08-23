
namespace KSoft.Phoenix.Runtime
{
	struct BPlayerPop
		: IO.IEndianStreamSerializable
	{
		public float Existing, Cap, Max, Future;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Existing); s.Stream(ref Cap); s.Stream(ref Max); s.Stream(ref Future);
		}
		#endregion
#if false
		public void ToStream(IO.IndentedTextWriter s, string popName)
		{
			s.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}",
				Existing.ToString("r"), Cap.ToString("r"), Max.ToString("r"), Future.ToString("r"),
				popName);
		}
#endif
	};
}