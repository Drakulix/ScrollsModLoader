using System;

namespace ScrollsModLoader
{
	public class Dialogs
	{
		public static void showNotification(String title, String text) {
			switch (Platform.getOS()) {
				case Platform.OS.Win:
					System.Reflection.Assembly forms = System.Reflection.Assembly.LoadFile("System.Windows.Forms");
					forms.GetType("MessageBox").GetMethod("Show", new Type[] {typeof(string), typeof(string)}).Invoke(null, new object[] {text, title});
					break;
				case Platform.OS.Mac:
					System.Reflection.Assembly monoMac = System.Reflection.Assembly.LoadFile(Environment.CurrentDirectory+"/MonoMac.dll");
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
			//TO-DO implement Windows Dialog
			switch (Platform.getOS()) {
				case Platform.OS.Win:
					//Note: This crashes
					return null;
					//throw System.NotImplementedException;
				case Platform.OS.Mac:
					System.Reflection.Assembly monoMac = System.Reflection.Assembly.LoadFile(Environment.CurrentDirectory+"/MonoMac.dll");
					object panel = monoMac.CreateInstance("MonoMac.AppKit.NSOpenPanel");
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

