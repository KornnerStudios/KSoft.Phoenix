using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFile
		: IO.IEndianStreamSerializable
		, System.IDisposable
	{
		public const string kExtensionEncrypted = ".era";
		public const string kExtensionDecrypted = ".bin";

		#region Compression utils
		public static byte[] Compress(byte[] bytes, out uint resultAdler, int lvl = 5)
		{
			byte[] result = new byte[bytes.Length];
			uint adler32;
			result = IO.Compression.ZLib.LowLevelCompress(bytes, lvl, out adler32, result);

			resultAdler = KSoft.Security.Cryptography.Adler32.Compute(result);

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

		const int kAlignmentBit = IntegerMath.kFourKiloAlignmentBit;
		const string kFileNamesTableName = "_filenames.bin";
		static readonly Memory.Strings.StringMemoryPoolSettings kFileNamesTablePoolConfig = new Memory.Strings.
			StringMemoryPoolSettings(Memory.Strings.StringStorage.CStringAscii, false);

		private EraFileHeader mHeader = new EraFileHeader();
		private List<EraFileEntryChunk> mFiles = new List<EraFileEntryChunk>();
		private Dictionary<string, EraFileEntryChunk> mFileNameToChunk = new Dictionary<string, EraFileEntryChunk>();

		public Security.Cryptography.TigerHashBase TigerHasher { get; private set; }

		private int FileChunksFirstIndex { get {
			// First comes the filenames table in mFiles, then all the files defined in the listing
			return 1;
		} }
		/// <summary>All files destined for the ERA, excluding the internal filenames table</summary>
		private IEnumerable<EraFileEntryChunk> FileChunks { get {
			// Skip the first chunk, as it is the filenames table
			return Enumerable.Skip(mFiles, FileChunksFirstIndex);
		} }
		/// <summary>Number of files destined for the ERA, excluding the internal filenames table</summary>
		private int FileChunksCount { get {
			// Exclude the first chunk from the count, as it is the filenames table
			return mFiles.Count - FileChunksFirstIndex;
		} }

		public EraFile()
		{
			TigerHasher = Security.Cryptography.PhxHash.CreateHaloWarsTigerHash();
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (TigerHasher != null)
			{
				TigerHasher.Dispose();
				TigerHasher = null;
			}
		}
		#endregion

		public int CalculateHeaderAndFileChunksSize()
		{
			return
				EraFileHeader.CalculateHeaderSize() +
				EraFileEntryChunk.CalculateFileChunksSize(mFiles.Count);
		}

		private void ValidateAdler32(EraFileEntryChunk fileEntry, IO.EndianStream blockStream)
		{
			var actual_adler = fileEntry.ComputeAdler32(blockStream);

			if (actual_adler != fileEntry.Adler32)
			{
				string chunk_name = !string.IsNullOrEmpty(fileEntry.FileName)
					? fileEntry.FileName
					: "FileNames";//fileEntry.EntryId.ToString("X16");

				throw new System.IO.InvalidDataException(string.Format(
					"Invalid chunk adler32 for '{0}' offset={1} size={2} " +
					"expected {3} but got {4}",
					chunk_name, fileEntry.DataOffset, fileEntry.DataSize.ToString("X8"),
					fileEntry.Adler32.ToString("X8"),
					actual_adler.ToString("X8")
					));
			}
		}

		private void ValidateHashes(EraFileEntryChunk fileEntry, IO.EndianStream blockStream)
		{
			fileEntry.ComputeHash(blockStream, TigerHasher);

			if (!fileEntry.CompressedDataTiger128.EqualsArray(TigerHasher.Hash))
			{
				string chunk_name = !string.IsNullOrEmpty(fileEntry.FileName)
					? fileEntry.FileName
					: "FileNames";//fileEntry.EntryId.ToString("X16");

				throw new System.IO.InvalidDataException(string.Format(
					"Invalid chunk hash for '{0}' offset={1} size={2} " +
					"expected {3} but got {4}",
					chunk_name, fileEntry.DataOffset, fileEntry.DataSize.ToString("X8"),
					Text.Util.ByteArrayToString(fileEntry.CompressedDataTiger128),
					Text.Util.ByteArrayToString(TigerHasher.Hash, 0, EraFileEntryChunk.kCompresssedDataTigerHashSize)
					));
			}

			if (fileEntry.CompressionType == ECF.EcfCompressionType.Stored)
			{
				ulong tiger64;
				TigerHasher.TryGetAsTiger64(out tiger64);

				if (fileEntry.DecompressedDataTiger64 != tiger64)
				{
					string chunk_name = !string.IsNullOrEmpty(fileEntry.FileName)
						? fileEntry.FileName
						: "FileNames";//fileEntry.EntryId.ToString("X16");

					throw new System.IO.InvalidDataException(string.Format(
						"Chunk id mismatch for '{0}' offset={1} size={2} " +
						"expected {3} but got {4}",
						chunk_name, fileEntry.DataOffset, fileEntry.DataSize.ToString("X8"),
						fileEntry.DecompressedDataTiger64.ToString("X16"),
						Text.Util.ByteArrayToString(TigerHasher.Hash, 0, sizeof(ulong))
						));
				}
			}
		}

		private int FileIndexToListingIndex(int fileIndex)
		{
			return fileIndex - 1;
		}

		private void BuildFileNameMaps(System.IO.TextWriter verboseOutput)
		{
			for (int x = FileChunksFirstIndex; x < mFiles.Count; )
			{
				var file = mFiles[x];

				EraFileEntryChunk existingFile;
				if (mFileNameToChunk.TryGetValue(file.FileName, out existingFile))
				{
					if (verboseOutput != null)
					{
						verboseOutput.WriteLine("Removing duplicate {0} entry at #{1}",
							file.FileName, FileIndexToListingIndex(x));
					}
					mFiles.RemoveAt(x);
					continue;
				}

				mFileNameToChunk.Add(file.FileName, file);
				x++;
			}
		}

		private void RemoveXmbFilesWhereXmlExists(System.IO.TextWriter verboseOutput)
		{
			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				var file = mFiles[x];
				if (!IsXmbFile(file.FileName))
					continue;

				string xml_name = file.FileName;
				RemoveXmbExtension(ref xml_name);
				EraFileEntryChunk xml_file;
				if (!mFileNameToChunk.TryGetValue(xml_name, out xml_file))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tRemoving XMB file #{0} '{1}' from listing since its XML already exists {2}",
						FileIndexToListingIndex(x),
						file.FileName,
						xml_file.FileName);

				mFiles.RemoveAt(x);
				x--;
			}
		}

		private void RemoveXmlFilesWhereXmbExists(System.IO.TextWriter verboseOutput)
		{
			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				var file = mFiles[x];
				if (!IsXmlBasedFile(file.FileName))
					continue;

				string xmb_name = file.FileName;
				xmb_name += ".xmb";
				EraFileEntryChunk xmb_file;
				if (!mFileNameToChunk.TryGetValue(xmb_name, out xmb_file))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tRemoving XML file #{0} '{1}' from listing since its XMB already exists {2}",
						FileIndexToListingIndex(x),
						file.FileName,
						xmb_file.FileName);

				mFiles.RemoveAt(x);
				x--;
			}
		}

		#region Xml definition Streaming
		static EraFileEntryChunk GenerateFilenamesTableEntryChunk()
		{
			var chunk = new EraFileEntryChunk();
			chunk.CompressionType = ECF.EcfCompressionType.DeflateStream;

			return chunk;
		}
		private void ReadChunks(IO.XmlElementStream s)
		{
			foreach (var n in s.ElementsByName(ECF.EcfChunk.kXmlElementStreamName))
			{
				var f = new EraFileEntryChunk();
				using (s.EnterCursorBookmark(n))
				{
					f.Read(s, false);
				}

				mFiles.Add(f);
			}
		}
		private void WriteChunks(IO.XmlElementStream s)
		{
			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				mFiles[x].Write(s, false);
			}
		}

		public bool ReadDefinition(IO.XmlElementStream s)
		{
			mFiles.Clear();

			// first entry should always be the null terminated filenames table
			mFiles.Add(GenerateFilenamesTableEntryChunk());

			using (s.EnterCursorBookmark("Files"))
				ReadChunks(s);

			// there should be at least one file destined for the ERA, excluding the filenames table
			return FileChunksCount != 0;
		}
		public void WriteDefinition(IO.XmlElementStream s)
		{
			using (s.EnterCursorBookmark("Files"))
				WriteChunks(s);
		}
		#endregion

		#region Expand \ Buid
		public void ExpandTo(IO.EndianStream blockStream, string basePath)
		{
			Contract.Requires(blockStream.IsReading);

			var eraUtil = blockStream.Owner as EraFileUtil;

			if (eraUtil != null && eraUtil.VerboseOutput != null)
			{
				eraUtil.VerboseOutput.WriteLine("\tUnpacking files...");
			}

			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				var file = mFiles[x];

				if (eraUtil != null && eraUtil.VerboseOutput != null)
				{
					eraUtil.VerboseOutput.Write("\r\t\t{0} ", file.EntryId.ToString("X16"));
				}
				file.Unpack(blockStream, basePath);
			}

			if (eraUtil != null && eraUtil.VerboseOutput != null)
			{
				eraUtil.VerboseOutput.Write("\r\t\t{0} \r", new string(' ', 16));
				eraUtil.VerboseOutput.WriteLine("\tDone");
			}
		}

		private bool BuildFileNamesTable(IO.EndianStream blockStream)
		{
			Contract.Requires(blockStream.IsWriting);

			using (var ms = new System.IO.MemoryStream(mFiles.Count * 128))
			using (var s = new IO.EndianWriter(ms, blockStream.ByteOrder))
			{
				var smp = new Memory.Strings.StringMemoryPool(kFileNamesTablePoolConfig);
				for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
				{
					var file = mFiles[x];

					file.FileNameOffset = smp.Add(file.FileName).u32;
				}
				smp.WriteStrings(s);

				var filenames_chunk = mFiles[0];
				bool success = filenames_chunk.Pack(blockStream, ms, TigerHasher);

				return success;
			}
		}
		public bool Build(IO.EndianStream blockStream, string basePath)
		{
			Contract.Requires(blockStream.IsWriting);

			var builder = blockStream.Owner as EraFileBuilder;

			BuildFileNameMaps(builder != null ? builder.VerboseOutput : null);
			bool success = BuildFileNamesTable(blockStream);
			for (int x = FileChunksFirstIndex; x < mFiles.Count && success; x++)
			{
				if (builder != null && builder.VerboseOutput != null)
				{
					builder.VerboseOutput.Write("\r\t\t{0} ", mFiles[x].EntryId.ToString("X16"));
				}

				success &= mFiles[x].Pack(blockStream, basePath, TigerHasher);
			}

			if (builder != null && builder.VerboseOutput != null)
			{
				builder.VerboseOutput.Write("\r\t\t{0} \r", new string(' ', 16));
			}

			if (success)
			{
				blockStream.AlignToBoundry(kAlignmentBit);
			}

			return success;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public void ReadPostprocess(IO.EndianStream s)
		{
			if (mFiles.Count == 0)
			{
				return;
			}

			var expander = s.Owner as EraFileExpander;
			var verboseOutput = expander != null ? expander.VerboseOutput : null;

			ReadFileNamesChunk(s);
			ValidateFileHashes(s);

			BuildFileNameMaps(verboseOutput);

			if (expander != null && !expander.ExpanderOptions.Test(EraFileExpanderOptions.DontRemoveXmlOrXmbFiles))
			{
				if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					if (verboseOutput != null)
						verboseOutput.WriteLine("Removing any XML files if their XMB counterpart exists...");

					RemoveXmlFilesWhereXmbExists(verboseOutput);
				}
				else
				{
					if (verboseOutput != null)
						verboseOutput.WriteLine("Removing any XMB files if their XML counterpart exists...");

					RemoveXmbFilesWhereXmlExists(verboseOutput);
				}
			}
		}

		void ReadFileNamesChunk(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			var filenames_chunk = mFiles[0];

			if (eraUtil != null &&
				!eraUtil.Options.Test(EraFileUtilOptions.SkipVerification))
			{
				ValidateAdler32(filenames_chunk, s);
				ValidateHashes(filenames_chunk, s);
			}

			filenames_chunk.FileName = kFileNamesTableName;

			byte[] filenames_buffer = filenames_chunk.GetBuffer(s);
			using (var ms = new System.IO.MemoryStream(filenames_buffer))
			using (var er = new IO.EndianReader(ms, s.ByteOrder))
			{
				for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
				{
					var file = mFiles[x];

					if (file.FileNameOffset != er.BaseStream.Position)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"#{0} {1} has bad filename offset {2} != {3}",
							FileIndexToListingIndex(x),
							file.EntryId.ToString("X16"),
							file.FileNameOffset.ToString("X8"),
							er.BaseStream.Position.ToString("X8")
							));
					}

					file.FileName = er.ReadString(Memory.Strings.StringStorage.CStringAscii);
				}
			}
		}

		void ValidateFileHashes(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			if (eraUtil != null &&
				eraUtil.Options.Test(EraFileUtilOptions.SkipVerification))
			{
				return;
			}

			if (eraUtil != null && eraUtil.VerboseOutput != null)
			{
				eraUtil.VerboseOutput.WriteLine("\tVerifying file hashes...");
			}

			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				var file = mFiles[x];

				if (eraUtil != null && eraUtil.VerboseOutput != null)
				{
					eraUtil.VerboseOutput.Write("\r\t\t{0} ", file.EntryId.ToString("X16"));
				}

				ValidateAdler32(file, s);
				ValidateHashes(file, s);
			}

			if (eraUtil != null && eraUtil.VerboseOutput != null)
			{
				eraUtil.VerboseOutput.Write("\r\t\t{0} \r", new string(' ', 16));
				eraUtil.VerboseOutput.WriteLine("\t\tDone");
			}
		}

		void CalculateFileCompressedDataHashes(IO.EndianStream s)
		{
			for (int x = 0/*FileChunksFirstIndex*/; x < mFiles.Count; x++)
			{
				var file = mFiles[x];

				file.ComputeHash(s, TigerHasher);
				System.Array.Copy(TigerHasher.Hash,
					file.CompressedDataTiger128, file.CompressedDataTiger128.Length);
			}
		}

		public void Serialize(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			if (s.IsWriting)
			{
				mHeader.UpdateFileCount(mFiles.Count);
			}

			mHeader.Serialize(s);

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.WriteLine("Header position end: {0}",
					s.BaseStream.Position);
				eraUtil.DebugOutput.WriteLine();
			}

			SerializeFileEntryChunks(s);

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.WriteLine();
			}
		}

		public void SerializeFileEntryChunks(IO.EndianStream s)
		{
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
			else if (s.IsWriting)
			{
				foreach (var f in mFiles)
				{
					f.Serialize(s);
				}
			}
		}
		#endregion
	};
}