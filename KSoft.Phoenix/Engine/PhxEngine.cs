using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Engine
{
	public partial class PhxEngine
	{
		public PhxEngineBuild Build { get; private set; }

		public bool TargetsXbox360 { get; private set; }

		public GameDirectories? Directories { get; private set; }

		public Phx.BDatabaseBase? Database { get; private set; }

		public Phx.TriggerDatabase? TriggerDb { get; private set; }

		internal Dictionary<XmlFileInfo, XmlFileLoadState> XmlFileLoadStatus { get; private set; }
			= new();

		public event EventHandler<XmlFileLoadStateChangedEventArgs>? XmlFileLoadStateChanged;

		internal void UpdateFileLoadStatus(XmlFileInfo file, XmlFileLoadState state)
		{
			ArgumentNullException.ThrowIfNull(file);

			lock (XmlFileLoadStatus)
			{
				XmlFileLoadStatus[file] = state;
			}

			var handler = XmlFileLoadStateChanged;
			if (handler != null)
			{
				var args = new XmlFileLoadStateChangedEventArgs(file, state);
				handler(this, args);
			}
		}

		public XmlFileLoadState GetFileLoadStatus(XmlFileInfo file)
		{
			ArgumentNullException.ThrowIfNull(file);

			XmlFileLoadState state;
			lock (XmlFileLoadStatus)
			{
				if (!XmlFileLoadStatus.TryGetValue(file, out state))
				{
					return XmlFileLoadState.NotLoaded;
				}
			}

			return state;
		}

		public bool HasAlreadyPreloaded { get; private set; }
		public virtual bool Preload()
		{
			if (HasAlreadyPreloaded)
			{
				Debug.Trace.Engine.TraceDataSansId(System.Diagnostics.TraceEventType.Error,
					"Failed to load preload data: " + "preload has already be performed");
				return false;
			}

			var database = Database ?? throw new NullReferenceException();
			bool success = database.Preload();

			if (success)
			{
				HasAlreadyPreloaded = true;
			}

			return success;
		}

		public bool HasAlreadyLoaded { get; private set; }
		public virtual bool Load()
		{
			Exception? exception = null;
			bool success = false;

			try
			{
				do
				{
					if (HasAlreadyLoaded)
					{
						exception = new Exception("Load has already been performed");
						break;
					}

					var database = Database ?? throw new NullReferenceException();

					if (!database.Load())
					{
						exception = new Exception("Database.Load failed");
						break;
					}

					if (!database.LoadAllTactics())
					{
						exception = new Exception("Database.LoadAllTactics failed");
						break;
					}

					success = true;

				} while (false);
			} catch (Exception ex)
			{
				exception = ex;
			}

			if (!success)
			{
				Debug.Trace.Engine.TraceDataSansId(System.Diagnostics.TraceEventType.Error,
					"Failed to load engine data",
					exception);
			}
			else
			{
				HasAlreadyLoaded = true;
			}

			return success;
		}

		public IO.XmlElementStream OpenXmlOrXmbForRead(string fileName)
		{
			string ext = System.IO.Path.GetFileNameWithoutExtension(fileName);

			var xml_or_xmb = ext.Equals(Xmb.XmbFile.kFileExt, StringComparison.OrdinalIgnoreCase)
				? GetXmlOrXmbFileResult.Xmb
				: GetXmlOrXmbFileResult.Xml;

			return xml_or_xmb == GetXmlOrXmbFileResult.Xmb
				? OpenXmbForRead(fileName)
				: new IO.XmlElementStream(fileName, System.IO.FileAccess.Read);
		}

		public IO.XmlElementStream? OpenXmlOrXmbForRead(GetXmlOrXmbFileResult xmlOrXmb, string fileName)
		{
			return xmlOrXmb switch
			{
				GetXmlOrXmbFileResult.Xml => new IO.XmlElementStream(fileName, System.IO.FileAccess.Read),
				GetXmlOrXmbFileResult.Xmb => OpenXmbForRead(fileName),
				_ => null,
			};
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

		public ObjectDatabaseForFileResult GetObjectDatabase(XmlFileInfo? file)
		{
			if (file == null)
			{
				return ObjectDatabaseForFileResult.Null;
			}

			var status = GetFileLoadStatus(file);
			if (status < XmlFileLoadState.Preloaded)
			{
				throw new InvalidOperationException(string.Format(
					"GetObjectDatabase called on {0} when its load status was {1}",
					file, status));
			}

			var kvp = GetObjectDatabaseForFile(file);

			if (kvp.Key == null)
			{
				throw new InvalidOperationException(string.Format(
					"GetObjectDatabase called on {0} which didn't resolve to a DB",
					file));
			}

			return new ObjectDatabaseForFileResult(file, kvp.Key, kvp.Value);
		}

		protected virtual KeyValuePair<Phx.ProtoDataObjectDatabase?, int> GetObjectDatabaseForFile(XmlFileInfo file)
		{
			var database = Database ?? throw new NullReferenceException();
			Phx.ProtoDataObjectDatabase? db = null;
			int specificObjectKind = TypeExtensions.kNone;

			if (file == Phx.BGameData.kXmlFileInfo)
			{
				db = database.GameData.ObjectDatabase;
				specificObjectKind = PhxUtil.kObjectKindNone;
			}
			else if (file == Phx.HPBarData.kXmlFileInfo)
			{
				db = database.HPBars.ObjectDatabase;
				specificObjectKind = PhxUtil.kObjectKindNone;
			}
			else if (file == Phx.BAbility.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Ability;
			}
			else if (file == Phx.BCiv.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Civ;
			}
			else if (file == Phx.BDamageType.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.DamageType;
			}
			else if (file == Phx.BProtoImpactEffect.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.ImpactEffect;
			}
			else if (file == Phx.BLeader.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Leader;
			}
			else if (
				file == Phx.BProtoObject.kXmlFileInfo ||
				file == Phx.BProtoObject.kXmlFileInfoUpdate)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Object;
			}
			else if (file == Phx.BDatabaseBase.kObjectTypesXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.ObjectType;
			}
			else if (file == Phx.BProtoPower.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Power;
			}
			else if (
				file == Phx.BProtoSquad.kXmlFileInfo ||
				file == Phx.BProtoSquad.kXmlFileInfoUpdate)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Squad;
			}
			else if (file.Directory == GameDirectory.Tactics)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Tactic;
			}
			else if (
				file == Phx.BProtoTech.kXmlFileInfo ||
				file == Phx.BProtoTech.kXmlFileInfoUpdate)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Tech;
			}
			else if (file == Phx.TerrainTileType.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.TerrainTileType;
			}
			else if (file == Phx.BUserClass.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.UserClass;
			}
			else if (file == Phx.BWeaponType.kXmlFileInfo)
			{
				db = database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.WeaponType;
			}
			else if (
				file.Directory == GameDirectory.AbilityScripts ||
				file.Directory == GameDirectory.PowerScripts ||
				file.Directory == GameDirectory.TriggerScripts)
			{
				throw new NotImplementedException(file.ToString());
			}

			return new KeyValuePair<Phx.ProtoDataObjectDatabase?, int>(db, specificObjectKind);
		}
	};

	public struct ObjectDatabaseForFileResult
	{
		public XmlFileInfo? File { get; private set; }
		public Phx.ProtoDataObjectDatabase? Database { get; private set; }
		public int SpecificObjectKind { get; private set; }

		public ObjectDatabaseForFileResult(XmlFileInfo? file, Phx.ProtoDataObjectDatabase? db, int objectKind)
		{
			File = file;
			Database = db;
			SpecificObjectKind = objectKind;
		}

		public readonly bool IsNull => Database == null;

		public static ObjectDatabaseForFileResult Null
			=> new(null, null, TypeExtensions.kNone);
	};
}