using System.IO;

using KSoft.Phoenix.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Runtime.Test;

[TestClass]
public sealed class BObjectTest
{
	sealed class ObjectWithId
	{
		public int Id;
	}

	[TestMethod]
	public void DefaultVisual_IsNull()
	{
		var value = new BObject();

		Assert.IsNull(value.Visual);
	}

	[TestMethod]
	public void StreamObjectId_InvalidIdClearsExistingReference()
	{
		using var buffer = new MemoryStream();
		ObjectWithId? value = null;

		using (var writer = new KSoft.IO.EndianStream(
			buffer, KSoft.Shell.EndianFormat.Big, permissions: FileAccess.Write))
		{
			writer.BaseStreamOwner = false;
			writer.StreamMode = FileAccess.Write;

			Assert.IsFalse(BSaveGame.StreamObjectId(
				writer, ref value,
				static () => new ObjectWithId(),
				static (obj, id) => obj.Id = id,
				static obj => obj.Id));
		}

		buffer.Position = 0;
		value = new ObjectWithId { Id = 7 };

		using (var reader = new KSoft.IO.EndianStream(
			buffer, KSoft.Shell.EndianFormat.Big, permissions: FileAccess.Read))
		{
			reader.StreamMode = FileAccess.Read;

			Assert.IsFalse(BSaveGame.StreamObjectId(
				reader, ref value,
				static () => new ObjectWithId(),
				static (obj, id) => obj.Id = id,
				static obj => obj.Id));
		}

		Assert.IsNull(value);
	}
}
