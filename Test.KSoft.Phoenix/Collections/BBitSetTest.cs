using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class BBitSetTest
{
	enum TestBits { First, Second }

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void Indexer_AllClearStorage_SetsFirstBit(bool optimize)
	{
		var bits = new BBitSet(new BBitSetParams(() => new CodeEnum<TestBits>()));
		if (optimize)
		{
			bits.OptimizeStorage();
		}

		bits[0] = true;

		Assert.IsTrue(bits[0]);
		Assert.IsFalse(bits[1]);
		Assert.AreEqual(1, bits.EnabledCount);
		Assert.AreEqual(2, bits.Count);
		bits.Clear();
		bits[1] = true;
		Assert.IsFalse(bits[0]);
		Assert.IsTrue(bits[1]);
		Assert.AreEqual(1, bits.EnabledCount);
		bits[1] = false;
		Assert.IsTrue(bits.IsEmpty);
		Assert.AreEqual(0, bits.EnabledCount);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public void Indexer_EmptySetFalseAndInvalidWrites_UsesNormalBounds(bool optimize)
	{
		var bits = new BBitSet(new BBitSetParams(() => new CodeEnum<TestBits>()));
		if (optimize)
		{
			bits.OptimizeStorage();
		}

		bits[0] = false;
		Assert.IsTrue(bits.IsEmpty);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits[-1] = true);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits[2] = true);
		Assert.IsTrue(bits.IsEmpty);
	}

	[TestMethod]
	public void Indexer_DefaultsAndStorageReinitialization_PreservesDefaultRules()
	{
		var bits = new BBitSet(new BBitSetParams(() => new CodeEnum<TestBits>())
		{
			kGetMemberDefaultValue = index => index == 1,
		});
		Assert.IsTrue(bits[1]);
		bits.Clear();
		bits[0] = true;
		Assert.IsTrue(bits[0]);
		Assert.IsFalse(bits[1]);

		bits.Clear();
		bits.OptimizeStorage();
		bits[0] = true;
		Assert.IsTrue(bits[0]);
		Assert.IsTrue(bits[1]);
		Assert.AreEqual(2, bits.EnabledCount);
	}

	[TestMethod]
	public void Indexer_UninitializedDatabaseDomain_ThrowsInsteadOfIgnoringWrite()
	{
		var bits = new BBitSet(new BBitSetParams(db => db.GameObjectTypes));

		Assert.ThrowsExactly<InvalidOperationException>(() => bits[0] = true);
		Assert.IsTrue(bits.IsEmpty);
	}

	[TestMethod]
	public void Indexer_InitializedDatabaseDomain_UsesExistingStorage()
	{
		var database = new Phoenix.HaloWars.BDatabase(new Phoenix.Engine.PhxEngine());
		var bits = new BBitSet(new BBitSetParams(db => db.GameObjectTypes), database);

		bits[0] = true;

		Assert.IsTrue(bits[0]);
		Assert.AreEqual(1, bits.EnabledCount);
		Assert.AreEqual(database.GameObjectTypes.MemberCount, bits.RawBits.Length);
		bits.Clear();
		bits[0] = true;
		Assert.IsTrue(bits[0]);
	}
}
