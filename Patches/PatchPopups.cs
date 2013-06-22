using System;
using Mono.Cecil;
using System.Reflection;
using UnityEngine;
using ScrollsModLoader.Interfaces;

namespace ScrollsModLoader
{
	public class PatchPopups : Patch, IOkStringCancelCallback
	{
		private static PatchPopups instance;
		public String encryptedPassword;
		public String popupType;
		public IOkStringsCancelCallback callback;

		public PatchPopups (TypeDefinitionCollection types) : base (types) {
			instance = this;
		}

		public override Mono.Cecil.MethodDefinition[] patchedMethods ()
		{
			MethodDefinition PopupOk = Hooks.getMethDef (Hooks.getTypeDef (assembly, "Popups"), "OnGUI");
			if (PopupOk == null)
				return new MethodDefinition[] { };
			return new MethodDefinition[] { PopupOk };
		}

		public override object Intercept (LinFu.AOP.Interfaces.IInvocationInfo info)
		{
			object ret = info.TargetMethod.Invoke (info.Target, info.Arguments);
			if (((PopupType)typeof(Popups).GetField ("currentPopupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (info.Target)) == PopupType.SAVE_DECK
			    && typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (info.Target).Equals("login")) {
				float num2 = Screen.height * 0.03f;
				Rect rect = new Rect((Screen.width * 0.5f) - (Screen.height * 0.35f), Screen.height * 0.3f, Screen.height * 0.7f, Screen.height * 0.4f);
				Rect popupInner = new Rect(rect.x + num2, rect.y + num2, rect.width - (2f * num2), rect.height - (2f * num2));
				Rect rect9 = new Rect(popupInner.x + (popupInner.width * 0.2f), popupInner.y + (popupInner.height * 0.5f) + popupInner.height * 0.13f, popupInner.width * 0.6f, popupInner.height * 0.14f);
				this.encryptedPassword = GUI.PasswordField (rect9, this.encryptedPassword, '*');
			}
			return ret;
		}

		public static void ShowLogin(Popups popups, IOkStringsCancelCallback callback, string username, string problems, string popupType, string header, string description, string okText) {
			popups.ShowSaveDeck (instance, username, problems);
			instance.popupType = popupType;
			instance.callback = callback;
			typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, "login");
			typeof(Popups).GetField ("header", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, header);
			typeof(Popups).GetField ("description", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, description);
			typeof(Popups).GetField ("okText", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, okText);
		}

		public void PopupOk (string popupType, string choice) {
			callback.PopupOk(this.popupType, choice, encryptedPassword);
		}

		public void PopupCancel (string popupType) {
			callback.PopupCancel (popupType);
		}

		private enum PopupType
    	{
        	NONE,
        	OK_CANCEL,
        	OK,
        	MULTIBUTTON,
        	DECK_SELECTOR,
        	SAVE_DECK,
       		JOIN_ROOM,
        	INFO_PROGCLOSE,
        	SHARD_PURCHASE_ONE,
        	SHARD_PURCHASE_TWO,
        	TOWER_CHALLENGE_SELECTOR,
        	GOLD_SHARDS_SELECT,
        	PURCHASE_PASSWORD_ENTRY,
        	SCROLL_TEXT
    	}
	}
}

