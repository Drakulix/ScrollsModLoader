using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using LinFu.AOP.Interfaces;
using System.Collections.ObjectModel;

namespace UnityModLoader
{
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
}

