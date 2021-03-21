using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SimpleCache.UnitTests
{
    public class CacheTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public CacheTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Should_BeInitiatedCorrectly()
        {
            // Arrange

            // Act
            var sut = new Cache<string>(100);

            // Assert
            sut.Count.Should().Be(0);
            sut.Capacity.Should().Be(100);
        }
 
        
        [Fact]
        public void Should_Get_ReturnsNullValue_WhenTheCacheIsEmpty()
        {
            // Arrange
            var sut = new Cache<string>(100);

            // 
            var value = sut.Get("SampleKey");

            // Assert
            value.Should().BeNull();
        }
        
        [Theory]
        [InlineData("Key#1", null)]
        [InlineData("Key#2", "Key#2Value")]
        public void Should_Get_ReturnsValueBasedOnIfTheKeyISAvailableInTheCache(string key, string expectedValue)
        {
            // Arrange
            var sut = new Cache<string>(100);
            sut.Set("Key#2", "Key#2Value");

            // Act
            var value = sut.Get(key);

            // Assert
            value.Should().Be(expectedValue);
        }
        
        [Fact]
        public void Should_Set_ReplacesTheValue_WhenTheKeyIsAlreadyAvailableInCache()
        {
            // Arrange
            var key = "Key#2";
            var sut = new Cache<string>(100);
            
            sut.Set(key, "Key#2Value");
            sut.Set(key, "Key#2Value-Replaced");

            // Act
            var value = sut.Get(key);

            // Assert
            value.Should().Be("Key#2Value-Replaced");
            sut.Count.Should().Be(1);
        }
        
        [Fact]
        public void Should_Set_RemoveTheFirstItem_WhenTheCacheIsOnItsCapacityAndTheFirstItemHadNoUse()
        {
            // Arrange
            var sut = new Cache<string>(2);

            // Act
            sut.Set("Key#1", "Key#1Value");
            sut.Set("Key#2", "Key#2Value");
            sut.Set("Key#3", "Key#3Value");

            var value1 = sut.Get("Key#1");
            var value2 = sut.Get("Key#2");
            var value3 = sut.Get("Key#3");

            // Assert
            sut.Count.Should().Be(2);
            value1.Should().BeNull();
            value2.Should().Be("Key#2Value");
            value3.Should().Be("Key#3Value");
        }
        
        [Fact]
        public void Should_Set_RemoveTheLeastUsedItem_WhenTheCacheIsOnItsCapacity()
        {
            // Arrange
            var sut = new Cache<string>(3);

            // Act
            sut.Set("Key#1", "Key#1Value");
            sut.Set("Key#2", "Key#2Value");
            sut.Set("Key#3", "Key#3Value");
            var value = sut.Get("Key#1");
            sut.Set("Key#2", "Key#2Value_Replaced");
            sut.Set("Key#4", "Key#4Value");

            var value1 = sut.Get("Key#1");
            var value2 = sut.Get("Key#2");
            var value3 = sut.Get("Key#3");
            var value4 = sut.Get("Key#4");

            // Assert
            sut.Count.Should().Be(3);
            value4.Should().Be("Key#4Value");
            value3.Should().BeNull();
            value2.Should().Be("Key#2Value_Replaced");
            value1.Should().Be("Key#1Value");
        }
        
        [Fact]
        public void Should_Set_GetItemEvictedNotification_WhenRemoveTheLeastUsedItemAndTheCacheIsOnItsCapacity()
        {
            // Arrange
            var notified = false;
            var sut = new Cache<string>(2, opt => { opt.OnItemEvicted = n => notified = true;});

            // Act
            sut.Set("Key#1", "Key#1Value");
            sut.Set("Key#2", "Key#2Value");
            sut.Set("Key#3", "Key#3Value");

            // Assert
            sut.Count.Should().Be(2);
            notified.Should().BeTrue();
        }
        
        [Fact]
        public void Should_Set_RemoveTheLeastUsedItemInMultiThreadEnvironment_WhenTheCacheIsOnItsCapacity()
        {
            // Arrange
            var sut = new Cache<string>(3);

            // Act
            var task1 = Task.Run(async () =>
            {
                sut.Set("Key#1", "Key#1Value");
                sut.Set("Key#2", "Key#2Value");
                sut.Set("Key#3", "Key#3Value");
                sut.Get("Key#1");

                await Task.Delay(1000);
                sut.Get("Key#3");
            }, CancellationToken.None);
            
            var task2 = Task.Run(async () =>
            {
                await Task.Delay(1000);
                sut.Set("Key#4", "Key#4Value");
            }, CancellationToken.None);

            Task.WaitAll(task1, task2);
            
            var value1 = sut.Get("Key#1");
            var value2 = sut.Get("Key#2");
            var value3 = sut.Get("Key#3");
            var value4 = sut.Get("Key#4");

            _testOutputHelper.WriteLine(value1 ?? "Null");
            _testOutputHelper.WriteLine(value2 ?? "Null");
            _testOutputHelper.WriteLine(value3 ?? "Null");
            _testOutputHelper.WriteLine(value4 ?? "Null");
            
            // Assert
            sut.Count.Should().Be(3);
            value1.Should().Be("Key#1Value");
            value3.Should().Be("Key#3Value");
            value4.Should().Be("Key#4Value");
            value2.Should().BeNull();
        }
    }
}