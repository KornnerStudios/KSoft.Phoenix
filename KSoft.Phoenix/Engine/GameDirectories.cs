using System;
using System.Collections.Generic;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.Engine
{
	public class GameDirectories
	{
		#region Art
		protected const string kArtPath = @"art\";
		#endregion
		#region Data
		protected const string kDataPath = @"data\";
		protected const string kAbilitiesPath = @"abilities\";
		protected const string kAIPath = @"ai\";
		protected const string kPowersPath = @"powers\";
		protected const string kTacticsPath = @"tactics\";
		protected const string kTriggerScriptsPath = @"triggerscripts\";
		#endregion
		protected const string kScenariosPath = @"Scenario\";
		protected const string kSoundPath = @"Sound\";
		protected const string kTalkingHeadsPath = @"video\talkingheads\";

		/*public*/ string RootDirectory { get; /*private*/ set; }
		/*public*/ string UpdateDirectory { get; /*private*/ set; }
		bool UpdateDirectoryIsValid { get { return UpdateDirectory != null; } }
		public bool UseTitleUpdates { get; set; }

		public GameDirectories(string root, string updateRoot = null)
		{
			RootDirectory = root;
			UpdateDirectory = updateRoot;
			UseTitleUpdates = true;

			// Leave some breadcrumbs for the programmer in the event that they're confused as why an update file isn't loading.
			if (!UpdateDirectoryIsValid)
				Debug.Trace.Engine.TraceInformation("GameDirectories: No matching update directory for '{0}'", updateRoot);

			Art = kArtPath;//Path.Combine(RootDirectory, kArtPath);

			Data = kDataPath;//Path.Combine(RootDirectory, kDataPath);
			Abilities = Path.Combine(Data, kAbilitiesPath);
			AI = Path.Combine(Data, kAIPath);
			Powers = Path.Combine(Data, kPowersPath);
			Tactics = Path.Combine(Data, kTacticsPath);
			TriggerScripts = Path.Combine(Data, kTriggerScriptsPath);

			Scenario = kScenariosPath;
			Sound = kSoundPath;
		}

		#region Art
		public virtual string Art { get; protected set; }
		#endregion
		#region Data
		public virtual string Data { get; protected set; }
		public virtual string Abilities { get; protected set; }
		public virtual string AI { get; protected set; }
		public virtual string Powers { get; protected set; }
		public virtual string Tactics { get; protected set; }
		public virtual string TriggerScripts { get; protected set; }
		#endregion
		public virtual string Scenario { get; protected set; }
		public virtual string Sound { get; protected set; }

		public string GetContentLocation(ContentStorage location)
		{
			switch (location)
			{
			case ContentStorage.Game: return RootDirectory;
			case ContentStorage.Update:
				return UpdateDirectoryIsValid ? UpdateDirectory : RootDirectory;

			default: throw new NotImplementedException();
			}
		}
		public string GetDirectory(GameDirectory dir)
		{
			switch (dir)
			{
			#region Art
			case GameDirectory.Art: return Art;
			#endregion
			#region Data
			case GameDirectory.Data: return Data;
			case GameDirectory.AbilityScripts: return Abilities;
			case GameDirectory.AIData: return AI;
			case GameDirectory.PowerScripts: return Powers;
			case GameDirectory.Tactics: return Tactics;
			case GameDirectory.TriggerScripts: return TriggerScripts;
			#endregion
			case GameDirectory.Scenario: return Scenario;
			case GameDirectory.Sound: return Sound;

			default: throw new NotImplementedException();
			}
		}
		public string GetAbsoluteDirectory(ContentStorage loc, GameDirectory gameDir)
		{
			string root = GetContentLocation(loc);
			string dir = GetDirectory(gameDir);
			return Path.Combine(root, dir);
		}

		bool TryGetFileImpl(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			file = null;

			string root = GetContentLocation(loc);
			string dir = GetDirectory(gameDir);
			string file_path = Path.Combine(root, dir, filename);
			if (!string.IsNullOrEmpty(ext))
				file_path += ext;

			return (file = new FileInfo(file_path)).Exists;
		}
		bool TryGetFileFromUpdateOrGame(GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			file = null;

			if (!UseTitleUpdates)
				return TryGetFileImpl(ContentStorage.Game, gameDir, filename, out file, ext);

			//////////////////////////////////////////////////////////////////////////
			// Try to get the file from the TU storage first
			string dir = GetDirectory(gameDir);
			string file_path = Path.Combine(dir, filename);
			if (!string.IsNullOrEmpty(ext))
				file_path += ext;

			string full_path;

			if (UpdateDirectoryIsValid)
			{
				full_path = Path.Combine(UpdateDirectory, file_path);
				file = new FileInfo(full_path);
			}

			//////////////////////////////////////////////////////////////////////////
			// No update file exists, fall back to regular game storage
			if (file == null || !file.Exists)
			{
				full_path = Path.Combine(RootDirectory, file_path);
				file = new FileInfo(full_path);
				return file.Exists;
			}
			return true;
		}
		public bool TryGetFile(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(filename));
			file = null;

			return loc == ContentStorage.UpdateOrGame
				? TryGetFileFromUpdateOrGame(gameDir, filename, out file, ext)
				: TryGetFileImpl(loc, gameDir, filename, out file, ext);
		}

		public IEnumerable<string> GetFiles(ContentStorage loc, GameDirectory gameDir, string searchPattern)
		{
			Contract.Requires(loc != ContentStorage.UpdateOrGame, "Must iterate storages separately");
			Contract.Requires(!string.IsNullOrEmpty(searchPattern));

			string dir = GetAbsoluteDirectory(loc, gameDir);

			return Directory.EnumerateFiles(dir, searchPattern);
		}
	};
}