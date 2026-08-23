
namespace KSoft.Phoenix.Phx
{
	/*public*/ sealed class BTacticState // suicide grunts use this...name and action are omitted, so fuck this
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "State",
			DataName = "Name",
			Flags = XML.BCollectionXmlParamsFlags.UseElementForData |
				XML.BCollectionXmlParamsFlags.ForceNoRootElementStreaming,
		};

		//////////////////////////////////////////////////////////////////////////
		// anim names
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementIdleAnim = "IdleAnim";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementWalkAnim = "WalkAnim";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementJogAnim = "JogAnim";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementRunAnim = "RunAnim";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementDeathAnim = "DeathAnim";
		//////////////////////////////////////////////////////////////////////////
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Retains XML schema reference data.")]
		const string kXmlElementAction = "Action";
		#endregion
	};
}