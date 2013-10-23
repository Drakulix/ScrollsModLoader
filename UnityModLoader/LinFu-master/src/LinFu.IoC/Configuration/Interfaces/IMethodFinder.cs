﻿using System.Collections.Generic;
using System.Reflection;
using LinFu.IoC.Interfaces;

namespace LinFu.IoC.Configuration.Interfaces
{
    /// <summary>
    /// Represents a class that determines which method best matches the
    /// services currently in the target container.
    /// </summary>
    /// <typeparam name="T">The method type to search.</typeparam>
    public interface IMethodFinder<T>
        where T : MethodBase
    {
        /// <summary>
        /// Determines which method best matches the
        /// services currently in the target container.
        /// </summary>
        /// <param name="items">The list of methods to search.</param>
        /// <param name="finderContext">The <see cref="IMethodFinderContext"/> that describes the target method.</param>        
        /// <returns>Returns the method with the most resolvable parameters from the target <see cref="IServiceContainer"/> instance.</returns>
        T GetBestMatch(IEnumerable<T> items, IMethodFinderContext finderContext);
    }
}