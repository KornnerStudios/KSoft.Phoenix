using System;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	/// <summary>Various flags for <see cref="BCollectionParams"/></summary>
	[Flags]
	public enum BCollectionParamsFlags
	{
		ToLowerDataNames,

		kNumberOf
	};

	public abstract class BCollectionParams
	{
		public const int kDefaultCapacity = 16;

		/// <summary>For fine tuning the BDictionary initialization, to avoid reallocations</summary>
		public /*readonly*/ int InitialCapacity = kDefaultCapacity;

		#region Flags
		/// <summary>BCollectionParamsFlags</summary>
		public BitVector32 Flags;

		public bool ToLowerDataNames
		{
			get { return Flags.Test(BCollectionParamsFlags.ToLowerDataNames); }
			set { Flags.Set(BCollectionParamsFlags.ToLowerDataNames, value); }
		}
		#endregion

		protected BCollectionParams() {}
	};
}