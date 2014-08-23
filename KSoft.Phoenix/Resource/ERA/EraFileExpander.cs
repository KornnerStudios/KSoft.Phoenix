using System;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	[Flags]
	public enum EraFileExpanderOptions
	{
		OnlyDumpListing = 1<<0,
		DontOverwriteExistingFiles = 1<<1,
		DontTranslateXmbFiles = 1<<2,
		DecompressUIFiles = 1<<3,
		TranslateGfxFiles = 1<<4,
	};

	public sealed class EraFileExpander
		: EraFileUtil
	{
		public const string kNameExtension = ".era.bin";

		IO.EndianStream mEraStream;

		public EraFileExpanderOptions Options { get; private set; }

		public EraFileExpander(string eraPath, EraFileExpanderOptions options = 0)
		{
			mSourceFile = eraPath;
			Options = options;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref mEraStream);
		}

		bool ReadEra()
		{
			const FA k_mode = FA.Read;

			bool result = true;

			VerboseOutput.WriteLine("Opening and reading ERA file {0}...", mSourceFile);

			mEraStream = new IO.EndianStream(System.IO.File.OpenRead(mSourceFile), Shell.EndianFormat.Big, this, permissions: k_mode);
			mEraStream.StreamMode = k_mode;

			result = EraFileHeader.VerifyIsEraAndDecrypted(mEraStream.Reader);
			if (!result)
				VerboseOutput.WriteLine("\tFailed: File is either not decrypted, corrupt, or not even an ERA");
			else
			{
				mEraStream.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

				mEraFile = new EraFile();
				mEraFile.Serialize(mEraStream);
				mEraFile.ReadPostprocess(mEraStream);
			}

			return result;
		}
		public bool Read()
		{
			bool result = true;

			try { result &= ReadEra(); }
			catch (Exception ex)
			{
				VerboseOutput.WriteLine("\tEncountered an error while trying to read the ERA: {0}", ex);
				result = false;
			}

			return result;
		}

		void SaveListing(string path, string listingName)
		{
			string listing_filename = System.IO.Path.Combine(path, listingName);

			var root = new System.Xml.XmlDocument();
			var e = root.CreateElement("EraArchive");
			root.AppendChild(e);
			using (var xml = new IO.XmlElementStream(root, root.DocumentElement, FA.Write, this))
				mEraFile.WriteDefinition(xml);
			root.Save(listing_filename + EraFileBuilder.kNameExtension);
		}
		public bool ExpandTo(string path, string listingName)
		{
			if (mEraFile == null)
				return false;

			if (!System.IO.Directory.Exists(path))
				System.IO.Directory.CreateDirectory(path);

			bool result = true;

			VerboseOutput.WriteLine("Outputting listing...");
			try { SaveListing(path, listingName); }
			catch (Exception ex)
			{
				VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !Options.HasFlag(EraFileExpanderOptions.OnlyDumpListing))
			{
				VerboseOutput.WriteLine("Expanding archive to {0}...", path);

				try { mEraFile.ExpandTo(mEraStream, path); }
				catch (Exception ex)
				{
					VerboseOutput.WriteLine("\tEncountered an error while expanding archive: {0}", ex);
					result = false;
				}

				VerboseOutput.WriteLine("Done");
			}

			mEraStream.Close();

			return result;
		}
	};
}