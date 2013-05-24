﻿using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LinFu.AOP.Cecil.Interfaces
{
    /// <summary>
    /// Represents a type that can modify method bodies.
    /// </summary>
    public interface IMethodBodyRewriter
    {
        /// <summary>
        /// Rewrites a target method using the given CilWorker.
        /// </summary>
        /// <param name="method">The target method.</param>
        /// <param name="IL">The CilWorker that will be used to rewrite the target method.</param>
        /// <param name="oldInstructions">The original instructions from the target method body.</param>
        void Rewrite(MethodDefinition method, CilWorker IL, IEnumerable<Instruction> oldInstructions);
    }
}