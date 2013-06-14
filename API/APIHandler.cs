using System;
using System.IO;
using ScrollsModLoader.Interfaces;

namespace ScrollsModLoader
{
	public class APIHandler : ModAPI
	{
		private PatchSettingsMenu sceneHandler = null;

		public APIHandler ()
		{
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

		public string OwnFolder(string modName)
		{
			return Platform.getGlobalScrollsInstallPath () + "ModLoader" + Path.DirectorySeparatorChar + "mods" + Path.DirectorySeparatorChar + modName;
		}
	}
}

