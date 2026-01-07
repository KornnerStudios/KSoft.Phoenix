using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DirectXTexBlob
		: IDisposable
	{
		IntPtr Pointer;

		public readonly bool IsNull => Pointer == IntPtr.Zero;
		public readonly bool IsNotNull => Pointer != IntPtr.Zero;

		public void Dispose()
		{
			if (IsNull || DirectXTexDLL.EntryPointsNotFound)
			{
				return;
			}

			try
			{
				var hresult = DirectXTexDLL.DirectXTex_BlobFree(this);
				DirectXTexDLL.ThrowIfFailed(hresult);

				Pointer = IntPtr.Zero;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}
		}

		public static DirectXTexBlob New()
		{
			try
			{
				var hresult = DirectXTexDLL.DirectXTex_BlobNew(
					out DirectXTexBlob blob);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return blob;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return new DirectXTexBlob();
		}

		public readonly IntPtr Buffer { get {
			if (IsNull || DirectXTexDLL.EntryPointsNotFound)
			{
				return IntPtr.Zero;
			}

			try
			{
				var hresult = DirectXTexDLL.DirectXTex_BlobGetBuffer(this,
					out nint bufferPointer, out uint bufferSize);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return bufferPointer;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return IntPtr.Zero;
		} }

		public readonly uint BufferSize { get {
			if (IsNull || DirectXTexDLL.EntryPointsNotFound)
			{
				return 0;
			}

			try
			{
				var hresult = DirectXTexDLL.DirectXTex_BlobGetBuffer(this,
					out nint bufferPointer, out uint bufferSize);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return bufferSize;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return 0;
		} }
	};
}
