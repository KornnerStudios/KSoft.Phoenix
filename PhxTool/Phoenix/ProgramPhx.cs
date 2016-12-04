using System;
using System.Collections.Generic;
using Mono.Options;

namespace KSoft.Tool
{
	sealed class ProgramPhx : ProgramBase
	{
		protected override Environment ProgramEnvironment { get { return Environment.Phx; } }
		public static void _Main(List<string> args)
		{
			var prog = new ProgramPhx();
			prog.MainImpl(args);
		}

		#region ToolType
		enum ToolType
		{
			None,

			Era,
			Wwise,
		};
		static string GetValidTools()
		{
			var sb = new System.Text.StringBuilder(64);
			sb.Append("Valid tools: ");

			sb.AppendFormat("{0},", ToolType.Era.ToString().ToLowerInvariant());
			sb.AppendFormat("{0},", ToolType.Wwise.ToString().ToLowerInvariant());

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
				case ToolType.Era:
					Phoenix.EraTool._Main(help_name, args);
					break;
				case ToolType.Wwise:
					Phoenix.WwiseTool._Main(help_name, args);
					break;

				default: Program.UnavailableOption(mToolType); break;
			}
		}
		void MainImpl(List<string> args)
		{
			List<string> extra;
			MainImpl_Prologue(args, out extra, () => mToolType == ToolType.None);
			MainImpl_Program(extra, MainBody);
		}
	};
}