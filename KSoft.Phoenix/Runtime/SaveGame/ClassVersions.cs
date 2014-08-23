
namespace KSoft.Phoenix.Runtime
{
	static class kClassVersions
	{
		public const uint BSaveGame = 0xE;

		public const uint
			BAction=0x34,		BActionManager=3,	BAI=5,				BAIGlobals = 3,		BAIDifficultySetting=1,
			BAIManager=3,		BAIMission=4,		BAIMissionScore=1,	BAIMissionTarget=2, BAIMissionTargetWrapper=2,
			BAIPowerMission=3,	BAIScoreModifier=1,	BAISquadAnalysis=1,	BAITopic=2,			BArmy=1,
			BBid=1,				BBidManager=1,		BChatManager=2,		BConvexHull=1,		BCost=1,
			BCustomCommand=1,	BDamageTracker=1,	BDopple=1,			BEntity=2,			BEntityFilter=1,
				
			BEntityFilterSet=1,	BFatalityManager=2,	BFormation2=1,		BGeneralEventManager=1, BHintEngine=1,
			BHintManager=1,		BKB=1,				BKBBase=2,			BKBBaseQuery=1,			BKBSquad=2,
			BKBSquadFilter=1,	BKBSquadFilterSet=1,BKBSquadQuery=1,	BObject=8,				BObjectAnimationState=1,
			BObjectiveManager=1,BPath=1,			BPathingLimiter=2,	BPiecewiseDataPoint=1,	BPiecewiseFunc=1,
			BPlatoon=3,			BPlayer=9,			BPower=0x16,		BPowerEntry=1,			BPowerEntryItem=1,
				
			BPowerManager=1,BPowerUser=9,	BProjectile=4,		BProtoAction=2,			BProtoObject=3,
			BProtoSquad=3,	BProtoTech=1,	BSaveDB=9,			BSavePlayer=1,			BSaveTeam=1,
			BSaveUser=3,	BScoreManager=4,BSelectionAbility=1,BSimOrder=2,			BSimOrderEntry=2,
			BSimTarget=1,	BSquad=6,		BSquadAI=4,			BSquadPlotterResult=1,	BStatsManager=1,
			BTactic=2,		BTeam=2,		BTrigger=1,			BTriggerCondition=2,	BTriggerEffect=2,
				
			BTriggerGroup=1,	BTriggerManager=1,	BTriggerScript=2,			BTriggerScriptExternals=1,	BTriggerVar=3,
			BUICallouts=1,		BUIManager=4,		BUnit=7,					BUser=5,					BVisibleMap=1,
			BWeapon=3,			BWorld=0xE,			BStoredAnimEventManager=2,	BAIGroup=3,					BAIGroupTask=2,
			BAIMissionCache=4,	BAITeleporterZone=1,BAIPlayerModifier=1,		BEntityScheduler=2,			BCollectiblesManager=1,
			BVisual=1,			BVisualItem=3,		BVisualAnimationData=2,		BGrannyInstance=3,			BTimerManager=1,

			BUIWidgets=2,	BUIObjectiveProgressControl=1,	BUITalkingHeadControl=1,	BSquadActionEntry=2
			;

		#region IEndianStreamSerializable Members
		public static void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(BAction);			s.StreamVersion(BActionManager);	s.StreamVersion(BAI);				s.StreamVersion(BAIGlobals);		s.StreamVersion(BAIDifficultySetting);
			s.StreamVersion(BAIManager);		s.StreamVersion(BAIMission);		s.StreamVersion(BAIMissionScore);	s.StreamVersion(BAIMissionTarget);	s.StreamVersion(BAIMissionTargetWrapper);
			s.StreamVersion(BAIPowerMission);	s.StreamVersion(BAIScoreModifier);	s.StreamVersion(BAISquadAnalysis);	s.StreamVersion(BAITopic);			s.StreamVersion(BArmy);
			s.StreamVersion(BBid);				s.StreamVersion(BBidManager);		s.StreamVersion(BChatManager);		s.StreamVersion(BConvexHull);		s.StreamVersion(BCost);
			s.StreamVersion(BCustomCommand);	s.StreamVersion(BDamageTracker);	s.StreamVersion(BDopple);			s.StreamVersion(BEntity);			s.StreamVersion(BEntityFilter);
				
