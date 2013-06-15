using System;
using System.IO;
using ScrollsModLoader.Interfaces;

namespace ScrollsModLoader
{
	public class APIHandler : ModAPI
	{
		private PatchSettingsMenu sceneHandler = null;
		private ModLoader loader;
		private LocalMod currentlyLoading = null;

		public APIHandler (ModLoader loader)
		{
			this.loader = loader;
		}

		public void setSceneCallback(PatchSettingsMenu patch) {
			sceneHandler = patch;
		}

		public bool AddScene (string desc, SceneProvider provider)
		{
			return sceneHandler.AddScene(desc, provider);
		}

		public void LoadScene (string providerDesc)
		{
			sceneHandler.LoadScene (providerDesc);
		}

		public string OwnFolder(BaseMod mod)
		{
			String installpath = null;

			foreach (String id in loader.modInstances.Keys) {
				if (loader.modInstances [id].Equals (mod))
					installpath = loader.modManager.installedMods.Find (delegate(LocalMod lmod) {
						return (lmod.localId.Equals (id));
					}).installPath;
			}

			if (installpath == null && currentlyLoading != null)
				return Path.GetDirectoryName(currentlyLoading.installPath);
			if (installpath == null)
				return Platform.getGlobalScrollsInstallPath () + "ModLoader" + Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + "Unknown" + Path.DirectorySeparatorChar;
			return Path.GetDirectoryName(installpath);
		}

		public void setCurrentlyLoading(LocalMod mod) {
			currentlyLoading = mod;
		}
	}
}

