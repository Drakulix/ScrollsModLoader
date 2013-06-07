using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Mono.Cecil;
using UnityEngine;
using ScrollsModLoader.Interfaces;

namespace GameReplay.Mod
{
	public class Replay : BaseMod, ICommListener
	{

		public Replay ()
		{
		}

		public override string GetName ()
		{
			return "GameReplay";
		}

		public override int GetVersion ()
		{
			return 1;
		}

		public override void Init ()
		{
			App.Communicator.addListener (this);
		}


		public void handleMessage (Message msg)
		{
			if (msg is BattleRedirectMessage) {

			} else if (msg is EMEndGame) {

			}
		}

		public void onReconnect ()
		{
			return;
		}

		public override MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
			return new MethodDefinition[] { scrollsTypes["ChatUI"].Methods.GetMethod("SetAcceptTrades")[0] };
		}

		public override bool BeforeInvoke (InvocationInfo info, out object returnValue)
		{

			switch ((String)info.TargetMethod ()) {
			case "SetAcceptTrades":
				LaunchReplay ();
				break;
			}

			returnValue = null;
			return false;
		}


		public void LaunchReplay() {

			String log = File.ReadAllText ("gameLog.txt");

			App.SceneValues.battleMode = new SceneValues.SV_BattleMode (true);
			App.Communicator.isActive = false;
			SceneLoader.loadScene ("_BattleModeView");

			App.Communicator.setData (log);

			App.SceneValues.battleMode = new SceneValues.SV_BattleMode (false);
			App.Communicator.isActive = true;
		}

		public override void AfterInvoke (InvocationInfo info, ref object returnValue)
		{
		}
	}
}

