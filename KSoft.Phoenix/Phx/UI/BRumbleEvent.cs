
namespace KSoft.Phoenix.Phx
{
	public sealed class BRumbleEvent
		: IO.ITagElementStringNameStreamable
	{
		#region LeftRumbleType
		BRumbleType mLeftRumbleType;
		public BRumbleType LeftRumbleType
		{
			get { return mLeftRumbleType; }
			set { mLeftRumbleType = value; }
		}
		#endregion

		#region RightRumbleType
		BRumbleType mRightRumbleType;
		public BRumbleType RightRumbleType
		{
			get { return mRightRumbleType; }
			set { mRightRumbleType = value; }
		}
		#endregion

		#region Duration
		float mDuration;
		public float Duration
		{
			get { return mDuration; }
			set { mDuration = value; }
		}
		#endregion

		#region LeftStrength
		float mLeftStrength;
		public float LeftStrength
		{
			get { return mLeftStrength; }
			set { mLeftStrength = value; }
		}
		#endregion

		#region RightStrength
		float mRightStrength;
		public float RightStrength
		{
			get { return mRightStrength; }
			set { mRightStrength = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("LeftRumbleType", ref mLeftRumbleType, e => e != BRumbleType.None);
			s.StreamAttributeEnumOpt("RightRumbleType", ref mRightRumbleType, e => e != BRumbleType.None);
			s.StreamAttributeOpt("Duration", ref mDuration, Predicates.IsNotZero);
			s.StreamAttributeOpt("LeftStrength", ref mLeftStrength, Predicates.IsNotZero);
			s.StreamAttributeOpt("RightStrength", ref mRightStrength, Predicates.IsNotZero);
		}
		#endregion
	};
}