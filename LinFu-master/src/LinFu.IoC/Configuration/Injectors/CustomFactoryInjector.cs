﻿using System;
using LinFu.IoC.Interfaces;

namespace LinFu.IoC.Configuration.Injectors
{
    /// <summary>
    /// A class that injects unnamed custom <see cref="IFactory"/> instances into a given
    /// service container.
    /// </summary>
    public class CustomFactoryInjector : IPreProcessor
    {
        private readonly IFactory _factory;
        private readonly Type _serviceType;

        /// <summary>
        /// Initializes the class with the given service type and factory.
        /// </summary>
        /// <param name="serviceType">The service type that will be created by the factory.</param>
        /// <param name="factory">The <see cref="IFactory"/> instance that will be used to create the service instance.</param>
        public CustomFactoryInjector(Type serviceType, IFactory factory)
        {
            _serviceType = serviceType;
            _factory = factory;
        }

        #region IPreProcessor Members

        /// <summary>
        /// Injects the given factory into the target container.
        /// </summary>
        /// <param name="request">The <see cref="IServiceRequest"/> instance that describes the service that is currently being requested.</param>
        public void Preprocess(IServiceRequest request)
        {
            // Inject the custom factory if no other
            // replacement exists
            if (request.ActualFactory != null)
                return;

            Type serviceType = request.ServiceType;

            // Skip any service requests for types that are generic type definitions
            if (serviceType.IsGenericTypeDefinition)
                return;

            // If the current service type is a generic type,
            // its type definition must match the given service type
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() != _serviceType)
                return;

            // The service types must match
            if (!serviceType.IsGenericType && serviceType != _serviceType)
                return;

            // Inject the custom factory itself            
            request.ActualFactory = _factory;
        }

        #endregion
    }
}