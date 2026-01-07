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
			{
				return SourceKind.ToString();
			}

			return string.Format("{0} - {1}",
				SourceKind, FileReference);
		}

		public ProtoDataObjectDatabase GetObjectDatabase(Engine.PhxEngine engine)
		{
			return SourceKind switch
			{
				ProtoDataObjectSourceKind.Database => new(engine.Database, typeof(DatabaseObjectKind)),
				ProtoDataObjectSourceKind.GameData => new(engine.Database.GameData, typeof(GameDataObjectKind)),
				ProtoDataObjectSourceKind.HPData => new(engine.Database.HPBars, typeof(HPBarDataObjectKind)),
				_ => throw new System.NotImplementedException(string.Format(
					nameof(GetObjectDatabase) + " needs support for {0}",
					this)),
			};
		}
	};
}