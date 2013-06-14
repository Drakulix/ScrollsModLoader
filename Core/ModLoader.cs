using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using LinFu.AOP.Interfaces;
using LinFu.Reflection;
using Mono.Cecil;
using OX.Copyable;
using ScrollsModLoader.Interfaces;
using UnityEngine;

namespace ScrollsModLoader {

	/*
	 * handels mod loading and debug file logging
	 * this class should only pass invokes through itself to the right mods
	 * 
	 */

	public class ModInterceptor : IInterceptor
	{
		private ModLoader loader;
		private TypeDefinitionCollection types;

		public ModInterceptor(ModLoader loader) {
			this.loader = loader;
			types = AssemblyFactory.GetAssembly (Platform.getGlobalScrollsInstallPath()+"ModLoader/Assembly-CSharp.dll").MainModule.Types;

		}

		public object Intercept (IInvocationInfo info)
		{
			//list for unloading
			List<String> modsToUnload = new List<String> ();

			//load beforeinvoke
			foreach (String id in loader.modOrder) {
				BaseMod mod = loader.modInstances [id];
				MethodDefinition[] requestedHooks = (MethodDefinition[])mod.GetType().GetMethod("GetHooks").Invoke(null, new object[] { types, SharedConstants.getGameVersion() } );
				if (requestedHooks.Where(item => item.Name.Equals(info.TargetMethod.Name) && item.DeclaringType.Name.Equals(info.TargetMethod.DeclaringType.Name)).Count() > 0) {
					object returnVal;
					try {
						if (mod.BeforeInvoke(new InvocationInfo(info), out returnVal)) {
							return returnVal;
						}
					} catch (Exception exp) {
						Console.WriteLine(exp);
						modsToUnload.Add (id);
					}
				}
			}

			//unload
			foreach (String id in modsToUnload) {
				loader.unloadMod (loader.modManager.installedMods.Find (delegate(LocalMod lmod) {
					return (lmod.id.Equals (id));
				}));
			}
			modsToUnload.Clear ();


			//check for patch call
			object ret = null;
			bool patchFound = false;
			foreach (Patch patch in loader.patches) {
				if (patch.patchedMethods ().Where (item => item.Name.Equals (info.TargetMethod.Name) && item.DeclaringType.Name.Equals (info.TargetMethod.DeclaringType.Name)).Count () > 0) {
					ret = patch.Intercept (info);
					patchFound = true;
				}
			}
			if (!patchFound)
				ret = info.TargetMethod.Invoke (info.Target, info.Arguments);


			//backup return value
			object retBack = null;
			try {
				retBack = ret.Copy ();
			} catch {
				retBack = ret;
			}


			//load afterinvoke
			foreach (String id in loader.modOrder) {
				BaseMod mod = loader.modInstances [id];
				MethodDefinition[] requestedHooks = (MethodDefinition[]) mod.GetType().GetMethod("GetHooks").Invoke(null, new object[] { types, SharedConstants.getGameVersion() } );
				if (requestedHooks.Where(item => item.Name.Equals(info.TargetMethod.Name) && item.DeclaringType.Name.Equals(info.TargetMethod.DeclaringType.Name)).Count() > 0) {
					try {
						mod.AfterInvoke (new InvocationInfo(info), ref ret);
						try {
							retBack = ret.Copy ();
						} catch {
							retBack = ret;
						}
					} catch (Exception exp) {
						ret = retBack;
						Console.WriteLine(exp);
						modsToUnload.Add (id);
					}
				}
			}

			//unload
			foreach (String id in modsToUnload) {
				loader.unloadMod (loader.modManager.installedMods.Find (delegate(LocalMod lmod) {
					return (lmod.id.Equals (id));
				}));
			}
			modsToUnload.Clear ();

			return ret;
		}
		
	}

	public class SimpleMethodReplacementProvider : IMethodReplacementProvider 
	{
		private ModInterceptor interceptor;
		public SimpleMethodReplacementProvider(ModLoader loader) {
			interceptor = new ModInterceptor (loader);
		}

		public bool CanReplace (object host, IInvocationInfo info)
		{
			StackTrace trace = info.StackTrace;
			//Console.WriteLine (trace);
			foreach (StackFrame frame in trace.GetFrames()) {
				if (frame.GetMethod ().Name.Equals(info.TargetMethod.Name))
					return false;
			}
			return true;
		}

