using System.Diagnostics.CodeAnalysis;
using LargeDictionary.Core.Models;

namespace LargeDictionary.Core.Abstractions;

/// <summary>
/// Стратегия распределения хранения для значений типа <typeparamref name="TValue"/>.
/// Отвечает за добавление, получение и управление элементами по ключу.
/// </summary>
/// <typeparam name="TValue">Тип значения, сохраняемого в хранилище.</typeparam>
public interface IStorageDistributionStrategy<TValue>
{
    /// <summary>
    /// Попытаться добавить значение по указанному ключу, если ключ ещё не присутствует.
    /// </summary>
    /// <param name="key">Ключ элемента.</param>
    /// <param name="value">Добавляемое значение.</param>
    /// <returns>True если элемент был успешно добавлен; false если ключ уже существует или добавление не удалось.</returns>
    bool TryAdd(long key, TValue value);
    
    /// <summary>
    /// Добавляет новое значение или обновляет существующее по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ элемента.</param>
    /// <param name="value">Добавляемое или обновляемое значение.</param>
    /// <returns>True если операция (добавление или обновление) прошла успешно; false в противном случае.</returns>
    bool AddOrUpdate(long key, TValue value);
    
    /// <summary>
    /// Пытается получить значение по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ искомого элемента.</param>
    /// <param name="value">Выходной параметр. Будет иметь значение, соответствующее ключу, если метод вернёт true.</param>
    /// <returns>True если значение найдено (в этом случае <paramref name="value"/> не null); false если значение по ключу отсутствует.</returns>
    bool TryGetValue(long key, [NotNullWhen(true)] out TValue? value);
    
    /// <summary>
    /// Удаляет элемент по указанному ключу.
    /// </summary>
    /// <param name="key">Ключ удаляемого элемента.</param>
    /// <returns>True если элемент был найден и удалён; false если ключ не найден.</returns>
    bool Remove(long key);
    
    /// <summary>
    /// Очищает всё хранилище, удаляя все элементы.
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Проверяет наличие элемента с указанным ключом.
    /// </summary>
    /// <param name="key">Ключ для проверки.</param>
    /// <returns>True если элемент с таким ключом существует; false в противном случае.</returns>
    bool ContainsKey(long key);
}