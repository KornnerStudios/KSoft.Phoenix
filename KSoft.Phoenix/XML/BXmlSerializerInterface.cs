using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	public abstract class BXmlSerializerInterface
		: IDisposable
	{
		#region NullInterface
		sealed class NullInterface : BXmlSerializerInterface
		{
			Phx.BDatabaseBase mDatabase;
			internal override Phx.BDatabaseBase Database { get { return mDatabase; } }

			public NullInterface(Phx.BDatabaseBase db) { mDatabase = db; }

			public override void Dispose() {}
		};
		public static BXmlSerializerInterface GetNullInterface(Phx.BDatabaseBase db)
		{
			Contract.Requires(db != null);

			return new NullInterface(db);
		}
		#endregion

		internal abstract Phx.BDatabaseBase Database { get; }
		internal Engine.PhxEngine GameEngine { get { return Database.Engine; } }

		#region IDisposable Members
		public abstract void Dispose();
		#endregion

		#region Stream files utils
		static void SetupStream(IO.XmlElementStream s, FA mode, XML.BXmlSerializerInterface xs)
		{
			s.IgnoreCaseOnEnums = true;
			s.ExceptionOnEnumParseFail = true;
			s.StreamMode = mode;
			s.InitializeAtRootElement();
			s.SetSerializerInterface(xs);
		}

		public bool TryStreamData<TContext>(
			Engine.XmlFileInfo xfi, FA mode,
			Action<IO.XmlElementStream, TContext> streamProc, TContext ctxt,
			string ext = null)
		{
			Contract.Requires(xfi != null);
			Contract.Requires(streamProc != null);

			System.IO.FileInfo file;
			bool result = GameEngine.Directories.TryGetFile(xfi.Location, xfi.Directory, xfi.FileName, out file, ext);

			if (mode == FA.Read)
			{
				if (result) using (var s = new IO.XmlElementStream(file.FullName, mode))
				{
					SetupStream(s, mode, this);
					streamProc(s, ctxt);
				}
			}
			else if (mode == FA.Write)
			{
				result = result && xfi.Writable;

				if (result) using (var s = IO.XmlElementStream.CreateForWrite(xfi.RootName))
				{
					SetupStream(s, mode, this);
					streamProc(s, ctxt);
					s.Document.Save(file.FullName);
				}
			}

			return result;
		}
		public bool TryStreamData(
			Engine.XmlFileInfo xfi, FA mode,
			Action<IO.XmlElementStream> streamProc,
			string ext = null)
		{
			Contract.Requires(xfi != null);
			Contract.Requires(streamProc != null);

			System.IO.FileInfo file;
			bool result = GameEngine.Directories.TryGetFile(xfi.Location, xfi.Directory, xfi.FileName, out file, ext);

			if (mode == FA.Read)
			{
				if (result) using (var s = new KSoft.IO.XmlElementStream(file.FullName, mode))
				{
					SetupStream(s, mode, this);
					streamProc(s);
				}
			}
			else if (mode == FA.Write)
			{
				result = result && xfi.Writable;

				if (result) using (var s = KSoft.IO.XmlElementStream.CreateForWrite(xfi.RootName))
				{
					SetupStream(s, mode, this);
					streamProc(s);
					s.Document.Save(file.FullName);
				}
			}

			return result;
		}
		public void ReadDataFilesAsync<TContext>(
			Engine.ContentStorage loc, Engine.GameDirectory gameDir, string searchPattern,
			Action<IO.XmlElementStream, TContext> streamProc, TContext ctxt,
			out ParallelLoopResult result)
		{
			Contract.Requires(!string.IsNullOrEmpty(searchPattern));
			Contract.Requires(streamProc != null);

			result = Parallel.ForEach(GameEngine.Directories.GetFiles(loc, gameDir, searchPattern), (filename) =>
			{
				const FA k_mode = FA.Read;

				using (var s = new KSoft.IO.XmlElementStream(filename, k_mode))
				{
					SetupStream(s, k_mode, this);
					streamProc(s, ctxt);
				}
			});
		}
		public void ReadDataFilesAsync(
			Engine.ContentStorage loc, Engine.GameDirectory gameDir, string searchPattern,
			Action<IO.XmlElementStream> streamProc,
			out ParallelLoopResult result)
		{
			Contract.Requires(!string.IsNullOrEmpty(searchPattern));

			result = Parallel.ForEach(GameEngine.Directories.GetFiles(loc, gameDir, searchPattern), (filename) =>
			{
				const FA k_mode = FA.Read;

				using (var s = new KSoft.IO.XmlElementStream(filename, k_mode))
				{
					SetupStream(s, k_mode, this);
					streamProc(s);
				}
			});
		}
		#endregion

		public bool StreamStringID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName,
			ref int value, IO.TagElementNodeType xmlSource = XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			bool was_streamed = false;

			if (xmlSource == XmlUtil.kSourceElement)
				was_streamed = s.StreamElementOpt(xmlName, ref value, Predicates.IsNotNone);
			else if (xmlSource == XmlUtil.kSourceAttr)
				was_streamed = s.StreamAttributeOpt(xmlName, ref value, Predicates.IsNotNone);
			else if (xmlSource == XmlUtil.kSourceCursor)
			{
				was_streamed = true;
				s.StreamCursor(ref value);
			}

			if (s.IsReading)
			{
				if (value.IsNotNone())
					Database.AddStringIDReference(value);
			}

			return was_streamed;
		}

		[System.Diagnostics.Conditional("TRACE")]
		static void TraceUndefinedHandle<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string name,
			string xmlName,
			int id, string kind)
			where TDoc : class
			where TCursor : class
		{
			Contract.Assert(s.IsReading);

			var line_info = Text.TextLineInfo.Empty;
			var cursor_name = "<unknown element>";
			var text_stream = s as IO.TagElementTextStream<TDoc, TCursor>;
			if (text_stream != null)
			{
				cursor_name = text_stream.CursorName;
				line_info = text_stream.TryGetLastReadLineInfo();
			}

			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"Generated UndefinedHandle in '{0} @ {1}' for '{2}.{3}' ({4}). {5}={6}",
				s.StreamName, Text.TextLineInfo.ToString(line_info, true),
				cursor_name, xmlName ?? "InnerText",
				kind, name, PhxUtil.GetUndefinedReferenceDataIndex(id).ToString());
		}


		protected static bool ToLowerName(Phx.DatabaseObjectKind kind)
		{
			switch (kind)
			{
#if false
			case Phx.DatabaseObjectKind.Object:
			case Phx.DatabaseObjectKind.Unit:
				return Phx.BProtoObject.kBListXmlParams.ToLowerDataNames;

			case Phx.DatabaseObjectKind.Squad:
				return Phx.BProtoSquad.kBListXmlParams.ToLowerDataNames;

			case Phx.DatabaseObjectKind.Tech:
				return Phx.BProtoTech.kBListXmlParams.ToLowerDataNames;
#endif

			default:
				return false;
			}
		}
		public bool StreamTypeName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string xmlName, ref int dbid,
			Phx.GameDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			string id_name = null;
			bool was_streamed = true;
			bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);

				if (was_streamed)
				{
					dbid = Database.GetId(kind, id_name);
					Contract.Assert(dbid.IsNotNone());
					if (PhxUtil.IsUndefinedReferenceHandle(dbid))
						TraceUndefinedHandle(s, id_name, xmlName, dbid, kind.ToString());
				}
				else
					dbid = TypeExtensions.kNone;
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					was_streamed = false;
					return was_streamed;
				}

				id_name = Database.GetName(kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(id_name));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);
			}

			return was_streamed;
		}
		public bool StreamHPBarName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string xmlName, ref int dbid,
			Phx.HPBarDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			string id_name = null;
			bool was_streamed = true;
			bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);

				if (was_streamed)
				{
					dbid = Database.GetId(kind, id_name);
					Contract.Assert(dbid.IsNotNone());
					if (PhxUtil.IsUndefinedReferenceHandle(dbid))
						TraceUndefinedHandle(s, id_name, xmlName, dbid, kind.ToString());
				}
				else
					dbid = TypeExtensions.kNone;
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					was_streamed = false;
					return was_streamed;
				}

				id_name = Database.GetName(kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(id_name));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);
			}

			return was_streamed;
		}
		public bool StreamDBID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string xmlName, ref int dbid,
			Phx.DatabaseObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			string id_name = null;
			bool was_streamed = true;
			bool to_lower = ToLowerName(kind);

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);

				if (was_streamed)
				{
					dbid = Database.GetId(kind, id_name);
					Contract.Assert(dbid.IsNotNone());
					if (PhxUtil.IsUndefinedReferenceHandle(dbid))
						TraceUndefinedHandle(s, id_name, xmlName, dbid, kind.ToString());
				}
				else
					dbid = TypeExtensions.kNone;
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					was_streamed = false;
					return was_streamed;
				}

				id_name = Database.GetName(kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(id_name));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);
			}

			return was_streamed;
		}

		public bool StreamDBID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, List<int> dbidList
			, Phx.DatabaseObjectKind kind
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));
			Contract.Requires(xmlSource != IO.TagElementNodeType.Attribute);

			bool was_streamed = false;

			if (s.IsReading)
			{
				XmlUtil.ReadDetermineListSize(s, dbidList);

				foreach (var n in XmlUtil.ReadGetNodes(s, xmlName, xmlSource))
				{
					using (s.EnterCursorBookmark(n))
					{
						int dbid = TypeExtensions.kNone;
						if (StreamDBID(s, xmlName, ref dbid, kind, isOptional, xmlSource))
						{
							was_streamed = true;
							dbidList.Add(dbid);
						}
					}
				}
			}
			else if (s.IsWriting && dbidList.Count > 0)
			{
				was_streamed = true;

				foreach (int dbid in dbidList)
				{
					int dbidCopy = dbid;
					using (s.EnterCursorBookmark(xmlName))
						StreamDBID(s, xmlName, ref dbidCopy, kind, isOptional, xmlSource);
				}
			}

			return was_streamed;
		}

		/// <summary>Stream the current element's Text as a a string</summary>
		internal static void StreamStringValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			ref string value)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref value);
		}
		/// <summary>Stream the current element's Text as a DamageType</summary>
		internal static void StreamDamageType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.BDamageTypeReference] ref int damangeType)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref damangeType, Phx.DatabaseObjectKind.DamageType,
				false, XmlUtil.kSourceCursor);
		}
		/// <summary>Stream the current element's Text as a ObjectType</summary>
		internal static void StreamObjectType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.ObjectTypeReference] ref int objectType)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref objectType, Phx.DatabaseObjectKind.ObjectType,
				false, XmlUtil.kSourceCursor);
		}
		/// <summary>Stream the current element's Text as a ProtoObject</summary>
		internal static void StreamObjectID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.BProtoObjectReference] ref int objectProtoId)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref objectProtoId, Phx.DatabaseObjectKind.Object,
				false, XmlUtil.kSourceCursor);
		}
		/// <summary>Stream the current element's Text as a ProtoSquad</summary>
		internal static void StreamSquadID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.BProtoSquadReference] ref int squadProtoId)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref squadProtoId, Phx.DatabaseObjectKind.Squad,
				false, XmlUtil.kSourceCursor);
		}
		/// <summary>Stream the current element's Text as a ProtoObject or ObjectType</summary>
		internal static void StreamUnitID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			ref int unitProtoId)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref unitProtoId, Phx.DatabaseObjectKind.Unit,
				false, XmlUtil.kSourceCursor);
		}
	};
}