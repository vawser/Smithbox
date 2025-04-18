using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;

namespace SoulsFormats
{
    /// <summary>
    /// An extended writer for binary data supporting big and little endianness, value reservation, and arrays.
    /// </summary>
    public class BinaryWriterEx
    {
        private BinaryWriter bw;
        private Stack<long> steps;
        private Dictionary<string, long> reservations;

        /// <summary>
        /// Interpret values as big-endian if set, or little-endian if not.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Varints are written as Int64 if set, otherwise Int32.
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
        /// Initializes a new <c>BinaryWriterEx</c> writing to an empty <c>MemoryStream</c>
        /// </summary>
        public BinaryWriterEx(bool bigEndian) : this(bigEndian, new MemoryStream()) { }

        /// <summary>
        /// Initializes a new <c>BinaryWriterEx</c> writing to the specified stream.
        /// </summary>
        public BinaryWriterEx(bool bigEndian, Stream stream)
        {
            BigEndian = bigEndian;
            steps = new Stack<long>();
            reservations = new Dictionary<string, long>();
            Stream = stream;
            bw = new BinaryWriter(stream);
        }

        private void WriteReversedBytes(byte[] bytes)
        {
            Array.Reverse(bytes);
            bw.Write(bytes);
        }

        private void Reserve(string name, string typeName, int length)
        {
            name = $"{name}:{typeName}";
            if (reservations.ContainsKey(name))
                throw new ArgumentException("Key already reserved: " + name);

            reservations[name] = Stream.Position;
            for (int i = 0; i < length; i++)
                WriteByte(0xFE);
        }

        private long Fill(string name, string typeName)
        {
            name = $"{name}:{typeName}";
            if (!reservations.TryGetValue(name, out long jump))
                throw new ArgumentException("Key is not reserved: " + name);

            reservations.Remove(name);
            return jump;
        }

        /// <summary>
        /// Verify that all reservations are filled and close the stream.
        /// </summary>
        public void Finish()
        {
            if (reservations.Count > 0)
            {
                throw new InvalidOperationException("Not all reservations filled: " + string.Join(", ", reservations.Keys));
            }
            bw.Close();
        }

        /// <summary>
        /// Verify that all reservations are filled, close the stream, and return the written data as an array of bytes.
        /// </summary>
        public byte[] FinishBytes()
        {
            MemoryStream ms = (MemoryStream)Stream;
            byte[] result = ms.ToArray();
            Finish();
            return result;
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
                throw new InvalidOperationException("Writer is already stepped all the way out.");

            Stream.Position = steps.Pop();
        }

        /// <summary>
        /// Writes 0x00 bytes until the stream position meets the specified alignment.
        /// </summary>
        public void Pad(int align)
        {
            while (Stream.Position % align > 0)
                WriteByte(0);
        }

        /// <summary>
        /// Writes 0x00 bytes until the stream position meets the specified alignment relative to the given starting position.
        /// </summary>
        public void PadRelative(long start, int align)
        {
            while ((Stream.Position - start) % align > 0)
                WriteByte(0);
        }

