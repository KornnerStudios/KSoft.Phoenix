using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerArg
		: IO.ITagElementStringNameStreamable
		, IComparable<BTriggerArg>
		, IEqualityComparer<BTriggerArg>
	{
		static readonly BTriggerArg kInvalid = new BTriggerArg();
		public bool IsInvalid { get { return object.ReferenceEquals(this, kInvalid); } }

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BTriggerArg> kBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BTriggerArg>(10)
			{
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BTriggerArg> kBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BTriggerArg>(null, kXmlAttrSigId);

		const string kXmlAttrSigId = "SigID";
		const string kXmlAttrOptional = "Optional";
		#endregion

		BTriggerParamType mType = BTriggerParamType.Invalid; // TODO: temporary!
		public BTriggerParamType Type { get { return mType; } }

		string mName; // TODO: temporary!
		public string Name { get { return mName; } }

		int mSigID = TypeExtensions.kNone;
		public int SigID { get { return mSigID; } }

		bool mOptional; // TODO: temporary!
		public bool Optional { get { return mOptional; } }

		int mVarID = TypeExtensions.kNone;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
				s.ReadCursorName(ref mType);
			s.StreamAttribute(kXmlAttrSigId, ref mSigID);
			s.StreamAttribute(DatabaseNamedObject.kXmlAttrNameN, ref mName);
			s.StreamAttribute(kXmlAttrOptional, ref mOptional);
			s.StreamCursor(ref mVarID);
		}
		#endregion

		#region IComparable<BTriggerArg> Members
		public int CompareTo(BTriggerArg other)
		{
			return this.mSigID - other.mSigID;
		}
		#endregion

		#region IEqualityComparer<BTriggerArg> Members
		public bool Equals(BTriggerArg x, BTriggerArg y)
		{
			return x.mSigID == y.mSigID;
		}

		public int GetHashCode(BTriggerArg obj)
		{
			return mSigID;
		}
		#endregion

		public BTriggerVarType GetVarType(BTriggerSystem root)
		{
			return root.GetVar(mVarID).Type;
		}
	};
}