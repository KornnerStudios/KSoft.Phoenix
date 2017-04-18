using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	using PhxEngineBuild = Engine.PhxEngineBuild;

	partial class BDatabaseXmlSerializerBase
	{
		public enum StreamXmlPriority
		{
			None,

			Lists,
			GameData,
			ProtoData,

			kNumberOf
		};
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
			public StreamXmlPriority Priority;
			public Engine.XmlFileInfo FileInfo;
			public Engine.XmlFileInfo FileInfoWithUpdates;
			public Action<IO.XmlElementStream> Preload;
			public Action<IO.XmlElementStream> Stream;
			public Action<IO.XmlElementStream> StreamUpdates;

			public StreamXmlContextData(Engine.ProtoDataXmlFileInfo protoFileInfo)
			{
				ProtoFileInfo = protoFileInfo;

				// #TODO remove:
				Priority = (StreamXmlPriority)ProtoFileInfo.Priority;
				FileInfo = ProtoFileInfo.FileInfo;
				FileInfoWithUpdates = ProtoFileInfo.FileInfoWithUpdates;
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
		private void ProcessStreamXmlContexts(ref bool r, FA mode, bool synchronous)
		{
			SetupStreamXmlContexts();

			for (var s = StreamXmlStage.Preload; s < StreamXmlStage.kNumberOf; s++)
			{
				List<Task<bool>> tasks = null;
				List<Exception> taskExceptions = null;
				if (!synchronous)
				{
					tasks = new List<Task<bool>>();
					taskExceptions = new List<Exception>();
				}

				for (var p = StreamXmlPriority.Lists; p < StreamXmlPriority.kNumberOf; p++)
				{
					foreach (var ctxt in mStreamXmlContexts)
					{
						if (ctxt.Priority != p)
							continue;

						switch (s)
						{
							#region Preload
							case StreamXmlStage.Preload:
								if (ctxt.Preload == null)
									break;

								if (synchronous)
									r &= TryStreamData(ctxt.FileInfo, mode, ctxt.Preload);
								else
								{
									var task = Task<bool>.Factory.StartNew(() => TryStreamData(ctxt.FileInfo, mode, ctxt.Preload));
									tasks.Add(task);
								}
								break;
							#endregion
							#region Stream
							case StreamXmlStage.Stream:
								if (ctxt.Stream == null)
									break;

								if (synchronous)
									r &= TryStreamData(ctxt.FileInfo, mode, ctxt.Stream);
								else
								{
									var task = Task<bool>.Factory.StartNew(() => TryStreamData(ctxt.FileInfo, mode, ctxt.Stream));
									tasks.Add(task);
								}
								break;
							#endregion
							#region StreamUpdates
							case StreamXmlStage.StreamUpdates:
								if (ctxt.FileInfoWithUpdates == null)
									break;
								if (ctxt.StreamUpdates == null)
									break;

								if (synchronous)
									r &= TryStreamData(ctxt.FileInfoWithUpdates, mode, ctxt.StreamUpdates);
								else
								{
									var task = Task<bool>.Factory.StartNew(() => TryStreamData(ctxt.FileInfoWithUpdates, mode, ctxt.StreamUpdates));
									tasks.Add(task);
								}
								break;
							#endregion
						}

						if (synchronous)
						{
							if (!r)
								throw new InvalidOperationException(string.Format(
									"Failed to process {0} during stage {1}",
									ctxt.FileInfo.FileName, s));
						}
					}

					if (tasks != null)
					{
						if (!UpdateResultWithTaskResults(ref r, tasks.ToArray(), taskExceptions))
						{
							throw new InvalidOperationException(string.Format(
								"Failed to process one or more files for priority={0}",
								p),
								new AggregateException(taskExceptions));
						}

						tasks.Clear();
						taskExceptions.Clear();
					}
				}
			}
		}

		static bool UpdateResultWithTaskResults(ref bool r, Task<bool>[] tasks, List<Exception> exceptions = null)
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
		void StreamTacticsSync(ref bool r, FA mode)
		{
			var xfi = StreamTacticsGetFileInfo(mode);

			var keys_copy = new List<string>(mTacticsMap.Keys);
			foreach (var name in keys_copy)
			{
				xfi.FileName = name;
				r &= TryStreamData(xfi, mode, StreamTactic, xfi.FileName, Phx.BTacticData.kFileExt);
			}
		}
		void StreamTacticsAsync(ref bool r, FA mode)
		{
			var keys_copy = new List<string>(mTacticsMap.Keys);
			var tasks = new Task<bool>[keys_copy.Count];
			int task_index = 0;

			foreach (var name in keys_copy)
			{
				var xfi = StreamTacticsGetFileInfo(mode, name);
				tasks[task_index++] = Task<bool>.Factory.StartNew((state) =>
					TryStreamData(xfi, mode, StreamTactic, (state as Engine.XmlFileInfo).FileName, Phx.BTacticData.kFileExt),
					xfi);
			}
			UpdateResultWithTaskResults(ref r, tasks);
		}
		void StreamTactics(FA mode, bool synchronous)
		{
			if (GameEngine.Build == PhxEngineBuild.Alpha)
			{
				Debug.Trace.XML.TraceInformation("BDatabaseXmlSerializer: Alpha build detected, skipping Tactics streaming");
				return;
			}
			bool r = true;

			if(!synchronous)
				StreamTacticsAsync(ref r, mode);
			else
				StreamTacticsSync(ref r, mode);
		}

		void StreamDataSync(ref bool r, FA mode)
		{
			r &= TryStreamData(Phx.BDatabaseBase.kObjectTypesXmlFileInfo, mode, StreamXmlObjectTypes);
			r &= TryStreamData(Phx.BDamageType.kXmlFileInfo, mode, StreamXmlDamageTypes);
			r &= TryStreamData(Phx.BProtoImpactEffect.kXmlFileInfo, mode, StreamXmlImpactEffects);
			r &= TryStreamData(Phx.BWeaponType.kXmlFileInfo, mode, StreamXmlWeaponTypes);
			r &= TryStreamData(Phx.BUserClass.kXmlFileInfo, mode, StreamXmlUserClasses);

			r &= TryStreamData(Phx.BGameData.kXmlFileInfo, mode, StreamXmlGameData);
			r &= TryStreamData(Phx.BAbility.kXmlFileInfo, mode, StreamXmlAbilities);

			r &= TryStreamData(Phx.BProtoObject.kXmlFileInfo, mode, StreamXmlObjects);
			r &= TryStreamData(Phx.BProtoSquad.kXmlFileInfo, mode, StreamXmlSquads);
			r &= TryStreamData(Phx.BProtoPower.kXmlFileInfo, mode, StreamXmlPowers);
			r &= TryStreamData(Phx.BProtoTech.kXmlFileInfo, mode, StreamXmlTechs);
			r &= TryStreamData(Phx.BCiv.kXmlFileInfo, mode, StreamXmlCivs);
			r &= TryStreamData(Phx.BLeader.kXmlFileInfo, mode, StreamXmlLeaders);
		}
		void StreamDataAsync(ref bool r, FA mode)
		{
#if false
			Task<bool>[] tasks0 = {
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BDatabaseBase.kObjectTypesXmlFileInfo, mode, StreamXmlObjectTypes)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BDamageType.kXmlFileInfo, mode, StreamXmlDamageTypes)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BWeaponType.kXmlFileInfo, mode, StreamXmlWeaponTypes)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BUserClass.kXmlFileInfo, mode, StreamXmlUserClasses)),
			};
			UpdateResultWithTaskResults(ref r, tasks0);
