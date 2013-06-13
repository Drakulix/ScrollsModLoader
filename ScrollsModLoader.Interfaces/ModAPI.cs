using System;

namespace ScrollsModLoader.Interfaces
{
	public interface ModAPI
	{
		//Folder
		string OwnFolder(String name);

		//GUI Stuff
		bool AddScene (String desc, SceneProvider provider);
		void LoadScene (String providerDesc);
	}
}