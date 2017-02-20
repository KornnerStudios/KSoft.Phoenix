using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource
{
	public enum ResourceTagPlatformId : byte
	{
		Any,
		Pc,
		Xbox,
	};

	public sealed class ResourceTagHeader
		: IO.IEndianStreamSerializable
	{
		public const ulong kChunkId = 0x00000000714BFE00;

		const ushort kSignature = 0x714C;
		const byte kMajorVersion = 0x11;
		const byte kMinorVersion = 0x0;

		public Shell.ProcessorSize CreatorPointerSize { get; private set; }

		public ushort HeaderSize;
		public ushort DataSize;
		public uint HeaderAdler32;

		public ulong TagTimeStamp; // FILETIME
		public Values.KGuid TagGuid;
		Values.PtrHandle TagMachineNameOffset;
		Values.PtrHandle TagUserNameOffset;

		Values.PtrHandle SourceFileName;
		public byte[] SourceDigest = new byte[Security.Cryptography.PhxHash.kSha1SizeOf];
		public ulong SourceFileSize;
		public ulong SourceFileTimeStamp;

		Values.PtrHandle CreatorToolCommandLine;
		public byte CreatorToolVersion;

		private byte mPlatformId;
		public ResourceTagPlatformId PlatformId
		{
			get { return (ResourceTagPlatformId)mPlatformId; }
			set { mPlatformId = (byte)value; }
		}

		public ResourceTagHeader(Shell.ProcessorSize pointerSize = Shell.ProcessorSize.x32)
		{
			if (pointerSize == Shell.ProcessorSize.x32)
			{
				TagMachineNameOffset = Values.PtrHandle.InvalidHandle32;
				TagUserNameOffset = Values.PtrHandle.InvalidHandle32;
				CreatorToolCommandLine = Values.PtrHandle.InvalidHandle32;
			}
			else
			{
				TagMachineNameOffset = Values.PtrHandle.InvalidHandle64;
				TagUserNameOffset = Values.PtrHandle.InvalidHandle64;
				CreatorToolCommandLine = Values.PtrHandle.InvalidHandle64;
			}
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			using (s.BeginEndianSwitch(Shell.EndianFormat.Little))
			{
				s.VirtualAddressTranslationInitialize(CreatorPointerSize);
				s.VirtualAddressTranslationPushPosition();
				SerializeBody(s);
				s.VirtualAddressTranslationPop();
			}
		}

		private void SerializeBody(IO.EndianStream s)
		{
			s.StreamSignature(kSignature);
			s.StreamVersion(kMajorVersion);
			s.StreamVersion(kMinorVersion);

			s.Stream(ref HeaderSize);
			s.Stream(ref DataSize);
			s.Stream(ref HeaderAdler32);

			s.Stream(ref TagTimeStamp);
			s.Stream(ref TagGuid);

			s.StreamVirtualAddress(ref TagMachineNameOffset);
			s.StreamVirtualAddress(ref TagUserNameOffset);

			s.StreamVirtualAddress(ref SourceFileName);
			s.Stream(SourceDigest);
			s.Stream(ref SourceFileSize);
			s.Stream(ref SourceFileTimeStamp);

			s.StreamVirtualAddress(ref CreatorToolCommandLine);
			s.Stream(ref CreatorToolVersion);

			s.Stream(ref mPlatformId);
			s.Pad(sizeof(byte) + sizeof(uint));
		}

		public bool StreamTagMachineName(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref TagMachineNameOffset, ref value);
		}

		public bool StreamTagUserName(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref TagUserNameOffset, ref value);
		}

		public bool StreamSourceFileNamee(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref SourceFileName, ref value);
		}

		public bool StreamCreatorToolCommandLine(IO.EndianStream s, ref string value)
		{
			return PhxUtil.StreamPointerizedCString(s, ref CreatorToolCommandLine, ref value);
		}
		#endregion
	};
}