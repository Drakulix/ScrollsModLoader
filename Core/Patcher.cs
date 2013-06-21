using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using LinFu.AOP.Cecil.Extensions;
using LinFu.Reflection.Emit;
using UnityEngine;

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

		public List<String> modPaths = new List<String>();

		public static void Main (string[] args)
		{

			Console.WriteLine ("Preparing...");

			//get Path of Scrolls Data Folder
			String installPath = Platform.getGlobalScrollsInstallPath();
			if (installPath == null) return;

			//create modloader folder
			if (!System.IO.Directory.Exists(installPath+"ModLoader")) {
				System.IO.Directory.CreateDirectory(installPath+"ModLoader");
			}

			//backup original assembly
			if (!System.IO.File.Exists(installPath+"ModLoader"+ System.IO.Path.DirectorySeparatorChar +"Assembly-CSharp.dll"))
				System.IO.File.Copy (installPath+"Assembly-CSharp.dll", installPath + "ModLoader"+ System.IO.Path.DirectorySeparatorChar +"Assembly-CSharp.dll");
			else {
			//if a backup already exists, it is much more likely that the current assembly is messed up and the user wants to repatch
				System.IO.File.Delete(installPath+"Assembly-CSharp.dll");
				System.IO.File.Copy(installPath+"ModLoader"+ System.IO.Path.DirectorySeparatorChar +"Assembly-CSharp.dll", installPath+"Assembly-CSharp.dll");
			}

			//copy modloader for patching
			if (System.IO.File.Exists(installPath+"ScrollsModLoader.dll"))
				System.IO.File.Delete(installPath+"ScrollsModLoader.dll");
			System.IO.File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, installPath+"ScrollsModLoader.dll");

			//reset ini
			if (System.IO.File.Exists(installPath+"ModLoader"+ System.IO.Path.DirectorySeparatorChar +"mods.ini"))
				System.IO.File.Delete(installPath+"ModLoader"+ System.IO.Path.DirectorySeparatorChar +"mods.ini");

			Console.WriteLine ("Patching...");
			//patch it
			Patcher patcher = new Patcher();
			if (!patcher.patchAssembly(installPath)) {
				Console.WriteLine("Patching failed");
				//don't safe patch at this point. If the "real" patcher fails, we should tell the user instead
				//save-patching is for installs, that get broken by updates, etc, to keep the install until ScrollsModLoader is updated
				Dialogs.showNotification ("Patching failed", "Scrolls Summoner was unable to prepare your client, you are likely using an incompatible version. More at ScrollsGuide.com");
				return;
			}

			Dialogs.showNotification ("Patching done", "Scrolls Summoner successfully patched your Scrolls install. More infos at ScrollsGuide.com");
			Console.WriteLine ("Done");
			return;
		}


		public bool patchAssembly(String installPath) {
			if (installPath == null) return false;

			//"weave" the assembly
			Console.WriteLine ("------------------------------");
			Console.WriteLine ("ModLoader Hooks:");
			ScrollsFilter.Log ();
			Console.WriteLine ("------------------------------");

			if (!weaveAssembly (installPath+"Assembly-CSharp.dll"))
				return false;
			Console.WriteLine ("Weaved Assembly");

			/*
			 * add init hack
			 */

			try {
				//load assembly
				Hooks.loadBaseAssembly(installPath+"Assembly-CSharp.dll");
				//load self
				Hooks.loadInjectAssembly(installPath+"ScrollsModLoader.dll");
			} catch (Exception exp) {
				//something must be gone horribly wrong if it crashes here
				Console.WriteLine (exp);
				return false;
			}

			//add hooks
			if (!Hooks.hookStaticVoidMethodAtEnd ("App.Awake", "ModLoader.Init"))
				return false;

			try {

				//save assembly
				Hooks.savePatchedAssembly();

				Platform.PlatformPatches(installPath);
				
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
				assembly.InterceptMethodBody (new ScrollsFilter(), new ScrollsFilter());
				//assembly.InterceptMethodBody(new ScrollsFilter().ShouldWeave);
				assembly.Save(path);
				return true;
			} catch (Exception exp) {
				Console.WriteLine (exp);
				return false;
			}
		}

		public bool safeModePatchAssembly() {
			String installPath = Platform.getGlobalScrollsInstallPath();
			if (installPath == null) return false;

			try {
				//load assembly
				Hooks.loadBaseAssembly(installPath+"Assembly-CSharp.dll");
				//load self
				Hooks.loadInjectAssembly(installPath+"ScrollsModLoader.dll");
			} catch (Exception exp) {
				//something must be gone horribly wrong if it crashes here
				Console.WriteLine (exp);
				return false;
			}

			if (!Hooks.hookStaticVoidMethodAtEnd ("App.Awake", "Patcher.safeLaunch"))
				return false;

			try {

				//save assembly
				Hooks.savePatchedAssembly();

				Platform.PlatformPatches(installPath);

			} catch (Exception exp) {

				//also very unlikely, but for safety
				Console.WriteLine (exp);
				return false;

			}

			return true;
		}

		public static void safeLaunch() {

			//if we get here, we NEED an update

			String installPath = Platform.getGlobalScrollsInstallPath();
			if (System.IO.File.Exists (installPath + System.IO.Path.DirectorySeparatorChar + "check.txt")) {
				System.IO.File.Delete (installPath + System.IO.Path.DirectorySeparatorChar + "check.txt");
				new Patcher ().patchAssembly (installPath);
			}
			if (Updater.tryUpdate()) { //updater did succeed
				System.IO.File.CreateText (installPath + System.IO.Path.DirectorySeparatorChar + "check.txt");
				Application.Quit ();
			}
		}
	}
}