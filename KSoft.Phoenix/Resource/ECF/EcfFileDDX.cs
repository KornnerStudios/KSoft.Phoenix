
namespace KSoft.Phoenix.Resource.ECF
{
	/*public*/ sealed class EcfFileDDX
		: EcfFile
	{
		const uint kSignature = 0x13CF5D01;
		const ulong kChunkIdTextureInfo = 0x1D8828C6ECAF45F2;
		// EcfFile
		const ulong kChunkId1 = 0x3F74B8E87D2B44BF;
		const ulong kChunkId2 = 0x93F8593B8A8493EF;
		const ulong kChunkIdBuildParams = 0x00000000714BFE00;

		const uint kChunkIdTextureInfoSignature = 0xDDBB7738;
		const uint kChunkId1Signature = 0x1234997E;
		const uint kBuildParamsSignature = 0x4C711100;
	};
}