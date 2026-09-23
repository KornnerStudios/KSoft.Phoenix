using System;
using System.IO;
using System.Text;
using KSoft.Phoenix;
using KSoft.Phoenix.Phx;
using KSoft.Phoenix.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Collections.Test;

[TestClass]
public sealed class TypedBBitSetTest
{
	enum Dense { First, Second }
	enum Sparse { First, Last = 3 }
	enum Alias { First, Same = First, Third = 2 }
	enum Bounded { First, Second, kNumberOf }
	enum Negative { None = -1, First }
	enum AmbiguousNames { First, FIRST }

	readonly Phoenix.HaloWars.BDatabase mDatabase = new(new Phoenix.Engine.PhxEngine());

	[TestMethod]
	public void PrototypeFlags_XmlNamesIncludingObsoleteAndHighBits_AreUnchanged()
	{
		var proto = new BProtoObject();
		var obsolete = Enum.Parse<BProtoObjectFlags>("NonSolid");
		Assert.IsGreaterThan(63, (int)BProtoObjectFlags.RenderBelowDecals);
		proto.Flags.Set(BProtoObjectFlags.AttackWhileCloaked);
		proto.Flags.Set(obsolete);
		proto.Flags.Set(BProtoObjectFlags.RenderBelowDecals);
		const string expected = "<Flags><Flag>AttackWhileCloaked</Flag><Flag>NonSolid</Flag><Flag>RenderBelowDecals</Flag></Flags>";

		Assert.AreEqual(expected, Write(proto.Flags, BBitSetXmlParams.kFlagsSansRoot));
		var read = new BProtoObject();
		Read(expected, read.Flags, BBitSetXmlParams.kFlagsSansRoot);
		Assert.IsTrue(read.Flags.Test(BProtoObjectFlags.AttackWhileCloaked));
		Assert.IsTrue(read.Flags.Test(obsolete));
		Assert.IsTrue(read.Flags.Test(BProtoObjectFlags.RenderBelowDecals));
		Assert.AreEqual(3, read.Flags.EnabledCount);
		Assert.AreEqual(Enum.GetValues<BProtoObjectFlags>().Length, read.Flags.RawBits.Length);

		Read("<Flags />", read.Flags, BBitSetXmlParams.kFlagsSansRoot);
		Assert.IsTrue(read.Flags.IsEmpty);
		read.Flags[BProtoObjectFlags.RenderBelowDecals] = true;
		Assert.AreEqual(1, read.Flags.EnabledCount);
		Assert.AreEqual("<Flags><Flag>RenderBelowDecals</Flag></Flags>", Write(read.Flags, BBitSetXmlParams.kFlagsSansRoot));
	}

	[TestMethod]
	public void LegacyDefaultsAndDynamicDomains_PreserveXmlBehavior()
	{
		var parameters = new BBitSetParams(() => new CodeEnum<Dense>())
		{
			kGetMemberDefaultValue = index => index == 1,
		};
		var defaults = new BBitSet(parameters);
		defaults[0] = true;
		defaults[1] = false;
		const string expected = "<Flags><First>true</First><Second>false</Second></Flags>";
		Assert.AreEqual(expected, Write(defaults, BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue));
		var read = new BBitSet(parameters);
		Read(expected, read, BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue);
		Assert.IsTrue(read[0]);
		Assert.IsFalse(read[1]);
		Read("<Flags />", read, BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue);
		Assert.IsFalse(read[0]);
		Assert.IsTrue(read[1]);

		var dynamicBits = new BBitSet(new BBitSetParams(db => db.GameObjectTypes), mDatabase);
		dynamicBits[0] = true;
		string firstName = mDatabase.GameObjectTypes.GetMemberName(0);
		Assert.AreEqual("<Flags><Flag>" + firstName + "</Flag></Flags>", Write(dynamicBits, BBitSetXmlParams.kFlagsSansRoot));
	}

	[TestMethod]
	[DataRow(Sparse.First)]
	[DataRow(Alias.First)]
	[DataRow(Bounded.First)]
	[DataRow(Negative.First)]
	[DataRow(AmbiguousNames.First)]
	public void Constructor_NonOrdinalCodeDomain_RejectsBeforeUse<TBits>(TBits marker) where TBits : struct, Enum
	{
		_ = marker;
		for (int i = 0; i < 2; i++)
		{
			Assert.ThrowsExactly<ArgumentException>(() => new BBitSet<TBits>());
		}
	}

	[TestMethod]
	public void TypedAccess_EmptyStorageAndInvalidValues_PreservesDomain()
	{
		var bits = new BBitSet<Dense>();
		bits.Set(Dense.First);
		Assert.IsTrue(bits.Test(Dense.First));
		bits.Clear();
		bits.OptimizeStorage();
		bits.Toggle(Dense.Second);
		Assert.IsTrue(bits[Dense.Second]);
		Assert.AreEqual(1, bits.EnabledCount);
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bits.Set((Dense)2));
	}

	string Write(BBitSetBase bits, BBitSetXmlParams parameters)
	{
		using var owner = BXmlSerializerInterface.GetNullInterface(mDatabase);
		using var stream = IO.XmlElementStream.CreateForWrite("Flags", owner);
		XmlUtil.Serialize(stream, bits, parameters);
		return stream.Document.DocumentElement!.OuterXml;
	}

	void Read(string xml, BBitSetBase bits, BBitSetXmlParams parameters)
	{
		using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
		using var owner = BXmlSerializerInterface.GetNullInterface(mDatabase);
		using var stream = new IO.XmlElementStream(input, FileAccess.Read, owner, "synthetic-flags.xml");
		stream.InitializeAtRootElement();
		XmlUtil.Serialize(stream, bits, parameters);
	}
}