		public IInterceptor GetMethodReplacement (object host, IInvocationInfo info)
		{
			return interceptor;
		}
	}
	

	public class ModLoader
	{
		static bool init = false;
		static ModLoader instance = null;
		private String modLoaderPath;

		public ModManager modManager;
		public List<String> modOrder = new List<String>();

		private bool isRepatchNeeded = false;

		public Dictionary<String, BaseMod> modInstances = new Dictionary<String, BaseMod>();
		public List<Patch> patches = new List<Patch>();

		private APIHandler publicAPI = null;

		public ModLoader ()
		{
			modLoaderPath = Platform.getGlobalScrollsInstallPath() + System.IO.Path.DirectorySeparatorChar + "ModLoader" + System.IO.Path.DirectorySeparatorChar;


			//load installed mods
			modManager = new ModManager (this);


			//load order list
			if (!File.Exists (modLoaderPath+"mods.ini")) {
				File.CreateText (modLoaderPath+"mods.ini").Close();
				//first launch, set hooks for patches
				this.queueRepatch();
			}
			modOrder = File.ReadAllLines (modLoaderPath+"mods.ini").ToList();



			//match order with installed mods
			foreach (LocalMod mod in modManager.installedMods) {
				if (mod.enabled)
					if (!modOrder.Contains (mod.localId))
						modOrder.Add (mod.localId);
			}

			//clean up not available mods
			foreach (String id in modOrder.ToArray()) {
				if (modManager.installedMods.Find (delegate(LocalMod mod) {
					return (mod.id.Equals (id));
				}) == null)
					modOrder.Remove (id);
			}



			//get Scrolls Types list
			TypeDefinitionCollection types = AssemblyFactory.GetAssembly (modLoaderPath+"Assembly-CSharp.dll").MainModule.Types;

			//get ModAPI
			publicAPI = new APIHandler (this);

			//loadPatches
			this.loadPatches (types);

			//loadModsStatic
			this.loadModsStatic (types);

			//save ModList
			File.Delete (modLoaderPath+"mods.ini");
			StreamWriter modOrderWriter = File.CreateText (modLoaderPath+"mods.ini");
			foreach (String modId in modOrder) {
				modOrderWriter.WriteLine (modId);
			}
			modOrderWriter.Flush ();
			modOrderWriter.Close ();

			//repatch
			this.repatchIfNeeded ();
		}

		public void loadPatches(TypeDefinitionCollection types) {

			//get Patches
			patches.Add (new PatchUpdater (types));
			//patches.Add(new PatchOffline(types));

			PatchSettingsMenu settingsMenuHook = new PatchSettingsMenu (types);
			publicAPI.setSceneCallback (settingsMenuHook);
			patches.Add (settingsMenuHook);

			PatchModsMenu modMenuHook = new PatchModsMenu (types);
			modMenuHook.Initialize (publicAPI);
			patches.Add (modMenuHook);

			//add Hooks
			foreach (Patch patch in patches) {
				foreach (MethodDefinition definition in patch.patchedMethods())
					ScrollsFilter.AddHook (definition);
			}
		}

		public void loadModsStatic(TypeDefinitionCollection types) {
			//get Mods
			foreach (LocalMod mod in modManager.installedMods) {
				if (mod.enabled) {
					if (this.loadModStatic (mod.installPath) == null) {
						modManager.disableMod(mod);
						modOrder.Remove (mod.localId);
					}
				}
			}
		}

		public Mod loadModStatic(String filePath) {
			//get Scrolls Types list
			TypeDefinitionCollection types = AssemblyFactory.GetAssembly (modLoaderPath+"Assembly-CSharp.dll").MainModule.Types;
			return this._loadModStatic (types, filePath);
		}

		public void loadModStatic(TypeDefinitionCollection types, String filepath) {
			this._loadModStatic (types, filepath);
		}

