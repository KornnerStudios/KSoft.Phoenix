using System;
using System.Collections.Generic;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Xmb
{
	/*public */sealed class XmbFileContext
	{
		public Shell.ProcessorSize PointerSize;
	};

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
			var context = s.UserData as XmbFileContext;

			using (s.ReadSignatureWithByteSwapSupport(kSignature))
			{
				if (context.PointerSize == Shell.ProcessorSize.x64)
				{
					// #HACK to deal with xmb files which weren't updated with new tools
					if (s.ByteOrder == Shell.EndianFormat.Big)
					{
						context.PointerSize = Shell.ProcessorSize.x32;
					}
				}

				s.VirtualAddressTranslationInitialize(context.PointerSize);

				Values.PtrHandle elements_offset_pos;

				if (context.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}
				#region Initialize elements
				{
					int count = s.ReadInt32();
					if (context.PointerSize == Shell.ProcessorSize.x64)
					{
						s.Pad32();
					}
					s.ReadVirtualAddress(out elements_offset_pos);

					mElements = new List<Element>(count);
				}
				#endregion
				#region Initialize and read pool
				{
					int size = s.ReadInt32();
					if (context.PointerSize == Shell.ProcessorSize.x64)
					{
						s.Pad32();
					}
					Values.PtrHandle pool_offset_pos = s.ReadVirtualAddress();

					s.Seek((long)pool_offset_pos);
					byte[] buffer = s.ReadBytes(size);

					mPool = new XmbVariantMemoryPool(buffer, s.ByteOrder);
				}
				#endregion

				if (context.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad64();
				}

				s.Seek((long)elements_offset_pos);
				for (int x = 0; x < mElements.Capacity; x++)
				{
					var e = new Element();
					mElements.Add(e);

					e.Index = x;
					e.Read(this, context, s);
				}

				foreach (var e in mElements)
				{
					e.ReadAttributes(this, s);
					e.ReadChildren(s);
				}
			}
		}

		public void Write(IO.EndianWriter s)
		{
			var context = s.UserData as XmbFileContext;

			s.Write(kSignature);
			if (context.PointerSize == Shell.ProcessorSize.x64)
			{
				s.Pad32();
			}

			#region Elements header
			s.Write(mElements.Count);
			if (context.PointerSize == Shell.ProcessorSize.x64)
			{
				s.Pad32();
			}
			var elements_offset_pos = s.MarkVirtualAddress(context.PointerSize);
			#endregion

			#region Pool header
			s.Write(mPool.Size);
			if (context.PointerSize == Shell.ProcessorSize.x64)
			{
				s.Pad32();
			}
			var pool_offset_pos = s.MarkVirtualAddress(context.PointerSize);
			#endregion

			if (context.PointerSize == Shell.ProcessorSize.x64)
			{
				s.Pad64();
			}

			var elements_offset = s.PositionPtr;
			foreach (var e in mElements)
			{
				e.Write(s);
			}
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

			var encoding = mHasUnicodeStrings
				? System.Text.Encoding.UTF8
				: System.Text.Encoding.ASCII;

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