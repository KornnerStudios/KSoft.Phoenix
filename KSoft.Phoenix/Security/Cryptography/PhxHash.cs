using System;
using System.Security.Cryptography;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Security.Cryptography
{
	using Debug = Phoenix.Debug;

	static class PhxHash
	{
		// NOTE: data is written to the buffer in MSB order
		static byte[] gUInt64Buffer = new byte[sizeof(ulong)];

		static void BufferFillUnicode(char unicode)
		{
			gUInt64Buffer[0] = (byte)(unicode >> 8);
			gUInt64Buffer[1] = (byte)(unicode >> 0);
		}

		public static void UInt8(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			gUInt64Buffer[0] = (byte)(word >> 0);

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(byte));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(byte), null, 0);
		}
		public static void UInt16(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			gUInt64Buffer[0] = (byte)(word >> 8);
			gUInt64Buffer[1] = (byte)(word >> 0);

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(ushort));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ushort), null, 0);
		}
		public static void UInt32(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			gUInt64Buffer[0] = (byte)(word >> 24);
			gUInt64Buffer[1] = (byte)(word >> 16);
			gUInt64Buffer[2] = (byte)(word >> 8);
			gUInt64Buffer[3] = (byte)(word >> 0);

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(uint));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(uint), null, 0);
		}
		public static void UInt64(SHA1CryptoServiceProvider sha, ulong word, bool isFinal = false)
		{
			gUInt64Buffer[0] = (byte)(word >> 56);
			gUInt64Buffer[1] = (byte)(word >> 48);
			gUInt64Buffer[2] = (byte)(word >> 40);
			gUInt64Buffer[3] = (byte)(word >> 32);
			gUInt64Buffer[4] = (byte)(word >> 24);
			gUInt64Buffer[5] = (byte)(word >> 16);
			gUInt64Buffer[6] = (byte)(word >> 8);
			gUInt64Buffer[7] = (byte)(word >> 0);

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(ulong));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ulong), null, 0);
		}

		public static void Ascii(SHA1CryptoServiceProvider sha, string str, int fixedLength = 0)
		{
			for (int x = 0; x < str.Length; x++)
			{
				gUInt64Buffer[0] = (byte)(str[x] >> 0);
				BufferFillUnicode(str[x]);
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(byte), null, 0);
			}

			gUInt64Buffer[0] = 0;
			for (int x = 0, null_count = fixedLength - str.Length; x < null_count; x++)
			{
				BufferFillUnicode('\0');
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(byte), null, 0);
			}
		}
		public static void Unicode(SHA1CryptoServiceProvider sha, string str, int fixedLength = 0)
		{
			for (int x = 0; x < str.Length; x++)
			{
				BufferFillUnicode(str[x]);
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ushort), null, 0);
			}

			BufferFillUnicode('\0');
			for (int x = 0, null_count = fixedLength - str.Length; x < null_count; x++)
			{
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ushort), null, 0);
			}
		}

		public const int kResultSize = 0x18;

		public static void Sha1Hash(string str, byte[] result)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(str));
			Contract.Requires<ArgumentNullException>(result != null);
			Contract.Requires(result.Length >= kResultSize);

			Array.Clear(result, 0, result.Length);

			byte[] str_bytes = System.Text.Encoding.ASCII.GetBytes(str);

			using (var sha = new SHA1CryptoServiceProvider())
			{
				byte[] result1;
				byte[] result_final;

				PhxHash.UInt32(sha, 0xA4800C14);
				//PhxHash.Ascii(sha, str);
				sha.TransformBlock(str_bytes, 0, str_bytes.Length, null, 0);
				PhxHash.UInt32(sha, 0x5AF4A9F1);
				PhxHash.UInt32(sha, 0xCA6884EC, true);
				result1 = sha.Hash;
				if (System.Diagnostics.Debugger.IsAttached)
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Result: {1}", str, Text.Util.ByteArrayToString(result1));

				sha.Initialize();
				PhxHash.UInt32(sha, 0xCB92EAEB);
				sha.TransformBlock(result1, 0, result1.Length, null, 0);
				PhxHash.UInt32(sha, 0x1D919BF8, true);
				result_final = sha.Hash;
				if (System.Diagnostics.Debugger.IsAttached)
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Final: {1}", str, Text.Util.ByteArrayToString(result_final));

				Array.Copy(result_final, result, result_final.Length);
			}
		}
	};
}