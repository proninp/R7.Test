using System.Diagnostics.CodeAnalysis;
using LargeDictionary.Core.Abstractions;
using LargeDictionary.Core.Strategy;

namespace LargeDictionary.Core;

public class LargeDictionary<TValue>
{
    private readonly IStorageDistributionStrategy<TValue> _storage;
    private long _count;

    public long Count => _count;

    public LargeDictionary(IStorageDistributionStrategy<TValue>? strategy = null)
    {
        _storage = strategy ?? new ThreeLevelShardingStrategyHandler<TValue>();
        _count = 0;
    }

    public TValue this[long key]
    {
        get
        {
            if (TryGetValue(key, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
        }
        set => AddOrUpdate(key, value);
    }

    public void Add(long key, TValue value)
    {
        if (!TryAdd(key, value))
        {
            throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
        }
    }

    public bool TryAdd(long key, TValue newValue)
    {
        if (_storage.TryAdd(key, newValue))
        {
            _count++;
            return true;
        }

        return false;
    }

    public bool TryGetValue(long key, [NotNullWhen(true)]out TValue? value) =>
        _storage.TryGetValue(key, out value);

    public bool ContainsKey(long key) => _storage.ContainsKey(key);

    public bool Remove(long key)
    {
        if (_storage.Remove(key))
        {
            _count--;
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _storage.Clear();
        _count = 0;
    }

    private void AddOrUpdate(long key, TValue value)
    {
        var isNew = _storage.AddOrUpdate(key, value);
        
        if (isNew)
        {
            _count++;
        }
    }
    
    public bool TryRemove(long key, out TValue? value) =>
        TryGetValue(key, out value) && Remove(key);

    public TValue GetOrAdd(long key, TValue value)
    {
        if (TryGetValue(key, out var existingValue))
        {
            return existingValue;
        }

        Add(key, value);
        return value;
    }
}