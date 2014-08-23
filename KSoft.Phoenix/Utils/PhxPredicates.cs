using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix
{
	static class PhxPredicates
	{
		[Contracts.Pure] public static bool IsNotInvalid(float x)		{ return x > PhxUtil.kInvalidSingle; }
		[Contracts.Pure] public static bool IsNotInvalidNaN(float x)	{ return !float.IsNaN(x); }
	};
}