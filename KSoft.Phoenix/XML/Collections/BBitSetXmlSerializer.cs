using System;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		[ThreadStatic]
		private static BBitSetXmlSerializer gBitSetXmlSerializer;

		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BBitSet bits, BBitSetXmlParams @params)
			where TDoc : class
			where TCursor : class
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentNullException.ThrowIfNull(bits);
			ArgumentNullException.ThrowIfNull(@params);
			if (!@params.UseElementName && !@params.ElementItselfMeansTrue)
			{
				throw new ArgumentException("Collection only supports element name filtering", nameof(@params));
			}

			if (gBitSetXmlSerializer == null)
			{
				gBitSetXmlSerializer = new BBitSetXmlSerializer();
			}

			var xs = gBitSetXmlSerializer;

			using (xs.Reset(@params, bits))
			{
				xs.Serialize(s);
			}
		}
	};
	internal sealed class BBitSetXmlSerializer
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		public BBitSetXmlParams Params { get; private set; }
		public Collections.BBitSet Bits { get; private set; }

		internal BBitSetXmlSerializer()
		{
		}

		internal BBitSetXmlSerializer Reset(BBitSetXmlParams @params, Collections.BBitSet bits)
		{
			Params = @params;
			Bits = bits;

			return this;
		}

		#region ITagElementStringNameStreamable Members
		Collections.IProtoEnum GetProtoEnum(Phx.BDatabaseBase db)
		{
			if (Bits.Params.kGetProtoEnum != null)
			{
				return Bits.Params.kGetProtoEnum();
			}

			return Bits.Params.kGetProtoEnumFromDB(db);
		}

		void ReadNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();
			Collections.IProtoEnum penum = Bits.InitializeFromEnum(xs.Database);

			if (Params.ElementItselfMeansTrue)
			{
				var getDefault = Bits.Params.kGetMemberDefaultValue;
				foreach (var e in s.Elements)
				{
					var element_name = s.GetElementName(e);
					int id = penum.TryGetMemberId(element_name);
					if (id.IsNone())
					{
						continue;
					}

					bool flag = true;
					s.StreamElementOpt(element_name, ref flag);

					if (getDefault != null && flag != getDefault(id))
					{
						// do nothing, allow the Set call below
					}
					else if (!flag)
					{
						continue;
					}

					Bits.Set(id, flag);
				}
			}
			else
			{
				foreach (var n in s.ElementsByName(Params.ElementName))
				{
					using (s.EnterCursorBookmark(n))
					{
						string name = null;
						Params.StreamDataName(s, ref name);

						int id = penum.GetMemberId(name);
						Bits.Set(id);
					}
				}
			}

			Bits.OptimizeStorage();
		}
		void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (Bits.EnabledCount == 0)
			{
				return;
			}

			var xs = s.GetSerializerInterface();
			Collections.IProtoEnum penum = GetProtoEnum(xs.Database);

			if (Bits.Params.kGetMemberDefaultValue != null)
			{
				if (!Params.ElementItselfMeansTrue)
				{
					throw new InvalidOperationException("Default-value serialization requires element text values.");
				}
				WriteNodesNotEqualToDefaultValues(s, penum);
				return;
			}

			foreach (var bitIndex in Bits.RawBits.SetBitIndices)
			{
				string name = penum.GetMemberName(bitIndex);

				if (Params.ElementItselfMeansTrue)
				{
					using (s.EnterCursorBookmark(name))
					{
						// do nothing
					}
				}
				else
				{
					using (s.EnterCursorBookmark(Params.ElementName))
					{
						Params.StreamDataName(s, ref name);
					}
				}
			}
		}
		void WriteNodesNotEqualToDefaultValues<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, Collections.IProtoEnum penum)
			where TDoc : class
			where TCursor : class
		{
			var getDefault = Bits.Params.kGetMemberDefaultValue;
			for (int x = 0; x < penum.MemberCount; x++)
			{
				bool bitDefault = getDefault(x);
				if (bitDefault == Bits[x])
				{
					continue;
				}

				string name = penum.GetMemberName(x);
				using (s.EnterCursorBookmark(name))
				{
					bool writtenValue = !bitDefault;
					s.WriteCursor(writtenValue);
				}
			}
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// #NOTE we don't check the book mark for null here because the root element is optional
			using (s.EnterCursorBookmarkOpt(Params.GetOptionalRootName()))
			{
				if (s.IsReading)
				{
					ReadNodes(s);
				}
				else if (s.IsWriting)
				{
					WriteNodes(s);
				}
			}
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			Params = null;
			Bits = null;
		}
		#endregion
	};
}