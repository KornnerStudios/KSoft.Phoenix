using System;
using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfFileExpanderOptions
	{
		/// <summary>Only the ECF's file listing (.xml) is generated</summary>
		OnlyDumpListing,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DontOverwriteExistingFiles,
		DontSaveChunksToFiles,
		DontLoadEntireEcfIntoMemory,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class EcfFileExpander
		: EcfFileUtil
	{
		Stream mEcfBaseStream;
		IO.EndianStream mEcfStream;

		/// <see cref="EcfFileExpanderOptions"/>
		public Collections.BitVector32 ExpanderOptions;

		public EcfFileExpander(string ecfPath)
		{
			mSourceFile = ecfPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref mEcfBaseStream);
			Util.DisposeAndNull(ref mEcfStream);
		}

		#region Reading
		public bool Read()
		{
			bool result = true;

			try { result &= ReadEcfFromFile(); }
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while trying to read the ECF: {0}", ex);
				result = false;
			}

			return result;
		}

		bool ReadEcfFromFile()
		{
			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Opening and reading ECF file {0}...",
					mSourceFile);

			if (ExpanderOptions.Test(EcfFileExpanderOptions.DontLoadEntireEcfIntoMemory))
				mEcfBaseStream = File.OpenRead(mSourceFile);
			else
			{
				byte[] ecf_bytes = File.ReadAllBytes(mSourceFile);

				mEcfBaseStream = new MemoryStream(ecf_bytes, writable: false);
			}

			mEcfStream = new IO.EndianStream(mEcfBaseStream, Shell.EndianFormat.Big, this, permissions: FileAccess.Read);
			mEcfStream.StreamMode = FileAccess.Read;

			return ReadEcfFromStream();
		}

		bool ReadEcfFromStream()
		{
			bool result = true;

			result = EcfHeader.VerifyIsEcf(mEcfStream.Reader);
			if (!result)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tFailed: File is either not even an ECF-based file, or corrupt");
			}
			else
			{
				mEcfFile = new EcfFile();
				mEcfFile.Serialize(mEcfStream);
			}

			return result;
		}
		#endregion

		#region Expanding
		bool WriteChunksToFile { get { return ExpanderOptions.Test(EcfFileExpanderOptions.DontSaveChunksToFiles) == false; } }

		public bool ExpandTo(string workPath, string listingName)
		{
			if (mEcfFile == null)
				return false;

			if (!Directory.Exists(workPath))
				Directory.CreateDirectory(workPath);

			bool result = true;

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Outputting listing...");

			try
			{
				PopulateEcfDefinitionFromEcfFile(workPath);
				SaveListing(workPath, listingName);
			}
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !ExpanderOptions.Test(EcfFileExpanderOptions.OnlyDumpListing) && WriteChunksToFile)
			{
				if (ProgressOutput != null)
					ProgressOutput.WriteLine("Expanding ECF to {0}...", workPath);

				try
				{
					ExpandChunksToFiles();
				}
				catch (Exception ex)
				{
					if (VerboseOutput != null)
						VerboseOutput.WriteLine("\tEncountered an error while expanding ECF: {0}", ex);
					result = false;
				}

				if (ProgressOutput != null)
					ProgressOutput.WriteLine("Done");
			}

			mEcfStream.Close();

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listing_filename = Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("EcfFile", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Write;

				EcfDefinition.Serialize(xml);

				xml.Document.Save(listing_filename + EcfFileDefinition.kFileExtension);
			}
		}

		void PopulateEcfDefinitionFromEcfFile(string workPath)
		{
			EcfDefinition.WorkingDirectory = workPath;
			EcfDefinition.Initialize(mSourceFile);

			mEcfFile.CopyHeaderDataTo(EcfDefinition);

			int raw_chunk_index = 0;
			foreach (var rawChunk in mEcfFile)
			{
				var chunk = EcfDefinition.Add(rawChunk, raw_chunk_index++);

				if (WriteChunksToFile)
					chunk.SetFilePathFromParentNameAndId();
			}

			if (!WriteChunksToFile)
				ReadEcfChunksToDefinitionBytes();
		}

		void ReadEcfChunksToDefinitionBytes()
		{
			foreach (var chunk in EcfDefinition.Chunks)
			{
				var raw_chunk = mEcfFile.GetChunk(chunk.RawChunkIndex);

				try
				{
					var chunk_bytes = raw_chunk.GetBuffer(mEcfStream);
					chunk.SetFileBytes(chunk_bytes);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ReadEcfChunksToDefinitionBytes failed on chunk {0} in {1}",
						chunk.Id.ToString("X8"), mEcfStream.StreamName
					), e);
				}
			}
		}

		void ExpandChunksToFiles()
		{
			foreach (var chunk in EcfDefinition.Chunks)
			{
				var raw_chunk = mEcfFile.GetChunk(chunk.RawChunkIndex);

				try
				{
					ExpandChunkToFile(chunk, raw_chunk);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ExpandChunksToFiles failed on chunk {0} in {1}",
						chunk.Id.ToString("X8"), mEcfStream.StreamName
					), e);
				}
			}
		}

		void ExpandChunkToFile(EcfFileChunkDefinition chunk, EcfChunk rawChunk)
		{
			string file_path = EcfDefinition.GetChunkAbsolutePath(chunk);

			if (!ExpanderOptions.Test(EcfFileExpanderOptions.DontOverwriteExistingFiles))
			{
				if (File.Exists(file_path))
				{
					if (VerboseOutput != null)
						VerboseOutput.WriteLine("\tSkipping chunk, output file already exists: {0}", file_path);

					return;
				}
			}

			using (var fs = File.OpenWrite(file_path))
			{
				var chunk_bytes = rawChunk.GetBuffer(mEcfStream);
				fs.Write(chunk_bytes, 0, chunk_bytes.Length);
			}
		}
		#endregion
	};
}