			s.StreamVersion(BEntityFilterSet);	s.StreamVersion(BFatalityManager);	s.StreamVersion(BFormation2);		s.StreamVersion(BGeneralEventManager);	s.StreamVersion(BHintEngine);
			s.StreamVersion(BHintManager);		s.StreamVersion(BKB);				s.StreamVersion(BKBBase);			s.StreamVersion(BKBBaseQuery);			s.StreamVersion(BKBSquad);
			s.StreamVersion(BKBSquadFilter);	s.StreamVersion(BKBSquadFilterSet);	s.StreamVersion(BKBSquadQuery);		s.StreamVersion(BObject);				s.StreamVersion(BObjectAnimationState);
			s.StreamVersion(BObjectiveManager);	s.StreamVersion(BPath);				s.StreamVersion(BPathingLimiter);	s.StreamVersion(BPiecewiseDataPoint);	s.StreamVersion(BPiecewiseFunc);
			s.StreamVersion(BPlatoon);			s.StreamVersion(BPlayer);			s.StreamVersion(BPower);			s.StreamVersion(BPowerEntry);			s.StreamVersion(BPowerEntryItem);
				
			s.StreamVersion(BPowerManager);	s.StreamVersion(BPowerUser);	s.StreamVersion(BProjectile);		s.StreamVersion(BProtoAction);			s.StreamVersion(BProtoObject);
			s.StreamVersion(BProtoSquad);	s.StreamVersion(BProtoTech);	s.StreamVersion(BSaveDB);			s.StreamVersion(BSavePlayer);			s.StreamVersion(BSaveTeam);
			s.StreamVersion(BSaveUser);		s.StreamVersion(BScoreManager);	s.StreamVersion(BSelectionAbility);	s.StreamVersion(BSimOrder);				s.StreamVersion(BSimOrderEntry);
			s.StreamVersion(BSimTarget);	s.StreamVersion(BSquad);		s.StreamVersion(BSquadAI);			s.StreamVersion(BSquadPlotterResult);	s.StreamVersion(BStatsManager);
			s.StreamVersion(BTactic);		s.StreamVersion(BTeam);			s.StreamVersion(BTrigger);			s.StreamVersion(BTriggerCondition);		s.StreamVersion(BTriggerEffect);
				
			s.StreamVersion(BTriggerGroup);		s.StreamVersion(BTriggerManager);	s.StreamVersion(BTriggerScript);			s.StreamVersion(BTriggerScriptExternals);	s.StreamVersion(BTriggerVar);
			s.StreamVersion(BUICallouts);		s.StreamVersion(BUIManager);		s.StreamVersion(BUnit);						s.StreamVersion(BUser);						s.StreamVersion(BVisibleMap);
			s.StreamVersion(BWeapon);			s.StreamVersion(BWorld);			s.StreamVersion(BStoredAnimEventManager);	s.StreamVersion(BAIGroup);					s.StreamVersion(BAIGroupTask);
			s.StreamVersion(BAIMissionCache);	s.StreamVersion(BAITeleporterZone);	s.StreamVersion(BAIPlayerModifier);			s.StreamVersion(BEntityScheduler);			s.StreamVersion(BCollectiblesManager);
			s.StreamVersion(BVisual);			s.StreamVersion(BVisualItem);		s.StreamVersion(BVisualAnimationData);		s.StreamVersion(BGrannyInstance);			s.StreamVersion(BTimerManager);

			s.StreamVersion(BUIWidgets);	s.StreamVersion(BUIObjectiveProgressControl);	s.StreamVersion(BUITalkingHeadControl);	s.StreamVersion(BSquadActionEntry);

			s.StreamSignature(cSaveMarker.Versions);
		}
		#endregion
	};
}