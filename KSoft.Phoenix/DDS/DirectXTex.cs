using System;
using System.IO;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	public static class DirectXTex
	{
		// BMP, JPG, JPEG, PNG, TIF, TIFF, WDP
		public static string DefaultFileExtensionForWIC => ".png";

		public static string ToFileExtension(this DirectXTexFileType type)
		{
			return type switch
			{
				DirectXTexFileType.DDS => ".dds",
				DirectXTexFileType.HDR => ".hdr",
				DirectXTexFileType.TGA => ".tga",
				DirectXTexFileType.WIC => DefaultFileExtensionForWIC,
				_ => "",
			};
		}

		public static DirectXTexFileType FileTypeFromFileExtension(string file)
		{
			ArgumentException.ThrowIfNullOrEmpty(file);

			string ext = Path.GetExtension(file);
			ext = ext.ToLowerInvariant();

			return ext switch
			{
				".dds" or
				".ddx" // #NOTE specific to Phoneix code
				=> DirectXTexFileType.DDS,
				".hdr" => DirectXTexFileType.HDR,
				".tga" => DirectXTexFileType.TGA,
				".bmp" or
				".jpg" or
				".jpeg" or
				".png" or
				".tif" or
				".tiff" or
				".wdp"
				=> DirectXTexFileType.WIC,
				_ => DirectXTexFileType.Unknown,
			};
		}

		public static TexMetadata GetMetadataFromFile(string file
			, DirectXTexFileType fileType = DirectXTexFileType.Unknown
			, uint flags = 0)
		{
			ArgumentException.ThrowIfNullOrEmpty(file);

			if (!File.Exists(file))
			{
				throw new FileNotFoundException(file);
			}

			if (fileType == DirectXTexFileType.Unknown)
			{
				fileType = FileTypeFromFileExtension(file);
			}
			if (fileType == DirectXTexFileType.Unknown)
			{
				throw new NotSupportedException(file);
			}

			var result = TexMetadata.Empty;

			if (DirectXTexDLL.EntryPointsNotFound)
			{
				return result;
			}

			try
			{
				var hresult = DirectXTexDLL.DirectXTex_GetMetadataFromFile(
					fileType, out result, file, flags);
				DirectXTexDLL.ThrowIfFailed(hresult);
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return result;
		}

		public static TexMetadata GetMetadataFromMemory(
			byte[] buffer, int startIndex, int length
			, DirectXTexFileType fileType
			, uint flags = 0)
		{
			ArgumentNullException.ThrowIfNull(buffer);
			ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
			ArgumentOutOfRangeException.ThrowIfNegative(length);
			if (startIndex > buffer.Length - length)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			var result = TexMetadata.Empty;

			if (DirectXTexDLL.EntryPointsNotFound)
			{
				return result;
			}

			var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try
			{
				var buf = bufferHandle.AddrOfPinnedObject();
				buf += startIndex;

				var hresult = DirectXTexDLL.DirectXTex_GetMetadataFromMemory(
					fileType, out result, buf, (uint)length, flags);
				DirectXTexDLL.ThrowIfFailed(hresult);
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}
			finally
			{
				bufferHandle.Free();
			}

			return result;
		}
	};
}