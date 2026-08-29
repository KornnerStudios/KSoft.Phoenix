using System;
using System.Runtime.InteropServices;

namespace KSoft.Granny3D
{
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents a native pointer wrapper.")]
	public struct CharPtr
	{
		public IntPtr Address;

		public readonly bool IsNull => Address == IntPtr.Zero;
		public readonly bool IsNotNull => Address != IntPtr.Zero;

		public override readonly string? ToString()
		{
			if (IsNull)
			{
				return null;
			}

			return Marshal.PtrToStringAnsi(Address);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents a native pointer wrapper.")]
	public struct TPtr<T>
		where T : struct
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

			int offset = checked(Marshal.SizeOf<T>() * index);

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

			int offset = checked(Marshal.SizeOf<T>() * toIndex);

			Marshal.StructureToPtr(s, Address + offset, fDeleteOld: false);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents a native pointer wrapper.")]
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
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(structSize, 0);

			int offset = checked(structSize * index);

			return Array + offset;
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents a native pointer wrapper.")]
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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents a native pointer wrapper.")]
	public struct ArrayPtr<T>
		where T : struct
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
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Represents a native pointer wrapper.")]
	public struct ArrayOfRefsPtr<T>
		where T : struct
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

			int offset = checked(IntPtr.Size * index);

			var ptr = Marshal.PtrToStructure<IntPtr>(Array + offset);

			return new TPtr<T>(ptr);
		}
	};
}
