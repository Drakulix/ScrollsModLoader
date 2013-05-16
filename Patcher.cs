using System;

namespace ScrollsModLoader
{
	class Patcher
	{
		public static void Main (string[] args)
		{
			/* TO-DO
			 * get path of Scrolls
			 * backup old Assembly
			 * patch it
			 * run ILRepack and inject self
			 */


			//get Path of Scrolls Data Folder
			//Note: AllowedFileTypes is currently broken on MonoMac. it freezes the app

			String installPath = Platform.getGlobalScrollsInstallPath();
			if (installPath == null) return;


			//create modloader folder
			if (!System.IO.Directory.Exists(installPath+"/Managed/ModLoader/")) {
				System.IO.Directory.CreateDirectory(installPath+"/Managed/ModLoader/");
			}

			//backup original assembly
			System.IO.File.Copy(installPath+"/Managed/Assembly.CSharp.dll", installPath+"/Managed/ModLoader/Assembly.CSharp.dll");


			//patch it
			Patcher patcher = new Patcher();
			if (!patcher.patchAssembly(installPath+"/Managed/ModLoader/Assembly.CSharp.dll")) {
				Console.WriteLine("Patching failed");
			}
		}


		public bool patchAssembly(String path) {

			return true;
		}

		public bool saveModePatchAssembly(String path) {

			return true;
		}
	}
}
