using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuaDecompilerCore.Utilities
{
    /// <summary>
    /// An extended reader for binary data supporting big and little endianness, value assertions, and arrays.
    /// </summary>
    public class BinaryReaderEx
    {
        private static readonly Encoding ASCII = Encoding.ASCII;
        // Smithbox: upstream used Encoding.UTF8 here, which turns the Japanese strings in FromSoftware's Lua 5.0 files
        // into U+FFFD (and distinct strings into equal ones, shifting constant indices on recompile).
        private static readonly Encoding ShiftJIS = GetShiftJis();

        private static Encoding GetShiftJis()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding("shift_jis");
        }
        private static readonly Encoding UTF8 = Encoding.UTF8;
        private static readonly Encoding UTF16 = Encoding.Unicode;
        private static readonly Encoding UTF16BE = Encoding.BigEndianUnicode;

        private BinaryReader br;
        private Stack<long> steps;

        /// <summary>
        /// Interpret values as big-endian if set, or little-endian if not.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// The underlying stream.
        /// </summary>
        public Stream Stream { get; private set; }

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
        /// Initializes a new BinaryReaderEx reading from the specified byte array.
        /// </summary>
        public BinaryReaderEx(bool bigEndian, byte[] input) : this(bigEndian, new MemoryStream(input)) { }

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
        private T AssertValue<T>(T value, string typeName, string valueFormat, T[] options) where T : IEquatable<T>
        {
            foreach (T option in options)
                if (value.Equals(option))
                    return value;

            string strValue = string.Format(valueFormat, value);
            string strOptions = string.Join(", ", options.Select(o => string.Format(valueFormat, o)));
            throw new InvalidDataException($"Read {typeName}: {strValue} | Expected: {strOptions} | Ending position: 0x{Position:X}");
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
        /// Advances the stream position by count bytes.
        /// </summary>
        public void Skip(int count)
        {
            Stream.Position += count;
        }

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
        public short ReadInt16()
        {
            if (BigEndian)
                return BitConverter.ToInt16(ReadReversedBytes(2), 0);
            else
                return br.ReadInt16();
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
        public short AssertInt16(params short[] options)
        {
            return AssertValue(ReadInt16(), "Int16", "0x{0:X}", options);
        }
        #endregion

        #region UInt16
        /// <summary>
        /// Reads a two-byte unsigned integer.
        /// </summary>
        public ushort ReadUInt16()
        {
            if (BigEndian)
                return BitConverter.ToUInt16(ReadReversedBytes(2), 0);
            else
                return br.ReadUInt16();
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
        public ushort AssertUInt16(params ushort[] options)
        {
            return AssertValue(ReadUInt16(), "UInt16", "0x{0:X}", options);
        }
        #endregion

        #region Int32
        /// <summary>
        /// Reads a four-byte signed integer.
        /// </summary>
        public int ReadInt32()
        {
            if (BigEndian)
                return BitConverter.ToInt32(ReadReversedBytes(4), 0);
            else
                return br.ReadInt32();
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
        public int AssertInt32(params int[] options)
        {
            return AssertValue(ReadInt32(), "Int32", "0x{0:X}", options);
        }
        #endregion

        #region UInt32
        /// <summary>
        /// Reads a four-byte unsigned integer.
        /// </summary>
        public uint ReadUInt32()
        {
            if (BigEndian)
                return BitConverter.ToUInt32(ReadReversedBytes(4), 0);
            else
                return br.ReadUInt32();
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
        public uint AssertUInt32(params uint[] options)
        {
            return AssertValue(ReadUInt32(), "UInt32", "0x{0:X}", options);
        }
        #endregion

        #region Int64
        /// <summary>
        /// Reads an eight-byte signed integer.
        /// </summary>
        public long ReadInt64()
        {
            if (BigEndian)
                return BitConverter.ToInt64(ReadReversedBytes(8), 0);
            else
                return br.ReadInt64();
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
        public long AssertInt64(params long[] options)
        {
            return AssertValue(ReadInt64(), "Int64", "0x{0:X}", options);
        }
        #endregion

        #region UInt64
        /// <summary>
        /// Reads an eight-byte unsigned integer.
        /// </summary>
        public ulong ReadUInt64()
        {
            if (BigEndian)
                return BitConverter.ToUInt64(ReadReversedBytes(8), 0);
            else
                return br.ReadUInt64();
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
        public ulong AssertUInt64(params ulong[] options)
        {
            return AssertValue(ReadUInt64(), "UInt64", "0x{0:X}", options);
        }
        #endregion

        #region Single
        /// <summary>
        /// Reads a four-byte floating point number.
        /// </summary>
        public float ReadSingle()
        {
            if (BigEndian)
                return BitConverter.ToSingle(ReadReversedBytes(4), 0);
            else
                return br.ReadSingle();
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
        public float AssertSingle(params float[] options)
        {
            return AssertValue(ReadSingle(), "Single", "{0}", options);
        }
        #endregion

        #region Double
        /// <summary>
        /// Reads an eight-byte floating point number.
        /// </summary>
        public double ReadDouble()
        {
            if (BigEndian)
                return BitConverter.ToDouble(ReadReversedBytes(8), 0);
            else
                return br.ReadDouble();
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
        public double AssertDouble(params double[] options)
        {
            return AssertValue(ReadDouble(), "Double", "{0}", options);
        }
        #endregion

        #region Enum
        private TEnum ReadEnum<TEnum, TValue>(Func<TValue> readValue, string valueFormat)
        {
            TValue value = readValue();
            if (!Enum.IsDefined(typeof(TEnum), value ?? throw new Exception()))
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
        public TEnum ReadEnum16<TEnum>() where TEnum : Enum
        {
            return ReadEnum<TEnum, ushort>(ReadUInt16, "0x{0:X}");
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
            return ReadEnum<TEnum, uint>(ReadUInt32, "0x{0:X}");
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
            return ReadEnum<TEnum, ulong>(ReadUInt64, "0x{0:X}");
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
            return ReadCharsTerminated(ASCII);
        }

        /// <summary>
        /// Reads an ASCII string with the specified length in bytes.
        /// </summary>
        public string ReadASCII(int length)
        {
            return ReadChars(ASCII, length);
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
        /// Reads a null-terminated Shift JIS string.
        /// </summary>
        public string ReadShiftJIS()
        {
            return ReadCharsTerminated(ShiftJIS);
        }

        /// <summary>
        /// Reads a Shift JIS string with the specified length in bytes.
        /// </summary>
        public string ReadShiftJIS(int length)
        {
            return ReadChars(ShiftJIS, length);
        }

        /// <summary>
        /// Reads a Shift JIS string with the specified length in bytes.
        /// </summary>
        public string ReadUTF8(int length)
        {
            return ReadChars(UTF8, length);
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
            List<byte> bytes = new List<byte>();
            byte[] pair = ReadBytes(2);
            while (pair[0] != 0 || pair[1] != 0)
            {
                bytes.Add(pair[0]);
                bytes.Add(pair[1]);
                pair = ReadBytes(2);
            }

            if (BigEndian)
                return UTF16BE.GetString(bytes.ToArray());
            else
                return UTF16.GetString(bytes.ToArray());
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
            return ShiftJIS.GetString(bytes, 0, terminator);
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
                return UTF16BE.GetString(bytes, 0, terminator);
            else
                return UTF16.GetString(bytes, 0, terminator);
        }
        #endregion
    }
}
