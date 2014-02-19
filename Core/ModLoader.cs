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
using ScrollsModLoader.Interfaces;
using UnityEngine;
using System.Threading;
using JsonFx.Json;

namespace ScrollsModLoader {

	/*
	 * handels mod loading and debug file logging
	 * this class should only pass invokes through itself to the right mods
	 * 
	 */

	public class ModInterceptor : IInterceptor
	{
		private struct BaseModWithId {
			public BaseModWithId(BaseMod mod, string id) {
				this.mod = mod;
				this.id = id;
			}
			public BaseMod mod;
			public string id;
		}
		private ModLoader loader;
		private TypeDefinitionCollection types;

		private Dictionary<string, MethodDefinition[]> modHooks;
		private Dictionary<string, Dictionary<string, List<BaseModWithId>>> hooks;

		public ModInterceptor(ModLoader loader) {
			this.loader = loader;
			types = AssemblyFactory.GetAssembly (Platform.getGlobalScrollsInstallPath()+"ModLoader/Assembly-CSharp.dll").MainModule.Types;
		}

		private void GenerateHookDict() {
			List<String> modsToUnload = new List<String> ();
			modHooks = new Dictionary<string, MethodDefinition[]> ();
			hooks = new Dictionary<string, Dictionary<string, List<BaseModWithId>>> ();
			foreach(String modId in loader.modOrder) {
				BaseMod mod = null;
				try {
					mod = loader.modInstances [modId];
				} catch {
					continue;
				}
				if (mod != null) {
					Dictionary<string, List<BaseModWithId>> methodHooks;
					List<BaseModWithId> hookedMods;
					MethodDefinition[] requestedHooks;
					try {
						requestedHooks = (MethodDefinition[])mod.GetType ().GetMethod ("GetHooks").Invoke (null, new object[] {types,
							SharedConstants.getExeVersionInt () });
					} catch (Exception ex) {
						Console.WriteLine (ex);
						modsToUnload.Add (modId);
						continue;
					}
					modHooks.Add (modId, requestedHooks);
					foreach(MethodDefinition hookedMethod in requestedHooks) {
						//TODO: FIx for overloaded methods!
						if (!hooks.TryGetValue(hookedMethod.DeclaringType.Name, out methodHooks)) {
							methodHooks = new Dictionary<string, List<BaseModWithId>> ();
							hooks.Add (hookedMethod.DeclaringType.Name, methodHooks);
						}
						if (!methodHooks.TryGetValue(hookedMethod.Name, out hookedMods)) {
							hookedMods = new List<BaseModWithId> ();
							methodHooks.Add (hookedMethod.Name, hookedMods);
						}
						hookedMods.Add (new BaseModWithId(mod, modId));
					}
				}
			}
			Unload (modsToUnload);
			Console.WriteLine ("Hooks:");
			foreach (string hookedTypeName in hooks.Keys) {
				Console.WriteLine (hookedTypeName);
				foreach (string hookedMethodName in hooks[hookedTypeName].Keys) {
					Console.WriteLine ("\t" + hookedMethodName);
					foreach (BaseModWithId modWithId in hooks[hookedTypeName][hookedMethodName]) {
						Console.WriteLine ("\t\t" + modWithId.id);
					}
				}
			}

		}

		public void Unload(List<String> modsToUnload) {
			Dictionary<string, List<BaseModWithId>> methodHooks;
			List<BaseModWithId> hookedMods;
			//unload
			foreach (String id in modsToUnload) {
				//Removing the Mod from all Hooks it subscribed to.
				foreach (MethodDefinition m in modHooks[id]) {
					if (hooks.TryGetValue (m.DeclaringType.Name, out methodHooks)) {
						if (methodHooks.TryGetValue (m.Name, out hookedMods)) {
							hookedMods.RemoveAll ((BaseModWithId modWithId) => (modWithId.id.Equals (id)));
						}
					}
				}
				loader.unloadMod ((LocalMod)loader.modManager.installedMods.Find (delegate(Item lmod) {
					return (id.Equals ((lmod as LocalMod).id));
				}));
			}
			modsToUnload.Clear ();
		}

		private int lastModInstancesCount = -1;
		public object Intercept (IInvocationInfo info)
		{
			if (lastModInstancesCount != loader.modInstances.Count) {
				lastModInstancesCount = loader.modInstances.Count;
				hooks = null;
			}
			if (hooks == null) {
				GenerateHookDict ();
			}
			//list for unloading
			List<String> modsToUnload = new List<String> ();
			String replacement = "";

