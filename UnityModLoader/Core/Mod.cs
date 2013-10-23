using System;
using System.IO;
using JsonFx.Json;

namespace UnityModLoader
{
	public abstract class BaseMod
	{
		protected static ModAPI modAPI;

		public abstract void BeforeInvoke (InvocationInfo info);
		public abstract void AfterInvoke (InvocationInfo info, ref object returnValue);

		public virtual void ReplaceMethod (InvocationInfo info, out object returnValue) {
			returnValue = null;
			return;
		}
		public virtual bool WantsToReplace (InvocationInfo info) {
			return false;
		}

		//Mod API's need to be registered from the actual Game-Depended Assemblies, lets add and fix this later
		/*public static void Initialize(ModAPI api) {
			BaseMod.modAPI = api;
			ScrollsExtension.setAPI (api);
		}*/

		/*public string OwnFolder() {
			return modAPI.OwnFolder (this);
		}*/ 
	}

	public class InvocationInfo {

		public object target;
		public String targetMethod;
		public System.Diagnostics.StackTrace stackTrace;
		public Type returnType;
		public Type[] parameterTypes;
		public Type[] typeArguments;
		public object[] arguments;

		public InvocationInfo(IInvocationInfo info) {
			target = info.Target;
			targetMethod = info.TargetMethod.Name;
			stackTrace = info.StackTrace;
			returnType = info.ReturnType;
			parameterTypes = info.ParameterTypes;
			typeArguments = info.TypeArguments;
			arguments = info.Arguments;
		}
	}
}

public class Mod : BaseMod {

	public String localID;
	public String onlineID;

	public String name;
	public String description;

	public int version;
	public String versionCode;

	public String configPath;
	public String binaryPath;

	public String downloadLocationConfig;
	public String downloadLocationBinary;

	public bool enabled;
	public bool forceEnabled;

	public Mod(string path) {
		return new Mod (File.ReadAllText (path), true);
	}

	public Mod(string JSON, bool installed) {
		JsonReader reader = new JsonReader ();
		Mod mod = (Mod) reader.Read (JSON, typeof(Mod));

		if (!installed) {

		}

		return mod;
	}

	public string getName ()
	{
		return name;
	}
	public string getDesc ()
	{
		return description+", "+versionCode;
	}

	public override bool Equals(object mod) {
		if (mod is Mod) {
			return (mod as Mod).id.Equals (this.id);
		} else
			return false;
	}

	public override int GetHashCode () {
		return id.GetHashCode();
	}
}