using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Mono.Cecil;
using LinFu.AOP.Interfaces;
using UnityEngine;
using ScrollsModLoader.Interfaces;
using JsonFx.Json;

namespace ScrollsModLoader
{
	public class PatchModsMenu : Patch, SceneProvider, IListCallback, IOkStringCancelCallback, IOkCallback, IOkCancelCallback
	{
		private bool first = true;
		private int defaultTextSize;
		private Texture2D textnormal;
		private Texture2D texthover;
		private Texture2D textactive;
		private ModAPI modAPI;

		private UIListPopup repoListPopup;
		private UIListPopup downloadableListPopup;
		private UIListPopup modListPopup;

		private LocalMod deinstallCache;

		private ModLoader loader;
		private ModManager modManager;
		private RepoManager repoManager;

		public PatchModsMenu(TypeDefinitionCollection types, ModLoader loader) : base (types) {

			this.loader = loader;
			this.modManager = this.loader.modManager;
			this.repoManager = modManager.repoManager;

		}

		public override MethodDefinition[] patchedMethods() {
			MethodDefinition DrawHeaderButtons = Hooks.getMethDef (Hooks.getTypeDef (assembly, "LobbyMenu"), "drawHeaderButtons");
			MethodDefinition GetButtonPositioner = null;
			foreach (MethodDefinition def in (Hooks.getTypeDef (assembly, "LobbyMenu")).Methods) {
				if (def.Name.Equals ("getButtonPositioner") && def.Parameters [0].ParameterType.Name.Equals ("MockupCalc"))
					GetButtonPositioner = def;
			}
			if (GetButtonPositioner == null && DrawHeaderButtons == null)
				return new MethodDefinition[] { };
			if (GetButtonPositioner == null) {
				Console.WriteLine ("ERROR: unable to find Method");
				return new MethodDefinition[] {DrawHeaderButtons};
			} else {
				return new MethodDefinition[] {DrawHeaderButtons, GetButtonPositioner};
			}
		}

