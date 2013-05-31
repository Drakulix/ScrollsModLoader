using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Net;
using LinFu.AOP.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using Ionic.Zip;
using System.Threading;

namespace ScrollsModLoader
{
	public class PatchUpdater : Patch
	{
		private Thread patching;

		public PatchUpdater(TypeDefinitionCollection types) : base (types) {

		}

		public override MethodDefinition[] patchedMethods() {
			MethodDefinition PopupOk = Hooks.getMethDef (Hooks.getTypeDef (assembly, "Login"), "PopupOk");
			return new MethodDefinition[] {PopupOk};
		}

		public override object Intercept (IInvocationInfo info) {
			/*Dictionary<string, int> map = null;
			FieldInfo field = null;
			try {
				field = typeof(Login).GetField ("_<>f__switch$map21", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
			} catch (NullReferenceException exp) {
				return info.TargetMethod.Invoke (info.Target, info.Arguments);
			}
			try {
				map = (Dictionary<string, int>)field.GetValue(info.Target);
			} catch (NullReferenceException exp) {
				return info.TargetMethod.Invoke (info.Target, info.Arguments);
			}

			int i = 0;
			if (info.Arguments [0] != null && map != null && map.TryGetValue (((String)info.Arguments[0]), out i) && i == 0) {*/
			//update
				
				
				
			//show popup
			App.Popups.ShowInfo ("Patching", "Please wait, while Scrolls is patching");

			patching = new Thread (new ThreadStart(PatchScrolls));
			patching.Start();

			return null;
		}

		public void PatchScrolls() {

			String URL;
			if (Platform.getOS () == Platform.OS.Win) {
				URL = "http://download.scrolls.com/client/windows.zip";
			} else {
				URL = "http://download.scrolls.com/client/mac.zip";
			}

			String gameFolder = Path.GetFullPath(Platform.getGlobalScrollsInstallPath() + "/../../../../");

			//wait
			WebClient webClient = new WebClient();
			if (File.Exists (gameFolder + "/game.zip"))
				File.Delete (gameFolder + "/game.zip");
			webClient.DownloadFile(URL, gameFolder + "/game.zip");

			//backup assembly
			String backupPath = gameFolder +"/ScrollsModLoader.dll";
			if (File.Exists (backupPath))
				File.Delete (backupPath);
			File.Copy (Platform.getGlobalScrollsInstallPath ()+"ScrollsModLoader.dll", backupPath);

			//backup modloader folder
			String modBackupPath = gameFolder + "/ModLoader";
			if (Directory.Exists (modBackupPath))
				Extensions.DeleteDirectory (modBackupPath);
			Directory.Move (Platform.getGlobalScrollsInstallPath ()+"ModLoader", modBackupPath);
			File.Delete (modBackupPath+"/mods.ini");
			File.Delete (modBackupPath+"/Assembly-CSharp.dll");

			if (Platform.getOS () == Platform.OS.Win) {

			} else {
				Extensions.DeleteDirectory (gameFolder+ "/MacScrolls.app");
			}

			//extract
			ZipFile zip = ZipFile.Read(gameFolder + "/game.zip");
			foreach (ZipEntry e in zip)
			{
				e.Extract();
			}

			//move assembly
			File.Copy (backupPath, Platform.getGlobalScrollsInstallPath () + "ScrollsModLoader.dll");
			File.Delete (backupPath);

			//move modloader folder back
			Directory.Move (modBackupPath, Platform.getGlobalScrollsInstallPath ()+"ModLoader");

			//make new repatch backup
			File.Copy (Platform.getGlobalScrollsInstallPath () + "Assembly-CSharp.dll", Platform.getGlobalScrollsInstallPath () + "ModLoader/Assembly-CSharp.dll");

			//repatch
			Patcher patcher = new Patcher ();
			if (!patcher.patchAssembly ()) {
				if (!patcher.saveModePatchAssembly ()) {
					//TO-DO implement
					
				}
			}

			//restart the game
			if (Platform.getOS () == Platform.OS.Win) {
				new Process { StartInfo = { FileName = Platform.getGlobalScrollsInstallPath() + "/../../Scrolls.exe", Arguments = "" } }.Start ();
				Application.Quit ();
			} else if (Platform.getOS () == Platform.OS.Mac) {
				new Process { StartInfo = { FileName = Platform.getGlobalScrollsInstallPath() + "/../../../../../run.sh", Arguments = "", UseShellExecute=true } }.Start ();
				Application.Quit ();
			} else {
				Application.Quit ();
			}

			//} else
			//	return info.TargetMethod.Invoke (info.Target, info.Arguments);

		}

	}
}

