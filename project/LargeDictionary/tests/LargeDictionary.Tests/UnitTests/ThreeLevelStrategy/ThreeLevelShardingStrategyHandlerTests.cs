using LargeDictionary.Core.Strategy;

namespace LargeDictionary.Tests.UnitTests.ThreeLevelStrategy;

public class ThreeLevelShardingStrategyHandlerTests
{
    [Fact]
    public void Constructor_WithZeroCapacity_CreatesEmptyStorage()
    {
        // Arrange & Act
        var handler = new ThreeLevelShardingStrategyHandler<string>(0);

        // Assert
        Assert.False(handler.ContainsKey(0));
    }
    
    [Fact]
    public void Constructor_WithPositiveCapacity_CreatesStorageWithCapacity()
    {
        // Arrange & Act
        var handler = new ThreeLevelShardingStrategyHandler<string>(1000);

        // Assert - should not throw
        Assert.False(handler.ContainsKey(0));
    }
    
    [Fact]
    public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ThreeLevelShardingStrategyHandler<string>(-1));
        
        Assert.Equal("capacity", exception.ParamName);
        Assert.Contains("can not be less than zero", exception.Message);
    }
    
    [Fact]
    public void TryAdd_NewKey_ReturnsTrue()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act
        var result = handler.TryAdd(1, "value1");

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void TryAdd_DuplicateKey_ReturnsFalse()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");

        // Act
        var result = handler.TryAdd(1, "value2");

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void TryAdd_MultipleKeys_AllAdded()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<int>();

        // Act & Assert
        Assert.True(handler.TryAdd(1, 100));
        Assert.True(handler.TryAdd(2, 200));
        Assert.True(handler.TryAdd(3, 300));
    }
    
    [Fact]
    public void TryAdd_NegativeKey_AddsSuccessfully()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act
        var result = handler.TryAdd(-1, "negative");

        // Assert
        Assert.True(result);
        Assert.True(handler.ContainsKey(-1));
    }
    
    [Fact]
    public void TryAdd_MaxLongKey_AddsSuccessfully()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act
        var result = handler.TryAdd(long.MaxValue, "max");

        // Assert
        Assert.True(result);
        Assert.True(handler.ContainsKey(long.MaxValue));
    }
    
    [Fact]
    public void TryAdd_MinLongKey_AddsSuccessfully()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act
        var result = handler.TryAdd(long.MinValue, "min");

        // Assert
        Assert.True(result);
        Assert.True(handler.ContainsKey(long.MinValue));
    }
    
    [Fact]
    public void AddOrUpdate_NewKey_ReturnsTrueAndAddsValue()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        const string value1 = "value1";
        
        // Act
        var isNew = handler.AddOrUpdate(1, value1);

        // Assert
        Assert.True(isNew);
        Assert.True(handler.TryGetValue(1, out var value));
        Assert.Equal(value1, value);
    }
    
    [Fact]
    public void AddOrUpdate_ExistingKey_ReturnsFalseAndUpdatesValue()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");
        const string value2 = "value2";

        // Act
        var isNew = handler.AddOrUpdate(1, value2);

        // Assert
        Assert.False(isNew);
        Assert.True(handler.TryGetValue(1, out var value));
        Assert.Equal(value2, value);
    }
    
    [Fact]
    public void TryGetValue_ExistingKey_ReturnsTrueWithValue()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");

        // Act
        var result = handler.TryGetValue(1, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal("value1", value);
    }
    
    [Fact]
    public void TryGetValue_NonExistingKey_ReturnsFalseWithDefault()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act
        var result = handler.TryGetValue(1, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
    
    [Fact]
    public void TryGetValue_AfterRemove_ReturnsFalse()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");
        handler.Remove(1);

        // Act
        var result = handler.TryGetValue(1, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
    
    [Fact]
    public void TryGetValue_NullValue_ReturnsTrueWithNull()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string?>();
        handler.TryAdd(1, null);

        // Act
        var result = handler.TryGetValue(1, out var value);

        // Assert
        Assert.True(result);
        Assert.Null(value);
    }
    
    [Fact]
    public void Remove_ExistingKey_ReturnsTrueAndRemovesValue()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");

        // Act
        var result = handler.Remove(1);

        // Assert
        Assert.True(result);
        Assert.False(handler.ContainsKey(1));
    }

    [Fact]
    public void Remove_NonExistingKey_ReturnsFalse()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act
        var result = handler.Remove(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Remove_AlreadyRemovedKey_ReturnsFalse()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");
        handler.Remove(1);

        // Act
        var result = handler.Remove(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Remove_MultipleKeys_RemovesOnlySpecified()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");
        handler.TryAdd(2, "value2");
        handler.TryAdd(3, "value3");

        // Act
        handler.Remove(2);

        // Assert
        Assert.True(handler.ContainsKey(1));
        Assert.False(handler.ContainsKey(2));
        Assert.True(handler.ContainsKey(3));
    }
    
    [Fact]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");

        // Act
        var result = handler.ContainsKey(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsKey_NonExistingKey_ReturnsFalse()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act
        var result = handler.ContainsKey(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsKey_AfterRemove_ReturnsFalse()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");
        handler.Remove(1);

        // Act
        var result = handler.ContainsKey(1);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Clear_WithData_RemovesAllEntries()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");
        handler.TryAdd(2, "value2");
        handler.TryAdd(3, "value3");

        // Act
        handler.Clear();

        // Assert
        Assert.False(handler.ContainsKey(1));
        Assert.False(handler.ContainsKey(2));
        Assert.False(handler.ContainsKey(3));
    }

    [Fact]
    public void Clear_CanAddAfterClear()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();
        handler.TryAdd(1, "value1");
        handler.Clear();

        // Act
        var result = handler.TryAdd(1, "new_value");

        // Assert
        Assert.True(result);
        Assert.True(handler.ContainsKey(1));
    }
    
    [Fact]
    public void MultipleOperations_ComplexScenario_WorksCorrectly()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<string>();

        // Act & Assert
        Assert.True(handler.TryAdd(1, "value1"));
        Assert.True(handler.TryAdd(2, "value2"));
        Assert.False(handler.AddOrUpdate(1, "updated1"));
        Assert.True(handler.ContainsKey(1));
        Assert.True(handler.Remove(2));
        Assert.False(handler.ContainsKey(2));
        
        handler.Clear();
        Assert.False(handler.ContainsKey(1));
    }

    [Fact]
    public void LargeNumberOfKeys_DifferentLevels_WorksCorrectly()
    {
        // Arrange
        var handler = new ThreeLevelShardingStrategyHandler<int>();

        // Act
        var keys = new[]
        {
            0L,
            1L,
            100L,
            10_000L,
            1_000_000L,
            100_000_000L,
            10_000_000_000L
        };

        foreach (var key in keys)
        {
            handler.TryAdd(key, (int)key);
        }

        // Assert
        foreach (var key in keys)
        {
            Assert.True(handler.TryGetValue(key, out var value));
            Assert.Equal((int)key, value);
        }
    }
    
    [Fact]
    public void StressTest_ThousandOperations_WorksCorrectly()
    {
        // Arrange
        const int count = 10_000_000;
        var handler = new ThreeLevelShardingStrategyHandler<int>(count);

        // Act - Add
        for (long i = 0; i < count; i++)
        {
            Assert.True(handler.TryAdd(i, (int)i));
        }

        // Assert - Contains
        for (long i = 0; i < count; i++)
        {
            Assert.True(handler.ContainsKey(i));
        }

        // Assert - Get
        for (long i = 0; i < count; i++)
        {
            Assert.True(handler.TryGetValue(i, out var value));
            Assert.Equal((int)i, value);
        }

        // Act - Update
        for (long i = 0; i < count; i++)
        {
            Assert.False(handler.AddOrUpdate(i, (int)(i * 2)));
        }

        // Assert - Updated values
        for (long i = 0; i < count; i++)
        {
            Assert.True(handler.TryGetValue(i, out var value));
            Assert.Equal((int)(i * 2), value);
        }

        // Act - Remove half
        for (long i = 0; i < count / 2; i++)
        {
            Assert.True(handler.Remove(i));
        }

        // Assert - Removed and remaining
        for (long i = 0; i < count / 2; i++)
        {
            Assert.False(handler.ContainsKey(i));
        }
        for (long i = count / 2; i < count; i++)
        {
            Assert.True(handler.ContainsKey(i));
        }
    }
}