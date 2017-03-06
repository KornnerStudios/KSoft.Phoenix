using System;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	public enum EraFileExpanderOptions
	{
		/// <summary>Only the ERA's file listing (.xml) is generated</summary>
		OnlyDumpListing,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DontOverwriteExistingFiles,
		/// <summary>Don't perform XMB to XML translations</summary>
		DontTranslateXmbFiles,
		/// <summary>Decompresses Scaleform data</summary>
		DecompressUIFiles,
		/// <summary>Translates GFX files to SWF</summary>
		TranslateGfxFiles,
		Decrypt,
		DontLoadEntireEraIntoMemory,
		DontRemoveXmlOrXmbFiles,
		IgnoreNonDataFiles,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class EraFileExpander
		: EraFileUtil
	{
		public const string kNameExtension = ".era.bin";

		System.IO.Stream mEraBaseStream;
		IO.EndianStream mEraStream;

		/// <see cref="EraFileExpanderOptions"/>
		public Collections.BitVector32 ExpanderOptions;

		public EraFileExpander(string eraPath)
		{
			mSourceFile = eraPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref mEraStream);
			Util.DisposeAndNull(ref mEraBaseStream);
		}

		bool ReadEraFromStream()
		{
			bool result = true;

			result = EraFileHeader.VerifyIsEraAndDecrypted(mEraStream.Reader);
			if (!result)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tFailed: File is either not decrypted, corrupt, or not even an ERA");
			}
			else
			{
				mEraStream.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

				mEraFile = new EraFile();
				mEraFile.Serialize(mEraStream);
				mEraFile.ReadPostprocess(mEraStream);
			}

			return result;
		}

		bool ReadEraFromFile()
		{
			if (VerboseOutput != null)
				VerboseOutput.WriteLine("Opening and reading ERA file {0}...",
					mSourceFile);

			if (ExpanderOptions.Test(EraFileExpanderOptions.DontLoadEntireEraIntoMemory))
				mEraBaseStream = System.IO.File.OpenRead(mSourceFile);
			else
			{
				byte[] era_bytes = System.IO.File.ReadAllBytes(mSourceFile);
				if (ExpanderOptions.Test(EraFileExpanderOptions.Decrypt))
				{
					if (VerboseOutput != null)
						VerboseOutput.WriteLine("Decrypting...");

					DecryptFileBytes(era_bytes);
				}

				mEraBaseStream = new System.IO.MemoryStream(era_bytes, writable: false);
			}

			mEraStream = new IO.EndianStream(mEraBaseStream, Shell.EndianFormat.Big, this, permissions: FA.Read);
			mEraStream.StreamMode = FA.Read;

			return ReadEraFromStream();
		}

		void DecryptFileBytes(byte[] eraBytes)
		{
			using (var era_in_ms = new System.IO.MemoryStream(eraBytes, writable: false))
			using (var era_out_ms = new System.IO.MemoryStream(eraBytes, writable: true))
			using (var era_reader = new IO.EndianReader(era_in_ms, Shell.EndianFormat.Big))
			using (var era_writer = new IO.EndianWriter(era_out_ms, Shell.EndianFormat.Big))
			{
				CryptStream(era_reader, era_writer,
					Security.Cryptography.CryptographyTransformType.Decrypt);
			}
		}

		public bool Read()
		{
			bool result = true;

			try { result &= ReadEraFromFile(); }
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while trying to read the ERA: {0}", ex);
				result = false;
			}

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listing_filename = System.IO.Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("EraArchive", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FA.Write;

				mEraFile.WriteDefinition(xml);

				xml.Document.Save(listing_filename + EraFileBuilder.kNameExtension);
			}
		}
		public bool ExpandTo(string workPath, string listingName)
		{
			if (mEraFile == null)
				return false;

			if (!System.IO.Directory.Exists(workPath))
				System.IO.Directory.CreateDirectory(workPath);

			bool result = true;

			if (VerboseOutput != null)
				VerboseOutput.WriteLine("Outputting listing...");

			try { SaveListing(workPath, listingName); }
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !ExpanderOptions.Test(EraFileExpanderOptions.OnlyDumpListing))
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("Expanding archive to {0}...", workPath);

				try { mEraFile.ExpandTo(mEraStream, workPath); }
				catch (Exception ex)
				{
					if (VerboseOutput != null)
						VerboseOutput.WriteLine("\tEncountered an error while expanding archive: {0}", ex);
					result = false;
				}

				if (VerboseOutput != null)
					VerboseOutput.WriteLine("Done");
			}

			mEraStream.Close();

			return result;
		}
	};
}