using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ScrollsModLoader
{
	/*
	 *  very basic static call hocks via Mono.Cecil
	 *  Interpret them as Notification on calls
	 *  Note: They block the current game code
	 * 
	 */ 


	public class Hooks
	{
		static AssemblyDefinition baseAssembly = null;
		static AssemblyDefinition injectAssembly = null;
		static String baseAssemblySavePath = null;

		public static void loadBaseAssembly(String name) {
			baseAssembly = AssemblyFactory.GetAssembly(name);
			baseAssemblySavePath = name;
		}
		
		public static void loadInjectAssembly(string name) {
			injectAssembly = AssemblyFactory.GetAssembly(name);
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
				CilWorker initProc = hookedMethod.Body.CilWorker;
        		initProc.InsertBefore(hookedMethod.Body.Instructions[0], initProc.Create(OpCodes.Call,
				    baseAssembly.MainModule.Import(callMeth.Resolve())));
				    //callMeth.Resolve()));
				return true;
			} catch {
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
				CilWorker initProc = hookedMethod.Body.CilWorker;
				bool overriden = false;
				foreach (Instruction ret in retInstructions) {
   	        		initProc.InsertBefore(ret, initProc.Create(OpCodes.Call,
						baseAssembly.MainModule.Import(callMeth.Resolve())));
						//callMeth.Resolve()));
					overriden = true;
				}
				return overriden;
			} catch {
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
			//removing assemblies, that are part of the merged modloader assembly does crash, we need to add them manually
			/*List<AssemblyNameReference> names = new List<AssemblyNameReference>();
			foreach (AssemblyNameReference name in baseAssembly.MainModule.AssemblyReferences) {
				if (name.FullName.Contains ("LinFu"))
					names.Add (name);
			}
			foreach (AssemblyNameReference name in names) {
				baseAssembly.MainModule.AssemblyReferences.Remove (name);
			}*/
			
			System.IO.File.Delete(baseAssemblySavePath);
			AssemblyFactory.SaveAssembly(baseAssembly, baseAssemblySavePath);
		}
	}
}