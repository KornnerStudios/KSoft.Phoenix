using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;
using Exprs = System.Linq.Expressions;

namespace KSoft.Phoenix
{
	/// <summary>Extension methods for types in this assembly</summary>
	public static partial class TypeExtensionsPhx
	{
		#region Enum Bit Encoders
		public static class BitEncoders
		{
			// KSoft.Phoenix.Xmb
			public static readonly EnumBitEncoder32<Xmb.BinaryDataTreeVariantType>
				BinaryDataTreeVariantType = new EnumBitEncoder32<Xmb.BinaryDataTreeVariantType>();
			public static readonly EnumBitEncoder32<Xmb.BinaryDataTreeVariantTypeSizeInBytes>
				BinaryDataTreeVariantTypeSizeInBytes = new EnumBitEncoder32<Xmb.BinaryDataTreeVariantTypeSizeInBytes>();
		};
		#endregion

		#region PascalString32
		static readonly Memory.Strings.StringStorage Pascal32Storage =
					new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big);
		static readonly Text.StringStorageEncoding Pascal32Encoding = new Text.StringStorageEncoding(Pascal32Storage);

		public static IO.EndianStream StreamPascalString32(this IO.EndianStream s, ref string value)
		{
				 if (s.IsReading) value = s.Reader.ReadString(Pascal32Encoding);
			else if (s.IsWriting) s.Writer.Write(value, Pascal32Encoding);

			return s;
		}
		#endregion

		#region PascalWideString32
		static readonly Memory.Strings.StringStorage PascalUnicode32Storage =
					new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageLengthPrefix.Int32, Shell.EndianFormat.Big);
		static readonly Text.StringStorageEncoding PascalUnicode32Encoding = new Text.StringStorageEncoding(PascalUnicode32Storage);

		public static IO.EndianStream StreamPascalWideString32(this IO.EndianStream s, ref string value)
		{
				 if (s.IsReading) value = s.Reader.ReadString(PascalUnicode32Encoding);
			else if (s.IsWriting) s.Writer.Write(value, PascalUnicode32Encoding);

			return s;
		}
		#endregion

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

		public static Exception ToAggregateExceptionOrNull(this List<Exception> list)
		{
			if (list.IsNullOrEmpty())
				return null;

			if (list.Count == 1)
			{
				var e = list[0];
				var ae = e as AggregateException;
				if (ae != null)
					return ae.GetOnlyExceptionOrAll();

				return e;
			}

			return new AggregateException(list);
		}

		public static bool StreamCursorBytesOpt<TDoc, TCursor, T>(this IO.TagElementStream<TDoc, TCursor, string> s, T obj, Exprs.Expression<Func<T, byte[]>> propExpr)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);

			bool executed = false;

			var property = Reflection.Util.PropertyFromExpr(propExpr);
			if (s.IsReading)
			{
				string str_value = null;
				s.ReadCursor(ref str_value);
				if (str_value.IsNotNullOrEmpty())
				{
					var value = Text.Util.ByteStringToArray(str_value);
					if (value.IsNotNullOrEmpty())
					{
						property.SetValue(obj, value, null);
						executed = true;
					}
				}
			}
			else if (s.IsWriting)
			{
				var value = (byte[])property.GetValue(obj, null);
				if (value.IsNotNullOrEmpty())
				{
					string str_value = Text.Util.ByteArrayToString(value);
					if (str_value.IsNotNullOrEmpty())
					{
						s.WriteCursor(str_value);
						executed = true;
					}
				}
			}

			return executed;
		}
	};
}