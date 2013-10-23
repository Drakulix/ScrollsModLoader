﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LinFu.AOP.Cecil;
using LinFu.AOP.Cecil.Extensions;
using LinFu.AOP.Cecil.Interfaces;
using LinFu.AOP.Interfaces;
using LinFu.IoC;
using LinFu.IoC.Configuration;
using LinFu.Proxy;
using LinFu.Proxy.Interfaces;
using LinFu.UnitTests.Tools;
using NUnit.Framework;
using SampleLibrary;
using SampleLibrary.Proxy;

namespace LinFu.UnitTests.Proxy
{
    [TestFixture]
    public class ProxyFactoryTests : BaseTestFixture
    {
        #region Setup/Teardown

        public override void Init()
        {
            loader = new Loader();
            container = new ServiceContainer();

            container.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "*.dll");

            LoadAssemblyUsing(typeof (ProxyFactory));
            LoadAssemblyUsing(typeof (InvocationInfoEmitter));

            filename = string.Format("{0}.dll", Guid.NewGuid());

            // Add the PEVerifier to the proxy generation process
            container.AddService<IVerifier>(new PEVerifier(filename));
        }

        public override void Term()
        {
            loader = null;
            container = null;

            try
            {
                File.Delete(filename);
            }
            catch
            {
                // Do nothing
            }
        }

        #endregion

        private ServiceContainer container;
        private Loader loader;
        private string filename = string.Empty;

        private void LoadAssemblyUsing(Type embeddedType)
        {
            string location = embeddedType.Assembly.Location;
            string directory = Path.GetDirectoryName(location);
            string assemblyFilename = Path.GetFileName(location);

            container.LoadFrom(directory, assemblyFilename);
        }

        private T CreateProxy<T>(Func<IInvocationInfo, object> implementation)
        {
            var factory = container.GetService<IProxyFactory>();

            var interceptor = new MockInterceptor(implementation);
            return factory.CreateProxy<T>(interceptor);
        }

        [Test]
        public void ShouldAllowProxyToInheritFromMultipleInstancesOfTheSameGenericInterfaceType()
        {
            IInterceptor interceptor = new MockInterceptor(body => null);

            var interfaces = new[] {typeof (IList<int>), typeof (IList<double>), typeof (IList<object>)};
            var factory = container.GetService<IProxyFactory>();
            var proxy = factory.CreateProxy<object>(interceptor, interfaces);

            Type proxyType = proxy.GetType();

            // The proxy must implement all of the given interfaces
            foreach (Type currentType in interfaces)
            {
                Assert.IsTrue(currentType.IsAssignableFrom(proxyType));
            }
        }

        [Test]
        public void ShouldCacheProxyTypes()
        {
            var factory = new ProxyFactory();
            Type baseType = typeof (ISampleService);

            Type proxyType = factory.CreateProxyType(baseType, new Type[0]);
            int runCount = 10;

            // All subsequent results must return the same proxy type
            for (int i = 0; i < runCount; i++)
            {
                Type currentType = factory.CreateProxyType(baseType, new Type[0]);
                Assert.AreEqual(proxyType, currentType);
                Assert.AreSame(proxyType, currentType);
            }
        }

        [Test]
        public void ShouldCallInterceptorInstance()
        {
            var factory = container.GetService<IProxyFactory>();
            var mockInterceptor = new MockInterceptor(i => null);

            // Create the proxy instance and then make the call
            var proxyInstance = (ITest) factory.CreateProxy(typeof (object), mockInterceptor, typeof (ITest));
            proxyInstance.Execute();

            // The interceptor must be called
            Assert.IsTrue(mockInterceptor.Called);
        }

