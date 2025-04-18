using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using DotNext.Buffers;
using CommunityToolkit.HighPerformance;

namespace SoulsFormats
{
    /// <summary>
    /// An extended reader for binary data supporting big and little endianness, value assertions, and arrays.
    /// </summary>
    public sealed class BinaryReaderEx
    {
        public static bool IgnoreAsserts { get; set; }

        private Stack<long> _steps;
        public Memory<byte> _memory;

        /// <summary>
        /// Interpret values as big-endian if set, or little-endian if not.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Varints are read as Int64 if set, otherwise Int32.
        /// </summary>
        public bool VarintLong { get; set; }

        /// <summary>
        /// Current size of varints in bytes.
        /// </summary>
        public int VarintSize => VarintLong ? 8 : 4;

        /// <summary>
        /// The current position of the stream.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// The length of the stream.
        /// </summary>
        public long Length => _memory.Length;

        public BinaryReaderEx(bool bigEndian, Memory<byte> memory)
        {
            BigEndian = bigEndian;
            _steps = new Stack<long>();
            _memory = memory;
        }
        
        public unsafe T Read<T>() where T : unmanaged
        {
            var reader = new SpanReader<byte>(_memory.Span[(int)Position..]);
            var ret = reader.Read<T>();
            Position += sizeof(T);
            return ret;
        }

        public unsafe T[] ReadMulti<T>(int count) where T : unmanaged
        {
            var ret = _memory.Span.Slice((int)Position, sizeof(T) * count).Cast<byte, T>().ToArray();
            Position += sizeof(T) * count;
            return ret;
        }

        public unsafe Span<T> ReadSpanView<T>(int count) where T : unmanaged
        {
            var ret = _memory.Span.Slice((int)Position, sizeof(T) * count).Cast<byte, T>();
            Position += sizeof(T) * count;
            return ret;
        }

        public Memory<byte> ReadByteMemoryView(int size)
        {
            var ret = _memory.Slice((int)Position, size);
            Position += size;
            return ret;
        }

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
        /// Reads a value from the specified offset using the given function, returning the stream to its original position afterwards.
        /// </summary>
        private T GetValue<T>(Func<T> readValue, long offset)
        {
            StepIn(offset);
            T result = readValue();
            StepOut();
            return result;
        }

        /// <summary>
        /// Reads an array of values from the specified offset using the given function, returning the stream to its original position afterwards.
        /// </summary>
        private T[] GetValues<T>(Func<int, T[]> readValues, long offset, int count)
        {
            StepIn(offset);
            T[] result = readValues(count);
            StepOut();
            return result;
        }

        /// <summary>
        /// Compares a value to a list of options, returning it if found or excepting if not.
        /// </summary>
        private T AssertValue<T>(T value, string typeName, string valueFormat, T option) where T : IEquatable<T>
        {
            if (IgnoreAsserts) return value;

            if (value.Equals(option))
                return value;

            string strValue = string.Format(valueFormat, value);
            string strOptions = string.Join(", ", string.Format(valueFormat, option));
            throw new InvalidDataException($"Read {typeName}: {strValue} | Expected: {strOptions} | Ending position: 0x{Position:X}");
        }
        
        /// <summary>
        /// Compares a value to a list of options, returning it if found or excepting if not.
        /// </summary>
        private T AssertValue<T>(T value, string typeName, string valueFormat, ReadOnlySpan<T> options) where T : IEquatable<T>
        {
            foreach (T option in options)
                if (value.Equals(option))
                    return value;

            string strValue = string.Format(valueFormat, value);
            string strOptions = string.Join(", ", options.ToArray().Select(o => string.Format(valueFormat, o)));
            throw new InvalidDataException($"Read {typeName}: {strValue} | Expected: {strOptions} | Ending position: 0x{Position:X}");
        }

        /// <summary>
        /// Store the current position of the stream on a stack, then move to the specified offset.
        /// </summary>
        public void StepIn(long offset)
        {
            _steps.Push(Position);
            Position = offset;
        }

