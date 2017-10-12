
namespace KSoft.DDS
{
	[System.Flags]
	public enum DDS_FLAGS
	{
		/// <summary>
		/// Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)
		/// </summary>
		LegacyDWORD = 1<<0,

		/// <summary>
		/// Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8)
		/// </summary>
		NoLegacyExpansion = 1<<1,

		/// <summary>
		/// Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks
		/// </summary>
		NoFixupForR10B10G10A2 = 1<<2,

		/// <summary>
		/// Convert DXGI 1.1 BGR formats to DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats
		/// </summary>
		ForceRgb = 1<<3,

		/// <summary>
		/// Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats
		/// </summary>
		No16Bpp = 1<<4,

		/// <summary>
		/// When loading legacy luminance formats expand replicating the color channels rather than leaving them packed (L8, L16, A8L8)
		/// </summary>
		ExpandLuminance = 1<<5,

		/// <summary>
		/// Some older DXTn DDS files incorrectly handle mipchain tails for blocks smaller than 4x4
		/// </summary>
		BadDxtnTails = 1<<6,

		/// <summary>
		/// Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)
		/// </summary>
		ForceDx10 = 1<<16,

		/// <summary>
		/// <see cref="ForceDx10"/> including miscFlags2 information (result may not be compatible with D3DX10 or D3DX11)
		/// </summary>
		ForceDx10Misc2,
	};
}