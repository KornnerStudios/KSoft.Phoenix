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
			mObjectIdToTacticsMap = new Dictionary<int, string>();
			mTacticsMap = new Dictionary<string, Phx.BTacticData>();
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

		public bool StreamXmlTactic<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string xmlName, Phx.BProtoObject obj,
			ref bool wasStreamed, IO.TagElementNodeType xmlSource = XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			string id_name = null;
			bool to_lower = false;

			if (s.IsReading)
			{
				wasStreamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource);

				if (wasStreamed)
				{
					id_name = System.IO.Path.GetFileNameWithoutExtension(id_name);

					mObjectIdToTacticsMap.Add(obj.AutoId, id_name);
					if (!mTacticsMap.ContainsKey(id_name))
						mTacticsMap.Add(id_name, null);
				}
			}
			else if (s.IsWriting && wasStreamed)
			{
				if (!mObjectIdToTacticsMap.TryGetValue(obj.AutoId, out id_name))
					Contract.Assert(false, obj.Name);
				id_name += Phx.BTacticData.kFileExt;
				s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource);
			}

			return wasStreamed;
		}
	};
}