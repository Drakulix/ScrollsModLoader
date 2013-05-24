using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ScrollsInjector
{
	public class Hooks
	{
		static AssemblyDefinition baseAssembly = null;
		static AssemblyDefinition injectAssembly = null;
		static String baseAssemblySavePath = null;

		public static void loadBaseAssembly(String name) {
			baseAssembly = AssemblyDefinition.ReadAssembly(name);
			baseAssemblySavePath = name.Replace(".bak.dll", ".patched.dll");;
		}
		
		public static void loadInjectAssembly(String name) {
			injectAssembly = AssemblyDefinition.ReadAssembly(name);
		}

		public static TypeDefinition getTypeDef(ICollection types, String name) {
			TypeDefinition typeDef = null; 
			foreach (TypeDefinition def in types) {
				if (def.Name.Equals(name)) {
					typeDef = def;
					break;
				}
			}
			return typeDef;
		}
				
		public static MethodDefinition getMethDef(TypeDefinition type, String name) {
			MethodDefinition foundMethod = null;
			foreach (MethodDefinition def in type.Methods) {
				if (def.Name.Equals(name)) {
					foundMethod = def;
					break;
				}
			}
			return foundMethod;
		}

		private static bool hookStaticVoidMethodAtBegin_Int(MethodDefinition hookedMethod, MethodDefinition callMeth) {
			try {
				var initProc = hookedMethod.Body.GetILProcessor();
        		initProc.InsertBefore(hookedMethod.Body.Instructions[0], initProc.Create(OpCodes.Call,
				    hookedMethod.Module.Assembly.MainModule.Import(callMeth.Resolve())));
				return true;
			} catch (Exception exp) {
				return false;
			}
		}

		private static bool hookStaticVoidMethodAtEnd_Int(MethodDefinition hookedMethod, MethodDefinition callMeth) {
			try {
				ArrayList retInstructions = new ArrayList();
				foreach (Instruction instr in hookedMethod.Body.Instructions) {
					if (instr.OpCode == OpCodes.Ret) {
						retInstructions.Add(instr);
					}
				}
				var initProc = hookedMethod.Body.GetILProcessor();
				bool overriden = false;
				foreach (Instruction ret in retInstructions) {
   	        		initProc.InsertBefore(ret, initProc.Create(OpCodes.Call,
						hookedMethod.Module.Assembly.MainModule.Import(callMeth.Resolve())));
					overriden = true;
				}
				return overriden;
			} catch (Exception exp) {
				return false;
			}
		}

		public static bool hookStaticVoidMethodAtBegin(String hookLoc, String callLoc) {
			String[] fullname = hookLoc.Split('.');
			String methodName = fullname[fullname.Length-1];
			ICollection types = baseAssembly.MainModule.Types;
			TypeDefinition typeDef = null;
			for (int i = 0; i <= fullname.Length-2; i++) {
				typeDef = getTypeDef(types, fullname[i]);
				types = typeDef.NestedTypes;
			}
			MethodDefinition methodToHook = getMethDef(typeDef, methodName);

			fullname = callLoc.Split('.');
			methodName = fullname[fullname.Length-1];
			types = injectAssembly.MainModule.Types;
			typeDef = null;
			for (int i = 0; i <= fullname.Length-2; i++) {
				typeDef = getTypeDef(types, fullname[i]);
				types = typeDef.NestedTypes;
			}
			MethodDefinition methodToCall = getMethDef(typeDef, methodName);

			return hookStaticVoidMethodAtBegin_Int(methodToHook, methodToCall);
		}
			
		public static bool hookStaticVoidMethodAtEnd(String hookLoc, String callLoc) {
			String[] fullname = hookLoc.Split('.');
			String methodName = fullname[fullname.Length-1];
			ICollection types = baseAssembly.MainModule.Types;
			TypeDefinition typeDef = null;
			for (int i = 0; i <= fullname.Length-2; i++) {
				typeDef = getTypeDef(types, fullname[i]);
				types = typeDef.NestedTypes;
			}
			MethodDefinition methodToHook = getMethDef(typeDef, methodName);

			fullname = callLoc.Split('.');
			methodName = fullname[fullname.Length-1];
			types = injectAssembly.MainModule.Types;
			typeDef = null;
			for (int i = 0; i <= fullname.Length-2; i++) {
				typeDef = getTypeDef(types, fullname[i]);
				if (typeDef.HasNestedTypes) types = typeDef.NestedTypes;
			}
			MethodDefinition methodToCall = getMethDef(typeDef, methodName);

			return hookStaticVoidMethodAtEnd_Int(methodToHook, methodToCall);
		}

		public static void savePatchedAssembly() {
			baseAssembly.Write(baseAssemblySavePath);
		}
	}
}