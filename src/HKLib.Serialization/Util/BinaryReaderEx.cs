using System.Drawing;
using System.Numerics;
using System.Text;

namespace HKLib.Serialization.Util;

// Adapted from JKAnderson's Soulsformats: https://github.com/JKAnderson/SoulsFormats

/// <summary>
/// An extended reader for binary data supporting big and little endianness, value assertions, and arrays.
/// </summary>
public class BinaryReaderEx
{
    private BinaryReader br;
    private Stack<long> steps;

    /// <summary>
    /// Initializes a new BinaryReaderEx reading from the specified byte array.
    /// </summary>
    public BinaryReaderEx(bool bigEndian, byte[] input) : this(bigEndian, new MemoryStream(input))
    {
    }

    /// <summary>
    /// Initializes a new BinaryReaderEx reading from the specified stream.
    /// </summary>
    public BinaryReaderEx(bool bigEndian, Stream stream)
    {
        BigEndian = bigEndian;
        steps = new Stack<long>();
        Stream = stream;
        br = new BinaryReader(stream);
    }

    /// <summary>
    /// Interpret values as big-endian if set, or little-endian if not.
    /// </summary>
    public bool BigEndian { get; }

    /// <summary>
    /// Varints are read as Int64 if set, otherwise Int32.
    /// </summary>
    public bool VarintLong { get; set; }

    /// <summary>
    /// Current size of varints in bytes.
    /// </summary>
    public int VarintSize => VarintLong ? 8 : 4;

    /// <summary>
    /// The underlying stream.
    /// </summary>
    public Stream Stream { get; }

    /// <summary>
    /// The current position of the stream.
    /// </summary>
    public long Position
    {
        get => Stream.Position;
        set => Stream.Position = value;
    }

    /// <summary>
    /// The length of the stream.
    /// </summary>
    public long Length => Stream.Length;

    /// <summary>
    /// Reads length bytes and returns them in reversed order.
    /// </summary>
    private byte[] ReadReversedBytes(int length)
    {
        byte[] bytes = ReadBytes(length);
        Array.Reverse(bytes);
        return bytes;
    }

