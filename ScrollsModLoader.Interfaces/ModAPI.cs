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
	}
}