using System;
using System.Collections.Generic;
using System.Xml;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	/*public */sealed class XmbFileContext
	{
		public Shell.ProcessorSize PointerSize;

		public bool CallOnRawDataRead;
	};

	/*public*/ sealed partial class XmbFile
		: IO.IEndianStreamable
		, IDisposable
	{
		public const string kFileExt = ".xmb";
		const uint kSignature = 0x71439800;

		List<Element> mElements;
		XmbVariantMemoryPool mPool;
		bool mHasUnicodeStrings;

		public bool HasUnicodeStrings => mHasUnicodeStrings;

		/// <summary>#HACK only valid during reading and when asked for</summary>
		internal Dictionary<uint, XmbVariant> mRawDataToSingle24Hack;

		Element NewElement(int rootElementIndex = TypeExtensions.kNone)
		{
			var e = new Element
			{
				Index = mElements.Count,
				RootElementIndex = rootElementIndex
			};

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
					var e = new XmbFile.Element();
					mElements.Add(e);

					e.Index = x;
					e.Read(this, context, s);
				}

				foreach (XmbFile.Element e in mElements)
				{
					e.ReadAttributes(this, context, s);
					e.ReadChildren(this, context, s);
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

		string ToString(XmbVariant v) => v.ToString(mPool);

		public XmlDocument ToXmlDocument()
		{
			Contract.Ensures(Contract.Result<XmlDocument>() != null);

			var doc = new XmlDocument();

			return ToXmlDocument(doc);
		}

		public XmlDocument ToXmlDocument(XmlDocument doc)
		{
			Contract.Ensures(doc == null || Contract.Result<XmlDocument>() != null);

			if (doc != null && mElements != null && mElements.Count > 1)
			{
				XmbFile.Element root = mElements[0];
				var root_e = root.ToXml(this, doc, null);

				doc.AppendChild(root_e);
			}

			return doc;
		}

		#region FromXml
		public void FromXml(XmlElement /*root*/_)
		{
			//var e = new Element();

			// #TODO
		}
		#endregion
		#region ToXml
		public void ToXml(string file)
		{
			Contract.Requires(!string.IsNullOrEmpty(file));

			using (var fs = System.IO.File.Create(file))
			{
				ToXml(fs);
			}
		}
		public void ToXml(System.IO.Stream stream)
		{
			Contract.Requires(stream != null);

			var doc = ToXmlDocument();

			var encoding = mHasUnicodeStrings
				? System.Text.Encoding.UTF8
				: System.Text.Encoding.ASCII;
			var xml_writer_settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "\t",
				CloseOutput = false,
				Encoding = encoding,
			};
			using (var xml = XmlWriter.Create(stream, xml_writer_settings))
			{
				doc.Save(xml);
			}
		}
		#endregion

		// #HACK
		private void OnRawDataRead(XmbFile.Element element, uint rawData, XmbVariant variant)
		{
			Util.MarkUnusedVariable(ref element);

			XmbVariantSerialization.RawVariantType rawVariantType = XmbVariantSerialization.GetTypeFromRawData(rawData);

			if (rawVariantType == XmbVariantSerialization.RawVariantType.Single24)
			{
				if (mRawDataToSingle24Hack == null)
				{
					mRawDataToSingle24Hack = new Dictionary<uint, XmbVariant>();
				}

				mRawDataToSingle24Hack[rawData] = variant;
			}
		}

		public bool DumpSingle24Values(Xmb.Single24DumpInfo dumpInfo)
		{
			ArgumentNullException.ThrowIfNull(dumpInfo);

			if (mRawDataToSingle24Hack != null && mRawDataToSingle24Hack.Count > 1)
			{
				foreach (KeyValuePair<uint, XmbVariant> kvp in mRawDataToSingle24Hack)
				{
					uint rawDataValue = XmbVariantSerialization.GetValueFromRawData(kvp.Key);
					dumpInfo.AddEntry(rawDataValue, kvp.Value.Single, null);
				}

				return true;
			}

			return false;
		}
	};
}
