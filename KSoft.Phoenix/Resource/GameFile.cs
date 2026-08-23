using System;
using System.Security.Cryptography;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	using FileFlagsStreamer = IO.EnumBinaryStreamer<GameFile.FileFlags, ushort>;

	// BGameFile
	public sealed class GameFile
		: IDisposable
		, IO.IEndianStreamSerializable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains game-file format hash-seed reference data.")]
		const string kHashSeed = "TehHaloz";

		[Flags]
		internal enum FileFlags : ushort
		{
			CompressContent = 1<<0,
			EncryptContent = 1<<1,
			EncryptHeader = 1<<2,

			kAll = CompressContent | EncryptContent | EncryptHeader,
		};
		const ushort kVersion = 0x17;

		const int kMaxContentSize = 0xDD240;
		/// <summary>Number of random words that follow the content payload</summary>
		const int kRandomBlockWords = 0xA00;

		FileFlags Flags;
		internal SHA1? ShaContext { get; set; }

		public MediaHeader Header { get; private set; }

		public byte[]? Content { get; set; }
		byte[]? PaddingBytes { get; set; }

		public GameFile()
		{
			Flags = FileFlags.kAll;
			ShaContext = SHA1.Create();

			Header = new MediaHeader();
		}

		public void GenerateHash()
		{
			var shaContext = ShaContext ?? throw new ObjectDisposedException(nameof(GameFile));
			shaContext.Initialize();

			PhxHash.UInt16(shaContext, (ushort)Flags);
			PhxHash.UInt16(shaContext, kVersion);

			Header.UpdateHash(shaContext);
		}

		static uint WriteRandomBlock(IO.EndianWriter s, uint seed = 1)
		{
			for (int x = kRandomBlockWords; x > 0; x--)
			{
				uint r8 = seed << 17;
				uint r7 = r8 ^ seed;
				uint r6 = r7 >> 13;
				uint r5 = r6 ^ r7;
				uint r4 = r5 << 5;
				seed = r4 ^ r5;
				s.Write(seed);
			}

			return seed;
		}
		void WriteRandomBlocks(IO.EndianWriter s)
		{
			WriteRandomBlock(s);
		}

		static void Stream(IO.EndianStream s, bool crypt, IO.IEndianStreamSerializable obj,
			long size = 0, ulong userKey = 0, Action<IO.EndianStream>? streamLeftovers = null)
		{
			if (!crypt)
			{
				obj.Serialize(s);

				if (s.IsWriting && streamLeftovers != null)
				{
					streamLeftovers(s);
				}
			}
			else
			{
				using (var ms = new System.IO.MemoryStream())
				using (var crypted = new IO.EndianStream(ms, s.ByteOrder))
				{
					crypted.StreamMode = s.StreamMode;

					if (s.IsReading)
					{
						var tea = new Security.Cryptography.PhxTEA(s.Reader, crypted.Writer);
						tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyGameFile, userKey);
						tea.Decrypt(size);

						crypted.Seek(0);
					}

					obj.Serialize(crypted);

					streamLeftovers?.Invoke(crypted);

					if (s.IsWriting)
					{
						crypted.Seek(0);

						var tea = new Security.Cryptography.PhxTEA(crypted.Reader, s.Writer);
						tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyGameFile, userKey);
						tea.Encrypt(size);
					}
				}
			}
		}
		static void Read(IO.EndianReader s, bool decrypt, IO.IEndianStreamable obj,
			long size = 0, ulong userKey = 0, Action<IO.EndianReader>? readLeftovers = null)
		{
			if (!decrypt)
			{
				obj.Read(s);
			}
			else
			{
				using (var ms = new System.IO.MemoryStream())
				using (var sout = new IO.EndianWriter(ms, Shell.EndianFormat.Big))
				using (var decrypted = new IO.EndianReader(ms, Shell.EndianFormat.Big))
				{
					long position = s.BaseStream.Position;

					var tea = new Security.Cryptography.PhxTEA(s, sout);
					tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyGameFile, userKey);
					tea.Decrypt(size);

					decrypted.Seek(0);
					obj.Read(decrypted);

					readLeftovers?.Invoke(decrypted);
				}
			}
		}
		static void Write(IO.EndianWriter s, bool encrypt, IO.IEndianStreamable obj,
			long size = 0, ulong userKey = 0, Action<IO.EndianWriter>? writeLeftovers = null)
		{
			if (!encrypt)
			{
				obj.Write(s);

				writeLeftovers?.Invoke(s);
			}
			else
			{
				using (var ms = new System.IO.MemoryStream())
				using (var sin = new IO.EndianWriter(ms, Shell.EndianFormat.Big))
				using (var encrypted = new IO.EndianReader(ms, Shell.EndianFormat.Big))
				{
					obj.Write(sin);

					writeLeftovers?.Invoke(sin);

					encrypted.Seek(0);

					var tea = new Security.Cryptography.PhxTEA(encrypted, s);
					tea.InitializeKey(Security.Cryptography.PhxTEA.kKeyGameFile, userKey);
					tea.Encrypt(size);
				}
			}
		}

		public void Dispose()
		{
			ShaContext?.Dispose();
			ShaContext = null;
		}

		#region IEndianStreamSerializable Members
		void ReadLeftovers(IO.EndianReader er)
		{
			var paddingBytes = new byte[(int)(er.BaseStream.Length - er.BaseStream.Position)];
			PaddingBytes = paddingBytes;
			er.Read(paddingBytes, 0, paddingBytes.Length);
		}
		void WriteLeftovers(IO.EndianWriter ew)
		{
//			WriteRandomBlocks(ew);

			if (ew.BaseStream.Length < kMaxContentSize)
			{
				var paddingBytes = PaddingBytes ?? throw new InvalidOperationException("Padding bytes must be initialized before writing.");
				int padding_bytes_count = System.Math.Min(paddingBytes.Length, kMaxContentSize - (int)(ew.BaseStream.Length));
				ew.Write(paddingBytes, 0, padding_bytes_count);
			}

			if (ew.BaseStream.Length < kMaxContentSize)
			{
				byte[] zero = new byte[kMaxContentSize - (int)(ew.BaseStream.Length)];
				Array.Clear(zero, 0, zero.Length);
				ew.Write(zero, 0, zero.Length);
			}
		}
		void StreamLeftovers(IO.EndianStream s)
		{
				 if (s.IsReading) { ReadLeftovers(s.Reader); }
			else if (s.IsWriting) { WriteLeftovers(s.Writer); }
		}
		void StreamCompressedContent(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				using (var cs = new CompressedStream(true))
				{
					Stream(s, Flags.HasFlag(FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, streamLeftovers: StreamLeftovers);

					cs.Decompress();
					Content = cs.UncompressedData;
				}
			}
			else if (s.IsWriting)
			{
				using (var cs = new CompressedStream(true))
				using (var ms = new System.IO.MemoryStream(kMaxContentSize))
				using (var sout = new IO.EndianWriter(ms, s.ByteOrder))
				{
					var content = Content ?? throw new InvalidOperationException("Content must be initialized before writing.");
					sout.Write(content);
					sout.Seek(0);

					cs.InitializeFromStream(ms);
					cs.Compress();

					Stream(s, Flags.HasFlag(FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, streamLeftovers: StreamLeftovers);
				}
			}
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Owner = this;

			if (s.IsWriting)
			{
				//Flags = EnumFlags.Remove(Flags, FileFlags.EncryptHeader | FileFlags.EncryptContent);
			}

			// actually a union here:
			// BGameFileVersion
			//	uint32 mReserved : 13;
			//	uint32 mEncryptHeader : 1;
			//	uint32 mEncryptData : 1;
			//	uint32 mCompressData : 1;
			//	uint32 mVersion : 16;
			// This code was based on the Xbox360 layout, originally. Remember, that was PowerPC/BigEndian.
			// So, the bit packing was MSB first. Hence why FileFlags.CompressContent was first.
			// #REVIEW Fix this for HWDE (including GenerateHash). Basically:
			// byte Reserved : 8
			// byte Reserved : 5
			// mEncryptHeader, mEncryptData, mCompressData
			s.Stream(ref Flags, FileFlagsStreamer.Instance);
			s.StreamVersion(kVersion);

			Stream(s, Flags.HasFlag(FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (Flags.HasFlag(FileFlags.CompressContent))
			{
				StreamCompressedContent(s);
			}
			else
			{
				if (s.IsReading)
				{
					Content = new byte[(int)(s.BaseStream.Length - s.BaseStream.Position)];
				}

				// base layout:
				//		uint32 unused local checksum (always 0)
				//		bool multiplayer game
				//		int32 unused local player id (this is ALWAYS 1, it is no longer used)
				//		...BSettings
				//			...BGameSettings
				//			uint32 unused .scn file crc32 (always 0)
				//			...BConfigSettings
				// then any of the derived types:
				//		BSaveGame, BRecordGame
				var content = Content ?? throw new InvalidOperationException("Content must be initialized before writing.");
				s.Stream(content);
			}
		}
		#endregion

		#region IEndianStreamable Members
#if false // TODO: verify the new IEndianStreamSerializable impl
		public void Read(IO.EndianReader s)
		{
			s.Owner = this;

			Flags = s.Read(FileFlagsStreamer.Instance);
			Version = s.ReadUInt16();
			if (Version != kVersion) throw new IO.VersionMismatchException(s.BaseStream,
				kVersion, Version);

			Read(s, Flags.HasFlag(FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (Flags.HasFlag(FileFlags.CompressContent))
			{
				using (var cs = new CompressedStream(true))
				{
					Read(s, Flags.HasFlag(FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, readLeftovers: ReadLeftovers);

					cs.Decompress();
					Content = cs.UncompressedData;
				}
			}
			else
				Content = s.ReadBytes((int)(s.BaseStream.Length - s.BaseStream.Position));
		}
		public void Write(IO.EndianWriter s)
		{
			//Flags = EnumFlags.Remove(Flags, FileFlags.EncryptHeader | FileFlags.EncryptContent);

			s.Write(Flags, FileFlagsStreamer.Instance);
			s.Write((ushort)kVersion);

			Write(s, Flags.HasFlag(FileFlags.EncryptHeader), Header, MediaHeader.kSizeOf);
			GenerateHash();

			if (Flags.HasFlag(FileFlags.CompressContent))
			{
				using (var cs = new CompressedStream(true))
				using (var ms = new System.IO.MemoryStream(kMaxContentSize))
				using (var sout = new IO.EndianWriter(ms, Shell.EndianFormat.Big))
				{
					sout.Write(Content);
					sout.Seek(0);

					cs.InitializeFromStream(ms);
					cs.Compress();

					Write(s, Flags.HasFlag(FileFlags.EncryptContent), cs,
						userKey: Header.DataCryptKey, writeLeftovers: WriteLeftovers);
				}
			}
			else
				s.Write(Content);
		}
#endif
		#endregion
	};
}
