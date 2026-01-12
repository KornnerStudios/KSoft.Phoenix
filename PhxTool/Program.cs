//#define TEST_ARGS
//#define TEST_ENV_PHX
//#define TEST_ENV_PHX_XMB

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Mono.Options;

/*
 * KSoft.Tool
 *	?
 *	env=
 *		hogan
 *			tool=
 *				pkg
 *					mode=
 *		phx
 *			help
 *			tool=
 *				gsave
 *					mode=
 *				era
 *					mode=
 *					path=
 *					name=
 *					out=
 *					switches=
 *				xmb
 *					mode=
 *					path=
 *					out=
 *					switches=
*/

namespace KSoft.Tool
{
	partial class Program : ProgramBase
	{
		protected override Environment ProgramEnvironment => Environment.None;

		static string gName;
		public static string GetName()
		{
			if (gName == null)
			{
				gName = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Name;
			}

			return gName;
		}
		static string gVersion;
		public static string GetVersion()
		{
			if (gVersion == null)
			{
				gVersion = System.Reflection.Assembly.GetAssembly(typeof(Program)).GetName().Version.ToString();
			}

			return gVersion;
		}

		static void Initialize()
		{
			KSoft.Program.Initialize();
			KSoft.Phoenix.Program.Initialize();
		}
		static void Dispose()
		{
			KSoft.Phoenix.Program.Dispose();
			KSoft.Program.Dispose();
		}

		static void Main(string[] args)
		{
			Console.WriteLine(); // newline, because long command lines don't go great with real messages

			DependentAssemblyExists("ICSharpCode.SharpZipLib", false);
			if (!DependentAssemblyExists("KSoft"))
			{
				return;
			}

			Initialize();

			var prog = new KSoft.Tool.Program();
			try { prog.MainImpl(args); }
			catch (Exception e)
			{
				Console.Write("UNHANDLED EXCEPTION: ");
				Console.WriteLine(e);
				if (System.Diagnostics.Debugger.IsAttached)
				{
					throw;
				}
			}

			Dispose();

			if (System.Diagnostics.Debugger.IsAttached)
			{
				Console.WriteLine("Done");
				Console.Read();
			}
		}

		#region Environment
		static string GetValidEnvironments()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid environments: ");

			sb.AppendFormat("{0},", Environment.Phx.ToString().ToLowerInvariant());

			return sb.ToString(0, sb.Length-1); // don't include the last ','
		}
		static string ToString(Environment env)
		{
			if (env == Environment.None)
			{
				return "Main";
			}

			return env.ToString();
		}
		#endregion

		Environment mArgEnv;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{ "env=", GetValidEnvironments(),
					v => ParseEnum(v, out mArgEnv) },
			};
			InitializeOptionArgShowHelp(useQuestionMark:true);
		}

#if TEST_ARGS
		static string[] kTestArgs = {
	#if TEST_ENV_PHX
			"-env=phx",
				"-tool=era",
// 					"-mode=" +		 "expand",
// 					//"-path=" +		@"C:\Mount\A\Xbox\Xbox360\Games\Halo Wars\decrypted\chasms",
// 					"-path=" +		@"C:\Users\Sean\Downloads\HW\test\chasms",
// 					"-name=" +		 "chasms",
// 					"-out=" +		@"C:\Users\Sean\Downloads\HW\test\2\",

	//				"-mode=" +		 "build",
	//				"-path=" +		@"C:\Users\Sean\Downloads\HW\test",
	//				"-name=" +		 "chasms",

				#if false
					"-mode=" +		 "expand",
					"-path=" +		@"C:\Mount\A\Xbox\Xbox360\Games\Halo Wars\decrypted\loadingUI",
					"-name=" +		 "loadingUI",
					"-out=" +		@"C:\Users\Sean\Downloads\HW\test\",
				#endif

					"-mode=" +		 "decrypt",
					"-path=" +		@"C:\KStudio\HaloWars\PC\",
					"-name=" +		 "shader",
					"-out=" +		@"C:\KStudio\Vita\_test_results\KSoft.Phoenix\",

					"-switches=" +	 "00110",
	#elif TEST_ENV_PHX_XMB
			"-env=phx",
				"-tool=xmb",
					"-mode=" +       "DumpSingle24Values",
					"-path=" +      @"D:\HW\HWDE\Extracted\",

					"-switches=" +   "0",
	#elif !TEST_ENV_PHX_WWISE
			"-env=phx",
				"-tool=wwise",
					"-mode=" +		 "extract",
					"-path=" +		@"C:\Mount\A\Xbox\Xbox360\Games\Halo Wars\sound",
					"-out=" +		@"D:\HW\test",

					"-switches=" +	 "0",
	#endif
		};
#endif
		void MainImpl(string[] args)
		{
#if TEST_ARGS
			args = kTestArgs;
#endif
			//return;

			if (!TryParse(Environment.None, mOptions, args, out List<string> extra) || mArgEnv == Environment.None)
			{
				mArgShowHelp = true;
			}

			if (mArgShowHelp)
			{
				ShowHelp(Environment.None, mOptions);
			}
			else
			{
				switch (mArgEnv)
				{
					case Environment.Hogan:
						ProgramHogan.MainEntryPoint(extra);
						break;

					case Environment.Phx:
						ProgramPhx.MainEntryPoint(extra);
						break;

					default:
						UnavailableOption(mArgEnv);
						break;
				}
			}
		}

		#region Util
		public static bool TryParse(Environment env, OptionSet ops, IEnumerable<string> args, out List<string> extra)
		{
			bool result = true;
			extra = null;

			string help = env == Environment.None ? "?" : "help";

			try { extra = ops.Parse(args); }
			catch (OptionException e)
			{
				Console.Write("{0}:", GetName());
				Console.WriteLine(e.Message);
				Console.WriteLine("Use {0} for more information", help);
				result = false;
			}

			return result;
		}
		public static void ShowHelp(Environment env, OptionSet ops, string subtoolName = "")
		{
			Console.WriteLine();
			Console.WriteLine("Version: {0}", GetVersion());
			Console.WriteLine("Usage: {0} <environment> <environment-options>", GetName());
			Console.WriteLine("{0} Options: {1}", ToString(env), subtoolName);
			ops.WriteOptionDescriptions(Console.Out);
		}
		public static void UnavailableOption<T>(
			[SuppressMessage("Microsoft.Design", "IDE0060:ReviewUnusedParameters")]
			T option)
		{
			//string option_str = option == null ? null : option.ToString();

			Console.WriteLine("I don't think so scooter");
		}

		public static void ParseEnum<T>(string input, out T value)
			where T : struct
		{
			Enum.TryParse<T>(input, true, out value);
		}
		public static void ParseEnum<T>(string input, out T value, T invalidValue)
			where T : struct
		{
			if (!Enum.TryParse<T>(input, true, out value))
			{
				value = invalidValue;
			}
		}
		#endregion
	};
}
