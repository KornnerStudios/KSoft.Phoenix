using System;
using System.IO;

namespace KSoft.Phoenix.Xmb
{
	public enum BinaryDataTreeHeaderSignature : byte
	{
		LittleEndian = 0x3E,
		BigEndian = 0xE3,
	};

	public enum BinaryDataTreeSectionID
	{
		NodeSectionIndex,
		NameValueSectionIndex, // 4b align
		NameDataSectionIndex, // 4b align
		ValueDataSectionIndex, // 16b align

		kNumberOf,
	};

	public struct BinaryDataTreeHeader
		: IO.IEndianStreamSerializable
	{
		private const byte kExpectedSize = 1+1+1+1+4+4+ (4 * (int)BinaryDataTreeSectionID.kNumberOf);
		private const byte kExpectedHeaderDwordCount = kExpectedSize / sizeof(uint);

		public const uint kSizeOf = kExpectedSize;

		public BinaryDataTreeHeaderSignature Signature;
		//private byte mHeaderDwordCount;
		public byte HeaderCrc8;
		public byte UserSectionCount;

		public uint DataCrc32;
		public uint DataSize;

		public uint BaseSectionSize0;
		public uint BaseSectionSize1;
		public uint BaseSectionSize2;
		public uint BaseSectionSize3;

		public uint this[BinaryDataTreeSectionID sectionId]
		{
			readonly get
			{
				return (int)sectionId switch
				{
					0 => BaseSectionSize0,
					1 => BaseSectionSize1,
					2 => BaseSectionSize2,
					3 => BaseSectionSize3,
					_ => throw new KSoft.Debug.UnreachableException(sectionId.ToString()),
				};
			}
			set
			{
				switch ((int)sectionId)
				{
					case 0:
						BaseSectionSize0 = value;
						break;
					case 1:
						BaseSectionSize1 = value;
						break;
					case 2:
						BaseSectionSize2 = value;
						break;
					case 3:
						BaseSectionSize3 = value;
						break;

					default:
						throw new KSoft.Debug.UnreachableException(sectionId.ToString());
				}
			}
		}

		public Shell.EndianFormat SignatureAsEndianFormat
		{
			readonly get
			{
				return Signature == BinaryDataTreeHeaderSignature.LittleEndian
					? Shell.EndianFormat.Little
					: Shell.EndianFormat.Big;
			}
			set
			{
				Signature = value == Shell.EndianFormat.Little
					? BinaryDataTreeHeaderSignature.LittleEndian
					: BinaryDataTreeHeaderSignature.BigEndian;
			}
		}

		public readonly ushort GetCrc16()
		{
			var computer = new Security.Cryptography.Crc16.BitComputer(PhxUtil.kCrc16Definition);
			computer.ComputeBegin();

			computer.Compute((byte)Signature);
			computer.Compute(kExpectedHeaderDwordCount);
			computer.Compute(byte.MinValue);
			computer.Compute(UserSectionCount);

			var byte_order = SignatureAsEndianFormat;

			computer.Compute(byte_order, DataCrc32);
			computer.Compute(byte_order, DataSize);

			computer.Compute(byte_order, BaseSectionSize0);
			computer.Compute(byte_order, BaseSectionSize1);
			computer.Compute(byte_order, BaseSectionSize2);
			computer.Compute(byte_order, BaseSectionSize3);

			return computer.ComputeFinish();
		}

		public void Serialize(IO.EndianStream s)
		{
			bool reading = s.IsReading;

			byte signature = (byte)Signature;

			if (!reading)
			{
				HeaderCrc8 = (byte)GetCrc16();
			}

			s.Stream(ref signature);
			s.StreamSignature(kExpectedHeaderDwordCount);
			s.Stream(ref HeaderCrc8);
			s.Stream(ref UserSectionCount);

			s.Stream(ref DataCrc32);
			s.Stream(ref DataSize);

			s.Stream(ref BaseSectionSize0);
			s.Stream(ref BaseSectionSize1);
			s.Stream(ref BaseSectionSize2);
			s.Stream(ref BaseSectionSize3);

			if (reading)
			{
				Signature = (BinaryDataTreeHeaderSignature)signature;
			}
		}

		public readonly void Validate()
		{
			var actual_crc = (byte)GetCrc16();
			if (actual_crc != HeaderCrc8)
			{
				throw new InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Invalid CRC 0x{actual_crc:X2}, expected 0x{HeaderCrc8:X2}"));
			}
		}

		public static BinaryDataTreeHeaderSignature PeekSignature(BinaryReader reader)
		{
			ArgumentNullException.ThrowIfNull(reader);

			var peek = reader.PeekByte();

			return peek switch
			{
				(int)BinaryDataTreeHeaderSignature.LittleEndian or
				(int)BinaryDataTreeHeaderSignature.BigEndian
				=> (BinaryDataTreeHeaderSignature)peek,
				_ => throw new InvalidDataException(peek.ToString("X8", KSoft.Util.InvariantCultureInfo)),
			};
		}

		public static Shell.EndianFormat PeekSignatureAsEndianFormat(BinaryReader reader)
		{
			ArgumentNullException.ThrowIfNull(reader);

			var signature = PeekSignature(reader);

			return signature == BinaryDataTreeHeaderSignature.LittleEndian
				? Shell.EndianFormat.Little
				: Shell.EndianFormat.Big;
		}
	};

	public struct BinaryDataTreeSectionHeader
		: IO.IEndianStreamSerializable
		, IEquatable<BinaryDataTreeSectionHeader>
	{
		public const uint kSizeOf = sizeof(uint) * 3;

		public uint Id;
		public uint Size;
		public uint Offset;

		#region Overrides
		public override readonly bool Equals(object? obj) => obj is BinaryDataTreeSectionHeader other && Equals(other);
		public readonly bool Equals(BinaryDataTreeSectionHeader other) => Id == other.Id && Size == other.Size && Offset == other.Offset;
		public static bool operator ==(BinaryDataTreeSectionHeader left, BinaryDataTreeSectionHeader right) => left.Equals(right);
		public static bool operator !=(BinaryDataTreeSectionHeader left, BinaryDataTreeSectionHeader right) => !left.Equals(right);
		public override readonly int GetHashCode() => HashCode.Combine(Id, Size, Offset);
		#endregion

		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Id);
			s.Stream(ref Size);
			s.Stream(ref Offset);
		}
	};
}