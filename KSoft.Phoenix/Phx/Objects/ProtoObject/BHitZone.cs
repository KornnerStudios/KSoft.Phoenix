
namespace KSoft.Phoenix.Phx
{
	public sealed class BHitZone
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new()
		{
			ElementName = "HitZone",
		};
		#endregion

		#region AttachmentName
		string? mAttachmentName;
		public string? AttachmentName
		{
			get { return mAttachmentName; }
			set { mAttachmentName = value; }
		}
		#endregion

		#region Hitpoints
		float mHitpoints = PhxUtil.kInvalidSingle;
		public float Hitpoints
		{
			get { return mHitpoints; }
			set { mHitpoints = value; }
		}
		#endregion

		#region Shieldpoints
		float mShieldpoints = PhxUtil.kInvalidSingle;
		public float Shieldpoints
		{
			get { return mShieldpoints; }
			set { mShieldpoints = value; }
		}
		#endregion

		#region Active
		float mActive;
		public float Active
		{
			get { return mActive; }
			set { mActive = value; }
		}
		#endregion

		#region HasShields
		float mHasShields;
		public float HasShields
		{
			get { return mHasShields; }
			set { mHasShields = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			string attachmentName = s.IsReading ? string.Empty : mAttachmentName ?? throw new System.ArgumentNullException(nameof(mAttachmentName));
			s.StreamCursor(ref attachmentName);
			mAttachmentName = attachmentName;
			s.StreamElementOpt("Hitpoints", ref mHitpoints, PhxPredicates.IsNotInvalid);
			s.StreamElementOpt("Shieldpoints", ref mShieldpoints, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}