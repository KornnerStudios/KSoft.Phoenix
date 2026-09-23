using KSoft.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Xmb.Test;

[TestClass]
public sealed class XmbFileBuilderOptionsTest
{
	[TestMethod]
	public void TypedOptions_DefaultsAndCommandLine_PreserveExistingText()
	{
		var builder = new XmbFileBuilder();
		Assert.IsTrue(builder.AllowUnicode);
		Assert.IsFalse(builder.ForceStringVariants);
		Assert.IsFalse(builder.ForceUnicode);
		Assert.AreEqual("AllowUnicode", builder.DebugBuilderOptions);
		Assert.AreEqual("XMLCOMP -file fixture.xml", builder.GetCreatorToolCommandLine("fixture.xml"));

		builder.BuilderOptions = builder.BuilderOptions
			.With(XmbFileBuilderOptions.ForceStringVariants)
			.With(XmbFileBuilderOptions.ForceUnicode)
			.With(XmbFileBuilderOptions.LittleEndian);

		Assert.AreEqual("ForceStringVariants,AllowUnicode,ForceUnicode,LittleEndian", builder.DebugBuilderOptions);
		Assert.AreEqual("XMLCOMP -file fixture.xml -littleEndian -disableNumerics -forceUnicode",
			builder.GetCreatorToolCommandLine("fixture.xml"));
		builder.BuilderOptions = BitVector32<XmbFileBuilderOptions>.FromRaw(new BitVector32(1 << 20));
		Assert.AreEqual(string.Empty, builder.DebugBuilderOptions);
		Assert.AreEqual(1 << 20, builder.BuilderOptions.ToRaw().Data);
	}
}
