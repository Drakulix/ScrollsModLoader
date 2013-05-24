﻿using System.Collections.Generic;
using System.Linq;
using LinFu.AOP.Cecil.Interfaces;
using Mono.Cecil;

namespace LinFu.AOP.Cecil.Extensions
{
    /// <summary>
    /// Adds helper methods to the <see cref="TypeDefinition"/> class.
    /// </summary>
    public static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Applies a <see cref="IMethodWeaver"/> instance to all methods
        /// within the given <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">The target module.</param>
        /// <param name="weaver">The <see cref="ITypeWeaver"/> instance that will modify the methods in the given target type.</param>
        public static void WeaveWith(this TypeDefinition targetType, IMethodWeaver weaver)
        {
            ModuleDefinition module = targetType.Module;
            IEnumerable<MethodDefinition> targetMethods = from MethodDefinition method in targetType.Methods
                                                          where weaver.ShouldWeave(method)
                                                          select method;

            // Modify the host module
            weaver.ImportReferences(module);

            // Add any additional members to the target type
            weaver.AddAdditionalMembers(targetType);

            foreach (MethodDefinition item in targetMethods)
            {
                weaver.Weave(item);
            }
        }
    }
}