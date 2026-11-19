using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LuaDecompilerCore.Utilities;

/// <summary>
/// A space efficient implementation for an "array of arrays" where the sizes of the arrays are known up front
/// </summary>
public struct JaggedArray<T> : IDisposable
{
    private int _size;
    private uint[] _offsets;
    private T[] _array;

    /// <summary>
    /// Create a new jagged array with each child array having the specified size. Array elements will be
    /// uninitialized by default.
    /// </summary>
    /// <param name="sizes">The sizes of the child arrays</param>
    /// <param name="initialize">Whether to initialize all the values in the array</param>
    public JaggedArray(ReadOnlySpan<uint> sizes, bool initialize = false)
    {
        _size = sizes.Length;
        _offsets = ArrayPool<uint>.Shared.Rent(sizes.Length);
        uint totalSum = 0;
        for (var i = 0; i < sizes.Length; i++)
        {
            totalSum += sizes[i];
            _offsets[i] = totalSum;
        }
        if (totalSum > 1000000)
            Debugger.Break();
        _array = ArrayPool<T>.Shared.Rent((int)totalSum);

        if (initialize)
        {
            for (var i = 0; i < totalSum; i++)
            {
                _array[i] = default(T) ?? throw new Exception($"No default value for type {typeof(T)}");
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int OffsetOf(int index) => index == 0 ? 0 : (int)_offsets[index - 1];

    public Span<T> this[int index]
    {
        get
        {
            if (_size == 0)
                throw new Exception("Indexing 0 sized jagged array");
            var offset = OffsetOf(index);
            return new Span<T>(_array, offset, (int)_offsets[index] - offset);
        }
    }

    public ReadOnlySpan<T> ReadOnlySpan(int index)
    {
        if (_size == 0)
            throw new Exception("Indexing 0 sized jagged array");
        var offset = OffsetOf(index);
        return new ReadOnlySpan<T>(_array, offset, (int)_offsets[index] - offset);
    }

    public void Dispose()
    {
        ArrayPool<uint>.Shared.Return(_offsets);
        ArrayPool<T>.Shared.Return(_array);
    }
}