using System.Collections;
using System.Collections.Generic;

namespace LuaDecompilerCore.Utilities;

/// <summary>
/// An alternate implementation of hashset that is designed to be 
/// </summary>
public struct ArraySet<T> : ICollection<T>, ISet<T>, IReadOnlyCollection<T>, IReadOnlySet<T>
{
    private int _count;
    private int _count1;

    public IEnumerator<T> GetEnumerator()
    {
        throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    void ICollection<T>.Add(T item)
    {
        throw new System.NotImplementedException();
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool IReadOnlySet<T>.Contains(T item)
    {
        throw new System.NotImplementedException();
    }

    bool IReadOnlySet<T>.IsProperSubsetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool IReadOnlySet<T>.IsProperSupersetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool IReadOnlySet<T>.IsSubsetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool IReadOnlySet<T>.IsSupersetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool IReadOnlySet<T>.Overlaps(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool IReadOnlySet<T>.SetEquals(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool ISet<T>.IsSubsetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool ISet<T>.IsSupersetOf(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool ISet<T>.Overlaps(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool ISet<T>.SetEquals(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    public void UnionWith(IEnumerable<T> other)
    {
        throw new System.NotImplementedException();
    }

    bool ISet<T>.Add(T item)
    {
        throw new System.NotImplementedException();
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    bool ICollection<T>.Contains(T item)
    {
        throw new System.NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new System.NotImplementedException();
    }

    int ICollection<T>.Count => _count;

    public bool IsReadOnly { get; }

    int IReadOnlyCollection<T>.Count => _count1;
}