		public override object Intercept (IInvocationInfo info)
		{
			//typeof(LobbyMenu).GetField ("_hoverButtonInside", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).SetValue (info.Target, false);

			/*if (typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue (info.Target).Equals("_Settings")) {
				typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).SetValue (info.Target, "_SomethingElse");
			}*/

			try {
				//FIX
				if (info.TargetMethod.Name.Equals("getButtonPositioner")) {
					info.Arguments[2] = 75.0f;
					if (Screen.width * 3 == Screen.height * 4)
						info.Arguments[2] = 45.0f;
					return info.TargetMethod.Invoke(info.Target, info.Arguments);
				}

				Type lobbyMenu = typeof(LobbyMenu);

				if (first) {

					GUISkin gUISkin7 = ScriptableObject.CreateInstance<GUISkin> ();

					textnormal = new Texture2D(84, 30, TextureFormat.ARGB32, false, true); //115, 39
					textnormal.LoadImage(System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("ScrollsModLoader.Resources.button_mods_normal.png").ReadToEnd ());
					textnormal.filterMode = FilterMode.Bilinear;
					texthover = new Texture2D(84, 30, TextureFormat.ARGB32, false, true); //115, 39
					texthover.LoadImage(System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("ScrollsModLoader.Resources.button_mods_light.png").ReadToEnd ());
					texthover.filterMode = FilterMode.Bilinear;
					textactive = new Texture2D(84, 30, TextureFormat.ARGB32, false, true); //115, 39
					textactive.LoadImage(System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("ScrollsModLoader.Resources.button_mods_dark.png").ReadToEnd ());
					textactive.filterMode = FilterMode.Bilinear;

					gUISkin7.button.normal.background = textnormal;
					gUISkin7.button.hover.background = texthover;
					gUISkin7.button.active.background = textactive;

					defaultTextSize = GUI.skin.label.fontSize;

					//info.Target.GUISkins.Add (gUISkin5);
					FieldInfo GUISkins = lobbyMenu.GetField("GUISkins", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
					((List<GUISkin>)(GUISkins.GetValue(info.Target))).Add(gUISkin7);

					((LobbyMenu)info.Target).AdjustForResolution();

					first = false;
				}

				object index = typeof(LobbyMenu).GetField ("_hoverButtonIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.Target);
				object ret = info.TargetMethod.Invoke(info.Target, info.Arguments);

				bool newinside = (bool)typeof(LobbyMenu).GetField ("_hoverButtonInside", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.Target);
				int newindex = (int)typeof(LobbyMenu).GetField ("_hoverButtonIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(info.Target);

				typeof(LobbyMenu).GetField ("_hoverButtonIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.Target, index);
				typeof(LobbyMenu).GetField ("_hoverButtonInside", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.Target, false);

				MethodInfo drawHeader = lobbyMenu.GetMethod ("drawHeaderButton", BindingFlags.NonPublic | BindingFlags.Instance);
				drawHeader.Invoke(info.Target, new object[] {7, "_Mods"});

				if (!((bool)typeof(LobbyMenu).GetField ("_hoverButtonInside", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (info.Target))) {
					typeof(LobbyMenu).GetField ("_hoverButtonIndex", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.Target, newindex);
					typeof(LobbyMenu).GetField ("_hoverButtonInside", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.Target, newinside);
				}

				return ret;

				//object headerPositioner = typeof(LobbyMenu).GetField ("_headerPositioner", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (info.Target);
				//typeof(LobbyMenu).GetField ("buttonMaxX", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info.Target, ((Rect)headerPositioner.GetType().GetMethod("getButtonRect").Invoke(headerPositioner, new object[] { 6f, 0f })).x);

			} catch (Exception exp) {
				Console.WriteLine (exp);
				return info.TargetMethod.Invoke(info.Target, info.Arguments);
			}	

		}

		public void Initialize(ModAPI api) {
			this.modAPI = api;
			this.modAPI.AddScene ("_Mods", this);
		}

		public string SceneName ()
		{
			return "_Mods";
		}
		public void OnCreate (MonoBehaviour parentScene)
		{
			App.ChatUI.Show (false);
			repoListPopup = new GameObject ("Repo List").AddComponent<UIListPopup> ();
			repoListPopup.transform.parent = parentScene.transform;
			repoListPopup.Init (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f+40.0f, (float)Screen.width/4.5f, (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f), false, true, repoManager.repositories, this, null, null, false, true, true, true, null, true, false);
			repoListPopup.enabled = true;
			repoListPopup.SetOpacity(1f);
			if (repoManager.repositories.Count > 0)
				repoListPopup.setSelectedItem (repoManager.repositories [0]);

			downloadableListPopup = new GameObject ("Downloadable List").AddComponent<UIListPopup> ();
			downloadableListPopup.transform.parent = parentScene.transform;
			if (repoManager.repositories.Count > 0)
				downloadableListPopup.Init (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f, (float)Screen.height/5.0f+(float)Screen.height/30.0f+40.0f, (float)Screen.width/4.1f, (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f), false, true, repoManager.getModListForRepo((Repo)repoListPopup.selectedItem()), this, null, null, true, true, true, true, null, true, false);
			else 
				downloadableListPopup.Init (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f, (float)Screen.height/5.0f+(float)Screen.height/30.0f+40.0f, (float)Screen.width/4.1f, (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f), false, true, new List<Item>(), this, null, null, true, true, true, true, null, true, false);

			downloadableListPopup.enabled = true;
			downloadableListPopup.SetOpacity(1f);

			modListPopup = new GameObject ("Mod List").AddComponent<UIListPopup> ();
			modListPopup.transform.parent = parentScene.transform;
			modListPopup.Init (new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f+40.0f, (float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f, (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f), false, true, modManager.installedMods, this, null, null, true, true, true, true, null, true, true);
			modListPopup.enabled = true;
			modListPopup.SetOpacity(1f);
			modListPopup.setSelectedItems(modManager.installedMods.FindAll(delegate (Item mod) {
				return (mod as LocalMod).enabled;
			}));
		}
		public void OnGUI ()
		{
			GUI.DrawTexture (new Rect (0f, 0f, (float)Screen.width, (float)Screen.height), ResourceManager.LoadTexture ("DeckBuilder/bg"));
			new ScrollsFrame (new Rect((float)Screen.width/15.0f, (float)Screen.height/5.0f, (float)Screen.width/15.0f*8.0f, (float)Screen.height/6.0f*4.0f)).AddNinePatch (ScrollsFrame.Border.LIGHT_CURVED, NinePatch.Patches.CENTER).Draw();
			new ScrollsFrame (new Rect((float)Screen.width/15.0f*9.5f, (float)Screen.height/5.0f, (float)Screen.width/15.0f*4.5f, (float)Screen.height/6.0f*4.0f)).AddNinePatch (ScrollsFrame.Border.LIGHT_CURVED, NinePatch.Patches.CENTER).Draw();

			Color textColor = GUI.skin.label.normal.textColor;
			GUI.skin.label.normal.textColor = Color.white;

			GUI.skin.label.fontSize = (int) (((float) (0x1c * Screen.height)) / 1080f);
			GUI.Label (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f + (float)Screen.width/4.5f/2.0f - (float)Screen.width/8.0f/2.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f, (float)Screen.width/8.0f, 35.0f), "Repositories");
			GUI.Label (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f + (float)Screen.width/4.1f/2.0f - (float)Screen.width/8.0f/2.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f, (float)Screen.width/8.0f, 35.0f), "Downloadable Mods");
			GUI.Label (new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f+((float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f)/2.0f - (float)Screen.width/8.0f/2.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f, (float)Screen.width/8.0f, 35.0f), "Installed Mods");

			GUI.skin.label.fontSize = 18;
			if (GUI.Button (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), string.Empty)) {
				App.Popups.ShowTextInput (this, "http://", "WARNING: Other repositories are NOT trusted by Scrollsguide.", "addRepo", "Add Repository", "Please enter the URL of the repository you want to add", "Add");
			}
			GUI.Label(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), "Add Repository");

			if (GUI.Button(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f + (float)Screen.width/9.0f + 1.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), string.Empty)) {
				try {	
					if (repoListPopup.selectedItem ().Equals (repoManager.repositories[0])) {
						App.Popups.ShowOk (this, "remWarning", "Invalid Operation", "You cannot remove Scrollsguide from your repository list", "OK");
					} else {
						repoManager.removeRepository ((Repo)repoListPopup.selectedItem ());
						repoListPopup.SetItemList (repoManager.repositories);
						repoListPopup.setSelectedItem (repoManager.repositories[0]);
						downloadableListPopup.SetItemList (repoManager.getModListForRepo ((Repo)repoListPopup.selectedItem ()));
					}
				} catch {}
			}
			GUI.Label(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f + (float)Screen.width/9.0f + 1.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), "Remove Repository");

			if (GUI.Button(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/4.1f, (float)Screen.width/35.0f), string.Empty)) {
				App.Popups.ShowInfo ("Downloading", "Please wait while the requested mods are being downloaded");
				new Thread(downloadMods).Start();
			}
			GUI.Label(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/4.1f, (float)Screen.width/35.0f), "Download");

