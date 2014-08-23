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
		const string kGameRoot = @"C:\Users\Sean\Downloads\HW\Release\";
		const string kGameRootAlpha = @"C:\Users\Sean\Downloads\HW\phx_alpha\";
		const string kUpdateRoot = @"C:\Users\Sean\Downloads\HW\phx_tu6\";

		[TestMethod]
		public void HaloWars_LoadAlphaTest()
		{
			var hw = PhxEngine.CreateForHaloWarsAlpha(kGameRootAlpha);
			hw.Load();
		}

		[TestMethod]
		public void HaloWars_LoadTest()
		{
			var hw = PhxEngine.CreateForHaloWars(kGameRoot, kUpdateRoot);
			hw.Load();
		}

		[TestMethod]
		public void HaloWars_AppSaveTest()
		{
			var hw = PhxEngine.CreateForHaloWars(kGameRoot, kUpdateRoot);
			hw.Load();

			using (var s = IO.XmlElementStream.CreateForWrite("Serina", hw))
			{
				s.InitializeAtRootElement();
				s.StreamMode = FA.Write;

				hw.Database.Serialize(s);
				s.Document.Save(kTestResultsPath + "Serina.xml");
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
			hw.Load();

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
						s.WriteAttributeOptOnTrue("id", obj.Id, Predicates.IsNotNone);
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