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

			tasks.Add(load_task);
			tasks.Add(load_tactics_task);

			bool success = true;
			PhxUtil.UpdateResultWithTaskResults(ref success, tasks, task_exceptions);

			if (!success)
			{
				Debug.Trace.XML.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone,
					"Failed to load engine data",
					task_exceptions.ToAggregateExceptionOrNull().GetOnlyExceptionOrAllWhenAggregate());
			}

			return success;
		}

		public IO.XmlElementStream OpenXmlOrXmbForRead(string fileName)
		{
			string ext = System.IO.Path.GetFileNameWithoutExtension(fileName);

			var xml_or_xmb = ext.Equals(Xmb.XmbFile.kFileExt, StringComparison.OrdinalIgnoreCase)
				? GetXmlOrXmbFileResult.Xmb
				: GetXmlOrXmbFileResult.Xml;

			return OpenXmlOrXmbForRead(xml_or_xmb, fileName);
		}

		public IO.XmlElementStream OpenXmlOrXmbForRead(GetXmlOrXmbFileResult xmlOrXmb, string fileName)
		{
			switch (xmlOrXmb)
			{
				case GetXmlOrXmbFileResult.Xml:
					return new IO.XmlElementStream(fileName, System.IO.FileAccess.Read);

				case GetXmlOrXmbFileResult.Xmb:
					return OpenXmbForRead(fileName);
			}

			return null;
		}
		public IO.XmlElementStream OpenXmbForRead(string xmbFile)
		{
			var va_size = TargetsXbox360
				? Shell.ProcessorSize.x32
				: Shell.ProcessorSize.x64;
			var endian_format = Shell.EndianFormat.Big;

			byte[] file_bytes = System.IO.File.ReadAllBytes(xmbFile);

			using (var xmb_ms = new System.IO.MemoryStream(file_bytes, false))
			using (var xmb = new KSoft.IO.EndianStream(xmb_ms, endian_format, System.IO.FileAccess.Read))
			using (var xml_ms = new System.IO.MemoryStream(IntegerMath.kMega * 1))
			{
				xmb.StreamMode = System.IO.FileAccess.Read;

				Resource.ResourceUtils.XmbToXml(xmb, xml_ms, va_size);
				// need to do this else we'll get a Root element is missing exception
				xml_ms.Position = 0;

				var xml = new IO.XmlElementStream(xml_ms, System.IO.FileAccess.Read, streamNameOverride: xmbFile);
				return xml;
			}
		}
	};
}