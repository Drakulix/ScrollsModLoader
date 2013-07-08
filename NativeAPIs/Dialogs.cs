using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ScrollsModLoader
{
	public class Dialogs
	{
		public static void showNotification(String title, String text) {
			switch (Platform.getOS()) {
				case Platform.OS.Win:
					Assembly forms = Assembly.Load("System.Windows.Forms, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
					forms.GetType("System.Windows.Forms.MessageBox").GetMethod("Show", new Type[] {typeof(string), typeof(string)}).Invoke(null, new object[] {text, title});
					break;
				case Platform.OS.Mac:
					byte[] monoMacLoad = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScrollsModLoader.NativeAPIs.MonoMac.dll").ReadToEnd();
					System.Reflection.Assembly monoMac = System.Reflection.Assembly.Load(monoMacLoad);
				 	object alert = monoMac.CreateInstance("MonoMac.AppKit.NSAlert");
					alert.GetType().GetProperty("MessageText").SetValue(alert, title, null);
					alert.GetType().GetProperty("InformativeText").SetValue(alert, text, null);
					alert.GetType().GetMethod("RunModal").Invoke(alert, null);
					break;
				default:
					Console.WriteLine("Unsupported OS");
					break;
			}
		}

		public static String fileOpenDialog() {
			switch (Platform.getOS()) {
			case Platform.OS.Win:
				String ret = "";
				bool fullscreen = false;

				//in case we are patching
				try {
					fullscreen = Screen.fullScreen;
				} catch {}

				if (fullscreen) {
					App.Popups.ShowOk (null, "warningFullscreen", "Error", "File Dialogs do not work in fullscreen mode, please switch to proceed", "Cancel");
				} else {
					ret = WindowsDialog.ShowWindowsDialog ();
				}
				if (ret.Equals (""))
					return null;
				else
					return ret;
			case Platform.OS.Mac:
				byte[] monoMacLoad = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ScrollsModLoader.NativeAPIs.MonoMac.dll").ReadToEnd();
					System.Reflection.Assembly monoMac = System.Reflection.Assembly.Load(monoMacLoad);
					object panel = monoMac.CreateInstance("MonoMac.AppKit.NSOpenPanel");
					//Note: AllowedFileTypes is currently broken on MonoMac. it freezes the app
					panel.GetType().GetProperty("AllowsMultipleSelection").SetValue(panel, false, null);
					if ((int)panel.GetType().GetMethod("RunModal", new Type[] {}).Invoke(panel, null) == 0) return null;
					return ((String[])(panel.GetType().GetProperty("Filenames").GetValue(panel, null)))[0];
				default:
					Console.WriteLine("Unsupported OS");
					break;
			}
			return null;
		}
	}
}

