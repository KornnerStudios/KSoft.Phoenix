
namespace KSoft.Phoenix.Phx
{
	public enum BProtoObjectCommandType
	{
		Invalid = TypeExtensions.kNone,

		Research = 0,
		TrainUnit,
		Build,
		TrainSquad,
		Unload,
		Reinforce,
		ChangeMode,
		Ability,
		Kill,
		CancelKill,
		Tribute,
		CustomCommand,
		Power,
		BuildOther,
		TrainLock,
		TrainUnlock,
		RallyPoint,
		ClearRallyPoint,
		DestroyBase,
		CancelDestroyBase,
		ReverseHotDrop,
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		public static bool RequiresValidId(this Phx.BProtoObjectCommandType type)
		{
			switch(type)
			{
				case Phx.BProtoObjectCommandType.Research:
				case Phx.BProtoObjectCommandType.TrainUnit:
				case Phx.BProtoObjectCommandType.Build:
				case Phx.BProtoObjectCommandType.BuildOther:
				case Phx.BProtoObjectCommandType.TrainSquad:
				case Phx.BProtoObjectCommandType.Ability:
				case Phx.BProtoObjectCommandType.Power:
					return true;

				default:
					return false;
			}
		}

		public static Phx.DatabaseObjectKind GetIdKind(this Phx.BProtoObjectCommandType type)
		{
			switch (type)
			{
				case Phx.BProtoObjectCommandType.Research:
					return Phx.DatabaseObjectKind.Tech;

				case Phx.BProtoObjectCommandType.TrainUnit:
				case Phx.BProtoObjectCommandType.Build:
				case Phx.BProtoObjectCommandType.BuildOther:
					return Phx.DatabaseObjectKind.Object;

				case Phx.BProtoObjectCommandType.TrainSquad:
					return Phx.DatabaseObjectKind.Squad;

				case Phx.BProtoObjectCommandType.Ability:
					return Phx.DatabaseObjectKind.Ability;

				case Phx.BProtoObjectCommandType.Power:
					return Phx.DatabaseObjectKind.Power;

				default:
					return Phx.DatabaseObjectKind.None;
			}
		}
	};
}