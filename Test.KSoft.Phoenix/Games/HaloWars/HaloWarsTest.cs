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
		const string kBaseRoot
			//= @"D:\HW\data\"
			= @"U:\Ensemble\HaloWarsData\work\"
			;
		const string kGameRoot
			= kBaseRoot
			//+ @"Release\"
			;
		const string kGameRootAlpha
			= kBaseRoot
			//+ @"phx_alpha\"
			;
		const string kUpdateRoot
			= kBaseRoot
			//+ @"phx_tu6\"
			;

		private void Load(PhxEngine engine)
		{
			bool success;

			success = engine.Preload();
			Assert.IsTrue(success, "Failed to preload");

			success = engine.Load();
			Assert.IsTrue(success, "Failed to load");
		}

		[TestMethod]
		public void HaloWars_LoadAlphaTest()
		{
			var hw = PhxEngine.CreateForHaloWarsAlpha(kGameRootAlpha);
			Load(hw);
		}

		[TestMethod]
		public void HaloWars_LoadTest()
		{
			var hw = PhxEngine.CreateForHaloWars(kGameRoot, kUpdateRoot);
			Load(hw);

			Console.WriteLine("English StringTable range stats:");
			var stats = hw.Database.EnglishStringTable.RangeStats;
			foreach (var stat in stats)
				Console.WriteLine(stat.Value);
		}

		[TestMethod]
		public void HaloWars_AppSaveTest()
		{
			var hw = PhxEngine.CreateForHaloWars(kGameRoot, kUpdateRoot);
			Load(hw);

			using (var s = IO.XmlElementStream.CreateForWrite("Serina", hw))
			{
				s.InitializeAtRootElement();
				s.StreamMode = FA.Write;

				hw.Database.Serialize(s);

				var xw_settings = new System.Xml.XmlWriterSettings();
				xw_settings.Indent = true;
				xw_settings.IndentChars = "\t";
				xw_settings.NewLineChars = "\n";
				using (var xw = System.Xml.XmlWriter.Create(kTestResultsPath + "Serina.xml", xw_settings))
				{
					s.Document.Save(xw);
				}
			}
		}
		[TestMethod]
		public void HaloWars_AppLoadTest()
		{
			var hw = PhxEngine.CreateForHaloWars(kGameRoot, kUpdateRoot);
			using (var s = new IO.XmlElementStream(kTestResultsPath + "Serina.xml", FA.Read))
			{
				s.InitializeAtRootElement();
				s.StreamMode = FA.Read;

				hw.Database.Serialize(s);
			}
		}

		[TestMethod]
		public void HaloWars_DumpSortedObjectDbIdsTest()
		{
			var hw = PhxEngine.CreateForHaloWars(kGameRoot, kUpdateRoot);
			bool success;

			success = hw.Load();
			Assert.IsTrue(success, "Failed to preload");

			var objs = new List<Phx.BProtoObject>(hw.Database.Objects);
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
				s.Document.Save(kTestResultsPath + "ObjectDBIDs.xml");
			}
		}

		[TestMethod]
		public void HaloWars_WwiseTest()
		{
			const string k_sound_table_xml = kGameRoot + @"data\soundtable.xml";
			const string k_sounds_path = @"D:\HW\test\";
			const string k_sounds_pck = @"C:\Mount\A\Xbox\Xbox360\Games\Halo Wars\sound\wwise_material\GeneratedSoundBanks\xbox360\sounds.pck";
//			const string k_output_file = kTestResultsPath + @"sounds_pck.xml";

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
				extractor.ExtractSounds(k_sounds_path, towav, s.Reader);
		}
	};
}