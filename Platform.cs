using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScrollsModLoader
{

	/*
	 * OS Specific Code
	 */

	public class Platform
	{
		public enum OS {
			Win,
			Mac,
			Unix,
			Unknown
		}

		public static OS getOS() {
			OperatingSystem os = Environment.OSVersion;
			PlatformID     pid = os.Platform;
			switch (pid) 
    		{
    			case PlatformID.Win32NT:
    			case PlatformID.Win32S:
    			case PlatformID.Win32Windows:
    			case PlatformID.WinCE:
					return OS.Win;
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					try	{
						byte[] monoMacLoad = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ScrollsModLoader.MonoMac.dll").ReadToEnd();
						System.Reflection.Assembly.Load(monoMacLoad);
						//System.Reflection.Assembly.LoadFile(Environment.CurrentDirectory+"/MonoMac.dll");
					} catch (System.IO.FileLoadException) {
						return OS.Unix;
					}
					return OS.Mac;
				default:
					return OS.Unknown;
		
			}
		}

		public static String getGlobalScrollsInstallPath() {
			String path = null;
			switch (Platform.getOS()) 
    		{
    			case Platform.OS.Win:
        			//Windows Users have a fixed path
					path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mojang\\Scrolls\\game\\Scrolls_Data\\Managed\\";
					if (!System.IO.File.Exists(path+"Assembly-CSharp.dll")) {
						Dialogs.showNotification("Scrolls must be installed", "ScrollsModLoader was not able to find your Scrolls install");
						return null;
					}
				break;
			case Platform.OS.Mac:

					//if we are already loaded from the game folder, get that instead
					//TO-DO is that too unsecure? should be check if Assembly-CSharp is loaded instead?
					if ((from file in Directory.GetParent (System.Reflection.Assembly.GetExecutingAssembly().Location).GetFiles()
				    	 where file.Name.Contains ("Assembly-CSharp.dll")
				    	 select file).Count() > 0)
						return Directory.GetParent (System.Reflection.Assembly.GetExecutingAssembly().Location).ToString()+"/";

					//Apps are bundles (== folders) on MacOS
					if (System.IO.Directory.Exists("/Applications/Scrolls.app")) {
						path = "/Applications/Scrolls.app/Contents/MacOS/game/MacScrolls.app/Contents/Data/Managed/";
						break;
					}
					
					// MacOS User needs to tell us the path of their Scrolls.app
					Dialogs.showNotification("Scrolls was not found", "Please select your local install of Scrolls");
					
					path = Dialogs.fileOpenDialog();
					if (path == null) {
						Dialogs.showNotification("No Selection was made", "Scrolls ModLoader was not able to find your local install of Scrolls. Scrolls ModLoader will close now");
						return null;
					}
					path += "/Contents/MacOS/game/MacScrolls.app/Contents/Data/Managed/";
					
					if (!System.IO.File.Exists(path+"Assembly-CSharp.dll")) {
						Dialogs.showNotification("Wrong Selection", "The selected file is not a valid Scrolls.app. Scrolls ModLoader will close now");
						return null;
					}

				break;
    			default:
        			Console.WriteLine("Unsupported Platform detected");
					return null;
			}

			Console.WriteLine("Install Path: "+path);
			return path;
		}
	}
}

