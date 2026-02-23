using System;
using KSoft.Security.Cryptography;

namespace KSoft.Phoenix.Resource.SAV;

public sealed class WinSaveFile
{
	public enum OpenSavResult
	{
		Invalid,
		Valid,
		ValidAndNeedsDecryption,
	};

	public const string kExtensionEncrypted = ".sav";
	public const string kExtensionDecrypted = ".sav_decrypted";

	const int kTitleVersionSize = sizeof(Phoenix.Runtime.UserProfile.UserProfileTitleVersion);

	#region File Size utils
	/// <summary>
	/// BUserProfile data is in the first 8KiB of the save.sav, then after
	/// that is the campaign save.
	/// </summary>
	public const int cFixedProfileSettingSize = 8 * IntegerMath.kKilo;

	public static bool IsValidFileSize(long fileSize)
		=> FileSizeIncludesUserProfileSettings(fileSize);

	public static bool FileSizeIncludesUserProfileSettings(long fileSize)
		=> fileSize >= cFixedProfileSettingSize;

	public static bool FileSizeLikelyIncludesCampaignData(long fileSize)
		=> fileSize > (cFixedProfileSettingSize + XContentData.SizeOf);
	#endregion

	public static OpenSavResult OpenSav(string filePath, out System.IO.MemoryStream fileMemoryStream)
	{
		OpenSavResult result = OpenSavResult.Invalid;
		fileMemoryStream = null;

		// Intentionally not handling IO exceptions here
		using (var fs = System.IO.File.OpenRead(filePath))
		{
			if (!IsValidFileSize(fs.Length))
			{
				return OpenSavResult.Invalid;
			}

			int firstByte = fs.ReadByte();
			if (firstByte == -1)
			{
				// This code path should be impossible
				return OpenSavResult.Invalid;
			}

			fs.Position = 0;
			result = firstByte < (byte)Phoenix.Runtime.UserProfile.UserProfileTitleVersion.AddEncryption
				? OpenSavResult.Valid
				: OpenSavResult.ValidAndNeedsDecryption;

			// For now, I'm not going to deal with versioning
			if (firstByte != (byte)Phoenix.Runtime.UserProfile.UserProfileTitleVersion.kLatest)
			{
				return OpenSavResult.Invalid;
			}

			var fileBytes = new byte[fs.Length];
			fs.ReadExactly(fileBytes);
			fileMemoryStream = new System.IO.MemoryStream(
				fileBytes, 0, fileBytes.Length,
				writable: false, publiclyVisible: true);
		}

		return result;
	}

	public static void DecryptUserProfileData(Span<byte> fileMemorySpan)
	{
		if (!FileSizeIncludesUserProfileSettings(fileMemorySpan.Length))
		{
			throw new ArgumentException(
				$"Unexpected stream length {fileMemorySpan.Length}",
				nameof(fileMemorySpan));
		}

		// crypt starting after the always-unencrypted version number
		Span<byte> cryptedMemorySpan = fileMemorySpan.Slice(kTitleVersionSize, cFixedProfileSettingSize - kTitleVersionSize);
		int bytesLeft = cryptedMemorySpan.Length;

		// go from [0...cryptedMemorySpan.Length), ciphering on 64-bit blocks with TransformWithEightCycles
		const int bytesPerIteration = sizeof(ulong);
		while (bytesLeft > 0)
		{
			int cryptedMemoryPosition = cryptedMemorySpan.Length - bytesLeft;
			// if we're down to sub-block bytes remaining,
			int copySize = bytesLeft < bytesPerIteration
				? bytesLeft
				: bytesPerIteration;

			Span<byte> blockSpan = cryptedMemorySpan.Slice(cryptedMemoryPosition, copySize);
			ulong blockData = 0;
			// memcpy(&blockData, &blockSpan[0], copySize)
			// interpret the blockSpan bytes in LSB->MSB.
			// As this is only ever performed on little endian processors (HWDE, Xbox360 user profiles did not work this way)
			// CANNOT use BitConverter.ToUInt64, because you must give it exactly 8 bytes!
			for (int x = copySize-1; x >= 0; x--)
			{
				blockData <<= Bits.kByteBitCount;
				blockData |= blockSpan[x];
			}

			ulong cipheredBlockData = PhxTEA.TransformWithEightCycles(
				CryptographyTransformType.Decrypt, blockData,
				PhxTEA.GameFileKey0, PhxTEA.GameFileKey1);

			// memcpy(&blockSpan[0], &cipheredBlockData, copySize)
			// write the blockData back out in LSB->MSB
			// CANNOT use BitConverter.TryWriteBytes, because it expects the Span to be 8 bytes too!
			for (int x = 0; x < copySize; x++)
			{
				blockSpan[x] = (byte)cipheredBlockData;
				cipheredBlockData >>= Bits.kByteBitCount;
			}

			bytesLeft -= copySize;
		}
	}
};
