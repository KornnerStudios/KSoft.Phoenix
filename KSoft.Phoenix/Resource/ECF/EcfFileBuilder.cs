using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfFileBuilderOptions
	{
		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class EcfFileBuilder
		: EcfFileUtil
	{
		/// <see cref="EcfFileBuilderOptions"/>
		public Collections.BitVector32 BuilderOptions;

		public EcfFileBuilder(string listingPath)
		{
			if (Path.GetExtension(listingPath) != EcfFileDefinition.kFileExtension)
				listingPath += EcfFileDefinition.kFileExtension;

			mSourceFile = listingPath;
		}

		#region Reading
		bool ReadInternal()
		{
			bool result = true;

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Trying to read source listing {0}...", mSourceFile);

			if (!File.Exists(mSourceFile))
				result = false;
			else
			{
				using (var xml = new IO.XmlElementStream(mSourceFile, FA.Read, this))
				{
					xml.InitializeAtRootElement();
					EcfDefinition.Serialize(xml);
				}

				EcfDefinition.CullChunksPossiblyWithoutFileData((chunkIndex, chunk) =>
				{
					if (VerboseOutput != null)
						VerboseOutput.WriteLine("\t\tCulling chunk #{0} since it has no associated file data",
							chunkIndex);
				});
			}

			if (result == false)
			{
				if (ProgressOutput != null)
					ProgressOutput.WriteLine("\tFailed!");
			}

			return result;
		}
		public bool Read() // read the listing definition
		{
			bool result = true;

			try { result &= ReadInternal(); }
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while trying to read listing: {0}", ex);
				result = false;
			}

			return result;
		}
		#endregion

		#region Building
		public bool Build(string workPath, string outputPath = null)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = workPath;

			bool result = true;

			try
			{
				result = BuildInternal(workPath, outputPath);
			} catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while building the ECF: {0}", ex);
				result = false;
			}

			return result;
		}

		bool BuildInternal(string workPath, string outputPath)
		{
			EcfDefinition.WorkingDirectory = workPath;

			string ecf_name = EcfDefinition.EcfName;
			if (ecf_name.IsNotNullOrEmpty())
			{
				ecf_name = Path.GetFileNameWithoutExtension(mSourceFile);
			}

			string ecf_filename = Path.Combine(outputPath, ecf_name);
			#if false // I'm no longer doing this since we don't strip the file ext off listing when expanding
			// #TODO I bet a user could forget to include the preceding dot
			if (EcfDefinition.EcfFileExtension.IsNotNullOrEmpty())
				ecf_filename += EcfDefinition.EcfFileExtension;
			#endif

			if (File.Exists(ecf_filename))
			{
				var attrs = File.GetAttributes(ecf_filename);
				if (attrs.HasFlag(FileAttributes.ReadOnly))
					throw new IOException("ECF file is readonly, can't build: " + ecf_filename);
			}

			mEcfFile = new EcfFile();
			mEcfFile.SetupHeaderAndChunks(EcfDefinition);

			const FA k_mode = FA.Write;
			const int k_initial_buffer_size = 8 * IntegerMath.kMega; // 8MB

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Building {0} to {1}...", ecf_name, outputPath);

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("\tAllocating memory...");
			bool result = true;
			using (var ms = new MemoryStream(k_initial_buffer_size))
			using (var ecf_memory = new IO.EndianStream(ms, Shell.EndianFormat.Big, this, permissions: k_mode))
			{
				ecf_memory.StreamMode = k_mode;
				ecf_memory.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

				long preamble_size = mEcfFile.CalculateHeaderAndChunkEntriesSize();
				ms.SetLength(preamble_size);
				ms.Seek(preamble_size, SeekOrigin.Begin);

				// now we can start embedding the files
				if (ProgressOutput != null)
					ProgressOutput.WriteLine("\tPacking chunks...");

				result = result && PackChunks(ecf_memory);

				if (result)
				{
					if (ProgressOutput != null)
						ProgressOutput.WriteLine("\tFinializing...");

					// seek back to the start of the ECF and write out the finalized header and chunk descriptors
					ms.Seek(0, SeekOrigin.Begin);
					mEcfFile.Serialize(ecf_memory);

					Contract.Assert(ecf_memory.BaseStream.Position == preamble_size,
						"Written ECF header size is NOT EQUAL what we calculated");

					// Update sizes and checksums
					ms.Seek(0, SeekOrigin.Begin);
					mEcfFile.SerializeBegin(ecf_memory, isFinalizing: true);
					mEcfFile.SerializeEnd(ecf_memory);

					// finally, bake the ECF memory stream into a file

					using (var fs = new FileStream(ecf_filename, FileMode.Create, FA.Write))
						ms.WriteTo(fs);
				}
			}

			return result;
		}

		bool PackChunks(IO.EndianStream ecfStream)
		{
			bool success = true;

			foreach (var chunk in EcfDefinition.Chunks)
			{
				if (ProgressOutput != null)
					ProgressOutput.Write("\r\t\t{0} ", chunk.Id.ToString("X16"));

				success = success && BuildChunkToStream(ecfStream, chunk);

				if (!success)
					break;
			}

			if (success && ProgressOutput != null)
				ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));

			return success;
		}

		bool BuildChunkToStream(IO.EndianStream ecfStream, EcfFileChunkDefinition chunk)
		{
			bool success = true;

			var raw_chunk = mEcfFile.GetChunk(chunk.RawChunkIndex);

			using (var chunk_ms = EcfDefinition.GetChunkFileDataStream(chunk))
			{
				raw_chunk.BuildBuffer(ecfStream, chunk_ms);
			}

			return success;
		}
		#endregion
	};
}