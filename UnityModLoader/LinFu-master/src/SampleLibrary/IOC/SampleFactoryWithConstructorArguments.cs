﻿using System;
using LinFu.IoC.Configuration;
using LinFu.IoC.Interfaces;

namespace SampleLibrary.IOC
{
    [Factory(typeof (string), ServiceName = "SampleFactoryWithConstructorArguments")]
    public class SampleFactoryWithConstructorArguments : IFactory
    {
        public ISampleService _sample;

        public SampleFactoryWithConstructorArguments(ISampleService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            _sample = service;
        }

        #region IFactory Members

        public object CreateInstance(IFactoryRequest request)
        {
            return "42";
        }

        #endregion
    }
}