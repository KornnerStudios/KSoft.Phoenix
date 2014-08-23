using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix
{
	static partial class PhxUtil
	{
		public const float kInvalidSingle = (float)TypeExtensions.kNone;
		public const float kInvalidSingleNaN = float.NaN;

		/// <summary>Sentinel for cases which reference undefined data (eg, an undefined ProtoObject)</summary>
		public const int kInvalidReference = TypeExtensions.kNone - 1;

		public static readonly Func<int> kGetInvalidInt32 = () => TypeExtensions.kNone;
		public static readonly Func<float> kGetInvalidSingle = () => kInvalidSingle;

		public static bool StrEqualsIgnoreCase(string str1, string str2)
		{
			return string.Compare(str1, str2, true) == 0;
		}
	};
}