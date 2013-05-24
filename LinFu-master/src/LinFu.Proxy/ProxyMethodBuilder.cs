﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinFu.IoC.Configuration;
using LinFu.IoC.Interfaces;
using LinFu.Proxy.Interfaces;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;

namespace LinFu.Proxy
{
    /// <summary>
    /// Represents the default implementation of the
    /// <see cref="IMethodBuilder"/> interface.
    /// </summary>
    [Implements(typeof (IMethodBuilder), LifecycleType.OncePerRequest, ServiceName = "ProxyMethodBuilder")]
    public class ProxyMethodBuilder : IMethodBuilder, IInitialize
    {
        /// <summary>
        /// Initializes the <see cref="ProxyMethodBuilder"/> class with the default property values.
        /// </summary>
        public ProxyMethodBuilder()
        {
            Emitter = new MethodBodyEmitter();
        }

        /// <summary>
        /// The <see cref="IMethodBodyEmitter"/> instance that will be
        /// responsible for generating the method body.
        /// </summary>
        public virtual IMethodBodyEmitter Emitter { get; set; }

        #region IInitialize Members

        /// <summary>
        /// Initializes the class with the <paramref name="source"/> container.
        /// </summary>
        /// <param name="source">The <see cref="IServiceContainer"/> instance that will create the class instance.</param>
        public virtual void Initialize(IServiceContainer source)
        {
            Emitter = (IMethodBodyEmitter) source.GetService(typeof (IMethodBodyEmitter));
        }

        #endregion

        #region IMethodBuilder Members

        /// <summary>
        /// Creates a method that matches the signature defined in the
        /// <paramref name="method"/> parameter.
        /// </summary>
        /// <param name="targetType">The type that will host the new method.</param>
        /// <param name="method">The method from which the signature will be derived.</param>
        public virtual MethodDefinition CreateMethod(TypeDefinition targetType, MethodInfo method)
        {
            #region Match the method signature

            ModuleDefinition module = targetType.Module;
            string methodName = method.Name;

            // If the method is a member defined on an interface type,
            // we need to rename the method to avoid
            // any naming conflicts in the type itself
            if (method.DeclaringType.IsInterface)
            {
                string parentName = method.DeclaringType.FullName;

                // Rename the parent type to its fully qualified name
                // if it is a generic type
                methodName = string.Format("{0}.{1}", parentName, methodName);
            }

            MethodAttributes baseAttributes = MethodAttributes.Virtual |
                                              MethodAttributes.HideBySig;

            MethodAttributes attributes = default(MethodAttributes);

            #region Match the visibility of the target method

            if (method.IsFamilyOrAssembly)
                attributes = baseAttributes | MethodAttributes.FamORAssem;

            if (method.IsFamilyAndAssembly)
                attributes = baseAttributes | MethodAttributes.FamANDAssem;

            if (method.IsPublic)
                attributes = baseAttributes | MethodAttributes.Public;

            #endregion

            // Build the list of parameter types
            Type[] parameterTypes = (from param in method.GetParameters()
                                     let type = param.ParameterType
                                     let importedType = type
                                     select importedType).ToArray();


            //Build the list of generic parameter types
            Type[] genericParameterTypes = method.GetGenericArguments();

            MethodDefinition newMethod = targetType.DefineMethod(methodName, attributes,
                                                                 method.ReturnType, parameterTypes,
                                                                 genericParameterTypes);

            newMethod.Body.InitLocals = true;
            newMethod.ImplAttributes = MethodImplAttributes.IL | MethodImplAttributes.Managed;
            newMethod.HasThis = true;

            // Match the generic type arguments
            Type[] typeArguments = method.GetGenericArguments();

            if (typeArguments != null || typeArguments.Length > 0)
                MatchGenericArguments(newMethod, typeArguments);

            MethodReference originalMethodRef = module.Import(method);
            newMethod.Overrides.Add(originalMethodRef);

            #endregion

            // Define the method body
            if (Emitter != null)
                Emitter.Emit(method, newMethod);

            return newMethod;
        }

        #endregion

        /// <summary>
        /// Matches the generic parameters of <paramref name="newMethod">a target method</paramref>
        /// </summary>
        /// <param name="newMethod">The generic method that contains the generic type arguments.</param>
        /// <param name="typeArguments">The array of <see cref="Type"/> objects that describe the generic parameters for the current method.</param>
        private static void MatchGenericArguments(MethodDefinition newMethod, ICollection<Type> typeArguments)
        {
            foreach (Type argument in typeArguments)
            {
                newMethod.AddGenericParameter(argument);
            }
        }
    }
}