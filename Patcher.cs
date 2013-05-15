using System;

namespace ScrollsModLoader
{
	class Patcher
	{
		public static void Main (string[] args)
		{
			/* TO-DO
			 * get path of Scrolls
			 * backup old Assembly
			 * patch it
			 * run ILRepack and inject self
			 */


			//get Path of Scrolls Data Folder
			//TO-DO Test App to be valid on Mac. Note: AllowedFileTypes is currently broken on MonoMac. it freezes the app

			String path = "";

			OperatingSystem os = Environment.OSVersion;
			PlatformID     pid = os.Platform;
			switch (pid) 
    		{
    			case PlatformID.Win32NT:
    			case PlatformID.Win32S:
    			case PlatformID.Win32Windows:
    			case PlatformID.WinCE:
        			//Windows Users have a fixed path
					path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"/Local/Mojang/Scrolls/game/Scrolls_Data/";
				break;
				case PlatformID.MacOSX:
				case PlatformID.Unix:

					//Apps are bundles (== folders) on MacOS
					if (System.IO.Directory.Exists("/Applications/Scrolls-Alpha.app")) {
						path = "/Applications/Scrolls-Alpha.app/Contents/MacOS/game/MacScrolls.app/Contents/Data/";
						break;
					}

					// MacOS User need to tell us the path of their Scrolls.app

					System.Reflection.Assembly monoMac = null;
					try	{
						monoMac = System.Reflection.Assembly.LoadFile(Environment.CurrentDirectory+"/MonoMac.dll");
					} catch (System.IO.FileLoadException ex) {
						Console.WriteLine("MonoMac not found. Linux is currently not supported");
						Console.WriteLine(ex);
						return;
					}
					
					object alert = monoMac.CreateInstance("MonoMac.AppKit.NSAlert");
					alert.GetType().GetProperty("MessageText").SetValue(alert, "Scrolls was not found", null);
					alert.GetType().GetProperty("InformativeText").SetValue(alert, "Please select your local install of Scrolls", null);
					alert.GetType().GetMethod("RunModal").Invoke(alert, null);
					
					object panel = monoMac.CreateInstance("MonoMac.AppKit.NSOpenPanel");
					panel.GetType().GetProperty("AllowsMultipleSelection").SetValue(panel, false, null);
					if ((int)panel.GetType().GetMethod("RunModal", new Type[] {}).Invoke(panel, null) == 0) {
						alert.GetType().GetProperty("MessageText").SetValue(alert, "No Selection was made", null);
						alert.GetType().GetProperty("InformativeText").SetValue(alert, "Scrolls ModLoader was not able to find your local install of Scrolls. Scrolls ModLoader will close now", null);
						alert.GetType().GetMethod("RunModal").Invoke(alert, null);
						return;
					}
					path = ((string[])(panel.GetType().GetProperty("Filenames").GetValue(panel, null)))[0]+"/Contents/MacOS/game/MacScrolls.app/Contents/Data/";
					
					Console.WriteLine(path);
				break;
    			default:
        			Console.WriteLine("Unsupported Platform detected");
					return;
			}

			Console.WriteLine(path);
  		
		}

		public bool patchAssembly(String path) {

			return true;
		}

		public bool saveModePatchAssembly(String path) {

			return true;
		}
	}
}
