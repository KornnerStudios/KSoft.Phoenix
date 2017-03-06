﻿using System;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	public sealed class ResourceTag
	{
		public DateTime TimeStamp { get; set; }
		public Values.KGuid Guid { get; set; }
		public string MachineName { get; set; }
		public string UserName { get; set; }

		public string SourceFileName { get; private set; }
		public byte[] SourceDigest { get; private set; }
		public long SourceFileSize { get; private set; }
		public DateTime SourceFileTimeStamp { get; private set; }

		public int CreatorToolVersion { get; set; }
		public string CreatorToolCommandLine { get; set; }

		public ResourceTagPlatformId PlatformId { get; set; }

		public ResourceTag()
		{
			TimeStamp = DateTime.MinValue;
			SourceDigest = new byte[Security.Cryptography.PhxHash.kSha1SizeOf];
		}

		public bool SetSourceFile(string fileName)
		{
			try
			{
				if (!File.Exists(fileName))
					return false;

				var fileInfo = new FileInfo(fileName);
				var fileSize = fileInfo.Length;
				var createTime = fileInfo.CreationTimeUtc;
				var lastWriteTime = fileInfo.LastWriteTimeUtc;

				SourceFileName = fileName;
				Array.Clear(SourceDigest, 0, SourceDigest.Length);
				SourceFileSize = fileSize;
				SourceFileTimeStamp = lastWriteTime > createTime
					? lastWriteTime
					: createTime;
			}
			catch (Exception ex)
			{
				Debug.Trace.Resource.TraceInformation(ex.ToString());
				return false;
			}

			return true;
		}

		public bool ComputeSourceFileDigest()
		{
			bool result = false;
			try
			{
				long fileLength;
				result = Security.Cryptography.PhxHash.Sha1HashFile(SourceFileName, SourceDigest, out fileLength);

				if (result)
				{
					SourceFileSize = fileLength;
				}
			}
			catch (Exception ex)
			{
				Debug.Trace.Resource.TraceInformation(ex.ToString());
				return false;
			}
			return result;
		}

		public void SetCreatorToolInfo(int version, string cmdLine)
		{
			Contract.Requires(version >= 0 && version <= byte.MaxValue);

			CreatorToolVersion = version;
			CreatorToolCommandLine = cmdLine;
		}

		public void ComputeMetadata()
		{
			if (Guid.IsNotEmpty)
				return;

			TimeStamp = System.DateTime.UtcNow;
			Guid = Values.KGuid.NewGuid();
			MachineName = Environment.MachineName;
			UserName = Environment.UserName;
		}

		public void PopulateFromStream(IO.EndianStream s, ResourceTagHeader header)
		{
			Contract.Requires(s != null);
			Contract.Requires(s.IsReading);
			Contract.Requires(header != null);

			string streamedString = null;

			this.TimeStamp = DateTime.FromFileTimeUtc((long)header.TagTimeStamp);
			this.Guid = header.TagGuid;
			if (header.StreamTagMachineName(s, ref streamedString))
				this.MachineName = streamedString;
			if (header.StreamTagUserName(s, ref streamedString))
				this.UserName = streamedString;

			if (header.StreamSourceFileNamee(s, ref streamedString))
				this.SourceFileName = streamedString;
			Array.Copy(header.SourceDigest, this.SourceDigest, header.SourceDigest.Length);
			this.SourceFileSize = (long)header.SourceFileSize;
			this.SourceFileTimeStamp = DateTime.FromFileTimeUtc((long)header.SourceFileTimeStamp);

			if (header.StreamCreatorToolCommandLine(s, ref streamedString))
				this.CreatorToolCommandLine = streamedString;
			this.CreatorToolVersion = header.CreatorToolVersion;

			this.PlatformId = header.PlatformId;
		}
	};
}