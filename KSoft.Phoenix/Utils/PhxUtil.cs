﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

using BVector = SlimMath.Vector4;

namespace KSoft.Phoenix
{
	static partial class PhxUtil
	{
		public const float kInvalidSingle = (float)TypeExtensions.kNone;
		public const float kInvalidSingleNaN = float.NaN;

		/// <summary>Sentinel for cases which reference undefined data (eg, an undefined ProtoObject)</summary>
		public const int kInvalidReference = TypeExtensions.kNone - 1;

		private static Func<int> gGetInvalidInt32;
		public static Func<int> kGetInvalidInt32 { get {
			if (gGetInvalidInt32 == null)
				gGetInvalidInt32 = () => TypeExtensions.kNone;

			return gGetInvalidInt32;
		} }

		private static Func<float> gGetInvalidSingle;
		public static Func<float> kGetInvalidSingle { get {
			if (gGetInvalidSingle == null)
				gGetInvalidSingle = () => kInvalidSingle;

			return gGetInvalidSingle;
		} }

		public static bool StrEqualsIgnoreCase(string str1, string str2)
		{
			return string.Compare(str1, str2, true) == 0;
		}

		public static string ToLowerIfContainsUppercase(this string str)
		{
			if (str == null)
				return str;

			for (int x = 0; x < str.Length; x++)
			{
				char c = str[x];
				if (c >= 'A' && c <= 'Z')
					return str.ToLowerInvariant();
			}

			return str;
		}

		public static bool StreamPointerizedCString(IO.EndianStream s, ref Values.PtrHandle pointer, ref string value)
		{
			Contract.Requires(s != null);

			bool streamed = false;

			if (s.IsReading)
			{
				if (!pointer.IsInvalidHandle)
				{
					s.Seek((long)pointer.u64, System.IO.SeekOrigin.Begin);
					value = s.Reader.ReadString(Memory.Strings.StringStorage.CStringAscii);
					streamed = true;
				}
			}
			else if (s.IsWriting)
			{
				if (string.IsNullOrEmpty(value))
				{
					pointer = pointer.Is64bit
						? Values.PtrHandle.InvalidHandle64
						: Values.PtrHandle.InvalidHandle32;
				}
				else
				{
					pointer = new Values.PtrHandle(pointer, (ulong)s.BaseStream.Position);
					s.Writer.Write(value, Memory.Strings.StringStorage.CStringAscii);
					streamed = true;
				}
			}

			return streamed;
		}

		public static int CalculateHashCodeForDBIDs(IList<int> dbidList)
		{
			if (dbidList == null || dbidList.Count == 0)
				return 0;

			int hash_code = 0;
			for (int x = 0; x < dbidList.Count; x++)
			{
				int dbid = dbidList[x];
				hash_code ^= dbid.GetHashCode();
			}

			return hash_code;
		}

		private static string[] Trim(string[] array)
		{
			var trimmed = new string[array.Length];

			for (int x = 0; x < array.Length; x++)
				trimmed[x] = array[x].Trim();

			return trimmed;
		}

		public static bool ParseStringList(string line, List<string> list,
			bool sort = false, string valueSeperator = ",")
		{
			if (line == null)
			{
				return false;
			}
			if (list == null)
			{
				Contract.Assert(list != null);
				return false;
			}

			// LINQ stmt below allows there to be whitespace around the commas
			string[] values = System.Text.RegularExpressions.Regex.Split(line, valueSeperator);
			list.Clear();

			ParseStringList(Trim(values), list, sort);

			// handles cases where there's extra valueSeperator values
			list.RemoveAll(string.IsNullOrEmpty);

			return true;
		}
		public static bool ParseStringList(IEnumerable<string> collection, List<string> list,
			bool sort = false)
		{
			if (collection == null)
			{
				return false;
			}
			if (list == null)
			{
				Contract.Assert(list != null);
				return false;
			}

			list.AddRange(collection);

			if (sort)
				list.Sort();

			return true;
		}

		public static string StringListToString(IEnumerable<string> list,
			string valueSeperator = ",")
		{
			if (list == null)
			{
				Contract.Assert(list != null);
				return null;
			}

			var sb = new System.Text.StringBuilder();
			foreach (var str in list)
			{
				if (sb.Length > 0)
					sb.Append(valueSeperator);

				sb.Append(str);
			}

			return sb.ToString();
		}

		[ThreadStatic]
		private static List<string> gParseBVectorStringScratchList;
		public static BVector? ParseBVectorString(string vectorString)
		{
			var vector = new BVector();
			if (vectorString.IsNullOrEmpty())
				return vector;

			if (gParseBVectorStringScratchList == null)
				gParseBVectorStringScratchList = new List<string>(4);
			var list = gParseBVectorStringScratchList;

			if (!ParseStringList(vectorString, list))
				return null;

			if (list.Count >= 1 && !float.TryParse(list[0], out vector.X))
				return null;
			if (list.Count >= 2 && !float.TryParse(list[1], out vector.Y))
				return null;
			if (list.Count >= 3 && !float.TryParse(list[2], out vector.Z))
				return null;
			if (list.Count >= 4 && !float.TryParse(list[3], out vector.W))
				return null;

			return vector;
		}

