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
			public void ReadAttributes(XmbFile xmb, XmbFileContext xmbContext, IO.EndianReader s)
			{
				if (mAttributesOffset.IsInvalidHandle)
				{
					return;
				}

				s.Seek((long)mAttributesOffset);
				for (int x = 0; x < Attributes.Capacity; x++)
				{
					uint keyRawData = XmbVariantSerialization.Read(s, out XmbVariant k);
					uint valueRawData = XmbVariantSerialization.Read(s, out XmbVariant v);

					var kv = new KeyValuePair<XmbVariant, XmbVariant>(k, v);
					Attributes.Add(kv);

					if (k.HasUnicodeData || v.HasUnicodeData)
					{
						xmb.mHasUnicodeStrings = true;
					}

					if (xmbContext.CallOnRawDataRead) // #HACK
					{
						xmb.OnRawDataRead(this, keyRawData, k);
						xmb.OnRawDataRead(this, valueRawData, v);
					}
				}
			}
			public void ReadChildren(XmbFile xmb, XmbFileContext xmbContext, IO.EndianReader s)
			{
				Util.MarkUnusedVariable(ref xmb);
				Util.MarkUnusedVariable(ref xmbContext);

				if (mChildrenOffset.IsInvalidHandle)
				{
					return;
				}

				s.Seek((long)mChildrenOffset);
				for (int x = 0; x < ChildrenIndices.Capacity; x++)
				{
					ChildrenIndices.Add(s.ReadInt32());
				}
			}
			public void Read(XmbFile xmb, XmbFileContext xmbContext, IO.EndianReader s)
			{
				s.Read(out RootElementIndex);
				uint nameRawData = XmbVariantSerialization.Read(s, out NameVariant);
				uint innerTextRawData = XmbVariantSerialization.Read(s, out InnerTextVariant);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}

				#region Attributes header
				s.Read(out int count);
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
				{
					xmb.mHasUnicodeStrings = true;
				}

				if (xmbContext.CallOnRawDataRead) // #HACK
				{
					xmb.OnRawDataRead(this, nameRawData, NameVariant);
					xmb.OnRawDataRead(this, innerTextRawData, InnerTextVariant);
				}
			}

			public void WriteAttributes(IO.EndianWriter s)
			{
				if (Attributes.Count == 0)
				{
					return;
				}

				mAttributesOffset = s.PositionPtr;
				foreach (var kv in Attributes)
				{
					XmbVariantSerialization.Write(s, kv.Key);
					XmbVariantSerialization.Write(s, kv.Value);
				}

				// Update element entry
				long pos = s.BaseStream.Position;
				s.Seek((long)mAttributesOffsetPos);
				s.WriteVirtualAddress(mAttributesOffset);
				s.Seek(pos);
			}
			public void WriteChildren(IO.EndianWriter s)
			{
				if (ChildrenIndices.Count == 0)
				{
					return;
				}

				mChildrenOffset = s.PositionPtr;
				foreach (int ci in ChildrenIndices)
				{
					s.Write(ci);
				}

				// Update element entry
				long pos = s.BaseStream.Position;
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
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
			public void FromXmlProcessChildren(XmbFileBuilder builder, XmlElement e)
			{
				// #TODO
			}
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
			public void FromXmlProcessAttributes(XmbFileBuilder builder, XmlElement e)
			{
				// #TODO
			}
			public void FromXmlInitialize(XmbFileBuilder builder, int rootIndex, int index, XmlElement e)
			{
				this.Index = index;
				this.RootElementIndex = rootIndex;

				if (e.HasAttributes)
				{
					Attributes = new List<KeyValuePair<XmbVariant, XmbVariant>>(e.Attributes.Count);
				}

				if (e.HasChildNodes)
				{
					ChildrenIndices = new List<int>(e.ChildNodes.Count);
				}

#if DEBUG
				string name = e.Name;
				string text = e.Value;
#endif

				if (e.HasAttributes)
				{
					FromXmlProcessAttributes(builder, e);
				}

				if (e.HasChildNodes)
				{
					FromXmlProcessChildren(builder, e);
				}
			}
			#endregion
			#region ToXml
			void InnerTextToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (!InnerTextVariant.IsEmpty)
				{
					XmlText text = doc.CreateTextNode(xmb.ToString(InnerTextVariant));
					e.AppendChild(text);
				}
			}
			void AttributesToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (Attributes.Count > 0)
				{
					foreach (var kv in Attributes)
					{
						string k = xmb.ToString(kv.Key);
						string v = xmb.ToString(kv.Value);

						XmlAttribute attr = doc.CreateAttribute(k);
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
			}
			void ChildrenToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (ChildrenIndices.Count > 0)
				{
					foreach (int x in ChildrenIndices)
					{
						XmbFile.Element element = xmb.mElements[x];

						element.ToXml(xmb, doc, e);
					}
				}
			}
			public XmlElement ToXml(XmbFile xmb, XmlDocument doc, XmlElement root)
			{
				XmlElement e = doc.CreateElement(xmb.ToString(NameVariant));

				root?.AppendChild(e);

				AttributesToXml(xmb, doc, e);
				ChildrenToXml(xmb, doc, e);
				InnerTextToXml(xmb, doc, e);

				return e;
			}
			#endregion
		};
	};
}
