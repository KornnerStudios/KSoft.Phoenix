using Contracts = System.Diagnostics.Contracts;

using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix
{
	static class PhxPredicates
	{
		[Contracts.Pure] public static bool IsNotInvalid(float x)		{ return x > PhxUtil.kInvalidSingle; }
		[Contracts.Pure] public static bool IsNotInvalidNaN(float x)	{ return !float.IsNaN(x); }

		public static bool IsNotOne(float x) { return x != 1.0f; }
		public static bool IsNotOne(uint x) { return x != 1; }
		public static bool IsNotOne(int x) { return x != 1; }

		public static bool IsZero(BVector vector)
		{
			return vector.X == 0
				&& vector.Y == 0
				&& vector.Z == 0
				&& vector.W == 0;
		}
		public static bool IsNotZero(BVector vector)
		{
			return vector.X != 0
				|| vector.Y != 0
				|| vector.Z != 0
				|| vector.W != 0;
		}

		public static bool IsZero(System.Drawing.Color color)
		{
			return color.A == 0
				&& color.R == 0
				&& color.G == 0
				&& color.B == 0;
		}
		public static bool IsNotZero(System.Drawing.Color color)
		{
			return color.A != 0
				|| color.R != 0
				|| color.G != 0
				|| color.B != 0;
		}

		public static bool IsRgbZero(System.Drawing.Color color)
		{
			return color.R == 0
				&& color.G == 0
				&& color.B == 0;
		}
		public static bool IsRgbNotZero(System.Drawing.Color color)
		{
			return color.R != 0
				|| color.G != 0
				|| color.B != 0;
		}
	};
}