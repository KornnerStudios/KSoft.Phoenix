using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Phx
{
	partial class BProtoTechEffect
	{
		#region Xml constants
		const string kXmlAttrODT_Rate = "Rate";

		const string kXmlAttrODT_Cost_Resource = "Resource";
		const string kXmlAttrODT_Cost_UnitType = "UnitType";

		const string kXmlAttrODT_Command_Type = "commandType";
		const string kXmlAttrODT_Command_Data = "CommandData";

		const string kXmlAttrODT_DamageModifier_WeapType = "WeaponType";
		const string kXmlAttrODT_DamageModifier_DmgType = "DamageType";

		const string kXmlAttrODT_Pop_PopType = "popType";

		const string kXmlAttrODT_TrainLimit_Unit = "unitType";
		const string kXmlAttrODT_TrainLimit_Squad = "squadType";

		const string kXmlAttrODT_Power_Power = "power";

		const string kXmlAttrODT_AbilityRecoverTime_Ability = "Ability";
		#endregion

		[Interop.StructLayout(Interop.LayoutKind.Explicit, Size = DataUnion.kSizeOf)]
		struct DataUnion
		{
			internal const int kSizeOf = 12;
			/// <summary>Offset of the first parameter</summary>
			const int kFirstParam = 4;
			/// <summary>Offset of the second parameter</summary>
			const int kSecondParam = 8;

			[Interop.FieldOffset(0)] public BObjectDataType SubType;
			[Interop.FieldOffset(kFirstParam)] public int ID;
			[Interop.FieldOffset(kSecondParam)] public int ID2;

			[Interop.FieldOffset(kFirstParam)] public int Cost_Type;
			[Interop.FieldOffset(kSecondParam)] public int Cost_UnitType; // proto object or type ID

			[Interop.FieldOffset(kFirstParam)] public BProtoObjectCommandType CommandType;
			[Interop.FieldOffset(kSecondParam)] public int CommandData;
			[Interop.FieldOffset(kSecondParam)] public BSquadMode CommandDataSM;

			[Interop.FieldOffset(kFirstParam)] public int DmgMod_WeapType;
			[Interop.FieldOffset(kSecondParam)] public int DmgMod_DmgType;

			[Interop.FieldOffset(kSecondParam)] public int TrainLimitType; // proto object or squad ID

			[Interop.FieldOffset(kFirstParam)] public int FromTypeID;
			[Interop.FieldOffset(kSecondParam)] public int ToTypeID;

			[Interop.FieldOffset(kFirstParam)] public BProtoTechEffectSetAgeLevel SetAgeLevel;

			public void Initialize()
			{
				SubType = BObjectDataType.Invalid;
				ID = ID2 = TypeExtensions.kNone;
			}

			public void StreamCost<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				xs.StreamTypeName(s, kXmlAttrODT_Cost_Resource, ref Cost_Type, GameDataObjectKind.Cost, false, XML.XmlUtil.kSourceAttr);
				xs.StreamDBID(s, kXmlAttrODT_Cost_UnitType, ref Cost_UnitType, DatabaseObjectKind.Unit, true, XML.XmlUtil.kSourceAttr);
			}
			void StreamCommandData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				const string attr_name = kXmlAttrODT_Command_Data;

				switch (CommandType)
				{
				case BProtoObjectCommandType.Research: // proto tech
					xs.StreamDBID(s, attr_name, ref CommandData, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceAttr);
					break;
				case BProtoObjectCommandType.TrainUnit: // proto object
				case BProtoObjectCommandType.Build:
				case BProtoObjectCommandType.BuildOther:
					xs.StreamDBID(s, attr_name, ref CommandData, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
					break;
				case BProtoObjectCommandType.TrainSquad: // proto squad
					xs.StreamDBID(s, attr_name, ref CommandData, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
					break;

				case BProtoObjectCommandType.ChangeMode: // unused
					s.StreamAttributeEnum(attr_name, ref CommandDataSM);
					break;

				case BProtoObjectCommandType.Ability:
					xs.StreamDBID(s, attr_name, ref CommandData, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceAttr);
					break;
				case BProtoObjectCommandType.Power:
					xs.StreamDBID(s, attr_name, ref CommandData, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceAttr);
					break;
				}
			}
			public void StreamCommand<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				if (s.StreamAttributeEnumOpt(kXmlAttrODT_Command_Type, ref CommandType, e => e != BProtoObjectCommandType.Invalid))
					StreamCommandData(s, xs);
			}
			public void StreamDamageModifier<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				xs.StreamDBID(s, kXmlAttrODT_DamageModifier_WeapType, ref DmgMod_WeapType, DatabaseObjectKind.WeaponType, false, XML.XmlUtil.kSourceAttr);
				xs.StreamDBID(s, kXmlAttrODT_DamageModifier_DmgType, ref DmgMod_DmgType, DatabaseObjectKind.DamageType, false, XML.XmlUtil.kSourceAttr);
			}
			public void StreamTrainLimit<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs, DatabaseObjectKind kind)
				where TDoc : class
				where TCursor : class
			{
				if (kind == DatabaseObjectKind.Object)
					xs.StreamDBID(s, kXmlAttrODT_TrainLimit_Unit, ref TrainLimitType, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
				else if (kind == DatabaseObjectKind.Squad)
					xs.StreamDBID(s, kXmlAttrODT_TrainLimit_Squad, ref TrainLimitType, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
			}
		};

		#region ID variants
		public int WeaponTypeID { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.DamageModifier);
			return mDU.DmgMod_WeapType;
		} }
		public int DamageTypeID { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.DamageModifier);
			return mDU.DmgMod_DmgType;
		} }

		public int RateID { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.RateAmount || SubType == BObjectDataType.RateMultiplier);
			return mDU.ID;
		} }

		public int PopID { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.PopCap || SubType == BObjectDataType.PopMax);
			return mDU.ID;
		} }

		public int PowerID { get {
			Contract.Requires(Type == BProtoTechEffectType.Data);
			Contract.Requires(SubType == BObjectDataType.PowerRechargeTime || SubType == BObjectDataType.PowerUseLimit || SubType == BObjectDataType.PowerLevel);
			return mDU.ID;
		} }

		public int TransformUnitID { get {
			Contract.Requires(Type == BProtoTechEffectType.TransformUnit);
			return mDU.ToTypeID;
		} }
		public int TransformProtoFromID { get {
			Contract.Requires(Type == BProtoTechEffectType.TransformProtoUnit || Type == BProtoTechEffectType.TransformProtoSquad);
			return mDU.FromTypeID;
		} }
		public int TransformProtoToID { get {
			Contract.Requires(Type == BProtoTechEffectType.TransformProtoUnit || Type == BProtoTechEffectType.TransformProtoSquad);
			return mDU.ToTypeID;
		} }
		public int BuildObjectID { get {
			Contract.Requires(Type == BProtoTechEffectType.Build);
			return mDU.ToTypeID;
		} }
		public int GodPowerID { get {
			Contract.Requires(Type == BProtoTechEffectType.GodPower);
			return mDU.ID;
		} }
		public int TechStatusTechID { get {
			Contract.Requires(Type == BProtoTechEffectType.TechStatus);
			return mDU.ID;
		} }
		public int AbilityID { get {
			Contract.Requires(Type == BProtoTechEffectType.Ability);
			return mDU.ID;
		} }
		public int AttachSquadTypeObjectID { get {
			Contract.Requires(Type == BProtoTechEffectType.AttachSquad);
			return mDU.ID;
		} }
		#endregion
	};
}