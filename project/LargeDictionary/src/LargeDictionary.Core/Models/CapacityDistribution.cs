namespace LargeDictionary.Core.Models;

/// <summary>
/// Информация о распределении capacity между уровнями
/// </summary>
/// <param name="FirstLevelCapacity">Выделенная ёмкость словаря первого уровня (старшие биты)</param>
/// <param name="SecondLevelCapacity">Выделенная ёмкость словаря среднего уровня</param>
/// <param name="ThirdLevelCapacity">Выделенная ёмкость словаря третьего уровня (младшие биты)</param>
internal sealed record CapacityDistribution(
    int FirstLevelCapacity,
    int SecondLevelCapacity,
    int ThirdLevelCapacity);