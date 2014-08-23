using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Engine
{
	public sealed class XmlFileInfo
	{
		public ContentStorage Location { get; set; }
		public GameDirectory Directory { get; set; }
		public string FileName { get; set; }
		public string RootName { get; set; }

		public bool Writable { get; set; }
		public bool NonRequiredFile { get; set; }
	};

	public partial class PhxEngine
	{
		public PhxEngineBuild Build { get; private set; }

		public GameDirectories Directories { get; private set; }

		public Phx.BDatabaseBase Database { get; private set; }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		public virtual void Load()
		{
			Database.Load();
		}
	};
}