using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace LuaDecompilerCore.Utilities;

/// <summary>
/// An array of bitsets that all share the same size. This is intended to be a memory allocation friendly way to
/// represent sets for a variety of control flow graph analyses.
///
/// This implementation heavily leverages the official .NET BitSetArray class, which is licensed under the MIT license
/// by the .NET Foundation.
/// </summary>
public class BitSetArray : IDisposable
{
    private readonly int[] _array;
    private readonly int _setCount;
    private readonly int _setLength;
    private readonly int _setInt32Length;
    
    public readonly struct BitSet
    {
        private readonly BitSetArray _parent;
        private readonly int _setIndex;

        public BitSet(BitSetArray parent, int setIndex)
        {
            _parent = parent;
            _setIndex = setIndex;
        }

        public int Count => _parent._setLength;
        
        public bool this[int index]
        {
            get => _parent.Get(_setIndex, index);
            set => _parent.Set(_setIndex, index, value);
        }

        public void SetAll(bool value)
        {
            _parent.SetAll(_setIndex, value);
        }

        public bool Equals(BitSet other)
        {
            return _parent.Equals(_setIndex, other._setIndex, other._parent);
        }
        
        public bool CompareCopyFrom(BitSet other)
        {
            return _parent.CompareCopyFrom(_setIndex, other._setIndex, other._parent);
        }
        
        public void And(BitSet other)
        {
            _parent.And(_setIndex, other._setIndex, other._parent);
        }
        
        public void Or(BitSet other)
        {
            _parent.Or(_setIndex, other._setIndex, other._parent);
        }
        
        public void Xor(BitSet other)
        {
            _parent.Xor(_setIndex, other._setIndex, other._parent);
        }

        public void Not(BitSet other)
        {
            _parent.Not(_setIndex);
        }

        public override string ToString()
        {
            var popCount = (int)_parent.PopCount(_setIndex);
            StringBuilder b = new StringBuilder(popCount * 3);
            var elements = new uint[popCount];
            _parent.CopySetIndicesToSpan(_setIndex, new Span<uint>(elements));
            b.Append('{');
            for (var i = 0; i < elements.Length; i++)
            {
                b.Append(elements[i]);
                if (i != elements.Length - 1)
                    b.Append(", ");
            }
            b.Append('}');
            return b.ToString();
        }
    }
    
    /*=========================================================================
    ** Allocates space to hold length bit values. All of the values in the bit
    ** array are set to defaultValue.
    **
    ** Exceptions: ArgumentOutOfRangeException if length < 0.
    =========================================================================*/
    public BitSetArray(int setCount, int setLength, bool defaultValue = false)
    {
        if (setCount < 0 || setLength < 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        _setInt32Length = GetInt32ArrayLengthFromBitLength(setLength);
        _array = ArrayPool<int>.Shared.Rent(_setInt32Length * setCount);
        _setLength = setLength;
        _setCount = setCount;

        if (defaultValue)
        {
            Array.Fill(_array, -1, 0, _setInt32Length * setCount);

            // clear high bit values in the last int
            Div32Rem(setLength, out int extraBits);
            if (extraBits > 0)
            {
                for (var i = 0; i < _setCount; i++)
                    _array[_setInt32Length * (i + 1) - 1] = (1 << extraBits) - 1;
            }
        }
        else
        {
            Array.Fill(_array, 0, 0, _setInt32Length * setCount);
        }
    }

    private const int Vector128ByteCount = 16;
    private const int Vector128IntCount = 4;
    private const int Vector256ByteCount = 32;
    private const int Vector256IntCount = 8;

    /*=========================================================================
    ** Allocates a new BitSetArray with the same length and bit values as bits.
    **
    ** Exceptions: ArgumentException if bits == null.
    =========================================================================*/
    public BitSetArray(BitSetArray bits)
    {
        ArgumentNullException.ThrowIfNull(bits);
        
        _array = ArrayPool<int>.Shared.Rent(bits._setInt32Length * bits._setCount);
        Array.Copy(bits._array, _array, bits._setInt32Length * bits._setCount);
        _setInt32Length = bits._setInt32Length;
        _setLength = bits._setLength;
        _setCount = bits._setCount;
    }

    public BitSet this[int index] => new BitSet(this, index);

    /// <summary>
    /// Mostly for debugger support
    /// </summary>
    public BitSet[] Sets
    {
        get
        {
            var sets = new BitSet[_setCount];
            for (var i = 0; i < _setCount; i++)
                sets[i] = new BitSet(this, i);
            return sets;
        }
    }

    /*=========================================================================
    ** Returns the bit value at position index.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Get(int set, int index)
    {
        if ((uint)index >= (uint)_setLength)
            ThrowArgumentOutOfRangeException(index);

        return (_array[set * _setInt32Length + (index >> 5)] & (1 << index)) != 0;
    }

    /*=========================================================================
    ** Sets the bit value at position index to value.
    **
    ** Exceptions: ArgumentOutOfRangeException if index < 0 or
    **             index >= GetLength().
    =========================================================================*/
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int set, int index, bool value)
    {
        if ((uint)index >= (uint)_setLength)
            ThrowArgumentOutOfRangeException(index);

        int bitMask = 1 << index;
        ref int segment = ref _array[set * _setInt32Length + (index >> 5)];

        if (value)
        {
            segment |= bitMask;
        }
        else
        {
            segment &= ~bitMask;
        }
    }

