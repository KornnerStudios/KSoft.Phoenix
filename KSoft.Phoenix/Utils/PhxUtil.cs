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
	};
}