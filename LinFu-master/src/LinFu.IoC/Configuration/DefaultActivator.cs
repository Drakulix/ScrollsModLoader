﻿using System;
using System.Reflection;
using LinFu.AOP.Interfaces;
using LinFu.IoC.Configuration.Interfaces;
using LinFu.IoC.Interfaces;

namespace LinFu.IoC.Configuration
{
    /// <summary>
    /// Represents a class that can instantiate object instances.
    /// </summary>
    public class DefaultActivator : IActivator<IContainerActivationContext>, IInitialize
    {
        private IConstructorArgumentResolver _argumentResolver;
        private IMethodInvoke<ConstructorInfo> _constructorInvoke;
        private IMemberResolver<ConstructorInfo> _resolver;

        #region IActivator<IContainerActivationContext> Members

        /// <summary>
        /// Creates an object instance.
        /// </summary>
        /// <returns>A valid object instance.</returns>
        public object CreateInstance(IContainerActivationContext context)
        {
            IServiceContainer container = context.Container;
            object[] additionalArguments = context.AdditionalArguments;
            Type concreteType = context.TargetType;

            // Add the required services if necessary
            container.AddDefaultServices();

            var finderContext = new MethodFinderContext(new Type[0], additionalArguments, null);

            // Determine which constructor
            // contains the most resolvable
            // parameters            
            ConstructorInfo constructor = _resolver.ResolveFrom(concreteType, container, finderContext);

            // TODO: Allow users to insert their own custom constructor resolution routines here
            object[] arguments = _argumentResolver.GetConstructorArguments(constructor, container, additionalArguments);

            // Instantiate the object
            object result = _constructorInvoke.Invoke(null, constructor, arguments);

            return result;
        }

        #endregion

        #region IInitialize Members

        /// <summary>
        /// Initializes the class with the default services.
        /// </summary>
        /// <param name="container">The target service container.</param>
        public void Initialize(IServiceContainer container)
        {
            _resolver = container.GetService<IMemberResolver<ConstructorInfo>>();
            _constructorInvoke = container.GetService<IMethodInvoke<ConstructorInfo>>();
            _argumentResolver = container.GetService<IConstructorArgumentResolver>();
        }

        #endregion
    }
}