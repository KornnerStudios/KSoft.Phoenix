using KSoft.Phoenix.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Runtime.Test;

[TestClass]
public sealed class BObjectTest
{
	[TestMethod]
	public void DefaultVisual_IsNull()
	{
		var value = new BObject();

		Assert.IsNull(value.Visual);
	}
}
