using System;
using System.Collections.Generic;
using System.IO;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	public sealed class EraFileBuilder
		: EraFileUtil
	{
		/// <summary>Extension of the file listing used to build ERAs</summary>
		public const string kNameExtension = ".xml";

		public EraFileBuilder(string listingPath)
		{
			if (System.IO.Path.GetExtension(listingPath) != kNameExtension)
				listingPath += kNameExtension;

			mSourceFile = listingPath;
		}

		bool ReadInternal()
		{
			bool result = true;

			VerboseOutput.WriteLine("Trying to read source listing {0}...", mSourceFile);

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
				VerboseOutput.WriteLine("\tFailed!");

			return result;
		}
		public bool Read() // read the listing definition
		{
			bool result = true;

			try { result &= ReadInternal(); }
			catch (Exception ex)
			{
				VerboseOutput.WriteLine("\tEncountered an error while trying to read listing: {0}", ex);
				result = false;
			}

			return result;
		}

		bool BuildInternal(string path, string eraName, string outputPath)
		{
			const FA k_mode = FA.Write;
			const int k_initial_buffer_size = 24 * IntegerMath.kMega; // 24MB

			VerboseOutput.WriteLine("Building {0} to {1}...", eraName, outputPath);

			VerboseOutput.WriteLine("\tAllocating memory...");
			bool result = true;
			using (var ms = new MemoryStream(k_initial_buffer_size))
			using (var era_memory = new IO.EndianStream(ms, Shell.EndianFormat.Big, this, permissions: k_mode))
			{
				era_memory.StreamMode = k_mode;
				era_memory.VirtualAddressTranslationInitialize(Shell.ProcessorSize.x32);

				ms.Seek(mEraFile.CalculateHeaderAndFileChunksSize(), SeekOrigin.Begin);

				VerboseOutput.WriteLine("\tPacking files...");
				result &= mEraFile.Build(era_memory, path);

				if (result)
				{
					VerboseOutput.WriteLine("\tFinializing...");

					ms.Seek(0, SeekOrigin.Begin);
					mEraFile.Serialize(era_memory);

					string era_filename = Path.Combine(outputPath, eraName) + EraFileExpander.kNameExtension;
					using (var fs = new FileStream(era_filename, FileMode.Create, FA.Write))
						ms.WriteTo(fs);
				}
			}
			return result;
		}
		/// <summary>Builds the actual ERA file</summary>
		/// <param name="path">Base path of the ERA's files (defined by the listing xml)</param>
		/// <param name="eraName">Name of the final ERA file (without any directory or extension data)</param>
		/// <param name="outputPath">(Optional) The path to output the final ERA file. Defaults to <paramref name="path"/></param>
		/// <returns>True if all build operations were successful, false otherwise</returns>
		public bool Build(string path, string eraName, string outputPath = null)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = path;

			bool result = true;

			try { BuildInternal(path, eraName, outputPath); }
			catch (Exception ex)
			{
				VerboseOutput.WriteLine("\tEncountered an error while building the archive: {0}", ex);
				result = false;
			}

			return result;
		}
	};
}