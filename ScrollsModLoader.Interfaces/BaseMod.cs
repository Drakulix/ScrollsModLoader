using System;
using Mono.Cecil;
using LinFu.AOP.Interfaces;

namespace ScrollsModLoader.Interfaces
{
	public abstract class ModAPIContainer {
		public virtual void Initialize (ModAPI api) {}
		public virtual string OwnFolder () { return ""; }
	}

	public abstract class BaseMod : ModAPIContainer
	{
		public ModAPI modAPI;

		public abstract String GetName ();
		public abstract int GetVersion ();
		public abstract MethodDefinition[] GetHooks (TypeDefinitionCollection scrollsTypes, int version);

		public abstract void Init ();
		public abstract bool BeforeInvoke (InvocationInfo info, out object returnValue);
		public abstract void AfterInvoke (InvocationInfo info, ref object returnValue);

		public sealed override void Initialize(ModAPI api) {
			this.modAPI = api;
		}

		public sealed override string OwnFolder() {
			return modAPI.OwnFolder (this.GetName());
		}
	}

	public class InvocationInfo {

		object target;
		String targetMethod;
		System.Diagnostics.StackTrace stackTrace;
		Type returnType;
		Type[] parameterTypes;
		Type[] typeArguments;
		object[] arguments;

		public InvocationInfo(IInvocationInfo info) {
			target = info.Target;
			targetMethod = info.TargetMethod.Name;
			stackTrace = info.StackTrace;
			returnType = info.ReturnType;
			parameterTypes = info.ParameterTypes;
			typeArguments = info.TypeArguments;
			arguments = info.Arguments;
		}

		public object Target()
		{
			return target;
		}
		public String TargetMethod()
		{
			return targetMethod;
		}
		public System.Diagnostics.StackTrace StackTrace()
		{
			return stackTrace;
		}
		public System.Type ReturnType()
		{
			return returnType;
		}
		public System.Type[] ParameterTypes()
		{
			return parameterTypes;
		}
		public System.Type[] TypeArguments()
		{
			return typeArguments;
		}
		public object[] Arguments()
		{
			return arguments;
		}
	}
}

