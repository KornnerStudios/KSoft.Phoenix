using System.Diagnostics;
using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public sealed class EcfFileChunkDefinition
		: IO.ITagElementStringNameStreamable
	{
		public const string kFileExtension = ".ecf_chunk";

		internal int RawChunkIndex = TypeExtensions.kNone;

		public EcfFileDefinition Parent { get; private set; }

		public ulong Id { get; private set; }
		public byte AlignmentBit { get; private set; }
			= EcfChunk.kDefaultAlignmentBit;
		/// <summary>Compression to use when baking into an ECF stream</summary>
		public EcfCompressionType CompressionType { get; private set; }

		/// <summary>Should be relative to the Parent's BasePath</summary>
		public string FilePath { get; private set; }
		public byte[] FileBytes { get; private set; }

		public bool HasPossibleFileData => FilePath.IsNotNullOrEmpty() || FileBytes.IsNotNullOrEmpty();

		#region ResourceFlags
		private uint mResourceFlags;

		public bool IsContiguous
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.Contiguous); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.Contiguous); }
		}

		public bool IsWriteCombined
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.WriteCombined); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.WriteCombined); }
		}

		public bool IsDeflateStream
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.IsDeflateStream); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.IsDeflateStream); }
		}

		public bool IsResourceTag
		{
			get { return Bitwise.Flags.Test(mResourceFlags, 1U<<(ushort)EcfChunkResourceFlags.IsResourceTag); }
			set { Bitwise.Flags.Modify(value, ref mResourceFlags, (ushort)1U<<(ushort)EcfChunkResourceFlags.IsResourceTag); }
		}
		#endregion

		public void Initialize(EcfFileDefinition parent, EcfChunk rawChunk, int rawChunkIndex)
		{
			RawChunkIndex = rawChunkIndex;

			Parent = parent;
			Id = rawChunk.EntryId;
			AlignmentBit = rawChunk.DataAlignmentBit;
			CompressionType = rawChunk.CompressionType;
			IsContiguous = rawChunk.IsContiguous;
			IsWriteCombined = rawChunk.IsWriteCombined;
			IsDeflateStream = rawChunk.IsDeflateStream;
			IsResourceTag = rawChunk.IsResourceTag;
		}

		internal void SetupRawChunk(EcfChunk rawChunk, int rawChunkIndex)
		{
			RawChunkIndex = rawChunkIndex;

			rawChunk.EntryId = Id;
			rawChunk.DataAlignmentBit = AlignmentBit;
			rawChunk.CompressionType = CompressionType;
			rawChunk.IsContiguous = IsContiguous;
			rawChunk.IsWriteCombined = IsWriteCombined;
			rawChunk.IsDeflateStream = IsDeflateStream;
			rawChunk.IsResourceTag = IsResourceTag;
		}

		public void SetFilePathFromParentNameAndId()
		{
			bool requiresUniqueChunkFileName = Parent.HasMultipleChunksWithSameId(Id);

			string relativeFilePath;
			if (requiresUniqueChunkFileName)
			{
				// e.g., blood_gulch.xtt_00002222_00000002.ecf_chunk
				// XTT can contain >1 TerrainAtlasLinkChunk entries.
				// Other ECF files may have similar situations, this is to split them out into separate files.
				relativeFilePath = $"{Parent.EcfName}_{Id:X8}_{RawChunkIndex:X8}{kFileExtension}";
			}
			else
			{
				// e.g., blood_gulch.xtt_00001111.ecf_chunk
				relativeFilePath = $"{Parent.EcfName}_{Id:X8}{kFileExtension}";
			}

			FilePath = relativeFilePath;
		}

		internal void SetFileBytes(byte[] bytes)
		{
			FileBytes = bytes;
		}

		#region ITagElementStringNameStreamable
		// #NOTE the attributes here should match the ones used in EcfChunk's code

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			KSoft.Debug.TypeCheck.TryCastReference(s.Owner, out EcfFileExpander ecf_expander);

			if (s.IsReading)
			{
				Parent = KSoft.Debug.TypeCheck.CastReference<EcfFileDefinition>(s.UserData);
			}

			s.StreamAttribute("id", this, obj => Id, NumeralBase.Hex);
			s.StreamAttributeOpt("align", this, obj => AlignmentBit, b => b != EcfChunk.kDefaultAlignmentBit, NumeralBase.Hex);

			if (s.StreamAttributeEnumOpt("Compression", this, obj => CompressionType, e => e != EcfCompressionType.Stored))
			{
				// #NOTE DeflateRaw requires the decompressed size to be known somewhere, and generic ECF files do not store such info
				// Only available in ERAs
				if (CompressionType == EcfCompressionType.DeflateRaw)
				{
					s.ThrowReadException(new InvalidDataException(CompressionType + " is not supported in this context"));
				}
			}

			if (s.IsReading)
			{
				ReadResourceFlags(s);
			}
			else if (s.IsWriting)
			{
				WriteResourceFlags(s);
			}

			s.StreamAttributeOpt("Path", this, obj => FilePath, Predicates.IsNotNullOrEmpty);

			// Don't try to write the file bytes
			bool try_to_serialize_file_bytes = s.IsReading || FilePath.IsNullOrEmpty();
			if (ecf_expander != null)
			{
				if (!ecf_expander.ExpanderOptions.Test(EcfFileExpanderOptions.DontSaveChunksToFiles))
				{
					try_to_serialize_file_bytes = true;
				}
			}

			if (try_to_serialize_file_bytes)
			{
				if (!s.StreamCursorBytesOpt(this, obj => FileBytes))
				{
					if (FilePath.IsNullOrEmpty())
					{
						s.ThrowReadException(new InvalidDataException("Expect Path attribute or file hex bytes"));
					}
				}
			}
		}

		private void ReadResourceFlags<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool flag = false;
			if (s.ReadAttributeOpt("IsContiguous", ref flag))
			{
				IsContiguous = flag;
			}
			if (s.ReadAttributeOpt("IsWriteCombined", ref flag))
			{
				IsWriteCombined = flag;
			}
			if (s.ReadAttributeOpt("IsDeflateStream", ref flag))
			{
				IsDeflateStream = flag;
			}
			if (s.ReadAttributeOpt("IsResourceTag", ref flag))
			{
				IsResourceTag = flag;
			}
		}
		private void WriteResourceFlags<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (mResourceFlags == 0)
			{
				return;
			}

			if (IsContiguous)
			{
				s.WriteAttribute("IsContiguous", true);
			}
			if (IsWriteCombined)
			{
				s.WriteAttribute("IsWriteCombined", true);
			}
			if (IsDeflateStream)
			{
				s.WriteAttribute("IsDeflateStream", true);
			}
			if (IsResourceTag)
			{
				s.WriteAttribute("IsResourceTag", true);
			}
		}
		#endregion

		private string GetDebuggerDisplay()
			=> $"{RawChunkIndex} {Id:X16} {FilePath}";
	};
}
