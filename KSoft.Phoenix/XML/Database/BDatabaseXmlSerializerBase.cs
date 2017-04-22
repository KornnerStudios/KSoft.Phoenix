using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	public abstract partial class BDatabaseXmlSerializerBase
		: BXmlSerializerInterface
		, IO.ITagElementStringNameStreamable
	{
		XML.IBListAutoIdXmlSerializer mDamageTypesSerializer
			, mImpactEffectsSerializer
			, mObjectsSerializer
			, mSquadsSerializer
			, mPowersSerializer
			, mTechsSerializer
			;

		protected BDatabaseXmlSerializerBase()
		{
		}

		#region IDisposable Members
		public override void Dispose()
		{
			AutoIdSerializersDispose();
		}
		#endregion

		protected virtual void AutoIdSerializersInitialize()
		{
			if (mDamageTypesSerializer == null)
				mDamageTypesSerializer = XmlUtil.CreateXmlSerializer(Database.DamageTypes, Phx.BDamageType.kBListXmlParams);
			if (mImpactEffectsSerializer == null)
				mImpactEffectsSerializer = XmlUtil.CreateXmlSerializer(Database.ImpactEffects, Phx.BProtoImpactEffect.kBListXmlParams);

			if (mObjectsSerializer == null)
				mObjectsSerializer = XmlUtil.CreateXmlSerializer(Database.Objects, Phx.BProtoObject.kBListXmlParams);
			if (mSquadsSerializer == null)
				mSquadsSerializer = XmlUtil.CreateXmlSerializer(Database.Squads, Phx.BProtoSquad.kBListXmlParams);
			if (mPowersSerializer == null)
				mPowersSerializer = XmlUtil.CreateXmlSerializer(Database.Powers, Phx.BProtoPower.kBListXmlParams);
			if (mTechsSerializer == null)
				mTechsSerializer = XmlUtil.CreateXmlSerializer(Database.Techs, Phx.BProtoTech.kBListXmlParams);
		}
		protected virtual void AutoIdSerializersDispose()
		{
			Util.DisposeAndNull(ref mDamageTypesSerializer);
			Util.DisposeAndNull(ref mImpactEffectsSerializer);

			Util.DisposeAndNull(ref mObjectsSerializer);
			Util.DisposeAndNull(ref mSquadsSerializer);
			Util.DisposeAndNull(ref mPowersSerializer);
			Util.DisposeAndNull(ref mTechsSerializer);
		}
	};
}