// <copyright file="ObservableDictionaryUnitTests.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Unit tests framework.
// </copyright>

using Xunit;

namespace IX.Observable.UnitTests
{
    public class ObservableDictionaryUnitTests
    {
        [Fact(DisplayName = "ObservableDictionary ctor, Indexer and Count")]
        public void ObservableDictionaryCount()
        {
            // Arrange
            var numberOfItems = UnitTestsUtils.Random.Next(UnitTestConstants.TestsGeneralMagnitude);
            int[] items = new int[numberOfItems];

            for (var i = 0; i < numberOfItems; i++)
            {
                items[i] = UnitTestsUtils.Random.Next(numberOfItems);
            }

            var numberOfItemsToCheck = UnitTestsUtils.Random.Next(numberOfItems);
            int[] itemsToCheck = new int[numberOfItemsToCheck];

            for (int i = 0; i < numberOfItemsToCheck; i++)
            {
                itemsToCheck[i] = UnitTestsUtils.Random.Next(numberOfItems);
            }

            // Act
            var x = new ObservableDictionary<int, int>(numberOfItems);

            for (var i = 0; i < numberOfItems; i++)
            {
                x.Add(i, items[i]);
            }

            // Assert
            Assert.True(x.Count == numberOfItems);

            foreach (var i in itemsToCheck)
            {
                Assert.Equal(x[i], items[i]);
            }
        }
    }
}