using LargeDictionary.Core.Models;

namespace LargeDictionary.Core.Abstractions;

public interface IMatrixDistributionStrategyHandler<TValue>
{
    Dictionary<int, Dictionary<int, Dictionary<long, TValue>>>  Initialize(long capacity);
    
    MatrixIndex Distribute(long key);
}