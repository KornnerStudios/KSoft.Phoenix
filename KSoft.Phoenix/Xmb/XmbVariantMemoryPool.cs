using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using Vector2f = SlimMath.Vector2;
using Vector3f = SlimMath.Vector3;
using Vector4f = SlimMath.Vector4;

namespace KSoft.Phoenix.Xmb
{
	sealed partial class XmbVariantMemoryPool
		: IDisposable
	{
		const uint kValueMask = 0x00FFFFFF;

		/// <summary>Default amount of entry memory allocated for use</summary>
		const int kEntryStartCount = 16;

		Dictionary<uint, PoolEntry> mEntries;
		uint mPoolSize;

		public uint Size { get { return mPoolSize; } }

		IO.EndianReader mBuffer;
		uint mBufferedDataRemaining;

		public XmbVariantMemoryPool(int initialEntryCount = kEntryStartCount)
		{
			if (initialEntryCount < 0) initialEntryCount = kEntryStartCount;

			mEntries = new Dictionary<uint, PoolEntry>(kEntryStartCount);
		}
		public XmbVariantMemoryPool(byte[] buffer, Shell.EndianFormat byteOrder = Shell.EndianFormat.Big) : this()
		{
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
		uint Add(PoolEntry e)
		{
			uint size = e.CalculateSize();

			uint offset = mPoolSize += size;
			// In case the entry needs to be aligned
			mPoolSize += e.CalculatePadding(offset);

			mEntries.Add(offset, e);
			return offset;
		}

		public uint Add(int v)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			return Add(entry);
		}
		public uint Add(uint v)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			return Add(entry);
		}
		public uint Add(float v)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			return Add(entry);
		}
		public uint Add(double v)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			return Add(entry);
		}
		public uint Add(string v, bool isUnicode = false)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			entry.IsUnicode = isUnicode;
			return Add(entry);
		}
		public uint Add(Vector2f v)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			return Add(entry);
		}
		public uint Add(Vector3f v)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			return Add(entry);
		}
		public uint Add(Vector4f v)
		{
			foreach (var kv in mEntries) if (kv.Value.Equals(v)) return kv.Key;

			var entry = PoolEntry.New(v);
			return Add(entry);
		}
		#endregion

		#region Get
		bool ValidOffset(uint offset)	{ return offset < mPoolSize; }

		PoolEntry DeBuffer(XmbVariantType type, uint offset, byte flags = 0)
		{
			if (!ValidOffset(offset)) throw new ArgumentOutOfRangeException("offset",
				string.Format("{0} > {1}", offset.ToString("X8"), mPoolSize.ToString("X6")));

			PoolEntry e;
			if (!mEntries.TryGetValue(offset, out e))
			{
					 if (mBufferedDataRemaining == 0)	throw new InvalidOperationException("No data left in buffer");
				else if (mBuffer == null)				throw new InvalidOperationException("No underlying buffer");

				// Create our new entry, setting any additional properties
				e = PoolEntry.New(type);
					 if (type == XmbVariantType.String) e.IsUnicode = flags != 0;
				else if (type == XmbVariantType.Vector) e.VectorLength = flags;
				// Great, now read the entry's value data
				mBuffer.Seek32(offset);
				e.Read(mBuffer);

				// Update how much data is still
				uint bytes_read = (uint)(mBuffer.BaseStream.Position - offset);
				mBufferedDataRemaining -= bytes_read;

				if (mBufferedDataRemaining == 0)
					DisposeBuffer();

				mEntries.Add(offset, e);
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

			return e.String;
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
			foreach (var e in mEntries.Values)
				e.Write(s);
		}
	};
}