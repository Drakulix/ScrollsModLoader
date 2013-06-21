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

			return new MethodDefinition[] { PopupOk };//, Name};
		}

		public override object Intercept (IInvocationInfo info) {

			if (!info.Arguments[0].Equals("update"))	{
				return info.TargetMethod.Invoke (info.Target, info.Arguments);
			}
				
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

			String gameFolder = Path.GetFullPath(Directory.GetParent(Platform.getGlobalScrollsInstallPath()).Parent.Parent.Parent.FullName)+ Path.DirectorySeparatorChar;

			//wait
			WebClient webClient = new WebClient();
			if (File.Exists (gameFolder + "game.zip"))
				File.Delete (gameFolder + "game.zip");
			webClient.DownloadFile(URL, gameFolder + "game.zip");

			//backup assembly
			String backupPath = gameFolder +"ScrollsModLoader.dll";
			if (File.Exists (backupPath))
				File.Delete (backupPath);
			File.Copy (Platform.getGlobalScrollsInstallPath ()+"ScrollsModLoader.dll", backupPath);

			//backup modloader folder
			String modBackupPath = gameFolder + "ModLoader";
			if (Directory.Exists (modBackupPath))
				Extensions.DeleteDirectory (modBackupPath);
			Directory.Move (Platform.getGlobalScrollsInstallPath ()+"ModLoader", modBackupPath);
			File.Delete (modBackupPath+"mods.ini");
			File.Delete (modBackupPath+"Assembly-CSharp.dll");

			if (Platform.getOS () == Platform.OS.Win) {
				Extensions.DeleteDirectory (gameFolder+ "Scrolls_Data");
				File.Delete (gameFolder + "Scrolls.exe");
			} else {
				Extensions.DeleteDirectory (gameFolder+ "MacScrolls.app");
			}

			//extract
			ZipFile zip = ZipFile.Read(gameFolder + "game.zip");
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
			File.Copy (Platform.getGlobalScrollsInstallPath () + "Assembly-CSharp.dll", Platform.getGlobalScrollsInstallPath () + "ModLoader" + Path.DirectorySeparatorChar + "Assembly-CSharp.dll");

			//repatch
			Patcher patcher = new Patcher ();
			if (!patcher.patchAssembly (Platform.getGlobalScrollsInstallPath ())) {
				if (!patcher.safeModePatchAssembly ()) {
					Dialogs.showNotification ("Scrolls Summoner patch failed", "Scrolls Summoner failed in patch itself into the updated files. It will uninstall itself, for more informations visit scrollsguide.com");
					File.Delete (Platform.getGlobalScrollsInstallPath () + "ScrollsModLoader.dll");
					Extensions.DeleteDirectory (Platform.getGlobalScrollsInstallPath () + "ModLoader");
				}
			}

			//restart the game
			Platform.RestartGame ();

		}

	}
}

