using System;
using Shouldly;
using Xunit;

namespace Test
{
    [Trait("Category", "Sample")]
    public class SampleTests
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(4, 2 + 2); //Force Rebuild
        }

        [Fact(Skip = "Test skipped because it is a test of the fail")]
        public void BadMath()
        {
            Assert.Equal(4, 1 + 1);
        }

        [Theory]
        [InlineData(3, true)]
        [InlineData(3, false, Skip = "An example of a skipped theory")]
        [InlineData(5, true)]
        [InlineData(6, false)]
        public void MyFirstTheory(int value, bool expected)
        {
            IsOdd(value).ShouldBe(expected);
        }

        bool IsOdd(int value)
        {
            return value % 2 == 1;
        }
    }
}