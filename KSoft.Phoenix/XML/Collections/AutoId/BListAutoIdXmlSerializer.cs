using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static IBListAutoIdXmlSerializer CreateXmlSerializer<T>(Collections.BListAutoId<T> list, BListXmlParams @params)
			where T : class, Collections.IListAutoIdObject, new()
		{
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			var xs = new BListAutoIdXmlSerializer<T>(@params, list);

			return xs;
		}

		public static void Serialize<T, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BListAutoId<T> list, BListXmlParams @params, bool forceNoRootElementStreaming = false)
			where T : class, Collections.IListAutoIdObject, new()
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(@params != null);

			if (forceNoRootElementStreaming) @params.SetForceNoRootElementStreaming(true);
			using (var xs = CreateXmlSerializer(list, @params))
			{
				xs.Serialize(s);
			}
			if (forceNoRootElementStreaming) @params.SetForceNoRootElementStreaming(false);
		}

		public static void SerializePreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			IBListAutoIdXmlSerializer xs, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(xs != null);

			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(true);
			xs.StreamPreload(s);
			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(false);
		}
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			IBListAutoIdXmlSerializer xs, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(xs != null);

			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(true);
			xs.Serialize(s);
			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(false);
		}
		public static void SerializeUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			IBListAutoIdXmlSerializer xs, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(xs != null);

			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(true);
			xs.StreamUpdate(s);
			if (forceNoRootElementStreaming) xs.Params.SetForceNoRootElementStreaming(false);
		}
	};

	internal sealed class BListAutoIdXmlSerializer<T>
		: BListXmlSerializerBase<T>
		, IBListAutoIdXmlSerializer
		where T : class, Collections.IListAutoIdObject, new()
	{
		BListXmlParams mParams;
		Collections.BListAutoId<T> mList;

		public override BListXmlParams Params { get { return mParams; } }
		public override Collections.BListBase<T> List { get { return mList; } }

		public BListAutoIdXmlSerializer(BListXmlParams @params, Collections.BListAutoId<T> list)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
			Contract.Requires<ArgumentNullException>(list != null);

			mParams = @params;
			mList = list;
		}

		bool mIsPreloaded;
		bool RequiresDataNamePreloading { get { return Params.RequiresDataNamePreloading; } }

		int mCountBeforeUpdate;
		bool mIsUpdating;

		#region Database interfaces
		bool SetupItem(out T item, string item_name, int iteration)
		{
			bool stream_item = !RequiresDataNamePreloading || (RequiresDataNamePreloading && mIsPreloaded);

			if (mIsUpdating)
			{
				// The update system in HW is fucked...just because the "update" attribute is true or left out, doesn't mean the value existed before or is not a new value
				// So just try
				int idx = mList.TryGetMemberId(item_name);
				if (idx.IsNotNone())
				{
					item = mList[idx];
					return stream_item;
				}

				iteration += mCountBeforeUpdate;
			}

			if (RequiresDataNamePreloading && mIsPreloaded)
			{
				item = mList[iteration];
				return stream_item;
			}

			mList.DynamicAdd(item = new T(), item_name, iteration);

			return stream_item;
		}
		#endregion

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			string item_name = null;
			Params.StreamDataName(s, ref item_name);

			T item;
			if (SetupItem(out item, item_name, iteration))
				item.Serialize(s);
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
		{
			string item_name = data.Data;
			if (item_name != null)
				Params.StreamDataName(s, ref item_name);

			try
			{
				data.Serialize(s);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format("Failed to write {0}", item_name),
					ex);
			}
		}
		protected override void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			base.WriteNodes(s, xs);

			ProtoEnumUndefinedMembers.Write(s, mParams, mList.UndefinedInterface);
		}

		void Preload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mIsPreloaded = false;

			Serialize(s);

			mIsPreloaded = true;
		}
		public void StreamPreload<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			Preload(s);
		}
		public void StreamUpdate<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			mIsUpdating = true;
			mCountBeforeUpdate = mList.Count;

			if (RequiresDataNamePreloading)
				Preload(s);
			Serialize(s);

			mIsUpdating = false;
			//mCountBeforeUpdate = 0;
		}
		#endregion
	};
}