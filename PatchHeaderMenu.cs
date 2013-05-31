using System;
using System.Reflection;
using System.Collections.Generic;
using Mono.Cecil;
using LinFu.AOP.Interfaces;
using UnityEngine;

namespace ScrollsModLoader
{
	public class PatchHeaderMenu : Patch
	{
		public PatchHeaderMenu(TypeDefinitionCollection types) : base (types) {}

		private bool first = true;
		private Texture2D text;

		public override MethodDefinition[] patchedMethods() {
			MethodDefinition DrawHeaderButtons = Hooks.getMethDef (Hooks.getTypeDef (assembly, "LobbyMenu"), "drawHeaderButtons");
			return new MethodDefinition[] {DrawHeaderButtons};
		}

		public override object Intercept (IInvocationInfo info)
		{
			//typeof(LobbyMenu).GetField ("_hoverButtonInside", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).SetValue (info.Target, false);
			try {
				Type lobbyMenu = typeof(LobbyMenu);

				if (first) {

					GUISkin gUISkin7 = ScriptableObject.CreateInstance<GUISkin> ();

					text = new Texture2D(87, 39); //115, 39
					text.LoadImage(System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("ScrollsModLoader.Mods.png").ReadToEnd ());

					gUISkin7.button.normal.background = text;
					gUISkin7.button.hover.background = text;
					gUISkin7.button.active.background = text;

					//info.Target.GUISkins.Add (gUISkin5);
					FieldInfo GUISkins = lobbyMenu.GetField("GUISkins", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
					if (GUISkins == null)
						Console.WriteLine ("GuiSkins == null");
					((List<GUISkin>)(GUISkins.GetValue(info.Target))).Add(gUISkin7);
					first = false;
				}

				MethodInfo drawHeader = lobbyMenu.GetMethod ("drawHeaderButton", BindingFlags.NonPublic | BindingFlags.Instance);
				drawHeader.Invoke(info.Target, new object[] {6, "_Settings"});


			} catch (Exception exp) {
				Console.WriteLine (exp);
			}	
			return info.TargetMethod.Invoke(info.Target, info.Arguments);
		}
	}
}

