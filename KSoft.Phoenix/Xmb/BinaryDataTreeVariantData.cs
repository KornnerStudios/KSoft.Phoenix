using System;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Xmb
{
	using BDTypeDesc = BinaryDataTreeVariantTypeDesc;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	public struct BinaryDataTreeVariantData
	{
		#region Direct Data
		[Interop.FieldOffset(0)]
		public bool Bool;
		[Interop.FieldOffset(0)]
		public uint Offset;
		[Interop.FieldOffset(0)]
		public uint Int;
		[Interop.FieldOffset(0)]
		public ulong Int64;
		[Interop.FieldOffset(0)]
		public float Single;
		[Interop.FieldOffset(0)]
		public double Double;
		#endregion

		[Interop.FieldOffset(8)]
		public BDTypeDesc TypeDesc;
		[Interop.FieldOffset(12)]
		public int ArrayLength;

		// String must come last, because we don't know how big a .NET reference really is (we could be compiling for x64!)
		[Interop.FieldOffset(16)]
		public string String;
		[Interop.FieldOffset(16)]
		public Array OpaqueArrayRef;

		public BinaryDataTreeVariantType Type { get { return TypeDesc.Type; } }
		public bool IsUnicode { get { return TypeDesc.IsUnicode; } }

		public bool UseDirectEncoding { get { return TypeDesc.SizeOf <= sizeof(uint) && ArrayLength <= 1; } }

		public int StringOrArrayLength { get {
			if (Type == BinaryDataTreeVariantType.String)
				return String != null ? String.Length : 0;

			return OpaqueArrayRef != null ? OpaqueArrayRef.Length : 0;
		} }

		internal void Read(BinaryDataTreeMemoryPool pool, BinaryDataTreeNameValue nameValue)
		{
			TypeDesc = nameValue.GuessTypeDesc();

			bool direct_encoding = nameValue.DirectEncoding;

			uint total_data_size = nameValue.Size;
			if (nameValue.SizeIsIndirect)
			{
				if (direct_encoding)
					throw new InvalidOperationException();

				total_data_size = pool.GetSizeValue(nameValue.Offset);

				if (total_data_size < BinaryDataTreeNameValue.kIndirectSizeThreshold)
					throw new InvalidOperationException();
			}

			if (TypeDesc.SizeOf > 0)
			{
				if ((total_data_size % TypeDesc.SizeOf) != 0)
					throw new InvalidOperationException(nameValue.ToString());

				ArrayLength = (int)(total_data_size / TypeDesc.SizeOf);
			}

			if (ArrayLength > 1)
			{
				if (Type != BinaryDataTreeVariantType.String)
					OpaqueArrayRef = TypeDesc.MakeArray(ArrayLength);

				pool.InternalBuffer.Seek(nameValue.Offset);
			}

			switch (Type)
			{
				case BinaryDataTreeVariantType.Null:
					break;

				case BinaryDataTreeVariantType.Bool:
					if (ArrayLength > 1)
						TypeDesc.ReadArray(pool.InternalBuffer, OpaqueArrayRef);
					else
						this.Bool = nameValue.Bool;
					break;

				case BinaryDataTreeVariantType.Int:
					if (ArrayLength > 1)
						TypeDesc.ReadArray(pool.InternalBuffer, OpaqueArrayRef);
					else
					{
						if (TypeDesc.SizeOf < sizeof(long))
							this.Int = nameValue.Int;
						else
							this.Int64 = pool.InternalBuffer.ReadUInt64();
					}
					break;

				case BinaryDataTreeVariantType.Float:
					if (ArrayLength > 1)
						TypeDesc.ReadArray(pool.InternalBuffer, OpaqueArrayRef);
					else
					{
						if (TypeDesc.SizeOf < sizeof(double))
							this.Single = nameValue.Single;
						else
							this.Double = pool.InternalBuffer.ReadDouble();
					}
					break;

				case BinaryDataTreeVariantType.String:
					ArrayLength = 1;
					if (!IsUnicode && total_data_size <= sizeof(uint))
					{
						var sb = new System.Text.StringBuilder();
						for (uint x = 0, v = nameValue.Int; x < sizeof(uint); x++, v >>= Bits.kByteBitCount)
						{
							sb.Append((char)(v & 0xFF));
						}

						this.String = sb.ToString();
					}
					else
					{
						this.String = pool.InternalBuffer.ReadString(IsUnicode
							? Memory.Strings.StringStorage.CStringUnicode
							: Memory.Strings.StringStorage.CStringAscii);
					}
					break;

				default: throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}

		public void ToStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (Type == BinaryDataTreeVariantType.Null)
				return;

			TypeDesc.ToStream(s, ArrayLength);

			if (ArrayLength > 1 && Type != BinaryDataTreeVariantType.String)
			{
				var array_str = TypeDesc.ArrayToString(OpaqueArrayRef);
				s.WriteCursor(array_str);
				return;
			}

			switch (Type)
			{
				case BinaryDataTreeVariantType.Bool:
					s.WriteCursor(this.Bool);
					break;

				case BinaryDataTreeVariantType.Int:
					s.WriteCursor(this.Int64);
					break;

				case BinaryDataTreeVariantType.Float:
					if (TypeDesc.SizeOf < sizeof(double))
						s.WriteCursor(this.Single);
					else
						s.WriteCursor(this.Double);
					break;

				case BinaryDataTreeVariantType.String:
					if (this.String.IsNotNullOrEmpty())
						s.WriteCursor(this.String);
					break;

				default: throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}

		public void ToStreamAsAttribute<TDoc, TCursor>(string attributeName, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (Type == BinaryDataTreeVariantType.Null)
				return;

			if (ArrayLength > 1 && Type != BinaryDataTreeVariantType.String)
			{
				var array_str = TypeDesc.ArrayToString(OpaqueArrayRef);
				s.WriteAttribute(attributeName, array_str);
				return;
			}

			switch (Type)
			{
				case BinaryDataTreeVariantType.Bool:
					s.WriteAttribute(attributeName, this.Bool);
					break;

				case BinaryDataTreeVariantType.Int:
					s.WriteAttribute(attributeName, this.Int64);
					break;

				case BinaryDataTreeVariantType.Float:
					if (TypeDesc.SizeOf < sizeof(double))
						s.WriteAttribute(attributeName, this.Single);
					else
						s.WriteAttribute(attributeName, this.Double);
					break;

				case BinaryDataTreeVariantType.String:
					if (this.String.IsNotNullOrEmpty())
						s.WriteAttribute(attributeName, this.String);
					break;

				default: throw new KSoft.Debug.UnreachableException(Type.ToString());
			}
		}

		public void FromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			TypeDesc = BDTypeDesc.GuessFromStream(s, out ArrayLength);

			// #TODO
			throw new NotImplementedException();
		}
	};
}