using System.Diagnostics.CodeAnalysis;
using LargeDictionary.Core.Abstractions;
using LargeDictionary.Core.Strategy;

namespace LargeDictionary.Core;

/// <summary>
/// Представляет словарь с ключами типа <see cref="long"/> и значениями типа <typeparamref name="TValue"/>,
/// использующий стратегию распределения хранилища.
/// </summary>
public class LargeDictionary<TValue>
{
    private readonly IStorageDistributionStrategy<TValue> _storage;
    private long _count;

    /// <summary>
    /// Количество элементов в словаре.
    /// </summary>
    public long Count => _count;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="LargeDictionary{TValue}"/>.
    /// </summary>
    /// <param name="capacity">Ожидаемая ёмкость словаря (опционально).</param>
    /// <param name="strategy">Стратегия распределения хранилища (если не указана, используется ThreeLevelShardingStrategyHandler).</param>
    public LargeDictionary(long capacity = 0, IStorageDistributionStrategy<TValue>? strategy = null)
    {
        _storage = strategy ?? new ThreeLevelShardingStrategyHandler<TValue>(capacity);
        _count = 0;
    }

    /// <summary>
    /// Получает или задаёт значение по ключу.
    /// </summary>
    /// <param name="key">Ключ элемента.</param>
    /// <returns>Значение, связанное с ключом.</returns>
    /// <exception cref="KeyNotFoundException">Бросается при попытке получить несуществующий ключ.</exception>
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

    /// <summary>
    /// Добавляет новый элемент в словарь.
    /// </summary>
    /// <param name="key">Ключ добавляемого элемента.</param>
    /// <param name="value">Значение добавляемого элемента.</param>
    /// <exception cref="ArgumentException">Бросается, если элемент с таким ключом уже существует.</exception>
    public void Add(long key, TValue value)
    {
        if (!TryAdd(key, value))
        {
            throw new ArgumentException($"An item with the same key has already been added. Key: {key}");
        }
    }

    /// <summary>
    /// Пытается добавить новый элемент.
    /// </summary>
    /// <param name="key">Ключ добавляемого элемента.</param>
    /// <param name="newValue">Добавляемое значение.</param>
    /// <returns><c>true</c>, если элемент успешно добавлен; иначе <c>false</c>.</returns>
    public bool TryAdd(long key, TValue newValue)
    {
        if (_storage.TryAdd(key, newValue))
        {
            _count++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Пытается получить значение по ключу.
    /// </summary>
    /// <param name="key">Ключ искомого элемента.</param>
    /// <param name="value">Выходной параметр: значение, связанное с ключом. Гарантированно ненулевой, если возвращается <c>true</c>.</param>
    /// <returns><c>true</c>, если значение найдено; иначе <c>false</c>.</returns>
    public bool TryGetValue(long key, [NotNullWhen(true)]out TValue? value) =>
        _storage.TryGetValue(key, out value);

    /// <summary>
    /// Проверяет наличие ключа в словаре.
    /// </summary>
    /// <param name="key">Ключ для проверки.</param>
    /// <returns><c>true</c>, если ключ присутствует; иначе <c>false</c>.</returns>
    public bool ContainsKey(long key) => _storage.ContainsKey(key);

    /// <summary>
    /// Удаляет элемент по ключу.
    /// </summary>
    /// <param name="key">Ключ удаляемого элемента.</param>
    /// <returns><c>true</c>, если элемент был удалён; иначе <c>false</c>.</returns>
    public bool Remove(long key)
    {
        if (_storage.Remove(key))
        {
            _count--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Очищает словарь.
    /// </summary>
    public void Clear()
    {
        _storage.Clear();
        _count = 0;
    }
    
    /// <summary>
    /// Пытается получить и удалить значение по ключу.
    /// </summary>
    /// <param name="key">Ключ элемента.</param>
    /// <param name="value">Выходной параметр: значение, если удаление успешно.</param>
    /// <returns><c>true</c>, если элемент был найден и удалён; иначе <c>false</c>.</returns>
    public bool TryRemove(long key, out TValue? value) =>
        TryGetValue(key, out value) && Remove(key);

    /// <summary>
    /// Возвращает существующее значение по ключу или добавляет указанное значение, если ключ отсутствует.
    /// </summary>
    /// <param name="key">Ключ элемента.</param>
    /// <param name="value">Значение, которое будет добавлено, если ключ отсутствует.</param>
    /// <returns>Существующее или только что добавленное значение.</returns>
    public TValue GetOrAdd(long key, TValue value)
    {
        if (TryGetValue(key, out var existingValue))
        {
            return existingValue;
        }

        Add(key, value);
        return value;
    }
    
    /// <summary>
    /// Добавляет или обновляет значение для указанного ключа.
    /// </summary>
    /// <param name="key">Ключ элемента.</param>
    /// <param name="value">Новое значение.</param>
    private void AddOrUpdate(long key, TValue value)
    {
        var isNew = _storage.AddOrUpdate(key, value);
        
        if (isNew)
        {
            _count++;
        }
    }
}