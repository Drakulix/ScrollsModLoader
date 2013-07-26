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

		public Player(String saveFolder)
		{
			App.Communicator.addListener(this);
			playButton = new Texture2D(83, 131);
			playButton.LoadImage(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("GameReplay.Mod.Play.png").ReadToEnd());
			pauseButton = new Texture2D(83, 131);
			pauseButton.LoadImage(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("GameReplay.Mod.Pause.png").ReadToEnd());
			//this.saveFolder = saveFolder;
		}

		public static MethodDefinition[] GetPlayerHooks(TypeDefinitionCollection scrollsTypes, int version)
		{
			return new MethodDefinition[] { scrollsTypes["Communicator"].Methods.GetMethod("handleNextMessage")[0],
											scrollsTypes["GUIBattleModeMenu"].Methods.GetMethod("toggleMenu")[0],
											scrollsTypes["BattleMode"].Methods.GetMethod("OnGUI")[0],
											scrollsTypes["BattleMode"].Methods.GetMethod("runEffect")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("Start")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("Init")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("Raycast")[0],
											scrollsTypes["BattleModeUI"].Methods.GetMethod("ShowEndTurn")[0]
										   };
		}

		public bool WantsToReplace (InvocationInfo info)
		{
			if (!playing)
				return false;

			switch ((String)info.targetMethod)
			{
				case "runEffect":
				{
					return paused;
				}
				case "handleNextMessage":
				{
					return paused | !readNextMsg;
				}
				case "ShowEndTurn":
				{
					return true;
				}
			}
			return false;
		}

		public void ReplaceMethod (InvocationInfo info, out object returnValue) {
			switch ((String)info.targetMethod) {
				case "ShowEndTurn":
				case "runEffect":
					returnValue = null;
					return;
				case "handleNextMessage":
					returnValue = true;
					return;
			}
		}

		public void BeforeInvoke(InvocationInfo info)
		{

			switch ((String)info.targetMethod)
			{
				case "OnGUI":
					{
						if (playing) {
							typeof(BattleMode).GetMethod ("deselectAllTiles", BindingFlags.Instance | BindingFlags.NonPublic).Invoke (info.target, null);
						}
					}
					break;
				case "handleNextMessage":
					{
						if (playing && readNextMsg != false)
						{
							readNextMsg = false;
						}
					}
					break;
				case "toggleMenu":
					{
						if (playing)
						{ //quit on Esc/Back Arrow
							playing = false;
							App.Communicator.setData("");
							SceneLoader.loadScene("_Lobby");
						}
					}
					break;
			}
		}

		public void handleMessage(Message msg)
		{
			if (playing && msg is NewEffectsMessage && msg.getRawText().Contains("EndGame"))
			{
				playing = false;
				App.Communicator.setData("");
			}
		}
		public void onReconnect()
		{
			return;
		}

		public void LaunchReplay(String name)
		{
			if (name == null || name.Equals (""))
				return;

			fileName = name;
			replay = new Thread(new ThreadStart(LaunchReplay));
			replay.Start();
		}

		private void LaunchReplay()
		{
			playing = true;

			String log = File.ReadAllText (fileName).Split(new char[] {'}'}, 2)[1];

			//FIX Profile ID
			JsonMessageSplitter jsonms = new JsonMessageSplitter();
			jsonms.feed(log);
			jsonms.runParsing();
			String line = jsonms.getNextMessage();
			String idWhite = null;
			String idBlack = null;
			String realID = null;

			while (line != null) {
				try {
					Message msg = Message.createMessage (Message.getMessageName (line), line);
					if (msg is GameInfoMessage) {
						idWhite = (msg as GameInfoMessage).getPlayerProfileId (TileColor.white);
						idBlack = (msg as GameInfoMessage).getPlayerProfileId (TileColor.black);
					}
					if (msg is NewEffectsMessage) {
						if (msg.getRawText ().Contains (idWhite)) {
							realID = idWhite;
						}
						if (msg.getRawText ().Contains (idBlack)) {
							realID = idBlack;
						}
					}
					if (msg is PingMessage) {
						log.Replace(msg.getRawText(), "");
					}
				} catch {
				}
				jsonms.runParsing();
				line = jsonms.getNextMessage();
			}

			if (realID != null) {
				log = log.Replace (realID, App.MyProfile.ProfileInfo.id);
			}

			SceneLoader.loadScene("_BattleModeView");
			App.Communicator.setData(log);

			while (playing)
			{
				if (readNextMsg == false)
				{
					//delay messages otherwise the game rushes through in about a minute.
					Thread.Sleep(2000);
					while (paused)
					{
						Thread.Sleep(1000);
					}
					readNextMsg = true;
				}
			}

		}

		public void AfterInvoke(InvocationInfo info, ref object returnValue)
		{
			switch (info.targetMethod)
			{
				case "Start":
					{
						if (playing)
						{
							battleModeUI = (BattleModeUI)info.target;
							endGameButton = ((GameObject)typeof(BattleModeUI).GetField("endTurnButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(info.target));
							endGameButton.renderer.material.mainTexture = pauseButton;
							battleModeUI.StartCoroutine("FadeInEndTurn");
						}
					}
					break;
				case "Init":
					{
						if (playing)
						{
							typeof(BattleModeUI).GetField("callback", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(info.target, this);
							// NOTE: Not yet working, needs alternative ICommListener for Chat messages
							/*App.ChatUI.SetEnabled(true);
							App.ChatUI.SetLocked(false);
							App.ChatUI.Show(false);
							App.ChatUI.SetCanOpenContextMenu(false);*/
							//activate chat on replays but disable profile or trading menus (wired bugs)
						}
					}
					break;
				case "Raycast":
					{
						if (playing && endGameButton.renderer.material.mainTexture != pauseButton && endGameButton.renderer.material.mainTexture != playButton)
						{
							if (paused)
							{
								endGameButton.renderer.material.mainTexture = playButton;
							}
							else
							{
								endGameButton.renderer.material.mainTexture = pauseButton;
							}
						}
					}
					break;
			}
		}

		public bool allowEndTurn()
		{
			return true;
		}

		public void endturnPressed()
		{
			if (!playing) return;

			paused = !paused;
			if (paused)
			{
				endGameButton.renderer.material.mainTexture = playButton;
			}
			else
			{
				endGameButton.renderer.material.mainTexture = pauseButton;
			}
		}
	}
}

