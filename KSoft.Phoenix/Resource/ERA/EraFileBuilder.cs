using System;
using System.Collections.Generic;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	public enum EraFileBuilderOptions
	{
		Encrypt,
		AlwaysUseXmlOverXmb,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class EraFileBuilder
		: EraFileUtil
	{
		/// <summary>Extension of the file listing used to build ERAs</summary>
		public const string kNameExtension = ".eradef";

		/// <see cref="EraFileBuilderOptions"/>
		public Collections.BitVector32 BuilderOptions;

		public EraFileBuilder(string listingPath)
		{
			if (System.IO.Path.GetExtension(listingPath) != kNameExtension)
				listingPath += kNameExtension;

			mSourceFile = listingPath;
		}

		bool ReadInternal()
		{
			bool result = true;

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Trying to read source listing {0}...", mSourceFile);

			if (!File.Exists(mSourceFile))
				result = false;
			else
			{
				mEraFile = new EraFile();

				using (var xml = new IO.XmlElementStream(mSourceFile, FA.Read, this))
				{
					xml.InitializeAtRootElement();
					result &= mEraFile.ReadDefinition(xml);
				}
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

		void EncryptFileBytes(byte[] eraBytes, int eraBytesLength)
		{
			using (var era_in_ms = new System.IO.MemoryStream(eraBytes, 0, eraBytesLength, writable: false))
			using (var era_out_ms = new System.IO.MemoryStream(eraBytes, 0, eraBytesLength, writable: true))
			using (var era_reader = new IO.EndianReader(era_in_ms, Shell.EndianFormat.Big))
			using (var era_writer = new IO.EndianWriter(era_out_ms, Shell.EndianFormat.Big))
			{
				CryptStream(era_reader, era_writer,
					Security.Cryptography.CryptographyTransformType.Encrypt);
			}
		}

		bool BuildInternal(string workPath, string eraName, string outputPath)
		{
			string era_filename = Path.Combine(outputPath, eraName);
			if (!BuilderOptions.Test(EraFileBuilderOptions.Encrypt))
			{
				era_filename += EraFileExpander.kNameExtension;
			}
			else
			{
				era_filename += EraFileBuilder.kExtensionEncrypted;
			}

			if (System.IO.File.Exists(era_filename))
			{
				var attrs = System.IO.File.GetAttributes(era_filename);
				if (attrs.HasFlag(FileAttributes.ReadOnly))
					throw new IOException("ERA file is readonly, can't build: " + era_filename);
			}

			if (BuilderOptions.Test(EraFileBuilderOptions.AlwaysUseXmlOverXmb))
			{
				if (ProgressOutput != null)
					ProgressOutput.WriteLine("Finding XML files to use over XMB references...");

				mEraFile.TryToReferenceXmlOverXmbFies(workPath, VerboseOutput);
			}

			const FA k_mode = FA.Write;
			const int k_initial_buffer_size = 24 * IntegerMath.kMega; // 24MB

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Building {0} to {1}...", eraName, outputPath);

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("\tAllocating memory...");
			bool result = true;
			using (var ms = new MemoryStream(k_initial_buffer_size))
			using (var era_memory = new IO.EndianStream(ms, Shell.EndianFormat.Big, this, permissions: k_mode))
			{
				era_memory.StreamMode = k_mode;
				// we can use our custom VAT system to generate relative-offset (to a given chunk) information which ECFs use
				era_memory.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

				// create null bytes for the header and embedded file chunk descriptors
				// used to just use Seek to do this, but it doesn't update Length.
				long preamble_size = mEraFile.CalculateHeaderAndFileChunksSize();
				ms.SetLength(preamble_size);
				ms.Seek(preamble_size, SeekOrigin.Begin);

				// now we can start embedding the files
				if (ProgressOutput != null)
					ProgressOutput.WriteLine("\tPacking files...");
				result &= mEraFile.Build(era_memory, workPath);

				if (result)
				{
					if (ProgressOutput != null)
						ProgressOutput.WriteLine("\tFinializing...");

					// seek back to the start of the ERA and write out the finalized header and file chunk descriptors
					ms.Seek(0, SeekOrigin.Begin);
					mEraFile.Serialize(era_memory);

					// Right now we don't actually perform any file removing (eg, duplicates) until EraFile.Build so
					// we also allow the written size to be LESS THAN the assumed preamble size
					Contract.Assert(era_memory.BaseStream.Position <= preamble_size,
						"Written ERA header size is greater than what we calculated");

					// finally, bake the ERA memory stream into a file
					if (BuilderOptions.Test(EraFileBuilderOptions.Encrypt))
					{
						if (ProgressOutput != null)
							ProgressOutput.WriteLine("\tEncrypting...");

						var era_bytes = ms.GetBuffer();
						EncryptFileBytes(era_bytes, (int)ms.Length);
					}
					else // not encrypted
					{
					}

					using (var fs = new FileStream(era_filename, FileMode.Create, FA.Write))
						ms.WriteTo(fs);
				}
			}
			return result;
		}
		/// <summary>Builds the actual ERA file</summary>
		/// <param name="workPath">Base path of the ERA's files (defined by the listing xml)</param>
		/// <param name="eraName">Name of the final ERA file (without any directory or extension data)</param>
		/// <param name="outputPath">(Optional) The path to output the final ERA file. Defaults to <paramref name="workPath"/></param>
		/// <returns>True if all build operations were successful, false otherwise</returns>
		public bool Build(string workPath, string eraName, string outputPath = null)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = workPath;

			bool result = true;

			try { BuildInternal(workPath, eraName, outputPath); }
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while building the archive: {0}", ex);
				result = false;
			}

			return result;
		}
	};
}