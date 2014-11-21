using System;
using System.Collections.Generic;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	using Phx = Phoenix.Phx;

	public sealed class BBitSetParams
	{
		/// <summary>Get the source IProtoEnum from a global object</summary>
		public readonly Func<IProtoEnum> kGetProtoEnum;
		/// <summary>Get the source IProtoEnum from an engine's main database</summary>
		public readonly Func<Phx.BDatabaseBase, IProtoEnum> kGetProtoEnumFromDB;

		public BBitSetParams(Func<Phx.BDatabaseBase, IProtoEnum> protoEnumGetter)
		{
			kGetProtoEnumFromDB = protoEnumGetter;
		}
		public BBitSetParams(Func<IProtoEnum> protoEnumGetter)
		{
			kGetProtoEnum = protoEnumGetter;
		}
	};
}