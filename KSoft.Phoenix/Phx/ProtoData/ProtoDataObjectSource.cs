#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	public sealed class ProtoDataObjectSource
	{
		public ProtoDataObjectSourceKind SourceKind { get; private set; }
		public Engine.XmlFileInfo FileReference { get; private set; }

		public ProtoDataObjectSource(ProtoDataObjectSourceKind kind, Engine.XmlFileInfo fileReference)
		{
			Contract.Requires(kind != ProtoDataObjectSourceKind.None);
			Contract.Requires(!kind.RequiresFileReference() || fileReference != null);

			SourceKind = kind;
			FileReference = fileReference;
		}

		public override string ToString()
		{
			if (FileReference == null)
				return SourceKind.ToString();

			return string.Format("{0} - {1}",
				SourceKind, FileReference);
		}

		public ProtoDataObjectDatabase GetObjectDatabase(Engine.PhxEngine engine)
		{
			switch (SourceKind)
			{
				case ProtoDataObjectSourceKind.Database:
					return new ProtoDataObjectDatabase(engine.Database, typeof(DatabaseObjectKind));

				case ProtoDataObjectSourceKind.GameData:
					return new ProtoDataObjectDatabase(engine.Database.GameData, typeof(GameDataObjectKind));

				case ProtoDataObjectSourceKind.HPData:
					return new ProtoDataObjectDatabase(engine.Database.HPBars, typeof(HPBarDataObjectKind));

				default:
					throw new System.NotImplementedException(string.Format(
						nameof(GetObjectDatabase) + " needs support for {0}",
						this));
			}
		}
	};
}