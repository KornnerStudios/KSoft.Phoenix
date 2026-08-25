using System;
using System.Collections.Generic;
using Mono.Options;

namespace KSoft.Tool
{
	sealed class ProgramHogan : ProgramBase
	{
		protected override Environment ProgramEnvironment => Environment.Hogan;
		public static void MainEntryPoint(List<string> args)
		{
			var prog = new ProgramHogan();
			prog.MainImpl(args);
		}

		#region ToolType
		enum ToolType
		{
			None,

			Pkg,
		};
		static string GetValidTools()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid tools: ");

			sb.Append(ToolType.Pkg.ToString().ToLowerInvariant()).Append(',');

			return sb.ToString();
		}
		#endregion

		ToolType mToolType;

		protected override void InitializeOptions()
		{
			mOptions = new OptionSet() {
				{ "tool=", GetValidTools(),
					v => Program.ParseEnum(v, out mToolType) },
			};
			InitializeOptionArgShowHelp();
		}

		void MainBody(List<string> args)
		{
			string help_name = "tool=" + mToolType.ToString().ToLowerInvariant();

			switch (mToolType)
			{
				case ToolType.Pkg:
					Hogan.PkgTool.MainEntryPoint(help_name, args);
					break;

				default: Program.UnavailableOption(mToolType); break;
			}
		}
		void MainImpl(List<string> args)
		{
			MainImpl_Prologue(args, out List<string> extra, () => mToolType == ToolType.None);
			MainImpl_Program(extra, MainBody);
		}
	};
}
