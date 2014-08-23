using System;
using System.Security.Cryptography;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	public sealed class MediaHeader
		: IO.IEndianStreamSerializable
	{
		static readonly Memory.Strings.StringStorage kNameStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageType.CString, 
			Shell.EndianFormat.Big, 32);
		static readonly Memory.Strings.StringStorage kDescStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageType.CString,
			Shell.EndianFormat.Big, 128);
		static readonly Memory.Strings.StringStorage kOwnerStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageType.CString, 16);

		const byte kVersion = 2;
		internal const int kSizeOf = 0x1C0;

		const int kPaddingLength = 13;

		public ulong RandomSeed;
		public string Name;
		public string Description;
		public string Owner;
		#region DateTime
		PhxDateTime mDateTime;
		public PhxDateTime DateTime { get { return mDateTime; } }
		#endregion
		public ulong Id;
		public float Length;
		public short GameType;
		public int MapType;
		public ulong DataCryptKey;

		byte[] unknown183 = new byte[20];
		ulong unknown197;

		public byte[] Hash { get; private set; }

		public MediaHeader()
		{
			Hash = new byte[20];
		}

		public void UpdateHash(SHA1CryptoServiceProvider sha)
		{
			PhxHash.UInt8(sha, kVersion);
			PhxHash.UInt64(sha, RandomSeed);
			PhxHash.Unicode(sha, Name, kNameStorage.FixedLength-1);
			PhxHash.Unicode(sha, Description, kDescStorage.FixedLength-1);
			PhxHash.Ascii(sha, Owner, kOwnerStorage.FixedLength);
			DateTime.UpdateHash(sha);
			PhxHash.UInt64(sha, Id);
			PhxHash.UInt32(sha, Bitwise.ByteSwap.SingleToUInt32(Length));
			PhxHash.UInt16(sha, (uint)GameType);
			PhxHash.UInt32(sha, (uint)MapType);
			PhxHash.UInt64(sha, DataCryptKey);
			sha.TransformBlock(unknown183, 0, unknown183.Length, null, 0);
			PhxHash.UInt64(sha, unknown197, true); // final
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersion);
			s.Stream(ref RandomSeed);
			s.Stream(ref Name, kNameStorage);
			s.Stream(ref Description, kDescStorage);
			s.Stream(ref Owner, kOwnerStorage);
			s.Stream(ref mDateTime);
			s.Stream(ref Id);
			s.Stream(ref Length);
			s.Stream(ref GameType);
			s.Stream(ref MapType);
			s.Stream(ref DataCryptKey);
			s.Stream(unknown183, 0, unknown183.Length);
			s.Stream(ref unknown197);
			s.Stream(Hash, 0, Hash.Length);
			s.Pad(kPaddingLength);
		}
		#endregion
	};
}