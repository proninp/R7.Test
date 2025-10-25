using LargeDictionary.Core;

namespace LargeDictionary.Tests.UnitTests;

public class LargeDictionaryTests
{
    #region Constructor Tests
    
    [Fact]
    public void Constructor_InitialCount_IsZero()
    {
        // Arrange & Act
        var dictionary = new LargeDictionary<int>();
        

        // Assert
        Assert.Equal(0, dictionary.Count);
    }
    
    #endregion
    
    #region Add Tests
    
    [Fact]
    public void Add_NewKey_AddsSuccessfully()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        dictionary.Add(1, "value1");

        // Assert
        Assert.Equal(1, dictionary.Count);
        Assert.True(dictionary.ContainsKey(1));
    }
    
    [Fact]
    public void Add_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => dictionary.Add(1, "value2"));
        Assert.Contains("An item with the same key has already been added", exception.Message);
        Assert.Contains("Key: 1", exception.Message);
    }
    
    [Fact]
    public void Add_MultipleKeys_IncrementsCount()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        dictionary.Add(1, "value1");
        dictionary.Add(2, "value2");
        dictionary.Add(3, "value3");

        // Assert
        Assert.Equal(3, dictionary.Count);
    }
    
    #endregion
    
    #region TryAdd Tests

    [Fact]
    public void TryAdd_NewKey_ReturnsTrueAndIncrementsCount()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result = dictionary.TryAdd(1, "value1");

        // Assert
        Assert.True(result);
        Assert.Equal(1, dictionary.Count);
    }

    [Fact]
    public void TryAdd_DuplicateKey_ReturnsFalseAndCountUnchanged()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.TryAdd(1, "value1");

        // Act
        var result = dictionary.TryAdd(1, "value2");

        // Assert
        Assert.False(result);
        Assert.Equal(1, dictionary.Count);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(-1L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void TryAdd_VariousKeys_WorksCorrectly(long key)
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result = dictionary.TryAdd(key, $"value_{key}");

        // Assert
        Assert.True(result);
        Assert.Equal(1, dictionary.Count);
    }

    #endregion
    
    #region Indexer Tests

    [Fact]
    public void Indexer_Get_ExistingKey_ReturnsValue()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");

        // Act
        var value = dictionary[1];

        // Assert
        Assert.Equal("value1", value);
    }

    [Fact]
    public void Indexer_Get_NonExistingKey_ThrowsKeyNotFoundException()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act & Assert
        var exception = Assert.Throws<KeyNotFoundException>(() => dictionary[1]);
        Assert.Contains("The given key '1' was not present in the dictionary", exception.Message);
    }

    [Fact]
    public void Indexer_Set_NewKey_AddsValue()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        dictionary[1] = "value1";

        // Assert
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("value1", dictionary[1]);
    }

    [Fact]
    public void Indexer_Set_ExistingKey_UpdatesValue()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary[1] = "value1";

        // Act
        dictionary[1] = "value2";

        // Assert
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("value2", dictionary[1]);
    }

    [Fact]
    public void Indexer_Set_MultipleUpdates_CountStaysSame()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        dictionary[1] = "value1";
        dictionary[1] = "value2";
        dictionary[1] = "value3";

        // Assert
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("value3", dictionary[1]);
    }

    #endregion
    
    #region TryGetValue Tests

    [Fact]
    public void TryGetValue_ExistingKey_ReturnsTrueWithValue()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");

        // Act
        var result = dictionary.TryGetValue(1, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal("value1", value);
    }

    [Fact]
    public void TryGetValue_NonExistingKey_ReturnsFalseWithDefault()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result = dictionary.TryGetValue(1, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_NullValue_ReturnsTrueWithNull()
    {
        // Arrange
        var dictionary = new LargeDictionary<string?>();
        dictionary.Add(1, null);

        // Act
        var result = dictionary.TryGetValue(1, out var value);

        // Assert
        Assert.True(result);
        Assert.Null(value);
    }

    #endregion
    
    #region ContainsKey Tests

    [Fact]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");

        // Act
        var result = dictionary.ContainsKey(1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsKey_NonExistingKey_ReturnsFalse()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result = dictionary.ContainsKey(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsKey_AfterRemove_ReturnsFalse()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");
        dictionary.Remove(1);

        // Act
        var result = dictionary.ContainsKey(1);

        // Assert
        Assert.False(result);
    }

    #endregion
    
    #region Remove Tests

    [Fact]
    public void Remove_ExistingKey_ReturnsTrueAndDecrementsCount()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");

        // Act
        var result = dictionary.Remove(1);

        // Assert
        Assert.True(result);
        Assert.Equal(0, dictionary.Count);
        Assert.False(dictionary.ContainsKey(1));
    }

    [Fact]
    public void Remove_NonExistingKey_ReturnsFalseAndCountUnchanged()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result = dictionary.Remove(1);

        // Assert
        Assert.False(result);
        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void Remove_MultipleKeys_DecrementsCountCorrectly()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");
        dictionary.Add(2, "value2");
        dictionary.Add(3, "value3");

        // Act
        dictionary.Remove(2);

        // Assert
        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary.ContainsKey(1));
        Assert.False(dictionary.ContainsKey(2));
        Assert.True(dictionary.ContainsKey(3));
    }

    [Fact]
    public void Remove_AllKeys_CountBecomesZero()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");
        dictionary.Add(2, "value2");

        // Act
        dictionary.Remove(1);
        dictionary.Remove(2);

        // Assert
        Assert.Equal(0, dictionary.Count);
    }

    #endregion
    
    #region Clear Tests

    [Fact]
    public void Clear_EmptyDictionary_CountRemainsZero()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        dictionary.Clear();

        // Assert
        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void Clear_WithData_RemovesAllAndResetsCount()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");
        dictionary.Add(2, "value2");
        dictionary.Add(3, "value3");

        // Act
        dictionary.Clear();

        // Assert
        Assert.Equal(0, dictionary.Count);
        Assert.False(dictionary.ContainsKey(1));
        Assert.False(dictionary.ContainsKey(2));
        Assert.False(dictionary.ContainsKey(3));
    }

    [Fact]
    public void Clear_CanAddAfterClear()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");
        dictionary.Clear();

        // Act
        dictionary.Add(1, "new_value");

        // Assert
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("new_value", dictionary[1]);
    }

    #endregion

    #region TryRemove Tests

    [Fact]
    public void TryRemove_ExistingKey_ReturnsTrueWithValueAndRemoves()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");

        // Act
        var result = dictionary.TryRemove(1, out var value);

        // Assert
        Assert.True(result);
        Assert.Equal("value1", value);
        Assert.Equal(0, dictionary.Count);
        Assert.False(dictionary.ContainsKey(1));
    }

    [Fact]
    public void TryRemove_NonExistingKey_ReturnsFalseWithDefault()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result = dictionary.TryRemove(1, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void TryRemove_NullValue_ReturnsTrueWithNull()
    {
        // Arrange
        var dictionary = new LargeDictionary<string?>();
        dictionary.Add(1, null);

        // Act
        var result = dictionary.TryRemove(1, out var value);

        // Assert
        Assert.True(result);
        Assert.Null(value);
        Assert.Equal(0, dictionary.Count);
    }

    #endregion

    #region GetOrAdd Tests

    [Fact]
    public void GetOrAdd_NonExistingKey_AddsAndReturnsValue()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result = dictionary.GetOrAdd(1, "value1");

        // Assert
        Assert.Equal("value1", result);
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("value1", dictionary[1]);
    }

    [Fact]
    public void GetOrAdd_ExistingKey_ReturnsExistingValueWithoutAdding()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "existing_value");

        // Act
        var result = dictionary.GetOrAdd(1, "new_value");

        // Assert
        Assert.Equal("existing_value", result);
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("existing_value", dictionary[1]);
    }

    [Fact]
    public void GetOrAdd_MultipleCallsWithSameKey_ReturnsFirstValue()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act
        var result1 = dictionary.GetOrAdd(1, "value1");
        var result2 = dictionary.GetOrAdd(1, "value2");
        var result3 = dictionary.GetOrAdd(1, "value3");

        // Assert
        Assert.Equal("value1", result1);
        Assert.Equal("value1", result2);
        Assert.Equal("value1", result3);
        Assert.Equal(1, dictionary.Count);
    }

    #endregion

    #region Count Tests

    [Fact]
    public void Count_InitiallyZero()
    {
        // Arrange & Act
        var dictionary = new LargeDictionary<string>();

        // Assert
        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void Count_AfterAdditions_IncrementsCorrectly()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act & Assert
        Assert.Equal(0, dictionary.Count);
        
        dictionary.Add(1, "value1");
        Assert.Equal(1, dictionary.Count);
        
        dictionary.Add(2, "value2");
        Assert.Equal(2, dictionary.Count);
        
        dictionary.Add(3, "value3");
        Assert.Equal(3, dictionary.Count);
    }

    [Fact]
    public void Count_AfterRemovals_DecrementsCorrectly()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");
        dictionary.Add(2, "value2");
        dictionary.Add(3, "value3");

        // Act & Assert
        Assert.Equal(3, dictionary.Count);
        
        dictionary.Remove(1);
        Assert.Equal(2, dictionary.Count);
        
        dictionary.Remove(2);
        Assert.Equal(1, dictionary.Count);
        
        dictionary.Remove(3);
        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void Count_AfterIndexerSet_UpdatesCorrectly()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act & Assert
        dictionary[1] = "value1"; // New key
        Assert.Equal(1, dictionary.Count);
        
        dictionary[1] = "value2"; // Update existing
        Assert.Equal(1, dictionary.Count);
        
        dictionary[2] = "value3"; // New key
        Assert.Equal(2, dictionary.Count);
    }

    [Fact]
    public void Count_AfterClear_BecomesZero()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        dictionary.Add(1, "value1");
        dictionary.Add(2, "value2");

        // Act
        dictionary.Clear();

        // Assert
        Assert.Equal(0, dictionary.Count);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void ComplexScenario_MixedOperations_WorksCorrectly()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();

        // Act & Assert
        // Add
        dictionary.Add(1, "value1");
        Assert.Equal(1, dictionary.Count);

        // TryAdd duplicate
        Assert.False(dictionary.TryAdd(1, "value2"));
        Assert.Equal(1, dictionary.Count);

        // Indexer set (update)
        dictionary[1] = "updated1";
        Assert.Equal(1, dictionary.Count);
        Assert.Equal("updated1", dictionary[1]);

        // Indexer set (new)
        dictionary[2] = "value2";
        Assert.Equal(2, dictionary.Count);

        // GetOrAdd existing
        var result = dictionary.GetOrAdd(1, "value3");
        Assert.Equal("updated1", result);
        Assert.Equal(2, dictionary.Count);

        // GetOrAdd new
        result = dictionary.GetOrAdd(3, "value3");
        Assert.Equal("value3", result);
        Assert.Equal(3, dictionary.Count);

        // TryRemove
        Assert.True(dictionary.TryRemove(2, out var removedValue));
        Assert.Equal("value2", removedValue);
        Assert.Equal(2, dictionary.Count);

        // Clear
        dictionary.Clear();
        Assert.Equal(0, dictionary.Count);
        Assert.False(dictionary.ContainsKey(1));
    }

    [Fact]
    public void StressTest_ThousandOperations_CountIsAccurate()
    {
        // Arrange
        var dictionary = new LargeDictionary<int>();
        const int count = 1_000_000;

        // Act - Add
        for (var i = 0; i < count; i++)
        {
            dictionary.Add(i, i);
        }

        // Assert
        Assert.Equal(count, dictionary.Count);

        // Act - Update via indexer (count should not change)
        for (var i = 0; i < count; i++)
        {
            dictionary[i] = i * 2;
        }

        // Assert
        Assert.Equal(count, dictionary.Count);

        // Act - Remove half
        for (long i = 0; i < count / 2; i++)
        {
            dictionary.Remove(i);
        }

        // Assert
        Assert.Equal(count / 2, dictionary.Count);

        // Act - Clear
        dictionary.Clear();

        // Assert
        Assert.Equal(0, dictionary.Count);
    }

    [Fact]
    public void NullValues_HandledCorrectly()
    {
        // Arrange
        var dictionary = new LargeDictionary<string?>();

        // Act & Assert
        dictionary.Add(1, null);
        Assert.Equal(1, dictionary.Count);
        Assert.Null(dictionary[1]);

        dictionary[2] = null;
        Assert.Equal(2, dictionary.Count);
        Assert.Null(dictionary[2]);

        var getOrAddResult = dictionary.GetOrAdd(3, null);
        Assert.Null(getOrAddResult);
        Assert.Equal(3, dictionary.Count);

        Assert.True(dictionary.TryRemove(1, out var removed));
        Assert.Null(removed);
        Assert.Equal(2, dictionary.Count);
    }

    [Fact]
    public void LargeKeys_WorkCorrectly()
    {
        // Arrange
        var dictionary = new LargeDictionary<string>();
        var keys = new[] { long.MinValue, -1_000_000L, 0L, 1_000_000L, long.MaxValue };

        // Act
        foreach (var key in keys)
        {
            dictionary.Add(key, $"value_{key}");
        }

        // Assert
        Assert.Equal(keys.Length, dictionary.Count);
        
        foreach (var key in keys)
        {
            Assert.True(dictionary.ContainsKey(key));
            Assert.Equal($"value_{key}", dictionary[key]);
        }
    }

    #endregion
}