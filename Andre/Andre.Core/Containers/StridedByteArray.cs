using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Andre.Core;

internal static class StudioEncoding
{
    public static readonly Encoding ASCII;

    public static readonly Encoding ShiftJIS;

    public static readonly Encoding UTF16;

    public static readonly Encoding UTF16BE;

    static StudioEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ASCII = Encoding.ASCII;
        ShiftJIS = Encoding.GetEncoding("shift-jis");
        UTF16 = Encoding.Unicode;
        UTF16BE = Encoding.BigEndianUnicode;
    }
}

/// <summary>
///     An unstructured array backed by a byte array and has a specified stride for the array
/// </summary>
public class StridedByteArray
{
    private byte[] _backing;

    private readonly HashSet<uint> _freeEntries = new();

    public StridedByteArray(uint initialCapacity, uint stride, bool bigEndian = false)
    {
        Capacity = initialCapacity;
        Count = 0;
        Stride = stride;
        _backing = new byte[Capacity * Stride];
        BigEndian = bigEndian;
    }

    public StridedByteArray(byte[] backing, uint stride, bool bigEndian = false)
    {
        if (backing.Length % (int)stride != 0)
            throw new ArgumentException();

        _backing = backing;
        Stride = stride;
        Capacity = (uint)backing.Length / stride;
        Count = Capacity;
        BigEndian = bigEndian;
    }

    public uint Capacity { get; private set; }
    public uint Count { get; private set; }
    public uint Stride { get; }

    public bool BigEndian { get; }

    private void GrowIfNeeded()
    {
        if (Count <= Capacity) return;

        while (Capacity < Count)
            Capacity = Math.Max(Capacity + ((Capacity + 1) / 2), 32);

        Array.Resize(ref _backing, (int)Capacity * (int)Stride);
    }

    private static void EndianSwap(Span<byte> swap)
    {
        if (swap.Length == 0 || (swap.Length & (swap.Length - 1)) != 0)
            throw new ArgumentException();

        swap.Reverse();
    }

    /// <summary>
    ///     Adds a new element that is zeroed out
    /// </summary>
    /// <returns>The index of the new element</returns>
    public uint AddZeroedElement()
    {
        if (_freeEntries.Count > 0)
        {
            var index = _freeEntries.First();
            _freeEntries.Remove(index);
            return index;
        }

        Count++;
        GrowIfNeeded();
        return Count - 1;
    }

    public void RemoveAt(uint index)
    {
        if (index >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(index))
            throw new IndexOutOfRangeException();

        if (index < Count - 1)
        {
            // If we're not deleting the last index, mark it as empty and able to be allocated
            _freeEntries.Add(index);
        }
        else
        {
            // Otherwise we can shrink the head
            Count -= 1;
        }

        // Clear the element
        Array.Clear(_backing, (int)index * (int)Stride, (int)Stride);
    }

    public Span<byte> DataForElement(uint index)
    {
        if (index >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(index))
            throw new IndexOutOfRangeException();

        return new Span<byte>(_backing, (int)index * (int)Stride, (int)Stride);
    }

    /// <summary>
    ///     Copies data at one index to another index
    /// </summary>
    /// <param name="dstindex">The index to copy to</param>
    /// <param name="srcindex">The index to copy from</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void CopyData(uint dstindex, uint srcindex)
    {
        if (dstindex >= Count || srcindex >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(dstindex) || _freeEntries.Contains(srcindex))
            throw new IndexOutOfRangeException();

        if (dstindex == srcindex)
            return;

        Array.Copy(_backing, (int)srcindex * (int)Stride,
            _backing, (int)dstindex * (int)Stride, (int)Stride);
    }

