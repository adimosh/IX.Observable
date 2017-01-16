// <copyright file="ObservableDictionaryUnitTests.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved. Part of the IX Framework.
// </copyright>

using Xunit;

namespace IX.Observable.UnitTests
{
    public class ObservableDictionaryUnitTests
    {
        [Fact]
        public void ObservableDictionaryStub()
        {
            ObservableDictionary<string, int> x = new ObservableDictionary<string, int>
            {
                ["aaa"] = 1,
                ["bbb"] = 2,
            };

            Assert.True(x.Count == 2);
        }
    }
}