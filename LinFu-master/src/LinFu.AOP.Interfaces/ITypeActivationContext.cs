﻿using System.Reflection;

namespace LinFu.AOP.Interfaces
{
    /// <summary>
    /// Represents a special type of <see cref="IActivationContext"/> that can be used to instantiate a given type
    /// and can be used to describe the method that invoked the instantiation operation as well as specify the object
    /// instance that invoked the instantiation itself.
    /// </summary>
    public interface ITypeActivationContext : IActivationContext
    {
        /// <summary>
        /// Gets the value indicating the object instance that initiated the object instantiation operation.
        /// </summary>
        object Target { get; }

        /// <summary>
        /// Gets the value indiating the <see cref="MethodBase"/> instance that initiated the object instantiation operation.
        /// </summary>
        MethodBase TargetMethod { get; }
    }
}