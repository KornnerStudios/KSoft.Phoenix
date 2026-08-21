using System;
using System.Collections.Generic;
using System.Linq;

namespace KSoft.Phoenix.Phx
{
	public interface IProtoDataObjectDatabaseProvider
	{
		Engine.XmlFileInfo SourceFileReference { get; }
		Collections.IBTypeNames GetNamesInterface(int objectKind);
		Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(int objectKind);
	};

	public class ProtoDataObjectDatabase
		: IEquatable<ProtoDataObjectDatabase>
	{
		public ProtoDataObjectSourceKind ObjectSourceKind { get; private set; }
		public IProtoDataObjectDatabaseProvider Provider { get; private set; }
		public Type ObjectKindEnum { get; private set; }
		public List<object> ObjectKinds { get; private set; } = new();
		public List<int> ObjectKindIds { get; private set; } = new();

		public ProtoDataObjectDatabase(IProtoDataObjectDatabaseProvider provider, Type objectKindEnum)
		{
			Provider = provider;
			ObjectKindEnum = objectKindEnum;

			foreach (object e in Enum.GetValues(ObjectKindEnum))
			{
				ObjectKinds.Add(e);
			}

			ObjectKindIds.AddRange(ObjectKinds.Cast<int>());

			ObjectSourceKind = GetSourceKind();
		}

		private ProtoDataObjectSourceKind GetSourceKind()
		{
			var kind = ProtoDataObjectSourceKind.None;

			var providerType = Provider.GetType();
			var kindAttr = providerType.GetCustomAttribute<ProtoDataTypeObjectSourceKindAttribute>(inherited: true);

			if (kindAttr != null)
			{
				kind = kindAttr.SourceKind;
			}
			else
			{
				throw new InvalidOperationException(string.Format(
					"Provider type {0} does not have a ProtoDataTypeObjectSourceKindAttribute.",
					providerType.FullName));
			}

			return kind;
		}

		public bool Equals(ProtoDataObjectDatabase other)
			=> other != null
				&& ObjectSourceKind == other.ObjectSourceKind
				&& Provider == other.Provider;

		public override bool Equals(object obj)
			=> obj is ProtoDataObjectDatabase database && Equals(database);

		public override int GetHashCode()
			=> HashCode.Combine(ObjectSourceKind, Provider);
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		public static int GetId(this Phx.IProtoDataObjectDatabaseProvider provider
			, int objectKind, string name)
		{
			ArgumentNullException.ThrowIfNull(provider);
			if (objectKind == PhxUtil.kObjectKindNone)
			{
				throw new ArgumentOutOfRangeException(nameof(objectKind));
			}

			var dbi = provider.GetNamesInterface(objectKind);
			return dbi.TryGetIdWithUndefined(name);
		}

		public static string GetName(this Phx.IProtoDataObjectDatabaseProvider provider
			, int objectKind, int id)
		{
			ArgumentNullException.ThrowIfNull(provider);
			if (objectKind == PhxUtil.kObjectKindNone)
			{
				throw new ArgumentOutOfRangeException(nameof(objectKind));
			}

			var dbi = provider.GetMembersInterface(objectKind);
			return dbi.TryGetNameWithUndefined(id);
		}

		public static object GetObject(this Phx.IProtoDataObjectDatabaseProvider provider
			, int objectKind, int id)
		{
			ArgumentNullException.ThrowIfNull(provider);
			if (objectKind == PhxUtil.kObjectKindNone)
			{
				throw new ArgumentOutOfRangeException(nameof(objectKind));
			}

			var dbi = provider.GetNamesInterface(objectKind);
			return dbi.GetObject(id);
		}
	};
}