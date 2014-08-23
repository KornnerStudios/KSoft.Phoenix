
using BVector = SlimMath.Vector4;
using BObjectiveID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	static partial class cSaveMarker
	{
		public const ushort 
			UIWidget = 0x2710
			;
	};

	sealed class BUIWidget
		: IO.IEndianStreamSerializable
	{
		public struct BReticulePointer
			: IO.IEndianStreamSerializable
		{
			public ulong Unknown0, Unknown8;
			public uint Unknown10;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Unknown0); s.Stream(ref Unknown8); s.Stream(ref Unknown10);
			}
			#endregion
		};

		public sealed class BUITalkingHeadControl
			: IO.IEndianStreamSerializable
		{
			public string TalkingHeadText;
			public int ObjectiveID, LastCount;
			public bool ShowBackground, ObjectiveVisible, TalkingHeadVisible, IsShown;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalWideString32(ref TalkingHeadText);
				s.Stream(ref ObjectiveID); s.Stream(ref LastCount);
				s.Stream(ref ShowBackground); s.Stream(ref ObjectiveVisible); s.Stream(ref TalkingHeadVisible); s.Stream(ref IsShown);
			}
			#endregion
		};

		public sealed class BUIObjectiveProgressControl
			: IO.IEndianStreamSerializable
		{
			public struct BUILabel
				: IO.IEndianStreamSerializable
			{
				public bool IsShown;
				public string Text;

				#region IEndianStreamSerializable Members
				public void Serialize(IO.EndianStream s)
				{
					s.Stream(ref IsShown);
					s.StreamPascalWideString32(ref Text);
				}
				#endregion
			};

			public struct BUIObjectiveProgressData
				: IO.IEndianStreamSerializable
			{
				public BObjectiveID ObjectiveID;
				public int LastCount;
				public uint FadeTime;
				public int LabelIndex, MinIncrement;

				#region IEndianStreamSerializable Members
				public void Serialize(IO.EndianStream s)
				{
					s.Stream(ref ObjectiveID);
					s.Stream(ref LastCount);
					s.Stream(ref FadeTime);
					s.Stream(ref LabelIndex); s.Stream(ref MinIncrement);
				}
				#endregion
			};

			public BUILabel[] ObjectiveLabels = new BUILabel[4];
			public BUIObjectiveProgressData[] Objectives;

			public BUIObjectiveProgressControl()
			{
				for (int x = 0; x < ObjectiveLabels.Length; x++)
					ObjectiveLabels[x] = new BUILabel();
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				for (int x = 0; x < ObjectiveLabels.Length; x++) s.Stream(ref ObjectiveLabels[x]);
				BSaveGame.StreamArray(s, ref Objectives);
			}
			#endregion
		};

		public int CounterCurrent, CounterMax, TimerID, TimerLabelID;
		public float ElapsedTimerTime;
		public int NumCitizensSaved, NumCitizensNeeded;

		public ushort GarrisonContainerVisible0; public byte GarrisonContainerVisible2;
		public int GarrisonContainerEntities0, GarrisonContainerEntities4, GarrisonContainerEntities8;
		public ushort GarrisonContainerUseEntity0; public byte GarrisonContainerUseEntity2;
		public int GarrisonContainerCounts0, GarrisonContainerCounts4, GarrisonContainerCounts8;

		public uint ReticulePointersVisible0; public byte ReticulePointersVisible4;
		public BReticulePointer ReticulePointerType;
		public BVector[] ReticulePointerArea = new BVector[3];
		public BReticulePointer ReticulePointerEntities, PointerRotation, PointerRotationFloat;
		public BUITalkingHeadControl TalkingHeadControl = new BUITalkingHeadControl();
		public BUIObjectiveProgressControl ObjectiveProgressControl = new BUIObjectiveProgressControl();
		public bool WidgetPanelVisible, TimerVisible, CitizensSavedVisible,
			CounterVisible, TimerShown;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref CounterCurrent); s.Stream(ref CounterMax); s.Stream(ref TimerID); s.Stream(ref TimerLabelID);
			s.Stream(ref ElapsedTimerTime);
			s.Stream(ref NumCitizensSaved); s.Stream(ref NumCitizensNeeded);

			s.Stream(ref GarrisonContainerVisible0); s.Stream(ref GarrisonContainerVisible2);
			s.Stream(ref GarrisonContainerEntities0); s.Stream(ref GarrisonContainerEntities4); s.Stream(ref GarrisonContainerEntities8);
			s.Stream(ref GarrisonContainerUseEntity0); s.Stream(ref GarrisonContainerUseEntity2);
			s.Stream(ref GarrisonContainerCounts0); s.Stream(ref GarrisonContainerCounts4); s.Stream(ref GarrisonContainerCounts8);

			s.Stream(ref ReticulePointersVisible0); s.Stream(ref ReticulePointersVisible4);
			s.Stream(ref ReticulePointerType);
			for (int x = 0; x < ReticulePointerArea.Length; x++) s.StreamV(ref ReticulePointerArea[x]);
			s.Stream(ref ReticulePointerEntities);
			s.Stream(ref PointerRotation);
			s.Stream(ref PointerRotationFloat);
			s.Stream(TalkingHeadControl);
			s.Stream(ObjectiveProgressControl);
			s.Stream(ref WidgetPanelVisible); s.Stream(ref TimerVisible); s.Stream(ref CitizensSavedVisible);
			s.Stream(ref CounterVisible); s.Stream(ref TimerShown);
			s.StreamSignature(cSaveMarker.UIWidget);
		}
		#endregion
	};
}