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
                ["bbb"] = 2
            };

            Assert.True(x.Count == 2);
        }
    }
}