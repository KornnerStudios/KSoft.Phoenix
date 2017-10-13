using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	public enum DirectXTexFileType : uint
	{
		Unknown,

		DDS,
		HDR,
		TGA,
		WIC,

		Count
	};

	/// <summary>
	/// https://github.com/KornnerStudios/DirectXTex/tree/DotNet
	/// </summary>
	static class DirectXTexDLL
	{
		public enum LibraryMode : uint
		{
			Debug,
			Release,
		};

		const string kDllName = @"DDS\DirectXTexDLL.dll";
		const CallingConvention kCallConv = CallingConvention.StdCall;
		const CharSet kCharSet = CharSet.Unicode;

		public static bool Initialized { get; private set; }

		internal static bool gEntryPointsNotFound;
		public static bool EntryPointsNotFound
		{
			get
			{
				if (!Initialized && !gEntryPointsNotFound)
					Initialize();

				return gEntryPointsNotFound;
			}
			private set { gEntryPointsNotFound = value; }
		}

		public static void HandleEntryPointNotFound(EntryPointNotFoundException ex)
		{
			if (EntryPointsNotFound)
				return;

			EntryPointsNotFound = true;
			Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
				"Failed to find a DirectXTexDLL method",
				ex);
		}

		public static void Initialize()
		{
			if (Initialized)
				return;

			try
			{
				var libPointerSize = DirectXTex_GetPointerSize();
				if (libPointerSize != IntPtr.Size)
				{
					Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
						"DirectXTexDLL was built for a platform that doesn't match what we're currently running in",
						libPointerSize,
						IntPtr.Size);
					return;
				}

				var libMode = DirectXTex_GetLibraryMode();
				Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.kNone,
					"Finished loading DirectXTexDLL",
					libMode);

				Initialized = true;
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
			}
		}

		public static void Dispose()
		{
			if (!Initialized)
				return;

			Initialized = false;
			EntryPointsNotFound = false;
		}

		public static void ThrowIfFailed(int hresult)
		{
			Marshal.ThrowExceptionForHR(hresult);
		}

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern uint DirectXTex_GetPointerSize();
		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern LibraryMode DirectXTex_GetLibraryMode();

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_GetMetadataFromFile(
			DirectXTexFileType fileType,
			out TexMetadata metadata,
			string file,
			uint flags);
		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_GetMetadataFromMemory(
			DirectXTexFileType fileType,
			out TexMetadata metadata,
			IntPtr source,
			uint size,
			uint flags);

		#region DirectX::Blob
		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_BlobNew(
			out DirectXTexBlob outBlob);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_BlobFree(
			DirectXTexBlob blob);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_BlobGetBuffer(
			DirectXTexBlob blob,
			out IntPtr bufferPointer,
			out uint bufferSize);
		#endregion

		#region DirectX::ScratchImage
		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageNew(
			out DirectXTexScratchImage outImage);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageFree(
			DirectXTexScratchImage image);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageGetMetadata(
			DirectXTexScratchImage image,
			out TexMetadata metadata);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageGetRawBytes(
			DirectXTexScratchImage image,
			uint arrayIndex,
			uint mip,
			uint slice,
			out DirectXTexBlob outBlob);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageGenerateMipMaps(
			DirectXTexScratchImage image,
			out DirectXTexScratchImage outMipChain);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageCreateEmptyMipChain(
			DirectXTexScratchImage image,
			out DirectXTexScratchImage outMipChain);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageCreateTexture2D(
			byte[] pixels,
			uint width,
			uint height,
			DXGI_FORMAT format,
			uint rowPitch,
			uint slicePitch,
			out DirectXTexScratchImage outImage);

		[DllImport(kDllName, CallingConvention=kCallConv, CharSet=kCharSet)]
		public static extern int DirectXTex_ScratchImageCreateTexture11(
			DirectXTexScratchImage image,
			IntPtr d11Device,
			out IntPtr outTexture);
		#endregion
	};
}