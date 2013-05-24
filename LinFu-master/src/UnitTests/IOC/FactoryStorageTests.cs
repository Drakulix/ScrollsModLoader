﻿using System;
using System.Collections.Generic;
using LinFu.IoC;
using LinFu.IoC.Interfaces;
using Moq;
using NUnit.Framework;

namespace LinFu.UnitTests.IOC
{
    [TestFixture]
    public class FactoryStorageTests : BaseTestFixture
    {
        #region Setup/Teardown

        public override void Init()
        {
            _storage = new FactoryStorage();
        }

        public override void Term()
        {
            _storage = null;
        }

        #endregion

        private IFactoryStorage _storage;

        [Test]
        public void ShouldDistinguishBetweenTwoServicesOfTheSameTypeButDifferentParameters()
        {
            var firstFactory = new Mock<IFactory>();
            var secondFactory = new Mock<IFactory>();

            Type serviceType = typeof (int);

            IEnumerable<Type> firstParameters = new[] {typeof (int), typeof (int)};
            IEnumerable<Type> secondParameters = new[] {typeof (int), typeof (int), typeof (int), typeof (int)};

            _storage.AddFactory("", serviceType, firstParameters, firstFactory.Object);
            _storage.AddFactory("", serviceType, secondParameters, secondFactory.Object);

            Assert.IsTrue(_storage.ContainsFactory("", serviceType, firstParameters));
            Assert.IsTrue(_storage.ContainsFactory("", serviceType, secondParameters));

            // Make sure that the factory returns the correct container
            IFactory firstResult = _storage.GetFactory("", serviceType, firstParameters);
            Assert.AreSame(firstFactory.Object, firstResult);

            IFactory secondResult = _storage.GetFactory("", serviceType, secondParameters);
            Assert.AreSame(secondFactory.Object, secondResult);
        }
    }
}