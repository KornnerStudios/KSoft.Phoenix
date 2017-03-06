using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	using PhxEngineBuild = Engine.PhxEngineBuild;

	partial class BDatabaseXmlSerializerBase
	{
		static bool UpdateResultWithTaskResults(ref bool r, Task<bool>[] tasks)
		{
			foreach (var task in tasks)
				r &= task.Result;

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
			bool r = true;

			if (!synchronous)
				StreamDataAsync(ref r, mode);
			else
				StreamDataSync(ref r, mode);
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
			bool r = true;

			if (!synchronous)
				PreloadAsync(ref r);
			else
				PreloadSync(ref r);
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
			bool r = true;

			if (!synchronous)
				StreamDataUpdatesAsync(ref r);
			else
				StreamDataUpdatesSync(ref r);
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