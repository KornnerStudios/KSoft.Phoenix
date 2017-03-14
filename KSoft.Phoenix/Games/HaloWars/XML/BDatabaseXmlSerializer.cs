
using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.HaloWars
{
	partial class BDatabase
	{
		protected override XML.BDatabaseXmlSerializerBase NewXmlSerializer()
		{
			return new BDatabaseXmlSerializer(this);
		}
	};

	partial class BDatabaseXmlSerializer
		: XML.BDatabaseXmlSerializerBase
	{
		BDatabase mDatabase;

		internal override Phx.BDatabaseBase Database { get { return mDatabase; } }

		public BDatabaseXmlSerializer(BDatabase db)
		{
			mDatabase = db;
		}

		protected override void PostStreamXml(FA mode)
		{
			base.PostStreamXml(mode);

			if (mode == FA.Read)
			{
				mDatabase.SetupDBIDs();
			}
		}
	};
}