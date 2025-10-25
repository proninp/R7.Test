using System.Diagnostics.CodeAnalysis;
using LargeDictionary.Core.Models;

namespace LargeDictionary.Core.Abstractions;

public interface IStorageDistributionStrategy<TValue>
{
    bool TryAdd(long key, TValue value);
    
    bool AddOrUpdate(long key, TValue value);
    
    bool TryGetValue(long key, [NotNullWhen(true)] out TValue? value);
    
    bool Remove(long key);
    
    void Clear();
    
    bool ContainsKey(long key);
}