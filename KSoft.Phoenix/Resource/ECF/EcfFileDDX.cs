
namespace KSoft.Phoenix.Resource.ECF
{
	/*public*/ sealed class EcfFileDDX
		: EcfFile
	{
		const uint kSignature = 0x13CF5D01;
		const ulong kChunkIdHeader = 0x1D8828C6ECAF45F2;
		// EcfFile
		const ulong kChunkIdMip0 = 0x3F74B8E87D2B44BF;
		const ulong kChunkIdMipChain = 0x46F1FD3F394348B8;
		//ResourceTagHeader.kChunkId

		const uint kChunkIdHeaderSignature = 0xDDBB7738;
		const uint kChunkIdMip0Signature = 0x1234997E;
	};
}