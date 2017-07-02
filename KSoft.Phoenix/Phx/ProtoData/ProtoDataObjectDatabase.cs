using System;
using System.Collections.Generic;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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
		public List<object> ObjectKinds { get; private set; }
			= new List<object>();
		public List<int> ObjectKindIds { get; private set; }
			= new List<int>();

		public ProtoDataObjectDatabase(IProtoDataObjectDatabaseProvider provider, Type objectKindEnum)
		{
			Provider = provider;
			ObjectKindEnum = objectKindEnum;

			foreach (object e in Enum.GetValues(ObjectKindEnum))
				ObjectKinds.Add(e);

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
				Contract.Assert(false, "Provider's Type doesn't have a ProtoDataTypeObjectSourceKindAttribute");
			}

			return kind;
		}

		public bool Equals(ProtoDataObjectDatabase other)
		{
			return ObjectSourceKind == other.ObjectSourceKind
				&& Provider == other.Provider;
		}

		public override bool Equals(object obj)
		{
			return obj is ProtoDataObjectDatabase && Equals((ProtoDataObjectDatabase)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash *= 23 + ObjectSourceKind.GetHashCode();
				hash *= 23 + Provider.GetHashCode();
				return hash;
			}
		}
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		public static int GetId(this Phx.IProtoDataObjectDatabaseProvider provider
			, int objectKind, string name)
		{
			Contract.Requires(provider != null);
			Contract.Requires<ArgumentOutOfRangeException>(objectKind != PhxUtil.kObjectKindNone);

			var dbi = provider.GetNamesInterface(objectKind);
			return dbi.TryGetIdWithUndefined(name);
		}

		public static string GetName(this Phx.IProtoDataObjectDatabaseProvider provider
			, int objectKind, int id)
		{
			Contract.Requires(provider != null);
			Contract.Requires<ArgumentOutOfRangeException>(objectKind != PhxUtil.kObjectKindNone);

			var dbi = provider.GetMembersInterface(objectKind);
			return dbi.TryGetNameWithUndefined(id);
		}

		public static object GetObject(this Phx.IProtoDataObjectDatabaseProvider provider
			, int objectKind, int id)
		{
			Contract.Requires(provider != null);
			Contract.Requires<ArgumentOutOfRangeException>(objectKind != PhxUtil.kObjectKindNone);

			var dbi = provider.GetNamesInterface(objectKind);
			return dbi.GetObject(id);
		}
	};
}