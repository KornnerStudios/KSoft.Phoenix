using System;
using KSoft.PropertyChanged.SourceGeneration;

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

	public sealed partial class LocString
		: ObjectModel.BasicViewModel
		, IO.ITagElementStringNameStreamable
	{
		#region ID
		int mID = TypeExtensions.kNone;
		[GeneratedPropertyChanged(BackingField = nameof(mID))]
		public partial int ID { get; private set; }
		#endregion

		#region Category
		// #NOTE this engine doesn't specifically limit the category values to the stuff in the enum, but, to reduce memory overhead, I am
		LocStringCategory mCategory = LocStringCategory.None;
		[GeneratedPropertyChanged(BackingField = nameof(mCategory))]
		public partial LocStringCategory Category { get; set; }
		#endregion

		#region Scenario
		string? mScenario;
		[GeneratedPropertyChanged(BackingField = nameof(mScenario))]
		public partial string? Scenario { get; set; }
		#endregion

		#region IsSubtitle
		bool mIsSubtitle;
		[GeneratedPropertyChanged(BackingField = nameof(mIsSubtitle))]
		public partial bool IsSubtitle { get; set; }
		#endregion

		#region IsUpdate
		bool mIsUpdate;
		[GeneratedPropertyChanged(BackingField = nameof(mIsUpdate))]
		public partial bool IsUpdate { get; set; }
		#endregion

		#region MouseKeyboardID
		int mMouseKeyboardID = TypeExtensions.kNone;
		[GeneratedPropertyChanged(BackingField = nameof(mMouseKeyboardID))]
		public partial int MouseKeyboardID { get; set; }
		#endregion

		#region OriginalID
		// this is a string because there are cases with "and" in them. eg:
		// "25045 and 23441"
		string? mOriginalID;
		[GeneratedPropertyChanged(BackingField = nameof(mOriginalID))]
		public partial string? OriginalID { get; set; }
		#endregion

		#region Text
		string? mText;
		[GeneratedPropertyChanged(BackingField = nameof(mText))]
		public partial string? Text { get; set; }
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
			if (s.IsReading)
			{
				string text = mText ?? string.Empty;
				s.StreamCursor(ref text);
				mText = text;
			}
			else if (mText is { Length: > 0 })
			{
				string text = mText;
				s.StreamCursor(ref text);
				mText = text;
			}
		}
		#endregion

		public string ToString(IFormatProvider provider)
		{
			ArgumentNullException.ThrowIfNull(provider);

			return string.Create(provider, $"({ID}) '{Text ?? ""}'");
		}
		public override string ToString() => ToString(KSoft.Util.InvariantCultureInfo);
	};
}