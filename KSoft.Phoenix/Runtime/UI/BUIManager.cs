
namespace KSoft.Phoenix.Runtime
{
	// Since BWorld serialization hasn't been finished, this code hasn't even been tested

	sealed class BUIManager
		: IO.IEndianStreamSerializable
	{
		public BTimerManager TimerManager = new BTimerManager();
		public BUICallouts UICallouts = new BUICallouts();
		public BUIWidget UIWidgets = new BUIWidget();
		public bool WidgetsVisible, TalkingHeadShown, ObjectiveTrackerShown,
			TimerShown, ObjectiveWidgetsShown, HintsVisible,
			MinimapVisible, UnitStatsVisible, ReticleVisible,
			ResourcePanelVisible, DpadPanelVisible, GameTimeVisible;
		public float MinimapRotationOffset;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(TimerManager);
			s.Stream(UICallouts);
			s.Stream(UIWidgets);
			s.Stream(ref WidgetsVisible); s.Stream(ref TalkingHeadShown); s.Stream(ref ObjectiveTrackerShown);
			s.Stream(ref TimerShown); s.Stream(ref ObjectiveWidgetsShown); s.Stream(ref HintsVisible);
			s.Stream(ref MinimapVisible); s.Stream(ref UnitStatsVisible); s.Stream(ref ReticleVisible);
			s.Stream(ref ResourcePanelVisible); s.Stream(ref DpadPanelVisible); s.Stream(ref GameTimeVisible);
			s.Stream(ref MinimapRotationOffset);
		}
		#endregion
	};
}