			//Find Mods that hooked into Method
			Dictionary<string, List<BaseModWithId>> methodHooks;
			List<BaseModWithId> hookedMods;

			string declaringTypeName = info.TargetMethod.DeclaringType.Name;
			string targetMethodName = info.TargetMethod.Name;

			if(hooks.TryGetValue(declaringTypeName, out methodHooks)) {
				if(methodHooks.TryGetValue(targetMethodName, out hookedMods)) {
					//determine replacement
					foreach(BaseModWithId modWithId in hookedMods) {
						BaseMod mod = modWithId.mod;
						string id = modWithId.id;
						try {
							if (mod.WantsToReplace (new InvocationInfo(info)))
								replacement = id;
						} catch (Exception ex) {
							Console.WriteLine (ex);
							modsToUnload.Add (id);
						}
					}

					//unload
					Unload (modsToUnload);

					//load beforeinvoke
					foreach(BaseModWithId modWithId in hookedMods) {
						BaseMod mod = modWithId.mod;
						string id = modWithId.id;
						if (id.Equals (replacement)) {
							continue;
						}
						try {
							mod.BeforeInvoke (new InvocationInfo (info));
						} catch (Exception ex) {
							Console.WriteLine (ex);
							modsToUnload.Add (id);
						}
					}
				}
			}

			//unload
			Unload (modsToUnload);


			//TODO: Simplify the Patch-Finding Process - Implement as Mod?
			//check for patch call
			object ret = null;
			bool patchFound = false;
			foreach (Patch patch in loader.patches) {
				if (patch.patchedMethods ().Any (item => ((item.Name.Equals (info.TargetMethod.Name)) && (item.DeclaringType.Name.Equals (info.TargetMethod.DeclaringType.Name))))){
					try {
						ret = patch.Intercept (info);
						patchFound = true;
					} catch (Exception ex) {
						Console.WriteLine (ex);
					}
				}
			}
			if (!patchFound) {
				if (replacement.Equals(""))
					ret = info.TargetMethod.Invoke (info.Target, info.Arguments);
				else {
					try {
						BaseMod mod = loader.modInstances [replacement];
						mod.ReplaceMethod(new InvocationInfo(info), out ret);
					} catch (Exception exp) {
						Console.WriteLine (exp);
						modsToUnload.Add (replacement);
					}
				}
			}

			//Additional unload?
			Unload (modsToUnload);

			//load afterinvoke
			if (hooks.TryGetValue (declaringTypeName, out methodHooks)) {
				if (methodHooks.TryGetValue (targetMethodName, out hookedMods)) {
					foreach(BaseModWithId modWithId in hookedMods) {
						BaseMod mod = modWithId.mod;
						string id = modWithId.id;
						try {
							mod.AfterInvoke (new InvocationInfo (info), ref ret);
						} catch (Exception exp) {
							Console.WriteLine (exp);
							modsToUnload.Add (id);
						}
					}
				}
			}

