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
	 * 
	 * TO-DO: https://github.com/philiplaureano/LinFu/issues/23
	 */



	public class ScrollsFilter : ITypeFilter, IMethodFilter
	{
		public static List<MethodDefinition> hooks = new List<MethodDefinition>();

		public static void AddHook(MethodDefinition method) {
			Console.WriteLine (method.Name);
			hooks.Add (method);
		}

		public ScrollsFilter ()
		{
		}

		#region IMethodFilter implementation
		public bool ShouldWeave (MethodReference targetMethod)
		{
			foreach (MethodDefinition foundMethod in hooks)
				if (foundMethod.EqualsReference (targetMethod)) {
					Console.WriteLine ("MethodFilter: "+targetMethod.Name);
					return true;
				}
			return false;
			/*if (targetMethod.DeclaringType.Name.Equals ("Communicator") && (targetMethod.Name.Equals ("sendRequest") || targetMethod.Name.Equals("sendSilentRequest"))  && targetMethod.Parameters[0].ParameterType.Name.Equals("String")) {
				Console.WriteLine ("MethodFilter: "+targetMethod.Name);
				return true;
			} else
			    return false;*/
		}
		#endregion

		#region ITypeFilter implementation
		public bool ShouldWeave (TypeReference targetType)
		{
			foreach (MethodDefinition foundMethod in hooks)
				if (foundMethod.DeclaringType.FullName.Equals (targetType.FullName)) {
					Console.WriteLine ("TypeFilter: "+foundMethod.Name);
					return true;
				}
			return false;
			/*if (targetType.Name.Equals ("Communicator")) {
				Console.WriteLine ("TypeFilter: "+targetType.FullName);
				return true;
			} else
				return false;*/
		}
		#endregion

	}

	//not needed

	/*public class ScrollsMethodCallFilter : IMethodCallFilter
	{
		public ScrollsMethodCallFilter ()
		{
		}

		#region IMethodCallFilter implementation
		public bool ShouldWeave (.TypeReference targetType, .MethodReference hostMethod, .MethodReference currentMethodCall)
		{
			Console.WriteLine ("MethodCallFilter: "+targetType.FullName+" ,"+hostMethod.Name+" ,"+currentMethodCall.Name);
			if (targetType.Name.Equals("Communicator") && (hostMethod.Name.Equals("addListener") || currentMethodCall.Name.Equals("addListener")))
				return true;
			else
				return false;
		}
		#endregion
	}

	public class ScrollsInstFilter : INewInstanceFilter
	{
		public ScrollsInstFilter ()
		{
		}

		#region INewInstanceFilter implementation
		public bool ShouldWeave (.MethodReference currentConstructor, .TypeReference concreteType, .MethodReference hostMethod)
		{
			Console.WriteLine ("NewInstanceFilter: "+currentConstructor.Name+" ,"+concreteType.FullName+" ,"+hostMethod.Name);
			return false;
		}
		#endregion
	}

	public class ScrollsFieldFilter : IFieldFilter
	{
		public ScrollsFieldFilter ()
		{
		}

		#region IFieldFilter implementation
		public bool ShouldWeave (.MethodReference hostMethod, .FieldReference targetField)
		{
			Console.WriteLine ("FieldFilter: "+hostMethod.Name+" ,"+targetField.Name);
			return false;
		}
		#endregion
	}*/
}