    /// <summary>
    ///     Copies data at one index to another index
    /// </summary>
    /// <param name="dstArray">The array to copy to</param>
    /// <param name="dstindex">The index to copy to</param>
    /// <param name="srcindex">The index to copy from</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void CopyData(StridedByteArray dstArray, uint dstindex, uint srcindex)
    {
        if (dstindex >= dstArray.Count || srcindex >= Count)
            throw new IndexOutOfRangeException();

        if (dstArray._freeEntries.Contains(dstindex) || _freeEntries.Contains(srcindex))
            throw new IndexOutOfRangeException();

        if (Stride > dstArray.Stride)
            throw new ArgumentException();

        Array.Copy(_backing, (int)srcindex * (int)Stride,
            dstArray._backing, (int)dstindex * (int)dstArray.Stride, (int)Stride);
    }

    public bool DataEquals(StridedByteArray dstArray, uint dstindex, uint srcindex)
    {
        if (dstindex >= dstArray.Count || srcindex >= Count)
            throw new IndexOutOfRangeException();

        if (dstArray._freeEntries.Contains(dstindex) || _freeEntries.Contains(srcindex))
            throw new IndexOutOfRangeException();

        if (Stride != dstArray.Stride)
            return false;

        for (var i = 0; i < Stride; i++)
        {
            if (_backing[((int)srcindex * (int)Stride) + i] != dstArray._backing[((int)dstindex * (int)Stride) + i])
                return false;
        }

        return true;
    }

