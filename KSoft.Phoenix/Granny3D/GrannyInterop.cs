using System;
using System.Runtime.InteropServices;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Granny3D
{
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct CharPtr
	{
		public IntPtr Address;

		public readonly bool IsNull => Address == IntPtr.Zero;
		public readonly bool IsNotNull => Address != IntPtr.Zero;

		public override readonly string ToString()
		{
			if (IsNull)
			{
				return null;
			}

			return Marshal.PtrToStringAnsi(Address);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct TPtr<T>
	{
		public IntPtr Address;

		public readonly bool IsNull => Address == IntPtr.Zero;
		public readonly bool IsNotNull => Address != IntPtr.Zero;

		public TPtr(IntPtr address)
		{
			Address = address;
		}
		readonly void ThrowIfNull()
		{
			if (IsNull)
			{
				throw new NullReferenceException();
			}
		}

		public readonly T ToStruct()
		{
			ThrowIfNull();

			return Marshal.PtrToStructure<T>(Address);
		}
		public readonly T ToStruct(int index)
		{
			ThrowIfNull();
			ArgumentOutOfRangeException.ThrowIfNegative(index);

			int offset = Marshal.SizeOf<T>();
			offset += index;

			return Marshal.PtrToStructure<T>(Address + offset);
		}

		public readonly void CopyStruct(ref T s)
		{
			ThrowIfNull();

			Marshal.StructureToPtr(s, Address, fDeleteOld: false);
		}
		public readonly void CopyStruct(int toIndex, ref T s)
		{
			ThrowIfNull();
			ArgumentOutOfRangeException.ThrowIfNegative(toIndex);

			int offset = Marshal.SizeOf<T>();
			offset += toIndex;

			Marshal.StructureToPtr(s, Address + offset, fDeleteOld: false);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayPtr
	{
		public int Count;
		public IntPtr Array;

		public readonly bool IsNull => Count == 0 || Array == IntPtr.Zero;
		public readonly bool IsNotNull => Count > 0 && Array != IntPtr.Zero;
		readonly void ThrowIfNull()
		{
			if (IsNull)
			{
				throw new NullReferenceException();
			}
		}

		public readonly IntPtr ToStructPtr(int index, int structSize)
		{
			ThrowIfNull();
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			Contract.Requires(structSize > 0);

			int offset = structSize;
			offset += index;

			return Array + offset;
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayCharPtr
	{
		public int Count;
		public TPtr<CharPtr> Array;

		public readonly bool IsNull => Count == 0 || Array.IsNull;
		public readonly bool IsNotNull => Count > 0 && Array.IsNotNull;
		readonly void ThrowIfNull()
		{
			if (IsNull)
			{
				throw new NullReferenceException();
			}
		}

		public readonly CharPtr ToStruct(int index)
		{
			ThrowIfNull();
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return Array.ToStruct(index);
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayPtr<T>
	{
		public int Count;
		public TPtr<T> Array;

		public readonly bool IsNull => Count == 0 || Array.IsNull;
		public readonly bool IsNotNull => Count > 0 && Array.IsNotNull;
		readonly void ThrowIfNull()
		{
			if (IsNull)
			{
				throw new NullReferenceException();
			}
		}

		public readonly T ToStruct(int index)
		{
			ThrowIfNull();
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return Array.ToStruct(index);
		}

		public readonly void CopyStruct(int toIndex, ref T s)
		{
			ThrowIfNull();
			if (toIndex < 0 || toIndex >= Count)
			{
				throw new ArgumentOutOfRangeException(nameof(toIndex));
			}

			Array.CopyStruct(toIndex, ref s);
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayOfRefsPtr<T>
	{
		public int Count;
		public IntPtr Array; // T**

		public readonly bool IsNull => Count == 0 || Array == IntPtr.Zero;
		public readonly bool IsNotNull => Count > 0 && Array != IntPtr.Zero;
		readonly void ThrowIfNull()
		{
			if (IsNull)
			{
				throw new NullReferenceException();
			}
		}

		public readonly TPtr<T> ToStructPtr(int index)
		{
			ThrowIfNull();
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			int offset = IntPtr.Size;
			offset += index;

			var ptr = Marshal.PtrToStructure<IntPtr>(Array + offset);

			return new TPtr<T>(ptr);
		}
	};
}
