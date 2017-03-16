
namespace KSoft.Phoenix.Phx
{
	// #TODO we shouldn't serialize out hardpoints without a name, the engine ASSERTs
	public sealed class BHardpoint
		: IO.ITagElementStringNameStreamable
	{
		const double cPiOver12 = (float)((1.0 / 12.0) * System.Math.PI);
		const float cPiOver12InDegrees = (float)(cPiOver12 * TypeExtensions.kDegreesPerRadian);

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Hardpoint",
		};
		#endregion

		#region Name
		string mName;
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		#endregion

		#region YawAttachment
		string mYawAttachment;
		[Meta.AttachmentTypeReference]
		public string YawAttachment
		{
			get { return mYawAttachment; }
			set { mYawAttachment = value; }
		}
		#endregion

		#region PitchAttachment
		string mPitchAttachment;
		[Meta.AttachmentTypeReference]
		public string PitchAttachment
		{
			get { return mPitchAttachment; }
			set { mPitchAttachment = value; }
		}
		#endregion

		#region AutoCenter
		bool mAutoCenter = true;
		public bool AutoCenter
		{
			get { return mAutoCenter; }
			set { mAutoCenter = value; }
		}
		#endregion

		#region SingleBoneIK
		bool mSingleBoneIK;
		public bool SingleBoneIK
		{
			get { return mSingleBoneIK; }
			set { mSingleBoneIK = value; }
		}
		#endregion

		#region Combined
		bool mCombined;
		public bool Combined
		{
			get { return mCombined; }
			set { mCombined = value; }
		}
		#endregion

		#region HardPitchLimits
		bool mHardPitchLimits;
		public bool HardPitchLimits
		{
			get { return mHardPitchLimits; }
			set { mHardPitchLimits = value; }
		}
		#endregion

		#region RelativeToUnit
		bool mRelativeToUnit;
		public bool RelativeToUnit
		{
			get { return mRelativeToUnit; }
			set { mRelativeToUnit = value; }
		}
		#endregion

		#region UseYawAndPitchAsTolerance
		bool mUseYawAndPitchAsTolerance;
		public bool UseYawAndPitchAsTolerance
		{
			get { return mUseYawAndPitchAsTolerance; }
			set { mUseYawAndPitchAsTolerance = value; }
		}
		#endregion

		#region InfiniteRateWhenHasTarget
		bool mInfiniteRateWhenHasTarget;
		public bool InfiniteRateWhenHasTarget
		{
			get { return mInfiniteRateWhenHasTarget; }
			set { mInfiniteRateWhenHasTarget = value; }
		}
		#endregion

		#region YawRotationRate
		float mYawRotationRate = cPiOver12InDegrees;
		// angle
		public float YawRotationRate
		{
			get { return mYawRotationRate; }
			set { mYawRotationRate = value; }
		}
		#endregion

		#region PitchRotationRate
		float mPitchRotationRate = cPiOver12InDegrees;
		// angle
		public float PitchRotationRate
		{
			get { return mPitchRotationRate; }
			set { mPitchRotationRate = value; }
		}
		#endregion

