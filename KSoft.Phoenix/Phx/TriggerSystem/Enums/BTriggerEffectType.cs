
namespace KSoft.Phoenix.Phx
{
	public enum BTriggerEffectType
	{
		#region 0

		#region 30
		TriggerActivate = 31,

		PlaySound = 33,


		CreateSquad = 36,
		#endregion
		#region 40
		#endregion
		#region 50
		Shutdown = 51,



		RandomLocation = 55,
		#endregion
		#region 60
		#endregion
		#region 70
		AttachmentAddType = 74,


		PowerChargeUseOf = 77,
		#endregion
		#region 80
		IsAlive = 80,


		AttachmentRemoveType = 83,

		CopyTech = 85,


		CopyProtoObject = 88,
		CopyObjectType = 89,
		#endregion
		#region 90
		CopyProtoSquad = 90,

		CopyPlayer = 97,
		CopyInt = 98,
		CopyLocation = 99,
		#endregion

		#endregion

		#region 100

		#region 00
		CopyFloat = 103,
		#endregion
		#region 10
		#endregion
		#region 20
		IteratorPlayerList = 120,
		//Iterator?List = 121,


		PlayerListAdd = 124,
		#endregion
		#region 30
		SetPlayerState = 130,






		ChangeOwner = 137,
		#endregion
		#region 40






		CopySquadList = 145,
		UnitListGetSize = 146,
		SquadListGetSize = 147,
		UnitListAdd = 148,
		SquadListAdd = 149,
		#endregion
		#region 50
		UnitListRemove = 150,
		SquadListRemove = 151,
		IteratorUnitList = 152,
		IteratorSquadList = 153,
		#endregion
		#region 60
		CopyString = 169,
		#endregion
		#region 70
		RandomInt = 178,
		MathInt = 179,
		#endregion
		#region 80
		GetLocation = 189,
		#endregion
		#region 90
		GetOwner = 193,



		DebugVar_Tech = 197,
		#endregion

		#endregion

		#region 200

		#region 00
		DebugVar_ProtoSquad = 202,





		DebugVar_Player = 208,
		DebugVar_Int = 209,
		#endregion
		#region 10
		DebugVar_Float = 218,
		#endregion
		#region 20
		#endregion
		#region 30
		DebugVar_String = 230,








		GetPlayerCiv = 239,
		#endregion
		#region 40
		#endregion
		#region 50
		#endregion
		#region 60
		MathTime = 263,

		GetGameTime = 265,
		#endregion
		#region 70
		SetResources = 277,
		#endregion
		#region 80
		//Iterator?List = 281,

		IteratorLocationList = 288,
		#endregion
		#region 90
		#endregion

		#endregion

		#region 300

		#region 00
		ProtoSquadListShuffle = 305,
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		UIUnlock = 330,
		#endregion
		#region 40

		FilterClear = 341,


		FilterAddPlayers = 344,

		FilterAddProtoObjects = 346,
		FilterAddProtoSquads = 347,
		FilterAddObjectTypes = 348,
		UnitListFilter = 349,
		#endregion
		#region 50
		MathFloat = 353,
		#endregion
		#region 60
		#endregion
		#region 70
		#endregion
		#region 80
		#endregion
		#region 90
		LaunchScript = 392,
		GetPlayerTeam = 393,
		#endregion

		#endregion

		#region 400

		#region 00
		#endregion
		#region 10
		IsBuilt = 414,
		#endregion
		#region 20
		#endregion
		#region 30
		GetPlayers2 = 431,
		#endregion
		#region 40
		IteratorKBBaseList = 443,
		#endregion
		#region 50
		#endregion
		#region 60
		#endregion
		#region 70
		GetPlayerLeader = 475,
		#endregion
		#region 80
		CopyProtoSquadList = 482,
		#endregion
		#region 90
		//Iterator?List = 490,
		IteratorProtoSquadList = 491,
		//Iterator?List = 492,
		//Iterator?List = 493,

		NextProtoSquad = 495,


		AsInt = 498,
		#endregion

		#endregion

		#region 500

		#region 00
		GetLegalSquads = 505,
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		AIScoreMissionTargets = 544,
		AISortMissionTargets = 545,
		#endregion
		#region 50
		AIGetMissionTargets = 551,
		#endregion
		#region 60
		CopyIntegerList = 562,

		AISetScoringParms = 564,



		IntToCount = 568,
		#endregion
		#region 70
		ProtoSquadListAdd = 571,
		ProtoSquadListRemove = 572,
		#endregion
		#region 80
		IntegerListAdd = 580,
		IntegerListRemove = 581,
		IntegerListGetSize = 582,
		#endregion
		#region 90
		#endregion

		#endregion

		#region 600

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		InputUISquadList = 639,
		#endregion
		#region 40
		GetPlayerEconomy = 647,
		#endregion
		#region 50
		TimerCreate = 658,
		#endregion
		#region 60
		AIAnalyzeSquadList = 668,
		#endregion
		#region 70
		AISAGetComponent = 672,

		AIAnalyzeProtoSquadList = 674,

		CostToFloat = 678,
		GetCost = 679,
		#endregion
		#region 80
		#endregion
		#region 90
		//?ListGetSize = 694,
		#endregion

		#endregion

		#region 700

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		AIBindLog = 720,
		#endregion
		#region 30
		GetPop = 736,
		#endregion
		#region 40
		SetPlayerPop = 741,
		#endregion
		#region 50
		#endregion
		#region 60
		GetTableRow = 762,
		#endregion
		#region 70
		#endregion
		#region 80
		#endregion
		#region 90
		#endregion

		#endregion

		#region 800

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		//?ListGetSize = 843,
		#endregion
		#region 50
		//?ListGetSize = 859,
		#endregion
		#region 60
		#endregion
		#region 70
		//?ListGetSize = 870,


		//?ListGetSize = 873,
		#endregion
		#region 80
		//?ListGetSize = 889,
		#endregion
		#region 90
		#endregion

		#endregion

		#region 900

		#region 00
		#endregion
		#region 10
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		#endregion
		#region 50
		#endregion
		#region 60
		#endregion
		#region 70
		//?ListGetSize = 970,





		GetPowerRadius = 976,
		#endregion
		#region 80
		#endregion
		#region 90
		#endregion

		#endregion

		#region 1000

		#region 00
		AICreateAreaTarget = 1004,
		AIMissionCreate = 1005,


		AICreateTargetWrapper = 1008,
		#endregion
		#region 10

		AIWrapperModifyRadius = 1011,
		AIWrapperModifyFlags = 1012,
		AIWrapperModifyParms = 1013,

		ObjectTypeToProtoObjects = 1019,
		#endregion
		#region 20
		#endregion
		#region 30
		#endregion
		#region 40
		#endregion
		#region 50
		#endregion
		#region 60
		GetGameMode = 1065,
		AISetPlayerBuildSpeedModifiers = 1066,
		FilterAddCanChangeOwner = 1067,


		#endregion

		#endregion
	};
}