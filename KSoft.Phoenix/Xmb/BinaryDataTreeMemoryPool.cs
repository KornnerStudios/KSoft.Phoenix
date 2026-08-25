using System;
using System.Collections.Generic;

using Vector2f = System.Numerics.Vector2;
using Vector3f = System.Numerics.Vector3;
using Vector4f = System.Numerics.Vector4;

namespace KSoft.Phoenix.Xmb
{
	public sealed partial class BinaryDataTreeMemoryPool
		: IDisposable
	{
		/// <summary>Default amount of entry memory allocated for use</summary>
		const int kEntryStartCount = 16;

		Dictionary<uint, PoolEntry>? mEntries;
		Dictionary<uint, PoolEntry> Entries => mEntries
			?? throw new ObjectDisposedException(nameof(BinaryDataTreeMemoryPool));
		readonly Dictionary<uint, uint> mDataOffsetToSizeValue;
		readonly uint mPoolSize;

		public uint Size => mPoolSize;

		IO.EndianReader? mBuffer;
		uint mBufferedDataRemaining;

		internal IO.EndianReader? InternalBuffer { get { return mBuffer; } }

		public BinaryDataTreeMemoryPool(int initialEntryCount = kEntryStartCount)
		{
			if (initialEntryCount < 0)
			{
				initialEntryCount = kEntryStartCount;
			}

			mEntries = new Dictionary<uint, PoolEntry>(initialEntryCount);
			mDataOffsetToSizeValue = new Dictionary<uint, uint>();
		}
		public BinaryDataTreeMemoryPool(byte[] buffer, Shell.EndianFormat byteOrder = Shell.EndianFormat.Big)
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
		#endregion

		#region Get
		bool ValidOffset(uint offset) => offset < mPoolSize;

		public uint GetSizeValue(uint dataOffset)
		{
			if (!ValidOffset(dataOffset))
			{
				throw new ArgumentOutOfRangeException(nameof(dataOffset), string.Create(KSoft.Util.InvariantCultureInfo,
					$"{dataOffset:X8} > {mPoolSize:X6}"));
			}

			if (dataOffset < sizeof(uint))
			{
				throw new ArgumentOutOfRangeException(nameof(dataOffset), "Offset doesn't have room for a size value");
			}

			if (!mDataOffsetToSizeValue.TryGetValue(dataOffset, out uint size_value))
			{
				if (mBufferedDataRemaining == 0)
				{
					throw new InvalidOperationException("No data left in buffer");
				}
				else if (mBuffer == null)
				{
					throw new InvalidOperationException("No underlying buffer");
				}

				uint size_offset = dataOffset - sizeof(uint);
				// Great, now read the entry's value data
				mBuffer.Seek32(size_offset);
				size_value = mBuffer.ReadUInt32();

				// Update how much data is still remaining
				mBufferedDataRemaining -= sizeof(uint);

				if (mBufferedDataRemaining == 0)
				{
					DisposeBuffer();
				}

				mDataOffsetToSizeValue.Add(dataOffset, size_value);
			}

			return size_value;
		}

		internal PoolEntry DeBuffer(BinaryDataTreeNameValue nameValue)
		{
			var type_desc = nameValue.GuessTypeDesc();
			uint offset = nameValue.Offset;
			bool size_is_indirect = nameValue.SizeIsIndirect;
			return DeBuffer(type_desc, offset, size_is_indirect);
		}

		PoolEntry DeBuffer(BinaryDataTreeVariantTypeDesc desc, uint offset, bool sizeIsIndirect = false)
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
				e = PoolEntry.New(desc);
				if (sizeIsIndirect)
				{
					uint size = GetSizeValue(offset);
					e.ArrayLength = (int)(size >> desc.SizeBit);
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
		#endregion

		public void Write(IO.EndianWriter s)
		{
			foreach (var e in Entries.Values)
			{
				e.Write(s);
			}
		}
	};
}