        [Test]
        public void ShouldCreateProxyWithVirtualSetterInitializedInCtor()
        {
            var factory = container.GetService<IProxyFactory>();

            // Assign the ref/out value for the int argument
            Func<IInvocationInfo, object> implementation = info =>
                                                               {
                                                                   string methodName = info.TargetMethod.Name;

                                                                   if (methodName == "DoSomething")
                                                                       info.Arguments[0] = 54321;

                                                                   if (methodName == "get_SomeProp")
                                                                       return "blah";

                                                                   return null;
                                                               };

            var interceptor = new MockInterceptor(implementation);
            var proxy = factory.CreateProxy<SampleClassWithPropertyInitializedInCtor>(interceptor);

            int value;
            proxy.DoSomething(out value);

            // The two given arguments should match
            Assert.AreEqual("blah", proxy.SomeProp);
            Assert.AreEqual(54321, value);
        }

        [Test]
        public void ShouldHaveDefaultConstructor()
        {
            var factory = container.GetService<IProxyFactory>();
            Type proxyType = factory.CreateProxyType(typeof (object), new Type[0]);
            Assert.IsNotNull(proxyType);

            ConstructorInfo constructor = proxyType.GetConstructor(new Type[0]);
            Assert.IsTrue(constructor != null);

            object instance = constructor.Invoke(new object[0]);
            Assert.IsNotNull(instance);
        }

        [Test]
        public void ShouldHaveDefaultProxyFactoryInstance()
        {
            var factory = container.GetService<IProxyFactory>();
            Assert.IsNotNull(factory);
            Assert.IsTrue(factory.GetType() == typeof (ProxyFactory));
        }

        [Test]
        public void ShouldHaveSerializableAttribute()
        {
            var factory = new ProxyFactory();
            Type proxyType = factory.CreateProxyType(typeof (ISampleService), new Type[0]);

            object[] customAttributes = proxyType.GetCustomAttributes(typeof (SerializableAttribute), false);
            Assert.IsTrue(customAttributes != null && customAttributes.Count() > 0);
        }

        [Test]
        public void ShouldImplementGivenInterfaces()
        {
            var interfaces = new[] {typeof (ISampleService), typeof (ISampleGenericService<int>)};

            // Note: The interceptor will never be executed
            var interceptor = new MockInterceptor(info => { throw new NotImplementedException(); });
            var factory = container.GetService<IProxyFactory>();

            object proxy = factory.CreateProxy(typeof (object), interceptor, interfaces.ToArray());
            Type proxyType = proxy.GetType();

            // Make sure that the generated proxy implements
            // all of the given interfaces
            foreach (Type currentType in interfaces)
            {
                Assert.IsTrue(currentType.IsAssignableFrom(proxyType));
            }
        }

        [Test]
        public void ShouldImplementIProxy()
        {
            var factory = container.GetService<IProxyFactory>();
            Type proxyType = factory.CreateProxyType(typeof (object), new[] {typeof (ISampleService)});

            object instance = Activator.CreateInstance(proxyType);
            Assert.IsTrue(instance is IProxy);
            Assert.IsTrue(instance is ISampleService);
        }

        [Test]
        public void ShouldReportTypeArgumentsUsedInGenericMethodCall()
        {
            Type genericParameterType = typeof (int);
            var proxy = CreateProxy<ClassWithGenericMethod>(info =>
                                                                {
                                                                    // The generic parameter type must match the given parameter type
                                                                    Assert.IsTrue(
                                                                        info.TypeArguments.Contains(genericParameterType));
                                                                    return null;
                                                                });

            proxy.DoSomething<int>();
        }

        [Test]
        public void ShouldSupportMethodCallWithNestedOpenGenericParameters()
        {
            var dummyList = new Dictionary<int, List<string>>();

            // The dummy list will be altered if the method body is called
            Func<IInvocationInfo, object> methodBody = info =>
                                                           {
                                                               Type[] typeArguments = info.TypeArguments;

                                                               // Match the type arguments

                                                               //Assert.AreEqual(typeArguments[0], typeof(int));

                                                               dummyList.Add(1, new List<string> {"SomeValue"});

                                                               return dummyList[1];
                                                           };

            var proxy = CreateProxy<ClassWithNestedOpenGenericParameters>(methodBody);
            proxy.DoSomething(dummyList);
            Assert.IsTrue(dummyList.Count > 0);
        }

