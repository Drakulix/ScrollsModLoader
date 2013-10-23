﻿using System;
using System.Collections.Generic;

namespace LinFu.Reflection
{
    /// <summary>
    /// Represents a specific <see cref="IActionLoader{TTarget, TInput}"/>
    /// type that can load configuration information from an assembly
    /// and apply it to a <typeparamref name="TTarget"/> instance.
    /// </summary>
    /// <typeparam name="TTarget">The target type to configure.</typeparam>
    /// <typeparam name="TAssembly">The assembly type.</typeparam>
    /// <typeparam name="TType">The input type.</typeparam>
    public interface IAssemblyTargetLoader<TTarget, TAssembly, TType> : IActionLoader<TTarget, string>
    {
        /// <summary>
        /// The <see cref="IAssemblyLoader"/> instance that will load
        /// the target assemblies.
        /// </summary>
        IAssemblyLoader<TAssembly> AssemblyLoader { get; set; }

        /// <summary>
        /// The list of ActionLoaders that will be used to
        /// configure the target.
        /// </summary>
        IList<IActionLoader<TTarget, TType>> TypeLoaders { get; }
    }

    /// <summary>
    /// Represents a specific <see cref="IActionLoader{TTarget, TInput}"/>
    /// type that can load configuration information from an assembly
    /// and apply it to a <typeparamref name="TTarget"/> instance.
    /// </summary>
    /// <typeparam name="TTarget">The target type to configure.</typeparam>
    public interface IAssemblyTargetLoader<TTarget>
    {
        /// <summary>
        /// The <see cref="IAssemblyLoader"/> instance that will load
        /// the target assemblies.
        /// </summary>
        IAssemblyLoader AssemblyLoader { get; set; }

        /// <summary>
        /// The list of ActionLoaders that will be used to
        /// configure the target.
        /// </summary>
        IList<IActionLoader<TTarget, Type>> TypeLoaders { get; }
    }
}