using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Xmb.Test
{
	[TestClass]
	public sealed class BinaryDataTreeTests
		: BaseTestClass
	{
		struct UgxBdtReference
		{
			public long FileOffset;
			public long ChunkSize;
			public string FilePath;

			public UgxBdtReference(long offset, long chunkSize, string ugxPath)
			{
				FileOffset = offset;
				ChunkSize = chunkSize;
				FilePath = ugxPath;
			}
		};

		static UgxBdtReference[] UgxFiles = new UgxBdtReference[]
		{
			new UgxBdtReference(0x154A0, 0x476, @"C:\HaloWars\Alpha\art\unsc\infantry\odst_01\odst_01.ugx"),
			//new UgxBdtReference(0, 0x0, @""),
			new UgxBdtReference(0x2FD0, 0x3B0, @"M:\Ensemble\HaloWars2\HaloWars2_uwp_x64_store_1_1_57_0\data\units\unsc\actors\infantry\spartans\cs_spartan_alice\mesh_cs_spartan_alice.ugx"),
			new UgxBdtReference(0x29BB4, 0x9F8, @"M:\Ensemble\HaloWars2\HaloWars2_uwp_x64_store_1_1_57_0\data\maps\rostermode\evenflowart\ivy_dressing\childmesh_child_asset284.ugx"),
		};

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void BDT_ToXmlTest()
		{
			bool any_failed = false;

			foreach (var reference in UgxFiles)
			{
				using (var fs = File.OpenRead(reference.FilePath))
				{
					fs.Seek(reference.FileOffset, SeekOrigin.Begin);
					var bdt_bytes = new byte[reference.ChunkSize];
					int bytes_read = fs.Read(bdt_bytes, 0, bdt_bytes.Length);
					Assert.AreEqual(bdt_bytes.Length, bytes_read);

					try
					{
						var bdt = new BinaryDataTree();
						bdt.DecompileAttributesWithTypeData = true;
						using (var bdt_ms = new MemoryStream(bdt_bytes))
						using (var es = new IO.EndianStream(bdt_ms, Shell.EndianFormat.Big, permissions: FileAccess.Read))
						{
							es.StreamMode = FileAccess.Read;

							bdt.Serialize(es);
						}

						var ms = new MemoryStream();
						bdt.ToXml(ms);
						ms.Flush();

						ms.Position = 0;
						Console.WriteLine();
						Console.WriteLine();
						Console.WriteLine(reference.FilePath);
						using (var sr = new StreamReader(ms))
							Console.WriteLine(sr.ReadToEnd());

					} catch (Exception ex)
					{
						Console.WriteLine("Test failed on {0}:\n{1}",
							reference.FilePath, ex);

						any_failed = true;
					}
				}
			}

			Assert.IsFalse(any_failed);
		}
	};
}