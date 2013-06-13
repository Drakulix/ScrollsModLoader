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
	public class Player : ICommListener, IBattleModeUICallback
	{

		Thread replay = null;
		private volatile bool playing = false;
		private volatile bool readNextMsg = true;
		private volatile bool paused = false;

		//private String saveFolder;
		private String fileName;

		private BattleModeUI battleModeUI;
		private GameObject endGameButton;
		private Texture2D pauseButton;
		private Texture2D playButton;

		public Player (String saveFolder)
		{
			App.Communicator.addListener (this);
			playButton = new Texture2D(83, 131);
			playButton.LoadImage(System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("GameReplay.Mod.Play.png").ReadToEnd ());
			pauseButton = new Texture2D(83, 131);
			pauseButton.LoadImage(System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("GameReplay.Mod.Pause.png").ReadToEnd ());
			//this.saveFolder = saveFolder;
		}

		public static MethodDefinition[] GetPlayerHooks (TypeDefinitionCollection scrollsTypes, int version)
		{
			return new MethodDefinition[] { scrollsTypes["Communicator"].Methods.GetMethod("readNextMessage")[0],
											scrollsTypes["GUIBattleModeMenu"].Methods.GetMethod("toggleMenu")[0],
											scrollsTypes["BattleMode"].Methods.GetMethod("OnGUI")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("Start")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("Init")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("Raycast")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("ShowEndTurn")[0]
										   };
		}

		public bool BeforeInvoke (InvocationInfo info, out object returnValue)
		{

			switch ((String)info.TargetMethod ()) {
			case "OnGUI":
				{
					if (playing) {
						typeof(BattleMode).GetMethod ("deselectAllTiles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke (info.Target(), null);
					}
					returnValue = null;
					return false;

				}
			case "readNextMessage":
				{
					if (playing) {
						if (paused) {
							returnValue = true;
							return true;
						}
						if (readNextMsg == false) {
							returnValue = true;
							return true;
						} else {
							readNextMsg = false;
						}
					}
				}
				break;
			case "toggleMenu":
				{
					if (playing) { //quit on Esc/Back Arrow
						playing = false;
						App.Communicator.setData ("");
						//App.SceneValues.battleMode = new SceneValues.SV_BattleMode (false);
						//App.Communicator.isActive = true;
						SceneLoader.loadScene("_Lobby");
						returnValue = null;
						return true;
					}
				}
				break;
			case "ShowEndTurn":
			//case "ShowReconnectPopup":
				{
					if (playing) {
						returnValue = null;
						return true;
					}
				}
				break;
			}

			returnValue = null;
			return false;
		}

		public void handleMessage (Message msg)
		{
			if (playing && msg is NewEffectsMessage && msg.getRawText ().Contains ("EndGame")) {
				playing = false;
				App.Communicator.setData ("");
				//App.SceneValues.battleMode = new SceneValues.SV_BattleMode (false);
				//App.Communicator.isActive = true;
				//SceneLoader.loadScene("_HomeScreen");
			}
		}
		public void onReconnect ()
		{
			return;
		}

		public void LaunchReplay(String name) {
			fileName = name;
			replay = new Thread(new ThreadStart(LaunchReplay));
			replay.Start();
		}

		private void LaunchReplay() {
			playing = true;

			String log = File.ReadAllText (fileName);

			//App.SceneValues.battleMode = new SceneValues.SV_BattleMode (true);
			App.Communicator.isActive = false;
			SceneLoader.loadScene ("_BattleModeView");
			App.Communicator.setData (log);

			while (playing) {
				if (readNextMsg == false) {
					//delay messages otherwise the game rushes through in about a minute.
					Thread.Sleep (1500);
					while (paused) Thread.Sleep (1000);
					readNextMsg = true;
				}
			}

			// not working
			/*jsonms.runParsing();
			String line = jsonms.getNextMessage ();
			while (line != null) {
				try {
					Message msg = Message.createMessage (Message.getMessageName(line), line);
					//Console.WriteLine (msg.getRawText());
					typeof(Communicator).GetMethod ("preHandleMessage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (App.Communicator, new object[] { msg });
					foreach (ICommListener listener in listeners) {
						listener.handleMessage (msg);
					}
				} catch {}
				//Console.WriteLine (line);
				jsonms.runParsing();
				line = jsonms.getNextMessage ();
			}*/

		}

		public void AfterInvoke (InvocationInfo info, ref object returnValue)
		{
			switch (info.TargetMethod ()) {
			case "Start":
				{
					if (playing) {
						battleModeUI = (BattleModeUI)info.Target ();
						endGameButton = ((GameObject)typeof(BattleModeUI).GetField ("endTurnButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue (info.Target()));
						endGameButton.renderer.material.mainTexture = pauseButton;
						battleModeUI.StartCoroutine ("FadeInEndTurn");
					}
				}
				break;
			case "Init":
				{
					if (playing) {
						typeof(BattleModeUI).GetField("callback", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(info.Target(), this);
						App.ChatUI.SetEnabled (true);
						App.ChatUI.SetLocked (false);
						App.ChatUI.Show (false);
						App.ChatUI.SetCanOpenContextMenu (false);
						//activate chat on replays but disable profile or trading menus (wired bugs)
					}
				}
				break;
			case "Raycast":
				{
					if (playing && endGameButton.renderer.material.mainTexture != pauseButton && endGameButton.renderer.material.mainTexture != playButton) {
						//Console.WriteLine(endGameButton.renderer.material.mainTexture.height+" "+endGameButton.renderer.material.mainTexture.width);
						if (paused) {
							endGameButton.renderer.material.mainTexture = playButton;
						} else {
							endGameButton.renderer.material.mainTexture = pauseButton;
						}
					}
				}
				break;
			}
		}

		public bool allowEndTurn () {
			return true;
		}

		public void endturnPressed () {
			paused = !paused;
			if (paused) {
				endGameButton.renderer.material.mainTexture = playButton;
			} else {
				endGameButton.renderer.material.mainTexture = pauseButton;
			}
		}
	}
}

