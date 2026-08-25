using System;

namespace KSoft.Phoenix.Phx
{

	static class BXmlCursorDatabaseId
	{
		[System.Diagnostics.Conditional("TRACE")]
		static void TraceUndefinedHandle<TDoc, TCursor>(
			IO.TagElementStream<TDoc, TCursor, string> s, string name, int id, string kind)
			where TDoc : class
			where TCursor : class
		{
			var line_info = Text.TextLineInfo.Empty;
			var cursor_name = "<unknown element>";
			if (s is IO.TagElementTextStream<TDoc, TCursor> text_stream)
			{
				cursor_name = text_stream.CursorName;
				line_info = text_stream.TryGetLastReadLineInfo();
			}

			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"{0} ({1}): Generated UndefinedHandle for '{2}.{3}' ({4}). {5}={6}",
				s.StreamName, Text.TextLineInfo.ToString(line_info, verboseString: true),
				cursor_name, "InnerText",
				kind, name, PhxUtil.GetUndefinedReferenceDataIndex(id).ToString(KSoft.Util.InvariantCultureInfo));
		}

		internal static bool Stream<TDoc, TCursor>(
			IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs,
			ref int dbid, DatabaseObjectKind kind)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
			{
				string id_name = string.Empty;
				s.StreamCursor(ref id_name);
				id_name = string.Intern(id_name);

				dbid = xs.Database.GetId(kind, id_name);
				if (dbid.IsNone())
				{
					s.ThrowReadException(new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
						$"Failed to resolve {kind} reference '{id_name}' from ElementText.")));
				}
				if (PhxUtil.IsUndefinedReferenceHandle(dbid))
				{
					TraceUndefinedHandle(s, id_name, dbid, kind.ToString());
				}
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					return false;
				}

				string? id_name = xs.Database.GetName(kind, dbid);
				if (string.IsNullOrEmpty(id_name))
				{
					throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
						$"Failed to resolve {kind} reference name for id {dbid}."));
				}

				string required_id_name = id_name;
				s.StreamCursor(ref required_id_name);
			}

			return true;
		}
	};

	// internal engine structure is only 0x34 bytes...
	public sealed partial class BProtoTechEffect
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new("Effect")
		{
			Flags = 0
		};
		#endregion

		BProtoTechEffectType mType;
		public BProtoTechEffectType Type { get { return mType; } }

		DataUnion mDU;

		void ThrowIfTypeIsNot(BProtoTechEffectType expected)
		{
			if (Type != expected)
			{
				throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Expected tech effect type {expected}, got {Type}."));
			}
		}
		void ThrowIfTypeIsNot(BProtoTechEffectType expected1, BProtoTechEffectType expected2)
		{
			if (Type != expected1 && Type != expected2)
			{
				throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Expected tech effect type {expected1} or {expected2}, got {Type}."));
			}
		}
		void ThrowIfDataSubtypeIsNot(BObjectDataType expected)
		{
			ThrowIfTypeIsNot(BProtoTechEffectType.Data);
			if (SubType != expected)
			{
				throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Expected tech effect subtype {expected}, got {SubType}."));
			}
		}
		void ThrowIfDataSubtypeIsNot(BObjectDataType expected1, BObjectDataType expected2)
		{
			ThrowIfTypeIsNot(BProtoTechEffectType.Data);
			if (SubType != expected1 && SubType != expected2)
			{
				throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Expected tech effect subtype {expected1} or {expected2}, got {SubType}."));
			}
		}
		void ThrowIfDataSubtypeIsNot(BObjectDataType expected1, BObjectDataType expected2, BObjectDataType expected3)
		{
			ThrowIfTypeIsNot(BProtoTechEffectType.Data);
			if (SubType != expected1 && SubType != expected2 && SubType != expected3)
			{
				throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Expected tech effect subtype {expected1}, {expected2}, or {expected3}, got {SubType}."));
			}
		}

		#region ObjectData
		bool mAllActions;

		string? mAction;

		public BObjectDataType SubType { get { return mDU.SubType; } }

		// Amount can be negative, so use NaN as the 'invalid' value instead
		float mAmount = PhxUtil.kInvalidSingleNaN;

		BObjectDataRelative mRelativity = BObjectDataRelative.Invalid;

		#region Command
		public BProtoObjectCommandType CommandType { get {
			ThrowIfDataSubtypeIsNot(BObjectDataType.CommandEnable, BObjectDataType.CommandSelectable);
			return mDU.CommandType;
		} }

		public int CommandDataID { get {
			ThrowIfDataSubtypeIsNot(BObjectDataType.CommandEnable, BObjectDataType.CommandSelectable);
			return mDU.CommandData;
		} }
		public BSquadMode CommandDataSquadMode { get {
			ThrowIfDataSubtypeIsNot(BObjectDataType.CommandEnable, BObjectDataType.CommandSelectable);
			return mDU.CommandDataSM;
		} }

		public DatabaseObjectKind CommandDataObjectKind { get {
			ThrowIfDataSubtypeIsNot(BObjectDataType.CommandEnable, BObjectDataType.CommandSelectable);
			return mDU.CommandType switch
			{
				BProtoObjectCommandType.Research		=> DatabaseObjectKind.Tech,
				BProtoObjectCommandType.TrainUnit or
				BProtoObjectCommandType.Build			=> DatabaseObjectKind.Object,
				BProtoObjectCommandType.TrainSquad or
				BProtoObjectCommandType.BuildOther		=> DatabaseObjectKind.Squad,
				BProtoObjectCommandType.Ability			=> DatabaseObjectKind.Ability,
				BProtoObjectCommandType.Power			=> DatabaseObjectKind.Power,
				_ => throw new KSoft.Debug.UnreachableException(mDU.CommandType.ToString()),
			};
		} }
		#endregion
		#endregion

		public BProtoTechEffectSetAgeLevel SetAgeLevel { get { return mDU.SetAgeLevel; } }

		public Collections.BListArray<BProtoTechEffectTarget> Targets { get; private set; } = new();
		public bool HasTargets => Targets != null && Targets.Count != 0;

		public BProtoTechEffect()
		{
			mDU.Initialize();
		}

		#region ITagElementStreamable<string> Members
		public DatabaseObjectKind TransformProtoObjectKind
			=> Type switch
			{
				BProtoTechEffectType.TransformProtoUnit => DatabaseObjectKind.Unit,
				BProtoTechEffectType.TransformProtoSquad => DatabaseObjectKind.Squad,
				_ => DatabaseObjectKind.None,
			};

		void StreamXmlObjectData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			// Unused - SubTypes (with data) which no techs in HW1 made use of
			switch (mDU.SubType)
			{
				#region Unused
				case BObjectDataType.RateAmount:
				case BObjectDataType.RateMultiplier:
					xs.StreamTypeName(s, "Rate", ref mDU.ID, GameDataObjectKind.Rate, false, XML.XmlUtil.kSourceAttr);
					break;
				#endregion

				case BObjectDataType.CommandEnable:
				case BObjectDataType.CommandSelectable: // Unused
					mDU.StreamCommand(s, xs);
					break;

				case BObjectDataType.Cost:
					mDU.StreamCost(s, xs);
					break;

				#region Unused
				case BObjectDataType.DamageModifier:
					mDU.StreamDamageModifier(s, xs);
					break;
				#endregion

				case BObjectDataType.PopCap:
				case BObjectDataType.PopMax:
					// #NOTE engine parses this as "PopType", but its parser ignores case
					xs.StreamTypeName(s, "popType", ref mDU.ID, GameDataObjectKind.Pop, false, XML.XmlUtil.kSourceAttr);
					break;

				#region Unused
				case BObjectDataType.UnitTrainLimit:
					mDU.StreamTrainLimit(s, xs, DatabaseObjectKind.Object);
					break;
				case BObjectDataType.SquadTrainLimit:
					mDU.StreamTrainLimit(s, xs, DatabaseObjectKind.Squad);
					break;
				#endregion

				case BObjectDataType.PowerRechargeTime:
				case BObjectDataType.PowerUseLimit:
				case BObjectDataType.PowerLevel:
					// #NOTE engine parses this as "Power", but its parser ignores case
					xs.StreamDBID(s, "power", ref mDU.ID, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceAttr);
					break;
				case BObjectDataType.ImpactEffect:
					// #NOTE engine parses this as "ImpactEffect", but its parser ignores case
					xs.StreamDBID(s, "impactEffect", ref mDU.ID, DatabaseObjectKind.ImpactEffect, false, XML.XmlUtil.kSourceAttr);
					break;

				#region Unused
				case BObjectDataType.DisplayNameID:
					xs.StreamStringID(s, "StringID", ref mDU.ID, XML.XmlUtil.kSourceAttr);
					break;
				#endregion

				case BObjectDataType.Icon:
				// #NOTE engine actually doesn't explicitly handle this case when loading, but it is supported at runtime for Squads
				case BObjectDataType.AltIcon:
					mDU.StreamIcon(s, xs);
					break;

				case BObjectDataType.TurretYawRate:
				case BObjectDataType.TurretPitchRate:
					// #TODO need to validate this type so that Targets.Count==1, TargetType=ProtoUnit, and resolve the Hardpoint name
					string? hardpoint_name = mDU.TurretRate_HardpointName;
					s.StreamStringOpt("Hardpoint", ref hardpoint_name, false);
					if (hardpoint_name is not null)
					{
						mDU.TurretRate_HardpointName = hardpoint_name;
					}
					break;

				case BObjectDataType.AbilityRecoverTime:
					xs.StreamDBID(s, "Ability", ref mDU.ID, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceAttr);
					break;

				case BObjectDataType.HPBar:
					// #TODO need to make an BProtoHPBar reference
					string? hpbar_name = mDU.HPBar_Name;
					s.StreamStringOpt("hpbar", ref hpbar_name, false);
					if (hpbar_name is not null)
					{
						mDU.HPBar_Name = hpbar_name;
					}
					break;

				#region Unused
				case BObjectDataType.DeathSpawn:
					xs.StreamDBID(s, "squadName", ref mDU.ID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
					break;
				#endregion

				#region Object effects
				// although some apply to squads too
				case BObjectDataType.Enable: // Amount>0
				case BObjectDataType.Shieldpoints:
				case BObjectDataType.Hitpoints:
				case BObjectDataType.AmmoMax:
				case BObjectDataType.LOS:
				case BObjectDataType.MaximumVelocity:
				#region Weapon effects
				case BObjectDataType.MaximumRange:
				case BObjectDataType.Damage:
				case BObjectDataType.MinRange:
				case BObjectDataType.AOERadius:
				case BObjectDataType.AOEPrimaryTargetFactor:
				case BObjectDataType.AOEDistanceFactor:
				case BObjectDataType.AOEDamageFactor:
				case BObjectDataType.Accuracy:
				case BObjectDataType.MaxDeviation:
				case BObjectDataType.MovingMaxDeviation:
				case BObjectDataType.DataAccuracyDistanceFactor:
				case BObjectDataType.AccuracyDeviationFactor:
				case BObjectDataType.MaxVelocityLead:
				case BObjectDataType.MaxDamagePerRam:
				case BObjectDataType.ReflectDamageFactor:
				case BObjectDataType.AirBurstSpan:
				case BObjectDataType.DOTrate:
				case BObjectDataType.DOTduration:
				case BObjectDataType.Stasis:

				case BObjectDataType.Projectile:
				#endregion
				#region ProtoAction effects
				case BObjectDataType.WorkRate:
				case BObjectDataType.ActionEnable:
				case BObjectDataType.BoardTime:
				#endregion
				case BObjectDataType.BuildPoints:
				case BObjectDataType.AutoCloak: // Amount>0
				case BObjectDataType.MoveWhileCloaked: // Amount>0
				case BObjectDataType.AttackWhileCloaked: // Amount>0
				case BObjectDataType.Bounty:
				case BObjectDataType.MaxContained:
				case BObjectDataType.AbilityDisabled: // Amount>0
				case BObjectDataType.AmmoRegenRate:
				case BObjectDataType.ShieldRegenRate:
				case BObjectDataType.ShieldRegenDelay:
				#endregion
				#region Squad effects
				case BObjectDataType.Level:
				case BObjectDataType.TechLevel:
				#endregion
				case BObjectDataType.ResearchPoints: // Tech and TechAll only
				#region Player effects
				case BObjectDataType.ResourceTrickleRate:
				case BObjectDataType.BountyResource: // Amount!=0, uses Cost
				case BObjectDataType.RepairCost:
				case BObjectDataType.RepairTime:
				case BObjectDataType.WeaponPhysicsMultiplier:
				#endregion
				default:
					mDU.StreamCost(s, xs, isResourceOptional: true);
					break;
			}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("type", ref mType);

			bool stream_targets = false;
			switch (mType)
			{
				case BProtoTechEffectType.Data:
					// #NOTE engine parses these as AllActions,Action,SubType,Amount,Relativity

					// e.g., SubType==Icon and these won't be used...TODO: is Icon the only one?
					s.StreamAttributeOpt("amount", ref mAmount, PhxPredicates.IsNotInvalidNaN);
					s.StreamAttributeEnum("subtype", ref mDU.SubType);
					// #NOTE the engine treats AllActions being present as 'true', no matter its actual value
					s.StreamAttributeOpt("allactions", ref mAllActions, Predicates.IsTrue);
					s.StreamStringOpt("action", ref mAction, false, intern: true);
					s.StreamAttributeEnumOpt("relativity", ref mRelativity, x => x != BObjectDataRelative.Invalid);
					StreamXmlObjectData(s, xs);
					stream_targets = true;
					break;
				case BProtoTechEffectType.TransformUnit:
				case BProtoTechEffectType.Build:
					BXmlCursorDatabaseId.Stream(s, xs, ref mDU.ToTypeID, DatabaseObjectKind.Object);
					break;
				case BProtoTechEffectType.TransformProtoUnit:
				case BProtoTechEffectType.TransformProtoSquad:
					xs.StreamDBID(s, "FromType", ref mDU.FromTypeID, TransformProtoObjectKind, false, XML.XmlUtil.kSourceAttr);
					xs.StreamDBID(s, "ToType", ref mDU.ToTypeID, TransformProtoObjectKind, false, XML.XmlUtil.kSourceAttr);
					break;
				#region Unused
				case BProtoTechEffectType.SetAge:
					s.StreamCursorEnum(ref mDU.SetAgeLevel);
					break;
				#endregion
				case BProtoTechEffectType.GodPower:
					BXmlCursorDatabaseId.Stream(s, xs, ref mDU.ID, DatabaseObjectKind.Power);
					s.StreamAttribute("amount", ref mAmount);
					break;
				#region Unused
				case BProtoTechEffectType.TechStatus:
					BXmlCursorDatabaseId.Stream(s, xs, ref mDU.ID, DatabaseObjectKind.Tech);
					break;
				case BProtoTechEffectType.Ability:
					BXmlCursorDatabaseId.Stream(s, xs, ref mDU.ID, DatabaseObjectKind.Ability);
					break;
				case BProtoTechEffectType.SharedLOS: // no extra parsed data
					break;
				case BProtoTechEffectType.AttachSquad:
					xs.StreamDBID(s, "squadType", ref mDU.ID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
					stream_targets = true;
					break;
				#endregion
			}

			if (stream_targets)
			{
				XML.XmlUtil.Serialize(s, Targets, BProtoTechEffectTarget.kBListXmlParams);
			}
		}
		#endregion
	};
}