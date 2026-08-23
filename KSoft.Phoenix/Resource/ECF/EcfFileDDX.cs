
namespace KSoft.Phoenix.Resource.ECF
{
	/// <summary>
	/// This only matters to the Xbox360 version, PC uses actual dds files, just renamed to ddx
	/// </summary>
	public sealed class EcfFileDDX
		: EcfFile
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains ECF format signature reference data.")]
		const uint kSignature = 0x13CF5D01;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains ECF format signature reference data.")]
		const ulong kChunkIdHeader = 0x1D8828C6ECAF45F2;
		// EcfFile
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains ECF format signature reference data.")]
		const ulong kChunkIdMip0 = 0x3F74B8E87D2B44BF;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains ECF format signature reference data.")]
		const ulong kChunkIdMipChain = 0x46F1FD3F394348B8;
		//ResourceTagHeader.kChunkId

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains ECF format signature reference data.")]
		const uint kChunkIdHeaderSignature = 0xDDBB7738;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains ECF format signature reference data.")]
		const uint kChunkIdMip0Signature = 0x1234997E;
	};
}