		public Mod _loadModStatic(TypeDefinitionCollection types, String filepath) {
			ResolveEventHandler resolver = new ResolveEventHandler(CurrentDomainOnAssemblyResolve);
			AppDomain.CurrentDomain.AssemblyResolve += resolver;
			

			Assembly modAsm = Assembly.LoadFile(filepath);
			Type modClass = (from _modClass in modAsm.GetTypes ()
			                 where _modClass.InheritsFrom (typeof(ScrollsModLoader.Interfaces.BaseMod))
			                 select _modClass).First();

			//no mod classes??
			if (modClass == null) {
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return null;
			}

			//get hooks
			MethodDefinition[] hooks = (MethodDefinition[]) modClass.GetMethod ("GetHooks").Invoke (null, new object[] {
				types,
				SharedConstants.getGameVersion ()
			});

			TypeDefinition[] typeDefs = new TypeDefinition[types.Count];
			types.CopyTo(typeDefs, 0);

			//check hooks
			foreach (MethodDefinition hook in hooks) {
				//type/method does not exists
				if ((from type in typeDefs
				     where type.Equals(hook.DeclaringType)
				     select type).Count() == 0) {
					//disable mod
					AppDomain.CurrentDomain.AssemblyResolve -= resolver;
					return null;
				}
			}

			//add hooks
			foreach (MethodDefinition hook in hooks) {
				ScrollsFilter.AddHook (hook);
			}

			//mod object for local mods on ModManager
			Mod mod = new Mod();
			mod.id = "00000000000000000000000000000000";
			mod.name = (String)modClass.GetMethod("GetName").Invoke(null, null);
			mod.version = (int)modClass.GetMethod("GetVersion").Invoke(null, null);
			mod.versionCode = ""+mod.version;
			mod.description = "";

			AppDomain.CurrentDomain.AssemblyResolve -= resolver;
			return mod;
		}


		public void loadMods() {

			foreach (String id in modOrder) {
				this.loadMod(modManager.installedMods.Find (delegate(LocalMod mod) {
					return (mod.id.Equals (id));
				}));
			}

		}

		public void loadMod(LocalMod mod) {

			ResolveEventHandler resolver = new ResolveEventHandler(CurrentDomainOnAssemblyResolve);
			AppDomain.CurrentDomain.AssemblyResolve += resolver;

			Assembly modAsm = Assembly.LoadFile(mod.installPath);
			Type modClass = (from _modClass in modAsm.GetTypes ()
			                 where _modClass.InheritsFrom (typeof(ScrollsModLoader.Interfaces.BaseMod))
			                 select _modClass).First();


			//no mod classes??
			if (modClass == null) {
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return;
			}

			modInstances.Add(mod.localId, (BaseMod)(modClass.GetConstructor (Type.EmptyTypes).Invoke (new object[0])));

		}

		public void unloadMod(LocalMod mod) {

			modManager.disableMod (mod);
			modOrder.Remove (mod.localId);
			modInstances.Remove (mod.localId);

		}

		public void queueRepatch() {
			isRepatchNeeded = true;
		}

		public void repatchIfNeeded() {
			if (isRepatchNeeded) {

				Patcher patcher = new Patcher ();
				if (!patcher.patchAssembly ()) {
					//normal patching should never fail at this point
					//because this is no update and we are already patched
					//TO-DO get hook that crashed the patching and deactive mod instead
					//No idea how to do that correctly however
					Dialogs.showNotification ("Scrolls is broken", "Your Scrolls install appears to be broken or modified by other tools. ModLoader failed to load and will de-install itself");
					File.Delete(Platform.getGlobalScrollsInstallPath()+"Assembly-CSharp.dll");
					File.Copy(Platform.getGlobalScrollsInstallPath()+"ModLoader"+ System.IO.Path.DirectorySeparatorChar +"Assembly-CSharp.dll", Platform.getGlobalScrollsInstallPath()+"Assembly-CSharp.dll");
					Application.Quit();
				}

				Platform.RestartGame ();
			}
		}



		private static System.Reflection.Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			try {
				var asm = (from a in AppDomain.CurrentDomain.GetAssemblies()
				           where a.GetName().FullName == args.Name
				           select a).FirstOrDefault();
				Console.WriteLine(args.Name);
				if (asm == null) {
					asm = System.Reflection.Assembly.GetExecutingAssembly();
				}
				return asm;
			} catch (NullReferenceException exp) {
				Console.WriteLine(exp);
				return null;
			}
		}



		//initial game callback
		public static void Init() {

			//wiredly App.Awake() calls Init multiple times, but we do not want multiple instances
			//TO-DO, find out why InjectBeforeEnd does this (Hooks.cs)
			if (init)
				return;
			init = true;

			instance = new ModLoader();
			MethodBodyReplacementProviderRegistry.SetProvider (new SimpleMethodReplacementProvider(instance));

			//otherwise we can finally load
			instance.loadMods ();
		}
	}
}