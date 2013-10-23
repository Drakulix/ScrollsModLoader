﻿using System.Reflection;
using LinFu.IoC.Configuration.Interfaces;

namespace LinFu.IoC.Configuration
{
    /// <summary>
    /// Represents a <see cref="IMethodBuilder{TMethod}"/> type that simply lets 
    /// methods pass through it without performing any modifications to those methods.
    /// </summary>
    public class ReflectionMethodBuilder<TMethod> : IMethodBuilder<TMethod>
        where TMethod : MethodBase
    {
        #region IMethodBuilder<TMethod> Members

        /// <summary>
        /// Returns the <paramref name="existingMethod"/> unmodified.
        /// </summary>
        /// <param name="existingMethod">The method to be modified.</param>
        /// <returns>The modified method.</returns>
        public MethodBase CreateMethod(TMethod existingMethod)
        {
            return existingMethod;
        }

        #endregion
    }
}