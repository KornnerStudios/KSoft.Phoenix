﻿using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Xmb
{
	using BitFieldTraits = Bitwise.BitFieldTraits;
	using BitEncoders = TypeExtensionsPhx.BitEncoders;

	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=kSizeOf)]
	/*public*/ struct BinaryDataTreeNameValue
		: IO.IEndianStreamSerializable
	{
		#region Constants
		public const int kSizeOf = 8;

		const int kMaxDirectEncodedStringLength = 4;

		const int kSizeBitCount = 7;

		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			// 0
			public static readonly BitFieldTraits kTypeIsUnsignedBitField =
				new BitFieldTraits(Bits.kBooleanBitCount);
			// 1
			public static readonly BitFieldTraits kDirectEncodingBitField =
				new BitFieldTraits(Bits.kBooleanBitCount, kTypeIsUnsignedBitField);
			// 2
			public static readonly BitFieldTraits kTypeBitField =
				new BitFieldTraits(BitEncoders.BinaryDataTreeVariantType.BitCountTrait, kDirectEncodingBitField);
			// 5
			public static readonly BitFieldTraits kTypeSizeInBytesLog2BitField =
				new BitFieldTraits(BitEncoders.BinaryDataTreeVariantTypeSizeInBytes.BitCountTrait, kTypeBitField);
			// 8
			public static readonly BitFieldTraits kIsLastNameValueBitField =
				new BitFieldTraits(Bits.kBooleanBitCount, kTypeSizeInBytesLog2BitField);
			// 9
			public static readonly BitFieldTraits kSizeBitField =
				new BitFieldTraits(kSizeBitCount, kIsLastNameValueBitField);

			public static readonly BitFieldTraits kLastBitField =
				kSizeBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		public static int BitCount { get { return Constants.kLastBitField.FieldsBitCount; } }
		public static uint Bitmask { get { return Constants.kLastBitField.FieldsBitmask.u32; } }

		public const uint kIndirectSizeThreshold = 1U << kSizeBitCount;
		#endregion

		public static BinaryDataTreeNameValue Empty { get { return new BinaryDataTreeNameValue() { Type = BinaryDataTreeVariantType.Null }; } }

		public override string ToString()
		{
			return string.Format("0x{0} 0x{1} 0x{2}",
				NameOffset.ToString("X4"), Flags.ToString("X4"), Int.ToString("X8"));
		}

		#region Properties
		[Interop.FieldOffset(4)]
		public ushort NameOffset;
		[Interop.FieldOffset(6)]
		public ushort Flags;

		public bool IsUnsigned
		{
			get { return Bits.BitDecode(Flags, Constants.kTypeIsUnsignedBitField).ToBoolean(); }
			set { Flags = Bits.BitEncode(value.ToUInt16(), Flags, Constants.kTypeIsUnsignedBitField); }
		}

		public bool DirectEncoding
		{
			get { return Bits.BitDecode(Flags, Constants.kDirectEncodingBitField).ToBoolean(); }
			set { Flags = Bits.BitEncode(value.ToUInt16(), Flags, Constants.kDirectEncodingBitField); }
		}

		public BinaryDataTreeVariantType Type
		{
			get { return BitEncoders.BinaryDataTreeVariantType.BitDecode(Flags, Constants.kTypeBitField); }
			set { Flags = BitEncoders.BinaryDataTreeVariantType.BitEncode(value, Flags, Constants.kTypeBitField); }
		}

		public BinaryDataTreeVariantTypeSizeInBytes TypeSizeInBytes
		{
			get { return BitEncoders.BinaryDataTreeVariantTypeSizeInBytes.BitDecode(Flags, Constants.kTypeSizeInBytesLog2BitField); }
			set { Flags = BitEncoders.BinaryDataTreeVariantTypeSizeInBytes.BitEncode(value, Flags, Constants.kTypeSizeInBytesLog2BitField); }
		}

		public bool IsLastNameValue
		{
			get { return Bits.BitDecode(Flags, Constants.kIsLastNameValueBitField).ToBoolean(); }
			set { Flags = Bits.BitEncode(value.ToUInt16(), Flags, Constants.kIsLastNameValueBitField); }
		}

		public byte Size
		{
			get { return (byte)Bits.BitDecode(Flags, Constants.kSizeBitField); }
			set { Flags = Bits.BitEncode(value, Flags, Constants.kSizeBitField); }
		}

		public bool IsEmpty { get { return Type == BinaryDataTreeVariantType.Null; } }

		public bool IsIndirect { get { return !DirectEncoding; } }
		public bool SizeIsIndirect { get { return Size == Constants.kSizeBitField.Bitmask32; } }
		public bool IsArray { get { return Size > TypeSize; } }

		public bool IsUnicode { get {
			return Type == BinaryDataTreeVariantType.String && TypeSizeInBytes == BinaryDataTreeVariantTypeSizeInBytes._2byte;
		} }

		public bool HasUnicodeData { get {
			return IsUnicode;
		} }

		public uint TypeSize { get {
			if (IsEmpty)
				return 0;
			return 1U << (int)TypeSizeInBytes;
		} }
		#endregion

		#region Data
		[Interop.FieldOffset(0)]
		public bool Bool;
		[Interop.FieldOffset(0)]
		public uint Offset;
		[Interop.FieldOffset(0)]
		public uint Int;
		[Interop.FieldOffset(0)]
		public float Single;

		[Interop.FieldOffset(0)]
		public byte Char0;
		[Interop.FieldOffset(1)]
		public byte Char1;
		[Interop.FieldOffset(2)]
		public byte Char2;
		[Interop.FieldOffset(3)]
		public byte Char3;
		#endregion

		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref Int);
			s.Stream(ref NameOffset);
			s.Stream(ref Flags);
		}

		#region ToString
		string StringToString(XmbVariantMemoryPool pool)
		{
			string result = null;

			if (IsIndirect)
				result = pool.GetString(Offset, IsUnicode);
			else
			{
				// Unicode is always indirect
				//if (IsUnicode) result = new string((char)Char0, 1);
				//else
				{
					var sb = new System.Text.StringBuilder(3);
					if (Char0 != '\0')
						sb.Append((char)Char0);
					if (Char1 != '\0')
						sb.Append((char)Char1);
					if (Char2 != '\0')
						sb.Append((char)Char2);

					result = sb.ToString();
				}
			}
			return result;
		}

		uint GetDataSize(XmbVariantMemoryPool pool)
		{
			uint size = Size;

			if (SizeIsIndirect)
			{
				size = pool.GetUInt32(Offset-sizeof(uint));
				Contract.Assert(size > Constants.kSizeBitField.Bitmask32);
			}

			return size;
		}

		[Obsolete]
		public uint GetLength(XmbVariantMemoryPool pool)
		{
			uint length = 0;
			switch (Type)
			{
				case BinaryDataTreeVariantType.Null:
					break;

				case BinaryDataTreeVariantType.String:
					length = (uint)StringToString(pool).Length;
					break;

				default:
					length = GetDataSize(pool);
					length >>= (int)TypeSizeInBytes;
					break;
			}

			return length;
		}

		internal string ToString(XmbVariantMemoryPool pool)
		{
			string result = "";

			switch (Type)
			{
				case BinaryDataTreeVariantType.Bool: {
					// Phoenix uses lower case and Boolean.ToString uppercases the first letter
					result = Bool ? "true" : "false";
				} break;

				case BinaryDataTreeVariantType.Int: {
					uint i = Int;
					if (IsIndirect)
						i = pool.GetUInt32(Offset);
					result = IsUnsigned
						? i.ToString()
						: ((int)i).ToString();
				} break;

				case BinaryDataTreeVariantType.Float: {
					if (TypeSizeInBytes == BinaryDataTreeVariantTypeSizeInBytes._8byte)
					{
						double d = pool.GetDouble(Offset);
						result = d.ToStringInvariant(Numbers.kDoubleRoundTripFormatSpecifier);
					}
					else
					{
						float f = Single;
						if (IsIndirect)
							f = pool.GetSingle(Offset);
						result = f.ToStringInvariant(Numbers.kFloatRoundTripFormatSpecifier);
					}
				} break;

				case BinaryDataTreeVariantType.String: {
					result = StringToString(pool);
				} break;
			}

			return result;
		}

		#endregion

		#region RequiresIndirectStorage
		public static bool SizeValueRequiresIndirectStorage(int size)
		{
			Contract.Requires(size >= 0);

			return (uint)size >= Constants.kSizeBitField.Bitmask32;
		}
		public static bool StringRequiresIndirectStorage(string s, bool isUnicode)
		{
			Contract.Requires(s != null);
#if false
			return s.Length > kMaxDirectEncodedStringLength || isUnicode;
#else
			return true;
#endif
		}
		#endregion

		public BinaryDataTreeVariantTypeDesc GuessTypeDesc()
		{
			switch (Type)
			{
				case BinaryDataTreeVariantType.Null:
					return BinaryDataTreeVariantTypeDesc.Null;

				case BinaryDataTreeVariantType.Bool:
					return BinaryDataTreeVariantTypeDesc.Bool;

				case BinaryDataTreeVariantType.Int:
					switch (TypeSize)
					{
						case sizeof(byte):
							return IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt8
								: BinaryDataTreeVariantTypeDesc. Int8;
						case sizeof(ushort):
							return IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt16
								: BinaryDataTreeVariantTypeDesc. Int16;
						case sizeof(uint):
							return IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt32
								: BinaryDataTreeVariantTypeDesc. Int32;
						case sizeof(ulong):
							return IsUnsigned
								? BinaryDataTreeVariantTypeDesc.UInt64
								: BinaryDataTreeVariantTypeDesc. Int64;
					}
					throw new KSoft.Debug.UnreachableException(Type + TypeSize.ToString());

				case BinaryDataTreeVariantType.Float:
					if (TypeSize == sizeof(double))
					{
						return BinaryDataTreeVariantTypeDesc.Double;
					}
					else
					{
						// #NOTE it doesn't matter if SizeIsIndirect, because SingleVector Size can fit within the 7-bits of Size
						uint elements = Size / TypeSize;
						switch (elements)
						{
							case 2:
							case 3:
							case 4:
								return BinaryDataTreeVariantTypeDesc.SingleVector;
						}

						return BinaryDataTreeVariantTypeDesc.Single;
					}

				case BinaryDataTreeVariantType.String:
					return IsUnicode
						? BinaryDataTreeVariantTypeDesc.UnicodeString
						: BinaryDataTreeVariantTypeDesc.String;

				default:
					throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}
	};

	public sealed class BinaryDataTreeBuildNameValue
	{
		public string Name;
		public BinaryDataTreeVariantData Variant;
	};
}