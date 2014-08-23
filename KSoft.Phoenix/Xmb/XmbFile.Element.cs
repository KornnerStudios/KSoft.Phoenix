using System;
using System.Collections.Generic;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Xmb
{
	partial class XmbFile
	{
		sealed class Element
		{
			internal int Index;
			Values.PtrHandle mAttributesOffsetPos, mAttributesOffset;
			Values.PtrHandle mChildrenOffsetPos, mChildrenOffset;

			public int RootElementIndex = TypeExtensions.kNone;
			public XmbVariant NameVariant;
			public XmbVariant InnerTextVariant;
			List<KeyValuePair<XmbVariant, XmbVariant>> Attributes;
			List<int> ChildrenIndices;

			#region IEndianStreamable Members
			public void ReadAttributes(XmbFile xmb, IO.EndianReader s)
			{
				if (mAttributesOffset.IsInvalidHandle) return;

				s.Seek((long)mAttributesOffset);
				for (int x = 0; x < Attributes.Capacity; x++)
				{
					XmbVariant k; XmbVariantSerialization.Read(s, out k);
					XmbVariant v; XmbVariantSerialization.Read(s, out v);

					var kv = new KeyValuePair<XmbVariant, XmbVariant>(k, v);
					Attributes.Add(kv);

					if (k.HasUnicodeData || v.HasUnicodeData)
						xmb.mHasUnicodeStrings = true;
				}
			}
			public void ReadChildren(IO.EndianReader s)
			{
				if (mChildrenOffset.IsInvalidHandle)
					return;

				s.Seek((long)mChildrenOffset);
				for (int x = 0; x < ChildrenIndices.Capacity; x++)
					ChildrenIndices.Add(s.ReadInt32());
			}
			public void Read(XmbFile xmb, IO.EndianReader s)
			{
				s.Read(out RootElementIndex);
				XmbVariantSerialization.Read(s, out NameVariant);
				XmbVariantSerialization.Read(s, out InnerTextVariant);
				int count;

				s.Read(out count);
				s.ReadVirtualAddress(out mAttributesOffset);
				Attributes = new List<KeyValuePair<XmbVariant, XmbVariant>>(count);

				s.Read(out count);
				s.ReadVirtualAddress(out mChildrenOffset);
				ChildrenIndices = new List<int>(count);

				if (NameVariant.HasUnicodeData || InnerTextVariant.HasUnicodeData) xmb.mHasUnicodeStrings = true;
			}

			public void WriteAttributes(IO.EndianWriter s)
			{
				if (Attributes.Count == 0)
					return;

				mAttributesOffset = s.PositionPtr;
				foreach (var kv in Attributes)
				{
					XmbVariantSerialization.Write(s, kv.Key);
					XmbVariantSerialization.Write(s, kv.Value);
				}

				// Update element entry
				var pos = s.BaseStream.Position;
				s.Seek((long)mAttributesOffsetPos);
				s.WriteVirtualAddress(mAttributesOffset);
				s.Seek(pos);
			}
			public void WriteChildren(IO.EndianWriter s)
			{
				if (ChildrenIndices.Count == 0)
					return;

				mChildrenOffset = s.PositionPtr;
				foreach (int ci in ChildrenIndices)
					s.Write(ci);

				// Update element entry
				var pos = s.BaseStream.Position;
				s.Seek((long)mChildrenOffsetPos);
				s.WriteVirtualAddress(mChildrenOffset);
				s.Seek(pos);
			}
			public void Write(IO.EndianWriter s)
			{
				s.Write(RootElementIndex);
				XmbVariantSerialization.Write(s, NameVariant);
				XmbVariantSerialization.Write(s, InnerTextVariant);

				s.Write(Attributes.Count);
				mAttributesOffsetPos = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);

				s.Write(ChildrenIndices.Count);
				mChildrenOffsetPos = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);
			}
			#endregion

			#region FromXml
			public void FromXmlProcessChildren(XmbFile xmb, XmlElement e)
			{
			}
			public void FromXmlProcessAttributes(XmbFile xmb, XmlElement e)
			{
			}
			public void FromXmlInitialize(XmbFile xmb, int rootIndex, int index, XmlElement e)
			{
				this.Index = index;
				this.RootElementIndex = rootIndex;

				if (e.HasAttributes) Attributes = new List<KeyValuePair<XmbVariant, XmbVariant>>(e.Attributes.Count);
				if (e.HasChildNodes) ChildrenIndices = new List<int>(e.ChildNodes.Count);

				string name = e.Name;
			}
			#endregion
			#region ToXml
			void InnerTextToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (!InnerTextVariant.IsEmpty)
				{
					var text = doc.CreateTextNode(xmb.ToString(InnerTextVariant));
					e.AppendChild(text);
				}
			}
			void AttributesToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (Attributes.Count > 0) foreach (var kv in Attributes)
				{
					string k = xmb.ToString(kv.Key);
					string v = xmb.ToString(kv.Value);

					var attr = doc.CreateAttribute(k);
					attr.Value = v;

					e.Attributes.Append(attr);
				}
			}
			void ChildrenToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (ChildrenIndices.Count > 0) foreach (int x in ChildrenIndices)
				{
					var element = xmb.mElements[x];

					element.ToXml(xmb, doc, e);
				}
			}
			public XmlElement ToXml(XmbFile xmb, XmlDocument doc, XmlElement root)
			{
				var e = doc.CreateElement(xmb.ToString(NameVariant));

				if (root != null) root.AppendChild(e);

				AttributesToXml(xmb, doc, e);
				ChildrenToXml(xmb, doc, e);
				InnerTextToXml(xmb, doc, e);

				return e;
			}
			#endregion
		};
	};
}