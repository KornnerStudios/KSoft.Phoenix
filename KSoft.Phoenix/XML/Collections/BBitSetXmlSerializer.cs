using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BBitSet bits, BBitSetXmlParams @params)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(bits != null);
			Contract.Requires(@params != null);

			using (var xs = new BBitSetXmlSerializer(@params, bits))
			{
				xs.Serialize(s);
			}
		}
	};
	internal sealed class BBitSetXmlSerializer
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		public BListXmlParams Params { get; private set; }
		public Collections.BBitSet Bits { get; private set; }

		public BBitSetXmlSerializer(BListXmlParams @params, Collections.BBitSet bits)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(bits != null);
			Contract.Requires(@params.UseElementName, "Collection only supports element name filtering");

			Params = @params;
			Bits = bits;
		}

		#region IXmlElementStreamable Members
		Collections.IProtoEnum GetProtoEnum(Phx.BDatabaseBase db)
		{
			if (Bits.Params.kGetProtoEnum != null)
				return Bits.Params.kGetProtoEnum();

			return Bits.Params.kGetProtoEnumFromDB(db);
		}

		void ReadNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			Collections.IProtoEnum penum = Bits.InitializeFromEnum(s.GetSerializerInterface().Database);

			foreach (var n in s.ElementsByName(Params.ElementName))
			{
				using (s.EnterCursorBookmark(n))
				{
					string name = null;
					Params.StreamDataName(s, ref name);

					int id = penum.GetMemberId(name);
					Bits[id] = true;
				}
			}

			Bits.OptimizeStorage();
		}
		void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (Bits.EnabledCount == 0)
				return;

			Collections.IProtoEnum penum = GetProtoEnum(s.GetSerializerInterface().Database);

			for (int x = 0; x < Bits.Count; x++)
				if (Bits[x])
					using (s.EnterCursorBookmark(Params.ElementName))
					{
						string name = penum.GetMemberName(x);
						Params.StreamDataName(s, ref name);
					}
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// #NOTE we don't check the book mark for null here because the root element is optional
			using (s.EnterCursorBookmarkOpt(Params.GetOptionalRootName()))
			{
					 if (s.IsReading)	ReadNodes(s);
				else if (s.IsWriting)	WriteNodes(s);
			}
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
		}
		#endregion
	};
}