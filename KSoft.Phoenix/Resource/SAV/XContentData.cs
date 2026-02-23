
namespace KSoft.Phoenix.Resource.SAV
{
	public class XContentData
		: IO.IEndianStreamSerializable
	{
		const int XCONTENT_MAX_DISPLAYNAME_LENGTH = 128;
		const int XCONTENT_MAX_FILENAME_LENGTH = 42;
		public static int SizeOf => 0
			+ (sizeof(ushort) * XCONTENT_MAX_DISPLAYNAME_LENGTH)
			+ (sizeof(byte) * XCONTENT_MAX_FILENAME_LENGTH);

		static readonly Memory.Strings.StringStorage kDisplayNameStorage = new(
			Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageType.CString,
			// ByteOrder should be whatever the stream's byte order is...
			fixedLength: XCONTENT_MAX_DISPLAYNAME_LENGTH);
		static readonly Memory.Strings.StringStorage kFileNameStorage = new(
			Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageType.CString,
			fixedLength: XCONTENT_MAX_FILENAME_LENGTH);

		public string DisplayName;
		public string FileName;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref DisplayName, kDisplayNameStorage);
			s.Stream(ref FileName, kFileNameStorage);
		}
		#endregion
	};
}
