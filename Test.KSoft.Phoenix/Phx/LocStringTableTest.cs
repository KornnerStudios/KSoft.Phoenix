using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Phx.Test
{
	[TestClass]
	public sealed class LocStringTableTest
		: BaseTestClass
	{
		[TestMethod]
		public void LocStringTable_IndexRangesTest()
		{
			var ranges = LocStringTable.IndexRanges;
			ranges = null;

			var st = new LocStringTable();
			var stats = st.RangeStats;
			foreach (var stat in stats)
				Console.WriteLine(stat.Value);
		}

		[TestMethod]
		public void LocStringTable_NextFreeIdTest()
		{
			var ranges = LocStringTable.IndexRanges;
			Assert.AreEqual("code", ranges.ReservedFor);
			var range = ranges.SubRanges[0];
			Assert.AreEqual("unused1", range.ReservedFor);

			var st = new LocStringTable();
			const int kExpectedFreeId = 5;
			// populate everything but the 5th and last ID
			for (int x = 0; x <= (range.EndIndex - 1); x++)
			{
				if (x == kExpectedFreeId)
					continue;

				var str = new Phx.LocString(x);
				st.Add(str);
			}

			// add the 5th ID
			{
				int actual_free_id = st.NextFreeId(range);
				Assert.AreEqual(kExpectedFreeId, actual_free_id);

				var free_str = new Phx.LocString(actual_free_id);
				st.Add(free_str);
			}

			// add the last ID
			{
				int next_free_id = st.NextFreeId(range);
				Assert.AreEqual(range.EndIndex, next_free_id);

				var free_str = new Phx.LocString(next_free_id);
				st.Add(free_str);
			}

			// there should be no more IDs free
			{
				int next_free_id = st.NextFreeId(range);
				Assert.AreEqual(TypeExtensions.kNone, next_free_id);
			}

			// remove the 5th ID and make sure it is the next free id again
			{
				int index = st.GetLocStringIndex(kExpectedFreeId);
				st.RemoveAt(index);

				int actual_free_id = st.NextFreeId(range);
				Assert.AreEqual(kExpectedFreeId, actual_free_id);
			}

			var stats = st.RangeStats;
			foreach (var stat in stats)
				Console.WriteLine(stat.Value);
		}
	};
}