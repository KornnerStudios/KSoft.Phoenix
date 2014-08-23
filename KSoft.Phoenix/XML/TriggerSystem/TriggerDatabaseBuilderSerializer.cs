using Contracts = System.Diagnostics.Contracts;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Phoenix.XML
{
	sealed class TriggerDatabaseBuilderSerializer
		: BXmlSerializerInterface
	{
		Phx.BDatabaseBase mDatabase;
		internal override Phx.BDatabaseBase Database { get { return mDatabase; } }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		public TriggerDatabaseBuilderSerializer(Engine.PhxEngine phx)
		{
			Contract.Requires(phx != null);

			mDatabase = phx.Database;
			TriggerDb = phx.TriggerDb;
		}

		#region IDisposable Members
		public override void Dispose() {}
		#endregion

		static void WaitUntilComplete(System.Threading.Tasks.ParallelLoopResult result)
		{
			while (!result.IsCompleted)
				System.Threading.Thread.Sleep(500);
		}

		void ParseTriggerScript<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
//			s.SetSerializerInterface(this);

			var ts = new Phx.BTriggerSystem();
			ts.Serialize(s);
		}
		void ParseTriggerScriptSansSkrimishAI<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// This HW script has all the debug info stripped :o
			if (s.StreamName.EndsWith("skirmishai.triggerscript"))
				return;

			ParseTriggerScript(s);
		}
		void ParseScenarioScripts<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
//			s.SetSerializerInterface(this);

			foreach (var e in s.ElementsByName(Phx.BTriggerSystem.kXmlRootName))
			{
				using (s.EnterCursorBookmark(e))
					new Phx.BTriggerSystem().Serialize(s);
			}
		}

		void ParseTriggerScripts(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.TriggerScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TriggerScript),
				ParseTriggerScriptSansSkrimishAI, out result);

			WaitUntilComplete(result);

			ReadDataFilesAsync(Engine.ContentStorage.Update, Engine.GameDirectory.TriggerScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TriggerScript),
				ParseTriggerScript, out result);

			WaitUntilComplete(result);
		}
		void ParseAbilities(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.AbilityScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.Ability),
				ParseTriggerScript, out result);

			WaitUntilComplete(result);
		}
		void ParsePowers(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.PowerScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.Power),
				ParseTriggerScript, out result);

			WaitUntilComplete(result);
		}
		void ParseScenarios(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			ReadDataFilesAsync(Engine.ContentStorage.Game, Engine.GameDirectory.Scenario,
				"*.scn",
				ParseScenarioScripts, out result);

			WaitUntilComplete(result);
		}

		public void ParseScriptFiles()
		{
			var e = GameEngine;

			ParseTriggerScripts(e);
			ParseAbilities(e);
			ParsePowers(e);
			ParseScenarios(e);
		}
	};
}