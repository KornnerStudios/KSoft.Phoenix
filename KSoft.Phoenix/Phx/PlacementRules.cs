
namespace KSoft.Phoenix.Phx
{
	public enum BPlacementRuleType
	{
		And,
		Or,

		DistanceAtMostFromType,
		DistanceAtLeastFromType,
		ObstructionAtLeastFromType,
	};
	public enum BPlacementRuleFromTypeKind
	{
		Builder,
		Unit,
	};
	public enum BPlacementRuleLifeType
	{
		Any,
		Alive,
		Dead,
	};
	public enum BPlacementRuleFoundationType
	{
		Any,
		Solid,
		FullyBuilt,
	};

	/*public*/ sealed class BPlacementRule
	{
		#region Xml constants
		const string kXmlAttrErrorStringID = "errorStringID";
		const string kXmlAttrSuccessStringID = "successStringID";

		const string kXmlAttrDistance = "distance"; // float
		const string kXmlAttrPlayer = "player"; // BPlayerType
		const string kXmlAttrLife = "life"; // BPlacementRuleLifeType
		const string kXmlAttr = "";

		// DistanceAtLeastFromType and ObstructionAtLeastFromType only
		const string kXmlAttrFoundation = "foundation"; // BPlacementRuleFoundationType, DistanceAtLeastFromType only supports 'Any'
		// DistanceAtLeastFromType only
		const string kXmlAttrIncludeObstructionRadius = "includeObstructionRadius"; // bool

		// Old files (which are included in HW) didn't use SIDs
		const string kXmlAttrErrorString = "errorString";
		const string kXmlAttrSuccessString = "successString";
		#endregion
	};
	/*public*/ sealed class BPlacementRules
	{
	};
}