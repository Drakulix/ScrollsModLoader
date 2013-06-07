using System;
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
	}
}

