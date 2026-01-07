
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
			=> type switch
			{
				Phx.BProtoObjectCommandType.Research or
				Phx.BProtoObjectCommandType.TrainUnit or
				Phx.BProtoObjectCommandType.Build or
				Phx.BProtoObjectCommandType.BuildOther or
				Phx.BProtoObjectCommandType.TrainSquad or
				Phx.BProtoObjectCommandType.Ability or
				Phx.BProtoObjectCommandType.Power
				=> true,
				_ => false,
			};

		public static Phx.DatabaseObjectKind GetIdKind(this Phx.BProtoObjectCommandType type)
			=> type switch
			{
				Phx.BProtoObjectCommandType.Research => Phx.DatabaseObjectKind.Tech,
				Phx.BProtoObjectCommandType.TrainUnit or
				Phx.BProtoObjectCommandType.Build or
				Phx.BProtoObjectCommandType.BuildOther => Phx.DatabaseObjectKind.Object,
				Phx.BProtoObjectCommandType.TrainSquad => Phx.DatabaseObjectKind.Squad,
				Phx.BProtoObjectCommandType.Ability => Phx.DatabaseObjectKind.Ability,
				Phx.BProtoObjectCommandType.Power => Phx.DatabaseObjectKind.Power,
				_ => Phx.DatabaseObjectKind.None,
			};
	};
}