#endif

			Task<bool>[] tasks1 = {
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BGameData.kXmlFileInfo, mode, StreamXmlGameData)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BAbility.kXmlFileInfo, mode, StreamXmlAbilities)),
			};
			UpdateResultWithTaskResults(ref r, tasks1);

			Task<bool>[] tasks2 = {
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoObject.kXmlFileInfo, mode, StreamXmlObjects)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoSquad.kXmlFileInfo, mode, StreamXmlSquads)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoPower.kXmlFileInfo, mode, StreamXmlPowers)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoTech.kXmlFileInfo, mode, StreamXmlTechs)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BCiv.kXmlFileInfo, mode, StreamXmlCivs)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BLeader.kXmlFileInfo, mode, StreamXmlLeaders)),
			};
			UpdateResultWithTaskResults(ref r, tasks2);
		}
		void StreamData(FA mode, bool synchronous)
		{
#if false
			bool r = true;

			if (!synchronous)
				StreamDataAsync(ref r, mode);
			else
				StreamDataSync(ref r, mode);
#else
			bool r = true;
			ProcessStreamXmlContexts(ref r, mode, synchronous: false);
#endif
		}

		void PreloadSync(ref bool r)
		{
			const FA k_mode = FA.Read;

			r &= TryStreamData(Phx.BProtoObject.kXmlFileInfo, k_mode, PreloadObjects);
			r &= TryStreamData(Phx.BProtoSquad.kXmlFileInfo, k_mode, PreloadSquads);

			r &= TryStreamData(Phx.BProtoTech.kXmlFileInfo, k_mode, PreloadTechs);
		}
		void PreloadAsync(ref bool r)
		{
			const FA k_mode = FA.Read;

			Task<bool>[] preload_tasks = {
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BDatabaseBase.kObjectTypesXmlFileInfo, k_mode, StreamXmlObjectTypes)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BDamageType.kXmlFileInfo, k_mode, StreamXmlDamageTypes)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoImpactEffect.kXmlFileInfo, k_mode, StreamXmlImpactEffects)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BWeaponType.kXmlFileInfo, k_mode, StreamXmlWeaponTypes)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BUserClass.kXmlFileInfo, k_mode, StreamXmlUserClasses)),

				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoObject.kXmlFileInfo, k_mode, PreloadObjects)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoSquad.kXmlFileInfo, k_mode, PreloadSquads)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoPower.kXmlFileInfo, k_mode, PreloadPowers)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoTech.kXmlFileInfo, k_mode, PreloadTechs)),
			};
			UpdateResultWithTaskResults(ref r, preload_tasks);
		}
		// objects, objects_update, objectTypes, squads, squads_update, tech, techs_update, leaders, civs, powers, damageTypes
		void Preload(bool synchronous)
		{
#if false
			bool r = true;

			if (!synchronous)
				PreloadAsync(ref r);
			else
				PreloadSync(ref r);
#endif
		}

		void StreamDataUpdatesSync(ref bool r)
		{
			const FA k_mode = FA.Read;

			// In serial mode, we don't need to preload, so don't waste CPU
			r &= TryStreamData(Phx.BProtoObject.kXmlFileInfoUpdate, k_mode, StreamXmlObjectsUpdate);
			r &= TryStreamData(Phx.BProtoSquad.kXmlFileInfoUpdate, k_mode, StreamXmlSquadsUpdate);

			r &= TryStreamData(Phx.BProtoTech.kXmlFileInfoUpdate, k_mode, StreamXmlTechsUpdate);
		}
		void StreamDataUpdatesAsync(ref bool r)
		{
			const FA k_mode = FA.Read;

			Task<bool>[] preload_tasks = {
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoObject.kXmlFileInfoUpdate, k_mode, PreloadObjects)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoSquad.kXmlFileInfoUpdate, k_mode, PreloadSquads)),

				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoTech.kXmlFileInfoUpdate, k_mode, PreloadTechs)),
			};
			UpdateResultWithTaskResults(ref r, preload_tasks);

			Task<bool>[] update_tasks = {
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoObject.kXmlFileInfoUpdate, k_mode, StreamXmlObjectsUpdate)),
				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoSquad.kXmlFileInfoUpdate, k_mode, StreamXmlSquadsUpdate)),

				Task<bool>.Factory.StartNew(() => TryStreamData(Phx.BProtoTech.kXmlFileInfoUpdate, k_mode, StreamXmlTechsUpdate)),
			};
			UpdateResultWithTaskResults(ref r, update_tasks);
		}
		void StreamDataUpdates(bool synchronous)
		{
#if false
			bool r = true;

			if (!synchronous)
				StreamDataUpdatesAsync(ref r);
			else
				StreamDataUpdatesSync(ref r);
#endif
		}


		protected virtual void LoadCore()
		{
		}
		protected virtual void LoadUpdates(bool synchronous)
		{
			StreamDataUpdates(synchronous);
		}

		void LoadImpl(bool synchronous)
		{
			const FA k_mode = FA.Read;

			Preload(synchronous);

			PreStreamXml(k_mode);

			StreamData(k_mode, synchronous);
			LoadCore();
			StreamTactics(k_mode, synchronous);

			PostStreamXml(k_mode);
		}
		public virtual void Load(BDatabaseXmlSerializerLoadFlags flags = 0)
		{
			AutoIdSerializersInitialize();

			bool synchronous = (flags & BDatabaseXmlSerializerLoadFlags.UseSynchronousLoading) != 0;
			LoadImpl(synchronous);
			if ((flags & BDatabaseXmlSerializerLoadFlags.LoadUpdates) != 0)
				LoadUpdates(synchronous);

			AutoIdSerializersDispose();
		}
	};
}