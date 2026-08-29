using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Xmb.Test;

[TestClass]
public sealed class BinaryDataTreeDecompilerTests
{
	[TestMethod]
	public void Decompile_NameValuesCountOverflow_StopsAtFollowingNameValue()
	{
		BinaryDataTreePackedNode[] packedNodes =
		[
			new()
			{
				ParentIndex = ushort.MaxValue,
				ChildNodeIndex = 1,
				ChildNodesCount = 1,
				NameValueOffset = 1,
				NameValuesCount = byte.MaxValue,
			},
			new()
			{
				ParentIndex = 0,
				NameValueOffset = 256,
				NameValuesCount = 1,
			},
		];
		var nameValues = new BinaryDataTreeNameValue[byte.MaxValue + 2];
		nameValues[byte.MaxValue].IsLastNameValue = true;
		nameValues[byte.MaxValue + 1].IsLastNameValue = true;
		var decompiler = new BinaryDataTreeDecompiler
		{
			PackedNodes = packedNodes,
			NameValues = nameValues,
			NameData = [0],
			ValueData = Array.Empty<byte>(),
		};

		decompiler.Decompile();

		var nodes = decompiler.Nodes;
		Assert.IsNotNull(nodes);
		Assert.HasCount(2, nodes);
		Assert.AreSame(nodes[0], nodes[1].Parent);

		var rootNameValues = nodes[0].NameValues;
		Assert.IsNotNull(rootNameValues);
		Assert.HasCount(byte.MaxValue, rootNameValues);

		var childNameValues = nodes[1].NameValues;
		Assert.IsNotNull(childNameValues);
		Assert.HasCount(1, childNameValues);
	}
}
