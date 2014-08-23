using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	using Phx = Phoenix.Phx;

	public class BBitSet
	{
		Collections.BitSet mBits;

		public int Count { get { return IsEmpty ? 0 : mBits.Length; } }
		public int EnabledCount { get { return IsEmpty ? 0 : mBits.Cardinality; } }

		/// <summary>Parameters that dictate the functionality of this list</summary>
		public BBitSetParams Params { get; private set; }

		public BBitSet(BBitSetParams @params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);

			Params = @params;

			InitializeFromEnum(null);
		}

		public bool IsEmpty { get { return mBits == null; } }
		internal void OptimizeStorage()
		{
			if (EnabledCount == 0)
				mBits = null;
		}
		internal IProtoEnum InitializeFromEnum(Phx.BDatabaseBase db)
		{
			IProtoEnum penum = null;

			if (Params.kGetProtoEnum != null)	penum = Params.kGetProtoEnum();
			else if (db != null)				penum = Params.kGetProtoEnumFromDB(db);

			if(penum != null)
				mBits = new Collections.BitSet(penum.MemberCount);

			return penum;
		}

		public bool this[int bit_index]
		{
			get { return IsEmpty ? false : mBits[bit_index]; }
			set
			{
				if (!IsEmpty)
					mBits[bit_index] = value;
			}
		}
	};
}