using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScrollsModLoader.Interfaces;
//using LinFu.AOP.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

public class Logger : BaseMod, ICommListener
{
	private StreamWriter Log;
	private StreamWriter GameLog;
	private bool gameStart = false;

	public Logger ()
	{
		Console.WriteLine ("Logger loading");
		try {
			Log = File.CreateText ("network.log");
			GameLog = File.CreateText ("game.txt");
		} catch (IOException exp) {
			Console.WriteLine (exp);
		}
		Console.WriteLine ("Logger loaded");
	}

	~Logger()
	{
		Log.Flush();
		Log.Close();
	}
	
	#region implemented abstract members of BaseMod
	public override string GetName ()
	{
		return "Logger";
	}
	public override int GetVersion ()
	{
		return 0;
	}
	public override MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version)
	{
		return new MethodDefinition[] { scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[] {typeof(String)}), scrollsTypes["Communicator"].Methods.GetMethod("sendBattleRequest")[0] };
	}

	public override void Init() {
		App.Communicator.addListener (this);
	}
	public override bool BeforeInvoke (InvocationInfo info, out object returnValue)
	{
		if (info.TargetMethod().Equals("sendBattleRequest")) {
			GameLog.WriteLine (info.Arguments()[0]);
		}

		Log.WriteLine ("Client: "+info.Arguments()[0]);
		returnValue = null;
		return false;
	}
	public override void AfterInvoke (InvocationInfo info, ref object returnValue)
	{
		return;
	}
	
	#endregion

	#region ICommListener implementation

	public void handleMessage (Message msg)
	{
		if (gameStart) {
			GameLog.WriteLine (msg.getRawText());
		}

		if (!gameStart && msg.ToString().Contains("BattleRedirect"))
			gameStart = true;
		if (gameStart && msg.getRawText ().Contains ("EndGame")) {
			gameStart = false;
			GameLog.Flush ();
			GameLog.Close ();
		}

		Log.WriteLine ("Server:" +msg.getRawText());
	}


	public void onReconnect ()
	{
		return; //I don't care
	}

	#endregion
}

