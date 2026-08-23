
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrErrorStringID = "errorStringID";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrSuccessStringID = "successStringID";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrDistance = "distance"; // float
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrPlayer = "player"; // BPlayerType
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrLife = "life"; // BPlacementRuleLifeType
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttr = "";

		// DistanceAtLeastFromType and ObstructionAtLeastFromType only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrFoundation = "foundation"; // BPlacementRuleFoundationType, DistanceAtLeastFromType only supports 'Any'
		// DistanceAtLeastFromType only
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrIncludeObstructionRadius = "includeObstructionRadius"; // bool

		// Old files (which are included in HW) didn't use SIDs
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrErrorString = "errorString";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlAttrSuccessString = "successString";
		#endregion
	};
	/*public*/ sealed class BPlacementRules
	{
	};
}