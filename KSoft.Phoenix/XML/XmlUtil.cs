using System.Collections.Generic;

namespace KSoft.Phoenix.XML
{
	public static partial class XmlUtil
	{
		/// <summary>Pass this to a Stream call when reading something from the Text data (which doesn't have a name), just to be clear of the code's intention</summary>
		public const string? kNoXmlName = null;
		public const IO.TagElementNodeType kSourceAttr = IO.TagElementNodeType.Attribute;
		public const IO.TagElementNodeType kSourceElement = IO.TagElementNodeType.Element;
		public const IO.TagElementNodeType kSourceCursor = IO.TagElementNodeType.Text;

		internal static void ValidateXmlSourceName(string? xmlName, IO.TagElementNodeType xmlSource)
		{
			if (xmlSource.RequiresName() != (xmlName != kNoXmlName))
			{
				throw new System.ArgumentException(
					"Element and attribute XML sources require a name; text XML sources require no name.",
					nameof(xmlName));
			}
		}
		internal static void ThrowIfAttributeSource(IO.TagElementNodeType xmlSource)
		{
			if (xmlSource == IO.TagElementNodeType.Attribute)
			{
				throw new System.ArgumentException("Attribute sources cannot be enumerated as XML nodes.", nameof(xmlSource));
			}
		}

		public static void ReadDetermineListSize<TDoc, TCursor, T>(IO.TagElementStream<TDoc, TCursor, string> s, List<T> list)
			where TDoc : class
			where TCursor : class
		{
			System.ArgumentNullException.ThrowIfNull(s);
			System.ArgumentNullException.ThrowIfNull(list);
			if (!s.IsReading)
			{
				throw new System.InvalidOperationException("List size can only be determined while reading XML.");
			}

			int child_element_count = s.TryGetCursorElementCount();
			if (list.Capacity < child_element_count)
			{
				list.Capacity = child_element_count;
			}
		}

		public static IEnumerable<TCursor> ReadGetNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string? xmlName, IO.TagElementNodeType xmlSource)
			where TDoc : class
			where TCursor : class
		{
			System.ArgumentNullException.ThrowIfNull(s);
			ValidateXmlSourceName(xmlName, xmlSource);
			ThrowIfAttributeSource(xmlSource);

			if (xmlSource == IO.TagElementNodeType.Text)
			{
				return EnumerateNodesByName(s, xmlName);
			}

			return s.Elements;
		}

		static IEnumerable<TCursor> EnumerateNodesByName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string? xmlName)
			where TDoc : class
			where TCursor : class
		{
			System.ArgumentNullException.ThrowIfNull(xmlName, "localName");
			foreach (var node in s.ElementsByName(xmlName))
			{
				yield return node;
			}
		}
		internal static void StreamString<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string? xmlName, ref string? value, bool toLower,
			IO.TagElementNodeType xmlSource, bool intern = false)
			where TDoc : class
			where TCursor : class
		{
			ValidateXmlSourceName(xmlName, xmlSource);

			if (s.IsReading)
			{
				string streamedValue = string.Empty;
				if (xmlSource == IO.TagElementNodeType.Text)
				{
					s.StreamCursor(ref streamedValue);
				}
				else
				{
					System.ArgumentNullException.ThrowIfNull(xmlName);
					s.StreamString(xmlName, ref streamedValue, toLower, xmlSource, intern);
					value = streamedValue;
					return;
				}

				if (toLower) streamedValue = streamedValue.ToLowerInvariant();
				if (intern) streamedValue = string.Intern(streamedValue);
				value = streamedValue;
			}
			else if (s.IsWriting)
			{
				System.ArgumentNullException.ThrowIfNull(value);
				string streamedValue = value;
				if (xmlSource == IO.TagElementNodeType.Text)
				{
					s.StreamCursor(ref streamedValue);
				}
				else
				{
					System.ArgumentNullException.ThrowIfNull(xmlName);
					s.StreamString(xmlName, ref streamedValue, toLower, xmlSource, intern);
				}
				value = streamedValue;
			}
		}


		internal static bool StreamStringOpt<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string? xmlName, ref string? value, bool toLower,
			IO.TagElementNodeType xmlSource, bool intern = false)
			where TDoc : class
			where TCursor : class
		{
			ValidateXmlSourceName(xmlName, xmlSource);

			if (xmlSource != IO.TagElementNodeType.Text)
			{
				System.ArgumentNullException.ThrowIfNull(xmlName);
				return s.StreamStringOpt(xmlName, ref value, toLower, xmlSource, intern);
			}

			value ??= string.Empty;
			string streamedValue = value;
			s.StreamCursor(ref streamedValue);

			if (s.IsReading)
			{
				System.ArgumentNullException.ThrowIfNull(streamedValue);
				if (toLower) streamedValue = streamedValue.ToLowerInvariant();
				if (intern) streamedValue = string.Intern(streamedValue);
			}

			value = streamedValue;
			return true;
		}
	};
}

namespace KSoft.Phoenix
{
	static partial class PhxUtil
	{
		public static bool StreamBVector<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string? xmlName, ref BVector vector
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.ValidateXmlSourceName(xmlName, xmlSource);

