using System;
using System.Collections.Generic;
using System.IO;

namespace KSoft.Phoenix.Resource
{
	public static class ResourceUtils
	{
		#region Compression utils
		public static byte[] Compress(byte[] bytes, out uint resultAdler, int lvl = 5)
		{
			byte[] result = new byte[bytes.Length];
			result = IO.Compression.ZLib.LowLevelCompress(bytes, lvl, out uint /*adler32*/_, result);

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
		static readonly HashSet<string> kXmlBasedFilesExtensions = new() {
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

		/// <summary>Pattern that can be fed to Directory.GetFiles, etc to find all XMB files</summary>
		public static string XmbExtensionSearchPattern => $"*{Xmb.XmbFile.kFileExt}";


		public static bool IsXmbFile(string filename)
		{
			string ext = Path.GetExtension(filename);

			return ext == Xmb.XmbFile.kFileExt;
		}

		public static string AddXmbExtension(string filename)
		{
			return filename + Xmb.XmbFile.kFileExt;
		}

		public static void RemoveXmbExtension(ref string filename)
		{
			filename = filename.Replace(Xmb.XmbFile.kFileExt, "", StringComparison.Ordinal);

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
		static readonly HashSet<string> kDataBasedFileExtensions = new() {
			".cfg",
			".txt",
		};

		public static bool IsDataBasedFile(string filename)
		{
			string ext = Path.GetExtension(filename);

			if (ext == ".xmb")
			{
				return true;
			}

			if (kXmlBasedFilesExtensions.Contains(ext))
			{
				return true;
			}

			if (kDataBasedFileExtensions.Contains(ext))
			{
				return true;
			}

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
			return signature switch
			{
				kSwfSignature or kGfxSignature or kSwfCompressedSignature or kGfxCompressedSignature
				=> true,
				_ => false,
			};
		}
		public static uint GfxHeaderToSwf(uint signature)
		{
			return signature switch
			{
				kGfxSignature => kSwfSignature,
				kGfxCompressedSignature => kSwfCompressedSignature,
				_ => throw new KSoft.Debug.UnreachableException(signature.ToString("X8", KSoft.Util.InvariantCultureInfo)),
			};
		}
		public static bool IsSwfHeader(uint signature)
		{
			return signature switch
			{
				kSwfSignature or kSwfCompressedSignature
				=> true,
				_ => false,
			};
		}
		#endregion

		#region Local file utils
		public static bool IsLocalScenarioFile(string fileName)
		{
			if (string.Equals(fileName, "pfxFileList.txt", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (string.Equals(fileName, "tfxFileList.txt", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (string.Equals(fileName, "visFileList.txt", System.StringComparison.OrdinalIgnoreCase))
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

		public static void ConvertXmbToXml(
			string xmlFile,
			string xmbFile,
			Shell.EndianFormat endianFormat,
			Shell.ProcessorSize vaSize)
		{
			byte[] file_bytes = File.ReadAllBytes(xmbFile);

			using (var xmb_ms = new MemoryStream(file_bytes, false))
			using (var xmb = new IO.EndianStream(xmb_ms, endianFormat, System.IO.FileAccess.Read, name: xmbFile))
			using (var xml_ms = new MemoryStream(IntegerMath.kMega * 1))
			{
				xmb.StreamMode = FileAccess.Read;

				ResourceUtils.XmbToXml(xmb, xml_ms, vaSize);

				using (var xml_fs = File.Create(xmlFile))
				{
					xml_ms.WriteTo(xml_fs);
				}
			}
		}

		public static void ConvertBinaryDataTreeToXml(
			string xmlFile,
			string xmbFile,
			bool decompileAttributesWithTypeData = true)
		{
			var bdt = new Xmb.BinaryDataTree
			{
				DecompileAttributesWithTypeData = decompileAttributesWithTypeData
			};

			byte[] bdt_bytes;
			using (var fs = File.OpenRead(xmbFile))
			{
				bdt_bytes = new byte[fs.Length];
				int bytes_read = fs.Read(bdt_bytes, 0, bdt_bytes.Length);
				if (bytes_read != bdt_bytes.Length)
				{
					throw new IOException("Failed to read all BinaryDataTree bytes");
				}
			}

			using (var bdt_ms = new MemoryStream(bdt_bytes, writable: false))
			using (var es = new IO.EndianStream(bdt_ms, Shell.EndianFormat.Big, permissions: FileAccess.Read))
			{
				es.StreamMode = FileAccess.Read;

				bdt.Serialize(es);
			}

			bdt.ToXml(xmlFile);
		}
	};
}
