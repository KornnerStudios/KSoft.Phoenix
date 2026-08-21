using System;

namespace KSoft.Phoenix.XML
{
	partial class XmlUtil
	{
		public static void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			Collections.BTypeNames list, BListXmlParams @params, bool forceNoRootElementStreaming = false)
			where TDoc : class
			where TCursor : class
		{
			ArgumentNullException.ThrowIfNull(s);
			ArgumentNullException.ThrowIfNull(list);
			ArgumentNullException.ThrowIfNull(@params);

			if (forceNoRootElementStreaming)
			{
				@params.SetForceNoRootElementStreaming(true);
			}

			using (var xs = new BTypeNamesXmlSerializer(@params, list))
			{
				xs.Serialize(s);
			}
			if (forceNoRootElementStreaming)
			{
				@params.SetForceNoRootElementStreaming(false);
			}
		}
	};

	internal class BTypeNamesXmlSerializer
		: BListXmlSerializerBase<string>
	{
		readonly BListXmlParams mParams;
		readonly Collections.BTypeNames mList;

		public override BListXmlParams Params => mParams;
		public override Collections.BListBase<string> List => mList;

		public BTypeNamesXmlSerializer(BListXmlParams @params, Collections.BTypeNames list)
		{
			ArgumentNullException.ThrowIfNull(@params);
			ArgumentNullException.ThrowIfNull(list);

			mParams = @params;
			mList = list;
		}

		#region IXmlElementStreamable Members
		protected override void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
		{
			string name = null;
			mParams.StreamDataName(s, ref name);

			mList.AddItem(name);
		}
		protected override void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, string name)
		{
			mParams.StreamDataName(s, ref name);
		}

		protected override void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
		{
			base.WriteNodes(s, xs);

			ProtoEnumUndefinedMembers.Write(s, mParams, mList.UndefinedInterface);
		}
		#endregion
	};
}