        [Test]
        public void ShouldSupportMethodCallsWithGenericParametersFromGenericMethodTypeArguments()
        {
            Type genericParameterType = typeof (int);
            var proxy = CreateProxy<ClassWithParametersFromGenericMethodTypeArguments>(info =>
                                                                                           {
                                                                                               // Match the type argument
                                                                                               Assert.IsTrue(
                                                                                                   info.TypeArguments.
                                                                                                       Contains(
                                                                                                           genericParameterType));
                                                                                               Assert.AreEqual(1,
                                                                                                               info.
                                                                                                                   Arguments
                                                                                                                   [0]);
                                                                                               Assert.AreEqual(1,
                                                                                                               info.
                                                                                                                   Arguments
                                                                                                                   [1]);
                                                                                               return null;
                                                                                           });

            proxy.DoSomething(1, 1);
        }

        [Test]
        public void ShouldSupportMethodCallsWithGenericParametersFromHostGenericTypeArguments()
        {
            var proxy = CreateProxy<ClassWithParametersFromHostGenericTypeArguments<double, string>>(info =>
                                                                                                         {
                                                                                                             // Match the type arguments
                                                                                                             Assert.
                                                                                                                 AreEqual
                                                                                                                 (info.
                                                                                                                      ParameterTypes
                                                                                                                      [0
                                                                                                                      ],
                                                                                                                  typeof
                                                                                                                      (
                                                                                                                      double
                                                                                                                      ));
                                                                                                             Assert.
                                                                                                                 AreEqual
                                                                                                                 (info.
                                                                                                                      ParameterTypes
                                                                                                                      [1
                                                                                                                      ],
                                                                                                                  typeof
                                                                                                                      (
                                                                                                                      string
                                                                                                                      ));

                                                                                                             // Match the argument values
                                                                                                             Assert.
                                                                                                                 AreEqual
                                                                                                                 (1.0,
                                                                                                                  info.
                                                                                                                      Arguments
                                                                                                                      [0
                                                                                                                      ]);
                                                                                                             Assert.
                                                                                                                 AreEqual
                                                                                                                 ("Test",
                                                                                                                  info.
                                                                                                                      Arguments
                                                                                                                      [1
                                                                                                                      ]);
                                                                                                             return null;
                                                                                                         });

            proxy.DoSomething(1.0, "Test");
        }

        [Test]
        public void ShouldSupportMethodCallsWithGenericReturnValuesFromGenericMethodTypeArguments()
        {
            var dummyList = new List<int>();

            // The dummy list will be altered if the method body is called
            Func<IInvocationInfo, object> methodBody = info =>
                                                           {
                                                               Type[] typeArguments = info.TypeArguments;

                                                               // Match the type arguments
                                                               Assert.AreEqual(typeArguments[0], typeof (int));
                                                               dummyList.Add(12345);
                                                               return 12345;
                                                           };

            var proxy = CreateProxy<ClassWithMethodReturnTypeFromGenericTypeArguments>(methodBody);
            proxy.DoSomething<int>();

            Assert.IsTrue(dummyList.Count > 0);
        }

        [Test]
        public void ShouldSupportMethodCallsWithGenericReturnValuesFromHostGenericTypeArguments()
        {
            var proxy = CreateProxy<ClassWithMethodReturnValueFromTypeArgument<int>>(
                info =>
                    {
                        // Make sure that the method return type 
                        // matches the given return type
                        Assert.IsTrue(info.ReturnType == typeof (int));
                        return 54321;
                    });

            int result = proxy.DoSomething();

            Assert.AreEqual(54321, result);
        }

