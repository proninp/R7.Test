using LargeDictionary.Core.Abstractions;
using LargeDictionary.Core.Models;
using Config = LargeDictionary.Core.Strategy.ThreeLevelStorageConfiguration;

namespace LargeDictionary.Core.Strategy;

/// <summary>
/// <inheritdoc /> 
/// </summary>
internal sealed class CapacityLevelsCalculator : ICapacityLevelsCalculator
{
    private const long FMax = Config.FirstLevelMaxSize;
    private const long SMax = Config.SecondLevelMaxSize;
    private const long TMax = Config.ThirdLevelMaxSize;
    
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <param name="capacity"><inheritdoc /></param>
    /// <returns><inheritdoc /></returns>
    public CapacityDistribution CalculateCapacities(long capacity)
    {
        if (capacity <= 0)
        {
            return new CapacityDistribution(0, 0, 0);
        }

        var firstLevelCapacity = CeilDiv(capacity, SMax * TMax);
        firstLevelCapacity = Math.Clamp(firstLevelCapacity, 1, FMax);
        
        var remainCapacityPerFirstLevel = CeilDiv(capacity, firstLevelCapacity);
        
        var secondLevelCapacity = CeilDiv(remainCapacityPerFirstLevel, TMax);
        secondLevelCapacity = Math.Clamp(secondLevelCapacity, 1, SMax);
        
        var remainCapacityPerSecondLevel = CeilDiv(remainCapacityPerFirstLevel, secondLevelCapacity);
        var thirdLevelCapacity = Math.Clamp(remainCapacityPerSecondLevel, 1, TMax);
        
        return new CapacityDistribution((int)firstLevelCapacity, (int)secondLevelCapacity, (int)thirdLevelCapacity);
    }
    
    private static long CeilDiv(long x, long y)
    {
        if (y <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }
        if (x <= 0)
        {
            return 1;
        }

        return (x + y - 1) / y;
    }
}