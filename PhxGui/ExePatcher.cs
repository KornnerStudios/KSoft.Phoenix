using System;
using System.Collections.Generic;
using System.Globalization;
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
				CanOverwriteFiles = Flags.Test(MiscFlags.DontOverwriteExistingFiles)==false,
			};

			var task = Task.Run(() =>
				//ExePatching.PatchGameExeBySha1,
				ExePatching.PatchGameExeByPatternMatching(args));

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			task.ContinueWith(t =>
			{
				if (t.IsFaulted || t.Result.StartsWith("ERROR", StringComparison.Ordinal))
				{
					MessagesText += string.Create(CultureInfo.CurrentCulture,
						$"Patch EXE finished with errors: {Environment.NewLine}{(t.IsFaulted ? t.Exception!.GetOnlyExceptionOrAll()!.ToString() : t.Result)}");
				}
				else
				{
					MessagesText = string.Create(CultureInfo.CurrentCulture,
						$"EXE is now ready for modding use: {t.Result}");
				}

				FinishProcessing();
			}, scheduler);
		}
	};

	static class ExePatching
	{
		#region old PatchGameExeBySha1
		sealed class PatchInfo
		{
			public string Sha1;
			public Dictionary<uint, byte[]> Patches = new();

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
		private static readonly List<PatchInfo> kPatches = new();

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

		static bool TryGetPatchInfo(string actualSha1, out PatchInfo? info)
		{
			info = null;

			info = (from i in kPatches
					where i.Sha1 == actualSha1
					select i).FirstOrDefault();

			return info != null;
		}
		#endregion

		public sealed class PatchGameExeByParameters
		{
			public string ExeFile = null!;
			public MainWindowViewModel.AcceptedFileType ExeFileType;
			public bool CanOverwriteFiles = true;

			public void BackupFile()
			{
				string extension = Path.GetExtension(ExeFile);
				string backup_file = Path.GetFileNameWithoutExtension(ExeFile);
				backup_file += "_UNTOUCHED.exe";
				backup_file = Path.ChangeExtension(backup_file, extension);
				backup_file = Path.Combine(Path.GetDirectoryName(ExeFile)!, backup_file);
				File.Copy(ExeFile, backup_file, CanOverwriteFiles);
			}
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Cryptography", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "Used only to identify known executable fingerprints for patch selection, not for security authentication.")]
		public static string PatchGameExeByPatternMatching(object? taskState)
		{
			var args = KSoft.Debug.TypeCheck.CastReference<PatchGameExeByParameters>(taskState!)!;

			#region boilerplate
			if (args.ExeFileType == MainWindowViewModel.AcceptedFileType.Xex)
			{
				return string.Create(CultureInfo.CurrentCulture,
					$"ERROR patching XEX files is not supported: {args.ExeFile}");
			}

			var exe_file_attrs = File.GetAttributes(args.ExeFile);
			if (exe_file_attrs.HasFlag(FileAttributes.ReadOnly))
			{
				return string.Create(CultureInfo.CurrentCulture,
					$"ERROR Cannot patch read-only file (this tool creates a backup): {args.ExeFile}");
			}

			try
			{
				args.BackupFile();
			} catch (Exception ex)
			{
				return string.Create(CultureInfo.CurrentCulture,
					$"ERROR Failed to create backup: {args.ExeFile}{Environment.NewLine}{ex}");
			}

			byte[] sourceExeBytes;
			try
			{
				sourceExeBytes = File.ReadAllBytes(args.ExeFile);
			}
			catch (Exception ex)
			{
				return string.Create(CultureInfo.CurrentCulture,
					$"ERROR Failed to read file to memory: {args.ExeFile}{Environment.NewLine}{ex}");
			}
			#endregion

			byte[]? exe_file_sha1_bytes = null;
			using (var ms = new MemoryStream(sourceExeBytes))
			using (var sha1_provider = System.Security.Cryptography.SHA1.Create())
			{
				exe_file_sha1_bytes = sha1_provider.ComputeHash(ms);
			}

			var exe_file_sha1 = KSoft.Text.Util.ByteArrayToString(exe_file_sha1_bytes!);

			var finalErrorMessage = new System.Text.StringBuilder();
			{
				string? errorMessage = PatchGameExeEraDigitalSignatureCheckByPatternMatching(sourceExeBytes, sourceExeBytes);
				if (errorMessage.IsNotNullOrEmpty())
				{
					finalErrorMessage.AppendFormat(CultureInfo.CurrentCulture, "ERROR EraDigitalSignatureCheck - {0}: {1}" +
						"SHA1={2}{3}" +
						"File={4}{5}",
						errorMessage, Environment.NewLine,
						exe_file_sha1, Environment.NewLine,
						args.ExeFile, Environment.NewLine);
				}
			}

			{
				string? errorMessage = PatchGameExeParticleGatewayAssertByPatternMatching(sourceExeBytes, sourceExeBytes);
				if (errorMessage.IsNotNullOrEmpty())
				{
					finalErrorMessage.AppendFormat(CultureInfo.CurrentCulture, "ERROR BParticleGateway cMaxDataSlots assert - {0}: {1}" +
						"SHA1={2}{3}" +
						"File={4}{5}",
						errorMessage, Environment.NewLine,
						exe_file_sha1, Environment.NewLine,
						args.ExeFile, Environment.NewLine);
				}
			}

			{
				string? errorMessage = PatchGameExeUserProfileSetupTickerInfoByPatternMatching(sourceExeBytes, sourceExeBytes);
				if (errorMessage.IsNotNullOrEmpty())
				{
					finalErrorMessage.AppendFormat(CultureInfo.CurrentCulture, "ERROR UserProfileSetupTickerInfo - {0}: {1}" +
						"SHA1={2}{3}" +
						"File={4}{5}",
						errorMessage, Environment.NewLine,
						exe_file_sha1, Environment.NewLine,
						args.ExeFile, Environment.NewLine);
				}
			}

			if (finalErrorMessage.Length > 0)
			{
				return finalErrorMessage.ToString();
			}

			using (var fs = File.OpenWrite(args.ExeFile))
			{
				fs.Write(sourceExeBytes, 0, sourceExeBytes.Length);
			}

			return args.ExeFile;
		}

		static string? PatchGameExeEraDigitalSignatureCheckByPatternMatching(ReadOnlySpan<byte> sourceExeBytes, byte[] dstExeBytes)
		{
			var patch_pattern = new KSoft.Phoenix.zPatching.WinExePatcherProcessHeaderData();
			bool found_pattern = patch_pattern.FindPatterns(sourceExeBytes);
			if (!found_pattern)
			{
				return "Failed to find the asm code that I need to patch";
			}

			bool calculate_mod = patch_pattern.CalculateModJmp(sourceExeBytes);
			if (!calculate_mod)
			{
				return "Found the asm code I needed to patch, but failed to calculate the correct patch code";
			}

			patch_pattern.ApplyModJmp(dstExeBytes);
			return null;
		}

		static string? PatchGameExeParticleGatewayAssertByPatternMatching(ReadOnlySpan<byte> sourceExeBytes, byte[] dstExeBytes)
		{
			var patch_pattern = new KSoft.Phoenix.Games.HaloWars.zPatching.WinExePatcherParticleGateway();
			bool found_pattern = patch_pattern.FindPatterns(sourceExeBytes);
			if (!found_pattern)
			{
				return "Failed to find the asm code that I need to patch";
			}

			bool calculate_mod = patch_pattern.CalculateModJmp(sourceExeBytes);
			if (!calculate_mod)
			{
				return "Found the asm code I needed to patch, but failed to calculate the correct patch code";
			}

			patch_pattern.ApplyModJmp(dstExeBytes);
			return null;
		}

		static string? PatchGameExeUserProfileSetupTickerInfoByPatternMatching(ReadOnlySpan<byte> sourceExeBytes, Span<byte> dstExeBytes)
		{
			var patch_pattern = new KSoft.Phoenix.Games.HaloWars.zPatching.WinExePatcherUserProfileTickerInfo();
			bool found_pattern = patch_pattern.FindPatterns(sourceExeBytes);
			if (!found_pattern)
			{
				return "Failed to find the asm code that I need to patch";
			}
			bool calculate_mod = patch_pattern.BuildPatchData(sourceExeBytes);
			if (!calculate_mod)
			{
				return "Found the asm code I needed to patch, but failed to calculate the correct patch code";
			}
			patch_pattern.ApplyPatches(dstExeBytes);
			return null;
		}

		#region old PatchGameExeBySha1
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Cryptography", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "Used only to identify known executable fingerprints for patch selection, not for security authentication.")]
		public static string PatchGameExeBySha1(object? taskState)
		{
			var args = (taskState as PatchGameExeByParameters)!;

			var exe_file_attrs = File.GetAttributes(args.ExeFile);
			if (exe_file_attrs.HasFlag(FileAttributes.ReadOnly))
			{
				return string.Create(CultureInfo.CurrentCulture,
					$"ERROR Cannot patch read-only file (this tool creates a backup): {args.ExeFile}");
			}

			args.BackupFile();

			byte[]? exe_file_sha1_bytes = null;
			using (var fs = File.OpenRead(args.ExeFile))
			using (var sha1_provider = System.Security.Cryptography.SHA1.Create())
			{
				exe_file_sha1_bytes = sha1_provider.ComputeHash(fs);
			}

			var exe_file_sha1 = KSoft.Text.Util.ByteArrayToString(exe_file_sha1_bytes!);
			if (!TryGetPatchInfo(exe_file_sha1, out PatchInfo? exe_paches) || exe_paches == null)
			{
				return string.Create(CultureInfo.CurrentCulture,
					$"ERROR Unrecongized file: {Environment.NewLine}SHA1={exe_file_sha1}{Environment.NewLine}File={args.ExeFile}{Environment.NewLine}");
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
		#endregion
	};
}
