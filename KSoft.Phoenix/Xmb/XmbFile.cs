using System;
using System.Collections.Generic;
using System.Xml;

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

		List<Element>? mElements;
		XmbVariantMemoryPool? mPool;
		bool mHasUnicodeStrings;

		public bool HasUnicodeStrings => mHasUnicodeStrings;

		/// <summary>#HACK only valid during reading and when asked for</summary>
		internal Dictionary<uint, XmbVariant>? mRawDataToSingle24Hack;

		List<Element> Elements => mElements ?? throw new NullReferenceException();
		XmbVariantMemoryPool Pool => mPool ?? throw new NullReferenceException();

		// TypeCheck, Element.ToXml, and Single24DumpInfo use null sentinels but cannot express those contracts.
		[System.Diagnostics.CodeAnalysis.AllowNull]
		static readonly object kNullContext = null;
		[System.Diagnostics.CodeAnalysis.AllowNull]
		static readonly XmlElement kNullRootElement = null;
		[System.Diagnostics.CodeAnalysis.AllowNull]
		static readonly string kNullSingle24Description = null;

		static XmbFileContext GetContext(object? userData)
		{
			return KSoft.Debug.TypeCheck.CastReference<XmbFileContext>(userData ?? kNullContext);
		}

		Element NewElement(int rootElementIndex = TypeExtensions.kNone)
		{
			var elements = Elements;
			var e = new Element
			{
				Index = elements.Count,
				RootElementIndex = rootElementIndex
			};

			elements.Add(e);
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
			var context = GetContext(s.UserData);

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

				var elements = Elements;
				s.Seek((long)elements_offset_pos);
				for (int x = 0; x < elements.Capacity; x++)
				{
					var e = new XmbFile.Element();
					elements.Add(e);

					e.Index = x;
					e.Read(this, context, s);
				}

				foreach (XmbFile.Element e in elements)
				{
					e.ReadAttributes(this, context, s);
					e.ReadChildren(this, context, s);
				}
			}
		}

		public void Write(IO.EndianWriter s)
		{
			var context = GetContext(s.UserData);
			var elements = Elements;
			var pool = Pool;

			s.Write(kSignature);
			if (context.PointerSize == Shell.ProcessorSize.x64)
			{
				s.Pad32();
			}

			#region Elements header
			s.Write(elements.Count);
			if (context.PointerSize == Shell.ProcessorSize.x64)
			{
				s.Pad32();
			}
			var elements_offset_pos = s.MarkVirtualAddress(context.PointerSize);
			#endregion

			#region Pool header
			s.Write(pool.Size);
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
			foreach (var e in elements)
			{
				e.Write(s);
			}
			foreach (var e in elements)
			{
				e.WriteAttributes(s);
				e.WriteChildren(s);
			}

			var pool_offset = s.PositionPtr;
			pool.Write(s);

			s.Seek((long)elements_offset_pos);
			s.WriteVirtualAddress(elements_offset);
			s.Seek((long)pool_offset_pos);
			s.WriteVirtualAddress(pool_offset);
		}
		#endregion

		string ToString(XmbVariant v) => v.ToString(Pool);

		public XmlDocument ToXmlDocument()
		{
			var doc = new XmlDocument();
			XmlDocument result = ToXmlDocument(doc);

			System.Diagnostics.Debug.Assert(result != null);
			return result;
		}

		[return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(doc))]
		public XmlDocument? ToXmlDocument(XmlDocument? doc)
		{
			XmlDocument? result = doc;
			var elements = mElements;

			if (result != null && elements != null && elements.Count > 1)
			{
				XmbFile.Element root = elements[0];
				var root_e = root.ToXml(this, result, kNullRootElement);

				result.AppendChild(root_e);
			}

			System.Diagnostics.Debug.Assert(doc == null || result != null);
			return result;
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
			ArgumentException.ThrowIfNullOrEmpty(file);

			using (var fs = System.IO.File.Create(file))
			{
				ToXml(fs);
			}
		}
		public void ToXml(System.IO.Stream stream)
		{
			ArgumentNullException.ThrowIfNull(stream);

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
					dumpInfo.AddEntry(rawDataValue, kvp.Value.Single, kNullSingle24Description);
				}

				return true;
			}

			return false;
		}
	};
}
