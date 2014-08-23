using System.Xml;

namespace KSoft.Phoenix.XML
{
	partial class BDatabaseXmlSerializerBase
	{
		protected virtual void FixWeaponTypes() {}

		protected static XmlNode XPathSelectNodeByName(KSoft.IO.XmlElementStream s, XML.BListXmlParams op, 
			string dataName, string attributeName = Phx.DatabaseNamedObject.kXmlAttrName)
		{
			string xpath = string.Format(
				"/{0}/{1}[@{2}='{3}']",
				op.RootName, op.ElementName, attributeName, dataName);
			return s.Document.SelectSingleNode(xpath);
		}

		protected virtual void FixGameDataXml(KSoft.IO.XmlElementStream s)
		{ }
		protected virtual void FixGameData()
		{ }

		protected virtual void FixObjectsXml(KSoft.IO.XmlElementStream s)
		{ }

		protected virtual void FixSquadsXml(KSoft.IO.XmlElementStream s)
		{ }

		protected virtual void FixTechsXml(KSoft.IO.XmlElementStream s)
		{ }

		protected static void FixTacticsTraceFixEvent(string tacticName, string xpath)
		{
			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
					"Fixing Tactic '{0}' with XPath={1}", tacticName, xpath);
		}
		protected virtual void FixTacticsXml(KSoft.IO.XmlElementStream s, string name)
		{ }
	};
}