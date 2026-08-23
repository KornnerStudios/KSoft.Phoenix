using System;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		[ThreadStatic]
		private static BBitSetXmlSerializer? gBitSetXmlSerializer;

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

			var xs = gBitSetXmlSerializer ??= new BBitSetXmlSerializer();

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
		private BBitSetXmlParams? mParams;
		public BBitSetXmlParams Params => mParams!;

		private Collections.BBitSet? mBits;
		public Collections.BBitSet Bits => mBits!;

		private string ElementName => Params.ElementName
			?? throw new InvalidOperationException("Bit-set XML serialization requires an element name.");

		internal BBitSetXmlSerializer()
		{
		}

		internal BBitSetXmlSerializer Reset(BBitSetXmlParams @params, Collections.BBitSet bits)
		{
			mParams = @params;
			mBits = bits;

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
					var element_name = s.GetElementName(e)!;
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
				foreach (var n in s.ElementsByName(ElementName))
				{
					using (s.EnterCursorBookmark(n))
					{
						string name = null!;
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
					using (s.EnterCursorBookmark(ElementName))
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
			var getDefault = Bits.Params.kGetMemberDefaultValue
				?? throw new InvalidOperationException("Bit-set default values require a default-value provider.");
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
			var rootName = Params.GetOptionalRootName();
			if (rootName is null)
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
			else
			{
				using (s.EnterCursorBookmarkOpt(rootName))
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
		}
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			mParams = null;
			mBits = null;
		}
		#endregion
	};
}