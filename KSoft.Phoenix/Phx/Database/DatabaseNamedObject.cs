using System;
using System.ComponentModel;

namespace KSoft.Phoenix.Phx
{
	public abstract class DatabaseNamedObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		internal const string kXmlAttrName = "name";
		internal const string kXmlAttrNameN = "Name";
		#endregion

		#region UserInterfaceTextData
		[Browsable(false)]
		public DatabaseObjectUserInterfaceTextData? UserInterfaceTextData { get; private set; }

		protected DatabaseObjectUserInterfaceTextData CreateDatabaseObjectUserInterfaceTextData()
		{
			if (UserInterfaceTextData != null)
			{
				throw new InvalidOperationException("User interface text data is already initialized.");
			}

			UserInterfaceTextData = new DatabaseObjectUserInterfaceTextData();
			return UserInterfaceTextData;
		}
		#endregion

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			UserInterfaceTextData?.Serialize(s);
		}
		#endregion
	};
}