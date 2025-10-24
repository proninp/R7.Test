using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LargeDictionary.Core.Abstractions;
using LargeDictionary.Core.Models;

namespace LargeDictionary.Core;

public class LargeDictionary<TValue> : IEnumerable<KeyValuePair<long, TValue>>
{
    private readonly IMatrixDistributionStrategyHandler<TValue> _strategy;
    private readonly Dictionary<int, Dictionary<int, Dictionary<long, TValue>>> _matrix;
    private long _count;

    public LargeDictionary(long capacity = 0, IMatrixDistributionStrategyHandler<TValue>? strategy = null)
    {
        _strategy = strategy ?? new MatrixDistributionStrategyHandler<TValue>();
        _matrix = _strategy.Initialize(capacity);
        _count = 0;
    }

    public long Count => _count;

    public TValue this[long key]
    {
        get
        {
            var index = _strategy.Distribute(key);
            if (!TryGetElement(index, out var value))
            {
                throw new KeyNotFoundException($"Key '{key}' was not found in the dictionary.");
            }
            return value;
        }
        set
        {
            var index = _strategy.Distribute(key);
            var segment = GetOrCreateSegment(index);
            var isNewKey = !segment.ContainsKey(index.BucketIndex);
            segment[key] = value;
            if (isNewKey)
            {
                _count++;
            }
        }
    }

    public void Add(long key, TValue value)
    {
        var index = _strategy.Distribute(key);
        var segment = GetOrCreateSegment(index);
        
        if (!segment.TryAdd(index.BucketIndex, value))
        {
            throw new ArgumentException($"Duplicate key {key}");
        }
        _count++;
    }

    public bool TryAdd(long key, TValue value)
    {
        var index = _strategy.Distribute(key);
        var segment = GetOrCreateSegment(index);
        if (segment.TryAdd(index.BucketIndex, value))
        {
            _count++;
            return true;
        }

        return false;
    }

    public bool TryGetValue(long key, [MaybeNullWhen(false)]out TValue value)
    {
        var index = _strategy.Distribute(key);
        if (!TryGetSegment(index, out var segment))
        {
            value = default;
            return false;
        }
        return segment.TryGetValue(index.BucketIndex, out value);
    }

    public bool Remove(long key)
    {
        var index = _strategy.Distribute(key);
        if (!TryGetSegment(index, out var segment))
        {
            return false;
        }

        var isRemoved = segment.Remove(index.BucketIndex);
        if (isRemoved)
        {
            _count--;
            if (segment.Count == 0)
            {
                _matrix[index.NodeIndex].Remove(index.SegmentIndex);
            }
        }

        return isRemoved;
    }

    public bool ContainsKey(long key)
    {
        var index = _strategy.Distribute(key);
        TryGetSegment(index, out var segment);
        return segment?.ContainsKey(index.BucketIndex) ?? false;
    }

    public void Clear()
    {
        _matrix.Clear();
        _count = 0;
    }

    public IEnumerator<KeyValuePair<long, TValue>> GetEnumerator()
    {
        foreach (var node in _matrix.Values)
        {
            foreach (var segment in node.Values)
            {
                foreach (var kvp in segment)
                {
                    yield return kvp;
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool TryGetSegment(MatrixIndex index, [MaybeNullWhen(false)] out Dictionary<long, TValue> segment)
    {
        segment = null;
        if (!_matrix.TryGetValue(index.NodeIndex, out var node))
        {
            return false;
        }

        if (!node.TryGetValue(index.SegmentIndex, out segment))
        {
            return false;
        }
        return true;
    }

    private Dictionary<long, TValue> GetOrCreateSegment(MatrixIndex index)
    {
        if (!_matrix.TryGetValue(index.NodeIndex, out var node))
        {
            node = new Dictionary<int, Dictionary<long, TValue>>();
            _matrix.Add(index.NodeIndex, node);
        }

        if (!node.TryGetValue(index.SegmentIndex, out var segment))
        {
            segment = new Dictionary<long, TValue>();
            node.Add(index.SegmentIndex, segment);
        }
        return segment;
    }

    private bool TryGetElement(MatrixIndex index, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;
        if (!_matrix.TryGetValue(index.NodeIndex, out var segments))
        {
            return false;
        }

        if (!segments.TryGetValue(index.SegmentIndex, out var segment))
        {
            return false;
        }

        if (!segment.TryGetValue(index.BucketIndex, out var segmentValue))
        {
            return false;
        }
        value = segmentValue;
        return true;
    }
}