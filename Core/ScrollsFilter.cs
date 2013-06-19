using System;
using System.Collections;
using System.Collections.Generic;
using LinFu.AOP.Cecil;
using LinFu.AOP.Cecil.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ScrollsModLoader
{
	/*
	 * This namespace filters classes and methods, that need to be patched
	 * Future version will obtain names from mods dynamically
	 */

	public class ScrollsFilter : ITypeFilter, IMethodFilter
	{
		public static List<MethodDefinition> hooks = new List<MethodDefinition>();

		public static void AddHook(MethodDefinition method) {
			hooks.Add (method);
		}

		public static void clearHooks() {
			hooks.Clear ();
		}

		public static void Log()
		{
			foreach (MethodDefinition def in hooks)
				Console.WriteLine (def);
		}

		public ScrollsFilter ()
		{
		}

		public bool ShouldWeave (MethodReference targetMethod)
		{
			foreach (MethodDefinition foundMethod in hooks)
				if (foundMethod.EqualsReference (targetMethod)) {
					return true;
				}
			return false;
		}
		public bool ShouldWeave (TypeReference targetType)
		{
			foreach (MethodDefinition foundMethod in hooks)
				if (foundMethod.DeclaringType.FullName.Equals (targetType.FullName)) {
					return true;
				}
			return false;
		}

	}

}

