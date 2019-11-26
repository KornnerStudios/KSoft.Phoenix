using System.Collections.Generic;
using System.Xml;

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
				if (mAttributesOffset.IsInvalidHandle)
					return;

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
			public void Read(XmbFile xmb, XmbFileContext xmbContext, IO.EndianReader s)
			{
				s.Read(out RootElementIndex);
				XmbVariantSerialization.Read(s, out NameVariant);
				XmbVariantSerialization.Read(s, out InnerTextVariant);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}

				#region Attributes header
				int count;
				s.Read(out count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}
				s.ReadVirtualAddress(out mAttributesOffset);
				Attributes = new List<KeyValuePair<XmbVariant, XmbVariant>>(count);
				#endregion

				#region Children header
				s.Read(out count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}
				s.ReadVirtualAddress(out mChildrenOffset);
				ChildrenIndices = new List<int>(count);
				#endregion

				if (NameVariant.HasUnicodeData || InnerTextVariant.HasUnicodeData)
					xmb.mHasUnicodeStrings = true;
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
				var xmbContext = s.UserData as XmbFileContext;

				s.Write(RootElementIndex);
				XmbVariantSerialization.Write(s, NameVariant);
				XmbVariantSerialization.Write(s, InnerTextVariant);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}

				#region Attributes header
				s.Write(Attributes.Count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}
				mAttributesOffsetPos = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);
				#endregion

				#region Children header
				s.Write(ChildrenIndices.Count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}
				mChildrenOffsetPos = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);
				#endregion
			}
			#endregion

			#region FromXml
			public void FromXmlProcessChildren(XmbFileBuilder builder, XmlElement e)
			{
			}
			public void FromXmlProcessAttributes(XmbFileBuilder builder, XmlElement e)
			{
			}
			public void FromXmlInitialize(XmbFileBuilder builder, int rootIndex, int index, XmlElement e)
			{
				this.Index = index;
				this.RootElementIndex = rootIndex;

				if (e.HasAttributes)
					Attributes = new List<KeyValuePair<XmbVariant, XmbVariant>>(e.Attributes.Count);
				if (e.HasChildNodes)
					ChildrenIndices = new List<int>(e.ChildNodes.Count);

				string name = e.Name;
				string text = e.Value;

				if (e.HasAttributes)
					FromXmlProcessAttributes(builder, e);
				if (e.HasChildNodes)
					FromXmlProcessChildren(builder, e);
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

					// #HACK avoids exceptions like:
					// "The prefix '' cannot be redefined from '' to 'http://www.w3.org/2000/09/xmldsig#' within the same start element tag."
					// for XML files that weren't meant for the game but were transformed to XMB anyway
					if (string.CompareOrdinal(k, "xmlns")==0)
					{
						var comment = doc.CreateComment(attr.OuterXml);
						e.AppendChild(comment);
						continue;
					}

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

				if (root != null)
					root.AppendChild(e);

				AttributesToXml(xmb, doc, e);
				ChildrenToXml(xmb, doc, e);
				InnerTextToXml(xmb, doc, e);

				return e;
			}
			#endregion
		};
	};
}