﻿using System;
using System.IO;

namespace KSoft.Phoenix.Resource
{
	using CryptographyTransformType = Security.Cryptography.CryptographyTransformType;

	public abstract class EraFileUtil
		: IDisposable
	{
		/// <summary>DON'T USE ME UNLESS YOU'RE NOT KSoft.Phoenix</summary>
		public const string kExtensionEncrypted = EraFile.kExtensionEncrypted;
		/// <summary>DON'T USE ME UNLESS YOU'RE NOT KSoft.Phoenix</summary>
		public const string kExtensionDecrypted = EraFile.kExtensionDecrypted;

		/*protected*/ internal EraFile mEraFile;
		protected string mSourceFile; // filename of the source file which the util stems from (.era, .xml)
		public System.IO.TextWriter VerboseOutput { get; set; }

		protected EraFileUtil()
		{
			VerboseOutput = Console.Out;
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			VerboseOutput = null;
		}
		#endregion

		public static void Crypt(string path, string eraName, string outputPath, CryptographyTransformType transformType,
			TextWriter verboseOutput = null)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = path;

			string input_file = Path.Combine(path, eraName) + EraFile.kExtensionEncrypted;
			string output_file = Path.Combine(outputPath, eraName) + EraFile.kExtensionEncrypted;

			// If we're encrypting, the input file will be a .bin, else the output file will be a .bin
			switch(transformType)
			{
			case CryptographyTransformType.Decrypt:
				output_file += EraFile.kExtensionDecrypted;
				break;

			case CryptographyTransformType.Encrypt:
				input_file += EraFile.kExtensionDecrypted;
				break;

			default:
				throw new KSoft.Debug.UnreachableException(transformType.ToString());
			}

			if (verboseOutput != null)
			{
				verboseOutput.WriteLine("Input:  {0}", input_file);
				verboseOutput.WriteLine("Output: {0}", output_file);
			}

			using (var er_fs = File.OpenRead(input_file))
			using (var er = new IO.EndianReader(er_fs, Shell.EndianFormat.Big))
			using (var ew_fs = new FileStream(output_file, FileMode.Create, FileAccess.Write))
			using (var ew = new IO.EndianWriter(ew_fs, Shell.EndianFormat.Big))
			{
				var tea = new Security.Cryptography.PhxTEA(er, ew);
				tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyEra);

				switch (transformType)
				{
				case CryptographyTransformType.Decrypt:
					tea.Decrypt();
					break;

				case CryptographyTransformType.Encrypt:
					tea.Encrypt();
					break;
				}
			}
		}
	};
}