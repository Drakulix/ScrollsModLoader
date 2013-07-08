using System;
using Mono.Cecil;
using LinFu.AOP.Interfaces;


namespace ScrollsModLoader.Interfaces
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

		public static void Initialize(ModAPI api) {
			BaseMod.modAPI = api;
			ScrollsExtension.setAPI (api);
		}

		public string OwnFolder() {
			return modAPI.OwnFolder (this);
		} 
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

