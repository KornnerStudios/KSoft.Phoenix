using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		public virtual bool Preload()
		{
			return Database.Preload();
		}

		public virtual bool Load()
		{
			var load_task = Task<bool>.Factory.StartNew((state) =>
			{
				var db = (Phx.BDatabaseBase)state;
				return db.Load();
			}, Database);

			var load_tactics_task = Task<bool>.Factory.StartNew((state) =>
			{
				var db = (Phx.BDatabaseBase)state;
				return db.LoadAllTactics();
			}, Database);

			var tasks = new List<Task<bool>>();
			var task_exceptions = new List<Exception>();

			bool success = true;
			PhxUtil.UpdateResultWithTaskResults(ref success, tasks, task_exceptions);

			if (!success)
			{
				Debug.Trace.XML.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone,
					"Failed to load engine data",
					task_exceptions.ToAggregateExceptionOrNull());
			}

			return success;
		}
	};
}