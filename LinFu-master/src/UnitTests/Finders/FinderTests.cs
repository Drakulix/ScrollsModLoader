﻿using System.Collections.Generic;
using LinFu.Finders;
using LinFu.Finders.Interfaces;
using Moq;
using NUnit.Framework;

namespace LinFu.UnitTests.Finders
{
    [TestFixture]
    public class FinderTests : BaseTestFixture
    {
        private Mock<ICriteria<object>> GetMockCriteria(bool predicateResult, CriteriaType criteriaType, int weight)
        {
            var criteria = new Mock<ICriteria<object>>();
            criteria.Expect(c => c.Predicate).Returns(predicate => predicateResult);
            criteria.Expect(c => c.Type).Returns(criteriaType);
            criteria.Expect(c => c.Weight).Returns(weight);
            return criteria;
        }

        [Test]
        public void ShouldBeAbleToAddCriteriaToList()
        {
            // Return a predicate that always returns true
            var mockCriteria = new Mock<ICriteria<object>>();
            ICriteria<object> criteria = mockCriteria.Object;

            var mockFuzzyItem = new Mock<IFuzzyItem<object>>();
            IFuzzyItem<object> fuzzyItem = mockFuzzyItem.Object;

            // The Test method must be called on the fuzzy item
            mockFuzzyItem.Expect(fuzzy => fuzzy.Test(criteria));

            // Initialize the list of fuzzy items
            var fuzzyList = new List<IFuzzyItem<object>>();
            fuzzyList.Add(fuzzyItem);

            // Apply the criteria
            fuzzyList.AddCriteria(criteria);

            mockCriteria.VerifyAll();
            mockFuzzyItem.VerifyAll();
        }

        [Test]
        public void ShouldBeAbleToAddLambdasAsCriteria()
        {
            var fuzzyList = new List<IFuzzyItem<int>>();
            fuzzyList.Add(5);

            // Directly apply the predicate instead of
            // having to manually construct the criteria
            fuzzyList.AddCriteria(item => item == 5);

            Assert.AreEqual(5, fuzzyList.BestMatch().Item);
        }

        [Test]
        public void ShouldBeAbleToDetermineBestFuzzyMatch()
        {
            var mockFuzzyItem = new Mock<IFuzzyItem<object>>();
            IFuzzyItem<object> fuzzyItem = mockFuzzyItem.Object;

            // This should be the best match
            mockFuzzyItem.Expect(f => f.Confidence).Returns(.8);

            var otherMockFuzzyItem = new Mock<IFuzzyItem<object>>();
            IFuzzyItem<object> fauxFuzzyItem = otherMockFuzzyItem.Object;

            // This fuzzy item should be ignored since it has
            // a lower confidence rate
            otherMockFuzzyItem.Expect(f => f.Confidence).Returns(.1);

            var fuzzyList = new List<IFuzzyItem<object>> {fuzzyItem, fauxFuzzyItem};

            IFuzzyItem<object> bestMatch = fuzzyList.BestMatch();
            Assert.AreSame(bestMatch, fuzzyItem);
        }

        [Test]
        public void ShouldBeAbleToIgnoreFailedOptionalCriteria()
        {
            // The criteria will be set up to fail by default            
            Mock<ICriteria<object>> falseCriteria = GetMockCriteria(false, CriteriaType.Optional, 1);

            // Use the true criteria as the control result
            Mock<ICriteria<object>> trueCriteria = GetMockCriteria(true, CriteriaType.Optional, 1);

            var fuzzyList = new List<IFuzzyItem<object>>();
            var fuzzyItem = new FuzzyItem<object>(new object());

            fuzzyList.Add(fuzzyItem);

            // Apply the criteria
            fuzzyList.AddCriteria(trueCriteria.Object);
            fuzzyList.AddCriteria(falseCriteria.Object);

            // The score must be left unchanged
            // since the criteria is optional and
            // the failed predicate does not count
            // against the current fuzzy item.
            Assert.AreEqual(fuzzyItem.Confidence, 1);
        }

        [Test]
        public void ShouldNotBeAbleToIgnoreFailedCriticalCriteria()
        {
            var fuzzyList = new List<IFuzzyItem<object>>();
            var fuzzyItem = new FuzzyItem<object>(new object());

            fuzzyList.Add(fuzzyItem);

            Mock<ICriteria<object>> trueCriteria = GetMockCriteria(true, CriteriaType.Standard, 2);
            Mock<ICriteria<object>> failedCriteria = GetMockCriteria(false, CriteriaType.Critical, 1);

            // Boost the first item results so that the best match
            // should be biased towards the first item
            fuzzyItem.Test(trueCriteria.Object);

            // Make both items pass the first test
            var secondItem = new FuzzyItem<object>(new object());
            fuzzyList.Add(secondItem);

            fuzzyList.AddCriteria(trueCriteria.Object);

            // The first item should be the best match at this point
            IFuzzyItem<object> bestMatch = fuzzyList.BestMatch();
            Assert.AreSame(bestMatch, fuzzyItem);

            // Remove the second item from the list to avoid the
            // failed critical match
            fuzzyList.Remove(secondItem);

            // The failed critical criteria should eliminate
            // the first match as the best possible match
            fuzzyList.AddCriteria(failedCriteria.Object);

            // Reinsert the second value into the list
            // so that it can be chosen as the best match
            fuzzyList.Add(secondItem);

            // Run the test again
            bestMatch = fuzzyList.BestMatch();

            // The second item should be the best possible match,
            // and the first item should be ignored
            // because of the failed criteria            
            Assert.AreSame(bestMatch, secondItem);
        }

        [Test]
        public void ShouldReturnNullIfAllMatchScoresAreZero()
        {
            var fuzzyItem = new FuzzyItem<object>(new object());
            var fuzzyList = new List<IFuzzyItem<object>> {fuzzyItem};

            IFuzzyItem<object> bestMatch = fuzzyList.BestMatch();

            Assert.IsNull(bestMatch);
        }
    }
}