using System;

namespace ScrollsModLoader.Interfaces
{
	public interface ModAPI
	{
		//Folder
		string OwnFolder(BaseMod mod);

		//File Open
		string FileOpenDialog ();

		//GUI Stuff
		bool AddScene (String desc, SceneProvider provider);
		void LoadScene (String providerDesc);

		void ShowLogin (Popups popups, IOkStringsCancelCallback callback, string username, string problems, string popupType, string header, string description, string okText);
	}
}