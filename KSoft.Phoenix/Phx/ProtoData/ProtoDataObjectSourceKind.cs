using System;

namespace KSoft.Phoenix.Phx
{
	public enum ProtoDataObjectSourceKind
	{
		None = PhxUtil.kObjectKindNone,

		Database,
		GameData,
		HPData,

		Scenario,
		TacticData,
		TriggerScript,
		Visual,
	};

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class ProtoDataTypeObjectSourceKindAttribute
		: Attribute
	{
		public ProtoDataObjectSourceKind SourceKind { get; private set; }

		public ProtoDataTypeObjectSourceKindAttribute(ProtoDataObjectSourceKind kind)
		{
			SourceKind = kind;
		}
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		public static bool RequiresFileReference(this Phx.ProtoDataObjectSourceKind kind)
		{
			return kind switch
			{
				Phx.ProtoDataObjectSourceKind.TacticData or
				Phx.ProtoDataObjectSourceKind.Visual or
				Phx.ProtoDataObjectSourceKind.TriggerScript or
				Phx.ProtoDataObjectSourceKind.Scenario
				=> true,
				_ => false,
			};
		}
	};
}