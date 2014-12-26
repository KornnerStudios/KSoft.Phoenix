using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix
{
	/// <summary>Extension methods for types in this assembly</summary>
	public static class TypeExtensionsPhx
	{
		static readonly Memory.Strings.StringStorage Pascal32Storage =
					new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big);
		static readonly Text.StringStorageEncoding Pascal32Encoding = new Text.StringStorageEncoding(Pascal32Storage);

		public static IO.EndianStream StreamPascalString32(this IO.EndianStream s, ref string value)
		{
				 if (s.IsReading) value = s.Reader.ReadString(Pascal32Encoding);
			else if (s.IsWriting) s.Writer.Write(value, Pascal32Encoding);

			return s;
		}

		static readonly Memory.Strings.StringStorage PascalUnicode32Storage =
					new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big);
		static readonly Text.StringStorageEncoding PascalUnicode32Encoding = new Text.StringStorageEncoding(PascalUnicode32Storage);

		public static IO.EndianStream StreamPascalWideString32(this IO.EndianStream s, ref string value)
		{
				 if (s.IsReading) value = s.Reader.ReadString(PascalUnicode32Encoding);
			else if (s.IsWriting) s.Writer.Write(value, PascalUnicode32Encoding);

			return s;
		}

		public static IO.EndianStream StreamNotNull<T>(this IO.EndianStream s, ref T obj)
			where T : class, IO.IEndianStreamSerializable, new()
		{
			bool not_null = obj != null;

			s.Stream(ref not_null);
			if (s.IsReading && not_null)
				obj = new T();

			if (not_null)
				s.Stream(obj);

			return s;
		}

		// stream a "condition". mainly a helper for GameFile code
		public static bool StreamCond<T>(this IO.EndianStream s, T ctxt, Predicate<T> writePredicate)
		{
			bool cond = s.IsReading
				? false
				: writePredicate(ctxt);

			s.Stream(ref cond);

			return cond;
		}

		public static bool HasXmbVariantSupport(this TypeCode c)
		{
			switch (c)
			{
			case TypeCode.Object:
			case TypeCode.DBNull:
			case TypeCode.Decimal:
			case TypeCode.String:
				return false;

			default:
				return true;
			}
		}

		public static XML.BXmlSerializerInterface GetSerializerInterface<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			Contract.Assert(s.Owner == null || s.Owner is XML.BXmlSerializerInterface);

			return (XML.BXmlSerializerInterface)s.Owner;
		}
		public static void SetSerializerInterface<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s,
			XML.BXmlSerializerInterface xsi)
			where TDoc : class
			where TCursor : class
		{
			if(xsi != null)
				Contract.Assert(s.Owner == null || !(s.Owner is XML.BXmlSerializerInterface));

			s.Owner = xsi;
		}
	};
}