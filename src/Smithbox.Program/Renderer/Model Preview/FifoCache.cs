using System;
using System.Collections.Generic;

namespace StudioCore.Renderer;

/// <summary>
///     A cache with a fixed number of entries, where the oldest entry is the first to go.
///     Looking up an entry doesn't change its age. The cache doesn't remove entries by itself:
///     the owner calls <see cref="TryEvictOldest" /> to make room, and disposes what it gets back.
/// </summary>
public class FifoCache<TKey, TValue>
{
    private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _lookup;
    private readonly LinkedList<(TKey Key, TValue Value)> _order = new();

    public FifoCache(int capacity, IEqualityComparer<TKey> comparer = null)
    {
        Capacity = capacity;
        _lookup = new Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>>(comparer);
    }

    public int Capacity { get; }
    public int Count => _lookup.Count;
    public bool IsFull => Count >= Capacity;

    public bool TryGet(TKey key, out TValue value)
    {
        if (_lookup.TryGetValue(key, out var node))
        {
            value = node.Value.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    ///     Adds an entry as the newest. The cache must have room for it.
    /// </summary>
    public void Add(TKey key, TValue value)
    {
        if (IsFull)
        {
            throw new InvalidOperationException("The cache is full.");
        }

        _lookup.Add(key, _order.AddLast((key, value)));
    }

    /// <summary>
    ///     Removes the oldest entry that <paramref name="canEvict" /> allows.
    /// </summary>
    /// <returns>False if none of the entries can be removed.</returns>
    public bool TryEvictOldest(Func<TKey, bool> canEvict, out TValue evicted)
    {
        for (var node = _order.First; node != null; node = node.Next)
        {
            if (canEvict(node.Value.Key))
            {
                evicted = node.Value.Value;
                _lookup.Remove(node.Value.Key);
                _order.Remove(node);
                return true;
            }
        }

        evicted = default;
        return false;
    }

    /// <summary>
    ///     Removes every entry, and returns them from oldest to newest.
    /// </summary>
    public List<TValue> Clear()
    {
        List<TValue> values = new(_order.Count);

        foreach (var (_, value) in _order)
        {
            values.Add(value);
        }

        _order.Clear();
        _lookup.Clear();

        return values;
    }
}
