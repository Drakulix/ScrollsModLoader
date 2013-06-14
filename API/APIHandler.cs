using System;
using System.IO;
using ScrollsModLoader.Interfaces;

namespace ScrollsModLoader
{
	public class APIHandler : ModAPI
	{
		private PatchSettingsMenu sceneHandler = null;
		private ModLoader loader;

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
			String name = null;

			if (!loader.modInstances.ContainsValue(mod))
				name = "Unknown";
			foreach (String id in loader.modInstances.Keys) {
				if (loader.modInstances [id].Equals (mod))
					name = loader.modManager.installedMods.Find (delegate(LocalMod lmod) {
						return (lmod.id.Equals (id));
					}).installPath;
			}

			return Platform.getGlobalScrollsInstallPath () + "ModLoader" + Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + name;
		}
	}
}

