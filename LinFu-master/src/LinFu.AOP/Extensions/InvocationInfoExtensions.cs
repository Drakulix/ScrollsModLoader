﻿using System.Reflection;
using LinFu.AOP.Cecil.Interfaces;
using LinFu.AOP.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LinFu.AOP.Cecil.Extensions
{
    /// <summary>
    /// Adds helper methods classes that implement the <see cref="IInvocationInfo"/> 
    /// interface.
    /// </summary>
    public static class InvocationInfoExtensions
    {
        /// <summary>
        /// Emits the IL instructions that will store information about the method <paramref name="targetMethod">currently being executed</paramref>
        /// and stores the results into the <paramref name="invocationInfo">variable.</paramref>
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitInvocationInfo"/> instance.</param>
        /// <param name="method">The method whose implementation will be intercepted.</param>
        /// <param name="targetMethod">The actual method that will contain the resulting instructions.</param>
        /// <param name="invocationInfo">The <see cref="VariableDefinition">local variable</see> that will store the current <see cref="IInvocationInfo"/> instance.</param>
        public static void Emit(this IEmitInvocationInfo emitter, MethodInfo method, MethodDefinition targetMethod,
                                VariableDefinition invocationInfo)
        {
            ModuleDefinition module = targetMethod.DeclaringType.Module;
            MethodReference interceptedMethod = module.Import(method);
            emitter.Emit(targetMethod, interceptedMethod, invocationInfo);
        }

        /// <summary>
        /// Invokes the currently executing method by using the <see cref="IInvocationInfo.Target"/>
        /// as the target instance, the <see cref="IInvocationInfo.TargetMethod"/> as the method, 
        /// and uses the <see cref="IInvocationInfo.Arguments"/> for the method
        /// arguments.
        /// </summary>
        /// <param name="info">The <see cref="IInvocationInfo"/> instance that contains information about the method call itself.</param>
        /// <returns>The return value of the method call.</returns>
        public static object Proceed(this IInvocationInfo info)
        {
            MethodBase targetMethod = info.TargetMethod;
            object target = info.Target;
            object[] arguments = info.Arguments;

            return targetMethod.Invoke(target, arguments);
        }

        /// <summary>
        /// Invokes the currently executing method by using the <paramref name="target"/>
        /// as the target instance, the <see cref="IInvocationInfo.TargetMethod"/> as the method, 
        /// and uses the <see cref="IInvocationInfo.Arguments"/> for the method
        /// arguments.
        /// </summary>
        /// <param name="info">The <see cref="IInvocationInfo"/> instance that contains information about the method call itself.</param>
        /// <param name="target">The target instance that will handle the method call.</param>
        /// <returns>The return value of the method call.</returns>
        public static object Proceed(this IInvocationInfo info, object target)
        {
            MethodBase targetMethod = info.TargetMethod;
            object[] arguments = info.Arguments;

            return targetMethod.Invoke(target, arguments);
        }

        /// <summary>
        /// Invokes the currently executing method by using the <paramref name="target"/>
        /// as the target instance, the <see cref="IInvocationInfo.TargetMethod"/> as the method, 
        /// and uses the <paramref name="arguments"/> for the method
        /// arguments.
        /// </summary>
        /// <param name="info">The <see cref="IInvocationInfo"/> instance that contains information about the method call itself.</param>
        /// <param name="target">The target instance that will handle the method call.</param>
        /// <param name="arguments">The arguments that will be used for the actual method call.</param>
        /// <returns>The return value of the method call.</returns>
        public static object Proceed(this IInvocationInfo info, object target,
                                     params object[] arguments)
        {
            MethodBase targetMethod = info.TargetMethod;
            return targetMethod.Invoke(target, arguments);
        }
    }
}