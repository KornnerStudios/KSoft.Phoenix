using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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
		};
		private List<StreamXmlContextData> mStreamXmlContexts;
		private void SetupStreamXmlContexts()
		{
			if (mStreamXmlContexts != null)
				return;

			// #NOTE place new DatabaseObjectKind code here

			mStreamXmlContexts = new List<StreamXmlContextData>()
			{
				#region Lists
				new StreamXmlContextData(Phx.BDatabaseBase.kObjectTypesProtoFileInfo)
				{
					Preload=StreamXmlObjectTypes,
				},
				new StreamXmlContextData(Phx.BDamageType.kProtoFileInfo)
				{
					Preload=PreloadDamageTypes,
					Stream=StreamXmlDamageTypes,
				},
				new StreamXmlContextData(Phx.BProtoImpactEffect.kProtoFileInfo)
				{
					Preload=StreamXmlImpactEffects,
				},
				new StreamXmlContextData(Phx.TerrainTileType.kProtoFileInfo)
				{
					Preload=StreamXmlTerrainTileTypes,
				},
				new StreamXmlContextData(Phx.BWeaponType.kProtoFileInfo)
				{
					Preload=StreamXmlWeaponTypes,
				},
				new StreamXmlContextData(Phx.BUserClass.kProtoFileInfo)
				{
					Preload=StreamXmlUserClasses,
				},
				#endregion

				#region GameData
				new StreamXmlContextData(Phx.HPBarData.kProtoFileInfo)
				{
					Stream=StreamXmlHPBars,
				},
				new StreamXmlContextData(Phx.BGameData.kProtoFileInfo)
				{
					Stream=StreamXmlGameData,
				},
				new StreamXmlContextData(Phx.BAbility.kProtoFileInfo)
				{
					Stream=StreamXmlAbilities,
				},
				#endregion

				#region ProtoData
				new StreamXmlContextData(Phx.BProtoObject.kProtoFileInfo)
				{
					Preload=PreloadObjects,
					Stream=StreamXmlObjects,
					StreamUpdates=StreamXmlObjectsUpdate,
				},
				new StreamXmlContextData(Phx.BProtoSquad.kProtoFileInfo)
				{
					Preload=PreloadSquads,
					Stream=StreamXmlSquads,
					StreamUpdates=StreamXmlSquadsUpdate,
				},
				new StreamXmlContextData(Phx.BProtoPower.kProtoFileInfo)
				{
					Preload=PreloadPowers,
					Stream=StreamXmlPowers,
				},
				new StreamXmlContextData(Phx.BProtoTech.kProtoFileInfo)
				{
					Preload=PreloadTechs,
					Stream=StreamXmlTechs,
					StreamUpdates=StreamXmlTechsUpdate,
				},

				new StreamXmlContextData(Phx.BCiv.kProtoFileInfo)
				{
					//Preload=PreloadCivs,
					Stream=StreamXmlCivs,
				},
				new StreamXmlContextData(Phx.BLeader.kProtoFileInfo)
				{
					//Preload=PreloadLeaders,
					Stream=StreamXmlLeaders,
				},
				#endregion
			};
		}
		private void ProcessStreamXmlContexts(ref bool r, FA mode, bool synchronous = false)
		{
			ProcessStreamXmlContexts(ref r, mode, synchronous
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

			public bool Synchronous { get { return Tasks == null; } }

			public ProcessStreamXmlContextStageArgs(FA mode, bool synchronous, StreamXmlStage stage
				, Engine.XmlFilePriority firstPriority
				, Engine.XmlFilePriority lastPriorityPlusOne)
			{
				Mode = mode;
				Stage = stage;
				FirstPriority = firstPriority;
				LastPriorityPlusOne = lastPriorityPlusOne;

				Tasks = null;
				TaskExceptions = null;
				if (!synchronous)
				{
					Tasks = new List<Task<bool>>();
					TaskExceptions = new List<Exception>();
				}
			}

			public bool UpdateResultWithTaskResults(ref bool r)
			{
				BDatabaseXmlSerializerBase.UpdateResultWithTaskResults(ref r, Tasks, TaskExceptions);

				return r;
			}

			public void ClearTaskData()
			{
				Tasks.Clear();
				TaskExceptions.Clear();
			}
		};

		private void ProcessStreamXmlContexts(ref bool r, FA mode, bool synchronous
			, StreamXmlStage firstStage// = StreamXmlStage.Preload
			, StreamXmlStage lastStagePlusOne// = StreamXmlStage.kNumberOf
			, Engine.XmlFilePriority firstPriority// = Engine.XmlFilePriority.Lists
			, Engine.XmlFilePriority lastPriorityPlusOne// = Engine.XmlFilePriority.kNumberOf
			)
		{
			SetupStreamXmlContexts();

			for (var s = firstStage; s < lastStagePlusOne; s++)
			{
				var args = new ProcessStreamXmlContextStageArgs(mode, synchronous, s, firstPriority, lastPriorityPlusOne);
				ProcessStreamXmlContextStage(ref r, args);
			}
		}

		private void ProcessStreamXmlContextStage(ref bool r, ProcessStreamXmlContextStageArgs args)
		{
			var mode = args.Mode;

			for (var p = args.FirstPriority; p < args.LastPriorityPlusOne; p++)
			{
				foreach (var ctxt in mStreamXmlContexts)
				{
					if (ctxt.ProtoFileInfo.Priority != p)
						continue;

					switch (args.Stage)
					{
						#region Preload
						case StreamXmlStage.Preload:
							if (ctxt.Preload == null)
								break;

							if (args.Synchronous)
								r &= TryStreamData(ctxt.FileInfo, mode, ctxt.Preload);
							else
							{
								var task = Task<bool>.Factory.StartNew(() => TryStreamData(ctxt.FileInfo, mode, ctxt.Preload));
								args.Tasks.Add(task);
							}
							break;
						#endregion
						#region Stream
						case StreamXmlStage.Stream:
							if (ctxt.Stream == null)
								break;

							if (args.Synchronous)
								r &= TryStreamData(ctxt.FileInfo, mode, ctxt.Stream);
							else
							{
								var task = Task<bool>.Factory.StartNew(() => TryStreamData(ctxt.FileInfo, mode, ctxt.Stream));
								args.Tasks.Add(task);
							}
							break;
						#endregion
						#region StreamUpdates
						case StreamXmlStage.StreamUpdates:
							if (ctxt.FileInfoWithUpdates == null)
								break;
							if (ctxt.StreamUpdates == null)
								break;

							if (args.Synchronous)
								r &= TryStreamData(ctxt.FileInfoWithUpdates, mode, ctxt.StreamUpdates);
							else
							{
								var task = Task<bool>.Factory.StartNew(() => TryStreamData(ctxt.FileInfoWithUpdates, mode, ctxt.StreamUpdates));
								args.Tasks.Add(task);
							}
							break;
						#endregion
					}

					if (args.Synchronous)
					{
						if (!r)
							throw new InvalidOperationException(string.Format(
								"Failed to process {0} during stage {1}",
								ctxt.FileInfo.FileName, args.Stage));
					}
				}

				if (args.Tasks != null)
				{
					if (!args.UpdateResultWithTaskResults(ref r))
					{
						throw new InvalidOperationException(string.Format(
							"Failed to process one or more files for priority={0}",
							p),
							new AggregateException(args.TaskExceptions));
					}

					args.ClearTaskData();
				}
			}
		}

		static bool UpdateResultWithTaskResults(ref bool r, List<Task<bool>> tasks, List<Exception> exceptions = null)
		{
			foreach (var task in tasks)
			{
				if (task.IsFaulted)
				{
					r = false;
					if (exceptions != null)
						exceptions.Add(task.Exception);
				}
				r &= task.Result;
			}

			return r;
		}

		void StreamTacticsAsync(ref bool r, FA mode)
		{
			var tactics = Database.Tactics;
			var tasks = new List<Task<bool>>(tactics.Count);

			foreach (var tactic in tactics)
			{
				if (mode == FA.Read)
				{
					if (tactic.SourceXmlFile != null)
						continue;

					tactic.SourceXmlFile = StreamTacticsGetFileInfo(mode, tactic.Name);
				}
				else if (mode == FA.Write)
				{
					Contract.Assert(tactic.SourceXmlFile != null, tactic.Name);
				}

				var arg = tactic;
				var task = Task<bool>.Factory.StartNew((state) =>
				{
					var _tactic = state as Phx.BTacticData;
					return TryStreamData(_tactic.SourceXmlFile, mode, StreamTactic, _tactic, Phx.BTacticData.kFileExt);
				}, arg);
				tasks.Add(task);
			}
			UpdateResultWithTaskResults(ref r, tasks);
		}
		void StreamTactics(FA mode)
		{
			if (GameEngine.Build == PhxEngineBuild.Alpha)
			{
				Debug.Trace.XML.TraceInformation("BDatabaseXmlSerializer: Alpha build detected, skipping Tactics streaming");
				return;
			}

			bool r = true;
			StreamTacticsAsync(ref r, mode);
		}

		void StreamData(FA mode)
		{
			bool r = true;
			ProcessStreamXmlContexts(ref r, mode);
		}

		void LoadImpl(bool autoLoadTactics)
		{
			const FA k_mode = FA.Read;

			PreStreamXml(k_mode);

			StreamData(k_mode);

			if (autoLoadTactics)
				StreamTactics(k_mode);

			PostStreamXml(k_mode);
		}
		public virtual void Load(BDatabaseXmlSerializerLoadFlags flags = 0)
		{
			AutoIdSerializersInitialize();

			bool auto_load_tactics = (flags & BDatabaseXmlSerializerLoadFlags.DoNotAutoLoadTactics) == 0;
			LoadImpl(auto_load_tactics);

			AutoIdSerializersDispose();
		}
	};
}