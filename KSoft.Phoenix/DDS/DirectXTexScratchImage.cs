using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	[StructLayout(LayoutKind.Sequential)]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Owns a native DirectXTex handle.")]
	public struct DirectXTexScratchImage
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
				var hresult = DirectXTexDLL.DirectXTex_ScratchImageFree(this);
				DirectXTexDLL.ThrowIfFailed(hresult);

				Pointer = IntPtr.Zero;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}
		}

		public static DirectXTexScratchImage New()
		{
			try
			{
				var hresult = DirectXTexDLL.DirectXTex_ScratchImageNew(
					out DirectXTexScratchImage image);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return image;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return new DirectXTexScratchImage();
		}

		public readonly TexMetadata Metadata { get {
			if (IsNull || DirectXTexDLL.EntryPointsNotFound)
			{
				return TexMetadata.Empty;
			}

			try
			{
				var hresult = DirectXTexDLL.DirectXTex_ScratchImageGetMetadata(this,
					out TexMetadata metadata);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return metadata;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return TexMetadata.Empty;
		} }
	};
}