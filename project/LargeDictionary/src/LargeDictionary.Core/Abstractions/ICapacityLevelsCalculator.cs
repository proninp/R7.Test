using LargeDictionary.Core.Models;

namespace LargeDictionary.Core.Abstractions;

/// <summary>
/// Вычисляет оптимальные capacity для каждого уровня трёхуровневого хранилища
/// на основе ожидаемого количества элементов.
/// </summary>
internal interface ICapacityLevelsCalculator
{
    /// <summary>
    /// Вычисляет capacity для словарей каждого уровня.
    /// </summary>
    /// <param name="capacity">Ожидаемое количество элементов в хранилище.</param>
    /// <returns>Распределение capacity между тремя уровнями.</returns>
    CapacityDistribution CalculateCapacities(long capacity);
}