        #region Boolean
        /// <summary>
        /// Writes a one-byte boolean value.
        /// </summary>
        public void WriteBoolean(bool value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// Writes an array of one-byte boolean values.
        /// </summary>
        public void WriteBooleans(IList<bool> values)
        {
            foreach (bool value in values)
                WriteBoolean(value);
        }

        /// <summary>
        /// Reserves the current position and advance the stream by one byte.
        /// </summary>
        public void ReserveBoolean(string name)
        {
            Reserve(name, "Boolean", 1);
        }

        /// <summary>
        /// Writes a one-byte boolean value to a reserved position.
        /// </summary>
        public void FillBoolean(string name, bool value)
        {
            StepIn(Fill(name, "Boolean"));
            WriteBoolean(value);
            StepOut();
        }
        #endregion

        #region SByte
        /// <summary>
        /// Writes a one-byte signed integer.
        /// </summary>
        public void WriteSByte(sbyte value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// Writes an array of one-byte signed integers.
        /// </summary>
        public void WriteSBytes(IList<sbyte> values)
        {
            foreach (sbyte value in values)
                WriteSByte(value);
        }

        /// <summary>
        /// Reserves the current position and advance the stream by one byte.
        /// </summary>
        public void ReserveSByte(string name)
        {
            Reserve(name, "SByte", 1);
        }

        /// <summary>
        /// Writes a one-byte signed integer to a reserved position.
        /// </summary>
        public void FillSByte(string name, sbyte value)
        {
            StepIn(Fill(name, "SByte"));
            WriteSByte(value);
            StepOut();
        }
        #endregion

        #region Byte
        /// <summary>
        /// Writes a one-byte unsigned integer.
        /// </summary>
        public void WriteByte(byte value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// Writes an array of one-byte unsigned integers.
        /// </summary>
        public void WriteBytes(byte[] bytes)
        {
            bw.Write(bytes);
        }
        
        /// <summary>
        /// Writes an array of one-byte unsigned integers.
        /// </summary>
        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            bw.Write(bytes);
        }

        /// <summary>
        /// Writes an array of one-byte unsigned integers.
        /// </summary>
        public void WriteBytes(IList<byte> values)
        {
            foreach (byte value in values)
                WriteByte(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by one byte.
        /// </summary>
        public void ReserveByte(string name)
        {
            Reserve(name, "Byte", 1);
        }

        /// <summary>
        /// Writes a one-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillByte(string name, byte value)
        {
            StepIn(Fill(name, "Byte"));
            WriteByte(value);
            StepOut();
        }
        #endregion

        #region Int16
        /// <summary>
        /// Writes a two-byte signed integer.
        /// </summary>
        public void WriteInt16(short value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of two-byte signed integers.
        /// </summary>
        public void WriteInt16s(IList<short> values)
        {
            foreach (short value in values)
                WriteInt16(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by two bytes.
        /// </summary>
        public void ReserveInt16(string name)
        {
            Reserve(name, "Int16", 2);
        }

        /// <summary>
        /// Writes a two-byte signed integer to a reserved position.
        /// </summary>
        public void FillInt16(string name, short value)
        {
            StepIn(Fill(name, "Int16"));
            WriteInt16(value);
            StepOut();
        }
        #endregion

        #region UInt16
        /// <summary>
        /// Writes a two-byte unsigned integer.
        /// </summary>
        public void WriteUInt16(ushort value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of two-byte unsigned integers.
        /// </summary>
        public void WriteUInt16s(IList<ushort> values)
        {
            foreach (ushort value in values)
                WriteUInt16(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by two bytes.
        /// </summary>
        public void ReserveUInt16(string name)
        {
            Reserve(name, "UInt16", 2);
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillUInt16(string name, ushort value)
        {
            StepIn(Fill(name, "UInt16"));
            WriteUInt16(value);
            StepOut();
        }
        #endregion

        #region Int32
        /// <summary>
        /// Writes a four-byte signed integer.
        /// </summary>
        public void WriteInt32(int value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of four-byte signed integers.
        /// </summary>
        public void WriteInt32s(IList<int> values)
        {
            foreach (int value in values)
                WriteInt32(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by four bytes.
        /// </summary>
        public void ReserveInt32(string name)
        {
            Reserve(name, "Int32", 4);
        }

        /// <summary>
        /// Writes a four-byte signed integer to a reserved position.
        /// </summary>
        public void FillInt32(string name, int value)
        {
            StepIn(Fill(name, "Int32"));
            WriteInt32(value);
            StepOut();
        }
        #endregion

        #region UInt32
        /// <summary>
        /// Writes a four-byte unsigned integer.
        /// </summary>
        public void WriteUInt32(uint value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of four-byte unsigned integers.
        /// </summary>
        public void WriteUInt32s(IList<uint> values)
        {
            foreach (uint value in values)
                WriteUInt32(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by four bytes.
        /// </summary>
        public void ReserveUInt32(string name)
        {
            Reserve(name, "UInt32", 4);
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillUInt32(string name, uint value)
        {
            StepIn(Fill(name, "UInt32"));
            WriteUInt32(value);
            StepOut();
        }
        #endregion

        #region Int64
        /// <summary>
        /// Writes an eight-byte signed integer.
        /// </summary>
        public void WriteInt64(long value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of eight-byte signed integers.
        /// </summary>
        public void WriteInt64s(IList<long> values)
        {
            foreach (long value in values)
                WriteInt64(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by eight bytes.
        /// </summary>
        public void ReserveInt64(string name)
        {
            Reserve(name, "Int64", 8);
        }

        /// <summary>
        /// Writes an eight-byte signed integer to a reserved position.
        /// </summary>
        public void FillInt64(string name, long value)
        {
            StepIn(Fill(name, "Int64"));
            WriteInt64(value);
            StepOut();
        }
        #endregion

        #region UInt64
        /// <summary>
        /// Writes an eight-byte unsigned integer.
        /// </summary>
        public void WriteUInt64(ulong value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of eight-byte unsigned integers.
        /// </summary>
        public void WriteUInt64s(IList<ulong> values)
        {
            foreach (ulong value in values)
                WriteUInt64(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by eight bytes.
        /// </summary>
        public void ReserveUInt64(string name)
        {
            Reserve(name, "UInt64", 8);
        }

        /// <summary>
        /// Writes an eight-byte unsigned integer to a reserved position.
        /// </summary>
        public void FillUInt64(string name, ulong value)
        {
            StepIn(Fill(name, "UInt64"));
            WriteUInt64(value);
            StepOut();
        }
        #endregion

        #region Varint
        /// <summary>
        /// Writes either a four or eight-byte signed integer depending on VarintLong.
        /// </summary>
        public void WriteVarint(long value)
        {
            if (VarintLong)
                WriteInt64(value);
            else
                WriteInt32((int)value);
        }

        /// <summary>
        /// Writes an array of either four or eight-byte signed integers depending on VarintLong.
        /// </summary>
        public void WriteVarints(IList<long> values)
        {
            foreach (long value in values)
            {
                if (VarintLong)
                    WriteInt64(value);
                else
                    WriteInt32((int)value);
            }
        }

        /// <summary>
        /// Reserves the current position and advances the stream by either four or eight bytes depending on VarintLong.
        /// </summary>
        public void ReserveVarint(string name)
        {
            if (VarintLong)
                Reserve(name, "Varint64", 8);
            else
                Reserve(name, "Varint32", 4);
        }

        /// <summary>
        /// Writes either a four or eight-byte signed integer depending on VarintLong to a reserved position.
        /// </summary>
        public void FillVarint(string name, long value)
        {
            if (VarintLong)
            {
                StepIn(Fill(name, "Varint64"));
                WriteInt64(value);
                StepOut();
            }
            else
            {
                StepIn(Fill(name, "Varint32"));
                WriteInt32((int)value);
                StepOut();
            }
        }
        #endregion

        #region Single
        /// <summary>
        /// Writes a four-byte floating point number.
        /// </summary>
        public void WriteSingle(float value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of four-byte floating point numbers.
        /// </summary>
        public void WriteSingles(IList<float> values)
        {
            foreach (float value in values)
                WriteSingle(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by four bytes.
        /// </summary>
        public void ReserveSingle(string name)
        {
            Reserve(name, "Single", 4);
        }

        /// <summary>
        /// Writes a four-byte floating point number to a reserved position.
        /// </summary>
        public void FillSingle(string name, float value)
        {
            StepIn(Fill(name, "Single"));
            WriteSingle(value);
            StepOut();
        }
        #endregion

        #region Double
        /// <summary>
        /// Writes an eight-byte floating point number.
        /// </summary>
        public void WriteDouble(double value)
        {
            if (BigEndian)
                WriteReversedBytes(BitConverter.GetBytes(value));
            else
                bw.Write(value);
        }

        /// <summary>
        /// Writes an array of eight-byte floating point numbers.
        /// </summary>
        public void WriteDoubles(IList<double> values)
        {
            foreach (double value in values)
                WriteDouble(value);
        }

        /// <summary>
        /// Reserves the current position and advances the stream by eight bytes.
        /// </summary>
        public void ReserveDouble(string name)
        {
            Reserve(name, "Double", 8);
        }

        /// <summary>
        /// Writes a eight-byte floating point number to a reserved position.
        /// </summary>
        public void FillDouble(string name, double value)
        {
            StepIn(Fill(name, "Double"));
            WriteDouble(value);
            StepOut();
        }
        #endregion

        #region String
        private void WriteChars(string text, Encoding encoding, bool terminate)
        {
            if (terminate)
                text += '\0';
            byte[] bytes = encoding.GetBytes(text);
            bw.Write(bytes);
        }

        /// <summary>
        /// Writes an ASCII string, with null terminator if specified.
        /// </summary>
        public void WriteASCII(string text, bool terminate = false)
        {
            WriteChars(text, SFEncoding.ASCII, terminate);
        }

        /// <summary>
        /// Writes a Shift JIS string, with null terminator if specified.
        /// </summary>
        public void WriteShiftJIS(string text, bool terminate = false)
        {
            WriteChars(text, SFEncoding.ShiftJIS, terminate);
        }

        /// <summary>
        /// Writes a UTF-16 string, with null terminator if specified.
        /// </summary>
        public void WriteUTF16(string text, bool terminate = false)
        {
            if (BigEndian)
                WriteChars(text, SFEncoding.UTF16BE, terminate);
            else
                WriteChars(text, SFEncoding.UTF16, terminate);
        }

        /// <summary>
        /// Writes a null-terminated Shift JIS string in a fixed-size field.
        /// </summary>
        public void WriteFixStr(string text, int size, byte padding = 0)
        {
            byte[] fixstr = new byte[size];
            for (int i = 0; i < size; i++)
                fixstr[i] = padding;

            byte[] bytes = SFEncoding.ShiftJIS.GetBytes(text + '\0');
            Array.Copy(bytes, fixstr, Math.Min(size, bytes.Length));
            bw.Write(fixstr);
        }

        /// <summary>
        /// Writes a null-terminated UTF-16 string in a fixed-size field.
        /// </summary>
        public void WriteFixStrW(string text, int size, byte padding = 0)
        {
            byte[] fixstr = new byte[size];
            for (int i = 0; i < size; i++)
                fixstr[i] = padding;

            byte[] bytes;
            if (BigEndian)
                bytes = SFEncoding.UTF16BE.GetBytes(text + '\0');
            else
                bytes = SFEncoding.UTF16.GetBytes(text + '\0');
            Array.Copy(bytes, fixstr, Math.Min(size, bytes.Length));
            bw.Write(fixstr);
        }
        #endregion

        #region Other
        /// <summary>
        /// Writes a vector of two four-byte floating point numbers.
        /// </summary>
        public void WriteVector2(Vector2 vector)
        {
            WriteSingle(vector.X);
            WriteSingle(vector.Y);
        }

        /// <summary>
        /// Writes a vector of three four-byte floating point numbers.
        /// </summary>
        public void WriteVector3(Vector3 vector)
        {
            WriteSingle(vector.X);
            WriteSingle(vector.Y);
            WriteSingle(vector.Z);
        }

        /// <summary>
        /// Writes a vector of four four-byte floating point numbers.
        /// </summary>
        public void WriteVector4(Vector4 vector)
        {
            WriteSingle(vector.X);
            WriteSingle(vector.Y);
            WriteSingle(vector.Z);
            WriteSingle(vector.W);
        }

        /// <summary>
        /// Write length number of the given value.
        /// </summary>
        public void WritePattern(int length, byte pattern)
        {
            byte[] bytes = new byte[length];
            if (pattern != 0)
            {
                for (int i = 0; i < length; i++)
                    bytes[i] = pattern;
            }
            WriteBytes(bytes);
        }

        /// <summary>
        /// Writes a 4-byte color in ARGB order.
        /// </summary>
        public void WriteARGB(Color color)
        {
            bw.Write(color.A);
            bw.Write(color.R);
            bw.Write(color.G);
            bw.Write(color.B);
        }

        /// <summary>
        /// Writes a 4-byte color in ABGR order.
        /// </summary>
        public void WriteABGR(Color color)
        {
            bw.Write(color.A);
            bw.Write(color.B);
            bw.Write(color.G);
            bw.Write(color.R);
        }

        /// <summary>
        /// Writes a 4-byte color in RGBA order.
        /// </summary>
        public void WriteRGBA(Color color)
        {
            bw.Write(color.R);
            bw.Write(color.G);
            bw.Write(color.B);
            bw.Write(color.A);
        }

        /// <summary>
        /// Writes a 4-byte color in BGRA order.
        /// </summary>
        public void WriteBGRA(Color color)
        {
            bw.Write(color.B);
            bw.Write(color.G);
            bw.Write(color.R);
            bw.Write(color.A);
        }
        #endregion
    }
}
