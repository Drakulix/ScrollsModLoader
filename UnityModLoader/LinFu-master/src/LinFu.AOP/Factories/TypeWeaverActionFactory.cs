﻿using System;
using LinFu.AOP.Cecil.Interfaces;
using LinFu.IoC.Configuration;
using LinFu.IoC.Interfaces;
using Mono.Cecil;

namespace LinFu.AOP.Cecil.Factories
{
    /// <summary>
    /// Represents a class that generates <see cref="Action{T1,T2}"/> instances
    /// that apply a specific method weaver (with the name given in the first delegate parameter)
    /// to a specific <see cref="TypeDefinition"/> instance.
    /// </summary>
    [Factory(typeof (Action<string, TypeDefinition>), ServiceName = "TypeWeaver")]
    public class TypeWeaverActionFactory : IFactory
    {
        #region IFactory Members

        /// <summary>
        /// Generates the <see cref="Action{T1, T2}"/> instance that will
        /// weave the target type.
        /// </summary>
        /// <param name="request">The <see cref="IFactoryRequest"/> that describes the service request.</param>
        /// <returns>The <see cref="Action{T1, T2}"/> instance that will weave the target type.</returns>
        public object CreateInstance(IFactoryRequest request)
        {
            IServiceContainer container = request.Container;
            Action<string, TypeDefinition> result =
                (weaverName, type) =>
                    {
                        // Get the method weaver instance that matches the weaverName
                        var methodWeaver =
                            (IHostWeaver<TypeDefinition>)
                            container.GetService(weaverName, typeof (IHostWeaver<TypeDefinition>));

                        // Wrap it in a type weaver
                        var typeWeaver =
                            (ITypeWeaver) container.GetService("AutoMethodWeaver", typeof (ITypeWeaver), methodWeaver);

                        ModuleDefinition module = type.Module;
                        if (!typeWeaver.ShouldWeave(type))
                            return;

                        // Modify the host module
                        typeWeaver.ImportReferences(module);
                        typeWeaver.AddAdditionalMembers(module);

                        // Weave the type itself
                        typeWeaver.Weave(type);
                    };


            return result;
        }

        #endregion
    }
}