using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Engine
{
	public partial class PhxEngine
	{
		public PhxEngineBuild Build { get; private set; }

		public bool TargetsXbox360 { get; private set; }

		public GameDirectories Directories { get; private set; }

		public Phx.BDatabaseBase Database { get; private set; }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		internal Dictionary<XmlFileInfo, XmlFileLoadState> XmlFileLoadStatus { get; private set; }
			= new Dictionary<XmlFileInfo, XmlFileLoadState>();

		internal void UpdateFileLoadStatus(XmlFileInfo file, XmlFileLoadState state)
		{
			Contract.Requires(file != null);

			lock (XmlFileLoadStatus)
				XmlFileLoadStatus[file] = state;
		}

		public XmlFileLoadState GetFileLoadStatus(XmlFileInfo file)
		{
			Contract.Requires(file != null);

			XmlFileLoadState state;
			lock (XmlFileLoadStatus)
				if (!XmlFileLoadStatus.TryGetValue(file, out state))
					return XmlFileLoadState.NotLoaded;

			return state;
		}

		public virtual void Load()
		{
			Database.Load();
		}
	};
}