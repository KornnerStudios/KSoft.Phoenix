
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
		: ObjectModel.BasicViewModel
		, IO.ITagElementStringNameStreamable
	{
		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return mID; }
			private set { this.SetFieldVal(ref mID, value); }
		}
		#endregion

		#region Category
		// #NOTE this engine doesn't specifically limit the category values to the stuff in the enum, but, to reduce memory overhead, I am
		LocStringCategory mCategory = LocStringCategory.None;
		public LocStringCategory Category
		{
			get { return mCategory; }
			set { this.SetFieldEnum(ref mCategory, value); }
		}
		#endregion

		#region Scenario
		string mScenario;
		public string Scenario
		{
			get { return mScenario; }
			set { this.SetFieldObj(ref mScenario, value); }
		}
		#endregion

		#region IsSubtitle
		bool mIsSubtitle;
		public bool IsSubtitle
		{
			get { return mIsSubtitle; }
			set { this.SetFieldVal(ref mIsSubtitle, value); }
		}
		#endregion

		#region IsUpdate
		bool mIsUpdate;
		public bool IsUpdate
		{
			get { return mIsUpdate; }
			set { this.SetFieldVal(ref mIsUpdate, value); }
		}
		#endregion

		#region MouseKeyboardID
		int mMouseKeyboardID = TypeExtensions.kNone;
		public int MouseKeyboardID
		{
			get { return mMouseKeyboardID; }
			set { this.SetFieldVal(ref mMouseKeyboardID, value); }
		}
		#endregion

		#region OriginalID
		// this is a string because there are cases with "and" in them. eg:
		// "25045 and 23441"
		string mOriginalID;
		public string OriginalID
		{
			get { return mOriginalID; }
			set { this.SetFieldObj(ref mOriginalID, value); }
		}
		#endregion

		#region Text
		string mText;
		public string Text
		{
			get { return mText; }
			set { this.SetFieldObj(ref mText, value); }
		}
		#endregion

		public LocString()
		{
		}

		public LocString(int id)
		{
			mID = id;
		}

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
			if (s.IsReading || mText.IsNotNullOrEmpty())
				s.StreamCursor(ref mText);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("({0}) '{1}'",
				ID, Text ?? "");
		}
	};
}