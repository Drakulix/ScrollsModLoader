﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinFu.IoC;
using LinFu.IoC.Configuration.Interfaces;
using Moq;
using NUnit.Framework;
using SampleLibrary;
using SampleLibrary.IOC;

namespace LinFu.UnitTests.IOC
{
    [TestFixture]
    public class PropertyInjectionTests : BaseTestFixture
    {
        [Test]
        public void ShouldAutoInjectClassCreatedWithAutoCreate()
        {
            // Configure the container
            var container = new ServiceContainer();
            container.LoadFromBaseDirectory("*.dll");

            var sampleService = new Mock<ISampleService>();
            container.AddService(sampleService.Object);

            var instance =
                (SampleClassWithInjectionProperties) container.AutoCreate(typeof (SampleClassWithInjectionProperties));

            // The container should initialize the SomeProperty method to match the mock ISampleService instance
            Assert.IsNotNull(instance.SomeProperty);
            Assert.AreSame(instance.SomeProperty, sampleService.Object);
        }

        [Test]
        public void ShouldAutoInjectProperty()
        {
            var container = new ServiceContainer();
            container.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "LinFu*.dll");

            var instance = new SampleClassWithInjectionProperties();

            // Initialize the container
            container.Inject<ISampleService>().Using<SampleClass>().OncePerRequest();
            container.Inject<ISampleService>("MyService").Using(c => instance).OncePerRequest();

            var result = container.GetService<ISampleService>("MyService");
            Assert.AreSame(result, instance);

            // On initialization, the instance.SomeProperty value
            // should be a SampleClass type
            Assert.IsNotNull(instance.SomeProperty);
            Assert.IsInstanceOfType(typeof (SampleClass), instance.SomeProperty);
        }

        [Test]
        public void ShouldAutoInjectPropertyWithoutCustomAttribute()
        {
            var container = new ServiceContainer();
            container.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "LinFu*.dll");

            var instance = new SampleClassWithUnmarkedInjectionProperties();

            // Initialize the container with the dummy service
            container.Inject<ISampleService>().Using<SampleClass>().OncePerRequest();
            container.Inject<ISampleService>("MyService").Using(c => instance).OncePerRequest();

            // Enable automatic property injection for every property
            container.SetCustomPropertyInjectionAttribute(null);

            // Get the service instance
            var result = container.GetService<ISampleService>("MyService");
            Assert.AreSame(result, instance);

            // Ensure that the injection occurred
            Assert.IsNotNull(instance.SomeProperty);
            Assert.IsInstanceOfType(typeof (SampleClass), instance.SomeProperty);
        }

        [Test]
        public void ShouldAutoInjectServiceListIntoArrayDependency()
        {
            var container = new ServiceContainer();
            container.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "LinFu*.dll");

            var instance = new SampleClassWithArrayPropertyDependency();

            // Initialize the container
            container.Inject<ISampleService>().Using<SampleClass>().OncePerRequest();
            container.Inject<SampleClassWithArrayPropertyDependency>().Using(c => instance).OncePerRequest();

            var result = container.GetService<SampleClassWithArrayPropertyDependency>();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Property);

            int serviceCount = result.Property.Count();
            Assert.IsTrue(serviceCount > 0);
        }

        [Test]
        public void ShouldDetermineWhichPropertiesShouldBeInjected()
        {
            Type targetType = typeof (SampleClassWithInjectionProperties);
            PropertyInfo targetProperty = targetType.GetProperty("SomeProperty");
            Assert.IsNotNull(targetProperty);

            // Load the property injection filter by default
            var container = new ServiceContainer();
            container.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "LinFu*.dll");

            var filter = container.GetService<IMemberInjectionFilter<PropertyInfo>>();

            Assert.IsNotNull(filter);

            // The filter should return the targetProperty
            IEnumerable<PropertyInfo> properties = filter.GetInjectableMembers(targetType);
            Assert.IsTrue(properties.Count() > 0);

            PropertyInfo result = properties.First();
            Assert.AreEqual(targetProperty, result);
        }

        [Test]
        public void ShouldSetPropertyValue()
        {
            Type targetType = typeof (SampleClassWithInjectionProperties);
            PropertyInfo targetProperty = targetType.GetProperty("SomeProperty");
            Assert.IsNotNull(targetProperty);

            // Configure the target
            var instance = new SampleClassWithInjectionProperties();

            // This is the service that should be assigned
            // to the SomeProperty property
            object service = new SampleClass();

            // Initialize the container
            var container = new ServiceContainer();
            container.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "LinFu*.dll");

            var setter = container.GetService<IPropertySetter>();
            Assert.IsNotNull(setter);

            setter.Set(instance, targetProperty, service);

            Assert.IsNotNull(instance.SomeProperty);
            Assert.AreSame(service, instance.SomeProperty);
        }
    }
}