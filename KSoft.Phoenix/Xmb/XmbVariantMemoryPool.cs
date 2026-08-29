using System;
using System.Collections.Generic;

using Vector2f = System.Numerics.Vector2;
using Vector3f = System.Numerics.Vector3;
using Vector4f = System.Numerics.Vector4;

namespace KSoft.Phoenix.Xmb
{
	sealed partial class XmbVariantMemoryPool
		: IDisposable
	{
		/// <summary>Default amount of entry memory allocated for use</summary>
		const int kEntryStartCount = 16;

		Dictionary<uint, PoolEntry>? mEntries;
		Dictionary<uint, PoolEntry> Entries => mEntries
			?? throw new ObjectDisposedException(nameof(XmbVariantMemoryPool));
		uint mPoolSize;

		public uint Size => mPoolSize;

		IO.EndianReader? mBuffer;
		uint mBufferedDataRemaining;

		public XmbVariantMemoryPool(int initialEntryCount = kEntryStartCount)
		{
			if (initialEntryCount < 0)
			{
				initialEntryCount = kEntryStartCount;
			}

			mEntries = new Dictionary<uint, PoolEntry>(initialEntryCount);
		}
		public XmbVariantMemoryPool(byte[] buffer, Shell.EndianFormat byteOrder = Shell.EndianFormat.Big)
			: this()
		{
			ArgumentNullException.ThrowIfNull(buffer);

			mPoolSize = (uint)buffer.Length;
			var ms = new System.IO.MemoryStream(buffer, false);
			mBuffer = new IO.EndianReader(ms, byteOrder, this);
			mBufferedDataRemaining = mPoolSize;
		}

		#region IDisposable Members
		void DisposeBuffer()
		{
			Util.DisposeAndNull(ref mBuffer);
		}
		public void Dispose()
		{
			DisposeBuffer();

			if (mEntries != null)
			{
				mEntries.Clear();
				mEntries = null;
			}
		}
		#endregion

		#region Add
		uint Add(XmbFileBuilder /*builder*/_, PoolEntry e)
		{
			uint size = e.CalculateSize();

			uint offset = mPoolSize += size;
			// In case the entry needs to be aligned
			mPoolSize += e.CalculatePadding(offset);

			Entries.Add(offset, e);
			return offset;
		}

		public uint Add(XmbFileBuilder builder, int v)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			return Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, uint v)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			return Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, float v)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			return Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, double v)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			return Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, string v, bool isUnicode = false)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			entry.IsUnicode = isUnicode;
			return Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, Vector2f v)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			return Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, Vector3f v)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			return Add(builder, entry);
		}
		public uint Add(XmbFileBuilder builder, Vector4f v)
		{
			foreach (var kv in Entries)
			{
				if (kv.Value.Equals(v))
				{
					return kv.Key;
				}
			}

			var entry = PoolEntry.New(v);
			return Add(builder, entry);
		}
		#endregion

		#region Get
		bool ValidOffset(uint offset) => offset < mPoolSize;

		PoolEntry DeBuffer(XmbVariantType type, uint offset, byte flags = 0)
		{
			if (!ValidOffset(offset))
			{
				throw new ArgumentOutOfRangeException(nameof(offset), string.Create(KSoft.Util.InvariantCultureInfo,
					$"{offset:X8} > {mPoolSize:X6}"));
			}

			if (!Entries.TryGetValue(offset, out PoolEntry? e))
			{
				if (mBufferedDataRemaining == 0)
				{
					throw new InvalidOperationException("No data left in buffer");
				}
				else if (mBuffer == null)
				{
					throw new InvalidOperationException("No underlying buffer");
				}

				// Create our new entry, setting any additional properties
				e = PoolEntry.New(type);
				if (type == XmbVariantType.String)
				{
					e.IsUnicode = flags != 0;
				}
				else if (type == XmbVariantType.Vector)
				{
					e.VectorLength = flags;
				}
				// Great, now read the entry's value data
				mBuffer.Seek32(offset);
				e.Read(mBuffer);

				// Update how much data is still remaining
				uint bytes_read = (uint)(mBuffer.BaseStream.Position - offset);
				mBufferedDataRemaining -= bytes_read;

				if (mBufferedDataRemaining == 0)
				{
					DisposeBuffer();
				}

				Entries.Add(offset, e);
			}

			return e;
		}

		public int GetInt32(uint offset)
		{
			var e = DeBuffer(XmbVariantType.Int, offset);

			return (int)e.Int;
		}
		public uint GetUInt32(uint offset)
		{
			var e = DeBuffer(XmbVariantType.Int, offset);

			return e.Int;
		}
		public float GetSingle(uint offset)
		{
			var e = DeBuffer(XmbVariantType.Single, offset);

			return e.Single;
		}
		public double GetDouble(uint offset)
		{
			var e = DeBuffer(XmbVariantType.Double, offset);

			return e.Double;
		}
		public string GetString(uint offset, bool isUnicode)
		{
			var e = DeBuffer(XmbVariantType.String, offset, (byte)(isUnicode ? 1 : 0));

			return e.String ?? throw new System.InvalidOperationException("String pool entry has no string value.");
		}
		public Vector2f GetVector2D(uint offset)
		{
			var e = DeBuffer(XmbVariantType.Vector, offset, 2);

			return e.Vector2d;
		}
		public Vector3f GetVector3D(uint offset)
		{
			var e = DeBuffer(XmbVariantType.Vector, offset, 3);

			return e.Vector3d;
		}
		public Vector4f GetVector4D(uint offset)
		{
			var e = DeBuffer(XmbVariantType.Vector, offset, 4);

			return e.Vector4d;
		}
		#endregion

		public void Write(IO.EndianWriter s)
		{
			foreach (var e in Entries.Values)
			{
				e.Write(s);
			}
		}

		public static bool IsInt32(string str, ref int value, bool useInt24)
		{
			if (str == "0")
			{
				value = 0;
				return true;
			}

			int v;
			try
			{
				v = Convert.ToInt32(str, (int)NumeralBase.Decimal);
			}
			catch (FormatException)
			{
				return false;
			}
			catch (OverflowException)
			{
				return false;
			}

			// VC++'s strtol returns 0, LONG_MIN, LONG_MAX when a conversion cannot be performed or when there's overflow
			if (v == 0 || v == int.MinValue || v == int.MaxValue)
			{
				return false;
			}

			if (useInt24)
			{
				var int24 = v & XmbVariantSerialization.kValueBitMask;
				if (int24 != v)
				{
					return false;
				}
			}

			int unpackedV = v;
			if ((unpackedV & 0x800000) != 0)
			{
				unpackedV |= unchecked( (int)0xFF000000 );
			}

			if (unpackedV != v)
			{
				return false;
			}

			value = unpackedV;
			return true;
		}

		public static bool IsUInt32(string str, ref uint value, bool useInt24)
		{
			if (str == "0")
			{
				value = 0;
				return true;
			}

			uint v;
			try
			{
				v = Convert.ToUInt32(str, (int)NumeralBase.Decimal);
			}
			catch (FormatException)
			{
				return false;
			}
			catch (OverflowException)
			{
				return false;
			}

			// VC++'s strtoul returns 0, ULONG_MAX when a conversion cannot be performed or when there's overflow
			if (v == uint.MinValue || v == uint.MaxValue)
			{
				return false;
			}

			if (useInt24)
			{
				var int24 = v & XmbVariantSerialization.kValueBitMask;
				if (int24 != v)
				{
					return false;
				}
			}

			value = v;
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
		public static bool IsFloat(string str, ref uint value, double epsilon, bool useInt24)
		{
			double v;
			try
			{
				v = Convert.ToDouble(str, KSoft.Util.InvariantCultureInfo);
			}
			catch (FormatException)
			{
				return false;
			}
			catch (OverflowException)
			{
				return false;
			}

			// VC++'s strtod returns 0, +HUGE_VAL, -HUGE_VAL when a conversion cannot be performed or when there's overflow
			if (v == 0.0f)
			{
				return false;
			}

			if (double.IsNegativeInfinity(v) || double.IsPositiveInfinity(v))
			{
				return false;
			}

			// #TODO finish this

			return false;
		}
	};
}