			string? string_value = null;
			bool was_streamed = true;
			const bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
				{
					was_streamed = XML.XmlUtil.StreamStringOpt(s, xmlName, ref string_value, to_lower, xmlSource);
				}
				else
				{
					XML.XmlUtil.StreamString(s, xmlName, ref string_value, to_lower, xmlSource);
				}

				if (was_streamed)
				{
					System.ArgumentNullException.ThrowIfNull(string_value);
					var parse_result = PhxUtil.ParseBVectorString(string_value);
					if (!parse_result.HasValue)
					{
						s.ThrowReadException(new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
							$"Failed to parse value (hint: {(xmlSource.RequiresName() ? xmlName : "ElementText")}) as vector: {string_value}")));
					}

					vector = parse_result.Value;
				}
			}
			else if (s.IsWriting)
			{
				if (isOptional && PhxPredicates.IsZero(vector))
				{
					was_streamed = false;
					return was_streamed;
				}

				string_value = vector.ToBVectorString();

				if (isOptional)
				{
					XML.XmlUtil.StreamStringOpt(s, xmlName, ref string_value, to_lower, xmlSource);
				}
				else
				{
					XML.XmlUtil.StreamString(s, xmlName, ref string_value, to_lower, xmlSource);
				}
			}

			return was_streamed;
		}

		public static bool StreamIntegerColor<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string? xmlName, ref System.Drawing.Color color
			, byte defaultAlpha = 0xFF
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.ValidateXmlSourceName(xmlName, xmlSource);

			string? string_value = null;
			bool was_streamed = true;
			const bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
				{
					was_streamed = XML.XmlUtil.StreamStringOpt(s, xmlName, ref string_value, to_lower, xmlSource);
				}
				else
				{
					XML.XmlUtil.StreamString(s, xmlName, ref string_value, to_lower, xmlSource);
				}

				if (was_streamed)
				{
					System.ArgumentNullException.ThrowIfNull(string_value);
					if (!PhxUtil.TokenizeIntegerColor(string_value, defaultAlpha, ref color))
					{
						s.ThrowReadException(new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
							$"Failed to parse value (hint: {(xmlSource.RequiresName() ? xmlName : "ElementText")}) as color: {string_value}")));
					}
				}
			}
			else if (s.IsWriting)
			{
				if (isOptional && PhxPredicates.IsZero(color))
				{
					was_streamed = false;
					return was_streamed;
				}

				string_value = color.ToIntegerColorString(defaultAlpha);

				if (isOptional)
				{
					XML.XmlUtil.StreamStringOpt(s, xmlName, ref string_value, to_lower, xmlSource);
				}
				else
				{
					XML.XmlUtil.StreamString(s, xmlName, ref string_value, to_lower, xmlSource);
				}
			}

			return was_streamed;
		}

		public static bool StreamProtoEnum<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string? xmlName, ref int dbid
			, Collections.IProtoEnum protoEnum
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement
			, int isOptionalDefaultValue = TypeExtensions.kNone)
			where TDoc : class
			where TCursor : class
		{
			XML.XmlUtil.ValidateXmlSourceName(xmlName, xmlSource);
			System.ArgumentNullException.ThrowIfNull(protoEnum);

			string? id_name = null;
			bool was_streamed = true;
			bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
				{
					was_streamed = XML.XmlUtil.StreamStringOpt(s, xmlName, ref id_name, to_lower, xmlSource, intern: true);
				}
				else
				{
					XML.XmlUtil.StreamString(s, xmlName, ref id_name, to_lower, xmlSource, intern: true);
				}

				if (was_streamed)
				{
					System.ArgumentNullException.ThrowIfNull(id_name);
					dbid = protoEnum.TryGetMemberId(id_name);
					if (dbid.IsNone())
					{
						s.ThrowReadException(new System.IO.InvalidDataException(string.Create(KSoft.Util.InvariantCultureInfo,
							$"Failed to resolve proto enum member '{id_name}' from {(xmlSource.RequiresName() ? xmlName : "ElementText")}.")));
					}
				}
				//else
				//	dbid = isOptionalDefaultValue;
			}
			else if (s.IsWriting)
			{
				if (isOptional && isOptionalDefaultValue.IsNotNone() && isOptionalDefaultValue == dbid)
				{
					was_streamed = false;
					return was_streamed;
				}

				id_name = protoEnum.TryGetMemberName(dbid);
				if (id_name.IsNullOrEmpty())
				{
					throw new System.InvalidOperationException(string.Create(KSoft.Util.InvariantCultureInfo,
						$"Failed to resolve proto enum member name for id {dbid}."));
				}

				if (isOptional)
				{
					XML.XmlUtil.StreamStringOpt(s, xmlName, ref id_name, to_lower, xmlSource, intern: true);
				}
				else
				{
					XML.XmlUtil.StreamString(s, xmlName, ref id_name, to_lower, xmlSource, intern: true);
				}
			}

			return was_streamed;
		}
	};
}