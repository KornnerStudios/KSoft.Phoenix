﻿using System;
using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfFileUtilOptions
	{
		DumpDebugInfo,
		SkipVerification,
		/// <summary>Built for 64-bit builds</summary>
		x64,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)] kNumberOf,
	};

	public abstract class EcfFileUtil
		: IDisposable
	{
		public EcfFileDefinition EcfDefinition { get; private set; }
		internal EcfFile mEcfFile;
		protected string mSourceFile; // filename of the source file which the util stems from
		public TextWriter ProgressOutput { get; set; }
		public TextWriter VerboseOutput { get; set; }
		public TextWriter DebugOutput { get; set; }

		/// <see cref="EcfFileUtilOptions"/>
		public Collections.BitVector32 Options = new Collections.BitVector32();

		protected EcfFileUtil()
		{
			EcfDefinition = new EcfFileDefinition();

			if (System.Diagnostics.Debugger.IsAttached)
				ProgressOutput = Console.Out;
			if (System.Diagnostics.Debugger.IsAttached)
				VerboseOutput = Console.Out;
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			ProgressOutput = null;
			VerboseOutput = null;
			DebugOutput = null;
			Util.DisposeAndNull(ref mEcfFile);
		}
		#endregion
	};
}