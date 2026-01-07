#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
	sealed class TriggerDatabaseBuilderSerializer
		: BXmlSerializerInterface
	{
		readonly Phx.BDatabaseBase mDatabase;
		internal override Phx.BDatabaseBase Database => mDatabase;

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
			{
				return;
			}

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
				{
					new Phx.BTriggerSystem().Serialize(s);
				}
			}
		}

		void ParseTriggerScripts(Engine.PhxEngine e)
		{
			ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.TriggerScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TriggerScript),
				ParseTriggerScriptSansSkrimishAI,
				out System.Threading.Tasks.ParallelLoopResult result);

			ReadDataFilesAsync(Engine.ContentStorage.Update, Engine.GameDirectory.TriggerScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TriggerScript),
				ParseTriggerScript,
				out result);
		}
		void ParseAbilities(Engine.PhxEngine e)
		{
			ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.AbilityScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.Ability),
				ParseTriggerScript,
				out System.Threading.Tasks.ParallelLoopResult result);
		}
		void ParsePowers(Engine.PhxEngine e)
		{
			ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.PowerScripts,
				Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.Power),
				ParseTriggerScript,
				out System.Threading.Tasks.ParallelLoopResult result);
		}
		void ParseScenarios(Engine.PhxEngine e)
		{
			ReadDataFilesAsync(Engine.ContentStorage.Game, Engine.GameDirectory.Scenario,
				"*.scn",
				ParseScenarioScripts,
				out System.Threading.Tasks.ParallelLoopResult result);
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