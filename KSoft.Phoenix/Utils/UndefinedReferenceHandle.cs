using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

// The integer type used to represent a handle
using HandleWord = System.Int32;
// The unsigned equivalent of HandleWord's underlying type
using HandleWordUnsigned = System.UInt32;

namespace KSoft.Phoenix
{
	partial class PhxUtil
	{
		const HandleWordUnsigned kUndefinedReferenceHandleBitmask =
			unchecked((HandleWordUnsigned)HandleWord.MinValue); // 0x80...

		public static bool IsUndefinedReferenceHandle(HandleWord handle)
		{
			var uhandle = (HandleWordUnsigned)handle;

			return (uhandle & kUndefinedReferenceHandleBitmask) != 0;
		}
		public static HandleWord GetUndefinedReferenceDataIndex(HandleWord undefinedRefHandle)
		{
			var uhandle = (HandleWordUnsigned)undefinedRefHandle;

			return (HandleWord)(uhandle & ~kUndefinedReferenceHandleBitmask);
		}
		public static HandleWord GetUndefinedReferenceHandle(HandleWord undefinedRefDataIndex)
		{
			Contract.Requires(undefinedRefDataIndex < HandleWord.MaxValue,
				"Index value would generate a handle that matches the general invalid-handle sentinel");

			var index = (HandleWordUnsigned)undefinedRefDataIndex;

			return (HandleWord)(index | kUndefinedReferenceHandleBitmask);
		}

		public static bool IsUndefinedReferenceHandleOrNone(HandleWord handle)
		{
			if (IsUndefinedReferenceHandle(handle))
				return true;

			return handle.IsNone();
		}
	};
};