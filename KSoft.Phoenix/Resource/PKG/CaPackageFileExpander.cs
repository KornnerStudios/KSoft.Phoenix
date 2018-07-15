using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageFileExpanderOptions
	{
		/// <summary>Only the PKG's file listing (.xml) is generated</summary>
		OnlyDumpListing,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DontOverwriteExistingFiles,
		DontLoadEntirePkgIntoMemory,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public sealed class CaPackageFileExpander
		: CaPackageFileUtil
	{
		Stream mPkgBaseStream;
		IO.EndianStream mPkgStream;

		/// <see cref="CaPackageFileExpanderOptions"/>
		public Collections.BitVector32 ExpanderOptions;

		public CaPackageFileExpander(string pkgPath)
		{
			base.mSourceFile = pkgPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref mPkgBaseStream);
			Util.DisposeAndNull(ref mPkgStream);
		}

		#region Reading
		public bool Read()
		{
			bool result = true;

			try { result &= ReadPkgFromFile(); }
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while trying to read the PKG: {0}", ex);
				result = false;
			}

			return result;
		}

		bool ReadPkgFromFile()
		{
			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Opening and reading PKG file {0}...",
					mSourceFile);

			if (ExpanderOptions.Test(CaPackageFileExpanderOptions.DontLoadEntirePkgIntoMemory))
				mPkgBaseStream = File.OpenRead(mSourceFile);
			else
			{
				byte[] ecf_bytes = File.ReadAllBytes(mSourceFile);

				mPkgBaseStream = new MemoryStream(ecf_bytes, writable: false);
			}

			mPkgStream = new IO.EndianStream(mPkgBaseStream, Shell.EndianFormat.Little, this, permissions: FileAccess.Read);
			mPkgStream.StreamMode = FileAccess.Read;

			return ReadPkgFromStream();
		}

		bool ReadPkgFromStream()
		{
			bool result = true;

			result = CaPackageFile.VerifyIsPkg(mPkgStream.Reader);
			if (!result)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tFailed: File is either not even an PKG file, or corrupt");
			}
			else
			{
				mPkgFile = new CaPackageFile();
				mPkgFile.Serialize(mPkgStream);
			}

			return result;
		}
		#endregion

		#region Expanding
		public bool ExpandTo(string workPath, string listingName)
		{
			if (mPkgFile == null)
				return false;

			if (!Directory.Exists(workPath))
				Directory.CreateDirectory(workPath);

			bool result = true;

			if (ProgressOutput != null)
				ProgressOutput.WriteLine("Outputting listing...");

			try
			{
				PopulatePkgDefinitionFromPkgFile(workPath);
				SaveListing(workPath, listingName);
			}
			catch (Exception ex)
			{
				if (VerboseOutput != null)
					VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !ExpanderOptions.Test(CaPackageFileExpanderOptions.OnlyDumpListing))
			{
				if (ProgressOutput != null)
					ProgressOutput.WriteLine("Expanding PKG to {0}...", workPath);

				try
				{
					ExpandEntriesToFiles(workPath);
				}
				catch (Exception ex)
				{
					if (VerboseOutput != null)
						VerboseOutput.WriteLine("\tEncountered an error while expanding PKG: {0}", ex);
					result = false;
				}

				if (ProgressOutput != null)
					ProgressOutput.WriteLine("Done");
			}

			mPkgStream.Close();

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listing_filename = Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("PkgFile", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Write;

				PkgDefinition.Serialize(xml);

				xml.Document.Save(listing_filename + CaPackageFileDefinition.kFileExtension);
			}
		}

		void PopulatePkgDefinitionFromPkgFile(string workPath)
		{
			foreach (var entry in mPkgFile.FileEntries)
			{
				PkgDefinition.FileNames.Add(entry.Name);
			}
		}

		void ExpandEntriesToFiles(string workPath)
		{
			foreach (var entry in mPkgFile.FileEntries)
			{
				try
				{
					ExpandEntryToFile(workPath, entry);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ExpandEntriesToFiles failed on {0} in {1}",
						entry.Name, mPkgStream.StreamName
					), e);
				}
			}
		}

		void ExpandEntryToFile(string workPath, CaPackageEntry entry)
		{
			string file_path = Path.Combine(workPath, entry.Name);

			if (!ExpanderOptions.Test(CaPackageFileExpanderOptions.DontOverwriteExistingFiles))
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
				var entry_bytes = mPkgFile.ReadEntryBytes(mPkgStream, entry);
				fs.Write(entry_bytes, 0, entry_bytes.Length);
			}
		}
		#endregion
	};
}

