﻿using System;
using System.Linq;
using System.Reflection;
using LinFu.IoC.Interfaces;

namespace LinFu.IoC.Factories
{
    /// <summary>
    /// Represents a class that uses a <see cref="MulticastDelegate"/>
    /// to instantiate a service instance.
    /// </summary>
    public class DelegateFactory : IFactory
    {
        private readonly MulticastDelegate _targetDelegate;

        /// <summary>
        /// Initializes the class with the given <paramref name="targetDelegate"/>
        /// </summary>
        /// <param name="targetDelegate">The delegate that will be used to instantiate the factory.</param>
        public DelegateFactory(MulticastDelegate targetDelegate)
        {
            if (targetDelegate.Method.ReturnType == typeof (void))
                throw new ArgumentException("The factory delegate must have a return type.");

            _targetDelegate = targetDelegate;
        }

        #region IFactory Members

        /// <summary>
        /// Instantiates the service type using the given delegate.
        /// </summary>
        /// <param name="request">The <see cref="IFactoryRequest"/> that describes the service that needs to be created.</param>
        /// <returns>The service instance.</returns>
        public object CreateInstance(IFactoryRequest request)
        {
            object result = null;
            try
            {
                object target = _targetDelegate.Target;
                MethodInfo method = _targetDelegate.Method;
                int argCount = request.Arguments.Length;
                int methodArgCount = method.GetParameters().Count();

                if (argCount != methodArgCount)
                    throw new ArgumentException("Parameter Count Mismatch");

                result = _targetDelegate.DynamicInvoke(request.Arguments);
            }
            catch (TargetInvocationException ex)
            {
                // Unroll the exception
                throw ex.InnerException;
            }

            return result;
        }

        #endregion
    }
}