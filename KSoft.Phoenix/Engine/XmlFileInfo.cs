using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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
		public static bool RespectWritableFlag { get { return true; } }

		public ContentStorage Location { get; set; }
		public GameDirectory Directory { get; set; }
		public string FileName { get; set; }
		public string RootName { get; set; }

		public bool Writable { get; set; }

		public int CompareTo(XmlFileInfo other)
		{
			if (Location != other.Location)
				return ((int)Location).CompareTo((int)other.Location);

			if (Directory != other.Directory)
				return ((int)Directory).CompareTo((int)other.Directory);

			return string.CompareOrdinal(FileName, other.FileName);
		}

		public bool Equals(XmlFileInfo other)
		{
			return Location == other.Location
				&& Directory == other.Directory
				&& FileName == other.FileName
				//&& RootName == other.RootName
				//&& Writable == other.Writable
				;
		}

		public override bool Equals(object obj)
		{
			return obj is XmlFileInfo && Equals((XmlFileInfo)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash *= 23 + Location.GetHashCode();
				hash *= 23 + Directory.GetHashCode();
				hash *= 23 + FileName.GetHashCode();
				return hash;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}",
				Location, Directory, FileName);
		}
	};

	public class XmlFileLoadStateChangedArgs
		: EventArgs
	{
		public XmlFileInfo XmlFile { get; private set; }
		public XmlFileLoadState NewState { get; private set; }

		public XmlFileLoadStateChangedArgs(XmlFileInfo xmlFile, XmlFileLoadState newState)
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
		public XmlFileInfo FileInfoWithUpdates;

		public ProtoDataXmlFileInfo(XmlFilePriority priority
			, XmlFileInfo fileInfo
			, XmlFileInfo fileInfoWithUpdates = null)
		{
			Priority = priority;
			FileInfo = fileInfo;
			FileInfoWithUpdates = fileInfoWithUpdates;
		}

		public string DebuggerDisplay { get {
			return string.Format("{0} {1}",
				Priority, FileInfo);
		} }
	};
}