﻿using System;

namespace LinFu.AOP.Interfaces
{
    /// <summary>
    /// Represents a class that describes a request to instantiate a particular object type.
    /// </summary>
    public class ActivationContext : IActivationContext
    {
        private readonly object[] _additionalArguments;
        private readonly Type _concreteType;

        /// <summary>
        /// Initializes the context with the given parameters.
        /// </summary>
        /// <param name="concreteType">The type to be instantiated.</param>
        /// <param name="additionalArguments">The additional arguments that must be passed to the constructor.</param>
        public ActivationContext(Type concreteType, object[] additionalArguments)
        {
            _concreteType = concreteType;
            _additionalArguments = additionalArguments;
        }

        #region IActivationContext Members

        /// <summary>
        /// Gets the value indicating the type to be instantiated.
        /// </summary>
        public Type TargetType
        {
            get { return _concreteType; }
        }

        /// <summary>
        /// Gets the value indicating the arguments that will be passed to the constructor during instantiation.
        /// </summary>
        public object[] AdditionalArguments
        {
            get { return _additionalArguments; }
        }

        #endregion
    }
}