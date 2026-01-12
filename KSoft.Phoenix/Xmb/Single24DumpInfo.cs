using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSoft.Phoenix.Xmb
{
	public sealed class Single24DumpEntry
		: IO.ITagElementStringNameStreamable
	{
		public uint Single24Bits { get; set; }
		public float FloatValue { get; set; } = float.NaN;
		public string Description { get; set; }

		#region ITagElementTextStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("Bits", this, (x) => x.Single24Bits, numBase: NumeralBase.Hex);
			s.StreamAttributeOpt("Float", this, (x) => x.FloatValue);
			s.StreamAttributeOpt("Description", this, (x) => x.Description, Predicates.IsNotNullOrEmpty);
		}
		#endregion
	};

	public sealed class Single24DumpInfo
		: IO.ITagElementStringNameStreamable
	{
		public string SourcePath { get; set; }

		public List<Single24DumpEntry> Entries { get; } = new();

		public void AddEntry(uint single24Bits, float floatValue, string description)
		{
			Entries.Add(new Single24DumpEntry()
			{
				Single24Bits = single24Bits,
				FloatValue = floatValue,
				Description = description,
			});
		}

		#region ITagElementTextStreamable Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("SourcePath", this, (x) => x.SourcePath, Predicates.IsNotNullOrEmpty);

			s.StreamableElements("Data", Entries);
		}
		#endregion
	};
}
