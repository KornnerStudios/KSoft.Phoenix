
namespace KSoft.Phoenix.Runtime
{
	sealed class BGameSetting
		: IO.IEndianStreamSerializable
	{
		string? mName;
		BGameSettingVariant mValue;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGameNullableSerialization.StreamString(s, ref mName);
			s.Stream(ref mValue);
		}
		#endregion
	};
}