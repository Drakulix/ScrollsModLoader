﻿namespace LinFu.AOP.Interfaces
{
    /// <summary>
    /// Represents a type that can intercept activation requests.
    /// </summary>
    public interface IActivatorHost
    {
        /// <summary>
        /// Gets or sets the value indicating the <see cref="ITypeActivator"/> that
        /// will be used to instantiate object types.
        /// </summary>
        ITypeActivator Activator { get; set; }
    }
}