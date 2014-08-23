﻿using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFile
		: IO.IEndianStreamSerializable
	{
		public const string kExtensionEncrypted = ".era";
		public const string kExtensionDecrypted = ".bin";

		public static byte[] Compress(byte[] bytes, out uint resultAdler, int lvl = 5)
		{
			byte[] result = new byte[bytes.Length];
			return IO.Compression.ZLib.LowLevelCompress(bytes, lvl, out resultAdler, result, trimCompressedBytes: false);
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
			string ext = System.IO.Path.GetExtension(filename);

			return kXmlBasedFilesExtensions.Contains(ext);
		}

		public static bool IsXmbFile(string filename)
		{
			string ext = System.IO.Path.GetExtension(filename);

			return ext == ".xmb";
		}

		public static void RemoveXmbExtension(ref string filename)
		{
			filename = filename.Replace(".xmb", "");

			//if (System.IO.Path.GetExtension(filename) != ".xml")
			//	filename += ".xml";
		}
		#endregion

		#region Scaleform extensions
		const uint kSwfSignature = 0x00535746; // \x00SWF
		const uint kGfxSignature = 0x00584647; // \x00XFG
		const uint kSwfCompressedSignature = 0x00535743; // \x00SWC
		const uint kGfxCompressedSignature = 0x00584643; // \x00XFC

		public static bool IsScaleformFile(string filename)
		{
			string ext = System.IO.Path.GetExtension(filename);

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
		#endregion

		const int kAlignmentBit = 12;
		const string kFilenamesTableName = "_filenames.bin";
		static readonly Memory.Strings.StringMemoryPoolSettings kFilenamesTablePoolConfig = new Memory.Strings.
			StringMemoryPoolSettings(Memory.Strings.StringStorage.CStringAscii, false);

		EraFileHeader mHeader = new EraFileHeader();
		List<EraFileEntryChunk> mFiles = new List<EraFileEntryChunk>();

		public int CalculateHeaderAndFileChunksSize()
		{
			return EraFileHeader.CalculateHeaderSize() + EraFileEntryChunk.CalculateFileChunksSize(mFiles.Count);
		}

		#region Xml Streaming
		static EraFileEntryChunk GenerateFilenamesTableEntryChunk()
		{
			var chunk = new EraFileEntryChunk();
			chunk.IsCompressedStream = true;

			return chunk;
		}
		public bool ReadDefinition(IO.XmlElementStream s)
		{
			mFiles.Clear();

			mFiles.Add(GenerateFilenamesTableEntryChunk());

			foreach (var n in s.ElementsByName(ECF.EcfChunk.kXmlElementStreamName))
			{
				var f = new EraFileEntryChunk();
				using (s.EnterCursorBookmark(n))
					f.Read(s, false);

				mFiles.Add(f);
			}

			if (mFiles.Count > 1)
				mFiles.RemoveAt(0); // internal filenames

			return mFiles.Count != 0;
		}
		public void WriteDefinition(IO.XmlElementStream s)
		{
			for (int x = 1; x < mFiles.Count; x++)
				mFiles[x].Write(s, false);
		}
		#endregion

		#region Expand \ Buid
		public void ExpandTo(IO.EndianStream blockStream, string basePath)
		{
			Contract.Requires(blockStream.IsReading);

			var expander = blockStream.Owner as EraFileExpander;

			for (int x = 1; x < mFiles.Count; x++)
			{
				if (expander != null) expander.VerboseOutput.Write("\r\t{0} ", mFiles[x].EntryId.ToString("X16"));
				mFiles[x].Unpack(blockStream, basePath);
			}

			if(expander != null)
				expander.VerboseOutput.Write("\r\t{0} \r", new string(' ', 16));
		}

		bool BuildFilenamesTable(IO.EndianStream blockStream)
		{
			Contract.Requires(blockStream.IsWriting);

			using (var ms = new System.IO.MemoryStream(mFiles.Count * 128))
			using (var s = new IO.EndianWriter(ms, blockStream.ByteOrder))
			{
				var smp = new Memory.Strings.StringMemoryPool(kFilenamesTablePoolConfig);
				for (int x = 1; x < mFiles.Count; x++)
				{
					var file = mFiles[x];
					if(file.EntryId == 0) file.EntryId = (ulong)x;
					file.FilenameOffset = smp.Add(file.Filename).u32;
				}
				smp.WriteStrings(s);
				ms.Seek(0, System.IO.SeekOrigin.Begin);
				
				return mFiles[0].Pack(blockStream, ms);
			}
		}
		public bool Build(IO.EndianStream blockStream, string basePath)
		{
			Contract.Requires(blockStream.IsWriting);

			var builder = blockStream.Owner as EraFileBuilder;

			bool success = BuildFilenamesTable(blockStream);
			for (int x = 1; x < mFiles.Count && success; x++)
			{
				if (builder != null)
					builder.VerboseOutput.Write("\r\t\t{0} ", mFiles[x].EntryId.ToString("X16"));

				success &= mFiles[x].Pack(blockStream, basePath);
			}

			if (builder != null)
				builder.VerboseOutput.Write("\r\t\t{0} \r", new string(' ', 16));

			if(success)
				blockStream.AlignToBoundry(kAlignmentBit);

			return success;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public void ReadPostprocess(IO.EndianStream s)
		{
			if (mFiles.Count == 0)
				return;

			var filenames_chunk = mFiles[0];
			filenames_chunk.Filename = kFilenamesTableName;

			byte[] filenames_buffer = filenames_chunk.GetBuffer(s);
			using (var ms = new System.IO.MemoryStream(filenames_buffer))
			using (var er = new IO.EndianReader(ms, s.ByteOrder))
			{
				for (int x = 1; x < mFiles.Count; x++)
				{
					var file = mFiles[x];

					if (file.FilenameOffset != er.BaseStream.Position)
						throw new System.IO.InvalidDataException(file.FilenameOffset.ToString("X8"));

					file.Filename = er.ReadString(Memory.Strings.StringStorage.CStringAscii);
				}
			}
		}

		public void Serialize(IO.EndianStream s)
		{
			if (s.IsWriting)
			{
				mHeader.UpdateFileCount(mFiles.Count);
			}

			mHeader.Serialize(s);

			if (s.IsReading)
			{
				mFiles.Capacity = mHeader.FileCount;

				for (int x = 0; x < mFiles.Capacity; x++)
				{
					var file = new EraFileEntryChunk();
					file.Serialize(s);

					mFiles.Add(file);
				}
			}
			else if(s.IsWriting)
			{
				foreach (var f in mFiles)
					f.Serialize(s);
			}
		}
		#endregion
	};
}