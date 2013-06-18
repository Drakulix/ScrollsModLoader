using System;
using System.IO;
using System.Net;
using System.Reflection;
using JsonFx.Json;

namespace ScrollsModLoader
{
	public class Updater
	{

		private static int myVersion = 1;
		private static byte[] token = {};

		public static void updateIfNeeded() {
			if (tryUpdate ())
				Platform.RestartGame ();
		}

		public static bool tryUpdate() {

			WebClient client = new WebClient ();
			String versionMessageRaw = client.DownloadString (new Uri("http://mods.scrollsguide.com/version"));

			JsonReader reader = new JsonReader ();
			VersionMessage versionMessage = (VersionMessage)reader.Read (versionMessageRaw, typeof(VersionMessage));

			int version = versionMessage.version ();

			if (version > myVersion) {

				byte[] asm = client.DownloadData(new Uri("http://mods.scrollsguide.com/download/update"));
				String installPath = Platform.getGlobalScrollsInstallPath () + Path.DirectorySeparatorChar + "ModLoader" + Path.DirectorySeparatorChar;
				FileStream updater = File.Create (installPath + "Updater.exe");
				updater.Write (asm, 0, asm.Length);
				updater.Close ();
				if (CheckToken (installPath + "Updater.exe", token)) {
					Assembly.LoadFrom (installPath + "Updater.exe").EntryPoint.Invoke (null, new object[] { new string[] {} });
					return true;
				} else {
					File.Delete (installPath + "Updater.exe");
				}
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

