using System;
using System.Collections.Generic;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Xmb
{
	/*public*/ sealed partial class XmbFile
		: IO.IEndianStreamable
		, IDisposable
	{
		const uint kSignature = 0x71439800;

		List<Element> mElements;
		XmbVariantMemoryPool mPool;
		bool mHasUnicodeStrings;

		Element NewElement(int rootElementIndex = -1)
		{
			var e = new Element();
			e.Index = mElements.Count;
			e.RootElementIndex = rootElementIndex;

			mElements.Add(e);
			return e;
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (mElements != null)
			{
				mElements.Clear();
				mElements = null;
			}

			Util.DisposeAndNull(ref mPool);
		}
		#endregion

		#region IEndianStreamable Members
		public void Read(IO.EndianReader s)
		{
			Values.PtrHandle elements_offset_pos;

			var signature = s.ReadUInt32();
			if (signature != kSignature)
				throw new IO.SignatureMismatchException(s.BaseStream, kSignature, signature);

			#region Initialize elements
			{
				int count = s.ReadInt32();
				s.ReadVirtualAddress(out elements_offset_pos);

				mElements = new List<Element>(count);
			}
			#endregion
			#region Initialize and read pool
			{
				int size = s.ReadInt32();
				Values.PtrHandle pool_offset_pos = s.ReadVirtualAddress();

				s.Seek((long)pool_offset_pos);
				byte[] buffer = s.ReadBytes(size);

				mPool = new XmbVariantMemoryPool(buffer, s.ByteOrder);
			}
			#endregion

			s.Seek((long)elements_offset_pos);
			for (int x = 0; x < mElements.Capacity; x++)
			{
				var e = new Element();
				mElements.Add(e);

				e.Index = x;
				e.Read(this, s);
			}

			foreach (var e in mElements)
			{
				e.ReadAttributes(this, s);
				e.ReadChildren(s);
			}
		}

		public void Write(IO.EndianWriter s)
		{
			s.Write(kSignature);
			s.Write(mElements.Count);
			var elements_offset_pos = s.MarkVirtualAddress32();
			s.Write(mPool.Size);
			var pool_offset_pos = s.MarkVirtualAddress32();

			var elements_offset = s.PositionPtr;
			foreach (var e in mElements) e.Write(s);
			foreach (var e in mElements)
			{
				e.WriteAttributes(s);
				e.WriteChildren(s);
			}

			var pool_offset = s.PositionPtr;
			mPool.Write(s);

			s.Seek((long)elements_offset_pos);
			s.WriteVirtualAddress(elements_offset);
			s.Seek((long)pool_offset_pos);
			s.WriteVirtualAddress(pool_offset);
		}
		#endregion

		string ToString(XmbVariant v) { return v.ToString(mPool); }

		#region FromXml
		public void FromXml(XmlElement root)
		{
			var e = new Element();
		}
		#endregion
		#region ToXml
		public void ToXml(string file)
		{
			var doc = new XmlDocument();

			var root = mElements[0];
			var root_e = root.ToXml(this, doc, null);

			// TODO: determine if the XMB has unicode strings and only switch to UTF8 when that's the case
			var encoding = mHasUnicodeStrings ? System.Text.Encoding.UTF8 : System.Text.Encoding.ASCII;

			doc.AppendChild(root_e);
			using (var xml = new XmlTextWriter(file, encoding))
			{
				xml.Formatting = Formatting.Indented;
				xml.IndentChar = '\t';
				xml.Indentation = 1;
				doc.Save(xml);
			}
		}
		#endregion
	};
}