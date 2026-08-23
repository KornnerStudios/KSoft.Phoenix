using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerParam
		: IO.ITagElementStringNameStreamable
		, IComparable<BTriggerParam>
		, IEquatable<BTriggerParam>
		, IEqualityComparer<BTriggerParam> // #REPLACE with IEquatable<>
	{
		static readonly BTriggerParam kInvalid = new();
		public bool IsInvalid => object.ReferenceEquals(this, kInvalid);

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BTriggerParam> kBListExplicitIndexParams =
			new(10)
			{
				kTypeGetInvalid = () => kInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BTriggerParam> kBListExplicitIndexXmlParams =
			new(/*null*/"Param", kXmlAttrSigId);

		const string kXmlAttrType = "Type";
		const string kXmlAttrSigId = "SigID";
		const string kXmlAttrOptional = "Optional";
		#endregion

		#region Properties
		BTriggerParamType mType = BTriggerParamType.Invalid;
		public BTriggerParamType Type { get { return mType; } }

		string? mName;
		public string? Name { get { return mName; } }

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
			string stream_name = mName ?? string.Empty;
			s.StreamAttribute(DatabaseNamedObject.kXmlAttrNameN, ref stream_name);
			if (s.IsReading)
			{
				mName = stream_name;
			}
			s.StreamAttributeOpt(kXmlAttrOptional, ref mOptional, Predicates.IsTrue);
			s.StreamCursorEnum(ref mVarType);
		}
		#endregion

		#region IComparable<BTriggerParam> Members
		public int CompareTo(BTriggerParam? other)
		{
			return other is null ? 1 : this.mSigID - other.mSigID;
		}
		#endregion

		#region IEquatable<BTriggerParam> Members
		public bool Equals(BTriggerParam? other)
			=> other is not null
				&& this.mSigID == other.mSigID;

		public override bool Equals(object? obj)
			=> Equals(obj as BTriggerParam);

		public override int GetHashCode()
			=> mSigID.GetHashCode();
		#endregion

		#region IEqualityComparer<BTriggerParam> Members
		public bool Equals(BTriggerParam? x, BTriggerParam? y)
		{
			return ReferenceEquals(x, y) || (x is not null && x.Equals(y));
		}

		public int GetHashCode(BTriggerParam obj)
		{
			return obj.GetHashCode();
		}
		#endregion

		public static Collections.BListExplicitIndex<BTriggerParam> BuildDefinition(
			BTriggerSystem root, Collections.BListExplicitIndex<BTriggerArg> args)
		{
			var p = new Collections.BListExplicitIndex<BTriggerParam>(kBListExplicitIndexParams);
			p.ResizeCount(args.Count);

			foreach (var arg in args)
			{
				if (arg.IsInvalid)
				{
					continue;
				}

				var param = new BTriggerParam
				{
					mType = arg.Type,
					mName = arg.Name,
					mSigID = arg.SigID,
					mOptional = arg.Optional,
					mVarType = arg.GetVarType(root)
				};

				p[param.mSigID-1] = param;
			}

			p.OptimizeStorage();
			return p;
		}
	};
}