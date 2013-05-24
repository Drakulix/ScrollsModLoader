﻿using System.Reflection;
using LinFu.AOP.Interfaces;
using LinFu.IoC.Configuration;
using LinFu.IoC.Configuration.Interfaces;

namespace LinFu.IoC.Interceptors
{
    /// <summary>
    /// A class that provides the most basic functionality for an interceptor.
    /// </summary>
    public abstract class BaseInterceptor : IInterceptor
    {
        private readonly IMethodInvoke<MethodInfo> _methodInvoke;

        /// <summary>
        /// The default constructor.
        /// </summary>
        protected BaseInterceptor()
        {
            _methodInvoke = new MethodInvoke();
        }

        /// <summary>
        /// Initializes the class with the <paramref name="methodInvoke"/> instance.
        /// </summary>
        /// <param name="methodInvoke">The <see cref="IMethodInvoke{TMethod}"/> instance that will invoke the target method.</param>
        protected BaseInterceptor(IMethodInvoke<MethodInfo> methodInvoke)
        {
            _methodInvoke = methodInvoke;
        }

        /// <summary>
        /// Gets the value indicating the <see cref="IMethodInvoke{TMethod}"/> instance
        /// that will be used to invoke the target method.
        /// </summary>
        protected IMethodInvoke<MethodInfo> MethodInvoker
        {
            get { return _methodInvoke; }
        }

        #region IInterceptor Members

        /// <summary>
        /// Intercepts a method call using the given
        /// <see cref="IInvocationInfo"/> instance.
        /// </summary>
        /// <param name="info">The <see cref="IInvocationInfo"/> instance that will 
        /// contain all the necessary information associated with a 
        /// particular method call.</param>
        /// <returns>The return value of the target method. If the return type of the target
        /// method is <see cref="void"/>, then the return value will be ignored.</returns>
        public virtual object Intercept(IInvocationInfo info)
        {
            object target = GetTarget(info);
            MethodBase method = info.TargetMethod;
            object[] arguments = info.Arguments;

            return _methodInvoke.Invoke(target, (MethodInfo) method, arguments);
        }

        #endregion

        /// <summary>
        /// Gets the target object instance.
        /// </summary>
        /// <param name="info">The <see cref="IInvocationInfo"/> instance that describes the current execution context.</param>
        protected abstract object GetTarget(IInvocationInfo info);
    }
}