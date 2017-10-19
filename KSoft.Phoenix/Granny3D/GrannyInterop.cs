using System;
using System.Runtime.InteropServices;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Granny3D
{
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct CharPtr
	{
		public IntPtr Address;

		public bool IsNull { get { return Address == IntPtr.Zero; } }
		public bool IsNotNull { get { return Address != IntPtr.Zero; } }

		public override string ToString()
		{
			if (IsNull)
				return null;

			return Marshal.PtrToStringAnsi(Address);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct TPtr<T>
	{
		public IntPtr Address;

		public bool IsNull { get { return Address == IntPtr.Zero; } }
		public bool IsNotNull { get { return Address != IntPtr.Zero; } }

		public TPtr(IntPtr address)
		{
			Address = address;
		}

		public T ToStruct()
		{
			Contract.Requires<NullReferenceException>(IsNotNull);

			return Marshal.PtrToStructure<T>(Address);
		}
		public T ToStruct(int index)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);
			Contract.Requires(index >= 0);

			int offset = Marshal.SizeOf<T>();
			offset += index;

			return Marshal.PtrToStructure<T>(Address + offset);
		}

		public void CopyStruct(ref T s)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);

			Marshal.StructureToPtr(s, Address, fDeleteOld: false);
		}
		public void CopyStruct(int toIndex, ref T s)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);
			Contract.Requires(toIndex >= 0);

			int offset = Marshal.SizeOf<T>();
			offset += toIndex;

			Marshal.StructureToPtr(s, Address + offset, fDeleteOld: false);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct ArrayPtr
	{
		public int Count;
		public IntPtr Array;

		public bool IsNull { get { return Count == 0 || Array == IntPtr.Zero; } }
		public bool IsNotNull { get { return Count > 0 && Array != IntPtr.Zero; } }

		public IntPtr ToStructPtr(int index, int structSize)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);
			Contract.Requires(structSize > 0);

			int offset = structSize;
			offset += index;

			return Array + offset;
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct ArrayCharPtr
	{
		public int Count;
		public TPtr<CharPtr> Array;

		public bool IsNull { get { return Count == 0 || Array.IsNull; } }
		public bool IsNotNull { get { return Count > 0 && Array.IsNotNull; } }

		public CharPtr ToStruct(int index)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);

			return Array.ToStruct(index);
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct ArrayPtr<T>
	{
		public int Count;
		public TPtr<T> Array;

		public bool IsNull { get { return Count == 0 || Array.IsNull; } }
		public bool IsNotNull { get { return Count > 0 && Array.IsNotNull; } }

		public T ToStruct(int index)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);

			return Array.ToStruct(index);
		}

		public void CopyStruct(int toIndex, ref T s)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(toIndex >= 0 && toIndex < Count);

			Array.CopyStruct(toIndex, ref s);
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public struct ArrayOfRefsPtr<T>
	{
		public int Count;
		public IntPtr Array; // T**

		public bool IsNull { get { return Count == 0 || Array == IntPtr.Zero; } }
		public bool IsNotNull { get { return Count > 0 && Array != IntPtr.Zero; } }

		public TPtr<T> ToStructPtr(int index)
		{
			Contract.Requires<NullReferenceException>(IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < Count);

			int offset = IntPtr.Size;
			offset += index;

			var ptr = Marshal.PtrToStructure<IntPtr>(Array + offset);

			return new TPtr<T>(ptr);
		}
	};
}