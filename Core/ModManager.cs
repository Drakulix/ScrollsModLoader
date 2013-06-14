using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScrollsModLoader.Interfaces;
using JsonFx.Json;
using UnityEngine;

namespace ScrollsModLoader
{

	public class ModManager
	{

		private String modsPath;
		private ModLoader loader;
		private RepoManager repoManager;

		//private List<BaseMod> modInstances = new List<BaseMod>();
		public List<LocalMod> installedMods = new List<LocalMod> ();


		public ModManager(ModLoader loader) {

			this.loader = loader;
			repoManager = new RepoManager ();

			String installPath = Platform.getGlobalScrollsInstallPath();
			String modLoaderPath = installPath + "ModLoader" + System.IO.Path.DirectorySeparatorChar;

			if (!Directory.Exists (modLoaderPath+"mods"))
				Directory.CreateDirectory (modLoaderPath+"mods");

			modsPath = modLoaderPath + "mods";

			this.loadInstalledMods ();

		}




		public void loadInstalledMods() {

			List<String> folderList = (from subdirectory in Directory.GetDirectories(modsPath)
			                       where Directory.GetFiles(subdirectory, "*.mod.dll").Length != 0
			                       select subdirectory).ToList();

			foreach (String folder in folderList) {
				LocalMod mod = null;
				if (File.Exists (folder + Path.DirectorySeparatorChar + "config.json")) {
					JsonReader reader = new JsonReader ();
					mod = (LocalMod) reader.Read (File.ReadAllText (folder + Path.DirectorySeparatorChar + "config.json"), typeof(LocalMod));
				} else {
					//new local installed mod
					Mod localMod = loader.loadModStatic (Directory.GetFiles (folder, "*.mod.dll") [0]);
					if (localMod != null) {
						mod = new LocalMod(true, Path.GetDirectoryName(folder), null, null, null, true, localMod.name, localMod.description, localMod.version, localMod.versionCode);
						loader.queueRepatch ();
					}
				}
				if (mod != null)
					installedMods.Add (mod);
			}

		}

		public void checkForUpdates() {
			foreach (LocalMod mod in installedMods) {
				Mod onlineMod = repoManager.getModOnRepo (mod.source, mod);
				if (onlineMod == null)
					continue;
				if (onlineMod.version > mod.version) {
					mod.version = onlineMod.version;
					this.updateMod (mod);
				}
			}
		}





		public void installMod(Repo repo, Mod mod) {

			LocalMod lmod = new LocalMod (false, null, this.generateUniqueID(), mod.id, repo, true, mod.name, mod.description, mod.version, mod.versionCode); 

			String folder = modsPath + Path.DirectorySeparatorChar + lmod.localId + Path.DirectorySeparatorChar;
			if (Directory.Exists (folder))
				Directory.Delete (folder);
			Directory.CreateDirectory (folder);

			this.updateMod (lmod);
		}

		public void deinstallMod(LocalMod mod) {
			loader.unloadMod (mod);
			installedMods.Remove (mod);
			String folder = modsPath + Path.DirectorySeparatorChar + mod.localId + Path.DirectorySeparatorChar;
			if (Directory.Exists (folder))
				Directory.Delete (folder);
		}

		public void updateMod(LocalMod mod) {
			this.updateFile (mod);
			this.updateConfig (mod);
			loader.queueRepatch ();
		}

		public byte[] downloadMod(LocalMod mod) {
			return new WebClient ().DownloadData (mod.source.url + "/download/mod/" + mod.id);
		}

		public void updateFile(LocalMod mod) {
			String folder = modsPath + Path.DirectorySeparatorChar + mod.localId + Path.DirectorySeparatorChar;
			String filePath = folder + mod.name + ".mod.dll";
			byte[] modData = this.downloadMod(mod);

			File.Delete (filePath);
			FileStream modFile = File.Create (filePath);
			modFile.Write (modData, 0, modData.Length);
			modFile.Flush ();
			modFile.Close ();
		}

		public void updateConfig(LocalMod mod) {
			//update config
			String folder = modsPath + Path.DirectorySeparatorChar + mod.localId + Path.DirectorySeparatorChar;
			File.Delete (folder + "config.json");
			StreamWriter configFile = File.CreateText (folder + "config.json");
			configFile.Write (this.jsonConfigFromMod (mod));
			configFile.Flush ();
			configFile.Close ();
		}




		public void enableMod(LocalMod mod) {
			mod.enabled = true;
			this.updateConfig (mod);
		}
		public void disableMod(LocalMod mod) {
			mod.enabled = false;
			this.updateConfig (mod);
		}




		public String jsonConfigFromMod(LocalMod mod) {
			JsonWriter writer = new JsonWriter ();
			return writer.Write (mod);
		}

		public string generateUniqueID() {
			bool searching = true;
			String newGuid = null;
			while (searching) {
				newGuid = Guid.NewGuid ().ToString ("N");
				searching = false;
				foreach (LocalMod mod in installedMods)
					if (mod.localId.Equals (newGuid))
						searching = true;
			}
			return newGuid;
		}
	}

	public class LocalMod : Mod {

		public bool localInstall;
		public String installPath;
		public String localId;
		public Repo source;
		public bool enabled;

		public LocalMod(bool localInstall, String installPath, String localId, String serverId, Repo source, bool enabled, String name, String description, int version, String versionCode) {
			this.localId = localId;
			this.localInstall = localInstall;
			if (localInstall)
				this.installPath = installPath;
			else
				this.installPath = localId;
			this.source = source;
			this.enabled = enabled;

			this.id = serverId;
			this.name = name;
			this.description = description;
			this.version = version;
			this.versionCode = versionCode;
		}

		public override bool Equals(object mod) {
			if (mod is LocalMod) {
				return (mod as LocalMod).localId.Equals (this.localId);
			} else
				return false;
		}
	}

	public class Mod : Item {

		public String id;
		public String name;
		public String description;
		public int version;
		public String versionCode;

		public bool selectable ()
		{
			return true;
		}
		public Texture getImage ()
		{
			return null;
		}
		public string getName ()
		{
			return name;
		}
		public string getDesc ()
		{
			return description+", "+versionCode;
		}

		public override bool Equals(object mod) {
			if (mod is Mod) {
				return (mod as Mod).id.Equals (this.id);
			} else
				return false;
		}
	}
}

