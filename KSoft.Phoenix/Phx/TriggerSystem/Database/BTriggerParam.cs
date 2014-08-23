using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerParam
		: IO.ITagElementStringNameStreamable
		, IComparable<BTriggerParam>
		, IEqualityComparer<BTriggerParam>
	{
		static readonly BTriggerParam kInvalid = new BTriggerParam();
		public bool IsInvalid { get { return object.ReferenceEquals(this, kInvalid); } }

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BTriggerParam> kBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BTriggerParam>(10)
			{
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BTriggerParam> kBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BTriggerParam>(/*null*/"Param", kXmlAttrSigId);

		const string kXmlAttrType = "Type";
		const string kXmlAttrSigId = "SigID";
		const string kXmlAttrOptional = "Optional";
		#endregion

		#region Properties
		BTriggerParamType mType = BTriggerParamType.Invalid;
		public BTriggerParamType Type { get { return mType; } }

		string mName;
		public string Name { get { return mName; } }

		int mSigID = TypeExtensions.kNone;
		public int SigID { get { return mSigID; } }

		BTriggerVarType mVarType = BTriggerVarType.None;
		public BTriggerVarType VarType { get { return mVarType; } }

		bool mOptional;
		public bool Optional { get { return mOptional; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			//if (s.IsReading) s.ReadCursorName(ref mType);
			s.StreamAttributeEnum(kXmlAttrType, ref mType);
			s.StreamAttribute(kXmlAttrSigId, ref mSigID);
			s.StreamAttribute(DatabaseNamedObject.kXmlAttrNameN, ref mName);
			s.StreamAttributeOpt(kXmlAttrOptional, ref mOptional, Predicates.IsTrue);
			s.StreamCursorEnum(ref mVarType);
		}
		#endregion

		#region IComparable<BTriggerParam> Members
		public int CompareTo(BTriggerParam other)
		{
			return this.mSigID - other.mSigID;
		}
		#endregion

		#region IEqualityComparer<BTriggerParam> Members
		public bool Equals(BTriggerParam x, BTriggerParam y)
		{
			return x.mSigID == y.mSigID;
		}

		public int GetHashCode(BTriggerParam obj)
		{
			return mSigID;
		}
		#endregion

		public static Collections.BListExplicitIndex<BTriggerParam> BuildDefinition(
			BTriggerSystem root, Collections.BListExplicitIndex<BTriggerArg> args)
		{
			var p = new Collections.BListExplicitIndex<BTriggerParam>(kBListExplicitIndexParams);
			p.ResizeCount(args.Count);

			foreach (var arg in args)
			{
				if (arg.IsInvalid) continue;

				var param = new BTriggerParam();
				param.mType = arg.Type;
				param.mName = arg.Name;
				param.mSigID = arg.SigID;
				param.mOptional = arg.Optional;
				param.mVarType = arg.GetVarType(root);

				p[param.mSigID-1] = param;
			}

			p.OptimizeStorage();
			return p;
		}
	};
}