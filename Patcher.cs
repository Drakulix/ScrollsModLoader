using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using LinFu.AOP.Cecil.Extensions;
using LinFu.Reflection.Emit;

namespace ScrollsModLoader
{
	class Patcher
	{

		/*
		 *  Actual patcher class
		 *  Makes use of Mono.Cecil and Linfu
		 * 
		 *  May be called directly via Main for first time patches
		 *  or from the inside of modloader to adjust(re-patch) the (backup)assembly for new game/mod/modloader updates
		 * 
		 */

		public static void Main (string[] args)
		{

			//get Path of Scrolls Data Folder
			String installPath = Platform.getGlobalScrollsInstallPath();
			if (installPath == null) return;


			//create modloader folder
			if (!System.IO.Directory.Exists(installPath+"Managed/ModLoader/")) {
				System.IO.Directory.CreateDirectory(installPath+"Managed/ModLoader/");
			}

			//backup original assembly
			if (!System.IO.File.Exists(installPath+"Managed/ModLoader/Assembly-CSharp.dll"))
				System.IO.File.Copy (installPath+"Managed/Assembly-CSharp.dll", installPath + "Managed/ModLoader/Assembly-CSharp.dll");
			else {
			//if a backup already exists, it is much more likely that the current assembly is messed up and the user wants to repatch
				System.IO.File.Delete(installPath+"Managed/Assembly-CSharp.dll");
				System.IO.File.Copy(installPath+"Managed/ModLoader/Assembly-CSharp.dll", installPath+"Managed/Assembly-CSharp.dll");
			}

			Console.WriteLine ("Patching...");
			//patch it
			Patcher patcher = new Patcher();
			if (!patcher.patchAssembly(installPath+"Managed/Assembly-CSharp.dll")) {
				Console.WriteLine("Patching failed");
				//don't save patch at this point. If the "real" patcher fails, we should tell the user instead
				//save-patching is for installs, that get broken by updates, etc, to keep the install until ScrollsModLoader is updated
				Dialogs.showNotification ("Patching failed", "ScrollsModLoader was unable to prepare your client, you are likely using an incompatible version. More at ScrollsGuide.com");
			}
			Console.WriteLine ("Done");

			return;
		}


		public bool patchAssembly(String path) {
			//"weave" the assembly
			if (!weaveAssembly (path))
				return false;
			Console.WriteLine ("Weaved Assembly");

			/*
			 * add init hack
			 */

			try {
				//load assembly
				Hooks.loadBaseAssembly(path);
				//load self
				Hooks.loadInjectAssembly(System.Reflection.Assembly.GetExecutingAssembly().Location);
			} catch (Exception exp) {
				//something must be gone horribly wrong if it crashes here
				Console.WriteLine (exp);
				return false;
			}

			//add hook
			if (!Hooks.hookStaticVoidMethodAtEnd ("App.Awake", "ModLoader.Init"))
				return false;

			try {
				//save assembly
				Hooks.savePatchedAssembly();
				
				//inject self (make a copy at first to fix empty assembly creation)
				String installPath = Platform.getGlobalScrollsInstallPath();
				if (installPath == null) return false;

				if (System.IO.File.Exists(installPath+"Managed/Assembly-CSharp.cpy.dll"))
					System.IO.File.Delete(installPath+"Managed/Assembly-CSharp.cpy.dll");
				System.IO.File.Copy (installPath+"Managed/Assembly-CSharp.dll", installPath+"Managed/Assembly-CSharp.cpy.dll");
				System.Reflection.Assembly.Load (System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ScrollsModLoader.ILRepack.exe").ReadToEnd()).EntryPoint.Invoke(null, new object[] { new string[] {"/out:"+path, installPath+"Managed/Assembly-CSharp.cpy.dll", System.Reflection.Assembly.GetExecutingAssembly().Location} });
				System.IO.File.Delete(installPath+"Managed/Assembly-CSharp.cpy.dll");

			} catch (Exception exp) {
				//also very unlikely, but for safety
				Console.WriteLine (exp);
				return false;
			}

			return true;
		}

		public bool weaveAssembly(String path) {
			// let LinFu inject some call hooks into all required classes and methods to replace/extend method calls
			try {
				AssemblyDefinition assembly = AssemblyFactory.GetAssembly(path);
				assembly.InterceptMethodBody (new ScrollsFilter.ScrollsMethodFilter(), new ScrollsFilter.ScrollsTypeFilter());
				assembly.Save(path);
				return true;
			} catch (Exception exp) {
				Console.WriteLine (exp);
				return false;
			}
		}

		public bool saveModePatchAssembly(String path) {
			return true;
		}
	}
}