		public static string ToBVectorString(this BVector vector, int length = 3)
		{
			if (PhxPredicates.IsZero(vector))
			{
				switch (length)
				{
					case 1:
						return "0";
					case 2:
						return "0,0";
					case 3:
						return "0,0,0";
					case 4:
						return "0,0,0,0";

					default:
						return "";
				}
			}

			var sb = new System.Text.StringBuilder(32);
			if (length >= 1)
				sb.Append(vector.X);
			if (length >= 2)
				sb.AppendFormat(",{0}", vector.Y.ToString());
			if (length >= 3)
				sb.AppendFormat(",{0}", vector.Z.ToString());
			if (length >= 4)
				sb.AppendFormat(",{0}", vector.W.ToString());

			return sb.ToString();
		}

		public static int CompareTo(this BVector vector, BVector other)
		{
			if (vector.X != other.X)
				return vector.X.CompareTo(other.X);

			if (vector.Y != other.Y)
				return vector.Y.CompareTo(other.Y);

			if (vector.Z != other.Z)
				return vector.Z.CompareTo(other.Z);

			return vector.W.CompareTo(other.W);
		}

		/// <summary>Get the index of the next token</summary>
		/// <param name="src">string to process</param>
		/// <param name="tokens">characters to use as tokens</param>
		/// <param name="srcIndex">Where to start processing for tokens</param>
		/// <param name="distance">Number of characters traversed during this call</param>
		/// <returns></returns>
		public static int NextToken(string src, string tokens, int srcIndex, ref int distance)
		{
			Contract.Requires(!tokens.IsNullOrEmpty());

			if (src.IsNullOrEmpty())
				return -1;
			if (srcIndex >= src.Length)
				return -1;

			if (srcIndex < 0)
				srcIndex = 0;

			// skip any initial tokens
			int count = 0;
			for (; count < src.Length; count++, srcIndex++)
			{
				char c = src[srcIndex];
				if (tokens.IndexOf(c) < 0)
					break;
			}

			// figure out the distance until the next token
			int copy_length = 0;
			for (; count < src.Length && srcIndex+copy_length < src.Length; count++, copy_length++)
			{
				char c = src[srcIndex+copy_length];
				if (tokens.IndexOf(c) >= 0)
					break;
			}

			if (copy_length == 0)
				return -1;

			distance = copy_length;
			return srcIndex + copy_length;
		}

		const string kTokenizeTokens = " ,\t\r\n";

		public static bool TokenizeIntegerColor(string src, byte defaultAlpha, ref System.Drawing.Color color)
		{
			int next_index = -1;
			int v_length = -1;

			int v1 = 0;
			next_index = NextToken(src, kTokenizeTokens, next_index, ref v_length);
			if (next_index < 0)
				return false;
			if (!Numbers.TryParseRange(src, out v1, startIndex: next_index - v_length, length: v_length))
				return false;

			int v2 = 0;
			next_index = NextToken(src, kTokenizeTokens, next_index, ref v_length);
			if (next_index < 0)
				return false;
			if (!Numbers.TryParseRange(src, out v2, startIndex: next_index - v_length, length: v_length))
				return false;

			int v3 = 0;
			next_index = NextToken(src, kTokenizeTokens, next_index, ref v_length);
			if (next_index < 0)
				return false;
			if (!Numbers.TryParseRange(src, out v3, startIndex: next_index - v_length, length: v_length))
				return false;

			int v4 = 0;
			next_index = NextToken(src, kTokenizeTokens, next_index, ref v_length);
			// if v4 is present, ARGB, else RGB
			if (next_index >= 0)
			{
				if (!Numbers.TryParseRange(src, out v4, startIndex: next_index - v_length, length: v_length))
					return false;

				color = System.Drawing.Color.FromArgb(v1, v2, v3, v4);
			}
			else
			{
				color = System.Drawing.Color.FromArgb(defaultAlpha, v1, v2, v3);
			}

			return true;
		}

		public static string ToIntegerColorString(this System.Drawing.Color color, byte defaultAlpha = 0xFF)
		{
			if (PhxPredicates.IsZero(color) || (color.A==defaultAlpha && PhxPredicates.IsRgbZero(color)))
			{
				return "0 0 0";
			}

			var sb = new System.Text.StringBuilder(16);
			if (color.A != defaultAlpha)
			{
				sb.Append(color.A);
				sb.Append(' ');
			}

			sb.Append(' ');
			sb.Append(color.R);

			sb.Append(' ');
			sb.Append(color.G);

			sb.Append(' ');
			sb.Append(color.B);

			return sb.ToString();
		}

		#region Dummy comparer
		private sealed class DummyComparerAlwaysNonZero<T>
			: IComparer<T>
		{
			public int Compare(T x, T y)
			{
				return -1;
			}
		};

		public static IComparer<T> CreateDummyComparerAlwaysNonZero<T>()
		{
			return new DummyComparerAlwaysNonZero<T>();
		}
		#endregion

		public static bool UpdateResultWithTaskResults(ref bool r, List<Task<bool>> tasks, List<Exception> exceptions = null)
		{
			foreach (var task in tasks)
			{
				if (task.IsFaulted)
				{
					r = false;
					if (exceptions != null)
						exceptions.Add(task.Exception);
				}
				else
					r &= task.Result;
			}

			return r;
		}
	};
}