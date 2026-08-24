using System;

namespace KSoft.Phoenix.Engine
{
	public enum XmlFilePriority
	{
		None,

		Lists,
		GameData,
		ProtoData,

		kNumberOf
	};

	public enum XmlFileLoadState
	{
		NotLoaded,
		FileDoesNotExist,
		Failed,
		Preloading,
		Preloaded,
		Loading,
		Loaded,

		kNumberOf
	};

	public sealed class XmlFileInfo
		: IComparable<XmlFileInfo>
		, IEquatable<XmlFileInfo>
	{
		public static bool RespectWritableFlag => true;

		public ContentStorage Location { get; set; }
		public GameDirectory Directory { get; set; }
		public string? FileName { get; set; }
		public string? RootName { get; set; }

		public bool Writable { get; set; }

		public int CompareTo(XmlFileInfo? other)
		{
			if (other is null)
			{
				return 1;
			}

			if (Location != other.Location)
			{
				return ((int)Location).CompareTo((int)other.Location);
			}

			if (Directory != other.Directory)
			{
				return ((int)Directory).CompareTo((int)other.Directory);
			}

			return string.CompareOrdinal(FileName, other.FileName);
		}

		public bool Equals(XmlFileInfo? other)
		{
			return other is not null
				&& Location == other.Location
				&& Directory == other.Directory
				&& FileName == other.FileName
				//&& RootName == other.RootName
				//&& Writable == other.Writable
				;
		}

		public override bool Equals(object? obj)
		{
			return obj is XmlFileInfo info && Equals(info);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Location, Directory, FileName);
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}",
				Location, Directory, FileName);
		}
	};

	public class XmlFileLoadStateChangedEventArgs
		: EventArgs
	{
		public XmlFileInfo XmlFile { get; private set; }
		public XmlFileLoadState NewState { get; private set; }

		public XmlFileLoadStateChangedEventArgs(XmlFileInfo xmlFile, XmlFileLoadState newState)
		{
			XmlFile = xmlFile;
			NewState = newState;
		}
	};

	[System.Diagnostics.DebuggerDisplay("{"+ nameof(ProtoDataXmlFileInfo.DebuggerDisplay)  +"}")]
	public sealed class ProtoDataXmlFileInfo
	{
		public XmlFilePriority Priority;
		public XmlFileInfo FileInfo;
		public XmlFileInfo? FileInfoWithUpdates;

		public ProtoDataXmlFileInfo(XmlFilePriority priority
			, XmlFileInfo fileInfo
			, XmlFileInfo? fileInfoWithUpdates = null)
		{
			Priority = priority;
			FileInfo = fileInfo;
			FileInfoWithUpdates = fileInfoWithUpdates;
		}

		public string DebuggerDisplay
			=> string.Format("{0} {1}",
				Priority, FileInfo);
	};
}