    /*=========================================================================
    ** Sets all the bit values to value.
    =========================================================================*/
    public void SetAll(int set, bool value)
    {
        Span<int> span = _array.AsSpan(set * _setInt32Length, _setInt32Length);
        if (value)
        {
            span.Fill(-1);

            // clear high bit values in the last int
            Div32Rem(_setLength, out int extraBits);
            if (extraBits > 0)
            {
                span[^1] &= (1 << extraBits) - 1;
            }
        }
        else
        {
            span.Clear();
        }
    }

    public void ClearAll()
    {
        Array.Clear(_array);
    }
    
    private bool Equals(int set, int targetSet, BitSetArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        if (SetLength != value.SetLength)
            throw new ArgumentException();
        
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);
        var valueSpan = new Span<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        // Unroll loop for count less than Vector256 size.
        switch (_setInt32Length)
        {
            case 7:
                if (thisSpan[6] != valueSpan[6])
                    return false;
                goto case 6;
            case 6:
                if (thisSpan[5] != valueSpan[5])
                    return false;
                goto case 5;
            case 5:
                if (thisSpan[4] != valueSpan[4])
                    return false;
                goto case 4;
            case 4:
                if (thisSpan[3] != valueSpan[3])
                    return false;
                goto case 3;
            case 3:
                if (thisSpan[2] != valueSpan[2])
                    return false;
                goto case 2;
            case 2:
                if (thisSpan[1] != valueSpan[1])
                    return false;
                goto case 1;
            case 1:
                if (thisSpan[0] != valueSpan[0])
                    return false;
                goto Done;
            case 0: goto Done;
        }

        var i = 0;

