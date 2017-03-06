﻿using System.Collections.Generic;
using System.IO;
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

		const int kAlignmentBit = IntegerMath.kFourKiloAlignmentBit;
		const string kFileNamesTableName = "_file_names.bin";
		static readonly Memory.Strings.StringMemoryPoolSettings kFileNamesTablePoolConfig = new Memory.Strings.
			StringMemoryPoolSettings(Memory.Strings.StringStorage.CStringAscii, false);

		private EraFileHeader mHeader = new EraFileHeader();
		private List<EraFileEntryChunk> mFiles = new List<EraFileEntryChunk>();
		private Dictionary<string, string> mLocalFiles = new Dictionary<string, string>();
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
				if (!ResourceUtils.IsXmbFile(file.FileName))
					continue;

				string xml_name = file.FileName;
				ResourceUtils.RemoveXmbExtension(ref xml_name);
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
				if (!ResourceUtils.IsXmlBasedFile(file.FileName))
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

		public void TryToReferenceXmlOverXmbFies(string workPath, TextWriter verboseOutput)
		{
			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				var file = mFiles[x];
				if (!ResourceUtils.IsXmbFile(file.FileName))
					continue;

				string xml_name = file.FileName;
				ResourceUtils.RemoveXmbExtension(ref xml_name);

				// if the user already references the XML file too, just skip doing anything
				EraFileEntryChunk xml_file;
				if (mFileNameToChunk.TryGetValue(xml_name, out xml_file))
					continue;

				// does the XML file exist?
				string xml_path = Path.Combine(workPath, xml_name);
				if (!File.Exists(xml_path))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tReplacing XMB ref with {0}",
						xml_name);

				// right now, all we should need to do to update things is remove the XMB mapping and replace it with the XML we found
				mFileNameToChunk.Remove(file.FileName);
				file.FileName = xml_name;
				mFileNameToChunk.Add(xml_name, file);
			}
		}

		#region Xml definition Streaming
		static EraFileEntryChunk GenerateFileNamesTableEntryChunk()
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
		private void ReadLocalFiles(IO.XmlElementStream s)
		{
			foreach (var n in s.ElementsByName("file"))
			{
				using (s.EnterCursorBookmark(n))
				{
					string file_name = null;
					s.ReadAttribute("name", ref file_name);

					string file_data = "";
					s.ReadCursor(ref file_data);

					if (!string.IsNullOrEmpty(file_name))
						mLocalFiles[file_name] = file_data;
				}
			}
		}

		private void WriteChunks(IO.XmlElementStream s)
		{
			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				mFiles[x].Write(s, false);
			}
		}
		private void WriteLocalFiles(IO.XmlElementStream s)
		{
			foreach (var kvp in mLocalFiles)
			{
				string file_name = kvp.Key;
				string file_data = kvp.Value;

				using (s.EnterCursorBookmark("file"))
				{
					s.WriteAttribute("name", file_name);
					s.WriteCursor(file_data);
				}
			}
		}

		public bool ReadDefinition(IO.XmlElementStream s)
		{
			mFiles.Clear();

			// first entry should always be the null terminated filenames table
			mFiles.Add(GenerateFileNamesTableEntryChunk());

			using (s.EnterCursorBookmark("Files"))
				ReadChunks(s);

			using (var bm = s.EnterCursorBookmarkOpt("LocalFiles")) if (bm.IsNotNull)
				ReadLocalFiles(s);

			AddVersionFile();

			// there should be at least one file destined for the ERA, excluding the filenames table
			return FileChunksCount != 0;
		}
		public void WriteDefinition(IO.XmlElementStream s)
		{
			using (s.EnterCursorBookmark("Files"))
				WriteChunks(s);

			using (var bm = s.EnterCursorBookmarkOpt("LocalFiles", mLocalFiles, Predicates.HasItems)) if (bm.IsNotNull)
				WriteLocalFiles(s);
		}
		#endregion

		#region Expand
		public void ExpandTo(IO.EndianStream blockStream, string workPath)
		{
			Contract.Requires(blockStream.IsReading);

			var eraExpander = (EraFileExpander)blockStream.Owner;

			if (eraExpander.VerboseOutput != null)
			{
				eraExpander.VerboseOutput.WriteLine("\tUnpacking files...");
			}

			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				var file = mFiles[x];

				if (eraExpander.VerboseOutput != null)
				{
					eraExpander.VerboseOutput.Write("\r\t\t{0} ", file.EntryId.ToString("X16"));
				}
				TryUnpack(blockStream, workPath, eraExpander, file);
			}

			if (eraExpander.VerboseOutput != null)
			{
				eraExpander.VerboseOutput.Write("\r\t\t{0} \r", new string(' ', 16));
				eraExpander.VerboseOutput.WriteLine("\t\tDone");
			}

			mDirsThatExistForUnpacking = null;
		}

		private bool TryUnpack(IO.EndianStream blockStream, string workPath, EraFileExpander expander, EraFileEntryChunk file)
		{
			if (IsIgnoredLocalFile(file.FileName))
				return false;

			string full_path = System.IO.Path.Combine(workPath, file.FileName);

			if (ResourceUtils.IsLocalScenarioFile(file.FileName))
			{
				return false;
			}
			else if (!ShouldUnpack(expander, full_path))
			{
				return false;
			}

			CreatePathForUnpacking(full_path);

			UnpackToDisk(blockStream, full_path, expander, file);
			return true;
		}

		private void UnpackToDisk(IO.EndianStream blockStream, string fullPath, EraFileExpander expander, EraFileEntryChunk file)
		{
			byte[] buffer = file.GetBuffer(blockStream);

			using (var fs = System.IO.File.Create(fullPath))
			{
				fs.Write(buffer, 0, buffer.Length);
			}

			System.IO.File.SetCreationTimeUtc(fullPath, file.FileDateTime);
			System.IO.File.SetLastWriteTimeUtc(fullPath, file.FileDateTime);

			if (ResourceUtils.IsXmbFile(fullPath))
			{
				if (!expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					var va_size = Shell.ProcessorSize.x32;
					var builtFor64Bit = expander.Options.Test(EraFileUtilOptions.x64);
					if (builtFor64Bit)
					{
						va_size = Shell.ProcessorSize.x64;
					}

					TransformXmbToXml(buffer, fullPath, blockStream.ByteOrder, va_size);
				}
			}
			else if (ResourceUtils.IsScaleformFile(fullPath))
			{
				if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DecompressUIFiles))
				{
					DecompressUIFileToDisk(buffer, fullPath);
				}
				if (expander.ExpanderOptions.Test(EraFileExpanderOptions.TranslateGfxFiles))
				{
					TransformGfxToSwfFile(buffer, fullPath);
				}
			}
		}

		private void TransformXmbToXml(byte[] eraFileEntryBuffer, string fullPath, Shell.EndianFormat byteOrder, Shell.ProcessorSize vaSize)
		{
			byte[] xmb_buffer;

			using (var xmb = new ECF.EcfFileXmb())
			using (var ms = new System.IO.MemoryStream(eraFileEntryBuffer))
			using (var es = new IO.EndianStream(ms, byteOrder, permissions: System.IO.FileAccess.Read))
			{
				es.StreamMode = System.IO.FileAccess.Read;
				xmb.Serialize(es);

				xmb_buffer = xmb.FileData;
			}

			string xmb_path = fullPath;
			ResourceUtils.RemoveXmbExtension(ref xmb_path);

			var context = new Xmb.XmbFileContext()
			{
				PointerSize = vaSize,
			};

			using (var ms = new System.IO.MemoryStream(xmb_buffer, false))
			using (var s = new IO.EndianReader(ms, byteOrder))
			{
				s.UserData = context;

				using (var xmbf = new Phoenix.Xmb.XmbFile())
				{
					xmbf.Read(s);
					xmbf.ToXml(xmb_path);
				}
			}
		}

		private void DecompressUIFileToDisk(byte[] eraFileEntryBuffer, string fullPath)
		{
			using (var ms = new System.IO.MemoryStream(eraFileEntryBuffer, false))
			using (var s = new IO.EndianReader(ms, Shell.EndianFormat.Little))
			{
				uint buffer_signature;
				if (ResourceUtils.IsScaleformBuffer(s, out buffer_signature))
				{
					int decompressed_size = s.ReadInt32();
					int compressed_size = (int)(ms.Length - ms.Position);

					byte[] decompressed_data = ResourceUtils.DecompressScaleform(eraFileEntryBuffer, decompressed_size);
					using (var fs = System.IO.File.Create(fullPath + ".bin"))
					{
						fs.Write(decompressed_data, 0, decompressed_data.Length);
					}
				}
			}
		}

		private void TransformGfxToSwfFile(byte[] eraFileEntryBuffer, string fullPath)
		{
			using (var ms = new System.IO.MemoryStream(eraFileEntryBuffer, false))
			using (var s = new IO.EndianReader(ms, Shell.EndianFormat.Little))
			{
				uint buffer_signature;
				if (ResourceUtils.IsScaleformBuffer(s, out buffer_signature))
				{
					uint swf_signature = ResourceUtils.GfxHeaderToSwf(buffer_signature);
					using (var fs = System.IO.File.Create(fullPath + ".swf"))
					using (var out_s = new IO.EndianWriter(fs, Shell.EndianFormat.Little))
					{
						out_s.Write(swf_signature);
						out_s.Write(eraFileEntryBuffer, sizeof(uint), eraFileEntryBuffer.Length - sizeof(uint));
					}
				}
			}
		}

		private HashSet<string> mDirsThatExistForUnpacking;
		private void CreatePathForUnpacking(string full_path)
		{
			if (mDirsThatExistForUnpacking == null)
				mDirsThatExistForUnpacking = new HashSet<string>();

			string folder = System.IO.Path.GetDirectoryName(full_path);
			// don't bother checking the file system if we've already encountered this folder
			if (mDirsThatExistForUnpacking.Add(folder))
			{
				if (!System.IO.Directory.Exists(folder))
				{
					System.IO.Directory.CreateDirectory(folder);
				}
			}
		}

		private bool ShouldUnpack(EraFileExpander expander, string path)
		{
			if (expander.ExpanderOptions.Test(EraFileExpanderOptions.DontOverwriteExistingFiles))
			{
				// it's an XMB file and the user didn't say NOT to translate them
				if (ResourceUtils.IsXmbFile(path) && !expander.ExpanderOptions.Test(EraFileExpanderOptions.DontTranslateXmbFiles))
				{
					ResourceUtils.RemoveXmbExtension(ref path);
				}

				if (System.IO.File.Exists(path))
				{
					return false;
				}
			}

			if (expander.ExpanderOptions.Test(EraFileExpanderOptions.IgnoreNonDataFiles))
			{
				if (!ResourceUtils.IsDataBasedFile(path))
					return false;
			}

			return true;
		}
		#endregion

		#region Build
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
				PackFileNames(blockStream, ms, filenames_chunk);

				return true;
			}
		}

		public bool Build(IO.EndianStream blockStream, string workPath)
		{
			Contract.Requires(blockStream.IsWriting);

			var builder = blockStream.Owner as EraFileBuilder;

			Contract.Assert(blockStream.BaseStream.Position == CalculateHeaderAndFileChunksSize());

			BuildFileNameMaps(builder != null ? builder.VerboseOutput : null);
			bool success = BuildFileNamesTable(blockStream);
			for (int x = FileChunksFirstIndex; x < mFiles.Count && success; x++)
			{
				var file = mFiles[x];
				if (builder != null && builder.VerboseOutput != null)
				{
					builder.VerboseOutput.Write("\r\t\t{0} ", file.EntryId.ToString("X16"));
				}

				success &= TryPack(blockStream, workPath, file);
			}

			if (builder != null && builder.VerboseOutput != null)
			{
				builder.VerboseOutput.Write("\r\t\t{0} \r", new string(' ', 16));
			}

			if (success)
			{
				blockStream.AlignToBoundry(kAlignmentBit);
			}

#if false
			if (success)
			{
				if (builder != null && !builder.Options.Test(EraFileUtilOptions.SkipVerification))
				{
					var filenames_chunk = mFiles[0];

					ValidateAdler32(filenames_chunk, blockStream);
					ValidateHashes(filenames_chunk, blockStream);

					ValidateFileHashes(blockStream);
				}
			}
#endif

			return success;
		}

		private void PackFileData(IO.EndianStream blockStream, System.IO.Stream source, EraFileEntryChunk file)
		{
			file.BuildBuffer(blockStream, source, TigerHasher);

#if false
			ValidateAdler32(file, blockStream);
			ValidateHashes(file, blockStream);
#endif
		}

		private void PackFileNames(IO.EndianStream blockStream, System.IO.MemoryStream source, EraFileEntryChunk file)
		{
			file.FileDateTime = System.DateTime.UtcNow;
			PackFileData(blockStream, source, file);
		}

		private bool TryPack(IO.EndianStream blockStream, string workPath,
			EraFileEntryChunk file)
		{
			if (mLocalFiles.ContainsKey(file.FileName))
				return TryPackLocalFile(blockStream, file);

			return TryPackFileFromDisk(blockStream, workPath, file);
		}

		private bool TryPackLocalFile(IO.EndianStream blockStream,
			EraFileEntryChunk file)
		{
			string file_data;
			if (!mLocalFiles.TryGetValue(file.FileName, out file_data))
				return false;

			byte[] file_bytes = System.Text.Encoding.ASCII.GetBytes(file_data);
			using (var ms = new System.IO.MemoryStream(file_bytes, false))
			{
				PackFileData(blockStream, ms, file);
			}

			return true;
		}

		private bool TryPackFileFromDisk(IO.EndianStream blockStream, string workPath,
			EraFileEntryChunk file)
		{
			string path = System.IO.Path.Combine(workPath, file.FileName);
			if (!System.IO.File.Exists(path))
			{
				return false;
			}

			var creation_time = System.IO.File.GetCreationTimeUtc(path);
			var write_time = System.IO.File.GetLastWriteTimeUtc(path);
			file.FileDateTime = write_time > creation_time
				? write_time
				: creation_time;

			byte[] file_bytes = System.IO.File.ReadAllBytes(path);
			using (var ms = new System.IO.MemoryStream(file_bytes, false))
			{
				PackFileData(blockStream, ms, file);
			}

			return true;
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

			BuildLocalFiles(s);
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
			using (var ms = new System.IO.MemoryStream(filenames_buffer, false))
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

		void BuildLocalFiles(IO.EndianStream s)
		{
			for (int x = FileChunksFirstIndex; x < mFiles.Count; x++)
			{
				var file = mFiles[x];
				if (!ResourceUtils.IsLocalScenarioFile(file.FileName))
					continue;

				byte[] file_bytes = file.GetBuffer(s);
				using (var ms = new System.IO.MemoryStream(file_bytes, false))
				using (var sr = new System.IO.StreamReader(ms))
				{
					string file_data = sr.ReadToEnd();

					mLocalFiles[file.FileName] = file_data;
				}
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

		#region Local file utils
		private static bool IsIgnoredLocalFile(string fileName)
		{
			if (0==string.Compare(fileName, "version.txt", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		private void AddVersionFile()
		{
			var file = new EraFileEntryChunk();
			file.CompressionType = ECF.EcfCompressionType.Stored;
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			file.FileName = "version.txt";
			file.FileDateTime = System.DateTime.UtcNow;
			string version = string.Format("{0}\n{1}\n{2}",
				assembly.FullName,
				assembly.GetName().Version,
				System.Reflection.Assembly.GetEntryAssembly().FullName);
			mLocalFiles[file.FileName] = version;
			mFiles.Add(file);
		}
		#endregion
	};
}