using System.Diagnostics.CodeAnalysis;
using LargeDictionary.Core.Abstractions;
using LargeDictionary.Core.Models;

namespace LargeDictionary.Core.Strategy;

public sealed class ThreeLevelShardingStrategyHandler<TValue> : IStorageDistributionStrategy<TValue>
{
    private const int FirstLevelBits = 21;
    private const int SecondLevelBits = 21;
    private const int ThirdLevelBits = 22;

    private const int FirstLevelMask = (1 << FirstLevelBits) - 1;
    private const int SecondLevelMask = (1 << SecondLevelBits) - 1;
    private const int ThirdLevelMask = (1 << ThirdLevelBits) - 1;

    // Трёхуровневая структура для хранения до 2^62 элементов
    private readonly Dictionary<int, Dictionary<int, Dictionary<int, TValue>>> _storage;
    private readonly long _capacity;

    public ThreeLevelShardingStrategyHandler(long capacity = 0L)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity,
                "Величина capacity не может быть отрицательной.");
        }

        _capacity = capacity;
        _storage = new Dictionary<int, Dictionary<int, Dictionary<int, TValue>>>();
    }

    public bool TryAdd(long key, TValue value)
    {
        var keyHolder = SplitIndex(key);
        var thirdLevelStorage = GetOrCreateThirdLevelStorage(keyHolder);
        return thirdLevelStorage.TryAdd(keyHolder.ThirdLevelKey, value);
    }

    public bool AddOrUpdate(long key, TValue value)
    {
        var keyHolder = SplitIndex(key);
        var thirdLevelStorage = GetOrCreateThirdLevelStorage(keyHolder);
        var isNew = !thirdLevelStorage.ContainsKey(keyHolder.ThirdLevelKey);
        thirdLevelStorage[keyHolder.ThirdLevelKey] = value;
        return isNew;
    }

    public bool TryGetValue(long key, [NotNullWhen(true)] out TValue? value)
    {
        value = default;
        var keyHolder = SplitIndex(key);
        if (!_storage.TryGetValue(keyHolder.FirstLevelKey, out var secondLevelStorage))
        {
            return false;
        }

        if (!secondLevelStorage.TryGetValue(keyHolder.SecondLevelKey, out var thirdLevelStorage))
        {
            return false;
        }

        return thirdLevelStorage.TryGetValue(keyHolder.ThirdLevelKey, out value);
    }

    public bool Remove(long key)
    {
        var keyHolder = SplitIndex(key);
        if (!_storage.TryGetValue(keyHolder.FirstLevelKey, out var secondLevelStorage))
        {
            return false;
        }

        if (!secondLevelStorage.TryGetValue(keyHolder.SecondLevelKey, out var thirdLevelStorage))
        {
            return false;
        }

        if (!thirdLevelStorage.Remove(keyHolder.ThirdLevelKey))
        {
            return false;
        }

        if (thirdLevelStorage.Count == 0)
        {
            secondLevelStorage.Remove(keyHolder.SecondLevelKey);
            if (secondLevelStorage.Count == 0)
            {
                _storage.Remove(keyHolder.FirstLevelKey);
            }
        }

        return true;
    }

    public void Clear() => _storage.Clear();

    public bool ContainsKey(long key) => TryGetValue(key, out _);

    private Dictionary<int, Dictionary<int, TValue>> GetOrCreateSecondLevelStorage(LevelsKeyHolder keyHolder)
    {
        if (!_storage.TryGetValue(keyHolder.FirstLevelKey, out var secondLevelStorage))
        {
            secondLevelStorage = new Dictionary<int, Dictionary<int, TValue>>();
            _storage[keyHolder.FirstLevelKey] = secondLevelStorage;
        }

        return secondLevelStorage;
    }

    private Dictionary<int, TValue> GetOrCreateThirdLevelStorage(LevelsKeyHolder keyHolder)
    {
        var secondLevelStorage = GetOrCreateSecondLevelStorage(keyHolder);
        if (!secondLevelStorage.TryGetValue(keyHolder.SecondLevelKey, out var thirdLevelStorage))
        {
            thirdLevelStorage = new Dictionary<int, TValue>();
            secondLevelStorage[keyHolder.SecondLevelKey] = thirdLevelStorage;
        }

        return thirdLevelStorage;
    }


    /// <summary>
    /// Разбивает 64-битный индекс на три уровня ключей:
    /// первый уровень — 21 бит (старшие биты),
    /// второй уровень — 21 бит (средние биты),
    /// третий уровень — 22 бита (младшие биты).
    /// </summary>
    /// <param name="index">64-битный индекс/ключ для разбиения.</param>
    /// <returns>Объект <see cref="LevelsKeyHolder"/>, содержащий три целочисленных ключа для уровней.</returns>
    /// <remarks>
    /// Биты распределены следующим образом:
    /// биты 43-63 (21 бит) — FirstLevelKey,
    /// биты 22-42 (21 бит) — SecondLevelKey,
    /// биты 0-21  (22 бита) — ThirdLevelKey.
    /// Маски и смещения используются из констант FirstLevelMask, SecondLevelMask и ThirdLevelMask.
    /// </remarks>
    private static LevelsKeyHolder SplitIndex(long index)
    {
        var third = (int)(index & ThirdLevelMask);
        var second = (int)((index >> ThirdLevelBits) & SecondLevelMask);
        var first = (int)((index >> (ThirdLevelBits + SecondLevelBits)) & FirstLevelMask);

        return new LevelsKeyHolder(first, second, third);
    }
}