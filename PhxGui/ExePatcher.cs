using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KSoft;

namespace PhxGui
{
	partial class MainWindowViewModel
	{
		private void PatchGameExe(string exeFile, AcceptedFileType fileType)
		{
			ClearMessages();
			IsProcessing = true;

			var args = new ExePatching.PatchGameExeByParameters
			{
				ExeFile = exeFile,
				ExeFileType = fileType,
			};

			var task = Task.Factory.StartNew(
				//ExePatching.PatchGameExeBySha1,
				ExePatching.PatchGameExeByPatternMatching,
				args);

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.Result.StartsWith("ERROR", StringComparison.Ordinal))
				{
					MessagesText += string.Format("Patch EXE finished with errors: {0}{1}",
						Environment.NewLine,
						t.IsFaulted ? t.Exception.GetOnlyExceptionOrAll().ToString() : t.Result);
				}
				else
				{
					MessagesText = string.Format("EXE is now ready for modding use: {0}",
						t.Result);
				}

				FinishProcessing();
			}, scheduler);
		}
	};

	static class ExePatching
	{
		public sealed class PatchInfo
		{
			public string Sha1;
			public Dictionary<uint, byte[]> Patches = new Dictionary<uint,byte[]>();

			public PatchInfo(string sha1)
			{
				Sha1 = sha1;
			}

			public PatchInfo Add(uint offset, params byte[] newBytes)
			{
				Patches[offset] = newBytes;
				return this;
			}
		};
		private static List<PatchInfo> kPatches = new List<PatchInfo>();

		static ExePatching()
		{
			// Xbox360:
#if false // #TODO
			var Xbox_v1 = new PatchInfo("0723A9FBAB23DE2B0AB80656174EEF0D2ADC2D98")
				//.Add(0x6D971F, 0xE9, 0x0A, 0x01, 0x00, 0x00);
			kPatches.Add(Xbox_v1);
#endif

			// PC:
			var v1_11088_1_2 = new PatchInfo("2C1E144727CFF2AADDAE6BB71EE66B7820D3E163")
				.Add(0x6D971F, 0xE9, 0x0A, 0x01, 0x00, 0x00);
			kPatches.Add(v1_11088_1_2);

			var v1_11279_1_2 = new PatchInfo("BF664000801CAC222514B7ACE88C17768B86CC30")
				.Add(0x6DE03F, 0xE9, 0x0A, 0x01, 0x00, 0x00);
			kPatches.Add(v1_11279_1_2);
		}

		public static bool TryGetPatchInfo(string actualSha1, out PatchInfo info)
		{
			info = null;

			info = (from i in kPatches
					where i.Sha1 == actualSha1
					select i).FirstOrDefault();

			return info != null;
		}

		public sealed class PatchGameExeByParameters
		{
			public string ExeFile;
			public MainWindowViewModel.AcceptedFileType ExeFileType;

			public void BackupFile()
			{
				string extension = Path.GetExtension(ExeFile);
				string backup_file = Path.GetFileNameWithoutExtension(ExeFile);
				backup_file += "_UNTOUCHED.exe";
				backup_file = Path.ChangeExtension(backup_file, extension);
				backup_file = Path.Combine(Path.GetDirectoryName(ExeFile), backup_file);
				File.Copy(ExeFile, backup_file);
			}
		};

		public static string PatchGameExeByPatternMatching(object taskState)
		{
			var args = taskState as PatchGameExeByParameters;

			if (args.ExeFileType == MainWindowViewModel.AcceptedFileType.Xex)
			{
				return string.Format("ERROR patching XEX files is not supported: {0}",
					 args.ExeFile);
			}

			var exe_file_attrs = File.GetAttributes(args.ExeFile);
			if (exe_file_attrs.HasFlag(FileAttributes.ReadOnly))
			{
				return string.Format("ERROR Cannot patch read-only file (this tool creates a backup): {0}",
					args.ExeFile);
			}

			try
			{
				args.BackupFile();
			} catch (Exception ex)
			{
				return string.Format("ERROR Failed to create backup: {0}{1}{2}",
					 args.ExeFile,
					 Environment.NewLine,
					 ex);
			}

			var patch_pattern = new KSoft.Phoenix.zPatching.WinExePatcherProcessHeaderData();

			bool read_bytes = patch_pattern.ReadSourceExeBytes(args.ExeFile);
			if (!read_bytes)
			{
				return string.Format("ERROR Failed to read file to memory: {0}",
					args.ExeFile);
			}

			byte[] exe_file_sha1_bytes = null;
			using (var ms = new MemoryStream(patch_pattern.SourceExeBytes))
			using (var sha1_provider = new System.Security.Cryptography.SHA1CryptoServiceProvider())
			{
				exe_file_sha1_bytes = sha1_provider.ComputeHash(ms);
			}

			var exe_file_sha1 = KSoft.Text.Util.ByteArrayToString(exe_file_sha1_bytes);

			bool found_pattern = patch_pattern.FindPatterns();
			if (!found_pattern)
			{
				return string.Format("ERROR Failed to find the asm code that I need to patch: {0}" +
					"SHA1={1}{2}" +
					"File={3}{4}",
					Environment.NewLine,
					exe_file_sha1, Environment.NewLine,
					args.ExeFile, Environment.NewLine);
			}

			bool calculate_mod = patch_pattern.CalculateModJmp();
			if (!calculate_mod)
			{
				return string.Format("ERROR Found the asm code I needed to patch, but failed to calculate the correct patch code: {0}" +
					"SHA1={1}{2}" +
					"File={3}{4}",
					Environment.NewLine,
					exe_file_sha1, Environment.NewLine,
					args.ExeFile, Environment.NewLine);
			}

			patch_pattern.ApplyModJmp();

			using (var fs = File.OpenWrite(args.ExeFile))
			{
				fs.Write(patch_pattern.SourceExeBytes, 0, patch_pattern.SourceExeBytes.Length);
			}

			return args.ExeFile;
		}

		public static string PatchGameExeBySha1(object taskState)
		{
			var args = taskState as PatchGameExeByParameters;

			var exe_file_attrs = File.GetAttributes(args.ExeFile);
			if (exe_file_attrs.HasFlag(FileAttributes.ReadOnly))
			{
				return string.Format("ERROR Cannot patch read-only file (this tool creates a backup): {0}",
					args.ExeFile);
			}

			args.BackupFile();

			byte[] exe_file_sha1_bytes = null;
			using (var fs = File.OpenRead(args.ExeFile))
			using (var sha1_provider = new System.Security.Cryptography.SHA1CryptoServiceProvider())
			{
				exe_file_sha1_bytes = sha1_provider.ComputeHash(fs);
			}

			var exe_file_sha1 = KSoft.Text.Util.ByteArrayToString(exe_file_sha1_bytes);
			PatchInfo exe_paches;
			if (!TryGetPatchInfo(exe_file_sha1, out exe_paches))
			{
				return string.Format("ERROR Unrecongized file: {0}" +
					"SHA1={1}{2}" +
					"File={3}{4}",
					Environment.NewLine,
					exe_file_sha1, Environment.NewLine,
					args.ExeFile, Environment.NewLine);
			}

			using (var fs = File.OpenWrite(args.ExeFile))
			{
				foreach (var kvp in exe_paches.Patches)
				{
					fs.Seek(kvp.Key, SeekOrigin.Begin);
					fs.Write(kvp.Value, 0, kvp.Value.Length);
				}
			}

			return args.ExeFile;
		}
	};
}