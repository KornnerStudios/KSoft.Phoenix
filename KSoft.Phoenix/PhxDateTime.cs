using System;
using System.Security.Cryptography;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix
{
	using PhxHash = Security.Cryptography.PhxHash;

	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=PhxSYSTEMTIME.kSizeOf)]
	public struct PhxSYSTEMTIME
		: IO.IEndianStreamSerializable
	{
		public const int kSizeOf = sizeof(ulong);
		public static PhxSYSTEMTIME MinValue { get { return new PhxSYSTEMTIME(1601, 1, 1); } }
		public static PhxSYSTEMTIME MaxValue { get { return new PhxSYSTEMTIME(30827, 12, 31, 23, 59, 59, 999); } }

		[Interop.FieldOffset(0)] public ulong Bits;

		[Interop.FieldOffset( 0)] public ushort Year;
		[Interop.FieldOffset( 2)] public ushort Month;
		[Interop.FieldOffset( 4)] public ushort DayOfWeek;
		[Interop.FieldOffset( 6)] public ushort Day;
		[Interop.FieldOffset( 8)] public ushort Hour;
		[Interop.FieldOffset(10)] public ushort Minute;
		[Interop.FieldOffset(12)] public ushort Second;
		[Interop.FieldOffset(14)] public ushort Milliseconds;

		public PhxSYSTEMTIME(ulong bits) : this()
		{
			Bits = bits;
		}

		public PhxSYSTEMTIME(ushort year, ushort month, ushort day
			, ushort hour = 0, ushort minute = 0, ushort second = 0, ushort ms = 0)
		{
			Bits = 0;
			Year = year;
			Month = month;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = second;
			Milliseconds = ms;
			DayOfWeek = 0;
		}
		public PhxSYSTEMTIME(DateTime dt)
		{
			Bits = 0;
			dt = dt.ToUniversalTime();
			Year = (ushort)(dt.Year);
			Month = (ushort)(dt.Month);
			DayOfWeek = (ushort)(dt.DayOfWeek);
			Day = (ushort)(dt.Day);
			Hour = (ushort)(dt.Hour);
			Minute = (ushort)(dt.Minute);
			Second = (ushort)(dt.Second);
			Milliseconds = (ushort)(dt.Millisecond);
		}

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

		public override bool Equals(object obj)
		{
			if (obj is PhxSYSTEMTIME)
				return ((PhxSYSTEMTIME)obj) == this;

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(PhxSYSTEMTIME s1, PhxSYSTEMTIME s2)
		{
#if false
			return s1.Year == s2.Year
				&& s1.Month == s2.Month
				&& s1.Day == s2.Day
				&& s1.Hour == s2.Hour
				&& s1.Minute == s2.Minute
				&& s1.Second == s2.Second
				&& s1.Milliseconds == s2.Milliseconds;
#else
			return s1.Bits == s2.Bits;
#endif
		}

		public static bool operator !=(PhxSYSTEMTIME s1, PhxSYSTEMTIME s2)
		{
			return !(s1 == s2);
		}

		public System.DateTime ToDateTime()
		{
			if (Year == 0 || this == MinValue)
				return DateTime.MinValue;
			if (this == MaxValue)
				return DateTime.MaxValue;

			return new DateTime(Year, Month, Day, Hour, Minute, Second, Milliseconds,
				DateTimeKind.Unspecified);
		}

		public System.DateTime ToLocalTime()
		{
			if (Year == 0 || this == MinValue)
				return DateTime.MinValue;
			if (this == MaxValue)
				return DateTime.MaxValue;

			return new DateTime(Year, Month, Day, Hour, Minute, Second, Milliseconds,
				DateTimeKind.Local);
		}

		public System.DateTime ToUniversalTime()
		{
			if (Year == 0 || this == MinValue)
				return DateTime.MinValue;
			if (this == MaxValue)
				return DateTime.MaxValue;

			return new DateTime(Year, Month, Day, Hour, Minute, Second, Milliseconds,
				DateTimeKind.Utc);
		}
	};
}