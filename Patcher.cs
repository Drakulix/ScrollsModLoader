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
			//TO-DO check default Applications path on Mac. Test App to be valid on Mac. Note: AllowedFileTypes is currently broken on MonoMac. it freezes the app
			//TO-DO add Dialog to inform the user, what he shall select

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
					// MacOS User need to tell us the path of their Scrolls.app

					System.Reflection.Assembly monoMac = null;
					try	{
						monoMac = System.Reflection.Assembly.LoadFile(Environment.CurrentDirectory+"/MonoMac.dll");
					} catch (System.IO.FileLoadException ex) {
						Console.WriteLine("MonoMac not found. Linux is currently not supported");
						Console.WriteLine(ex);
						return;
					}
					object panel = monoMac.CreateInstance("MonoMac.AppKit.NSOpenPanel");
					panel.GetType().GetProperty("AllowsMultipleSelection").SetValue(panel, false, null);
					while ((int)panel.GetType().GetMethod("RunModal", new Type[] {}).Invoke(panel, null) == 0);
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
