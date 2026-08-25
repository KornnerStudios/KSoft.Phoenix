using System;

namespace KSoft.Phoenix.Phx
{
	public abstract class TriggerSystemProtoObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		const string kXmlAttrDbId = "DBID";
		const string kXmlAttrVersion = "Version";
		#endregion

		int mDbId = TypeExtensions.kNone;
		public int DbId { get { return mDbId; } }

		int mVersion = TypeExtensions.kNone;
		public int Version { get { return mVersion; } }

		public Collections.BListExplicitIndex<BTriggerParam> Params { get; private set; } = new(BTriggerParam.kBListExplicitIndexParams);

		protected TriggerSystemProtoObject()
		{
		}
		protected TriggerSystemProtoObject(BTriggerSystem root, TriggerScriptObjectWithArgs instance)
		{
			Name = instance.Name;

			mDbId = instance.DbId;
			mVersion = instance.Version;
			Params = BTriggerParam.BuildDefinition(root, instance.Args);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute(kXmlAttrDbId, ref mDbId);
			s.StreamAttribute(kXmlAttrVersion, ref mVersion);

			XML.XmlUtil.Serialize(s, Params, BTriggerParam.kBListExplicitIndexXmlParams);
		}

		static bool ContainsUserClassTypeVar(BTriggerSystem ts, TriggerScriptObjectWithArgs obj)
		{
			foreach (var arg in obj.Args)
			{
				if (arg.IsInvalid)
				{
					continue;
				}

				if (arg.GetVarType(ts) == BTriggerVarType.UserClassType)
				{
					return true;
				}
			}
			return false;
		}
		public virtual int CompareTo(BTriggerSystem ts, TriggerScriptObjectWithArgs obj)
		{
			if (Name != obj.Name)
			{
				Debug.Trace.Engine.TraceInformation(string.Create(KSoft.Util.InvariantCultureInfo,
					$"TriggerProtoDbObject: '{ts}' - Encountered different names for {DbId}, '{Name}' != '{obj.Name}'"));
			}

			if (ContainsUserClassTypeVar(ts, obj))
			{
				Debug.Trace.Engine.TraceInformation(string.Create(KSoft.Util.InvariantCultureInfo,
					$"TriggerProtoDbObject: {ts} - Encountered {DbId}/{Name} which has a UserClassType Var, skipping comparison"));
				return 0;
			}

			if (Version != obj.Version)
			{
				throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Trigger proto object version is {Version}, expected {obj.Version}."));
			}
			if (Params.Count != obj.Args.Count)
			{
				throw new InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
					$"Trigger proto object parameter count is {Params.Count}, expected {obj.Args.Count}."));
			}

			int diff = 0;
			for (int x = 0; x < Params.Count; x++)
			{
				int sig = Params[x].SigID;
				int obj_sig = obj.Args[x].SigID;
				sig = sig < 0 ? 0 : sig;
				obj_sig = obj_sig < 0 ? 0 : obj_sig;

				diff += sig - obj_sig;
			}

			return diff;
		}
	};
}