        var left = new ReadOnlySpan<int>(_array, _setInt32Length * set, _setInt32Length);
        var right = new ReadOnlySpan<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        if (Vector256.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector256IntCount - 1u); i += Vector256IntCount)
            {
                if (Vector256.Create(left[i..]) == Vector256.Create(right[i..]))
                    return false;
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector128IntCount - 1u); i += Vector128IntCount)
            {
                if (Vector128.Create(left[i..]) != Vector128.Create(right[i..]))
                    return false;
            }
        }

        for (; i < _setInt32Length; i++)
            if (thisSpan[i] != valueSpan[i])
                return false;

        Done:
        return true;
    }
    
    public uint PopCount(int set)
    {
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);
        uint count = 0;
        
        // Unroll loop for count less than Vector256 size.
        switch (_setInt32Length)
        {
            case 7: count += (uint)int.PopCount(thisSpan[6]); goto case 6;
            case 6: count += (uint)int.PopCount(thisSpan[5]); goto case 5;
            case 5: count += (uint)int.PopCount(thisSpan[4]); goto case 4;
            case 4: count += (uint)int.PopCount(thisSpan[3]); goto case 3;
            case 3: count += (uint)int.PopCount(thisSpan[2]); goto case 2;
            case 2: count += (uint)int.PopCount(thisSpan[1]); goto case 1;
            case 1: count += (uint)int.PopCount(thisSpan[0]); goto Done;
            case 0: goto Done;
        }

        var i = 0;

        for (; i < _setInt32Length; i++)
            count += (uint)int.PopCount(thisSpan[i]);

        Done:
        return count;
    }
    
    public bool CopySetIndicesToSpan(int set, Span<uint> output)
    {
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);
        int outputCounter = 0;
        var i = 0;

        var left = new ReadOnlySpan<int>(_array, _setInt32Length * set, _setInt32Length);

        while (i < _setInt32Length)
        {
            if (Vector256.IsHardwareAccelerated &&  i < _setInt32Length - (Vector256IntCount - 1u))
            {
                if (Vector256.Sum(Vector256.Create(left[i..])) == 0)
                {
                    i += Vector256IntCount;
                    continue;
                }
            }
            else if (Vector128.IsHardwareAccelerated && i < _setInt32Length - (Vector128IntCount - 1u))
            {
                if (Vector128.Sum(Vector128.Create(left[i..])) == 0)
                {
                    i += Vector128IntCount;
                    continue;
                }
            }

            var value = thisSpan[i++];
            if (value == 0)
                continue;
            
            for (var j = 0; j < 32; j++)
            {
                if ((value & (1 << j)) == 0) continue;
                if (outputCounter >= output.Length) return false;
                output[outputCounter] = (uint)((i - 1) * 32 + j);
                outputCounter++;
            }
        }

        return true;
    }

    private bool CompareCopyFrom(int set, int sourceSet, BitSetArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        if (SetLength != value.SetLength)
            throw new ArgumentException();
        
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);
        var valueSpan = new Span<int>(value._array, _setInt32Length * sourceSet, _setInt32Length);
        bool equals = true;

        // Unroll loop for count less than Vector256 size.
        switch (_setInt32Length)
        {
            case 7:
                if (thisSpan[6] != valueSpan[6])
                {
                    equals = false;
                    thisSpan[6] = valueSpan[6];
                }
                goto case 6;
            case 6:
                if (thisSpan[5] != valueSpan[5])
                {
                    equals = false;
                    thisSpan[5] = valueSpan[5];
                }
                goto case 5;
            case 5:
                if (thisSpan[4] != valueSpan[4])
                {
                    equals = false;
                    thisSpan[4] = valueSpan[4];
                }
                goto case 4;
            case 4:
                if (thisSpan[3] != valueSpan[3])
                {
                    equals = false;
                    thisSpan[3] = valueSpan[3];
                }
                goto case 3;
            case 3:
                if (thisSpan[2] != valueSpan[2])
                {
                    equals = false;
                    thisSpan[2] = valueSpan[2];
                }
                goto case 2;
            case 2:
                if (thisSpan[1] != valueSpan[1])
                {
                    equals = false;
                    thisSpan[1] = valueSpan[1];
                }
                goto case 1;
            case 1:
                if (thisSpan[0] != valueSpan[0])
                {
                    equals = false;
                    thisSpan[0] = valueSpan[0];
                }
                goto Done;
            case 0: goto Done;
        }

        var i = 0;

        var left = new ReadOnlySpan<int>(_array, _setInt32Length * set, _setInt32Length);
        var right = new ReadOnlySpan<int>(value._array, _setInt32Length * sourceSet, _setInt32Length);

        if (Vector256.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector256IntCount - 1u); i += Vector256IntCount)
            {
                var l = Vector256.Create(left[i..]);
                var r = Vector256.Create(right[i..]);
                if (l != r)
                {
                    equals = false;
                    r.CopyTo(thisSpan[i..]);
                }
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector128IntCount - 1u); i += Vector128IntCount)
            {
                var l = Vector128.Create(left[i..]);
                var r = Vector128.Create(right[i..]);
                if (l != r)
                {
                    equals = false;
                    r.CopyTo(thisSpan[i..]);
                }
            }
        }

        for (; i < _setInt32Length; i++)
        {
            if (thisSpan[i] != valueSpan[i])
            {
                equals = false;
                thisSpan[i] = valueSpan[i];
            }
        }

        Done:
        return equals;
    }

    public void And(int set, int targetSet, BitSetArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        if (SetLength != value.SetLength)
            throw new ArgumentException();
        
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);
        var valueSpan = new Span<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        // Unroll loop for count less than Vector256 size.
        switch (_setInt32Length)
        {
            case 7:
                thisSpan[6] &= valueSpan[6];
                goto case 6;
            case 6:
                thisSpan[5] &= valueSpan[5];
                goto case 5;
            case 5:
                thisSpan[4] &= valueSpan[4];
                goto case 4;
            case 4:
                thisSpan[3] &= valueSpan[3];
                goto case 3;
            case 3:
                thisSpan[2] &= valueSpan[2];
                goto case 2;
            case 2:
                thisSpan[1] &= valueSpan[1];
                goto case 1;
            case 1:
                thisSpan[0] &= valueSpan[0];
                goto Done;
            case 0: goto Done;
        }

        var i = 0;

        var left = new ReadOnlySpan<int>(_array, _setInt32Length * set, _setInt32Length);
        var right = new ReadOnlySpan<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        if (Vector256.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector256IntCount - 1u); i += Vector256IntCount)
            {
                Vector256<int> result = Vector256.Create(left[i..]) & Vector256.Create(right[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector128IntCount - 1u); i += Vector128IntCount)
            {
                Vector128<int> result = Vector128.Create(left[i..]) & Vector128.Create(right[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }

        for (; i < _setInt32Length; i++)
            thisSpan[i] &= valueSpan[i];

        Done:
        ;
    }

    public void Or(int set, int targetSet, BitSetArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        if (SetLength != value.SetLength)
            throw new ArgumentException();
        
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);
        var valueSpan = new Span<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        // Unroll loop for count less than Vector256 size.
        switch (_setInt32Length)
        {
            case 7:
                thisSpan[6] |= valueSpan[6];
                goto case 6;
            case 6:
                thisSpan[5] |= valueSpan[5];
                goto case 5;
            case 5:
                thisSpan[4] |= valueSpan[4];
                goto case 4;
            case 4:
                thisSpan[3] |= valueSpan[3];
                goto case 3;
            case 3:
                thisSpan[2] |= valueSpan[2];
                goto case 2;
            case 2:
                thisSpan[1] |= valueSpan[1];
                goto case 1;
            case 1:
                thisSpan[0] |= valueSpan[0];
                goto Done;
            case 0: goto Done;
        }

        var i = 0;

        var left = new ReadOnlySpan<int>(_array, _setInt32Length * set, _setInt32Length);
        var right = new ReadOnlySpan<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        if (Vector256.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector256IntCount - 1u); i += Vector256IntCount)
            {
                Vector256<int> result = Vector256.Create(left[i..]) | Vector256.Create(right[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector128IntCount - 1u); i += Vector128IntCount)
            {
                Vector128<int> result = Vector128.Create(left[i..]) | Vector128.Create(right[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }

        for (; i < _setInt32Length; i++)
            thisSpan[i] |= valueSpan[i];

        Done:
        ;
    }

    public void Xor(int set, int targetSet, BitSetArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        
        if (SetLength != value.SetLength)
            throw new ArgumentException();
        
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);
        var valueSpan = new Span<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        // Unroll loop for count less than Vector256 size.
        switch (_setInt32Length)
        {
            case 7:
                thisSpan[6] ^= valueSpan[6];
                goto case 6;
            case 6:
                thisSpan[5] ^= valueSpan[5];
                goto case 5;
            case 5:
                thisSpan[4] ^= valueSpan[4];
                goto case 4;
            case 4:
                thisSpan[3] ^= valueSpan[3];
                goto case 3;
            case 3:
                thisSpan[2] ^= valueSpan[2];
                goto case 2;
            case 2:
                thisSpan[1] ^= valueSpan[1];
                goto case 1;
            case 1:
                thisSpan[0] ^= valueSpan[0];
                goto Done;
            case 0: goto Done;
        }

        var i = 0;

        var left = new ReadOnlySpan<int>(_array, _setInt32Length * set, _setInt32Length);
        var right = new ReadOnlySpan<int>(value._array, _setInt32Length * targetSet, _setInt32Length);

        if (Vector256.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector256IntCount - 1u); i += Vector256IntCount)
            {
                Vector256<int> result = Vector256.Create(left[i..]) ^ Vector256.Create(right[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector128IntCount - 1u); i += Vector128IntCount)
            {
                Vector128<int> result = Vector128.Create(left[i..]) ^ Vector128.Create(right[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }

        for (; i < _setInt32Length; i++)
            thisSpan[i] ^= valueSpan[i];

        Done:
        ;
    }

    public void Not(int set)
    {
        var thisSpan = new Span<int>(_array, _setInt32Length * set, _setInt32Length);

        // Unroll loop for count less than Vector256 size.
        switch (_setInt32Length)
        {
            case 7: thisSpan[6] = ~thisSpan[6]; goto case 6;
            case 6: thisSpan[5] = ~thisSpan[5]; goto case 5;
            case 5: thisSpan[4] = ~thisSpan[4]; goto case 4;
            case 4: thisSpan[3] = ~thisSpan[3]; goto case 3;
            case 3: thisSpan[2] = ~thisSpan[2]; goto case 2;
            case 2: thisSpan[1] = ~thisSpan[1]; goto case 1;
            case 1: thisSpan[0] = ~thisSpan[0]; goto Done;
            case 0: goto Done;
        }

        var i = 0;

        var left = new ReadOnlySpan<int>(_array, _setInt32Length * set, _setInt32Length);

        if (Vector256.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector256IntCount - 1u); i += Vector256IntCount)
            {
                Vector256<int> result = ~Vector256.Create(left[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            for (; i < (uint)_setInt32Length - (Vector128IntCount - 1u); i += Vector128IntCount)
            {
                Vector128<int> result = ~Vector128.Create(left[i..]);
                result.CopyTo(thisSpan[i..]);
            }
        }

        for (; i < _setInt32Length; i++)
            thisSpan[i] = ~thisSpan[i];

        Done:
        // clear high bit values in the last int
        Div32Rem(_setLength, out int extraBits);
        if (extraBits > 0)
        {
            thisSpan[^1] &= (1 << extraBits) - 1;
        }
    }

    public int SetLength => _setLength;

    public int Count => _setCount;

    public object SyncRoot => this;

    public object Clone() => new BitSetArray(this);
    
    private const int BitShiftPerInt32 = 5;

    /// <summary>
    /// Used for conversion between different representations of bit array.
    /// Returns (n + (32 - 1)) / 32, rearranged to avoid arithmetic overflow.
    /// For example, in the bit to int case, the straightforward calc would
    /// be (n + 31) / 32, but that would cause overflow. So instead it's
    /// rearranged to ((n - 1) / 32) + 1.
    /// Due to sign extension, we don't need to special case for n == 0, if we use
    /// bitwise operations (since ((n - 1) >> 5) + 1 = 0).
    /// This doesn't hold true for ((n - 1) / 32) + 1, which equals 1.
    ///
    /// Usage:
    /// GetArrayLength(77): returns how many ints must be
    /// allocated to store 77 bits.
    /// </summary>
    /// <param name="n"></param>
    /// <returns>how many ints are required to store n bytes</returns>
    private static int GetInt32ArrayLengthFromBitLength(int n)
    {
        Debug.Assert(n >= 0);
        return (n - 1 + (1 << BitShiftPerInt32)) >>> BitShiftPerInt32;
    }

    private static int Div32Rem(int number, out int remainder)
    {
        uint quotient = (uint)number / 32;
        remainder = number & (32 - 1); // equivalent to number % 32, since 32 is a power of 2
        return (int)quotient;
    }

    private static void ThrowArgumentOutOfRangeException(int index)
    {
        throw new ArgumentOutOfRangeException();
    }

    public void Dispose()
    {
        ArrayPool<int>.Shared.Return(_array);
    }
}