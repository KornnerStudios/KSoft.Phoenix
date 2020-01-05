using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	static partial class cSaveMarker
	{
		public const ushort 
			UICallouts1 = 0x2710,
			UICallouts2 = 0x2711,
			UICallout = 0x2712
			;
	};

	sealed class BUICallouts
		: IO.IEndianStreamSerializable
	{
		const byte cNumCallouts = 5;

		public sealed class BUICallout
			: IO.IEndianStreamSerializable
		{
			public int ID;
			public uint Type;
			public BVector Location;
			public BEntityID EntityID, CalloutEntityID;
			public int LocStringIndex, UICalloutID, X, Y;
			public bool Visible;

			internal BVector Position; // Not actually part of this struct

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref ID);
				s.Stream(ref Type);
				s.StreamV(ref Location);
				s.Stream(ref EntityID); s.Stream(ref CalloutEntityID);
				s.Stream(ref LocStringIndex); s.Stream(ref UICalloutID); s.Stream(ref X); s.Stream(ref Y);
				s.Stream(ref Visible);
				s.StreamSignature(cSaveMarker.UICallout);

				s.StreamV(ref Position);
			}
			#endregion
		};
		static readonly CondensedListInfo kCalloutsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
		};

		public List<CondensedListItem16<BUICallout>> Callouts = new List<CondensedListItem16<BUICallout>>();
		public int[] CalloutWidgets = new int[cNumCallouts];
		public int NextCalloutID;
		public bool PanelVisible, CalloutsVisible;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamList(s, Callouts, kCalloutsListInfo);
			s.StreamSignature(cSaveMarker.UICallouts1);
			s.StreamSignature(cNumCallouts);
			for (int x = 0; x < CalloutWidgets.Length; x++)
				s.Stream(ref CalloutWidgets[x]);
			s.Stream(ref NextCalloutID);
			s.Stream(ref PanelVisible); s.Stream(ref CalloutsVisible);
			s.StreamSignature(cSaveMarker.UICallouts2);
		}
		#endregion
	};
}