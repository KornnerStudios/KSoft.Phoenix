using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Engine.Test
{
	[TestClass]
	public sealed class HaloWarsTest
		: BaseTestClass
	{
		private string? HaloWarsAlphaRoot =>
			TestContext.Properties[nameof(HaloWarsAlphaRoot)]?.ToString();
		private string? HaloWarsGameRoot =>
			TestContext.Properties[nameof(HaloWarsGameRoot)]?.ToString();
		private string? HaloWarsUpdateRoot =>
			TestContext.Properties[nameof(HaloWarsUpdateRoot)]?.ToString();

		private string TestRunResultsDirectory
		{
			get
			{
				var testRunResultsDirectory = TestContext.TestRunResultsDirectory;
				Assert.IsNotNull(testRunResultsDirectory);
				return testRunResultsDirectory;
			}
		}

		private void Load(PhxEngine engine)
		{
			bool success;

			success = engine.Preload();
			Assert.IsTrue(success, "Failed to preload");

			success = engine.Load();
			Assert.IsTrue(success, "Failed to load");
		}

		private PhxEngine CreateWithGameAndUpdateRoot()
		{
			var gameRoot = HaloWarsGameRoot;
			var updateRoot = HaloWarsUpdateRoot;
			Assert.IsTrue(System.IO.Directory.Exists(gameRoot),
				$"Directory does not exist: {gameRoot}");
			Assert.IsTrue(System.IO.Directory.Exists(updateRoot),
				$"Directory does not exist: {updateRoot}");
			Assert.IsNotNull(gameRoot);
			Assert.IsNotNull(updateRoot);

			var hw = PhxEngine.CreateForHaloWars(gameRoot, updateRoot);
			return hw;
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void HaloWars_LoadAlphaTest()
		{
			var alphaRoot = HaloWarsAlphaRoot;
			Assert.IsTrue(System.IO.Directory.Exists(alphaRoot),
				$"Directory does not exist: {alphaRoot}");
			Assert.IsNotNull(alphaRoot);

			var hw = PhxEngine.CreateForHaloWarsAlpha(alphaRoot);
			Load(hw);
		}

		// Break glass in case of emergency only:
		// I added this to stress test async loading issues that I at first thought
		// were somehow caused by non-ThreadStatic data being used.
		//[TestMethod]
		public void HaloWars_TestThreading()
		{
			int iterations = 10;
			for (int x = 0; x < iterations; x++)
			{
				var thread2 = new System.Threading.Thread(HaloWars_DumpSortedObjectDbIdsTest);
				var thread3 = new System.Threading.Thread(HaloWars_LoadTest);

				thread2.Start();
				thread3.Start();

				thread2.Join();
				thread3.Join();
			}
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void HaloWars_LoadTest()
		{
			PhxEngine hw = CreateWithGameAndUpdateRoot();
			Load(hw);
			var database = hw.Database;
			Assert.IsNotNull(database);

			Console.WriteLine("English StringTable range stats:");
			var stats = database.EnglishStringTable.RangeStats;
			foreach (var stat in stats)
			{
				Console.WriteLine(stat.Value);
			}
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void HaloWars_App_Step1SaveTest()
		{
			PhxEngine hw = CreateWithGameAndUpdateRoot();
			Load(hw);
			var database = hw.Database;
			Assert.IsNotNull(database);

			using (var s = IO.XmlElementStream.CreateForWrite("Serina", hw))
			{
				s.InitializeAtRootElement();
				s.StreamMode = FA.Write;

				database.Serialize(s);

				var xw_settings = new System.Xml.XmlWriterSettings
				{
					Indent = true,
					IndentChars = "\t",
					NewLineChars = "\n"
				};
				string output_path = System.IO.Path.Combine(TestRunResultsDirectory, "Serina.xml");
				Console.WriteLine("Saving to: {0}", output_path);
				using (var xw = System.Xml.XmlWriter.Create(output_path, xw_settings))
				{
					s.Document.Save(xw);
				}
				TestContext.AddResultFile(output_path);
			}
		}
		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void HaloWars_App_Step2LoadTest()
		{
			string input_path = System.IO.Path.Combine(TestRunResultsDirectory, "Serina.xml");

			// Requires HaloWars_App_Step1SaveTest to run first
			Assert.IsTrue(System.IO.File.Exists(input_path),
				$"Input file does not exist: {input_path}");

			PhxEngine hw = CreateWithGameAndUpdateRoot();
			var database = hw.Database;
			Assert.IsNotNull(database);

			Console.WriteLine("Reading from: {0}", input_path);
			using (var s = new IO.XmlElementStream(input_path, FA.Read))
			{
				s.InitializeAtRootElement();
				s.StreamMode = FA.Read;

				database.Serialize(s);
			}
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void HaloWars_DumpSortedObjectDbIdsTest()
		{
			PhxEngine hw = CreateWithGameAndUpdateRoot();
			Load(hw);
			var database = hw.Database;
			Assert.IsNotNull(database);

			var objs = new List<Phx.BProtoObject>(database.Objects);
			objs.Sort((x, y) => x.DbId - y.DbId);

			using (var s = IO.XmlElementStream.CreateForWrite("ObjectDBIDs"))
			{
				s.InitializeAtRootElement();
				s.StreamMode = FA.Write;

				foreach (var obj in objs)
				{
					using (s.EnterCursorBookmark("Object"))
					{
						s.WriteAttribute("dbid", obj.DbId);
						s.WriteAttribute("name", obj.Name);
						s.WriteAttributeOptOnTrue("is", obj.UnusedIs, Predicates.IsNotNone);
						s.WriteAttributeOptOnTrue("id", obj.UnusedId, Predicates.IsNotNone);
					}
				}

				string output_path = System.IO.Path.Combine(TestRunResultsDirectory, "ObjectDBIDs.xml");
				Console.WriteLine("Saving to: {0}", output_path);
				s.Document.Save(output_path);
				TestContext.AddResultFile(output_path);
			}
		}

		[TestMethod]
		[TestCategory("ExcludedFromAppveyor")]
		public void HaloWars_WwiseTest()
		{
			var gameRoot = HaloWarsGameRoot;
			Assert.IsNotNull(gameRoot);
			string k_sound_table_xml = System.IO.Path.Combine(gameRoot, @"data\soundtable.xml");
			const string k_sounds_path = @"D:\HW\test\";
			const string k_sounds_pck = @"C:\Mount\A\Xbox\Xbox360\Games\Halo Wars\sound\wwise_material\GeneratedSoundBanks\xbox360\sounds.pck";
//			const string k_output_file = kTestResultsPath + @"sounds_pck.xml";

			Assert.IsTrue(System.IO.File.Exists(k_sound_table_xml),
				$"Input file does not exist: {k_sound_table_xml}");
			Assert.IsTrue(System.IO.File.Exists(k_sounds_pck),
				$"Input file does not exist: {k_sounds_pck}");

			var sound_table = new Phx.BSoundTable();
			using (var s = new IO.XmlElementStream(k_sound_table_xml, FA.Read))
			{
				s.StreamMode = FA.Read;
				s.InitializeAtRootElement();
				sound_table.Serialize(s);
			}

			var pck_settings = new Wwise.FilePackage.AkFilePackageSettings()
			{
				Platform = Shell.Platform.Xbox360,
				SdkVersion = Wwise.AkVersion.k2009.Id,
				UseAsciiStrings = false,
			};
			var pck = new Wwise.FilePackage.AkFilePackage(pck_settings);

			using (var fs = System.IO.File.OpenRead(k_sounds_pck))
			using (var s = new IO.EndianStream(fs, Shell.EndianFormat.Big))
			{
				s.StreamMode = FA.Read;
				pck.Serialize(s);
				pck.SerializeSoundBanks(s);
			}

			var extractor = new Wwise.FilePackage.AkFilePackageExtractor(k_sounds_pck, pck, sound_table.EventsMap);
			extractor.PrepareForExtraction();

#if false
			using (var s = IO.XmlElementStream.CreateForWrite("soundsPack"))
			{
				pck.Serialize(s);

				s.Document.Save(k_output_file);
			}
#endif

			using (var fs = System.IO.File.OpenRead(k_sounds_pck))
			using (var s = new IO.EndianStream(fs, Shell.EndianFormat.Big))
			using (var towav = new System.IO.StreamWriter(k_sounds_path + "towav.bat"))
			{
				extractor.ExtractSounds(k_sounds_path, towav, s.Reader);
			}
		}
	};
}
