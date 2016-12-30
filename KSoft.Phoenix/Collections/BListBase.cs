using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	/// <summary>Our base interface for lists of comparable objects</summary>
	/// <typeparam name="T">Comparable object's type</typeparam>
	public abstract class BListBase<T>
		: IEqualityComparer<BListBase<T>>
		, IEnumerable<T>
	{
		#region kValueEqualityComparer
		private static IEqualityComparer<T> gValueEqualityComparer;
		protected static IEqualityComparer<T> kValueEqualityComparer { get {
			if (gValueEqualityComparer == null)
				gValueEqualityComparer = EqualityComparer<T>.Default;

			return gValueEqualityComparer;
		} }
		#endregion

		#region kEqualityComparer
		protected sealed class _EqualityComparer
			: IEqualityComparer<BListBase<T>>
		{
			#region IEqualityComparer<BListBase<T>> Members
			public bool Equals(BListBase<T> x, BListBase<T> y)
			{
				bool equals = x.Count == y.Count;
				if (equals)
				{
					var comparer = kValueEqualityComparer;
					for (int i = 0; i < x.Count && equals; i++)
						equals &= comparer.Equals(x[i], y[i]);
				}

				return equals;
			}

			public int GetHashCode(BListBase<T> obj)
			{
				int hash = 0;
				var comparer = kValueEqualityComparer;
				foreach (var o in obj)
					hash ^= comparer.GetHashCode(o);

				return hash;
			}
			#endregion
		};
		private static _EqualityComparer gEqualityComparer;
		protected static _EqualityComparer kEqualityComparer { get {
			if (gEqualityComparer == null)
				gEqualityComparer = new _EqualityComparer();

			return gEqualityComparer;
		} }
		#endregion

		protected List<T> mList;

		/// <summary>Parameters that dictate the functionality of this list</summary>
		public BListParams Params { get; private set; }

		protected BListBase(int capacity = BCollectionParams.kDefaultCapacity)
		{
			mList = new List<T>(capacity);
		}
		protected BListBase(BListParams @params)
			: this(@params != null ? @params.InitialCapacity : BCollectionParams.kDefaultCapacity)
		{
			Contract.Requires<ArgumentNullException>(@params != null);

			Params = @params;
		}

		#region List interface
		public int Count { get { return mList.Count; } }

		internal int Capacity
		{
			get { return mList.Capacity; }
			set { mList.Capacity = value; }
		}

		public virtual T this[int index]
		{
			get { return mList[index]; }
			set { mList[index] = value; }
		}

		internal void AddItem(T item)
		{
			mList.Add(item);
		}

		#region IEnumerable<T> Members
		public List<T>.Enumerator GetEnumerator()
		{
			return mList.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return mList.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mList.GetEnumerator();
		}
		#endregion
		#endregion

		public bool IsEmpty { get { return Count == 0; } }
		internal void OptimizeStorage()
		{
			//if (Count == 0)
			//	mList = null;
			mList.TrimExcess();
		}

		#region IEqualityComparer<BListBase<T>> Members
		public bool Equals(BListBase<T> x, BListBase<T> y)
		{
			return kEqualityComparer.Equals(x, y);
		}

		public int GetHashCode(BListBase<T> obj)
		{
			return kEqualityComparer.GetHashCode(obj);
		}
		#endregion

		public void Sort()
		{
			mList.Sort();
		}
		public void Sort(IComparer<T> comparer)
		{
			mList.Sort(comparer);
		}
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			mList.Sort(index, count, comparer);
		}
		public void Sort(Comparison<T> comparison)
		{
			mList.Sort(comparison);
		}
	};
}