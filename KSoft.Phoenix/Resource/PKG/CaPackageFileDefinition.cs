using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Resource.PKG
{
	public sealed class CaPackageFileDefinition
		: IO.ITagElementStringNameStreamable
	{
		public const string kFileExtension = ".pkgdef";

		/// <summary>This should be the source file's name or a user defined name</summary>
		public string PkgName { get; private set; }

		public List<string> FileNames { get; private set; }
			= new List<string>();

		#region ITagElementStringNameStreamable
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterUserDataBookmark(this))
			{
				s.StreamAttributeOpt("name", this, obj => PkgName, Predicates.IsNotNullOrEmpty);

				using (var bm = s.EnterCursorBookmarkOpt("Files", FileNames, Predicates.HasItems))
					s.StreamElements("File", FileNames);
			}
		}
		#endregion
	};
}

