using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

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
		public DatabaseObjectUserInterfaceTextData UserInterfaceTextData { get; private set; }

		protected DatabaseObjectUserInterfaceTextData CreateDatabaseObjectUserInterfaceTextData()
		{
			Contract.Requires(UserInterfaceTextData != null);

			UserInterfaceTextData = new DatabaseObjectUserInterfaceTextData();
			return UserInterfaceTextData;
		}
		#endregion

		#region IXmlElementStreamable Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			if (UserInterfaceTextData != null)
				UserInterfaceTextData.Serialize(s);
		}
		#endregion
	};
}