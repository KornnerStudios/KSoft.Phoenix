using System.Collections.Generic;
using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	partial class BDatabaseXmlSerializerBase
	{
		protected virtual void PreStreamXml(FA mode)
		{ }
		protected virtual void PostStreamXml(FA mode)
		{
			if (mode == FA.Read)
			{
				Database.BuildObjectTacticsMap(mObjectIdToTacticsMap, mTacticsMap);
			}
		}

		Dictionary<int, string> mObjectIdToTacticsMap;
		Dictionary<string, Phx.BTacticData> mTacticsMap;

		static Engine.XmlFileInfo StreamTacticsGetFileInfo(FA mode, string filename = null)
		{
			return new Engine.XmlFileInfo()
			{
				Location = Engine.ContentStorage.UpdateOrGame,
				Directory = Engine.GameDirectory.Tactics,

				RootName = Phx.BTacticData.kXmlRoot,
				FileName = filename,

				Writable = mode == FA.Write,
			};
		}
		void StreamTactic(IO.XmlElementStream s, string name)
		{
			var td = new Phx.BTacticData(name);

			if (s.IsReading)
				FixTacticsXml(s, name);
			td.Serialize(s);

			mTacticsMap[name] = td;
		}

		public bool ForceNoRootElementStreaming = true;

		/// <remarks>For streaming directly from gamedata.xml</remarks>
		void StreamXmlGameData(IO.XmlElementStream s)
		{
			if(s.IsReading)
				FixGameDataXml(s);
			Database.GameData.StreamGameData(s);
		}

		// #NOTE place new DatabaseObjectKind code here

		void PreloadDamageTypes(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, mDamageTypesSerializer, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from damagetypes.xml</remarks>
		void StreamXmlDamageTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, mDamageTypesSerializer, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from impacteffects.xml</remarks>
		void StreamXmlImpactEffects(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, mImpactEffectsSerializer, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from terraintiletypes.xml</remarks>
		void StreamXmlTerrainTileTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, Database.TerrainTileTypes, Phx.TerrainTileType.kBListXmlParams, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from weapontypes.xml</remarks>
		void StreamXmlWeaponTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, Database.WeaponTypes, Phx.BWeaponType.kBListXmlParams, ForceNoRootElementStreaming);
			if (s.IsReading) FixWeaponTypes();
		}
		/// <remarks>For streaming directly from UserClasses.xml</remarks>
		void StreamXmlUserClasses(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, Database.UserClasses, Phx.BUserClass.kBListXmlParams, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from objecttypes.xml</remarks>
		void StreamXmlObjectTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, Database.ObjectTypes, Phx.BDatabaseBase.kObjectTypesXmlParams, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from abilities.xml</remarks>
		void StreamXmlAbilities(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, Database.Abilities, Phx.BAbility.kBListXmlParams, ForceNoRootElementStreaming);
		}

		void PreloadObjects(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, mObjectsSerializer, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from objects.xml</remarks>
		void StreamXmlObjects(IO.XmlElementStream s)
		{
			if (s.IsReading)
				FixObjectsXml(s);
			XmlUtil.Serialize(s, mObjectsSerializer, ForceNoRootElementStreaming);
		}

		void PreloadSquads(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, mSquadsSerializer, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from squads.xml</remarks>
		void StreamXmlSquads(IO.XmlElementStream s)
		{
			if (s.IsReading)
				FixSquadsXml(s);
			XmlUtil.Serialize(s, mSquadsSerializer, ForceNoRootElementStreaming);
		}

		void PreloadPowers(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, mPowersSerializer, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from powers.xml</remarks>
		void StreamXmlPowers(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, mPowersSerializer, ForceNoRootElementStreaming);
		}

		void PreloadTechs(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, mTechsSerializer, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from techs.xml</remarks>
		void StreamXmlTechs(IO.XmlElementStream s)
		{
			if (s.IsReading)
				FixTechsXml(s);
			XmlUtil.Serialize(s, mTechsSerializer, ForceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from civs.xml</remarks>
		void StreamXmlCivs(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, Database.Civs, Phx.BCiv.kBListXmlParams, ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from leaders.xml</remarks>
		void StreamXmlLeaders(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, Database.Leaders, Phx.BLeader.kBListXmlParams, ForceNoRootElementStreaming);
		}

		#region Update
		/// <remarks>For streaming directly from objects_update.xml</remarks>
		void StreamXmlObjectsUpdate(IO.XmlElementStream s)
		{
			//if(s.IsReading) FixObjectsXml(s);
			XmlUtil.SerializeUpdate(s, mObjectsSerializer, ForceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from squads_update.xml</remarks>
		void StreamXmlSquadsUpdate(IO.XmlElementStream s)
		{
			//if (s.IsReading) FixSquadsXml(s);
			XmlUtil.SerializeUpdate(s, mSquadsSerializer, ForceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from techs_update.xml</remarks>
		void StreamXmlTechsUpdate(IO.XmlElementStream s)
		{
			if (s.IsReading)
				FixTechsXml(s);
			XmlUtil.SerializeUpdate(s, mTechsSerializer, ForceNoRootElementStreaming);
		}
		#endregion

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var db = Database;

			PreStreamXml(s.StreamMode);

			db.GameData.Serialize(s);
			// #NOTE place new DatabaseObjectKind code here
			XmlUtil.Serialize(s, db.DamageTypes, Phx.BDamageType.kBListXmlParams);
			XmlUtil.Serialize(s, db.WeaponTypes, Phx.BWeaponType.kBListXmlParams);
			XmlUtil.Serialize(s, db.ImpactEffects, Phx.BProtoImpactEffect.kBListXmlParams);
			XmlUtil.Serialize(s, db.TerrainTileTypes, Phx.TerrainTileType.kBListXmlParams);
			XmlUtil.Serialize(s, db.UserClasses, Phx.BUserClass.kBListXmlParams);
			XmlUtil.Serialize(s, db.ObjectTypes, Phx.BDatabaseBase.kObjectTypesXmlParams);
			XmlUtil.Serialize(s, db.Abilities, Phx.BAbility.kBListXmlParams);
			XmlUtil.Serialize(s, db.Objects, Phx.BProtoObject.kBListXmlParams);
			XmlUtil.Serialize(s, db.Squads, Phx.BProtoSquad.kBListXmlParams);
			XmlUtil.Serialize(s, db.Powers, Phx.BProtoPower.kBListXmlParams);
			XmlUtil.Serialize(s, db.Techs, Phx.BProtoTech.kBListXmlParams);
			XmlUtil.Serialize(s, db.Civs, Phx.BCiv.kBListXmlParams);
			XmlUtil.Serialize(s, db.Leaders, Phx.BLeader.kBListXmlParams);

			PostStreamXml(s.StreamMode);
		}
	};
}