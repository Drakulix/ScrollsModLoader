using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

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
						byte[] monoMacLoad = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ScrollsModLoader.NativeAPIs.MonoMac.dll").ReadToEnd();
						System.Reflection.Assembly.Load(monoMacLoad);
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

			//if we are already loaded from the game folder, get that instead
			if ((from file in Directory.GetParent (System.Reflection.Assembly.GetExecutingAssembly().Location).GetFiles()
			     where file.Name.Contains ("Assembly-CSharp.dll")
			     select file).Count() > 0)
				 return Directory.GetParent (System.Reflection.Assembly.GetExecutingAssembly().Location).ToString()+Path.DirectorySeparatorChar;

			switch (Platform.getOS()) 
    		{
    			case Platform.OS.Win:
        			path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mojang\\Scrolls\\game\\Scrolls_Data\\Managed\\";
					if (!System.IO.File.Exists(path+"Assembly-CSharp.dll")) {
						Console.WriteLine ("Expected Path: " + path);
						Dialogs.showNotification("Scrolls was not found", "Please select your local install of Scrolls");

						path = Dialogs.fileOpenDialog();
						if (path == null) {
							Dialogs.showNotification("No Selection was made", "Scrolls Summoner was not able to find your local install of Scrolls. Scrolls Summoner will close now");
							return null;
						}
						if (!System.IO.File.Exists(path+"\\Assembly-CSharp.dll")) {
							Dialogs.showNotification("Wrong Selection", "The selected file is not a valid Scrolls game folder. Scrolls Summoner will close now");
							return null;
						}
					}	
					return path;
				case Platform.OS.Mac:

					//Apps are bundles (== folders) on MacOS
					if (System.IO.Directory.Exists("/Applications/Scrolls.app")) {
						path = "/Applications/Scrolls.app/Contents/MacOS/game/MacScrolls.app/Contents/Data/Managed/";
						break;
					}
					
					// MacOS User needs to tell us the path of their Scrolls.app
					Dialogs.showNotification("Scrolls was not found", "Please select your local install of Scrolls");
					
					path = Dialogs.fileOpenDialog();
					if (path == null) {
						Dialogs.showNotification("No Selection was made", "Scrolls Summoner was not able to find your local install of Scrolls. Scrolls Summoner will close now");
						return null;
					}
					path += "/Contents/MacOS/game/MacScrolls.app/Contents/Data/Managed/";
					
					if (!System.IO.File.Exists(path+"Assembly-CSharp.dll")) {
						Dialogs.showNotification("Wrong Selection", "The selected file is not a valid Scrolls.app. Scrolls Summoner will close now");
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

		public static void RestartGame() {
			//restart the game
			if (getOS () == OS.Win) {
				string filename = getGlobalScrollsInstallPath () + "..\\..\\Scrolls.exe";
				if (!System.IO.File.Exists (filename)) {
					//If the Scrolls.exe does not exist we search for a .exe, that has the same name as the _Data folder (Scrolls.exe -> Scrolls_Data, ScrollsTest.exe -> ScrollsTest_Data)
					DirectoryInfo dir = new DirectoryInfo (getGlobalScrollsInstallPath ());
					dir = dir.Parent.Parent; //Go from Scrolls/game/Scrolls_data/Managed to Scrolls/game
					DirectoryInfo[] dirs = dir.GetDirectories ("*_Data");
					if (dirs.Length == 1) {
						string name = dirs [0].Name;
						name = name.Replace ("_Data", ".exe");
						FileInfo[] files = dir.GetFiles(name);
						if (files.Length == 1) {
							filename = files [0].FullName;
						} else {
							//... We just accept, that the Process will fail...
						}
					} 
				}
				new Process { StartInfo = { FileName = filename, Arguments = "" } }.Start ();
				Application.Quit ();
			} else if (getOS () == OS.Mac) {
				new Process { StartInfo = { FileName = "bash", Arguments = getGlobalScrollsInstallPath() + "/../../../../../run_sleep.sh", UseShellExecute=true } }.Start ();
				Application.Quit ();
			} else {
				Application.Quit ();
			}
		}


		public static void PlatformPatches(String path) {
			if (getOS() == OS.Mac) {
				String runsh = path + "/../../../../../run.sh";
				String runshsleep = path + "/../../../../../run_sleep.sh";

				if (!File.Exists (runshsleep)) {
					String[] lines = File.ReadAllLines (runsh);
					StreamWriter writer = File.CreateText (runshsleep);
					for (int i = 0; i < lines.Count(); i++) {
						String line = lines [i];
						if (line.Contains ("/installer") && !lines [i - 1].Contains ("sleep")) {
							writer.WriteLine ("sleep 2");
						}
						writer.WriteLine (line);
					}
					writer.Close ();
				}

				if (!File.Exists (path + "/System.Drawing.dll")) {
					byte[] sysdrawing = System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("ScrollsModLoader.Resources.System.Drawing.dll").ReadToEnd ();
					System.IO.File.Create (path + "/System.Drawing.dll").Write (sysdrawing, 0, sysdrawing.Length);
				}
			}
		}
	}
}