    /// <summary>
    ///     Reads an element interpreted as a specific type at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to read</param>
    /// <param name="offset">The byte offset to get the data from</param>
    /// <typeparam name="T">The type that is being read</typeparam>
    /// <returns>The read element</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public unsafe T ReadValueAtElementOffset<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                        DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            T>
        (uint element, uint offset) where T : struct
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + (uint)Marshal.SizeOf<T>() > Stride)
            throw new ArgumentOutOfRangeException();

        T result;
        fixed (byte* p = &_backing[((int)element * (int)Stride) + (int)offset])
        {
            if (BigEndian)
            {
                var data = new Span<byte>(p, Marshal.SizeOf<T>());
                Span<byte> swap = stackalloc byte[Marshal.SizeOf<T>()];
                data.CopyTo(swap);
                EndianSwap(swap);
                fixed (byte* p2 = swap)
                    result = Marshal.PtrToStructure<T>(new IntPtr(p2));
            }
            else
            {
                result = Marshal.PtrToStructure<T>(new IntPtr(p));
            }
        }

        return result;
    }

    /// <summary>
    ///     Writes an element interpreted as a specific type at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to write to</param>
    /// <param name="offset">The byte offset to write the data to</param>
    /// <param name="value">The value to write</param>
    /// <typeparam name="T">The type that is being read</typeparam>
    /// <exception cref="IndexOutOfRangeException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public unsafe void WriteValueAtElementOffset<T>(uint element, uint offset, T value) where T : struct
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + (uint)Marshal.SizeOf<T>() > Stride)
            throw new ArgumentOutOfRangeException();

        fixed (byte* p = &_backing[((int)element * (int)Stride) + (int)offset])
        {
            Marshal.StructureToPtr(value, new IntPtr(p), true);
            if (BigEndian)
            {
                // If big endian mode we need to byte swap the written data
                var data = new Span<byte>(p, Marshal.SizeOf<T>());
                EndianSwap(data);
            }
        }
    }

    /// <summary>
    ///     Reads an element interpreted as a byte array at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to read</param>
    /// <param name="offset">The byte offset to get the data from</param>
    /// <param name="size">The size of the array</param>
    /// <returns>The read bytes</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public byte[] ReadByteArrayAtElementOffset(uint element, uint offset, uint size)
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + size > Stride)
            throw new ArgumentOutOfRangeException();

        var ret = new byte[size];
        Array.Copy(_backing, ((int)element * (int)Stride) + (int)offset, ret, 0, (int)size);
        return ret;
    }

    /// <summary>
    ///     Writes an element interpreted as a byte array at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to write to</param>
    /// <param name="offset">The byte offset to write the data to</param>
    /// <param name="value">The array to write</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void WriteByteArrayAtElementOffset(uint element, uint offset, byte[] value)
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + value.Length > Stride)
            throw new ArgumentOutOfRangeException();

        Array.Copy(value, 0, _backing, ((int)element * (int)Stride) + (int)offset, value.Length);
    }

    /// <summary>
    ///     Reads an element interpreted as a fixed-length string at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to read</param>
    /// <param name="offset">The byte offset to read the string from</param>
    /// <param name="count">The number of fixed code points to read</param>
    /// <returns>The read string</returns>
    public unsafe string ReadFixedStringAtElementOffset(uint element, uint offset, uint count)
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + count > Stride)
            throw new ArgumentOutOfRangeException();

        string result;
        fixed (byte* p = &_backing[((int)element * (int)Stride) + (int)offset])
        {
            var data = new Span<byte>(p, (int)count);
            int terminator;
            for (terminator = 0; terminator < count; terminator++)
            {
                if (data[terminator] == 0)
                    break;
            }

            result = StudioEncoding.ShiftJIS.GetString(new Span<byte>(p, terminator));
        }

        return result;
    }

    /// <summary>
    ///     Writes a fixed-length string at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to write</param>
    /// <param name="offset">The byte offset to write the string to</param>
    /// <param name="value">The string to write</param>
    /// <param name="count">The maximum length of the string</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public unsafe void WriteFixedStringAtElementOffset(uint element, uint offset, string value, uint count)
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + count > Stride)
            throw new ArgumentOutOfRangeException();

        Array.Clear(_backing, ((int)element * (int)Stride) + (int)offset, (int)count);
        fixed (byte* p = &_backing[((int)element * (int)Stride) + (int)offset])
        {
            var data = new Span<byte>(p, (int)count);
            var bytes = StudioEncoding.ShiftJIS.GetBytes(value + '\0');
            Span<byte> span = new Span<byte>(bytes)[..Math.Min(bytes.Length, (int)count)];
            span.CopyTo(data);
        }
    }

    /// <summary>
    ///     Reads an element interpreted as a fixed-length wide string at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to read</param>
    /// <param name="offset">The byte offset to read the string from</param>
    /// <param name="count">The number of fixed code points to read</param>
    /// <returns>The read string</returns>
    public unsafe string ReadFixedStringWAtElementOffset(uint element, uint offset, uint count)
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + (count * 2) > Stride)
            throw new ArgumentOutOfRangeException();

        string result;
        fixed (byte* p = &_backing[((int)element * (int)Stride) + (int)offset])
        {
            var data = new Span<byte>(p, (int)count * 2);
            int terminator;
            for (terminator = 0; terminator < count; terminator++)
            {
                if (data[terminator * 2] == 0 && data[(terminator * 2) + 1] == 0)
                    break;
            }

            if (BigEndian)
                result = StudioEncoding.UTF16BE.GetString(new Span<byte>(p, terminator * 2));
            else
                result = StudioEncoding.UTF16.GetString(new Span<byte>(p, terminator * 2));
        }

        return result;
    }

    /// <summary>
    ///     Writes a fixed-length wide string at a specific offset of an element array
    /// </summary>
    /// <param name="element">The element to write</param>
    /// <param name="offset">The byte offset to write the string to</param>
    /// <param name="value">The string to write</param>
    /// <param name="count">The maximum length of the string</param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public unsafe void WriteFixedStringWAtElementOffset(uint element, uint offset, string value, uint count)
    {
        if (element >= Count)
            throw new IndexOutOfRangeException();

        if (_freeEntries.Contains(element))
            throw new IndexOutOfRangeException();

        if (offset + (count * 2) > Stride)
            throw new ArgumentOutOfRangeException();

        Array.Clear(_backing, ((int)element * (int)Stride) + (int)offset, (int)count * 2);
        fixed (byte* p = &_backing[((int)element * (int)Stride) + (int)offset])
        {
            var data = new Span<byte>(p, (int)count * 2);
            byte[] bytes;
            if (BigEndian)
                bytes = StudioEncoding.UTF16BE.GetBytes(value + '\0');
            else
                bytes = StudioEncoding.UTF16.GetBytes(value + '\0');
            Span<byte> span = new Span<byte>(bytes)[..Math.Min(bytes.Length, (int)count * 2)];
            span.CopyTo(data);
        }
    }
}
