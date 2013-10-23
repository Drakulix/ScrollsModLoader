﻿using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using LinFu.IoC.Configuration.Interfaces;
using LinFu.IoC.Interfaces;
using LinFu.Reflection;

namespace LinFu.IoC.Configuration
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IMethodInvoke{TMethod}"/> interface.
    /// </summary>
    public abstract class BaseMethodInvoke<TMethod> : IMethodInvoke<TMethod>, IInitialize
        where TMethod : MethodBase
    {
        private static readonly Dictionary<TMethod, MethodBase> _cache = new Dictionary<TMethod, MethodBase>();
        private IMethodBuilder<TMethod> _builder;

        /// <summary>
        /// Initializes the class with the default values.
        /// </summary>
        public BaseMethodInvoke()
        {
            // HACK: Set the ReflectionMethodBuilder as the default builder
            if (typeof (TMethod) == typeof (MethodInfo))
                _builder = new ReflectionMethodBuilder<MethodInfo>() as IMethodBuilder<TMethod>;
        }

        /// <summary>
        /// Gets or sets the value indicating the method builder that will be used to
        /// create the target method.
        /// </summary>
        protected IMethodBuilder<TMethod> MethodBuilder
        {
            get { return _builder; }
            set { _builder = value; }
        }

        #region IInitialize Members

        /// <summary>
        /// Initializes the class with the <paramref name="source">source service container.</paramref>
        /// </summary>
        /// <param name="source">The <see cref="IServiceContainer"/> instance that will initialize this class.</param>
        public void Initialize(IServiceContainer source)
        {
            _builder = source.GetService<IMethodBuilder<TMethod>>();
        }

        #endregion

        #region IMethodInvoke<TMethod> Members

        /// <summary>
        /// Instantiates an object instance with the <paramref name="targetMethod"/>
        /// and <paramref name="arguments"/>.
        /// </summary>
        /// <param name="target">The target object reference. In this particular case, this parameter will be ignored.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="arguments">The arguments to be used with the method.</param>
        /// <returns>An object reference that represents the method return value.</returns>
        public object Invoke(object target, TMethod targetMethod,
                             object[] arguments)
        {
            object result = null;

            // Reuse the cached results, if possible
            if (!_cache.ContainsKey(targetMethod))
                GenerateTargetMethod(targetMethod);

            MethodBase factoryMethod = _cache[targetMethod];

            try
            {
                result = DoInvoke(target, targetMethod, factoryMethod, arguments);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Invokes the <paramref name="targetMethod"/> with the given <paramref name="arguments"/>.
        /// </summary>
        /// <param name="target">The target instance.</param>
        /// <param name="originalMethod">The original method that describes the target method.</param>
        /// <param name="targetMethod">The actual method that will be invoked.</param>
        /// <param name="arguments">The method arguments.</param>
        /// <returns>The return value from the target method.</returns>
        protected virtual object DoInvoke(object target, TMethod originalMethod, MethodBase targetMethod,
                                          object[] arguments)
        {
            object result = targetMethod.Invoke(target, arguments);

            return result;
        }

        /// <summary>
        /// Creates a <see cref="DynamicMethod"/> that will be used as the 
        /// factory method and stores it in the method cache.
        /// </summary>
        /// <param name="targetMethod">The constructor that will be used to instantiate the target type.</param>
        protected virtual void GenerateTargetMethod(TMethod targetMethod)
        {
            MethodBase result = null;

            // HACK: Since the Mono runtime does not yet implement the DynamicMethod class,
            // we'll actually have to use the constructor itself to construct the target type            
            result = Runtime.IsRunningOnMono ? targetMethod : _builder.CreateMethod(targetMethod);

            // Save the results
            lock (_cache)
            {
                _cache[targetMethod] = result;
            }
        }
    }
}