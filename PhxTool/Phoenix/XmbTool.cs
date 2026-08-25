using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Options;

using Single24DumpInfo = KSoft.Phoenix.Xmb.Single24DumpInfo;

namespace KSoft.Tool.Phoenix
{
	sealed class XmbTool : ProgramBase
	{
		protected override Environment ProgramEnvironment => Environment.Phx;
		public static void MainEntryPoint(string helpName, List<string> args)
		{
			var prog = new XmbTool();
			prog.MainImpl(helpName, args);
		}

		enum Mode
		{
			None,

			DumpSingle24Values,
		};
		static string GetValidModes()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid modes: ");

			sb.Append(Mode.DumpSingle24Values.ToString().ToLowerInvariant()).Append(',');

			return sb.ToString();
		}

		Mode mMode;
		string? mPath,
			mOutputPath, mSwitches;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{"mode=", GetValidModes(),
					v => Program.ParseEnum(v, out mMode) },
				{"path=", "Source directory or file",
					v => mPath = v },
				{"out:", "Output directory. Defaults to the source directory if blank",
					v => mOutputPath = v },
				{"switches:", "Mode specific switches",
					v => mSwitches = v },
			};
			InitializeOptionArgShowHelp();
		}

		#region ValidateArgs
		bool ValidateArgsDumpSingle24Values()
		{
			if (string.IsNullOrWhiteSpace(mPath))
			{
				Console.WriteLine("Error: Invalid path");
				return false;
			}

			return true;
		}
		protected override bool ValidateArgs()
		{
			return mMode switch
			{
				Mode.DumpSingle24Values => ValidateArgsDumpSingle24Values(),
				_ => true,
			};
		}
		#endregion

		void MainImpl(string helpName, List<string> args)
		{
			if (!Program.TryParse(ProgramEnvironment, mOptions, args, out List<string> /*extra*/_) || mMode == Mode.None)
			{
				mArgShowHelp = true;
			}

			if (mArgShowHelp || !ValidateArgs())
			{
				Program.ShowHelp(ProgramEnvironment, mOptions, helpName);
			}
			else
			{
				try
				{
					switch (mMode)
					{
						case Mode.DumpSingle24Values:
							DumpSingle24Values(mPath!, mOutputPath, mSwitches);
							break;

						default: Program.UnavailableOption(mMode); break;
					}
				}
				catch (Exception e)
				{
					Console.Write("Exception while XMB processing: ");
					Console.WriteLine(e);
					if (System.Diagnostics.Debugger.IsAttached)
					{
						throw;
					}
				}
			}
		}

		static List<string> GetAllXmbFilePathsUnderDirectory(params string[] inputDirectories)
		{
			var xmbFiles = new List<string>();

			foreach (var inputDir in inputDirectories)
			{
				var input_files =
					from file in Directory.GetFiles(inputDir,
						KSoft.Phoenix.Resource.ResourceUtils.XmbExtensionSearchPattern,
						SearchOption.AllDirectories)
					select file;

				xmbFiles.AddRange(input_files);
			}

			return xmbFiles;
		}

		public enum DumpSingle24ValuesOptions
		{
			DumpDebugInfo,

			[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
		};
		static void DumpSingle24ValuesParseSwitches(string? switches,
			out Collections.BitVector32 options)
		{
			const string kSwitchesContext = "Xmb:DumpSingle24Values";

			options = new Collections.BitVector32();

			if (switches == null)
			{
				switches = "";
			}

			if (SwitchIsOn(switches, 0, kSwitchesContext, "Dump debug info"))
			{
				options.Set(DumpSingle24ValuesOptions.DumpDebugInfo);
			}

		}

		static void DumpSingle24Values(string workDirectory, string? outputPath, string? switches)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
			{
				outputPath = Path.GetDirectoryName(workDirectory);
			}

			DumpSingle24ValuesParseSwitches(switches,
				out Collections.BitVector32 options);

			StreamWriter? debugOutput = options.Test(DumpSingle24ValuesOptions.DumpDebugInfo)
				? new StreamWriter("debug_expander.txt")
				: null;

			List<string> xmbFilePaths = GetAllXmbFilePathsUnderDirectory(workDirectory);
			List<Single24DumpInfo> dumpInfos = new();

			// async read xmbFilePaths
			Parallel.ForEach(xmbFilePaths, xmbFilePath =>
			{
				byte[] file_bytes = File.ReadAllBytes(xmbFilePath);

				// so we dump relative paths in the xml
				string relativeXmbFilePath = Path.GetRelativePath(workDirectory, xmbFilePath);

				using (var xmb_ms = new MemoryStream(file_bytes, false))
				using (var xmb = new IO.EndianStream(xmb_ms, KSoft.Shell.EndianFormat.Big, System.IO.FileAccess.Read, name: relativeXmbFilePath))
				using (var xml_ms = new MemoryStream(IntegerMath.kMega * 1))
				{
					xmb.StreamMode = FileAccess.Read;

					Single24DumpInfo? dumpInfo = KSoft.Phoenix.Resource.ECF.EcfFileXmb.DumpSingle24Values(
						xmb, KSoft.Shell.ProcessorSize.x64); // #HACK HWDE

					if (dumpInfo == null)
					{
						return;
					}

					lock (dumpInfos)
					{
						dumpInfos.Add(dumpInfo);
					}
				}
			});

			Single24DumpInfo mergedDumpInfo = Single24DumpInfo.Merge(dumpInfos);

			const string kRootName = "Single24Dumps";
			using (var s = IO.XmlElementStream.CreateForWrite(kRootName))
			{
				// #TODO add XmlDeclaration support to XmlElementStream
				s.Document.InsertBefore(s.Document.CreateXmlDeclaration("1.0", "utf-8", null), s.Document.FirstChild);

				//Serialize(s, dumpInfos);
				mergedDumpInfo.Serialize(s);

				var sb = new System.Text.StringBuilder();
				sb.AppendLine();
				sb.AppendLine("static readonly KeyValuePair<uint, float>[] kSingle24BitsAndFloats =");
				sb.AppendLine("[");
				foreach (var entry in mergedDumpInfo.Entries)
				{
					string kvpCode = string.Create(KSoft.Util.InvariantCultureInfo,
						$"new(0x{entry.Single24Bits:X6}, {entry.FloatValue}f),");

					sb.Append('\t');
					sb.Append(kvpCode);
					// 30 being the longest kvpCode length we expect, from cleansing_air_impact_large_a.vis.xmb's 0x1618DF
					int spacesToDescription = 31 - kvpCode.Length;
					if (spacesToDescription > 0)
					{
						sb.Append(' ', spacesToDescription);
					}
					sb.AppendLine($"// {entry.Description}");
				}
				sb.AppendLine("];");

				var cdataElement = s.Document.CreateElement("Single24BitsAndFloats");
				s.Document[kRootName]!.AppendChild(cdataElement);
				var cdata = s.Document.CreateCDataSection(sb.ToString());
				cdataElement.AppendChild(cdata);

				s.Document.Save(Path.Combine(outputPath!, "_Single24Dumps.xml"));
			}

			debugOutput?.Close();
		}

		static void Serialize<TDoc, TCursor>(IO.TagElementTextStream<TDoc, TCursor> s, List<Single24DumpInfo> dumpInfos)
			where TDoc : class
			where TCursor : class
		{
			s.StreamableElements("File", dumpInfos);
		}
	};
}
