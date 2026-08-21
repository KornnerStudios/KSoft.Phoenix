using System;

namespace KSoft.Collections
{
	using Phx = Phoenix.Phx;

	public sealed class BBitSet
	{
		Collections.BitSet mBits;

		/// <summary>Is this bitset void of any ON bits?</summary>
		public bool IsEmpty => mBits == null || mBits.IsAllClear;
		/// <summary>Number of bits in the set, both ON and OFF</summary>
		public int Count =>	IsEmpty ? 0 : mBits.Length;
		/// <summary>Number of bits in the set which are ON</summary>
		public int EnabledCount => IsEmpty ? 0 : mBits.Cardinality;

		public Collections.BitSet RawBits => mBits;

		/// <summary>Parameters that dictate the functionality of this list</summary>
		public BBitSetParams Params { get; private set; }

		public BBitSet(BBitSetParams @params, Phx.BDatabaseBase db = null)
		{
			ArgumentNullException.ThrowIfNull(@params);

			Params = @params;

			InitializeFromEnum(db);
		}

		public void Clear()
		{
			mBits?.Clear();
		}

		internal void OptimizeStorage()
		{
			if (EnabledCount == 0)
			{
				mBits = null;
			}
		}

		internal void Set(int bitIndex, bool value = true)
		{
			if (mBits == null)
			{
				InitializeFromEnum(null);

				if (mBits == null)
				{
					throw new InvalidOperationException("Can't use Set on BBitSet that requires BDatabase to initialize");
				}
			}

			mBits.Set(bitIndex, value);
		}

		internal IProtoEnum InitializeFromEnum(Phx.BDatabaseBase db)
		{
			IProtoEnum penum = null;

			if (Params.kGetProtoEnum != null)
			{
				penum = Params.kGetProtoEnum();
			}
			else if (db != null)
			{
				penum = Params.kGetProtoEnumFromDB(db);
			}

			if (penum != null)
			{
				if (mBits == null)
				{
					mBits = new Collections.BitSet(penum.MemberCount);
				}
				else
				{
					mBits.Clear();

					if (mBits.Length != penum.MemberCount)
					{
						mBits.Length = penum.MemberCount;
					}
				}

				InitializeDefaultValues(penum);
			}

			return penum;
		}

		private void InitializeDefaultValues(IProtoEnum penum)
		{
			if (Params.kGetMemberDefaultValue == null)
			{
				return;
			}

			for (int x = 0; x < penum.MemberCount; x++)
			{
				bool bitDefault = Params.kGetMemberDefaultValue(x);
				if (bitDefault)
				{
					mBits[x] = true;
				}
			}
		}

		public bool this[int bit_index]
		{
			get => IsEmpty ? false : mBits[bit_index];
			set
			{
				if (!IsEmpty)
				{
					mBits[bit_index] = value;
				}
			}
		}
	};
}