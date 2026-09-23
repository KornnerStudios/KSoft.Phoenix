using System;

namespace KSoft.Collections
{
	using Phx = Phoenix.Phx;

	/// <summary>Shared reference-backed storage and initialization rules for Phoenix bit sets.</summary>
	public abstract class BBitSetBase
	{
		Collections.BitSet? mBits;

		/// <summary>Reports whether storage is absent or all its bits are clear.</summary>
		/// <remarks>This does not distinguish absent storage from an allocated all-clear set.</remarks>
		public bool IsEmpty => mBits == null || mBits.IsAllClear;
		/// <summary>Gets zero when empty or all-clear; otherwise gets the storage's logical bit length.</summary>
		public int Count =>	IsEmpty ? 0 : mBits!.Length;
		/// <summary>Gets the set-bit count, or zero when storage is absent.</summary>
		public int EnabledCount => IsEmpty ? 0 : mBits!.Cardinality;

		/// <summary>Gets the existing mutable storage, not a clone.</summary>
		/// <remarks>Despite the non-nullable signature, this can be null before a database-backed domain is initialized or after empty-storage optimization. <see cref="IsEmpty"/> does not distinguish those cases from allocated all-clear storage.</remarks>
		public Collections.BitSet RawBits => mBits!;

		/// <summary>Parameters that dictate the functionality of this list</summary>
		public BBitSetParams Params { get; private set; }

		protected BBitSetBase(BBitSetParams @params, Phx.BDatabaseBase? db = null)
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

		[return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(db))]
		internal IProtoEnum? InitializeFromEnum(Phx.BDatabaseBase? db)
		{
			IProtoEnum? penum = null;

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
					mBits![x] = true;
				}
			}
		}

		protected internal bool GetBit(int bitIndex) => IsEmpty ? false : mBits![bitIndex];
	};

	public sealed class BBitSet : BBitSetBase
	{
		public BBitSet(BBitSetParams @params, Phx.BDatabaseBase? db = null) : base(@params, db) { }

		/// <summary>Read or update a bit, initializing code-enum storage when needed.</summary>
		/// <exception cref="InvalidOperationException">A write requires database-defined storage that has not been initialized.</exception>
		public bool this[int bit_index]
		{
			get => GetBit(bit_index);
			set => Set(bit_index, value);
		}
	};
}