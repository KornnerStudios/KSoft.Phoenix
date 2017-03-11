
namespace KSoft.Phoenix.Phx
{
	public enum LocStringCategory
	{
		None,

		Code,
		Techs,
		Squads,
		Powers,
		Abilities,
		Leaders,
		Objects,
		UI,
		Campaign,
		Cinematics,
		Skirmish,

		steamVersion,

		kNumberOf
	};

	public sealed class LocString
		: IO.ITagElementStringNameStreamable
	{
		#region ID
		int mID;
		public int ID
		{
			get { return mID; }
			set { mID = value; }
		}
		#endregion

		#region Category
		// #NOTE this engine doesn't specifically limit the category values to the stuff in the enum, but, to reduce memory overhead, I am
		LocStringCategory mCategory = LocStringCategory.None;
		public LocStringCategory Category
		{
			get { return mCategory; }
			set { mCategory = value; }
		}
		#endregion

		#region Scenario
		string mScenario;
		public string Scenario
		{
			get { return mScenario; }
			set { mScenario = value; }
		}
		#endregion

		#region IsSubtitle
		bool mIsSubtitle;
		public bool IsSubtitle
		{
			get { return mIsSubtitle; }
			set { mIsSubtitle = value; }
		}
		#endregion

		#region IsUpdate
		bool mIsUpdate;
		public bool IsUpdate
		{
			get { return mIsUpdate; }
			set { mIsUpdate = value; }
		}
		#endregion

		#region MouseKeyboardID
		int mMouseKeyboardID = TypeExtensions.kNone;
		public int MouseKeyboardID
		{
			get { return mMouseKeyboardID; }
			set { mMouseKeyboardID = value; }
		}
		#endregion

		#region OriginalID
		string mOriginalID;
		public string OriginalID
		{
			get { return mOriginalID; }
			set { mOriginalID = value; }
		}
		#endregion

		#region Text
		string mText;
		public string Text
		{
			get { return mText; }
			set { mText = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("_locID", ref mID);
			s.StreamAttributeEnumOpt("category", ref mCategory, e => e != LocStringCategory.None);
			s.StreamAttributeOpt("scenario", ref mScenario, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("subtitle", ref mIsSubtitle, Predicates.IsTrue);
			s.StreamAttributeOpt("Update", ref mIsUpdate, Predicates.IsTrue);
			s.StreamAttributeOpt("_mouseKeyboard", ref mMouseKeyboardID, Predicates.IsNotNone);
			s.StreamAttributeOpt("originally", ref mOriginalID, Predicates.IsNotNullOrEmpty);
			s.StreamCursor(ref mText);
		}
		#endregion
	};
}