using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Xmb
{
	using EType = BinaryDataTreeVariantType;
	using ESizeInBytes = BinaryDataTreeVariantTypeSizeInBytes;

	public enum BinaryDataTreeVariantType : byte
	{
		Null,
		Bool,
		Int,
		Float,
		String,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public enum BinaryDataTreeVariantTypeSizeInBytes : byte
	{
		_1byte,
		_2byte,
		_4byte,
		_8byte,
		_16byte,

		/// <remarks>3 bits</remarks>
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public struct BinaryDataTreeVariantTypeDesc
		: IEquatable<BinaryDataTreeVariantTypeDesc>
	{
		public const int kSizeOf = 4;

		const byte kIsUnsigned = 1;

		public EType Type;
		private readonly ESizeInBytes Size;
		private readonly ESizeInBytes Alignment;
		private readonly byte Flags;

		public readonly bool IsUnsigned => Flags != 0;
		public readonly bool IsUnicode => Type == EType.String && Size == ESizeInBytes._2byte;
		public readonly int SizeBit => (int)Size;
		public readonly int SizeOf => 1 << SizeBit;
		public readonly int AlignmentBit => (int)Alignment;
		public readonly int AlignmentOf => 1 << AlignmentBit;

		private BinaryDataTreeVariantTypeDesc(EType type, ESizeInBytes size, ESizeInBytes alignment, byte flags = 0)
		{
			Type = type;
			Size = size;
			Alignment = alignment;
			Flags = flags;
		}

		public override readonly string ToString()
		{
			return string.Create(KSoft.Util.InvariantCultureInfo,
				$"{Size} {Alignment} {Flags} {Type}");
		}

		#region Equality utils
		public static bool operator ==(BinaryDataTreeVariantTypeDesc lhs, BinaryDataTreeVariantTypeDesc rhs)
		{
			return lhs.Type == rhs.Type
				&& lhs.Size == rhs.Size
				&& lhs.Alignment == rhs.Alignment
				&& lhs.Flags == rhs.Flags;
		}
		public static bool operator !=(BinaryDataTreeVariantTypeDesc lhs, BinaryDataTreeVariantTypeDesc rhs)
		{
			return lhs.Type != rhs.Type
				|| lhs.Size != rhs.Size
				|| lhs.Alignment != rhs.Alignment
				|| lhs.Flags != rhs.Flags;
		}

		public override readonly bool Equals(object? obj)
		{
			return obj is BinaryDataTreeVariantTypeDesc desc && Equals(desc);
		}

		public readonly bool Equals(BinaryDataTreeVariantTypeDesc other) => this == other;

		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Type, Size, Alignment, Flags);
		}

		public readonly uint GetSuperFastHashCode()
		{
			var buffer = PhxUtil.GetBufferForSuperFastHash(sizeof(uint));

			uint hash;

			Bitwise.ByteSwap.ReplaceBytes(buffer, 0, (uint)Type);
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(uint));

			Bitwise.ByteSwap.ReplaceBytes(buffer, 0, (ushort)Size);
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(ushort), hash);

			Bitwise.ByteSwap.ReplaceBytes(buffer, 0, (ushort)Alignment);
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(ushort), hash);

			buffer[0] = Flags;
			hash = PhxUtil.SuperFastHash(buffer, 0, sizeof(byte), hash);

			return hash;
		}
		#endregion

		#region Array utils
		public readonly Array? MakeArray(int length)
		{
			if (length <= 1)
			{
				return null;
			}

			switch (Type)
			{
				case EType.Null:
					return null;

				case EType.Bool:
					return new bool[length];

				case EType.Int:
				{
					if (IsUnsigned)
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return new byte[length];
							case ESizeInBytes._2byte: return new ushort[length];
							case ESizeInBytes._4byte: return new uint[length];
							case ESizeInBytes._8byte: return new ulong[length];
						}
					}
					else
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return new sbyte[length];
							case ESizeInBytes._2byte: return new short[length];
							case ESizeInBytes._4byte: return new int[length];
							case ESizeInBytes._8byte: return new long[length];
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.Float:
				{
					switch (Size)
					{
						case ESizeInBytes._4byte: return new float[length];
						case ESizeInBytes._8byte: return new double[length];
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.String:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public readonly Array? ReadArray(IO.EndianReader reader, Array array)
		{
			switch (Type)
			{
				case EType.Null:
					return null;

				case EType.Bool:
					return reader.ReadFixedArray((bool[])array);

				case EType.Int:
				{
					if (IsUnsigned)
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return reader.ReadFixedArray((byte[])array);
							case ESizeInBytes._2byte: return reader.ReadFixedArray((ushort[])array);
							case ESizeInBytes._4byte: return reader.ReadFixedArray((uint[])array);
							case ESizeInBytes._8byte: return reader.ReadFixedArray((ulong[])array);
						}
					}
					else
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return reader.ReadFixedArray((sbyte[])array);
							case ESizeInBytes._2byte: return reader.ReadFixedArray((short[])array);
							case ESizeInBytes._4byte: return reader.ReadFixedArray((int[])array);
							case ESizeInBytes._8byte: return reader.ReadFixedArray((long[])array);
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.Float:
				{
					switch (Size)
					{
						case ESizeInBytes._4byte: return reader.ReadFixedArray((float[])array);
						case ESizeInBytes._8byte: return reader.ReadFixedArray((double[])array);
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.String:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public readonly Array? WriteArray(IO.EndianWriter writer, Array array)
		{
			switch (Type)
			{
				case EType.Null:
					return null;

				case EType.Bool:
					return writer.WriteFixedArray((bool[])array);

				case EType.Int:
				{
					if (IsUnsigned)
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return writer.WriteFixedArray((byte[])array);
							case ESizeInBytes._2byte: return writer.WriteFixedArray((ushort[])array);
							case ESizeInBytes._4byte: return writer.WriteFixedArray((uint[])array);
							case ESizeInBytes._8byte: return writer.WriteFixedArray((ulong[])array);
						}
					}
					else
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return writer.WriteFixedArray((sbyte[])array);
							case ESizeInBytes._2byte: return writer.WriteFixedArray((short[])array);
							case ESizeInBytes._4byte: return writer.WriteFixedArray((int[])array);
							case ESizeInBytes._8byte: return writer.WriteFixedArray((long[])array);
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.Float:
				{
					switch (Size)
					{
						case ESizeInBytes._4byte: return writer.WriteFixedArray((float[])array);
						case ESizeInBytes._8byte: return writer.WriteFixedArray((double[])array);
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.String:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public readonly string? ArrayToString(Array array)
		{
			switch (Type)
			{
				case EType.Null:
					return null;

				case EType.Bool:
					return ((bool[])array).ToConcatBinaryString();

				case EType.Int:
					return array.ArrayToConcatString();

				case EType.Float:
				{
					switch (Size)
					{
						case ESizeInBytes._4byte: return ((float[])array).ToConcatStringInvariant();
						case ESizeInBytes._8byte: return ((double[])array).ToConcatStringInvariant();
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.String:
					throw new KSoft.Debug.UnreachableException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}

		public readonly Array? ArrayFromString(string? str)
		{
			if (str.IsNullOrEmpty() || Type == EType.Null)
			{
				return null;
			}

			var str_list = new List<string>();
			if (!Util.ParseStringList(str, str_list))
			{
				throw new System.IO.InvalidDataException(str);
			}

			if (str_list.Count == 0)
			{
				return null;
			}

			switch (Type)
			{
				case EType.Bool:
					return str_list.ConvertAllArray(Text.Util.ParseBooleanLazy);

				case EType.Int:
				{
					if (IsUnsigned)
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return str_list.ConvertAllArray(Convert.ToByte);
							case ESizeInBytes._2byte: return str_list.ConvertAllArray(Convert.ToUInt16);
							case ESizeInBytes._4byte: return str_list.ConvertAllArray(Convert.ToUInt32);
							case ESizeInBytes._8byte: return str_list.ConvertAllArray(Convert.ToUInt64);
						}
					}
					else
					{
						switch (Size)
						{
							case ESizeInBytes._1byte: return str_list.ConvertAllArray(Convert.ToSByte);
							case ESizeInBytes._2byte: return str_list.ConvertAllArray(Convert.ToInt16);
							case ESizeInBytes._4byte: return str_list.ConvertAllArray(Convert.ToInt32);
							case ESizeInBytes._8byte: return str_list.ConvertAllArray(Convert.ToInt64);
						}
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.Float:
				{
					switch (Size)
					{
						case ESizeInBytes._4byte: return str_list.ConvertAllArray(Numbers.FloatParseInvariant);
						case ESizeInBytes._8byte: return str_list.ConvertAllArray(Numbers.DoubleParseInvariant);
					}
				} throw new KSoft.Debug.UnreachableException(this.ToString());

				case EType.String:
					throw new NotSupportedException(this.ToString());

				default:
					throw new KSoft.Debug.UnreachableException(this.ToString());
			}
		}
		#endregion

		#region ITagElementTextStreamable Members
		readonly string GetSerializedTypeName()
		{
			return Type switch
			{
				EType.Null => "null",
				EType.Bool => "bool",
				EType.Int => string.Create(KSoft.Util.InvariantCultureInfo,
					$"{(IsUnsigned ? "u" : "")}int{SizeOf * Bits.kByteBitCount}"),
				EType.Float => SizeOf == sizeof(float)
					? "float"
					: "double",
				EType.String => IsUnicode
					? "ustring"
					: "string",
				_ => throw new KSoft.Debug.UnreachableException(this.ToString()),
			};
		}
		static BinaryDataTreeVariantTypeDesc GuessTypeFromSerializedString(string? typeName)
		{
			return typeName switch
			{
				null or
				"" or
				"null" => Null,
				"bool" => Bool,
				"uint8" => UInt8,
				"uint16" => UInt16,
				"uint32" => UInt32,
				"uint64" => UInt64,
				"int8" => Int8,
				"int16" => Int16,
				"int32" => Int32,
				"int64" => Int64,
				"float" => Single,
				"double" => Double,
				"ustring" => UnicodeString,
				"string" => String,
				_ => throw new System.IO.InvalidDataException(typeName),
			};
		}

		public readonly void ToStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, int arrayLength)
			where TDoc : class
			where TCursor : class
		{
			if (Type != EType.Null)
			{
				string typeName = GetSerializedTypeName();
				s.WriteAttribute("dataType", typeName);
			}

			if (Type != EType.Null && AlignmentOf > SizeOf)
			{
				s.WriteAttribute("alignment", AlignmentOf);
			}

			if (arrayLength > 1)
			{
				s.WriteAttribute("arraySize", arrayLength);
			}
		}

		public static BinaryDataTreeVariantTypeDesc GuessFromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, out int arrayLength)
			where TDoc : class
			where TCursor : class
		{
			arrayLength = 0;

			string? typeName = null;
			if (!s.ReadAttributeOpt("dataType", ref typeName))
			{
				return Null;
			}

			var guessedType = GuessTypeFromSerializedString(typeName);

			int alignment = -1;
			s.ReadAttributeOpt("alignment", ref alignment);

			if (s.ReadAttributeOpt("arraySize", ref arrayLength) && arrayLength > 1)
			{
				if (guessedType.Type == EType.Float)
				{
					guessedType = SingleVector;
				}
			}

			return guessedType;
		}
		#endregion

		#region Well known types
		public static BinaryDataTreeVariantTypeDesc Null { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Null, ESizeInBytes._1byte, ESizeInBytes._1byte);
		} }
		public static BinaryDataTreeVariantTypeDesc Bool { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Bool, ESizeInBytes._1byte, ESizeInBytes._1byte, kIsUnsigned);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt8 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._1byte, ESizeInBytes._1byte, kIsUnsigned);
		} }
		public static BinaryDataTreeVariantTypeDesc Int8 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._1byte, ESizeInBytes._1byte);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt16 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._2byte, ESizeInBytes._2byte, kIsUnsigned);
		} }
		public static BinaryDataTreeVariantTypeDesc Int16 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._2byte, ESizeInBytes._2byte);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt32 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._4byte, ESizeInBytes._4byte, kIsUnsigned);
		} }
		public static BinaryDataTreeVariantTypeDesc Int32 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._4byte, ESizeInBytes._4byte);
		} }
		public static BinaryDataTreeVariantTypeDesc UInt64 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._8byte, ESizeInBytes._8byte, kIsUnsigned);
		} }
		public static BinaryDataTreeVariantTypeDesc Int64 { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Int, ESizeInBytes._8byte, ESizeInBytes._8byte);
		} }
		public static BinaryDataTreeVariantTypeDesc Single { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Float, ESizeInBytes._4byte, ESizeInBytes._4byte);
		} }
		public static BinaryDataTreeVariantTypeDesc Double { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Float, ESizeInBytes._8byte, ESizeInBytes._8byte);
		} }
		public static BinaryDataTreeVariantTypeDesc String { get {
			return new BinaryDataTreeVariantTypeDesc(EType.String, ESizeInBytes._1byte, ESizeInBytes._1byte, kIsUnsigned);
		} }
		public static BinaryDataTreeVariantTypeDesc UnicodeString { get {
			return new BinaryDataTreeVariantTypeDesc(EType.String, ESizeInBytes._2byte, ESizeInBytes._2byte, kIsUnsigned);
		} }
		public static BinaryDataTreeVariantTypeDesc SingleVector { get {
			return new BinaryDataTreeVariantTypeDesc(EType.Float, ESizeInBytes._4byte, ESizeInBytes._16byte);
		} }
		#endregion
	};
}