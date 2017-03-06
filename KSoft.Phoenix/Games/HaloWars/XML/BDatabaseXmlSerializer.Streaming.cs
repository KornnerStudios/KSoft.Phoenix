using System.Xml;

namespace KSoft.Phoenix.HaloWars
{
	partial class BDatabaseXmlSerializer
	{
		static bool gRemoveUndefined = false;

		protected override void FixWeaponTypes()
		{
			// Don't add the types if we're not removing undefined data
			// as we assume the UndefinedHandle/ProtoEnum shit is in use
			if (!gRemoveUndefined) return;

			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"Fixing WeaponTypes with missing types");
			Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "Cannon");
			Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "needler");
			Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "HeavyNeedler");
			Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "Plasma");
			Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "HeavyPlasma");
		}

		#region Fix GameData
		static void FixGameDataXmlInfectionMapEntryInfected(IO.XmlElementStream s, string infected)
		{
			string xpath = string.Format("InfectionMap/InfectionMapEntry[contains(@infected, '{0}')]", infected);
			var elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0) foreach (XmlElement e in elements)
			{
				var attr = e.Attributes["infected"];
				attr.Value = attr.Value.Replace("_Inf", "_inf");
			}
		}
		static void FixGameDataXmlInfectionMap(Engine.PhxEngineBuild build, IO.XmlElementStream s)
		{
			string xpath;
			XmlNodeList elements;

			if (!ToLowerName(Phx.DatabaseObjectKind.Object))
			{
				xpath = "InfectionMap/InfectionMapEntry[contains(@base, 'needlergrunt')]";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var attr = e.Attributes["base"];
					attr.Value = attr.Value.Replace("needlergrunt", "needlerGrunt");
				}

				FixGameDataXmlInfectionMapEntryInfected(s, "fld_inf_InfectedBrute_01");
				FixGameDataXmlInfectionMapEntryInfected(s, "fld_inf_InfectedJackal_01");
				FixGameDataXmlInfectionMapEntryInfected(s, "fld_inf_InfectedGrunt_01");
			}

			if (!ToLowerName(Phx.DatabaseObjectKind.Squad))
			{
				xpath = "InfectionMap/InfectionMapEntry[contains(@infectedSquad, '_Inf')]";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var attr = e.Attributes["infectedSquad"];
					attr.Value = attr.Value.Replace("_Inf", "_inf");
				}
			}

			#region Alpha only
			if (build == Engine.PhxEngineBuild.Alpha)
			{
				xpath = "InfectionMap/InfectionMapEntry[contains(@base, 'unsc_inf_heavymarine_01')]";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					e.ParentNode.RemoveChild(e);
				}
			}
			#endregion
		}
		protected override void FixGameDataXml(IO.XmlElementStream s)
		{
			string xpath = null;
			XmlNodeList elements = null;
			#region Fix LeaderPowerChargeResource
			if (gRemoveUndefined)
			{
				xpath = "LeaderPowerChargeResource";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					if (e.InnerText != "LeaderPowerCharge")
						continue;

					Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
						"Fixing GameData XPath={0}");
					e.InnerText = "";
				}
			}
			#endregion

			FixGameDataXmlInfectionMap(Database.Engine.Build, s);
		}
		static void FixGameDataResources(Phx.BGameData gd)
		{
			// Don't add the types if we're not removing undefined data
			// as we assume the UndefinedHandle/ProtoEnum shit is in use
			if (!gRemoveUndefined) return;

			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"Fixing GameData with missing resource types");
			gd.Resources.DynamicAdd(new Phx.BResource(true), "Favor"); // [2]
			gd.Resources.DynamicAdd(new Phx.BResource(true), "Relics");// [3]
			gd.Resources.DynamicAdd(new Phx.BResource(true), "Honor"); // [4]
		}
		protected override void FixGameData()
		{
			//FixGameDataResources(Database.GameData);
		}
		#endregion

		#region Fix Objects
		// Fix float values which are in an invalid format for .NET's parsing
		static void FixObjectsXmlInvalidSinglesCobra(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "unsc_veh_cobra_01");
			if (node == null) return;

			var element = node[Phx.BProtoObject.kXmlElementAttackGradeDPS] as XmlElement;

			string txt = element.InnerText;
			int idx = txt.IndexOf('.');
			if (idx != -1 && (idx = txt.IndexOf('.', idx)) != -1)
				element.InnerText = txt.Remove(idx, txt.Length - idx);
		}
		static void FixObjectsXmlInvalidSinglesAlpha(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "cpgn8_air_strategicMissile_01");
			if (node == null) return;

			// <AttackGradeDPS>.</AttackGradeDPS>
			var element = node[Phx.BProtoObject.kXmlElementAttackGradeDPS] as XmlElement;
			element.ParentNode.RemoveChild(element);
		}
		static void FixObjectsXmlInvalidSingles(Engine.PhxEngineBuild build, IO.XmlElementStream s)
		{
			if (build == Engine.PhxEngineBuild.Alpha)
				FixObjectsXmlInvalidSinglesAlpha(s);
			else
				FixObjectsXmlInvalidSinglesCobra(s);
		}
		static void FixObjectsXmlInvalidFlags(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "fx_proj_fldbomb_01");
			if (node == null) return;

			var nodes = node.ChildNodes;
			foreach (XmlNode element in nodes)
				if (element.Name == "Flag" && element.InnerText == "NonCollidable")
				{
					var fc = element.FirstChild;
					fc.Value = "NonCollideable";
				}
		}
		protected override void FixObjectsXml(IO.XmlElementStream s)
		{
			var build = Database.Engine.Build;
			FixObjectsXmlInvalidSingles(build, s);
			if(build == Engine.PhxEngineBuild.Release)
				FixObjectsXmlInvalidFlags(s);
		}
		#endregion

		#region Fix Squads
		static void FixSquadsXmlAlphaCostElements(IO.XmlElementStream s)
		{
			const string kAttrNameOld = "ResourceType";
			const string kAttrName = "resourcetype";

			string xpath = "Squad/Cost[@" + kAttrNameOld + "]";

			var elements = s.Cursor.SelectNodes(xpath);

			foreach (XmlElement e in elements)
			{
				var attr_old = e.Attributes[kAttrNameOld];
				var attr = s.Document.CreateAttribute(kAttrName);
				attr.Value = attr_old.Value;
				e.Attributes.InsertBefore(attr, attr_old);
				e.RemoveAttribute(kAttrNameOld);
			}
		}
		static void FixSquadsXmlAphaUndefinedObjects(IO.XmlElementStream s, params string[] squadNames)
		{
			foreach (string name in squadNames)
			{
				var node = XPathSelectNodeByName(s, Phx.BProtoSquad.kBListXmlParams, name);
				if (node != null)
					node.ParentNode.RemoveChild(node);
			}
		}
		static void FixSquadsXmlAlpha(IO.XmlElementStream s)
		{
			if (gRemoveUndefined) FixSquadsXmlAphaUndefinedObjects(s,
				"unsc_air_shortsword_01", "unsc_con_turret_01", "unsc_con_base_01",
				"cov_inf_kamikazeGrunt_01", // needs to be 'cpgn_inf_kamikazegrunt_01', but fuck updating it
				"cov_con_turret_01", "cov_con_node_01", "cov_con_base_01"
				);
			FixSquadsXmlAlphaCostElements(s);
		}
		protected override void FixSquadsXml(IO.XmlElementStream s)
		{
			if (Database.Engine.Build == Engine.PhxEngineBuild.Alpha)
				FixSquadsXmlAlpha(s);
		}
		#endregion

		#region Fix Techs
		static void FixTechsXmlBadNames(IO.XmlElementStream s, XML.BListXmlParams op, Engine.PhxEngineBuild build)
		{
			const string k_attr_command_data = "CommandData";
			const string k_element_target = "Target";

			string invalid_command_data_format = string.Format(
				"/{0}/{1}/Effects/Effect[@{2}='",
				op.RootName, op.ElementName, k_attr_command_data) + "{0}']";
			string invalid_target_format = string.Format(
				"/{0}/{1}/Effects/Effect[Target='",
				op.RootName, op.ElementName) + "{0}']";

			string xpath;
			XmlNodeList elements;

			if (!ToLowerName(Phx.DatabaseObjectKind.Unit))
			{
				#region Alpha only
				if (build == Engine.PhxEngineBuild.Alpha)
				{
					xpath = string.Format(invalid_target_format, "cov_inf_eliteleader_01");
					elements = s.Cursor.SelectNodes(xpath);
					if (elements.Count > 0) foreach (XmlElement e in elements)
					{
						var fc = e[k_element_target].FirstChild;
						fc.Value = "cov_inf_eliteLeader_01";
					}
				}
				#endregion
			}
			#region Alpha only
			if (build == Engine.PhxEngineBuild.Alpha)
			{
				xpath = string.Format(invalid_target_format, "cov_inf_elite_leader01");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "cov_inf_eliteLeader_01";
				}
			}
			#endregion

			if (!ToLowerName(Phx.DatabaseObjectKind.Tech))
			{
				#region unsc_MAC_upgrade
				xpath = string.Format(invalid_command_data_format, "unsc_mac_upgrade1");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
						e.Attributes[k_attr_command_data].Value = "unsc_MAC_upgrade1";

				xpath = string.Format(invalid_command_data_format, "unsc_mac_upgrade2");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
						e.Attributes[k_attr_command_data].Value = "unsc_MAC_upgrade2";

				xpath = string.Format(invalid_command_data_format, "unsc_mac_upgrade3");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
						e.Attributes[k_attr_command_data].Value = "unsc_MAC_upgrade3";
				#endregion

				#region unsc_flameMarine_upgrade
				xpath = string.Format(invalid_target_format, "unsc_flamemarine_upgrade1");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_flameMarine_upgrade1";
				}
				xpath = string.Format(invalid_target_format, "unsc_flamemarine_upgrade2");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_flameMarine_upgrade2";
				}
				xpath = string.Format(invalid_target_format, "unsc_flamemarine_upgrade3");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_flameMarine_upgrade3";
				}
				#endregion
			}

			if (!ToLowerName(Phx.DatabaseObjectKind.Squad))
			{
				xpath = string.Format(invalid_target_format, "unsc_inf_flamemarine_01");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_inf_flameMarine_01";
				}
				xpath = string.Format(invalid_target_format, "unsc_inf_Marine_01");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_inf_marine_01";
				}
			}
		}
		// Rename the SubType attribute to be all lowercase (subType is only uppercase for 'TurretYawRate'...)
		static void FixTechsXmlEffectsDataSubType(XmlDocument doc, XmlNode n)
		{
			const string kAttrNameOld = "subType";
			const string kAttrName = "subtype";

			string xpath = "Effects/Effect[@" + kAttrNameOld + "]";

			var elements = n.SelectNodes(xpath);

			foreach (XmlElement e in elements)
			{
				var attr_old = e.Attributes[kAttrNameOld];
				var attr = doc.CreateAttribute(kAttrName);
				attr.Value = attr_old.Value;
				e.Attributes.InsertBefore(attr, attr_old);
				e.RemoveAttribute(kAttrNameOld);
			}
		}
		// Remove non-existent ProtoTechs that are referenced by effects
		static void FixTechsXmlEffectsInvalid(IO.XmlElementStream s, XML.BListXmlParams op, Engine.PhxEngineBuild build)
		{
			string xpath_target = string.Format(
				"/{0}/{1}/Effects/Effect/Target",
				op.RootName, op.ElementName);
			XmlNodeList elements;

			if (build == Engine.PhxEngineBuild.Release)
			{
				elements = s.Document.SelectNodes(xpath_target);

				foreach (XmlElement e in elements)
				{
					if (e.InnerText != "unsc_turret_upgrade3") continue;

					var p = e.ParentNode;
					p.ParentNode.RemoveChild(p);
				}
			}
		}
		protected override void FixTechsXml(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoTech.kBListXmlParams, "unsc_scorpion_upgrade3");
			if (node != null) FixTechsXmlEffectsDataSubType(s.Document, node);

			node = XPathSelectNodeByName(s, Phx.BProtoTech.kBListXmlParams, "unsc_grizzly_upgrade0");
			if (node != null) FixTechsXmlEffectsDataSubType(s.Document, node);

			FixTechsXmlEffectsInvalid(s, Phx.BProtoTech.kBListXmlParams, Database.Engine.Build);
			if(gRemoveUndefined)
				FixTechsXmlBadNames(s, Phx.BProtoTech.kBListXmlParams, Database.Engine.Build);
		}
		#endregion

		#region Fix Tactics
		static void FixTacticsXmlBadWeapons(IO.XmlElementStream s, string name)
		{
			string xpath;
			XmlNodeList elements;

			xpath = "Weapon[WeaponType='plasma']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["WeaponType"].FirstChild;
					fc.Value = "Plasma";
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: fx_proj_beam_01
			xpath = "Weapon[WeaponType='ForunnerBeam']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["WeaponType"].FirstChild;
					fc.Value = "Beam";
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: pow_proj_cleansing_01, pow_proj_wave_explode_01, pow_proj_wave_lightning_01,
			// cov_inf_brutechief_01
			xpath = "Weapon[WeaponType='leaderPower']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["WeaponType"].FirstChild;
					fc.Value = "LeaderPower";
				}
				FixTacticsTraceFixEvent(name, xpath);
			}

			// see: pow_gp_orbitalbombardment
			xpath = "Weapon/DamageRatingOverride[@type='TurretBuilding']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					e.ParentNode.RemoveChild(e);
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: unsc_veh_cobra_01, cpgn_scn10_warthog_01
			xpath = "Weapon/DamageRatingOverride[@type='Unarmored']"
				+ " | Weapon/DamageRatingOverride[@type='Air']"
				+ " | Weapon/DamageRatingOverride[@type='Vehicle']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					e.ParentNode.RemoveChild(e);
				}
				FixTacticsTraceFixEvent(name, xpath);
			}

			// see: unsc_air_vulture_01
			xpath = "Weapon[AirBurstSpan='25.0f']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["AirBurstSpan"].FirstChild;
					fc.Value = fc.Value.Substring(0, fc.Value.Length-1);
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: cov_inf_brute_01
			xpath = "Weapon[contains(Name, 'IncoverBrutegun')]";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["Name"].FirstChild;
					fc.Value = fc.Value.Replace("IncoverBrutegun", "InCoverBrutegun");
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: cov_inf_brutechief_01
			xpath = "Weapon[Name='stunHammer']"
				+ " | Weapon[Name='stunPullHammer']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["Name"].FirstChild;
					fc.Value = fc.Value.Replace("stun", "Stun");
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}
		}
		static void FixTacticsXmlBadActionWeaponNames(IO.XmlElementStream s, string name)
		{
			string xpath;
			XmlNodeList elements;

			// see: fx_proj_rocket_01,02,03
			xpath = "Action[contains(Weapon, '>')]";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["Weapon"].FirstChild;
					fc.Value = fc.Value.Substring(1);
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: pow_gp_rage_impact
			if (gRemoveUndefined)
			{
				xpath = "Action[Weapon='RageShockwave']";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0)
				{
					foreach (XmlElement e in elements)
					{
						e.ParentNode.RemoveChild(e);
					}
					FixTacticsTraceFixEvent(name, xpath);
					return;
				}
			}

			// see: unsc_inf_cyclops_01
			xpath = "Action[Weapon='Buildingjackhammer']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["Weapon"].FirstChild;
					fc.Value = "BuildingJackhammer";
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: unsc_veh_elephant_02
			xpath = "Action[Weapon='lightCannon']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["Weapon"].FirstChild;
					fc.Value = "LightCannon";
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}
		}
		static void FixTacticsXmlBadActions(IO.XmlElementStream s, string name)
		{
			FixTacticsXmlBadActionWeaponNames(s, name);
			string xpath;
			XmlNodeList elements;

			// see: fx_proj_fldbomb_01
			if (!ToLowerName(Phx.DatabaseObjectKind.Squad))
			{
				xpath = "Action[ProtoObject='fld_inf_InfectionForm_02']";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0)
				{
					foreach (XmlElement e in elements)
					{
						var fc = e["ProtoObject"].FirstChild;
						fc.Value = "fld_inf_infectionForm_02";
					}
					FixTacticsTraceFixEvent(name, xpath);
					return;
				}
			}

			// see: cov_inf_elitecommando_01, cpgn_scn10_warthog_01
			xpath = "Action[Name='GatherSupplies']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 1)
			{
				// I'm going to assume the first Action supersedes all proceeding Actions with the same name
				foreach (XmlElement e in elements)
				{
					e.ParentNode.RemoveChild(e);
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			// see: cpgn_scn10_warthog_01
			xpath = "Action[Name='Capture']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 1)
			{
				// I'm going to assume the first Action supersedes all proceeding Actions with the same name
				foreach (XmlElement e in elements)
				{
					e.ParentNode.RemoveChild(e);
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}

			if (!gRemoveUndefined) return;

			// see: cov_inf_grunt_01, cov_inf_needlergrunt_01, cov_inf_elite_01,
			// creep_inf_grunt_01, creep_inf_needlergrunt_01
			xpath = "Action[BaseDPSWeapon='InCoverPlasmaPistolAttackAction']"
				+ " | Action[BaseDPSWeapon='InCoverNeedlerAttackAction']"
				+ " | Action[BaseDPSWeapon='IcCoverPlasmaRifleAttackAction']"
				+ " | Action[BaseDPSWeapon='PlasmaPistolAttackAction']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					e.ParentNode.RemoveChild(e);
				}
				FixTacticsTraceFixEvent(name, xpath);
				return;
			}
		}
		protected override void FixTacticsXml(IO.XmlElementStream s, string name)
		{
			FixTacticsXmlBadWeapons(s, name);
			FixTacticsXmlBadActions(s, name);
		}
		#endregion
	};
}