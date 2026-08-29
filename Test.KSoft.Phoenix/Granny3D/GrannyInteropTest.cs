using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Granny3D.Test
{
	[TestClass]
	public sealed class GrannyInteropTest
		: Phoenix.BaseTestClass
	{
		[StructLayout(LayoutKind.Sequential)]
		struct Cell
		{
			public int First;
			public int Second;
		}

		[TestMethod]
		public void TPtr_IndexesAndCopiesSequentialCells()
		{
			int cell_size = Marshal.SizeOf<Cell>();
			IntPtr cells = Marshal.AllocHGlobal(cell_size * 3);

			try
			{
				Marshal.StructureToPtr(new Cell { First = 1, Second = 10 }, cells + (cell_size * 0), false);
				Marshal.StructureToPtr(new Cell { First = 2, Second = 20 }, cells + (cell_size * 1), false);
				Marshal.StructureToPtr(new Cell { First = 3, Second = 30 }, cells + (cell_size * 2), false);

				var pointer = new TPtr<Cell>(cells);

				Assert.AreEqual(1, pointer.ToStruct(0).First);
				Assert.AreEqual(2, pointer.ToStruct(1).First);
				Assert.AreEqual(3, pointer.ToStruct(2).First);

				var replacement = new Cell { First = 42, Second = 420 };
				pointer.CopyStruct(2, ref replacement);

				Cell copied = pointer.ToStruct(2);
				Assert.AreEqual(replacement.First, copied.First);
				Assert.AreEqual(replacement.Second, copied.Second);
			}
			finally
			{
				Marshal.FreeHGlobal(cells);
			}
		}

		[TestMethod]
		public void ArrayPtr_DelegatesAndCalculatesIndexedAddresses()
		{
			int cell_size = Marshal.SizeOf<Cell>();
			IntPtr cells = Marshal.AllocHGlobal(cell_size * 3);

			try
			{
				Marshal.StructureToPtr(new Cell { First = 1, Second = 10 }, cells + (cell_size * 0), false);
				Marshal.StructureToPtr(new Cell { First = 2, Second = 20 }, cells + (cell_size * 1), false);
				Marshal.StructureToPtr(new Cell { First = 3, Second = 30 }, cells + (cell_size * 2), false);

				var array = new ArrayPtr { Count = 3, Array = cells };
				Assert.AreEqual(cells, array.ToStructPtr(0, cell_size));
				Assert.AreEqual(cells + cell_size, array.ToStructPtr(1, cell_size));
				Assert.AreEqual(cells + (cell_size * 2), array.ToStructPtr(2, cell_size));

				var generic_array = new ArrayPtr<Cell> { Count = 3, Array = new TPtr<Cell>(cells) };
				Assert.AreEqual(1, generic_array.ToStruct(0).First);
				Assert.AreEqual(2, generic_array.ToStruct(1).First);
				Assert.AreEqual(3, generic_array.ToStruct(2).First);
			}
			finally
			{
				Marshal.FreeHGlobal(cells);
			}
		}

		[TestMethod]
		public void ArrayOfRefsPtr_IndexesPointerTable()
		{
			IntPtr pointers = Marshal.AllocHGlobal(IntPtr.Size * 3);
			IntPtr first = new IntPtr(0x1000);
			IntPtr second = new IntPtr(0x2000);
			IntPtr third = new IntPtr(0x3000);

			try
			{
				Marshal.WriteIntPtr(pointers, IntPtr.Size * 0, first);
				Marshal.WriteIntPtr(pointers, IntPtr.Size * 1, second);
				Marshal.WriteIntPtr(pointers, IntPtr.Size * 2, third);

				var array = new ArrayOfRefsPtr<Cell> { Count = 3, Array = pointers };

				Assert.AreEqual(first, array.ToStructPtr(0).Address);
				Assert.AreEqual(second, array.ToStructPtr(1).Address);
				Assert.AreEqual(third, array.ToStructPtr(2).Address);
			}
			finally
			{
				Marshal.FreeHGlobal(pointers);
			}
		}
	};
}
