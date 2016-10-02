using System.Security.Cryptography;

namespace KSoft.Phoenix
{
	using PhxHash = Security.Cryptography.PhxHash;

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public struct PhxSYSTEMTIME
		: IO.IEndianStreamSerializable
	{
		public ushort Year;
		public ushort Month;
		public ushort DayOfWeek;
		public ushort Day;
		public ushort Hour;
		public ushort Minute;
		public ushort Second;
		public ushort Milliseconds;

		public void UpdateHash(SHA1CryptoServiceProvider sha)
		{
			PhxHash.UInt16(sha, (uint)Year);
			PhxHash.UInt16(sha, (uint)Month);
			PhxHash.UInt16(sha, (uint)DayOfWeek);
			PhxHash.UInt16(sha, (uint)Day);
			PhxHash.UInt16(sha, (uint)Hour);
			PhxHash.UInt16(sha, (uint)Minute);
			PhxHash.UInt16(sha, (uint)Second);
			PhxHash.UInt16(sha, (uint)Milliseconds);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Year);
			s.Stream(ref Month);
			s.Stream(ref DayOfWeek);
			s.Stream(ref Day);
			s.Stream(ref Hour);
			s.Stream(ref Minute);
			s.Stream(ref Second);
			s.Stream(ref Milliseconds);
		}
		#endregion
	};
}