using System;
using System.Collections.Generic;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.ECF
{
	public sealed class EcfFileDefinition
		: IO.ITagElementStringNameStreamable
	{
		public const string kFileExtension = ".ecfdef";

		public string WorkingDirectory { get; set; }

		/// <summary>This should be the source file's name (without extension) or a user defined name</summary>
		public string EcfName { get; private set; }
		public string EcfFileExtension { get; private set; }
		public uint HeaderId { get; private set; }
		public uint ChunkExtraDataSize { get; private set; }

		public List<EcfFileChunkDefinition> Chunks { get; private set; }
			= new List<EcfFileChunkDefinition>();

		#region ITagElementStringNameStreamable
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterUserDataBookmark(this))
			{
				s.StreamAttributeOpt("name", this, obj => EcfName, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("ext", this, obj => EcfFileExtension, Predicates.IsNotNullOrEmpty);

				using (s.EnterCursorBookmark("Header"))
				{
					s.StreamAttribute("id", this, obj => HeaderId, NumeralBase.Hex);
					s.StreamAttributeOpt("ChunkExtraDataSize", this, obj => ChunkExtraDataSize, Predicates.IsNotZero, NumeralBase.Hex);
				}

				using (var bm = s.EnterCursorBookmarkOpt("Chunks", Chunks, Predicates.HasItems))
					s.StreamableElements("C", Chunks, obj => obj.HasPossibleFileData);
			}

			// #NOTE leaving this as an exercise for the caller instead, so they can yell when something is culled
			#if false
			if (s.IsReading)
			{
				CullChunksPossiblyWithoutFileData();
			}
			#endif
		}

		internal void CullChunksPossiblyWithoutFileData(
			Action<int, EcfFileChunkDefinition> cullCallback = null)
		{
			for (int x = Chunks.Count - 1; x >= 0; x--)
			{
				var chunk = Chunks[x];
				if (!chunk.HasPossibleFileData)
				{
					if (cullCallback != null)
					{
						cullCallback(x, chunk);
					}

					Chunks.RemoveAt(x);
					continue;
				}
			}
		}
		#endregion

		public string GetChunkAbsolutePath(EcfFileChunkDefinition chunk)
		{
			Contract.Requires(chunk != null && chunk.Parent == this);

			Contract.Assert(WorkingDirectory.IsNotNullOrEmpty());
			string abs_path = Path.Combine(WorkingDirectory, chunk.FilePath);

			abs_path = Path.GetFullPath(abs_path);
			return abs_path;
		}

		public void Initialize(string ecfFileName)
		{
			EcfName = ecfFileName;
			if (EcfName.IsNotNullOrEmpty())
			{
				EcfFileExtension = Path.GetExtension(EcfName);
				// We don't use GetFileNameWithoutExtension because there are cases where
				// files only differ by their extensions (like Terrain data XTD, XSD, etc)
				EcfName = Path.GetFileName(EcfName);
			}
		}

		public void CopyHeaderData(EcfHeader header)
		{
			HeaderId = header.Id;
			ChunkExtraDataSize = header.ExtraDataSize;
		}

		public void UpdateHeader(ref EcfHeader header)
		{
			Contract.Requires<InvalidOperationException>(HeaderId != 0);

			header.InitializeChunkInfo(HeaderId, ChunkExtraDataSize);
		}

		public EcfFileChunkDefinition Add(EcfChunk rawChunk, int rawChunkIndex)
		{
			Contract.Requires(rawChunk != null);

			var chunk = new EcfFileChunkDefinition();
			chunk.Initialize(this, rawChunk, rawChunkIndex);
			Chunks.Add(chunk);

			return chunk;
		}

		internal MemoryStream GetChunkFileDataStream(EcfFileChunkDefinition chunk)
		{
			Contract.Assume(chunk != null && chunk.Parent == this);

			MemoryStream ms;

			if (chunk.FileBytes != null)
			{
				ms = new MemoryStream(chunk.FileBytes, writable: false);
			}
			else
			{
				var source_file = GetChunkAbsolutePath(chunk);
				using (var fs = File.OpenRead(source_file))
				{
					ms = new MemoryStream((int)fs.Length);
					fs.CopyTo(ms);
				}
			}

			ms.Position = 0;
			return ms;
		}
	};
}