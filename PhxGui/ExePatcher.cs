using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhxGui
{
	partial class MainWindowViewModel
	{
		private void PatchGameExe(string exeFile, AcceptedFileType fileType)
		{
			ClearMessages();
			IsProcessing = true;

			var task = Task.Run(() =>
			{
				var exe_file_attrs = System.IO.File.GetAttributes(exeFile);
				if (exe_file_attrs.HasFlag(System.IO.FileAttributes.ReadOnly))
				{
					return string.Format("ERROR Cannot patch read-only file (this tool creates a backup): {0}",
						exeFile);
				}

				{
					string extension = System.IO.Path.GetExtension(exeFile);
					string backup_file = System.IO.Path.GetFileNameWithoutExtension(exeFile);
					backup_file += "_UNTOUCHED.exe";
					backup_file = System.IO.Path.ChangeExtension(backup_file, extension);
					backup_file = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(exeFile), backup_file);
					System.IO.File.Copy(exeFile, backup_file);
				}

				byte[] exe_file_sha1_bytes = null;
				using (var fs = System.IO.File.OpenRead(exeFile))
				using (var sha1_provider = new System.Security.Cryptography.SHA1CryptoServiceProvider())
				{
					exe_file_sha1_bytes = sha1_provider.ComputeHash(fs);
				}

				var exe_file_sha1 = KSoft.Text.Util.ByteArrayToString(exe_file_sha1_bytes);
				ExePatching.PatchInfo exe_paches;
				if (!ExePatching.TryGetPatchInfo(exe_file_sha1, out exe_paches))
				{
					return string.Format("ERROR Unrecongized file: {0}" +
						"SHA1={1}{2}" +
						"File={3}{4}",
						Environment.NewLine,
						exe_file_sha1, Environment.NewLine,
						exeFile, Environment.NewLine);
				}

				using (var fs = System.IO.File.OpenWrite(exeFile))
				{
					foreach (var kvp in exe_paches.Patches)
					{
						fs.Seek(kvp.Key, System.IO.SeekOrigin.Begin);
						fs.Write(kvp.Value, 0, kvp.Value.Length);
					}
				}

				return exeFile;
			});

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.Result.StartsWith("ERROR", StringComparison.Ordinal))
				{
					MessagesText += string.Format("Patch EXE finished with errors: {0}{1}",
						Environment.NewLine,
						t.IsFaulted ? t.Exception.ToString() : t.Result);
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
	};
}