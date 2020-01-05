using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix
{
	static partial class PhxUtil
	{
		public static Security.Cryptography.Crc16.Definition kCrc16Definition =
			new Security.Cryptography.Crc16.Definition(
				initialValue: ushort.MinValue,
				xorIn: ushort.MaxValue,
				xorOut: ushort.MaxValue);
		public static Security.Cryptography.Crc32.Definition kCrc32Definition =
			new Security.Cryptography.Crc32.Definition(
				initialValue: uint.MinValue,
				xorIn: uint.MaxValue,
				xorOut: uint.MaxValue);

		public const int kObjectKindNone = 0;

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
			return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
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

			if (!Util.ParseStringList(vectorString, list))
				return null;

			if (list.Count >= 1 && !Numbers.FloatTryParseInvariant(list[0], out vector.X))
				return null;
			if (list.Count >= 2 && !Numbers.FloatTryParseInvariant(list[1], out vector.Y))
				return null;
			if (list.Count >= 3 && !Numbers.FloatTryParseInvariant(list[2], out vector.Z))
				return null;
			if (list.Count >= 4 && !Numbers.FloatTryParseInvariant(list[3], out vector.W))
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
				sb.Append(vector.X.ToStringInvariant(Numbers.kFloatRoundTripFormatSpecifier));
			if (length >= 2)
				sb.AppendFormat(",{0}", vector.Y.ToStringInvariant(Numbers.kFloatRoundTripFormatSpecifier));
			if (length >= 3)
				sb.AppendFormat(",{0}", vector.Z.ToStringInvariant(Numbers.kFloatRoundTripFormatSpecifier));
			if (length >= 4)
				sb.AppendFormat(",{0}", vector.W.ToStringInvariant(Numbers.kFloatRoundTripFormatSpecifier));

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
				try
				{
					task.Wait();
				} catch (Exception ex)
				{
					ex.UnusedExceptionVar();
				}

				if (task.IsFaulted)
				{
					r = false;
					if (exceptions != null)
						exceptions.Add(task.Exception.GetOnlyExceptionOrAll());
				}
				else
				{
					r &= task.Result;
				}
			}

			return r;
		}

		public static bool FindBytePattern(List<int> results, byte[] input, ref int inOutOffset, params short[] pattern)
		{
			int end = input.Length - pattern.Length;
			int end_offset = inOutOffset;

			bool found_pattern = false;
			for (int start = inOutOffset; start < end && !found_pattern; start++, end_offset++)
			{
				var first_byte = pattern[0];
				if (first_byte != input[start])
					continue;

				for (int offset = 1; offset < pattern.Length; offset++)
				{
					int index = start + offset;
					var next_byte = pattern[offset];
					if (next_byte < 0)
						continue;
					else if (next_byte != input[index])
						break;
					else if (offset == pattern.Length-1)
					{
						results.Add(start);
						end_offset += pattern.Length;
						found_pattern = true;
						break;
					}
				}
			}

			inOutOffset = end_offset;
			return found_pattern;
		}

		[ThreadStatic]
		private static byte[] gSharedBufferForSuperFastHash;
		public static byte[] GetBufferForSuperFastHash(int bufferSize)
		{
			if (gSharedBufferForSuperFastHash == null)
				gSharedBufferForSuperFastHash = new byte[16];
			else
				gSharedBufferForSuperFastHash.FastClear();

			var buffer = gSharedBufferForSuperFastHash;
			if (bufferSize > buffer.Length)
				buffer = new byte[bufferSize];

			return buffer;
		}
		public static uint SuperFastHash(byte[] buffer, uint initialValue = 0)
		{
			Contract.Requires(buffer != null);

			return SuperFastHash(buffer, 0, buffer.Length, initialValue);
		}
		public static uint SuperFastHash(byte[] buffer, int startIndex, int length, uint initialValue = 0)
		{
			Contract.Requires(buffer != null);
			Contract.Requires(startIndex >= 0 && length >= 0);
			Contract.Requires(startIndex+length <= buffer.Length);

			// Based on code by Paul Hsieh
			// http://www.azillionmonkeys.com/qed/hash.html

			uint hash = initialValue;

			int length_rem = length & (sizeof(uint)-1);
			int words = length / sizeof(uint);
			int index = startIndex;

			// Main loop
			for (; words > 0; words--)
			{
				hash += (uint)(BitConverter.ToUInt16(buffer, index));
				index += sizeof(ushort);
				hash ^= hash << 16;
				hash ^= (uint)(BitConverter.ToUInt16(buffer, index) << 11);
				index += sizeof(ushort);
				hash += hash >> 11;
			}

			// Handle end cases
			switch (length_rem)
			{
				case sizeof(ushort)+1:
					hash += (uint)(BitConverter.ToUInt16(buffer, index));
					index += sizeof(ushort);
					hash ^= (uint)(buffer[index] << 18);
					hash += hash >> 11;
					break;
				case sizeof(ushort):
					hash += (uint)(BitConverter.ToUInt16(buffer, index));
					index += sizeof(ushort);
					hash ^= hash << 11;
					hash += hash >> 17;
					break;
				case sizeof(byte):
					hash += buffer[index++];
					hash ^= hash << 10;
					hash += hash >> 1;
					break;
			}

			// Force "avalanching" of final 127 bits
			hash ^= hash << 3;
			hash += hash >> 5;
			hash ^= hash << 2;
			hash += hash >> 15;
			hash ^= hash << 10;

			return hash;
		}
	};
}