        [Test]
        public void ShouldSupportMethodCallsWithOpenGenericParameters()
        {
            var dummyList = new List<int>();

            // The dummy list will be altered if the method body is called
            Func<IInvocationInfo, object> methodBody = info =>
                                                           {
                                                               Type[] typeArguments = info.TypeArguments;

                                                               // Match the type arguments

                                                               Assert.AreEqual(typeArguments[0], typeof (int));

                                                               dummyList.Add(12345);

                                                               return dummyList;
                                                           };

            var proxy = CreateProxy<ClassWithOpenGenericParameters>(methodBody);
            proxy.DoSomething(dummyList);
            Assert.IsTrue(dummyList.Count > 0);
        }

        [Test]
        public void ShouldSupportMethodsCallsWithGenericTypeDefinitionReturnType()
        {
            var dummyList = new List<int>();

            // The dummy list will be altered if the method body is called
            Func<IInvocationInfo, object> methodBody = info =>
                                                           {
                                                               Type[] typeArguments = info.TypeArguments;

                                                               // Match the type arguments

                                                               Assert.AreEqual(typeArguments[0], typeof (int));
                                                               dummyList.Add(12345);
                                                               return dummyList;
                                                           };

            var proxy = CreateProxy<ClassWithGenericTypeDefinitionReturnType>(methodBody);
            proxy.DoSomething<int>();
            Assert.IsTrue(dummyList.Count > 0);
        }

        [Test]
        public void ShouldSupportOutArguments()
        {
            var factory = container.GetService<IProxyFactory>();

            // Assign the ref/out value for the int argument
            Func<IInvocationInfo, object> implementation = info =>
                                                               {
                                                                   info.Arguments[0] = 54321;
                                                                   return null;
                                                               };

            var interceptor = new MockInterceptor(implementation);
            var proxy = factory.CreateProxy<ClassWithVirtualMethodWithOutParameter>(interceptor);

            int value;
            proxy.DoSomething(out value);

            // The two given arguments should match
            Assert.AreEqual(54321, value);
        }

        [Test]
        public void ShouldSupportRefArguments()
        {
            var factory = container.GetService<IProxyFactory>();

            // Assign the ref/out value for the int argument
            Func<IInvocationInfo, object> implementation = info =>
                                                               {
                                                                   info.Arguments[0] = 54321;
                                                                   return null;
                                                               };

            var interceptor = new MockInterceptor(implementation);
            var proxy = factory.CreateProxy<ClassWithVirtualByRefMethod>(interceptor);

            int value = 0;
            proxy.ByRefMethod(ref value);

            // The two given arguments should match
            Assert.AreEqual(54321, value);
        }


        [Test]
        public void ShouldSupportSerialization()
        {
            var dummyList = new List<int>();

            // The dummy list will be altered if the method body is called
            Func<IInvocationInfo, object> methodBody = info =>
                                                           {
                                                               Type[] typeArguments = info.TypeArguments;

                                                               // Match the type arguments

                                                               Assert.AreEqual(typeArguments[0], typeof (int));
                                                               dummyList.Add(12345);
                                                               return dummyList;
                                                           };

            var proxy = CreateProxy<ClassWithGenericTypeDefinitionReturnType>(methodBody);
            proxy.DoSomething<int>();
            Assert.IsTrue(dummyList.Count > 0);
        }

        [Test]
        public void ShouldSupportSubclassingFromGenericTypes()
        {
            var factory = container.GetService<IProxyFactory>();
            var actualList = new List<int>();

            Func<IInvocationInfo, object> implementation = info =>
                                                               {
                                                                   IList<int> list = actualList;
                                                                   return info.Proceed(list);
                                                               };
            var interceptor = new MockInterceptor(implementation);
            var proxy = factory.CreateProxy<IList<int>>(interceptor);

            // Any item added to the proxy list should be added to the 
            // actual list
            proxy.Add(12345);

            Assert.IsTrue(interceptor.Called);
            Assert.IsTrue(actualList.Count > 0);
            Assert.IsTrue(actualList[0] == 12345);
        }
    }
}