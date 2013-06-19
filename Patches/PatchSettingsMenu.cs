using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using LinFu.AOP.Interfaces;
using ScrollsModLoader.Interfaces;
using UnityEngine;

namespace ScrollsModLoader
{
	public class PatchSettingsMenu : Patch
	{
		private SceneProvider scene = null;
		private Dictionary<String, SceneProvider> sceneDescriptors = new Dictionary<String, SceneProvider>();

		public PatchSettingsMenu(TypeDefinitionCollection types) : base (types) {}

		public override MethodDefinition[] patchedMethods ()
		{
			TypeDefinition settingsMenu = Hooks.getTypeDef (assembly, "SettingsMenu");
			return new MethodDefinition[] {
				Hooks.getMethDef (settingsMenu, "Init"),
				Hooks.getMethDef (settingsMenu, "OnGUI"),
				Hooks.getMethDef (settingsMenu, "OnDestroy"),
				Hooks.getMethDef (Hooks.getTypeDef(assembly, "SceneLoader"), "loadScene"),
				Hooks.getMethDef(Hooks.getTypeDef(assembly, "LobbyMenu"), "isSceneJumpValid")
			};
		}

		public override object Intercept (IInvocationInfo info)
		{
			if (info.TargetMethod.Name.Equals ("isSceneJumpValid")) {
				return ((bool)info.TargetMethod.Invoke (info.Target, info.Arguments)) || (((String)info.Arguments[0]).Equals("_Settings") && scene != null);
			}

			if (info.TargetMethod.Name.Equals("loadScene")) {
				try {
					Console.WriteLine(info.Arguments[0]);
					if (sceneDescriptors.ContainsKey ((String)info.Arguments[0])) {
						scene = sceneDescriptors [(String)info.Arguments[0]];
						typeof(LobbyMenu).GetMethod ("fadeOutScene", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (App.LobbyMenu, new object[] {});
						LoadScene("_Settings");
					} else {
						if (scene != null)
							typeof(LobbyMenu).GetMethod ("fadeOutScene", BindingFlags.NonPublic | BindingFlags.Instance).Invoke (App.LobbyMenu, new object[] {});
						scene = null;
						LoadScene((String)info.Arguments[0]);
					}
				} catch (Exception exp) {
					Console.WriteLine (exp);
					LoadScene((String)info.Arguments[0]);
				}
				return null;
			}

			if (scene == null) {
				return info.TargetMethod.Invoke (info.Target, info.Arguments);
			}

			switch (info.TargetMethod.Name) {
			case "Init":
				try {
					scene.OnCreate ((MonoBehaviour)info.Target);
				} catch (Exception exp) {
					Console.WriteLine (exp);
				}
				//load style
				FieldInfo[] fields = typeof(SettingsMenu).GetFields (BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
				foreach (FieldInfo field in fields) {
					switch (field.Name) {
					case "activeButtonStyle":
						if (scene.ActiveButtonStyle () != null)
							try {
								field.SetValue (info.Target, scene.ActiveButtonStyle ());
							} catch (Exception exp) {
								Console.WriteLine (exp);
							}
						break;
					case "buttonStyle":
						if (scene.ButtonStyle () != null)
							try {
								field.SetValue (info.Target, scene.ButtonStyle ());
							} catch (Exception exp) {
								Console.WriteLine (exp);
							}
						break;
					case "regularUI":
						if (scene.UISkin () != null)
							try {
								field.SetValue (info.Target, scene.UISkin ());
							} catch (Exception exp) {
								Console.WriteLine (exp);
							}
						break;
					case "settingsSkin":
						if (scene.Skin () != null)
							try {
								field.SetValue (info.Target, scene.Skin ());
							} catch (Exception exp) {
								Console.WriteLine (exp);
							}
						break;
					}
				}
				break;

			case "OnGUI":
				try {
					GUI.depth = 21;
					GUI.skin = (GUISkin)typeof(SettingsMenu).GetField ("settingsSkin" ,BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(info.Target);
					GUI.skin.label.alignment = TextAnchor.MiddleCenter;
					scene.OnGUI ();
				} catch (Exception exp) {
					Console.WriteLine (exp);
				}
				break;

			case "OnDestroy":
				try {
					scene.OnDestroy ();
				} catch (Exception exp) {
					Console.WriteLine (exp);
				}
				break;
			}

			return null;
		}

		public bool AddScene (String desc, SceneProvider provider) {
			if (sceneDescriptors.ContainsKey (desc))
				return false;
			sceneDescriptors.Add (desc, provider);
			return true;
		}

		public void LoadScene (String providerDesc) {
			if (Application.loadedLevelName == "_DeckBuilderView" && providerDesc != "_BattleModeView")
			{
				App.AudioScript.PlayMusic ("Music/Theme");
			}
			if (providerDesc == "_DeckBuilderView")
			{
				App.SceneValues.deckBuilder = new SceneValues.SV_DeckBuilder ();
			}
			Application.LoadLevel (providerDesc);
		}
	}
}

