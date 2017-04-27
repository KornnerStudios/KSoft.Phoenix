using System;
using System.Collections.Generic;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	public static class ResourceUtils
	{
		#region Compression utils
		public static byte[] Compress(byte[] bytes, out uint resultAdler, int lvl = 5)
		{
			byte[] result = new byte[bytes.Length];
			uint adler32;
			result = IO.Compression.ZLib.LowLevelCompress(bytes, lvl, out adler32, result);

			resultAdler = Security.Cryptography.Adler32.Compute(result);

			return result;
		}
		public static byte[] Decompress(byte[] bytes, int uncompressedSize, out uint resultAdler)
		{
			byte[] result = new byte[uncompressedSize];
			resultAdler = IO.Compression.ZLib.LowLevelDecompress(bytes, result);
			return result;
		}
		public static byte[] DecompressScaleform(byte[] bytes, int uncompressedSize)
		{
			return IO.Compression.ZLib.LowLevelDecompress(bytes, uncompressedSize,
				sizeof(uint) * 2); // skip the header and decompressed size
		}
		#endregion

		#region Xml extensions
		static readonly HashSet<string> kXmlBasedFilesExtensions = new HashSet<string>() {
			".xml",

			".vis",

			".ability",
			".ai",
			".power",
			".tactics",
			".triggerscript",

			".fls",
			".gls",
			".sc2",
			".sc3",
			".scn",

			".blueprint",
			".physics",
			".shp",
		};

		public static bool IsXmlBasedFile(string filename)
		{
			string ext = Path.GetExtension(filename);

			return kXmlBasedFilesExtensions.Contains(ext);
		}

		public static bool IsXmbFile(string filename)
		{
			string ext = Path.GetExtension(filename);

			return ext == Xmb.XmbFile.kFileExt;
		}

		public static void RemoveXmbExtension(ref string filename)
		{
			filename = filename.Replace(Xmb.XmbFile.kFileExt, "");

			//if (System.IO.Path.GetExtension(filename) != ".xml")
			//	filename += ".xml";
		}
		public static string RemoveXmbExtension(string filename)
		{
			ResourceUtils.RemoveXmbExtension(ref filename);
			return filename;
		}
		#endregion

		#region IsDataBasedFile
		static readonly HashSet<string> kDataBasedFileExtensions = new HashSet<string>() {
			".cfg",
			".txt",
		};

		public static bool IsDataBasedFile(string filename)
		{
			string ext = Path.GetExtension(filename);

			if (ext == ".xmb")
				return true;

			if (kXmlBasedFilesExtensions.Contains(ext))
				return true;

			if (kDataBasedFileExtensions.Contains(ext))
				return true;

			return false;
		}
		#endregion

		#region Scaleform extensions
		const uint kSwfSignature = 0x00535746; // \x00SWF
		const uint kGfxSignature = 0x00584647; // \x00XFG
		const uint kSwfCompressedSignature = 0x00535743; // \x00SWC
		const uint kGfxCompressedSignature = 0x00584643; // \x00XFC

		public static bool IsScaleformFile(string filename)
		{
			string ext = Path.GetExtension(filename);

			return ext == ".swf" || ext == ".gfx";
		}
		public static bool IsScaleformBuffer(IO.EndianReader s, out uint signature)
		{
			signature = s.ReadUInt32() & 0x00FFFFFF;
			switch (signature)
			{
			case kSwfSignature:
			case kGfxSignature:
			case kSwfCompressedSignature:
			case kGfxCompressedSignature:
				return true;

			default: return false;
			}
		}
		public static uint GfxHeaderToSwf(uint signature)
		{
			switch (signature)
			{
			case kGfxSignature:				return kSwfSignature;
			case kGfxCompressedSignature:	return kSwfCompressedSignature;

			default: throw new KSoft.Debug.UnreachableException(signature.ToString("X8"));
			}
		}
		public static bool IsSwfHeader(uint signature)
		{
			switch(signature)
			{
			case kSwfSignature:
			case kSwfCompressedSignature:
				return true;

			default:
				return false;
			}
		}
		#endregion

		#region Local file utils
		public static bool IsLocalScenarioFile(string fileName)
		{
			if (0==string.Compare(fileName, "pfxFileList.txt", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (0==string.Compare(fileName, "tfxFileList.txt", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (0==string.Compare(fileName, "visFileList.txt", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}
		#endregion

		public static void XmbToXml(IO.EndianStream xmbStream, System.IO.Stream outputStream, Shell.ProcessorSize vaSize)
		{
			ECF.EcfFileXmb.XmbToXml(xmbStream, outputStream, vaSize);
		}
	};
}