			if (GUI.Button(new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f, (float)Screen.width/35.0f), string.Empty)) {
				loader.repatch ();
			}
			GUI.Label(new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f, (float)Screen.width/35.0f), "Apply (Restarts Scrolls)");

			TextAnchor alignment = GUI.skin.label.alignment;
			GUI.skin.label.alignment = TextAnchor.MiddleRight;
			GUI.skin.label.normal.textColor = Color.Lerp(Color.white, Color.yellow, 0.4f);

			GUI.Label(new Rect(Screen.width * 0.5f, Screen.height * 0.90f, Screen.width * 0.48f, Screen.height * 0.04f),  "The Summoner ModLoader v"+ModLoader.getVersion()+" is not an official Scrolls feature");
			GUI.Label(new Rect(Screen.width * 0.5f, Screen.height * 0.94f, Screen.width * 0.48f, Screen.height * 0.04f), "Read and submit bugs on http://www.scrollsguide.com/summoner");

			GUI.skin.label.normal.textColor = textColor;
			GUI.skin.label.fontSize = defaultTextSize;
			GUI.skin.label.alignment = alignment;
		}
		public void OnDestroy ()
		{
		}
		public GUISkin Skin ()
		{
			return null;
		}
		public GUISkin UISkin ()
		{
			return null;
		}
		public GUIStyle ButtonStyle ()
		{
			return null;
		}
		public GUIStyle ActiveButtonStyle ()
		{
			return null;
		}

		public void downloadMods () {
			foreach (Item mod in downloadableListPopup.selectedItems()) {
				modManager.installMod((Repo)repoListPopup.selectedItem(), (Mod)mod);
			}
			downloadableListPopup.SetItemList(repoManager.getModListForRepo ((Repo)repoListPopup.selectedItem ()));
			downloadableListPopup.setSelectedItems (new List<Item> ());
			modListPopup.SetItemList (modManager.installedMods);
			App.Popups.KillCurrentPopup ();
		}

		public void ButtonClicked (UIListPopup popup, ECardListButton button, List<Item> selectedCards, Item card) {
			this.ButtonClicked (popup, button, card);
		}
		public void ButtonClicked (UIListPopup popup, ECardListButton button, Item card) {
			if (popup == modListPopup) {
				if (button == ECardListButton.BUTTON_LEFT)
					if ((card as LocalMod).enabled)
						loader.moveModUp ((LocalMod)card);
				else if (button == ECardListButton.BUTTON_RIGHT)
					if ((card as LocalMod).enabled)
						loader.moveModDown((LocalMod)card);
			}
		}
		public void ItemButtonClicked (UIListPopup popup, Item card) {}
		public void ItemClicked (UIListPopup popup, Item card) {
			if (popup == repoListPopup) {
				repoListPopup.setSelectedItem(card);
				downloadableListPopup.SetItemList (repoManager.getModListForRepo ((Repo)repoListPopup.selectedItem ()));
			}
			if (popup == downloadableListPopup) {
				System.Diagnostics.Process.Start((repoListPopup.selectedItem() as Repo).url+"mod/"+(downloadableListPopup.selectedItem() as Mod).id);
			}
			if (popup == modListPopup) {
				if (modListPopup.selectedItems ().Contains (card))
					modManager.enableMod ((LocalMod)card);
				else
					modManager.disableMod ((LocalMod)card);
				modListPopup.SetItemList (modManager.installedMods);
			}
		}
		public void ItemHovered (UIListPopup popup, Item card) {}
		public void ItemCanceled (UIListPopup popup, Item card) {
			deinstallCache = (LocalMod)card;
			App.Popups.ShowOkCancel (this, "removeMod", "Uninstallation Warning", "Are you sure you want to remove " + card.getName () + "?", "Uninstall", "Cancel");
		}

		public void PopupOk (string popupType, string choice)
		{
			if (popupType.Equals("addRepo")) {
				repoManager.tryAddRepository (choice);
				repoListPopup.SetItemList (repoManager.repositories);
				repoListPopup.setSelectedItem (repoManager.repositories[0]);
				downloadableListPopup.SetItemList (repoManager.getModListForRepo ((Repo)repoListPopup.selectedItem ()));
			}
		}
		public void PopupCancel (string popupType)
		{
			return;
		}

		public void PopupOk (string popupType)
		{
			if (popupType.Equals ("removeMod")) {
				modManager.deinstallMod (deinstallCache);
				App.Popups.ShowOk (this, "remNotice", "Uninstallation Info", "Any uninstallation will not take place until you press 'Apply' or manually restart the game", "OK");
				//downloadableListPopup.SetItemList (repoManager.getModListForRepo ((Repo)repoListPopup.selectedItem ()));
			}
		}

	}
}

