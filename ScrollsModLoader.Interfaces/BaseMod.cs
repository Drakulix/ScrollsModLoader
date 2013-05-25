using System;
using Mono.Cecil;
using LinFu.AOP.Interfaces;

namespace ScrollsModLoader.Interfaces
{
	public abstract class BaseMod
	{
		public abstract String getName ();
		public abstract int getVersion ();

		public abstract String[] getHooks (TypeDefinition[] scrollsTypes, int version);
		public abstract bool BeforeInvoke (IInvocationInfo info);
		public abstract void AfterInvoke (IInvocationInfo info, ref object returnValue);
	}
}

