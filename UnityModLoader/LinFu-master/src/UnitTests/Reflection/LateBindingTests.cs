﻿using LinFu.IoC.Reflection;
using Moq;
using NUnit.Framework;
using SampleLibrary;
using SampleLibrary.IOC;

namespace LinFu.UnitTests.Reflection
{
    [TestFixture]
    public class LateBindingTests
    {
        [Test]
        public void ShouldCallLateBoundGenericMethodWithFourGenericArguments()
        {
            var mock = new Mock<ISampleServiceWithGenericMethods>();
            mock.Expect(m => m.SomeMethod<object, object, object, object>());

            ISampleServiceWithGenericMethods targetInstance = mock.Object;
            targetInstance.Invoke<object, object, object, object>("SomeMethod");

            mock.VerifyAll();
        }

        [Test]
        public void ShouldCallLateBoundGenericMethodWithThreeGenericArguments()
        {
            var mock = new Mock<ISampleServiceWithGenericMethods>();
            mock.Expect(m => m.SomeMethod<object, object, object>());

            ISampleServiceWithGenericMethods targetInstance = mock.Object;
            targetInstance.Invoke<object, object, object>("SomeMethod");

            mock.VerifyAll();
        }

        [Test]
        public void ShouldCallLateBoundGenericMethodWithTwoGenericArguments()
        {
            var mock = new Mock<ISampleServiceWithGenericMethods>();
            mock.Expect(m => m.SomeMethod<object, object>());

            ISampleServiceWithGenericMethods targetInstance = mock.Object;
            targetInstance.Invoke<object, object>("SomeMethod");

            mock.VerifyAll();
        }

        [Test]
        public void ShouldCallLateBoundMethod()
        {
            var mock = new Mock<ISampleService>();
            mock.Expect(m => m.DoSomething());

            ISampleService targetInstance = mock.Object;
            targetInstance.Invoke("DoSomething", new object[0]);

            mock.VerifyAll();
        }

        [Test]
        public void ShouldCallLateBoundMethodWithGenericMethod()
        {
            var mock = new Mock<ISampleServiceWithGenericMethods>();
            mock.Expect(m => m.SomeMethod<object>());

            ISampleServiceWithGenericMethods targetInstance = mock.Object;
            targetInstance.Invoke<object>("SomeMethod");

            mock.VerifyAll();
        }
    }
}