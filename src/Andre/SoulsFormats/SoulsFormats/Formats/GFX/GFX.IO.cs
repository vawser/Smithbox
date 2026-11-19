using System;
using System.IO;
using System.Text;

namespace SoulsFormats
{
    public partial class GFX
    {
        /// <summary>
        /// Little-endian SWF reader over a tag body: byte fields and MSB-first bit fields. A byte read after bit reads
        /// starts at the next whole byte, as SWF requires.
        /// </summary>
        internal class SwfReader
        {
            private readonly byte[] Data;
            private readonly int End;
            private int Pos;
            private int Bit; // bits used of Data[Pos] while reading bit fields; 0 = byte aligned

            public SwfReader(byte[] data, int start, int end)
            {
                Data = data;
                Pos = start;
                End = end;
            }

            public SwfReader(byte[] data) : this(data, 0, data.Length) { }

            public int Position { get { Align(); return Pos; } }
            public int Remaining => End - Position;

            public void Align()
            {
                if (Bit != 0)
                {
                    Pos++;
                    Bit = 0;
                }
            }

            private void Need(int n)
            {
                if (Pos + n > End)
                    throw new EndOfStreamException("SWF field past the end of the tag.");
            }

            public byte U8() { Align(); Need(1); return Data[Pos++]; }
            public ushort U16() { Align(); Need(2); var v = BitConverter.ToUInt16(Data, Pos); Pos += 2; return v; }
            public short S16() { Align(); Need(2); var v = BitConverter.ToInt16(Data, Pos); Pos += 2; return v; }
            public uint U32() { Align(); Need(4); var v = BitConverter.ToUInt32(Data, Pos); Pos += 4; return v; }

            public byte[] Bytes(int n)
            {
                Align();
                Need(n);
                var b = new byte[n];
                Buffer.BlockCopy(Data, Pos, b, 0, n);
                Pos += n;
                return b;
            }

            public byte[] Rest() => Bytes(Remaining);

            /// <summary>Null-terminated UTF-8 string.</summary>
            public string Str()
            {
                Align();
                int start = Pos;
                while (true)
                {
                    Need(1);
                    if (Data[Pos] == 0)
                        break;
                    Pos++;
                }
                var s = Encoding.UTF8.GetString(Data, start, Pos - start);
                Pos++;
                return s;
            }

            /// <summary>Length-prefixed (u8) UTF-8 string, as the GFx extension tags use.</summary>
            public string PStr()
            {
                int n = U8();
                return Encoding.UTF8.GetString(Bytes(n));
            }

            public uint UB(int n)
            {
                uint v = 0;
                for (int i = 0; i < n; i++)
                {
                    Need(1);
                    v = v << 1 | (uint)(Data[Pos] >> (7 - Bit) & 1);
                    if (++Bit == 8)
                    {
                        Bit = 0;
                        Pos++;
                    }
                }
                return v;
            }

            public int SB(int n)
            {
                if (n == 0)
                    return 0;
                uint v = UB(n);
                return (int)(v << (32 - n)) >> (32 - n);
            }
        }

        /// <summary>
        /// Little-endian SWF writer: byte fields and MSB-first bit fields, aligned to a byte before each byte field.
        /// </summary>
        internal class SwfWriter
        {
            private readonly MemoryStream Stream = new();
            private int Bit; // bits used of the last byte; 0 = aligned
            private byte Current;

            public void Align()
            {
                if (Bit != 0)
                {
                    Stream.WriteByte(Current);
                    Current = 0;
                    Bit = 0;
                }
            }

            public void U8(byte v) { Align(); Stream.WriteByte(v); }
            public void U16(ushort v) { Align(); Stream.Write(BitConverter.GetBytes(v)); }
            public void S16(short v) { Align(); Stream.Write(BitConverter.GetBytes(v)); }
            public void U32(uint v) { Align(); Stream.Write(BitConverter.GetBytes(v)); }
            public void Bytes(byte[] b) { Align(); Stream.Write(b); }

            public void Str(string s)
            {
                Align();
                Stream.Write(Encoding.UTF8.GetBytes(s));
                Stream.WriteByte(0);
            }

            public void PStr(string s)
            {
                var b = Encoding.UTF8.GetBytes(s);
                if (b.Length > 255)
                    throw new InvalidDataException($"GFx string longer than 255 bytes: {s}");
                U8((byte)b.Length);
                Bytes(b);
            }

            public void UB(int n, uint v)
            {
                for (int i = n - 1; i >= 0; i--)
                {
                    if ((v >> i & 1) != 0)
                        Current |= (byte)(0x80 >> Bit);
                    if (++Bit == 8)
                    {
                        Stream.WriteByte(Current);
                        Current = 0;
                        Bit = 0;
                    }
                }
            }

            public void SB(int n, int v)
            {
                if (n > 0 && (v < -(1L << (n - 1)) || v >= 1L << (n - 1)))
                    throw new InvalidDataException($"{v} does not fit in {n} signed bits.");
                UB(n, (uint)v);
            }

            public byte[] ToArray()
            {
                Align();
                return Stream.ToArray();
            }
        }

        /// <summary>
        /// The fewest bits a signed bit field needs to hold all the values (0 when they are all zero).
        /// </summary>
        internal static int SignedBits(params int[] values)
        {
            int n = 0;
            foreach (var v in values)
            {
                if (v == 0)
                    continue;
                int a = v < 0 ? ~v : v;
                int k = 1;
                while (a != 0)
                {
                    k++;
                    a >>= 1;
                }
                n = Math.Max(n, k);
            }
            return n;
        }
    }
}
