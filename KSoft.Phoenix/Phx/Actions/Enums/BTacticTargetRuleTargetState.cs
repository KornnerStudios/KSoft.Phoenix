using System;

namespace KSoft.Phoenix.Phx
{
	[Flags]
	public enum BTacticTargetRuleTargetState
	{
		// 0x28
		//
		GaiaOwned = 1<<1,
		//
		//
		Capturable = 1<<4,
		Damaged = 1<<5,
		Unbuilt = 1<<6,
	};
}