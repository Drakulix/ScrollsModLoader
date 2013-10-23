﻿using System;
using LinFu.IoC.Factories;
using LinFu.IoC.Interfaces;

namespace LinFu.IoC.Configuration
{
    /// <summary>
    /// Represents a fluent class that allows
    /// users to create specific types of factories.
    /// </summary>
    /// <typeparam name="TService">The type of service being created.</typeparam>
    internal class GenerateFactory<TService> : IGenerateFactory<TService>
    {
        private readonly InjectionContext<TService> _context;

        /// <summary>
        /// Instantiates the class using the given
        /// <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The <see cref="InjectionContext{T}"/> instance
        /// which will contain the information necessary to build a fluent command.</param>
        internal GenerateFactory(InjectionContext<TService> context)
        {
            _context = context;
        }

        #region IGenerateFactory<TService> Members

        /// <summary>
        /// Creates a singleton factory.
        /// </summary>
        /// <seealso cref="SingletonFactory{T}"/>
        public void AsSingleton()
        {
            AddFactory(adapter => new SingletonFactory<TService>(adapter));
        }

        /// <summary>
        /// Creates a once per thread factory.
        /// </summary>
        /// <seealso cref="OncePerThreadFactory{T}"/>
        public void OncePerThread()
        {
            AddFactory(adapter => new OncePerThreadFactory<TService>(adapter));
        }

        /// <summary>
        /// Creates a once per request factory.
        /// </summary>
        /// <seealso cref="OncePerRequestFactory{T}"/>
        public void OncePerRequest()
        {
            AddFactory(adapter => new OncePerRequestFactory<TService>(adapter));
        }

        #endregion

        /// <summary>
        /// Adds a factory to the container by using the 
        /// <paramref name="createFactory"/> delegate to
        /// create the actual <see cref="IFactory{T}"/>
        /// instance.
        /// </summary>
        /// <param name="createFactory">The delegate that will create the actual factory instance.</param>
        private void AddFactory(Func<Func<IFactoryRequest, TService>,
                                    IFactory<TService>> createFactory)
        {
            IServiceContainer container = _context.Container;
            Func<IFactoryRequest, TService> adapter = _context.FactoryMethod.CreateAdapter();
            IFactory<TService> factory = createFactory(adapter);
            string serviceName = _context.ServiceName;

            container.AddFactory(serviceName, factory);
        }
    }
}