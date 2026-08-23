using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerArg
		: IO.ITagElementStringNameStreamable
		, IComparable<BTriggerArg>
		, IEquatable<BTriggerArg>
		, IEqualityComparer<BTriggerArg> // #REPLACE with IEquatable<>
	{
		static readonly BTriggerArg kInvalid = new();
		public bool IsInvalid => object.ReferenceEquals(this, kInvalid);

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BTriggerArg> kBListExplicitIndexParams =
			new(10)
			{
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BTriggerArg> kBListExplicitIndexXmlParams =
			new()
			{
				DataName = kXmlAttrSigId,
			};

		const string kXmlAttrSigId = "SigID";
		const string kXmlAttrOptional = "Optional";
		#endregion

		BTriggerParamType mType = BTriggerParamType.Invalid; // TODO: temporary!
		public BTriggerParamType Type { get { return mType; } }

		string? mName; // TODO: temporary!
		public string? Name { get { return mName; } }

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
			{
				s.ReadCursorName(ref mType);
			}

			s.StreamAttribute(kXmlAttrSigId, ref mSigID);
			string streamName = mName ?? string.Empty;
			s.StreamAttribute(DatabaseNamedObject.kXmlAttrNameN, ref streamName);
			if (s.IsReading)
			{
				mName = streamName;
			}

			s.StreamAttribute(kXmlAttrOptional, ref mOptional);
			s.StreamCursor(ref mVarID);
		}
		#endregion

		#region IComparable<BTriggerArg> Members
		public int CompareTo(BTriggerArg? other)
		{
			return other is null ? 1 : this.mSigID - other.mSigID;
		}
		#endregion

		#region IEquatable<BTriggerArg> Members
		public bool Equals(BTriggerArg? other)
			=> other is not null
				&& this.mSigID == other.mSigID;

		public override bool Equals(object? obj)
			=> Equals(obj as BTriggerArg);

		public override int GetHashCode()
			=> mSigID.GetHashCode();
		#endregion

		#region IEqualityComparer<BTriggerArg> Members
		public bool Equals(BTriggerArg? x, BTriggerArg? y)
		{
			return ReferenceEquals(x, y) || (x is not null && x.Equals(y));
		}

		public int GetHashCode(BTriggerArg obj)
		{
			return obj.GetHashCode();
		}
		#endregion

		public BTriggerVarType GetVarType(BTriggerSystem root)
		{
			ArgumentNullException.ThrowIfNull(root);
			var triggerVar = root.GetVar(mVarID);
			if (triggerVar is null)
			{
				throw new InvalidOperationException($"Trigger variable with ID {mVarID} was not found.");
			}

			return triggerVar.Type;
		}
	};
}