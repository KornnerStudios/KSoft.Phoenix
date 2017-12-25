using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Interop = System.Runtime.InteropServices;

using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix.Xmb
{
	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size=kSizeOf)]
	/*public*/ struct XmbVariant
	{
		const int kSizeOf = 8;

		public static XmbVariant Empty { get { return new XmbVariant() { Type = XmbVariantType.Null }; } }

		#region Properties
		[Interop.FieldOffset(0)]
		public XmbVariantType Type;
		[Interop.FieldOffset(1)]
		public bool IsIndirect;

		// These are all type dependent so we reuse the memory space
		[Interop.FieldOffset(2)]
		public bool IsUnsigned;
		[Interop.FieldOffset(2)]
		public bool IsUnicode;
		[Interop.FieldOffset(2)]
		public byte VectorLength;

		public bool IsEmpty { get { return Type == XmbVariantType.Null; } }

		public bool HasUnicodeData { get {
			return Type == XmbVariantType.String && IsUnicode;
		} }
		#endregion

		#region Data
		[Interop.FieldOffset(4)]
		public bool Bool;
		[Interop.FieldOffset(4)]
		public uint Offset;
		[Interop.FieldOffset(4)]
		public uint Int;
		[Interop.FieldOffset(4)]
		public float Single;

		[Interop.FieldOffset(4)]
		public byte Char0;
		[Interop.FieldOffset(5)]
		public byte Char1;
		[Interop.FieldOffset(6)]
		public byte Char2;
		#endregion

		#region ToString
		static string VectorToString(uint offset, int length, XmbVariantMemoryPool pool)
		{
			float x = 0, y = 0, z = 0, w = 0;
			switch (length)
			{
				case 1: {
					x = pool.GetSingle(offset);
				} break;
				case 2: {
					var v = pool.GetVector2D(offset);
					x = v.X;
					y = v.Y;
				} break;
				case 3: {
					var v = pool.GetVector3D(offset);
					x = v.X;
					y = v.Y;
					z = v.Z;
				} break;
				case 4: {
					var v = pool.GetVector4D(offset);
					x = v.X;
					y = v.Y;
					z = v.Z;
					w = v.W;
				} break;

				default:
					throw new ArgumentOutOfRangeException("length", length.ToString());
			}

			var vector = new BVector(x, y, z, w);
			return vector.ToBVectorString(length);
		}
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
		internal string ToString(XmbVariantMemoryPool pool)
		{
			string result = "";

			switch (Type)
			{
				case XmbVariantType.Single: {
					float f = Single;
					if (IsIndirect)
						f = pool.GetSingle(Offset);
					result = f.ToStringInvariant(Numbers.kFloatRoundTripFormatSpecifier);
				} break;

				case XmbVariantType.Int: {
					uint i = Int;
					if (IsIndirect)
						i = pool.GetUInt32(Offset);
					result = IsUnsigned
						? i.ToString()
						: ((int)i).ToString();
				} break;

				case XmbVariantType.Double: {
					double d = pool.GetDouble(Offset);
					result = d.ToStringInvariant(Numbers.kDoubleRoundTripFormatSpecifier);
				} break;

				case XmbVariantType.Bool: {
					// Phoenix uses lower case and Boolean.ToString uppercases the first letter
					result = Bool ? "true" : "false";
				} break;

				case XmbVariantType.String: {
					result = StringToString(pool);
				} break;

				case XmbVariantType.Vector: {
					result = VectorToString(Offset, VectorLength, pool);
				} break;
			}

			return result;
		}
		#endregion
	};
}