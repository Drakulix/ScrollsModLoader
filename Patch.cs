using System;
using LinFu.AOP.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ScrollsModLoader
{
	public abstract class Patch : IInterceptor
	{
		protected TypeDefinitionCollection assembly;

		public Patch(TypeDefinitionCollection scrolls) {
			assembly = scrolls;
			foreach (MethodDefinition def in this.patchedMethods()) {
				ScrollsFilter.AddHook (def);
			}
		}
		public abstract MethodDefinition[] patchedMethods();
		public abstract object Intercept(IInvocationInfo info);
	}
}