		#region YawLeftMaxAngle
		const float cDefaultYawLeftMaxAngle = (float)-(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mYawLeftMaxAngle = cDefaultYawLeftMaxAngle;
		// angle
		public float YawLeftMaxAngle
		{
			get { return mYawLeftMaxAngle; }
			set { mYawLeftMaxAngle = value; }
		}
		#endregion

		#region YawRightMaxAngle
		const float cDefaultYawRightMaxAngle = (float)+(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mYawRightMaxAngle = cDefaultYawRightMaxAngle;
		// angle
		public float YawRightMaxAngle
		{
			get { return mYawRightMaxAngle; }
			set { mYawRightMaxAngle = value; }
		}
		#endregion

		#region PitchMaxAngle
		const float cDefaultPitchMaxAngle = (float)-(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mPitchMaxAngle = cDefaultPitchMaxAngle;
		// angle
		public float PitchMaxAngle
		{
			get { return mPitchMaxAngle; }
			set { mPitchMaxAngle = value; }
		}
		#endregion

		#region PitchMinAngle
		const float cDefaultPitchMinAngle = (float)+(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mPitchMinAngle = cDefaultPitchMinAngle;
		// angle
		public float PitchMinAngle
		{
			get { return mPitchMinAngle; }
			set { mPitchMinAngle = value; }
		}
		#endregion

		#region StartYawSound
		string mStartYawSound;
		[Meta.SoundCueReference]
		public string StartYawSound
		{
			get { return mStartYawSound; }
			set { mStartYawSound = value; }
		}
		#endregion

		#region StopYawSound
		string mStopYawSound;
		[Meta.SoundCueReference]
		public string StopYawSound
		{
			get { return mStopYawSound; }
			set { mStopYawSound = value; }
		}
		#endregion

		#region StartPitchSound
		string mStartPitchSound;
		[Meta.SoundCueReference]
		public string StartPitchSound
		{
			get { return mStartPitchSound; }
			set { mStartPitchSound = value; }
		}
		#endregion

		#region StopPitchSound
		string mStopPitchSound;
		[Meta.SoundCueReference]
		public string StopPitchSound
		{
			get { return mStopPitchSound; }
			set { mStopPitchSound = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("name", ref mName);
			s.StreamStringOpt("yawattachment", ref mYawAttachment, toLower: false);
			s.StreamStringOpt("pitchattachment", ref mPitchAttachment, toLower: false);
			s.StreamAttributeOpt("autocenter", ref mAutoCenter, Predicates.IsFalse);
			s.StreamAttributeOpt("singleboneik", ref mSingleBoneIK, Predicates.IsTrue);
			s.StreamAttributeOpt("combined", ref mCombined, Predicates.IsTrue);
			s.StreamAttributeOpt("hardpitchlimits", ref mHardPitchLimits, Predicates.IsTrue);
			s.StreamAttributeOpt("relativeToUnit", ref mRelativeToUnit, Predicates.IsTrue);
			s.StreamAttributeOpt("useYawAndPitchAsTolerance", ref mUseYawAndPitchAsTolerance, Predicates.IsTrue);
			s.StreamAttributeOpt("infiniteRateWhenHasTarget", ref mInfiniteRateWhenHasTarget, Predicates.IsTrue);
			s.StreamAttributeOpt("yawrate", ref mYawRotationRate, x => x != cPiOver12InDegrees);
			s.StreamAttributeOpt("pitchrate", ref mPitchRotationRate, x => x != cPiOver12InDegrees);

			#region YawLeftMaxAngle and YawRightMaxAngle
			if (s.IsReading)
			{
				float yawMaxAngle = PhxUtil.kInvalidSingleNaN;
				if (s.ReadAttributeOpt("yawMaxAngle", ref yawMaxAngle) ||
					// #HACK fucking deal with original HW game data that was hand edited, but only when reading
					s.ReadAttributeOpt("yawmaxangle", ref yawMaxAngle))
				{
					mYawLeftMaxAngle = -yawMaxAngle;
					mYawRightMaxAngle = yawMaxAngle;
				}

				s.StreamAttributeOpt("YawLeftMaxAngle", ref mYawLeftMaxAngle, x => x != cDefaultYawLeftMaxAngle);
				s.StreamAttributeOpt("YawRightMaxAngle", ref mYawRightMaxAngle, x => x != cDefaultYawRightMaxAngle);
			}
			else if (s.IsWriting)
			{
				if (mYawLeftMaxAngle == cDefaultYawLeftMaxAngle &&
					mYawRightMaxAngle == cDefaultYawRightMaxAngle)
				{
					// don't stream anything
				}
				else if (System.Math.Abs(mYawLeftMaxAngle) == mYawRightMaxAngle)
				{
					s.WriteAttribute("yawMaxAngle", mYawRightMaxAngle);
				}
				else
				{
					s.StreamAttributeOpt("YawLeftMaxAngle", ref mYawLeftMaxAngle, x => x != cDefaultYawLeftMaxAngle);
					s.StreamAttributeOpt("YawRightMaxAngle", ref mYawRightMaxAngle, x => x != cDefaultYawRightMaxAngle);
				}
			}
			#endregion

			s.StreamAttributeOpt("pitchMaxAngle", ref mPitchMaxAngle, x => x != cDefaultPitchMaxAngle);
			s.StreamAttributeOpt("pitchMinAngle", ref mPitchMinAngle, x => x != cDefaultPitchMinAngle);

			s.StreamStringOpt("StartYawSound", ref mStartYawSound, toLower: false);
			s.StreamStringOpt("StopYawSound", ref mStopYawSound, toLower: false);
			s.StreamStringOpt("StartPitchSound", ref mStartPitchSound, toLower: false);
			s.StreamStringOpt("StopPitchSound", ref mStopPitchSound, toLower: false);
		}
		#endregion
	};
}