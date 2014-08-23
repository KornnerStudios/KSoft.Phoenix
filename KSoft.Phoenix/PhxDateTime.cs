using System.Security.Cryptography;

namespace KSoft.Phoenix
{
	using PhxHash = Security.Cryptography.PhxHash;

	public struct PhxDateTime
		: IO.IEndianStreamSerializable
	{
		public short Year, Month, Day, Hour, Minute, Second;
		short unknownC, unknownE; // nanosecond?, timezone offset?

		public void UpdateHash(SHA1CryptoServiceProvider sha)
		{
			PhxHash.UInt16(sha, (uint)Year);
			PhxHash.UInt16(sha, (uint)Month);
			PhxHash.UInt16(sha, (uint)Day);
			PhxHash.UInt16(sha, (uint)Hour);
			PhxHash.UInt16(sha, (uint)Minute);
			PhxHash.UInt16(sha, (uint)Second);
			PhxHash.UInt16(sha, (uint)unknownC);
			PhxHash.UInt16(sha, (uint)unknownE);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Year);
			s.Stream(ref Month);
			s.Stream(ref Day);
			s.Stream(ref Hour);
			s.Stream(ref Minute);
			s.Stream(ref Second);
			s.Stream(ref unknownC);
			s.Stream(ref unknownE);
		}
		#endregion
	};
}