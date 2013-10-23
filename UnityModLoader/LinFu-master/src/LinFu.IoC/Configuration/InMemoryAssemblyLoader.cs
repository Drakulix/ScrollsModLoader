﻿using System.Reflection;
using LinFu.Reflection;

namespace LinFu.IoC.Configuration
{
    /// <summary>
    /// An assembly loader that returns an existing
    /// <see cref="Assembly"/> from memory.
    /// </summary>
    internal class InMemoryAssemblyLoader : IAssemblyLoader
    {
        private readonly Assembly _targetAssembly;

        /// <summary>
        /// Initializes the class with an existing
        /// <see cref="Assembly"/>.
        /// </summary>
        /// <param name="targetAssembly">The target assembly.</param>
        internal InMemoryAssemblyLoader(Assembly targetAssembly)
        {
            _targetAssembly = targetAssembly;
        }

        #region IAssemblyLoader Members

        public Assembly Load(string assemblyFile)
        {
            return _targetAssembly;
        }

        #endregion
    }
}