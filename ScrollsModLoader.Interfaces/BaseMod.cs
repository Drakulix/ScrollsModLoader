using System;
using Mono.Cecil;
using LinFu.AOP.Interfaces;

namespace ScrollsModLoader.Interfaces
{
	public abstract class BaseMod
	{
		public abstract String GetName ();
		public abstract int GetVersion ();

		public abstract MethodDefinition[] GetHooks (TypeDefinition[] scrollsTypes, int version);
		public abstract bool BeforeInvoke (IInvocationInfo info, out object returnValue);
		public abstract void AfterInvoke (IInvocationInfo info, ref object returnValue);
	}
}

