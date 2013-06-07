using System;
using UnityEngine;

namespace ScrollsModLoader.Interfaces
{
	public interface SceneProvider
	{
		String SceneName();
		void OnCreate(MonoBehaviour parentScene);
		void OnGUI();
		void OnDestroy();

		//returning null on the following is possible and will result in default settings
		GUISkin Skin();
		GUISkin UISkin();
		GUIStyle ButtonStyle();
		GUIStyle ActiveButtonStyle();
	}
}