			//unload
			Unload (modsToUnload);

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
			foreach (StackFrame frame in trace.GetFrames()) {
				if (frame.GetMethod ().Name.Equals(info.TargetMethod.Name))
					// this replacement disables us to hook rekursive functions, however the default one is broken
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
		public static Dictionary<String, ExceptionLogger> logger = new Dictionary<String, ExceptionLogger> ();
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
				if (modManager.installedMods.Find (delegate(Item mod) {
					return ((mod as LocalMod).localId.Equals (id));
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

			//repatch
			this.repatchIfNeeded ();

			Console.WriteLine ("------------------------------");
			Console.WriteLine ("ModLoader Hooks:");
			ScrollsFilter.Log ();
			Console.WriteLine ("------------------------------");

		}

		public void loadPatches(TypeDefinitionCollection types) {

			//get Patches
			patches.Add (new PatchUpdater (types));
			patches.Add (new PatchPopups (types));
			//patches.Add(new PatchOffline(types));

			PatchSettingsMenu settingsMenuHook = new PatchSettingsMenu (types);
			publicAPI.setSceneCallback (settingsMenuHook);
			patches.Add (settingsMenuHook);

			PatchModsMenu modMenuHook = new PatchModsMenu (types, this);
			modMenuHook.Initialize (publicAPI);
			patches.Add (modMenuHook);

			//add Hooks
			addPatchHooks ();
		}

		public void addPatchHooks () {
			foreach (Patch patch in patches) {
				try {
					foreach (MethodDefinition definition in patch.patchedMethods())
						ScrollsFilter.AddHook (definition);
				} catch {}
			}
		}

		public void loadModsStatic(TypeDefinitionCollection types) {
			//get Mods
			foreach (LocalMod mod in modManager.installedMods) {
				if (mod.enabled) {
					if (this.loadModStatic (mod.installPath) == null) {
						modManager.disableMod(mod, false);
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

		private Mod _loadModStatic(TypeDefinitionCollection types, String filepath) {
			ResolveEventHandler resolver = new ResolveEventHandler(CurrentDomainOnAssemblyResolve);
			AppDomain.CurrentDomain.AssemblyResolve += resolver;

			Assembly modAsm = null;
			try {
				modAsm = Assembly.LoadFile(filepath);
			} catch (BadImageFormatException) {
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return null;
			}
			Type modClass = (from _modClass in modAsm.GetTypes ()
			                 where _modClass.InheritsFrom (typeof(ScrollsModLoader.Interfaces.BaseMod))
			                 select _modClass).First();

			//no mod classes??
			if (modClass == null) {
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return null;
			}

			//get hooks
			MethodDefinition[] hooks = null;
			try {
				hooks =(MethodDefinition[]) modClass.GetMethod ("GetHooks").Invoke (null, new object[] {
					types,
					SharedConstants.getExeVersionInt ()
				});
			} catch (Exception e) {
				Console.WriteLine ("Error executing GetHooks for mod: " + filepath);
				Console.WriteLine (e);
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return null;
			}


			TypeDefinition[] typeDefs = new TypeDefinition[types.Count];
			types.CopyTo(typeDefs, 0);

			//check hooks
			foreach (MethodDefinition hook in hooks) {
				//type/method does not exists
				if (hook == null) {
					Console.WriteLine ("ERROR: GetHooks contains 'null'! ");
					Console.WriteLine ("=> Disabling " + filepath);
					AppDomain.CurrentDomain.AssemblyResolve -= resolver;
					return null;
				}
				if ((from type in typeDefs
				     where type.Equals(hook.DeclaringType) //Code above avoids NullReferenceException when hook is null.
				     select type).Count() == 0) {
					//disable mod
					Console.WriteLine ("ERROR: Mod hooks unexistant method! " + filepath);
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
			try {
				mod.id = "00000000000000000000000000000000";
				mod.name = (String)modClass.GetMethod("GetName").Invoke(null, null);
				mod.version = (int)modClass.GetMethod("GetVersion").Invoke(null, null);
				mod.versionCode = ""+mod.version;
				mod.description = "";
			} catch (Exception e){
				Console.WriteLine ("Error getting Name or Version: ");
				Console.WriteLine (e);
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return null;
			}

			AppDomain.CurrentDomain.AssemblyResolve -= resolver;
			return mod;
		}


		public void loadMods() {

			BaseMod.Initialize (publicAPI);
			foreach (String id in modOrder) {
				LocalMod lmod = (LocalMod)modManager.installedMods.Find (delegate(Item mod) {
					return ((mod as LocalMod).localId.Equals (id));
				});
				if (lmod.enabled)
					this.loadMod(lmod);
			}

		}

		public void loadMod(LocalMod mod) {

			ResolveEventHandler resolver = new ResolveEventHandler(CurrentDomainOnAssemblyResolve);
			AppDomain.CurrentDomain.AssemblyResolve += resolver;

			Assembly modAsm = null;
			try {
				modAsm = Assembly.LoadFile(mod.installPath);
			} catch (BadImageFormatException) {
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return;
			}
			Type modClass = (from _modClass in modAsm.GetTypes ()
			                 where _modClass.InheritsFrom (typeof(BaseMod))
			                 select _modClass).First();


			//no mod classes??
			if (modClass == null) {
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return;
			}

			publicAPI.setCurrentlyLoading (mod);

			int countmods = modInstances.Count;
			int countlog = logger.Count;
			try {
				ConstructorInfo modConstructor = modClass.GetConstructor (Type.EmptyTypes);
				if (modConstructor == null) {
					//GetConstructor did not find a public Constructor
					Console.WriteLine("Could not find public 0-Argument Constructor for mod: "+mod.getName());
					ConstructorInfo[] allConstructors = modClass.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
					foreach(ConstructorInfo c in allConstructors) {
						if (c.GetParameters().Length == 0) {
							Console.WriteLine("Found alternative Constructor! Please use a public Constructor for your Mod");
							modConstructor = c;
							break;
						}
					}
					if (modConstructor == null) {
						throw new Exception("Could not find any 0-Argument-Constructors");
					}
				}
				modInstances.Add(mod.localId, (BaseMod)(modConstructor.Invoke (new object[0])));
				if (!mod.localInstall)
					logger.Add (mod.localId, new ExceptionLogger (mod, mod.source));
			} catch (Exception exp) {
				Console.WriteLine (exp);
				if (modInstances.Count > countmods)
					modInstances.Remove (mod.localId);
				if (logger.Count > countlog)
					logger.Remove (mod.localId);
				AppDomain.CurrentDomain.AssemblyResolve -= resolver;
				return;
			}

			publicAPI.setCurrentlyLoading (null);

			if (!modOrder.Contains (mod.localId))
				modOrder.Add (mod.localId);

			AppDomain.CurrentDomain.AssemblyResolve -= resolver;
		}

		public void unloadMod(LocalMod mod) {
			modManager.disableMod (mod, true);
			this._unloadMod (mod);
		}
		public void _unloadMod(LocalMod mod) {
			modOrder.Remove (mod.localId);
			modInstances.Remove (mod.localId);
		}

		public void moveModUp(LocalMod mod) {
			int index = modOrder.IndexOf (mod.localId);
			if (index > 0) {
				modOrder.Remove (mod.localId);
				modOrder.Insert (index - 1, mod.localId);
			}
			modManager.sortInstalledMods ();
		}

		public void moveModDown(LocalMod mod) {
			int index = modOrder.IndexOf (mod.localId);
			if (index+1 < modOrder.Count) {
				modOrder.Remove (mod.localId);
				modOrder.Insert (index + 1, mod.localId);
			}
			modManager.sortInstalledMods ();
		}

		public void queueRepatch() {
			isRepatchNeeded = true;
		}

		public void repatchIfNeeded() {
			if (isRepatchNeeded) {
				repatch ();
			}
		}

		public void repatch()
		{
				//save ModList
				File.Delete (modLoaderPath+"mods.ini");
				StreamWriter modOrderWriter = File.CreateText (modLoaderPath+"mods.ini");
				foreach (String modId in modOrder) {
					modOrderWriter.WriteLine (modId);
				}
				modOrderWriter.Flush ();
				modOrderWriter.Close ();

				String installPath = Platform.getGlobalScrollsInstallPath ();
				File.Delete(installPath+"Assembly-CSharp.dll");
				File.Copy(installPath+"ModLoader"+ System.IO.Path.DirectorySeparatorChar +"Assembly-CSharp.dll", installPath+"Assembly-CSharp.dll");

				Patcher patcher = new Patcher ();
				if (!patcher.patchAssembly (Platform.getGlobalScrollsInstallPath ())) {
					//normal patching should never fail at this point
					//because this is no update and we are already patched
					//TO-DO get hook that crashed the patching and deactive mod instead
					//No idea how to do that correctly
					Dialogs.showNotification ("Scrolls is broken", "Your Scrolls install appears to be broken or modified by other tools. Scrolls Summoner failed to load and will de-install itself");
					File.Delete(Platform.getGlobalScrollsInstallPath()+"Assembly-CSharp.dll");
					File.Copy(Platform.getGlobalScrollsInstallPath()+"ModLoader"+ System.IO.Path.DirectorySeparatorChar +"Assembly-CSharp.dll", Platform.getGlobalScrollsInstallPath()+"Assembly-CSharp.dll");
					Application.Quit();
				}

				Platform.RestartGame ();
		}

		private static System.Reflection.Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var asm = (from a in AppDomain.CurrentDomain.GetAssemblies()
			           where a.GetName().FullName.Equals(args.Name)
			           select a).FirstOrDefault();
			if (asm == null) {
				asm = System.Reflection.Assembly.GetExecutingAssembly();
			}
			return asm;
		}

		public static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e) {
			if ((e.ExceptionObject as Exception).TargetSite.Module.Assembly.GetName().Name.Equals("UnityEngine")
			    || (e.ExceptionObject as Exception).TargetSite.Module.Assembly.GetName().Name.Equals("Assembly-CSharp")
			    || (e.ExceptionObject as Exception).TargetSite.Module.Assembly.GetName().Name.Equals("ScrollsModLoader")
			    || (e.ExceptionObject as Exception).TargetSite.Module.Assembly.Location.ToLower().Equals(Platform.getGlobalScrollsInstallPath().ToLower())
			    || (e.ExceptionObject as Exception).TargetSite.Module.Assembly.Location.Equals("")) { //no location or Managed => mod loader crash

				//log
				Console.WriteLine (e.ExceptionObject);
				new ExceptionLogger ().logException ((Exception)e.ExceptionObject);

				//unload ScrollsModLoader
				MethodBodyReplacementProviderRegistry.SetProvider (new NoMethodReplacementProvider());

				//check for frequent crashes
				if (!System.IO.File.Exists (Platform.getGlobalScrollsInstallPath () + System.IO.Path.DirectorySeparatorChar + "check.txt")) {
					System.IO.File.CreateText (Platform.getGlobalScrollsInstallPath () + System.IO.Path.DirectorySeparatorChar + "check.txt");
					Platform.RestartGame ();
				} else {
					try {
						foreach (String id in instance.modOrder) {
							BaseMod mod = instance.modInstances [id];
							if (mod != null) {
								try {
									instance.unloadMod((LocalMod)instance.modManager.installedMods.Find (delegate(Item lmod) {
										return ((lmod as LocalMod).id.Equals (id));
									}));
								} catch  (Exception exp) {
									Console.WriteLine (exp);
								}
							}
						}
					} catch  (Exception exp) {
						Console.WriteLine (exp);
					}
					instance.repatch ();
				}

			} else if (instance != null && logger != null && logger.Count > 0) {

				Console.WriteLine (e.ExceptionObject);

				Assembly asm = (e.ExceptionObject as Exception).TargetSite.Module.Assembly;
				Type modClass = (from _modClass in asm.GetTypes ()
				                 where _modClass.InheritsFrom (typeof(BaseMod))
				                 select _modClass).First();

				//no mod classes??
				if (modClass == null) {
					return;
				}

				foreach (String id in instance.modOrder) {
					BaseMod mod = null;
					try {
						mod = instance.modInstances [id];
					} catch  (Exception exp) {
						Console.WriteLine (exp);
					}
					if (mod != null) {
						if (modClass.Equals(mod.GetType())) {
							String folder = Path.GetDirectoryName (asm.Location);
							if (File.Exists (folder + Path.DirectorySeparatorChar + "config.json")) {
								JsonReader reader = new JsonReader ();
								LocalMod lmod = (LocalMod) reader.Read (File.ReadAllText (folder + Path.DirectorySeparatorChar + "config.json"), typeof(LocalMod));
								if (!lmod.localInstall)
									logger [lmod.localId].logException ((Exception)e.ExceptionObject);
							}
						}
					}
				}
			}
		}

		public class NoMethodReplacementProvider : IMethodReplacementProvider 
		{
			public bool CanReplace (object host, IInvocationInfo info)
			{
				return false;
			}

			public IInterceptor GetMethodReplacement (object host, IInvocationInfo info)
			{
				return null;
			}
		}

		//initial game callback
		public static void Init() {

			//wiredly App.Awake() calls Init multiple times, but we do not want multiple instances
			if (init)
				return;
			init = true;

			Console.WriteLine ("ModLoader version: " + ModLoader.getVersion ());

			if (Updater.tryUpdate ()) { //update
				Application.Quit ();
				return;
			}

			//Install global mod exception helper
			AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

			instance = new ModLoader();
			MethodBodyReplacementProviderRegistry.SetProvider (new SimpleMethodReplacementProvider(instance));

			//otherwise we can finally load
			instance.loadMods ();

			//delete checks for loading crashes
			if (System.IO.File.Exists (Platform.getGlobalScrollsInstallPath () + System.IO.Path.DirectorySeparatorChar + "check.txt")) {
				System.IO.File.Delete (Platform.getGlobalScrollsInstallPath () + System.IO.Path.DirectorySeparatorChar + "check.txt");
			}
		}

		public static int getVersion() {
			return 5;
		}
	}
}