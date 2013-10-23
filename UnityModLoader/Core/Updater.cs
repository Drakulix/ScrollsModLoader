using System;
using System.IO;
using System.Net;
using System.Reflection;
using JsonFx.Json;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace ScrollsModLoader
{
	public class Updater
	{

		private static byte[] token = new byte[] { 8, 95, 174, 161, 22, 41, 180, 133 }; //public key

		public static bool tryUpdate() {

			WebClientTimeOut client = new WebClientTimeOut ();
			String versionMessageRaw;
			try {
				versionMessageRaw = client.DownloadString (new Uri("http://mods.scrollsguide.com/version"));
			} catch (WebException) {
				return false;
			}

			JsonReader reader = new JsonReader ();
			VersionMessage versionMessage = (VersionMessage)reader.Read (versionMessageRaw, typeof(VersionMessage));

			int version = versionMessage.version ();
			String installPath = Platform.getGlobalScrollsInstallPath () + Path.DirectorySeparatorChar + "ModLoader" + Path.DirectorySeparatorChar;

			try {
				File.Delete (installPath + "Updater.exe");
			} catch {}

			if (!System.IO.Directory.Exists(installPath)) {
				System.IO.Directory.CreateDirectory(installPath);
			}

			if (version > ModLoader.getVersion()) {

				byte[] asm;
				try {
					asm = client.DownloadData(new Uri("http://mods.scrollsguide.com/download/update"));
				} catch (WebException) {
					return false;
				}
				File.WriteAllBytes (installPath + "Updater.exe", asm);
				if (CheckToken (installPath + "Updater.exe", token)) {

					try {
						App.Popups.ShowInfo ("Scrolls Summoner is updating", "Please wait while the update is being downloaded");
						Dialogs.showNotification("Scrolls Summoner is updating", "Please wait while the update is being downloaded");
					} catch { }

					if (Platform.getOS () == Platform.OS.Win) {
						new Process { StartInfo = { FileName = installPath + "Updater.exe", Arguments = "" } }.Start ();
					} else if (Platform.getOS () == Platform.OS.Mac) {
						Assembly.LoadFrom (installPath + "Updater.exe").EntryPoint.Invoke (null, new object[] { new string[] {} });
					}
					return true;
				}

				try {
					App.Popups.KillCurrentPopup();
				} catch {}
			}

			return false;
		}

		public static bool CheckToken(string assembly, byte[] expectedToken)
		{
			try
			{ 
				// Get the public key token of the given assembly 
				Assembly asm = Assembly.LoadFrom(assembly);
				byte[] asmToken =  asm.GetName().GetPublicKeyToken();

				// Compare it to the given token
				if(asmToken.Length != expectedToken.Length)
					return false;

				for(int i = 0; i < asmToken.Length; i++)
					if(asmToken[i] != expectedToken[i])
						return false;

				return true;
			}
			catch(System.IO.FileNotFoundException)
			{
				// couldn't find the assembly
				return false;
			}
			catch(BadImageFormatException)
			{
				// the given file couldn't get through the loader
				return false;
			}
		}
	}
}