    /// <summary>
    /// Reads a value from the specified offset using the given function, with the given endianness which defaults to the value
    /// of the <see cref="BigEndian" /> property when null, returning the stream to its original position
    /// afterwards.
    /// </summary>
    protected T GetValue<T>(Func<bool?, T> readValue, long offset, bool? bigEndian)
    {
        StepIn(offset);
        T result = readValue(bigEndian);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads a value from the specified offset using the given function, returning the stream to its original position
    /// afterwards.
    /// </summary>
    protected T GetValue<T>(Func<T> readValue, long offset)
    {
        StepIn(offset);
        T result = readValue();
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads an array of values from the specified offset using the given function, with the given endianness which defaults
    /// to the value of the <see cref="BigEndian" /> property when null, returning the stream to its original
    /// position afterwards.
    /// </summary>
    protected T[] GetValues<T>(Func<int, bool?, T[]> readValues, long offset, int count, bool? bigEndian)
    {
        StepIn(offset);
        T[] result = readValues(count, bigEndian);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads an array of values from the specified offset using the given function, returning the stream to its original
    /// position afterwards.
    /// </summary>
    protected T[] GetValues<T>(Func<int, T[]> readValues, long offset, int count)
    {
        StepIn(offset);
        T[] result = readValues(count);
        StepOut();
        return result;
    }

    /// <summary>
    /// Compares a value to a list of options, returning it if found or excepting if not.
    /// </summary>
    protected T AssertValue<T>(T value, string typeName, string valueFormat, T[] options) where T : IEquatable<T>
    {
        foreach (T option in options)
            if (value.Equals(option))
                return value;

        string strValue = string.Format(valueFormat, value);
        string strOptions = string.Join(", ", options.Select(o => string.Format(valueFormat, o)));
        throw new InvalidDataException(
            $"Read {typeName}: {strValue} | Expected: {strOptions} | Ending position: 0x{Position:X}");
    }

    /// <summary>
    /// Store the current position of the stream on a stack, then move to the specified offset.
    /// </summary>
    public void StepIn(long offset)
    {
        steps.Push(Stream.Position);
        Stream.Position = offset;
    }

    /// <summary>
    /// Restore the previous position of the stream from a stack.
    /// </summary>
    public void StepOut()
    {
        if (steps.Count == 0)
            throw new InvalidOperationException("Reader is already stepped all the way out.");

        Stream.Position = steps.Pop();
    }

    /// <summary>
    /// Advances the stream position until it meets the specified alignment.
    /// </summary>
    public void Pad(int align)
    {
        if (Stream.Position % align > 0)
            Stream.Position += align - (Stream.Position % align);
    }

    /// <summary>
    /// Advances the stream position until it meets the specified alignment relative to the given starting position.
    /// </summary>
    public void PadRelative(long start, int align)
    {
        long relPos = Stream.Position - start;
        if (relPos % align > 0)
            Stream.Position += align - (relPos % align);
    }

    /// <summary>
    /// Advances the stream position by count bytes.
    /// </summary>
    public void Skip(int count)
    {
        Stream.Position += count;
    }

    /// <inheritdoc cref="BinaryReader.Close" />
    public virtual void Close() => br.Close();

    #region Boolean

    /// <summary>
    /// Reads a one-byte boolean value.
    /// </summary>
    public bool ReadBoolean()
    {
        // BinaryReader.ReadBoolean accepts any non-zero value as true, which I don't want.
        byte b = br.ReadByte();
        if (b == 0)
            return false;
        else if (b == 1)
            return true;
        else
            throw new InvalidDataException($"ReadBoolean encountered non-boolean value: 0x{b:X2}");
    }

    /// <summary>
    /// Reads an array of one-byte boolean values.
    /// </summary>
    public bool[] ReadBooleans(int count)
    {
        var result = new bool[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadBoolean();
        return result;
    }

    /// <summary>
    /// Reads a one-byte boolean value from the specified offset without advancing the stream.
    /// </summary>
    public bool GetBoolean(long offset)
    {
        return GetValue(ReadBoolean, offset);
    }

    /// <summary>
    /// Reads an array of one-byte boolean values from the specified offset without advancing the stream.
    /// </summary>
    public bool[] GetBooleans(long offset, int count)
    {
        return GetValues(ReadBooleans, offset, count);
    }

    /// <summary>
    /// Reads a one-byte boolean value and throws an exception if it does not match the specified option.
    /// </summary>
    public bool AssertBoolean(bool option)
    {
        return AssertValue(ReadBoolean(), "Boolean", "{0}", new bool[] { option });
    }

    #endregion

    #region SByte

    /// <summary>
    /// Reads a one-byte signed integer.
    /// </summary>
    public sbyte ReadSByte()
    {
        return br.ReadSByte();
    }

    /// <summary>
    /// Reads an array of one-byte signed integers.
    /// </summary>
    public sbyte[] ReadSBytes(int count)
    {
        var result = new sbyte[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadSByte();
        return result;
    }

    /// <summary>
    /// Reads a one-byte signed integer from the specified offset without advancing the stream.
    /// </summary>
    public sbyte GetSByte(long offset)
    {
        return GetValue(ReadSByte, offset);
    }

    /// <summary>
    /// Reads an array of one-byte signed integers from the specified offset without advancing the stream.
    /// </summary>
    public sbyte[] GetSBytes(long offset, int count)
    {
        return GetValues(ReadSBytes, offset, count);
    }

    /// <summary>
    /// Reads a one-byte signed integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public sbyte AssertSByte(params sbyte[] options)
    {
        return AssertValue(ReadSByte(), "SByte", "0x{0:X}", options);
    }

    #endregion

    #region Byte

    /// <summary>
    /// Reads a one-byte unsigned integer.
    /// </summary>
    public byte ReadByte()
    {
        return br.ReadByte();
    }

    /// <summary>
    /// Reads an array of one-byte unsigned integers.
    /// </summary>
    public byte[] ReadBytes(int count)
    {
        byte[] result = br.ReadBytes(count);
        if (result.Length != count)
            throw new EndOfStreamException("Remaining size of stream was smaller than requested number of bytes.");
        return result;
    }

    /// <summary>
    /// Reads the specified number of bytes from the stream into the buffer starting at the specified index.
    /// </summary>
    public void ReadBytes(byte[] buffer, int index, int count)
    {
        int read = br.Read(buffer, index, count);
        if (read != count)
            throw new EndOfStreamException("Remaining size of stream was smaller than requested number of bytes.");
    }

    /// <summary>
    /// Reads a one-byte unsigned integer from the specified offset without advancing the stream.
    /// </summary>
    public byte GetByte(long offset)
    {
        return GetValue(ReadByte, offset);
    }

    /// <summary>
    /// Reads an array of one-byte unsigned integers from the specified offset without advancing the stream.
    /// </summary>
    public byte[] GetBytes(long offset, int count)
    {
        StepIn(offset);
        byte[] result = ReadBytes(count);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads the specified number of bytes from the offset into the buffer starting at the specified index without advancing
    /// the stream.
    /// </summary>
    public void GetBytes(long offset, byte[] buffer, int index, int count)
    {
        StepIn(offset);
        ReadBytes(buffer, index, count);
        StepOut();
    }

    /// <summary>
    /// Reads a one-byte unsigned integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public byte AssertByte(params byte[] options)
    {
        return AssertValue(ReadByte(), "Byte", "0x{0:X}", options);
    }

    #endregion

    #region Int16

    /// <summary>
    /// Reads a two-byte signed integer.
    /// </summary>
    public short ReadInt16(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToInt16(ReadReversedBytes(2), 0);
        else
            return br.ReadInt16();
    }

    /// <summary>
    /// Reads an array of two-byte signed integers.
    /// </summary>
    public short[] ReadInt16s(int count, bool? bigEndian = null)
    {
        var result = new short[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadInt16(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads a two-byte signed integer from the specified offset without advancing the stream.
    /// </summary>
    public short GetInt16(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadInt16, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of two-byte signed integers from the specified offset without advancing the stream.
    /// </summary>
    public short[] GetInt16s(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadInt16s, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads a two-byte signed integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public short AssertInt16(params short[] options)
    {
        return AssertValue(ReadInt16(), "Int16", "0x{0:X}", options);
    }

    /// <summary>
    /// Reads a two-byte signed integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public short AssertInt16(bool? bigEndian, params short[] options)
    {
        return AssertValue(ReadInt16(bigEndian), "Int16", "0x{0:X}", options);
    }

    #endregion

    #region UInt16

    /// <summary>
    /// Reads a two-byte unsigned integer.
    /// </summary>
    public ushort ReadUInt16(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToUInt16(ReadReversedBytes(2), 0);
        else
            return br.ReadUInt16();
    }

    /// <summary>
    /// Reads an array of two-byte unsigned integers.
    /// </summary>
    public ushort[] ReadUInt16s(int count, bool? bigEndian = null)
    {
        var result = new ushort[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadUInt16(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads a two-byte unsigned integer from the specified position without advancing the stream.
    /// </summary>
    public ushort GetUInt16(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadUInt16, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of two-byte unsigned integers from the specified position without advancing the stream.
    /// </summary>
    public ushort[] GetUInt16s(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadUInt16s, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads a two-byte unsigned integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public ushort AssertUInt16(params ushort[] options)
    {
        return AssertValue(ReadUInt16(), "UInt16", "0x{0:X}", options);
    }

    /// <summary>
    /// Reads a two-byte unsigned integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public ushort AssertUInt16(bool? bigEndian, params ushort[] options)
    {
        return AssertValue(ReadUInt16(bigEndian), "UInt16", "0x{0:X}", options);
    }

    #endregion

    #region Int32

    /// <summary>
    /// Reads a four-byte signed integer.
    /// </summary>
    public int ReadInt32(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToInt32(ReadReversedBytes(4), 0);
        else
            return br.ReadInt32();
    }

    /// <summary>
    /// Reads an array of four-byte signed integers.
    /// </summary>
    public int[] ReadInt32s(int count, bool? bigEndian = null)
    {
        int[] result = new int[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadInt32(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads a four-byte signed integer from the specified position without advancing the stream.
    /// </summary>
    public int GetInt32(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadInt32, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of four-byte signed integers from the specified position without advancing the stream.
    /// </summary>
    public int[] GetInt32s(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadInt32s, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads a four-byte signed integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public int AssertInt32(params int[] options)
    {
        return AssertValue(ReadInt32(), "Int32", "0x{0:X}", options);
    }

    /// <summary>
    /// Reads a four-byte signed integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public int AssertInt32(bool? bigEndian, params int[] options)
    {
        return AssertValue(ReadInt32(bigEndian), "Int32", "0x{0:X}", options);
    }

    #endregion

    #region UInt32

    /// <summary>
    /// Reads a four-byte unsigned integer.
    /// </summary>
    public uint ReadUInt32(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToUInt32(ReadReversedBytes(4), 0);
        else
            return br.ReadUInt32();
    }

    /// <summary>
    /// Reads an array of four-byte unsigned integers.
    /// </summary>
    public uint[] ReadUInt32s(int count, bool? bigEndian = null)
    {
        var result = new uint[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadUInt32(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads a four-byte unsigned integer from the specified position without advancing the stream.
    /// </summary>
    public uint GetUInt32(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadUInt32, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of four-byte unsigned integers from the specified position without advancing the stream.
    /// </summary>
    public uint[] GetUInt32s(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadUInt32s, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads a four-byte unsigned integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public uint AssertUInt32(params uint[] options)
    {
        return AssertValue(ReadUInt32(), "UInt32", "0x{0:X}", options);
    }

    /// <summary>
    /// Reads a four-byte unsigned integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public uint AssertUInt32(bool? bigEndian, params uint[] options)
    {
        return AssertValue(ReadUInt32(bigEndian), "UInt32", "0x{0:X}", options);
    }

    #endregion

    #region Int64

    /// <summary>
    /// Reads an eight-byte signed integer.
    /// </summary>
    public long ReadInt64(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToInt64(ReadReversedBytes(8), 0);
        else
            return br.ReadInt64();
    }

    /// <summary>
    /// Reads an array of eight-byte signed integers.
    /// </summary>
    public long[] ReadInt64s(int count, bool? bigEndian = null)
    {
        var result = new long[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadInt64(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads an eight-byte signed integer from the specified position without advancing the stream.
    /// </summary>
    public long GetInt64(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadInt64, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array eight-byte signed integers from the specified position without advancing the stream.
    /// </summary>
    public long[] GetInt64s(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadInt64s, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads an eight-byte signed integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public long AssertInt64(params long[] options)
    {
        return AssertValue(ReadInt64(), "Int64", "0x{0:X}", options);
    }

    /// <summary>
    /// Reads an eight-byte signed integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public long AssertInt64(bool? bigEndian, params long[] options)
    {
        return AssertValue(ReadInt64(bigEndian), "Int64", "0x{0:X}", options);
    }

    #endregion

    #region UInt64

    /// <summary>
    /// Reads an eight-byte unsigned integer.
    /// </summary>
    public ulong ReadUInt64(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToUInt64(ReadReversedBytes(8), 0);
        else
            return br.ReadUInt64();
    }

    /// <summary>
    /// Reads an array of eight-byte unsigned integers.
    /// </summary>
    public ulong[] ReadUInt64s(int count, bool? bigEndian = null)
    {
        var result = new ulong[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadUInt64(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads an eight-byte unsigned integer from the specified position without advancing the stream.
    /// </summary>
    public ulong GetUInt64(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadUInt64, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of eight-byte unsigned integers from the specified position without advancing the stream.
    /// </summary>
    public ulong[] GetUInt64s(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadUInt64s, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads an eight-byte unsigned integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public ulong AssertUInt64(params ulong[] options)
    {
        return AssertValue(ReadUInt64(), "UInt64", "0x{0:X}", options);
    }

    /// <summary>
    /// Reads an eight-byte unsigned integer and throws an exception if it does not match any of the specified options.
    /// </summary>
    public ulong AssertUInt64(bool? bigEndian, params ulong[] options)
    {
        return AssertValue(ReadUInt64(bigEndian), "UInt64", "0x{0:X}", options);
    }

    #endregion

    #region Varint

    /// <summary>
    /// Reads either a four or eight-byte signed integer depending on VarintLong.
    /// </summary>
    public long ReadVarint(bool? bigEndian = null)
    {
        if (VarintLong)
            return ReadInt64(bigEndian);
        else
            return ReadInt32(bigEndian);
    }

    /// <summary>
    /// Reads an array of either four or eight-byte signed integers depending on VarintLong.
    /// </summary>
    public long[] ReadVarints(int count, bool? bigEndian = null)
    {
        long[] result = new long[count];
        for (int i = 0; i < count; i++)
        {
            if (VarintLong)
                result[i] = ReadInt64(bigEndian);
            else
                result[i] = ReadInt32(bigEndian);
        }

        return result;
    }

    /// <summary>
    /// Reads either a four or eight-byte signed integer depending on VarintLong from the specified position without advancing
    /// the stream.
    /// </summary>
    public long GetVarint(long offset, bool? bigEndian = null)
    {
        if (VarintLong)
            return GetInt64(offset, bigEndian);
        else
            return GetInt32(offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of either four or eight-byte signed integers depending on VarintLong from the specified position without
    /// advancing the stream.
    /// </summary>
    public long[] GetVarints(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadVarints, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads either a four or eight-byte signed integer depending on VarintLong and throws an exception if it does not match
    /// any of the specified options.
    /// </summary>
    public long AssertVarint(bool? bigEndian = null, params long[] options)
    {
        return AssertValue(ReadVarint(bigEndian), VarintLong ? "Varint64" : "Varint32", "0x{0:X}", options);
    }

    #endregion

    #region Single

    /// <summary>
    /// Reads a four-byte floating point number.
    /// </summary>
    public float ReadSingle(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToSingle(ReadReversedBytes(4), 0);
        else
            return br.ReadSingle();
    }

    /// <summary>
    /// Reads an array of four-byte floating point numbers.
    /// </summary>
    public float[] ReadSingles(int count, bool? bigEndian = null)
    {
        var result = new float[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadSingle(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads a four-byte floating point number from the specified position without advancing the stream.
    /// </summary>
    public float GetSingle(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadSingle, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of four-byte floating point numbers from the specified position without advancing the stream.
    /// </summary>
    public float[] GetSingles(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadSingles, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads a four-byte floating point number and throws an exception if it does not match any of the specified options.
    /// </summary>
    public float AssertSingle(bool? bigEndian = null, params float[] options)
    {
        return AssertValue(ReadSingle(bigEndian), "Single", "{0}", options);
    }

    #endregion

    #region Double

    /// <summary>
    /// Reads an eight-byte floating point number.
    /// </summary>
    public double ReadDouble(bool? bigEndian = null)
    {
        if (bigEndian ?? BigEndian)
            return BitConverter.ToDouble(ReadReversedBytes(8), 0);
        else
            return br.ReadDouble();
    }

    /// <summary>
    /// Reads an array of eight-byte floating point numbers.
    /// </summary>
    public double[] ReadDoubles(int count, bool? bigEndian = null)
    {
        var result = new double[count];
        for (int i = 0; i < count; i++)
            result[i] = ReadDouble(bigEndian);
        return result;
    }

    /// <summary>
    /// Reads an eight-byte floating point number from the specified position without advancing the stream.
    /// </summary>
    public double GetDouble(long offset, bool? bigEndian = null)
    {
        return GetValue(ReadDouble, offset, bigEndian);
    }

    /// <summary>
    /// Reads an array of eight-byte floating point numbers from the specified position without advancing the stream.
    /// </summary>
    public double[] GetDoubles(long offset, int count, bool? bigEndian = null)
    {
        return GetValues(ReadDoubles, offset, count, bigEndian);
    }

    /// <summary>
    /// Reads an eight-byte floating point number and throws an exception if it does not match any of the specified options.
    /// </summary>
    public double AssertDouble(bool? bigEndian = null, params double[] options)
    {
        return AssertValue(ReadDouble(bigEndian), "Double", "{0}", options);
    }

    #endregion

    #region Enum

    private TEnum ReadEnum<TEnum, TValue>(Func<TValue> readValue, string valueFormat)
        where TValue : notnull
    {
        TValue value = readValue();
        if (!Enum.IsDefined(typeof(TEnum), value))
        {
            string strValue = string.Format(valueFormat, value);
            throw new InvalidDataException(string.Format(
                "Read Byte not present in enum: {0}", strValue));
        }

        return (TEnum)(object)value;
    }

    private TEnum ReadEnum<TEnum, TValue>(Func<bool?, TValue> readValue, string valueFormat, bool? bigEndian)
        where TValue : notnull
    {
        TValue value = readValue(bigEndian);
        if (!Enum.IsDefined(typeof(TEnum), value))
        {
            string strValue = string.Format(valueFormat, value);
            throw new InvalidDataException(string.Format(
                "Read Byte not present in enum: {0}", strValue));
        }

        return (TEnum)(object)value;
    }

    /// <summary>
    /// Reads a one-byte value as the specified enum, throwing an exception if not present.
    /// </summary>
    public TEnum ReadEnum8<TEnum>() where TEnum : Enum
    {
        return ReadEnum<TEnum, byte>(ReadByte, "0x{0:X}");
    }


    /// <summary>
    /// Reads a one-byte enum from the specified position without advancing the stream.
    /// </summary>
    public TEnum GetEnum8<TEnum>(long position) where TEnum : Enum
    {
        StepIn(position);
        TEnum result = ReadEnum8<TEnum>();
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads a two-byte value as the specified enum, throwing an exception if not present.
    /// </summary>
    public TEnum ReadEnum16<TEnum>(bool? bigEndian = null) where TEnum : Enum
    {
        return ReadEnum<TEnum, ushort>(ReadUInt16, "0x{0:X}", bigEndian);
    }

    /// <summary>
    /// Reads a two-byte enum from the specified position without advancing the stream.
    /// </summary>
    public TEnum GetEnum16<TEnum>(long position, bool? bigEndian = null) where TEnum : Enum
    {
        StepIn(position);
        TEnum result = ReadEnum16<TEnum>(bigEndian);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads a four-byte value as the specified enum, throwing an exception if not present.
    /// </summary>
    public TEnum ReadEnum32<TEnum>(bool? bigEndian = null) where TEnum : Enum
    {
        return ReadEnum<TEnum, uint>(ReadUInt32, "0x{0:X}", bigEndian);
    }

    /// <summary>
    /// Reads a four-byte enum from the specified position without advancing the stream.
    /// </summary>
    public TEnum GetEnum32<TEnum>(long position, bool? bigEndian = null) where TEnum : Enum
    {
        StepIn(position);
        TEnum result = ReadEnum32<TEnum>(bigEndian);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads an eight-byte value as the specified enum, throwing an exception if not present.
    /// </summary>
    public TEnum ReadEnum64<TEnum>(bool? bigEndian = null) where TEnum : Enum
    {
        return ReadEnum<TEnum, ulong>(ReadUInt64, "0x{0:X}", bigEndian);
    }

    /// <summary>
    /// Reads an eight-byte enum from the specified position without advancing the stream.
    /// </summary>
    public TEnum GetEnum64<TEnum>(long position, bool? bigEndian = null) where TEnum : Enum
    {
        StepIn(position);
        TEnum result = ReadEnum64<TEnum>(bigEndian);
        StepOut();
        return result;
    }

    #endregion

    #region String

    /// <summary>
    /// Reads the specified number of bytes and interprets them according to the specified encoding.
    /// </summary>
    private string ReadChars(Encoding encoding, int length)
    {
        byte[] bytes = ReadBytes(length);
        return encoding.GetString(bytes);
    }

    /// <summary>
    /// Reads bytes until a single-byte null terminator is found, then interprets them according to the specified encoding.
    /// </summary>
    private string ReadCharsTerminated(Encoding encoding)
    {
        var bytes = new List<byte>();

        byte b = ReadByte();
        while (b != 0)
        {
            bytes.Add(b);
            b = ReadByte();
        }

        return encoding.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Reads a null-terminated ASCII string.
    /// </summary>
    public string ReadASCII()
    {
        return ReadCharsTerminated(Encoding.ASCII);
    }

    /// <summary>
    /// Reads an ASCII string with the specified length in bytes.
    /// </summary>
    public string ReadASCII(int length)
    {
        return ReadChars(Encoding.ASCII, length);
    }

    /// <summary>
    /// Reads a null-terminated ASCII string from the specified position without advancing the stream.
    /// </summary>
    public string GetASCII(long offset)
    {
        StepIn(offset);
        string result = ReadASCII();
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads an ASCII string with the specified length in bytes from the specified position without advancing the stream.
    /// </summary>
    public string GetASCII(long offset, int length)
    {
        StepIn(offset);
        string result = ReadASCII(length);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads as many ASCII characters as are in the specified value and throws an exception if they do not match.
    /// </summary>
    public string AssertASCII(params string[] values)
    {
        string s = ReadASCII(values[0].Length);
        bool valid = false;
        foreach (string value in values)
            if (s == value)
                valid = true;

        if (!valid)
            throw new InvalidDataException(string.Format(
                "Read ASCII: {0} | Expected ASCII: {1}", s, string.Join(", ", values)));

        return s;
    }

    /// <summary>
    /// Reads a null-terminated UTF-16 string.
    /// </summary>
    public string ReadUTF16(bool? bigEndian = null)
    {
        List<byte> bytes = new List<byte>();
        byte[] pair = ReadBytes(2);
        while (pair[0] != 0 || pair[1] != 0)
        {
            bytes.Add(pair[0]);
            bytes.Add(pair[1]);
            pair = ReadBytes(2);
        }

        if (bigEndian ?? BigEndian)
            return Encoding.Unicode.GetString(bytes.ToArray());
        else
            return Encoding.BigEndianUnicode.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Reads a null-terminated UTF-16 string from the specified position without advancing the stream.
    /// </summary>
    public string GetUTF16(long offset, bool? bigEndian = null)
    {
        StepIn(offset);
        string result = ReadUTF16(bigEndian);
        StepOut();
        return result;
    }

    /// <summary>
    /// Reads a null-terminated UTF-16 string in a fixed-size field.
    /// </summary>
    public string ReadFixStrW(int size, bool? bigEndian = null)
    {
        byte[] bytes = ReadBytes(size);
        int terminator;
        for (terminator = 0; terminator < size; terminator += 2)
        {
            // If length is odd (which it really shouldn't be), avoid indexing out of the array and align the terminator to the end
            if (terminator == size - 1)
                terminator--;
            else if (bytes[terminator] == 0 && bytes[terminator + 1] == 0)
                break;
        }

        if (bigEndian ?? BigEndian)
            return Encoding.Unicode.GetString(bytes, 0, terminator);
        else
            return Encoding.BigEndianUnicode.GetString(bytes, 0, terminator);
    }

    #endregion

    #region Other

    /// <summary>
    /// Reads a vector of two four-byte floating point numbers.
    /// </summary>
    public Vector2 ReadVector2(bool? bigEndian = null)
    {
        float x = ReadSingle(bigEndian);
        float y = ReadSingle(bigEndian);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Reads a vector of three four-byte floating point numbers.
    /// </summary>
    public Vector3 ReadVector3(bool? bigEndian = null)
    {
        float x = ReadSingle(bigEndian);
        float y = ReadSingle(bigEndian);
        float z = ReadSingle(bigEndian);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Reads a vector of four four-byte floating point numbers.
    /// </summary>
    public Vector4 ReadVector4(bool? bigEndian = null)
    {
        float x = ReadSingle(bigEndian);
        float y = ReadSingle(bigEndian);
        float z = ReadSingle(bigEndian);
        float w = ReadSingle(bigEndian);
        return new Vector4(x, y, z, w);
    }

    /// <summary>
    /// Read length number of bytes and assert that they all match the given value.
    /// </summary>
    public void AssertPattern(int length, byte pattern)
    {
        byte[] bytes = ReadBytes(length);
        for (int i = 0; i < length; i++)
        {
            if (bytes[i] != pattern)
                throw new InvalidDataException($"Expected {length} 0x{pattern:X2}, got {bytes[i]:X2} at position {i}");
        }
    }

    /// <summary>
    /// Reads a 4-byte color in ARGB order.
    /// </summary>
    public Color ReadARGB()
    {
        byte a = br.ReadByte();
        byte r = br.ReadByte();
        byte g = br.ReadByte();
        byte b = br.ReadByte();
        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    /// Reads a 4-byte color in ABGR order.
    /// </summary>
    public Color ReadABGR()
    {
        byte a = br.ReadByte();
        byte b = br.ReadByte();
        byte g = br.ReadByte();
        byte r = br.ReadByte();
        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    /// Reads a 4-byte color in RGBA order.
    /// </summary>
    public Color ReadRGBA()
    {
        byte r = br.ReadByte();
        byte g = br.ReadByte();
        byte b = br.ReadByte();
        byte a = br.ReadByte();
        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    /// Reads a 4-byte color in BGRA order.
    /// </summary>
    public Color ReadBGRA()
    {
        byte b = br.ReadByte();
        byte g = br.ReadByte();
        byte r = br.ReadByte();
        byte a = br.ReadByte();
        return Color.FromArgb(a, r, g, b);
    }

    #endregion
}