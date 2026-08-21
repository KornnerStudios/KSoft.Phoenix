using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	using PhxEngineBuild = Engine.PhxEngineBuild;

	partial class BDatabaseXmlSerializerBase
	{
		public enum StreamXmlStage
		{
			Preload,
			Stream,
			StreamUpdates,

			kNumberOf
		};
		public delegate void StreamXmlCallback(IO.XmlElementStream s);

		[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
		public sealed class StreamXmlContextData
		{
			public Engine.ProtoDataXmlFileInfo ProtoFileInfo;
			public Engine.XmlFileInfo FileInfo { get { return ProtoFileInfo.FileInfo; } }
			public Engine.XmlFileInfo FileInfoWithUpdates { get { return ProtoFileInfo.FileInfoWithUpdates; } }
			public Action<IO.XmlElementStream> Preload;
			public Action<IO.XmlElementStream> Stream;
			public Action<IO.XmlElementStream> StreamUpdates;

			public StreamXmlContextData(Engine.ProtoDataXmlFileInfo protoFileInfo)
			{
				ProtoFileInfo = protoFileInfo;
			}

			private string GetDebuggerDisplay()
			{
				return FileInfo.ToString();
			}
		};
		private List<StreamXmlContextData> mStreamXmlContexts;
		private void SetupStreamXmlContexts()
		{
			if (mStreamXmlContexts != null)
			{
				return;
			}

			// #NOTE place new DatabaseObjectKind code here

			mStreamXmlContexts = new List<StreamXmlContextData>()
			{
				#region Lists
				new(Phx.LocStringTable.kProtoFileInfoEnglish)
				{
					Preload=PreloadStringTable,
				},
				new(Phx.BDatabaseBase.kObjectTypesProtoFileInfo)
				{
					Preload=StreamXmlObjectTypes,
				},
				new(Phx.BDamageType.kProtoFileInfo)
				{
					Preload=PreloadDamageTypes,
					Stream=StreamXmlDamageTypes,
				},
				new(Phx.BProtoImpactEffect.kProtoFileInfo)
				{
					Preload=StreamXmlImpactEffects,
				},
				new(Phx.TerrainTileType.kProtoFileInfo)
				{
					Preload=StreamXmlTerrainTileTypes,
				},
				new(Phx.BWeaponType.kProtoFileInfo)
				{
					// BWeaponType's BWeaponModifier list requires BDamageTypes to be loaded first
					Preload=PreloadWeaponTypes,
					Stream=StreamXmlWeaponTypes,
				},
				new(Phx.BUserClass.kProtoFileInfo)
				{
					Preload=StreamXmlUserClasses,
				},
				#endregion

				#region GameData
				new(Phx.HPBarData.kProtoFileInfo)
				{
					Stream=StreamXmlHPBars,
				},
				new(Phx.BGameData.kProtoFileInfo)
				{
					Stream=StreamXmlGameData,
				},
				new(Phx.BAbility.kProtoFileInfo)
				{
					Stream=StreamXmlAbilities,
				},
				#endregion

				#region ProtoData
				new(Phx.BProtoObject.kProtoFileInfo)
				{
					Preload=PreloadObjects,
					Stream=StreamXmlObjects,
					StreamUpdates=StreamXmlObjectsUpdate,
				},
				new(Phx.BProtoSquad.kProtoFileInfo)
				{
					Preload=PreloadSquads,
					Stream=StreamXmlSquads,
					StreamUpdates=StreamXmlSquadsUpdate,
				},
				new(Phx.BProtoPower.kProtoFileInfo)
				{
					Preload=PreloadPowers,
					Stream=StreamXmlPowers,
				},
				new(Phx.BProtoTech.kProtoFileInfo)
				{
					Preload=PreloadTechs,
					Stream=StreamXmlTechs,
					StreamUpdates=StreamXmlTechsUpdate,
				},

				new(Phx.BCiv.kProtoFileInfo)
				{
					//Preload=PreloadCivs,
					Stream=StreamXmlCivs,
				},
				new(Phx.BLeader.kProtoFileInfo)
				{
					//Preload=PreloadLeaders,
					Stream=StreamXmlLeaders,
				},
				#endregion
			};
		}
		private void ProcessStreamXmlContexts(ref bool r, FA mode)
		{
			ProcessStreamXmlContexts(ref r, mode
				, StreamXmlStage.Preload, StreamXmlStage.kNumberOf
				, Engine.XmlFilePriority.Lists, Engine.XmlFilePriority.kNumberOf);
		}

		private struct ProcessStreamXmlContextStageArgs
		{
			public FA Mode;
			public StreamXmlStage Stage;
			public Engine.XmlFilePriority FirstPriority;
			public Engine.XmlFilePriority LastPriorityPlusOne;

			public List<Task<bool>> Tasks;
			public List<Exception> TaskExceptions;

			public ProcessStreamXmlContextStageArgs(FA mode, StreamXmlStage stage
				, Engine.XmlFilePriority firstPriority
				, Engine.XmlFilePriority lastPriorityPlusOne)
			{
				Mode = mode;
				Stage = stage;
				FirstPriority = firstPriority;
				LastPriorityPlusOne = lastPriorityPlusOne;

				Tasks = null;
				TaskExceptions = null;
				Tasks = new List<Task<bool>>();
				TaskExceptions = new List<Exception>();
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0251:Make member 'readonly'")]
			public bool UpdateResultWithTaskResults(ref bool r)
			{
				PhxUtil.UpdateResultWithTaskResults(ref r, Tasks, TaskExceptions);

				return r;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0251:Make member 'readonly'")]
			public void ClearTaskData()
			{
				Tasks.Clear();
				TaskExceptions.Clear();
			}
		};

		private void ProcessStreamXmlContexts(ref bool r, FA mode
			, StreamXmlStage firstStage// = StreamXmlStage.Preload
			, StreamXmlStage lastStagePlusOne// = StreamXmlStage.kNumberOf
			, Engine.XmlFilePriority firstPriority = Engine.XmlFilePriority.Lists
			, Engine.XmlFilePriority lastPriorityPlusOne = Engine.XmlFilePriority.kNumberOf
			)
		{
			SetupStreamXmlContexts();

			for (var s = firstStage; s < lastStagePlusOne; s++)
			{
				var args = new ProcessStreamXmlContextStageArgs(mode, s, firstPriority, lastPriorityPlusOne);
				ProcessStreamXmlContextStage(ref r, args);
			}
		}

		private void ProcessStreamXmlContextStage(ref bool r, ProcessStreamXmlContextStageArgs args)
		{
			var mode = args.Mode;

			for (Engine.XmlFilePriority p = args.FirstPriority; p < args.LastPriorityPlusOne; p++)
			{
				foreach (StreamXmlContextData ctxt in mStreamXmlContexts)
				{
					if (ctxt.ProtoFileInfo.Priority != p)
					{
						continue;
					}

					switch (args.Stage)
					{
						#region Preload
						case StreamXmlStage.Preload:
						{
							if (ctxt.Preload == null)
							{
								break;
							}

							var task = Task<bool>.Factory.StartNew(
								() => TryStreamData(ctxt.FileInfo, mode, ctxt.Preload));
							args.Tasks.Add(task);
						} break;
						#endregion
						#region Stream
						case StreamXmlStage.Stream:
						{
							if (ctxt.Stream == null)
							{
								break;
							}

							var task = Task<bool>.Factory.StartNew(
								() => TryStreamData(ctxt.FileInfo, mode, ctxt.Stream));
							args.Tasks.Add(task);
						} break;
						#endregion
						#region StreamUpdates
						case StreamXmlStage.StreamUpdates:
						{
							if (ctxt.FileInfoWithUpdates == null)
							{
								break;
							}

							if (ctxt.StreamUpdates == null)
							{
								break;
							}

							var task = Task<bool>.Factory.StartNew(
								() => TryStreamData(ctxt.FileInfoWithUpdates, mode, ctxt.StreamUpdates));
							args.Tasks.Add(task);
						} break;
						#endregion
					}
				}

				if (!args.UpdateResultWithTaskResults(ref r))
				{
					var inner = args.TaskExceptions.ToAggregateExceptionOrNull().GetOnlyExceptionOrAllWhenAggregate();

					throw new InvalidOperationException(string.Format(
						"Failed to process one or more files for priority={0}",
						p),
						inner);
				}

				args.ClearTaskData();
			}
		}

		void StreamTacticsAsync(ref bool r, FA mode)
		{
			var tactics = Database.Tactics;
			var tasks = new List<Task<bool>>(tactics.Count);
			var task_exceptions = new List<Exception>(tactics.Count);

			foreach (var tactic in tactics)
			{
				if (mode == FA.Read)
				{
					if (tactic.SourceXmlFile != null)
					{
						continue;
					}

					tactic.SourceXmlFile = Phx.BTacticData.CreateFileInfo(mode, tactic.Name);
				}
				else if (mode == FA.Write)
				{
					if (tactic.SourceXmlFile == null)
					{
						throw new InvalidOperationException(string.Format(
							"Tactic '{0}' must have source XML metadata before writing.",
							tactic.Name));
					}
				}

				var engine = this.GameEngine;
				if (mode == FA.Read)
				{
					engine.UpdateFileLoadStatus(tactic.SourceXmlFile, Engine.XmlFileLoadState.Loading);
				}

				var arg = tactic;
				var task = Task<bool>.Factory.StartNew((state) =>
				{
					var _tactic = state as Phx.BTacticData;
					return TryStreamData(_tactic.SourceXmlFile, mode, StreamTactic, _tactic, Phx.BTacticData.kFileExt);
				}, arg);
				tasks.Add(task);
			}
			PhxUtil.UpdateResultWithTaskResults(ref r, tasks, task_exceptions);

			if (!r)
			{
				Debug.Trace.XML.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone,
					"Failed to " + mode + " tactics",
					task_exceptions.ToAggregateExceptionOrNull().GetOnlyExceptionOrAllWhenAggregate());
			}
		}
		bool StreamTactics(FA mode)
		{
			if (GameEngine.Build == PhxEngineBuild.Alpha)
			{
				Debug.Trace.XML.TraceInformation("BDatabaseXmlSerializer: Alpha build detected, skipping Tactics streaming");
				return false;
			}

			bool r = true;
			StreamTacticsAsync(ref r, mode);
			return r;
		}

		void StreamData(FA mode)
		{
			bool r = true;
			ProcessStreamXmlContexts(ref r, mode);
		}

		private bool mIsPreloading;
		protected bool IsNotPreloading { get { return !mIsPreloading; } }
		public bool Preload()
		{
			if (Database.LoadState == Phx.DatabaseLoadState.Failed)
			{
				Debug.Trace.Phoenix.TraceInformation("Not preloading Database because an earlier load stage failed");
				return false;
			}
			if (Database.LoadState >= Phx.DatabaseLoadState.Preloading)
			{
				Debug.Trace.Phoenix.TraceInformation("Skipping preloading of Database because it already is at a later stage");
				return true;
			}

			const FA k_mode = FA.Read;

			mIsPreloading = true;
			Database.LoadState = Phx.DatabaseLoadState.Preloading;

			AutoIdSerializersInitialize();
			PreStreamXml(k_mode);

			bool r = true;
			ProcessStreamXmlContexts(ref r, k_mode
				, StreamXmlStage.Preload
				, StreamXmlStage.Stream);

			PostStreamXml(k_mode);
			//AutoIdSerializersDispose();

			mIsPreloading = false;
			Database.LoadState = r
				? Phx.DatabaseLoadState.Preloaded
				: Phx.DatabaseLoadState.Failed;

			return r;
		}

		public bool Load()
		{
			if (Database.LoadState == Phx.DatabaseLoadState.Failed)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Database because an earlier load stage failed");
				return false;
			}
			if (Database.LoadState >= Phx.DatabaseLoadState.Loading)
			{
				Debug.Trace.Phoenix.TraceInformation("Skipping loading of Database because it already is at a later stage");
				return true;
			}

			const FA k_mode = FA.Read;

			Database.LoadState = Phx.DatabaseLoadState.Loading;
			AutoIdSerializersInitialize();
			PreStreamXml(k_mode);

			bool r = true;
			ProcessStreamXmlContexts(ref r, k_mode
				, StreamXmlStage.Stream
				, StreamXmlStage.kNumberOf);

			PostStreamXml(k_mode);
			//AutoIdSerializersDispose();

			Database.LoadState = r
				? Phx.DatabaseLoadState.Loaded
				: Phx.DatabaseLoadState.Failed;

			return r;
		}

		public bool LoadAllTactics()
		{
			if (Database.LoadState == Phx.DatabaseLoadState.Failed)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Tactics because an earlier load stage failed");
				return false;
			}
			if (Database.LoadState < Phx.DatabaseLoadState.Preloaded)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Tactics because an earlier the database is not at least preloaded");
				return true;
			}

			const FA k_mode = FA.Read;

			PreStreamXml(k_mode);

			bool r = StreamTactics(k_mode);

			PostStreamXml(k_mode);

			if (!r)
			{
				Database.LoadState = Phx.DatabaseLoadState.Failed;
			}

			return r;
		}
	};
}