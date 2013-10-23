﻿using System.Linq;
using System.Reflection;
using LinFu.IoC;
using LinFu.IoC.Configuration;
using LinFu.IoC.Configuration.Interfaces;
using NUnit.Framework;
using SampleLibrary.IOC;

namespace LinFu.UnitTests.Finders
{
    [TestFixture]
    public class MethodFinderTests
    {
        [Test]
        public void ShouldFindGenericMethod()
        {
            var container = new ServiceContainer();
            container.LoadFromBaseDirectory("*.dll");

            var context = new MethodFinderContext(new[] {typeof (object)}, new object[0], typeof (void));
            MethodInfo[] methods =
                typeof (SampleClassWithGenericMethod).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var finder = container.GetService<IMethodFinder<MethodInfo>>();
            MethodInfo result = finder.GetBestMatch(methods, context);

            Assert.IsTrue(result.IsGenericMethod);
            Assert.IsTrue(result.GetGenericArguments().Count() == 1);
        }
    }
}