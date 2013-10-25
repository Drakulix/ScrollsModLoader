using System;
using System.IO;
using System.Net;
using JsonFx.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using LinFu.AOP.Interfaces;
using System.Collections.ObjectModel;

namespace UnityModLoader
{
	public abstract class BaseMod
	{
		//protected static ModAPI modAPI;
	
		public abstract void BeforeInvoke (InvocationInfo info);
		public abstract void AfterInvoke (InvocationInfo info, ref object returnValue);
		
		
		delegate bool WantsToReplace (InvocationInfo info);
		public virtual WantsToReplace getWantsToReplace ()
		{
			return info => false;
		}

		public virtual void ReplaceMethod (InvocationInfo info, out object returnValue)
		{
			returnValue = null;
			return;
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

	public class BaseInvocationInfo
	{
		public object target;
		public String targetMethod;
		public System.Diagnostics.StackTrace stackTrace;
		public Type returnType;
		public Type[] parameterTypes;

		public BaseInvocationInfo (IInvocationInfo info)
		{
			target = info.Target;
			targetMethod = info.TargetMethod.Name;
			stackTrace = info.StackTrace;
			returnType = info.ReturnType;
			parameterTypes = info.ParameterTypes;
		}
	}

	public class InvocationInfo : BaseInvocationInfo
	{
		public Type[] typeArguments;
		public object[] arguments;

		public InvocationInfo (IInvocationInfo info) : base (info) {
			typeArguments = info.TypeArguments;
			arguments = info.Arguments;
		}
	}

	public class LimitedInvocationInfo : BaseInvocationInfo
	{
		public ReadOnlyCollection<Type> typeArguments;
		public ReadOnlyCollection<object> arguments;

		public LimitedInvocationInfo (IInvocationInfo info) : base (info) {
			this.typeArguments = Array.AsReadOnly(info.TypeArguments);
			this.arguments = Array.AsReadOnly(info.Arguments);
		}
	}

	public class Mod : BaseMod {

		public String localID;
		public String onlineID;
		public bool onlineOnly;

		public String name;
		public String description;

		public int version;
		public String versionCode;
		public String[] compatibleGameVersions;

		public String configPath;
		public String binaryPath;

		public String downloadLocationConfig;
		public String downloadLocationBinary;

		public bool enabled;
		public bool forceEnabled;

		public bool installed;

		public Mod(Uri path) {
			if (path.IsFile)
				return new Mod (File.ReadAllText (path.AbsolutePath));
			else {
				WebClientTimeOut wc = new WebClientTimeOut ();
				string config = null;
				try {
					config = wc.DownloadData(path);
				} catch (WebException) {
					return null;
				}
				return new Mod(System.Text.Encoding.UTF8.GetString(config));
			}
		}

		public Mod(string JSON) {
			JsonReader reader = new JsonReader ();
			Mod mod = null;
			try {
				 mod = (Mod) reader.Read (JSON, typeof(Mod));
			} catch (JsonFx.Serialization.SerializationException) {}
			return mod;
		}

		public string getFullDescription ()
		{
			return description+" - "+versionCode;
		}

		public override bool Equals(object mod) {
			if (mod is Mod) {
				if ((mod as Mod).onlineOnly)
					return (mod as Mod).onlineID.Equals (this.onlineID) && (mod as Mod).downloadLocationConfig.Equals (this.downloadLocationConfig);
				else 
					return (mod as Mod).localID.Equals (this.localID);
			} else
				return false;
		}
			
		public override int GetHashCode () {
			if (onlineOnly)
				return onlineID.GetHashCode () + downloadLocationConfig.GetHashCode ();
			else
				return localID.GetHashCode();
		}
	}
}