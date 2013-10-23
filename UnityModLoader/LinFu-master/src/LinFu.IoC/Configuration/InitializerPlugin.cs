﻿using LinFu.IoC.Interfaces;
using LinFu.Reflection;

namespace LinFu.IoC.Configuration
{
    /// <summary>
    /// A class that injects the <see cref="Initializer"/> postprocessor
    /// into every container that is created or loaded.
    /// </summary>
    public class InitializerPlugin : ILoaderPlugin<IServiceContainer>
    {
        #region ILoaderPlugin<IServiceContainer> Members

        /// <summary>
        /// This override does absolutely nothing.
        /// </summary>
        /// <param name="target">The target container.</param>
        public void BeginLoad(IServiceContainer target)
        {
            // Do nothing
        }

        /// <summary>
        /// Injects the <see cref="Initializer"/> postprocessor into
        /// the container.
        /// </summary>
        /// <param name="target"></param>
        public void EndLoad(IServiceContainer target)
        {
            target.PostProcessors.Add(new Initializer());
        }

        #endregion
    }
}