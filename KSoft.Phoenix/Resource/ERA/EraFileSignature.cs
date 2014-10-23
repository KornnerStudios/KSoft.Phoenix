using System;

namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFileSignature
		: IO.IEndianStreamSerializable
	{
		const uint kSignature = 0x05ABDBD8;
		const uint kSignatureMarker = 0xAAC94350;
		const byte kDefaultSizeBit = 0x13;

		const int kNonSignatureBytesSize = sizeof(uint) + sizeof(byte) + sizeof(uint);

		public byte SizeBit = kDefaultSizeBit;
		public byte[] SignatureData;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			bool reading = s.IsReading;

			int sig_data_length = reading || SignatureData == null
				? 0
				: SignatureData.Length;
			int size = reading
				? 0
				: kNonSignatureBytesSize + sig_data_length;

			s.StreamSignature(kSignature);
			s.Stream(ref size);
			if (size < kNonSignatureBytesSize)
				throw new System.IO.InvalidDataException(size.ToString("X8"));
			s.Pad(sizeof(ulong));

			s.StreamSignature(kSignatureMarker);
			s.Stream(ref SizeBit);
			if (reading)
				Array.Resize(ref SignatureData, size - kNonSignatureBytesSize);
			if (sig_data_length > 0)
				s.Stream(SignatureData);
			s.StreamSignature(kSignatureMarker);
		}
		#endregion
	};
}