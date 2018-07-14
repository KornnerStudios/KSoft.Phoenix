using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using System.Collections;
using KSoft.IO;

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageVersion
	{
		NoAlignment,
		UsesAlignment,

		kCount
	};

	public struct CaPackageHeader
		: IO.IEndianStreamSerializable
	{
		public const string kSignature = "capack";
		public const CaPackageVersion kCurrentVersion = CaPackageVersion.UsesAlignment;

		public CaPackageVersion Version;

		public void Serialize(IO.EndianStream s)
		{
		}
	};

	public struct CaPackageEntry
		: IO.IEndianStreamSerializable
	{
		public const int kMaxNameLength = 511;

		// Technically a Pascal string with 64-bit length prefix, but I'm not adding 64-bit prefix support just for this one use case :|
		public string Name;
		public long Offset;
		public long Size;

		public void Serialize(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				long position = s.Reader.BaseStream.Position;
				long name_length = s.Reader.ReadInt64();
				if (name_length < 0 || name_length > kMaxNameLength)
					throw new System.IO.InvalidDataException("Invalid name length {0} at offset {1} in {2}".Format(name_length, position, s.StreamName));
				Name = s.Reader.ReadString(Memory.Strings.StringStorage.AsciiString, (int)name_length);
				Offset = s.Reader.ReadInt64();
				Size = s.Reader.ReadInt64();
			}
			else if (s.IsWriting)
			{
				s.Writer.Write((long)Name.Length);
				s.Writer.Write(Name, Memory.Strings.StringStorage.AsciiString);
				s.Writer.Write(Offset);
				s.Writer.Write(Size);
			}
		}
	};

	public class CaPackageFile
		: IO.IEndianStreamSerializable
	{
		[Memory.Strings.StringStorageMarkupAscii]
		public const string kSignature = "capack";
		public const ulong kCurrentVersion = (ulong)CaPackageVersion.UsesAlignment;
		public const int kDefaultAlignment = sizeof(long);

		public List<CaPackageEntry> FileEntries = new List<CaPackageEntry>();

		public void Serialize(IO.EndianStream s)
		{

		}

		void SerializeAllocationTable(IO.EndianStream s)
		{
			s.StreamSignature(kSignature, Memory.Strings.StringStorage.AsciiString);

			ulong version = kCurrentVersion;
			s.Stream(ref version);
			if (version < 0 || version > kCurrentVersion)
				KSoft.IO.VersionMismatchException.Assert(s.Reader, kCurrentVersion);

			long entries_count = FileEntries.Count;
			s.Stream(ref entries_count);
			if (entries_count > 0)
			{
				if (s.IsReading)
				{
					FileEntries.Capacity = (int)entries_count;
					for (int x = 0; x < entries_count; x++)
					{
						var e = new CaPackageEntry();
						s.Stream(ref e);
						FileEntries.Add(e);
					}
				}
				else if (s.IsWriting)
				{
					foreach (var e in FileEntries)
					{
						var e_copy = e;
						s.Stream(ref e_copy);
					}
				}
			}

		}
	};
}