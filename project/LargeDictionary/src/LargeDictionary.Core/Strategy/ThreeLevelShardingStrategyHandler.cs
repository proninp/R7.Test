using System.Diagnostics.CodeAnalysis;
using LargeDictionary.Core.Abstractions;
using LargeDictionary.Core.Models;
using Configuration = LargeDictionary.Core.Strategy.ThreeLevelStorageConfiguration;

namespace LargeDictionary.Core.Strategy;

/// <summary>
/// <inheritdoc/>
/// </summary>
/// <typeparam name="TValue"><inheritdoc/></typeparam>
internal sealed class ThreeLevelShardingStrategyHandler<TValue> : IStorageDistributionStrategy<TValue>
{
    // Трёхуровневая структура для хранения до 2^62 элементов
    private readonly Dictionary<int, Dictionary<int, Dictionary<int, TValue>>> _storage;
    private readonly CapacityDistribution _distribution;

    public ThreeLevelShardingStrategyHandler(long capacity = 0L, ICapacityLevelsCalculator? calculator = null)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity,
                "The value of capacity can not be less than zero.");
        }
        
        var capacityLevelsCalculator = calculator ?? new CapacityLevelsCalculator();
        _distribution = capacityLevelsCalculator.CalculateCapacities(capacity);
        
        _storage = new Dictionary<int, Dictionary<int, Dictionary<int, TValue>>>(_distribution.FirstLevelCapacity);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="key"><inheritdoc/></param>
    /// <param name="value"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public bool TryAdd(long key, TValue value)
    {
        var keyHolder = SplitIndex(key);
        var thirdLevelStorage = GetOrCreateThirdLevelStorage(keyHolder);
        return thirdLevelStorage.TryAdd(keyHolder.ThirdLevelKey, value);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="key"><inheritdoc/></param>
    /// <param name="value"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public bool AddOrUpdate(long key, TValue value)
    {
        var keyHolder = SplitIndex(key);
        var thirdLevelStorage = GetOrCreateThirdLevelStorage(keyHolder);
        var isNew = !thirdLevelStorage.ContainsKey(keyHolder.ThirdLevelKey);
        thirdLevelStorage[keyHolder.ThirdLevelKey] = value;
        return isNew;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="key"><inheritdoc/></param>
    /// <param name="value"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="key"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Clear() => _storage.Clear();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="key"><inheritdoc/></param>
    /// <returns><inheritdoc/></returns>
    public bool ContainsKey(long key) => TryGetValue(key, out _);

    private Dictionary<int, Dictionary<int, TValue>> GetOrCreateSecondLevelStorage(LevelsKeyHolder keyHolder)
    {
        if (!_storage.TryGetValue(keyHolder.FirstLevelKey, out var secondLevelStorage))
        {
            secondLevelStorage = new Dictionary<int, Dictionary<int, TValue>>(_distribution.SecondLevelCapacity);
            _storage[keyHolder.FirstLevelKey] = secondLevelStorage;
        }

        return secondLevelStorage;
    }

    private Dictionary<int, TValue> GetOrCreateThirdLevelStorage(LevelsKeyHolder keyHolder)
    {
        var secondLevelStorage = GetOrCreateSecondLevelStorage(keyHolder);
        if (!secondLevelStorage.TryGetValue(keyHolder.SecondLevelKey, out var thirdLevelStorage))
        {
            thirdLevelStorage = new Dictionary<int, TValue>(_distribution.ThirdLevelCapacity);
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
        var third = (int)(index & Configuration.ThirdLevelMask);
        var second = (int)((index >> Configuration.ThirdLevelBits) & Configuration.SecondLevelMask);
        var first = (int)((index >> (Configuration.ThirdLevelBits + Configuration.SecondLevelBits)) &
                          Configuration.FirstLevelMask);

        return new LevelsKeyHolder(first, second, third);
    }
}