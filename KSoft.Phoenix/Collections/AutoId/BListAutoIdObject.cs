using System.ComponentModel;

namespace KSoft.Collections
{
	public abstract partial class BListAutoIdObject
		: ObjectModel.BasicViewModel
		, IListAutoIdObject
	{
		private string mName;
		[Browsable(false)]
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(
			BackingField = nameof(mName),
			DependentProperties = new[] { nameof(IListAutoIdObject.Data) })]
		public partial string Name { get; protected set; }

		protected BListAutoIdObject()
		{
			mName = Phoenix.Phx.BDatabaseBase.kInvalidString;
		}

		#region IListAutoIdObject Members
		private int mAutoId;
		[KSoft.PropertyChanged.SourceGeneration.GeneratedPropertyChanged(BackingField = nameof(mAutoId))]
		public partial int AutoId { get; set; }

		string IListAutoIdObject.Data
		{
			get { return mName; }
			set { Name = value; }
		}

		public abstract void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		#endregion

		public override string ToString() { return mName; }
	};
}