        /// <summary>
        /// Restore the previous position of the stream from a stack.
        /// </summary>
        public void StepOut()
        {
            if (_steps.Count == 0)
                throw new InvalidOperationException("Reader is already stepped all the way out.");

            Position = _steps.Pop();
        }

        /// <summary>
        /// Advances the stream position until it meets the specified alignment.
        /// </summary>
        public void Pad(int align)
        {
            if (Position % align > 0)
                Position += align - (Position % align);
        }

        /// <summary>
        /// Advances the stream position until it meets the specified alignment relative to the given starting position.
        /// </summary>
        public void PadRelative(long start, int align)
        {
            long relPos = Position - start;
            if (relPos % align > 0)
                Position += align - (relPos % align);
        }

        /// <summary>
        /// Advances the stream position by count bytes.
        /// </summary>
        public void Skip(int count)
        {
            Position += count;
        }

        #region Boolean
        /// <summary>
        /// Reads a one-byte boolean value.
        /// </summary>
        public bool ReadBoolean()
        {
            // BinaryReader.ReadBoolean accepts any non-zero value as true, which I don't want.
            byte b = ReadByte();
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
            return AssertValue(ReadBoolean(), "Boolean", "{0}", [option]);
        }
        #endregion

        #region SByte
        /// <summary>
        /// Reads a one-byte signed integer.
        /// </summary>
        public sbyte ReadSByte()
        {
            return Read<sbyte>();
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
        public sbyte AssertSByte(sbyte option)
        {
            return AssertValue(ReadSByte(), "SByte", "0x{0:X}", option);
        }
        
        /// <summary>
        /// Reads a one-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public sbyte AssertSByte(ReadOnlySpan<sbyte> options)
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
            return Read<byte>();
        }

        /// <summary>
        /// Reads an array of one-byte unsigned integers.
        /// </summary>
        public byte[] ReadBytes(int count)
        {
            return ReadMulti<byte>(count);
        }

