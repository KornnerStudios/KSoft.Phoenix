using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public abstract class BListAutoIdObject
		: IListAutoIdObject
	{
		protected string mName;
		public string Name { get { return mName; } }

		protected BListAutoIdObject()
		{
			mName = Phoenix.Phx.BDatabaseBase.kInvalidString;
		}

		#region IListAutoIdObject Members
		public int AutoId { get; set; }

		string IListAutoIdObject.Data
		{
			get { return mName; }
			set { mName = value; }
		}

		public abstract void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		#endregion

		public override string ToString() { return mName; }
	};
}