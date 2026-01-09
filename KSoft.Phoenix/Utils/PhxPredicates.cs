using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix
{
	static class PhxPredicates
	{
		[Contracts.Pure] public static bool IsNotInvalid(float x)		=> x > PhxUtil.kInvalidSingle;
		[Contracts.Pure] public static bool IsNotInvalidNaN(float x)	=> !float.IsNaN(x);

		public static bool IsNotOne(float x)	=> x != 1.0f;
		public static bool IsNotOne(uint x)		=> x != 1;
		public static bool IsNotOne(int x)		=> x != 1;

		public static bool IsZero(BVector vector)
			=> vector.X == 0
				&& vector.Y == 0
				&& vector.Z == 0
				&& vector.W == 0;
		public static bool IsNotZero(BVector vector)
			=> vector.X != 0
				|| vector.Y != 0
				|| vector.Z != 0
				|| vector.W != 0;

		public static bool IsZero(System.Drawing.Color color)
			=> color.A == 0
				&& color.R == 0
				&& color.G == 0
				&& color.B == 0;
		public static bool IsNotZero(System.Drawing.Color color)
			=> color.A != 0
				|| color.R != 0
				|| color.G != 0
				|| color.B != 0;

		public static bool IsRgbZero(System.Drawing.Color color)
			=> color.R == 0
				&& color.G == 0
				&& color.B == 0;
		public static bool IsRgbNotZero(System.Drawing.Color color)
			=> color.R != 0
				|| color.G != 0
				|| color.B != 0;
	};
}