        /// <summary>
        /// Reads the specified number of bytes from the stream into the buffer starting at the specified index.
        /// </summary>
        public void ReadBytes(byte[] buffer, int index, int count)
        {
            _memory.Span.Slice((int)Position, count).CopyTo(new Span<byte>(buffer, index, count));
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
        /// Reads the specified number of bytes from the offset into the buffer starting at the specified index without advancing the stream.
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
        public byte AssertByte(byte option)
        {
            return AssertValue(ReadByte(), "Byte", "0x{0:X}", option);
        }
        
        /// <summary>
        /// Reads a one-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public byte AssertByte(ReadOnlySpan<byte> options)
        {
            return AssertValue(ReadByte(), "Byte", "0x{0:X}", options);
        }
        #endregion

        #region Int16
        /// <summary>
        /// Reads a two-byte signed integer.
        /// </summary>
        public unsafe short ReadInt16()
        {
            if (BigEndian)
            {
                short i = Read<short>();
                return BinaryPrimitives.ReadInt16BigEndian(new ReadOnlySpan<byte>((byte*)&i, 2));
            }

            return Read<short>();
        }

        /// <summary>
        /// Reads an array of two-byte signed integers.
        /// </summary>
        public short[] ReadInt16s(int count)
        {
            var result = new short[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt16();
            return result;
        }

        /// <summary>
        /// Reads a two-byte signed integer from the specified offset without advancing the stream.
        /// </summary>
        public short GetInt16(long offset)
        {
            return GetValue(ReadInt16, offset);
        }

        /// <summary>
        /// Reads an array of two-byte signed integers from the specified offset without advancing the stream.
        /// </summary>
        public short[] GetInt16s(long offset, int count)
        {
            return GetValues(ReadInt16s, offset, count);
        }

        /// <summary>
        /// Reads a two-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public short AssertInt16(short option)
        {
            return AssertValue(ReadInt16(), "Int16", "0x{0:X}", option);
        }
        
        /// <summary>
        /// Reads a two-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public short AssertInt16(ReadOnlySpan<short> options)
        {
            return AssertValue(ReadInt16(), "Int16", "0x{0:X}", options);
        }
        #endregion

        #region UInt16
        /// <summary>
        /// Reads a two-byte unsigned integer.
        /// </summary>
        public unsafe ushort ReadUInt16()
        {
            if (BigEndian)
            {
                ushort i = Read<ushort>();
                return BinaryPrimitives.ReadUInt16BigEndian(new ReadOnlySpan<byte>((byte*)&i, 2));
            }
            return Read<ushort>();
        }

        /// <summary>
        /// Reads an array of two-byte unsigned integers.
        /// </summary>
        public ushort[] ReadUInt16s(int count)
        {
            var result = new ushort[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadUInt16();
            return result;
        }

        /// <summary>
        /// Reads a two-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public ushort GetUInt16(long offset)
        {
            return GetValue(ReadUInt16, offset);
        }

        /// <summary>
        /// Reads an array of two-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public ushort[] GetUInt16s(long offset, int count)
        {
            return GetValues(ReadUInt16s, offset, count);
        }

        /// <summary>
        /// Reads a two-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ushort AssertUInt16(ushort option)
        {
            return AssertValue(ReadUInt16(), "UInt16", "0x{0:X}", option);
        }
        
        /// <summary>
        /// Reads a two-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ushort AssertUInt16(ReadOnlySpan<ushort> options)
        {
            return AssertValue(ReadUInt16(), "UInt16", "0x{0:X}", options);
        }
        #endregion

        #region Int32
        /// <summary>
        /// Reads a four-byte signed integer.
        /// </summary>
        public unsafe int ReadInt32()
        {
            if (BigEndian)
            {
                int i = Read<int>();
                return BinaryPrimitives.ReadInt32BigEndian(new ReadOnlySpan<byte>((byte*)&i, 4));
            }
            return Read<int>();
        }

        /// <summary>
        /// Reads an array of four-byte signed integers.
        /// </summary>
        public int[] ReadInt32s(int count)
        {
            int[] result = new int[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt32();
            return result;
        }

        /// <summary>
        /// Reads a four-byte signed integer from the specified position without advancing the stream.
        /// </summary>
        public int GetInt32(long offset)
        {
            return GetValue(ReadInt32, offset);
        }

        /// <summary>
        /// Reads an array of four-byte signed integers from the specified position without advancing the stream.
        /// </summary>
        public int[] GetInt32s(long offset, int count)
        {
            return GetValues(ReadInt32s, offset, count);
        }

        /// <summary>
        /// Reads a four-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public int AssertInt32(int option)
        {
            return AssertValue(ReadInt32(), "Int32", "0x{0:X}", option);
        }
        
        /// <summary>
        /// Reads a four-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public int AssertInt32(ReadOnlySpan<int> options)
        {
            return AssertValue(ReadInt32(), "Int32", "0x{0:X}", options);
        }
        #endregion

        #region UInt32
        /// <summary>
        /// Reads a four-byte unsigned integer.
        /// </summary>
        public unsafe uint ReadUInt32()
        {
            if (BigEndian)
            {
                uint i = Read<uint>();
                return BinaryPrimitives.ReadUInt32BigEndian(new ReadOnlySpan<byte>((byte*)&i, 4));
            }
            return Read<uint>();
        }

        /// <summary>
        /// Reads an array of four-byte unsigned integers.
        /// </summary>
        public uint[] ReadUInt32s(int count)
        {
            var result = new uint[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadUInt32();
            return result;
        }

        /// <summary>
        /// Reads a four-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public uint GetUInt32(long offset)
        {
            return GetValue(ReadUInt32, offset);
        }

        /// <summary>
        /// Reads an array of four-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public uint[] GetUInt32s(long offset, int count)
        {
            return GetValues(ReadUInt32s, offset, count);
        }
        
        /// <summary>
        /// Reads a four-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public uint AssertUInt32(uint option)
        {
            return AssertValue(ReadUInt32(), "UInt32", "0x{0:X}", option);
        }

        /// <summary>
        /// Reads a four-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public uint AssertUInt32(ReadOnlySpan<uint> options)
        {
            return AssertValue(ReadUInt32(), "UInt32", "0x{0:X}", options);
        }
        #endregion

        #region Int64
        /// <summary>
        /// Reads an eight-byte signed integer.
        /// </summary>
        public unsafe long ReadInt64()
        {
            if (BigEndian)
            {
                long i = Read<long>();
                return BinaryPrimitives.ReadInt64BigEndian(new ReadOnlySpan<byte>((byte*)&i, 8));
            }
            return Read<long>();
        }

        /// <summary>
        /// Reads an array of eight-byte signed integers.
        /// </summary>
        public long[] ReadInt64s(int count)
        {
            var result = new long[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadInt64();
            return result;
        }

        /// <summary>
        /// Reads an eight-byte signed integer from the specified position without advancing the stream.
        /// </summary>
        public long GetInt64(long offset)
        {
            return GetValue(ReadInt64, offset);
        }

        /// <summary>
        /// Reads an array eight-byte signed integers from the specified position without advancing the stream.
        /// </summary>
        public long[] GetInt64s(long offset, int count)
        {
            return GetValues(ReadInt64s, offset, count);
        }
        
        /// <summary>
        /// Reads an eight-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public long AssertInt64(long option)
        {
            return AssertValue(ReadInt64(), "Int64", "0x{0:X}", option);
        }

        /// <summary>
        /// Reads an eight-byte signed integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public long AssertInt64(ReadOnlySpan<long> options)
        {
            return AssertValue(ReadInt64(), "Int64", "0x{0:X}", options);
        }
        #endregion

        #region UInt64
        /// <summary>
        /// Reads an eight-byte unsigned integer.
        /// </summary>
        public unsafe ulong ReadUInt64()
        {
            if (BigEndian)
            {
                ulong i = Read<ulong>();
                return BinaryPrimitives.ReadUInt64BigEndian(new ReadOnlySpan<byte>((byte*)&i, 8));
            }
            return Read<ulong>();
        }

        /// <summary>
        /// Reads an array of eight-byte unsigned integers.
        /// </summary>
        public ulong[] ReadUInt64s(int count)
        {
            var result = new ulong[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadUInt64();
            return result;
        }

        /// <summary>
        /// Reads an eight-byte unsigned integer from the specified position without advancing the stream.
        /// </summary>
        public ulong GetUInt64(long offset)
        {
            return GetValue(ReadUInt64, offset);
        }

        /// <summary>
        /// Reads an array of eight-byte unsigned integers from the specified position without advancing the stream.
        /// </summary>
        public ulong[] GetUInt64s(long offset, int count)
        {
            return GetValues(ReadUInt64s, offset, count);
        }

        /// <summary>
        /// Reads an eight-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ulong AssertUInt64(ulong option)
        {
            return AssertValue(ReadUInt64(), "UInt64", "0x{0:X}", option);
        }
        
        /// <summary>
        /// Reads an eight-byte unsigned integer and throws an exception if it does not match any of the specified options.
        /// </summary>
        public ulong AssertUInt64(ReadOnlySpan<ulong> options)
        {
            return AssertValue(ReadUInt64(), "UInt64", "0x{0:X}", options);
        }
        #endregion

        #region Varint
        /// <summary>
        /// Reads either a four or eight-byte signed integer depending on VarintLong.
        /// </summary>
        public long ReadVarint()
        {
            if (VarintLong)
                return ReadInt64();
            else
                return ReadInt32();
        }

        /// <summary>
        /// Reads an array of either four or eight-byte signed integers depending on VarintLong.
        /// </summary>
        public long[] ReadVarints(int count)
        {
            long[] result = new long[count];
            for (int i = 0; i < count; i++)
            {
                if (VarintLong)
                    result[i] = ReadInt64();
                else
                    result[i] = ReadInt32();
            }
            return result;
        }

        /// <summary>
        /// Reads either a four or eight-byte signed integer depending on VarintLong from the specified position without advancing the stream.
        /// </summary>
        public long GetVarint(long offset)
        {
            if (VarintLong)
                return GetInt64(offset);
            else
                return GetInt32(offset);
        }

        /// <summary>
        /// Reads an array of either four or eight-byte signed integers depending on VarintLong from the specified position without advancing the stream.
        /// </summary>
        public long[] GetVarints(long offset, int count)
        {
            return GetValues(ReadVarints, offset, count);
        }

        /// <summary>
        /// Reads either a four or eight-byte signed integer depending on VarintLong and throws an exception if it does not match any of the specified options.
        /// </summary>
        public long AssertVarint(params long[] options)
        {
            return AssertValue(ReadVarint(), VarintLong ? "Varint64" : "Varint32", "0x{0:X}", options);
        }
        
        /// <summary>
        /// Reads either a four or eight-byte signed integer depending on VarintLong and throws an exception if it does not match any of the specified options.
        /// </summary>
        public long AssertVarint(ReadOnlySpan<long> options)
        {
            return AssertValue(ReadVarint(), VarintLong ? "Varint64" : "Varint32", "0x{0:X}", options);
        }
        #endregion

        #region Single
        /// <summary>
        /// Reads a four-byte floating point number.
        /// </summary>
        public unsafe float ReadSingle()
        {
            if (BigEndian)
            {
                var i = Read<uint>();
                return BinaryPrimitives.ReadSingleBigEndian(new ReadOnlySpan<byte>((byte*)&i, 4));
            }
            return Read<float>();
        }

        /// <summary>
        /// Reads an array of four-byte floating point numbers.
        /// </summary>
        public float[] ReadSingles(int count)
        {
            var result = new float[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadSingle();
            return result;
        }

        /// <summary>
        /// Reads a four-byte floating point number from the specified position without advancing the stream.
        /// </summary>
        public float GetSingle(long offset)
        {
            return GetValue(ReadSingle, offset);
        }

        /// <summary>
        /// Reads an array of four-byte floating point numbers from the specified position without advancing the stream.
        /// </summary>
        public float[] GetSingles(long offset, int count)
        {
            return GetValues(ReadSingles, offset, count);
        }
        
        /// <summary>
        /// Reads a four-byte floating point number and throws an exception if it does not match any of the specified options.
        /// </summary>
        public float AssertSingle(float option)
        {
            return AssertValue(ReadSingle(), "Single", "{0}", option);
        }

        /// <summary>
        /// Reads a four-byte floating point number and throws an exception if it does not match any of the specified options.
        /// </summary>
        public float AssertSingle(ReadOnlySpan<float> options)
        {
            return AssertValue(ReadSingle(), "Single", "{0}", options);
        }
        #endregion

        #region Double
        /// <summary>
        /// Reads an eight-byte floating point number.
        /// </summary>
        public unsafe double ReadDouble()
        {
            if (BigEndian)
            {
                ulong i = Read<ulong>();
                return BinaryPrimitives.ReadDoubleBigEndian(new ReadOnlySpan<byte>((byte*)&i, 8));
            }
            return Read<double>();
        }

        /// <summary>
        /// Reads an array of eight-byte floating point numbers.
        /// </summary>
        public double[] ReadDoubles(int count)
        {
            var result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadDouble();
            return result;
        }

        /// <summary>
        /// Reads an eight-byte floating point number from the specified position without advancing the stream.
        /// </summary>
        public double GetDouble(long offset)
        {
            return GetValue(ReadDouble, offset);
        }

        /// <summary>
        /// Reads an array of eight-byte floating point numbers from the specified position without advancing the stream.
        /// </summary>
        public double[] GetDoubles(long offset, int count)
        {
            return GetValues(ReadDoubles, offset, count);
        }
        
        /// <summary>
        /// Reads an eight-byte floating point number and throws an exception if it does not match any of the specified options.
        /// </summary>
        public double AssertDouble(double option)
        {
            return AssertValue(ReadDouble(), "Double", "{0}", option);
        }

        /// <summary>
        /// Reads an eight-byte floating point number and throws an exception if it does not match any of the specified options.
        /// </summary>
        public double AssertDouble(ReadOnlySpan<double> options)
        {
            return AssertValue(ReadDouble(), "Double", "{0}", options);
        }
        #endregion

        #region Enum
        private TEnum ReadEnum<TEnum, TValue>(Func<TValue> readValue, string valueFormat)
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

        /// <summary>
        /// Reads a one-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum8<TEnum>() where TEnum : Enum
        {
            Type typ = Enum.GetUnderlyingType(typeof(TEnum));
            if (typ == typeof(byte))
                return ReadEnum<TEnum, byte>(ReadByte, "0x{0:X}");
            if (typ == typeof(sbyte))
                return ReadEnum<TEnum, sbyte>(ReadSByte, "0x{0:X}");
            throw new InvalidDataException($"Enum {typeof(TEnum).Name} has an invalid underlying value type: {typ.Name}");
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
        public TEnum ReadEnum16<TEnum>() where TEnum : Enum
        {
            Type typ = Enum.GetUnderlyingType(typeof(TEnum));
            if (typ == typeof(short))
                return ReadEnum<TEnum, short>(ReadInt16, "0x{0:X}");
            if (typ == typeof(ushort))
                return ReadEnum<TEnum, ushort>(ReadUInt16, "0x{0:X}");
            throw new InvalidDataException($"Enum {typeof(TEnum).Name} has an invalid underlying value type: {typ.Name}");
        }

        /// <summary>
        /// Reads a two-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum16<TEnum>(long position) where TEnum : Enum
        {
            StepIn(position);
            TEnum result = ReadEnum16<TEnum>();
            StepOut();
            return result;
        }

        /// <summary>
        /// Reads a four-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum32<TEnum>() where TEnum : Enum
        {
            Type typ = Enum.GetUnderlyingType(typeof(TEnum));
            if (typ == typeof(int))
                return ReadEnum<TEnum, int>(ReadInt32, "0x{0:X}");
            if (typ == typeof(uint))
                return ReadEnum<TEnum, uint>(ReadUInt32, "0x{0:X}");
            throw new InvalidDataException($"Enum {typeof(TEnum).Name} has an invalid underlying value type: {typ.Name}");
        }

        /// <summary>
        /// Reads a four-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum32<TEnum>(long position) where TEnum : Enum
        {
            StepIn(position);
            TEnum result = ReadEnum32<TEnum>();
            StepOut();
            return result;
        }

        /// <summary>
        /// Reads an eight-byte value as the specified enum, throwing an exception if not present.
        /// </summary>
        public TEnum ReadEnum64<TEnum>() where TEnum : Enum
        {
            Type typ = Enum.GetUnderlyingType(typeof(TEnum));
            if (typ == typeof(long))
                return ReadEnum<TEnum, long>(ReadInt64, "0x{0:X}");
            if (typ == typeof(ulong))
                return ReadEnum<TEnum, ulong>(ReadUInt64, "0x{0:X}");
            throw new InvalidDataException($"Enum {typeof(TEnum).Name} has an invalid underlying value type: {typ.Name}");
        }

        /// <summary>
        /// Reads an eight-byte enum from the specified position without advancing the stream.
        /// </summary>
        public TEnum GetEnum64<TEnum>(long position) where TEnum : Enum
        {
            StepIn(position);
            TEnum result = ReadEnum64<TEnum>();
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
            return ReadCharsTerminated(SFEncoding.ASCII);
        }

        /// <summary>
        /// Reads an ASCII string with the specified length in bytes.
        /// </summary>
        public string ReadASCII(int length)
        {
            return ReadChars(SFEncoding.ASCII, length);
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
        public string AssertASCII(string value)
        {
            string s = ReadASCII(value.Length);

            if (s != value)
                throw new InvalidDataException(string.Format(
                    "Read ASCII: {0} | Expected ASCII: {1}", s, value));

            return s;
        }
        
        /// <summary>
        /// Reads as many ASCII characters as are in the specified value and throws an exception if they do not match.
        /// </summary>
        public string AssertASCII(ReadOnlySpan<string> values)
        {
            string s = ReadASCII(values[0].Length);
            bool valid = false;
            foreach (string value in values)
                if (s == value)
                    valid = true;

            if (!valid)
                throw new InvalidDataException(string.Format(
                    "Read ASCII: {0} | Expected ASCII: {1}", s, string.Join(", ", values.ToArray())));

            return s;
        }

        /// <summary>
        /// Reads a null-terminated Shift JIS string.
        /// </summary>
        public string ReadShiftJIS()
        {
            return ReadCharsTerminated(SFEncoding.ShiftJIS);
        }

        /// <summary>
        /// Reads a Shift JIS string with the specified length in bytes.
        /// </summary>
        public string ReadShiftJIS(int length)
        {
            return ReadChars(SFEncoding.ShiftJIS, length);
        }

        /// <summary>
        /// Reads a null-terminated Shift JIS string from the specified position without advancing the stream.
        /// </summary>
        public string GetShiftJIS(long offset)
        {
            StepIn(offset);
            string result = ReadShiftJIS();
            StepOut();
            return result;
        }

        /// <summary>
        /// Reads a Shift JIS string with the specified length in bytes from the specified position without advancing the stream.
        /// </summary>
        public string GetShiftJIS(long offset, int length)
        {
            StepIn(offset);
            string result = ReadShiftJIS(length);
            StepOut();
            return result;
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string.
        /// </summary>
        public string ReadUTF16()
        {
            List<byte> bytes = new List<byte>(64);
            byte a = ReadByte();
            byte b = ReadByte();
            while (a != 0 || b != 0)
            {
                bytes.Add(a);
                bytes.Add(b);
                a = ReadByte(); 
                b = ReadByte();
            }

            if (BigEndian)
                return SFEncoding.UTF16BE.GetString(bytes.ToArray());
            else
                return SFEncoding.UTF16.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string from the specified position without advancing the stream.
        /// </summary>
        public string GetUTF16(long offset)
        {
            StepIn(offset);
            string result = ReadUTF16();
            StepOut();
            return result;
        }

        /// <summary>
        /// Reads a null-terminated Shift JIS string in a fixed-size field.
        /// </summary>
        public string ReadFixStr(int size)
        {
            byte[] bytes = ReadBytes(size);
            int terminator;
            for (terminator = 0; terminator < size; terminator++)
            {
                if (bytes[terminator] == 0)
                    break;
            }
            return SFEncoding.ShiftJIS.GetString(bytes, 0, terminator);
        }

        /// <summary>
        /// Reads a null-terminated UTF-16 string in a fixed-size field.
        /// </summary>
        public string ReadFixStrW(int size)
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

            if (BigEndian)
                return SFEncoding.UTF16BE.GetString(bytes, 0, terminator);
            else
                return SFEncoding.UTF16.GetString(bytes, 0, terminator);
        }
        #endregion

        #region Other
        /// <summary>
        /// Reads a vector of two four-byte floating point numbers.
        /// </summary>
        public Vector2 ReadVector2()
        {
            if (!BigEndian)
                return Read<Vector2>();
            float x = ReadSingle();
            float y = ReadSingle();
            return new Vector2(x, y);
        }

        /// <summary>
        /// Reads a vector of three four-byte floating point numbers.
        /// </summary>
        public Vector3 ReadVector3()
        {
            if (!BigEndian)
                return Read<Vector3>();
            float x = ReadSingle();
            float y = ReadSingle();
            float z = ReadSingle();
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads a vector of four four-byte floating point numbers.
        /// </summary>
        public Vector4 ReadVector4()
        {
            if (!BigEndian)
                return Read<Vector4>();
            float x = ReadSingle();
            float y = ReadSingle();
            float z = ReadSingle();
            float w = ReadSingle();
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
            byte a = ReadByte();
            byte r = ReadByte();
            byte g = ReadByte();
            byte b = ReadByte();
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Reads a 4-byte color in ABGR order.
        /// </summary>
        public Color ReadABGR()
        {
            byte a = ReadByte();
            byte b = ReadByte();
            byte g = ReadByte();
            byte r = ReadByte();
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Reads a 4-byte color in RGBA order.
        /// </summary>
        public Color ReadRGBA()
        {
            byte r = ReadByte();
            byte g = ReadByte();
            byte b = ReadByte();
            byte a = ReadByte();
            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Reads a 4-byte color in BGRA order.
        /// </summary>
        public Color ReadBGRA()
        {
            byte b = ReadByte();
            byte g = ReadByte();
            byte r = ReadByte();
            byte a = ReadByte();
            return Color.FromArgb(a, r, g, b);
        }
        #endregion
    }
}
