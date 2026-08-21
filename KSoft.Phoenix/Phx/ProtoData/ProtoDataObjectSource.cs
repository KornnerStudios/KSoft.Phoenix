using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class ProtoDataObjectSource
	{
		public ProtoDataObjectSourceKind SourceKind { get; private set; }
		public Engine.XmlFileInfo FileReference { get; private set; }

		public ProtoDataObjectSource(ProtoDataObjectSourceKind kind, Engine.XmlFileInfo fileReference)
		{
			if (kind == ProtoDataObjectSourceKind.None)
			{
				throw new ArgumentOutOfRangeException(nameof(kind), kind, "Source kind cannot be None.");
			}
			if (kind.RequiresFileReference())
			{
				ArgumentNullException.ThrowIfNull(fileReference);
			}

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