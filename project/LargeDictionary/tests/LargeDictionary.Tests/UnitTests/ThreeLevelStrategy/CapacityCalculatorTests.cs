using System.Numerics;
using LargeDictionary.Core.Abstractions;
using LargeDictionary.Core.Models;
using LargeDictionary.Core.Strategy;

namespace LargeDictionary.Tests.UnitTests.ThreeLevelStrategy;
using Config = ThreeLevelStorageConfiguration;

public class CapacityCalculatorTests
{
    private static long FMax => Config.FirstLevelMaxSize;
    private static long SMax => Config.SecondLevelMaxSize;
    private static long TMax => Config.ThirdLevelMaxSize;
    
    private readonly ICapacityLevelsCalculator _calculator = new CapacityLevelsCalculator();
    
    private static BigInteger Product(int f, int s, int t)
        => new BigInteger(f) * s * t;
    
    // Общие инварианты, которые должны выполняться для любого ответа
    private static void AssertInvariants(long capacity, CapacityDistribution dist)
    {
        Assert.InRange(dist.FirstLevelCapacity, 1, checked((int)Math.Min(FMax, int.MaxValue)));
        Assert.InRange(dist.SecondLevelCapacity, 1, checked((int)Math.Min(SMax, int.MaxValue)));
        Assert.InRange(dist.ThirdLevelCapacity, 1, checked((int)Math.Min(TMax, int.MaxValue)));

        // Произведение должно покрывать требуемую capacity
        Assert.True(Product(dist.FirstLevelCapacity, dist.SecondLevelCapacity, dist.ThirdLevelCapacity)
                    >= new BigInteger(capacity));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void NonPositiveCapacity_Returns_Minimal_1_1_1(long capacity)
    {
        var dist = Calculate(capacity);
        Assert.Equal(1, dist.FirstLevelCapacity);
        Assert.Equal(1, dist.SecondLevelCapacity);
        Assert.Equal(1, dist.ThirdLevelCapacity);
        AssertInvariants(1, dist); // трактуем как минимальное 1
    }
    
    [Fact]
    public void Capacity_1_Fits_Into_ThirdLevel()
    {
        var dist = Calculate(1);
        Assert.Equal(1, dist.FirstLevelCapacity);
        Assert.Equal(1, dist.SecondLevelCapacity);
        Assert.Equal(1, dist.ThirdLevelCapacity);
        AssertInvariants(1, dist);
    }
    
    [Fact]
    public void Capacity_Equal_ThirdMax_Fits_Into_Single_F_S()
    {
        var dist = Calculate(TMax);
        Assert.Equal(1, dist.FirstLevelCapacity);
        Assert.Equal(1, dist.SecondLevelCapacity);
        Assert.Equal((int)TMax, dist.ThirdLevelCapacity);
        AssertInvariants(TMax, dist);
    }
    
    [Fact]
    public void Capacity_ThirdMax_Plus_One_Splits_Into_Two_SecondLevel_Buckets()
    {
        var cap = TMax + 1;
        var dist = Calculate(cap);
        
        Assert.Equal(1, dist.FirstLevelCapacity);
        Assert.True(dist.SecondLevelCapacity == 2);
        Assert.True(dist.ThirdLevelCapacity <= (int)TMax);

        AssertInvariants(cap, dist);
    }
    
    [Theory]
    [InlineData(10L)]
    [InlineData(1_000L)]
    [InlineData(123_456L)]
    [InlineData(4_000_000L)]                 // чуть меньше TMax
    [InlineData(4_194_304L)]                 // ровно TMax
    [InlineData(4_194_305L)]                 // чуть больше TMax
    [InlineData(1_000_000_000L)]             // 1e9
    [InlineData(10_000_000_000L)]            // 1e10
    [InlineData(1_000_000_000_000L)]         // 1e12
    [InlineData(9_000_000_000_000_000L)]     // 9e15
    [InlineData(long.MaxValue >> 1)]         // 2^62 - 1
    public void TypicalCapacities_Satisfy_Invariants(long capacity)
    {
        var dist = Calculate(capacity);
        AssertInvariants(capacity <= 0 ? 1 : capacity, dist);
    }
    
    private CapacityDistribution Calculate(long c)
        => _calculator.CalculateCapacities(c);
}