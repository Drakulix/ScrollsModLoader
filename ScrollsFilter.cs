using System;
using LinFu.AOP.Cecil;
using LinFu.AOP.Cecil.Interfaces;

namespace ScrollsFilter
{
	public class ScrollsMethodFilter : IMethodFilter
	{
		public ScrollsMethodFilter ()
		{
		}

		#region IMethodFilter implementation
		public bool ShouldWeave (Mono.Cecil.MethodReference targetMethod)
		{
			Console.WriteLine ("MethodFilter: "+targetMethod.Name);
			if (targetMethod.DeclaringType.Name.Equals("Communicator") && targetMethod.Name.Equals("addListener"))
			    return true;
			else
			    return false;
		}
		#endregion
	}

	public class ScrollsMethodCallFilter : IMethodCallFilter
	{
		public ScrollsMethodCallFilter ()
		{
		}

		#region IMethodCallFilter implementation
		public bool ShouldWeave (Mono.Cecil.TypeReference targetType, Mono.Cecil.MethodReference hostMethod, Mono.Cecil.MethodReference currentMethodCall)
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
		public bool ShouldWeave (Mono.Cecil.MethodReference currentConstructor, Mono.Cecil.TypeReference concreteType, Mono.Cecil.MethodReference hostMethod)
		{
			Console.WriteLine ("NewInstanceFilter: "+currentConstructor.Name+" ,"+concreteType.FullName+" ,"+hostMethod.Name);
			return false;
		}
		#endregion
	}


	public class ScrollsTypeFilter : ITypeFilter
	{
		public ScrollsTypeFilter ()
		{
		}

		#region ITypeFilter implementation
		public bool ShouldWeave (Mono.Cecil.TypeReference targetType)
		{
			Console.WriteLine ("TypeFilter: "+targetType.FullName);
			if (targetType.Equals("Communicator"))
				return true;
			else
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
		public bool ShouldWeave (Mono.Cecil.MethodReference hostMethod, Mono.Cecil.FieldReference targetField)
		{
			Console.WriteLine ("FieldFilter: "+hostMethod.Name+" ,"+targetField.Name);
			return false;
		}
		#endregion
	}
}

