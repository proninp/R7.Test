namespace LargeDictionary.Core.Strategy;

internal static class ThreeLevelStorageConfiguration
{
    private const int FirstLevelBits = 21;
    public const int SecondLevelBits = 21;
    public const int ThirdLevelBits = 22;

    public const int FirstLevelMask = (1 << FirstLevelBits) - 1;
    public const int SecondLevelMask = (1 << SecondLevelBits) - 1;
    public const int ThirdLevelMask = (1 << ThirdLevelBits) - 1;
    
    public const long FirstLevelMaxSize = 1L << FirstLevelBits; // 2_097_152
    public const long SecondLevelMaxSize = 1L << SecondLevelBits; // 2_097_152
    public const long ThirdLevelMaxSize = 1L << ThirdLevelBits; // 4_194_304
}