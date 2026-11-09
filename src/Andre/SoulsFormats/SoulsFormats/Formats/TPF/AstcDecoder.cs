using System;
using System.Buffers.Binary;
using System.Threading.Tasks;

namespace SoulsFormats
{
    /// <summary>
    /// CPU decoder for 2D LDR ASTC blocks (any footprint), to R8G8B8A8. Used for Nintendo Switch 2 TPF textures, as
    /// desktop GPUs mostly do not sample ASTC. Follows the Khronos ASTC specification; HDR endpoint modes, reserved
    /// block modes and malformed blocks decode to the error colour (magenta).
    /// </summary>
    public static class AstcDecoder
    {
        private static readonly byte[] ErrorColor = { 0xFF, 0x00, 0xFF, 0xFF };

        /// <summary>
        /// The block footprint of a DXGI ASTC format (133..187), or false if the value is not one.
        /// </summary>
        public static bool TryGetBlockSize(int dxgiFormat, out int blockWidth, out int blockHeight, out bool srgb)
        {
            blockWidth = blockHeight = 0;
            srgb = false;
            if (dxgiFormat < 133 || dxgiFormat > 187)
                return false;

            int index = (dxgiFormat - 133) / 4;
            int kind = (dxgiFormat - 133) % 4; // 0 typeless, 1 unorm, 2 srgb, 3 unused
            if (kind == 3)
                return false;

            ReadOnlySpan<byte> dims = new byte[]
            {
                4, 4, 5, 4, 5, 5, 6, 5, 6, 6, 8, 5, 8, 6, 8, 8, 10, 5, 10, 6, 10, 8, 10, 10, 12, 10, 12, 12,
            };
            blockWidth = dims[index * 2];
            blockHeight = dims[index * 2 + 1];
            srgb = kind == 2;
            return true;
        }

        /// <summary>
        /// Decodes a linear (deswizzled) ASTC image of <paramref name="width"/> x <paramref name="height"/> texels
        /// into tightly packed R8G8B8A8.
        /// </summary>
        public static byte[] Decode(byte[] data, int width, int height, int blockWidth, int blockHeight, bool srgb)
        {
            int blocksX = (width + blockWidth - 1) / blockWidth;
            int blocksY = (height + blockHeight - 1) / blockHeight;
            var output = new byte[width * height * 4];

            // Rows of blocks are independent; large mips are split across threads
            if (blocksX * blocksY >= 4096)
                Parallel.For(0, blocksY, by => DecodeRow(data, output, by, width, height, blocksX, blockWidth, blockHeight, srgb));
            else
                for (int by = 0; by < blocksY; by++)
                    DecodeRow(data, output, by, width, height, blocksX, blockWidth, blockHeight, srgb);

            return output;
        }

        private static void DecodeRow(byte[] data, byte[] output, int by, int width, int height, int blocksX,
            int blockWidth, int blockHeight, bool srgb)
        {
            Span<byte> texels = stackalloc byte[blockWidth * blockHeight * 4];

            for (int bx = 0; bx < blocksX; bx++)
            {
                int offset = (by * blocksX + bx) * 16;
                if (offset + 16 > data.Length)
                    return;

                DecodeBlock(data.AsSpan(offset, 16), blockWidth, blockHeight, srgb, texels);

                for (int y = 0; y < blockHeight; y++)
                {
                    int py = by * blockHeight + y;
                    if (py >= height)
                        break;
                    int copyWidth = Math.Min(blockWidth, width - bx * blockWidth);
                    texels.Slice(y * blockWidth * 4, copyWidth * 4)
                        .CopyTo(output.AsSpan((py * width + bx * blockWidth) * 4, copyWidth * 4));
                }
            }
        }

        #region Bits

        private struct Bits
        {
            public ulong Lo, Hi;

            public Bits(ReadOnlySpan<byte> block)
            {
                Lo = BinaryPrimitives.ReadUInt64LittleEndian(block);
                Hi = BinaryPrimitives.ReadUInt64LittleEndian(block.Slice(8));
            }

            /// <summary>Up to 32 bits from <paramref name="start"/>; bits past 128 read as 0.</summary>
            public readonly uint Get(int start, int count)
            {
                if (count == 0 || start >= 128)
                    return 0;
                ulong value;
                if (start >= 64)
                    value = Hi >> (start - 64);
                else if (start == 0)
                    value = Lo;
                else
                    value = (Lo >> start) | (Hi << (64 - start));
                return (uint)(value & ((1UL << count) - 1));
            }

            public readonly Bits Reversed()
            {
                return new Bits { Lo = ReverseBits(Hi), Hi = ReverseBits(Lo) };
            }

            private static ulong ReverseBits(ulong v)
            {
                v = ((v >> 1) & 0x5555555555555555UL) | ((v & 0x5555555555555555UL) << 1);
                v = ((v >> 2) & 0x3333333333333333UL) | ((v & 0x3333333333333333UL) << 2);
                v = ((v >> 4) & 0x0F0F0F0F0F0F0F0FUL) | ((v & 0x0F0F0F0F0F0F0F0FUL) << 4);
                return BinaryPrimitives.ReverseEndianness(v);
            }
        }

        #endregion

        #region Integer sequence encoding

        /// <summary>Bits per value, and whether a trit or quint is added, for a range of <paramref name="levels"/>.</summary>
        private static bool GetEncoding(int levels, out int bits, out int trits, out int quints)
        {
            bits = trits = quints = 0;
            switch (levels)
            {
                case 2: bits = 1; return true;
                case 3: trits = 1; return true;
                case 4: bits = 2; return true;
                case 5: quints = 1; return true;
                case 6: trits = 1; bits = 1; return true;
                case 8: bits = 3; return true;
                case 10: quints = 1; bits = 1; return true;
                case 12: trits = 1; bits = 2; return true;
                case 16: bits = 4; return true;
                case 20: quints = 1; bits = 2; return true;
                case 24: trits = 1; bits = 3; return true;
                case 32: bits = 5; return true;
                case 40: quints = 1; bits = 3; return true;
                case 48: trits = 1; bits = 4; return true;
                case 64: bits = 6; return true;
                case 80: quints = 1; bits = 4; return true;
                case 96: trits = 1; bits = 5; return true;
                case 128: bits = 7; return true;
                case 160: quints = 1; bits = 5; return true;
                case 192: trits = 1; bits = 6; return true;
                case 256: bits = 8; return true;
            }
            return false;
        }

        private static int IseBitCount(int count, int levels)
        {
            GetEncoding(levels, out int bits, out int trits, out int quints);
            int total = count * bits;
            if (trits != 0)
                total += (count * 8 + 4) / 5;
            if (quints != 0)
                total += (count * 7 + 2) / 3;
            return total;
        }

        /// <summary>
        /// Decodes <paramref name="count"/> values of a range from the bits [start, start + length); bits past the end
        /// read as 0. Each output is (trit or quint) * 2^bits + low bits, with the low bits also given separately.
        /// </summary>
        private static void DecodeIse(Bits source, int start, int length, int count, int levels, Span<int> values, Span<int> tq, Span<int> low)
        {
            GetEncoding(levels, out int bits, out int trits, out int quints);
            int end = start + length;
            int pos = start;

            uint Read(int n)
            {
                if (n == 0)
                    return 0;
                uint v = 0;
                // Bits past the end of the sequence are zero
                int available = Math.Min(n, Math.Max(0, end - pos));
                if (available > 0)
                    v = source.Get(pos, available);
                pos += n;
                return v;
            }

            if (trits != 0)
            {
                Span<int> m = stackalloc int[5];
                Span<int> t = stackalloc int[5];
                for (int i = 0; i < count; i += 5)
                {
                    uint T = 0;
                    m[0] = (int)Read(bits); T |= Read(2);
                    m[1] = (int)Read(bits); T |= Read(2) << 2;
                    m[2] = (int)Read(bits); T |= Read(1) << 4;
                    m[3] = (int)Read(bits); T |= Read(2) << 5;
                    m[4] = (int)Read(bits); T |= Read(1) << 7;
                    DecodeTrits(T, t);
                    for (int j = 0; j < 5 && i + j < count; j++)
                    {
                        values[i + j] = (t[j] << bits) | m[j];
                        tq[i + j] = t[j];
                        low[i + j] = m[j];
                    }
                }
            }
            else if (quints != 0)
            {
                Span<int> m = stackalloc int[3];
                Span<int> q = stackalloc int[3];
                for (int i = 0; i < count; i += 3)
                {
                    uint Q = 0;
                    m[0] = (int)Read(bits); Q |= Read(3);
                    m[1] = (int)Read(bits); Q |= Read(2) << 3;
                    m[2] = (int)Read(bits); Q |= Read(2) << 5;
                    DecodeQuints(Q, q);
                    for (int j = 0; j < 3 && i + j < count; j++)
                    {
                        values[i + j] = (q[j] << bits) | m[j];
                        tq[i + j] = q[j];
                        low[i + j] = m[j];
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    int v = (int)Read(bits);
                    values[i] = v;
                    tq[i] = 0;
                    low[i] = v;
                }
            }
        }

        private static int Bit(uint v, int i) => (int)((v >> i) & 1);

        private static void DecodeTrits(uint T, Span<int> t)
        {
            int C;
            if (((T >> 2) & 7) == 7)
            {
                C = (int)(((T >> 5) & 7) << 2 | (T & 3));
                t[4] = 2;
                t[3] = 2;
            }
            else
            {
                C = (int)(T & 0x1F);
                if (((T >> 5) & 3) == 3)
                {
                    t[4] = 2;
                    t[3] = Bit(T, 7);
                }
                else
                {
                    t[4] = Bit(T, 7);
                    t[3] = (int)((T >> 5) & 3);
                }
            }

            uint c = (uint)C;
            if ((c & 3) == 3)
            {
                t[2] = 2;
                t[1] = Bit(c, 4);
                t[0] = (Bit(c, 3) << 1) | (Bit(c, 2) & ~Bit(c, 3) & 1);
            }
            else if (((c >> 2) & 3) == 3)
            {
                t[2] = 2;
                t[1] = 2;
                t[0] = (int)(c & 3);
            }
            else
            {
                t[2] = Bit(c, 4);
                t[1] = (int)((c >> 2) & 3);
                t[0] = (Bit(c, 1) << 1) | (Bit(c, 0) & ~Bit(c, 1) & 1);
            }
        }

        private static void DecodeQuints(uint Q, Span<int> q)
        {
            if (((Q >> 1) & 3) == 3 && ((Q >> 5) & 3) == 0)
            {
                int q0 = Bit(Q, 0);
                q[2] = (q0 << 2) | ((Bit(Q, 4) & ~q0 & 1) << 1) | (Bit(Q, 3) & ~q0 & 1);
                q[1] = 4;
                q[0] = 4;
                return;
            }

            uint C;
            if (((Q >> 1) & 3) == 3)
            {
                q[2] = 4;
                C = (((Q >> 3) & 3) << 3) | ((~(Q >> 5) & 3) << 1) | (Q & 1);
            }
            else
            {
                q[2] = (int)((Q >> 5) & 3);
                C = Q & 0x1F;
            }

            if ((C & 7) == 5)
            {
                q[1] = 4;
                q[0] = (int)((C >> 3) & 3);
            }
            else
            {
                q[1] = (int)((C >> 3) & 3);
                q[0] = (int)(C & 7);
            }
        }

        #endregion

        #region Unquantisation

        private static int UnquantizeColor(int value, int tq, int low, int levels)
        {
            GetEncoding(levels, out int bits, out int trits, out int quints);
            if (trits == 0 && quints == 0)
            {
                // Bit replication to 8 bits
                int v = value << (8 - bits);
                int result = v;
                for (int shift = bits; shift < 8; shift += bits)
                    result |= v >> shift;
                return result & 0xFF;
            }

            int A = (low & 1) != 0 ? 0x1FF : 0;
            int B = 0, C = 0;
            int D = tq;
            if (trits != 0)
            {
                switch (bits)
                {
                    case 1: C = 204; break;
                    case 2: { C = 93; int b = (low >> 1) & 1; B = (b << 8) | (b << 4) | (b << 2) | (b << 1); break; }
                    case 3: { C = 44; int cb = (low >> 1) & 3; B = (cb << 7) | (cb << 2) | cb; break; }
                    case 4: { C = 22; int dcb = (low >> 1) & 7; B = (dcb << 6) | dcb; break; }
                    case 5: { C = 11; int edcb = (low >> 1) & 0xF; B = (edcb << 5) | (edcb >> 2); break; }
                    case 6: { C = 5; int fedcb = (low >> 1) & 0x1F; B = (fedcb << 4) | (fedcb >> 4); break; }
                }
            }
            else
            {
                switch (bits)
                {
                    case 1: C = 113; break;
                    case 2: { C = 54; int b = (low >> 1) & 1; B = (b << 8) | (b << 3) | (b << 2); break; }
                    case 3: { C = 26; int cb = (low >> 1) & 3; B = (cb << 7) | (cb << 1) | (cb >> 1); break; }
                    case 4: { C = 13; int dcb = (low >> 1) & 7; B = (dcb << 6) | (dcb >> 1); break; }
                    case 5: { C = 6; int edcb = (low >> 1) & 0xF; B = (edcb << 5) | (edcb >> 3); break; }
                }
            }

            int T = D * C + B;
            T ^= A;
            return (A & 0x80) | (T >> 2);
        }

        private static int UnquantizeWeight(int value, int tq, int low, int levels)
        {
            GetEncoding(levels, out int bits, out int trits, out int quints);
            int result;
            if (trits == 0 && quints == 0)
            {
                switch (bits)
                {
                    case 1: result = value != 0 ? 63 : 0; break;
                    case 2: result = (value << 4) | (value << 2) | value; break;
                    case 3: result = (value << 3) | value; break;
                    case 4: result = (value << 2) | (value >> 2); break;
                    default: result = (value << 1) | (value >> 4); break;
                }
            }
            else if (bits == 0)
            {
                result = trits != 0
                    ? tq switch { 0 => 0, 1 => 32, _ => 63 }
                    : tq switch { 0 => 0, 1 => 16, 2 => 32, 3 => 47, _ => 63 };
            }
            else
            {
                int A = (low & 1) != 0 ? 0x7F : 0;
                int B = 0, C = 0;
                if (trits != 0)
                {
                    switch (bits)
                    {
                        case 1: C = 50; break;
                        case 2: { C = 23; int b = (low >> 1) & 1; B = (b << 6) | (b << 2) | b; break; }
                        case 3: { C = 11; int cb = (low >> 1) & 3; B = (cb << 5) | cb; break; }
                    }
                }
                else
                {
                    switch (bits)
                    {
                        case 1: C = 28; break;
                        case 2: { C = 13; int b = (low >> 1) & 1; B = (b << 6) | (b << 1); break; }
                    }
                }

                int T = tq * C + B;
                T ^= A;
                result = (A & 0x20) | (T >> 2);
            }

            if (result > 32)
                result++;
            return result;
        }

        #endregion

        #region Block decoding

        private static void FillError(Span<byte> texels)
        {
            for (int i = 0; i < texels.Length; i += 4)
                ErrorColor.CopyTo(texels.Slice(i, 4));
        }

        private static readonly int[] ColorLevels = { 256, 192, 160, 128, 96, 80, 64, 48, 40, 32, 24, 20, 16, 12, 10, 8, 6 };

        private static void DecodeBlock(ReadOnlySpan<byte> block, int bw, int bh, bool srgb, Span<byte> texels)
        {
            var bits = new Bits(block);
            uint mode = bits.Get(0, 11);

            // Void extent: one constant colour
            if ((mode & 0x1FF) == 0x1FC)
            {
                bool hdr = (mode & 0x200) != 0;
                Span<byte> c = stackalloc byte[4];
                for (int i = 0; i < 4; i++)
                {
                    uint v = bits.Get(64 + i * 16, 16);
                    c[i] = hdr ? HalfToUnorm8((ushort)v) : (byte)(v >> 8);
                }
                for (int i = 0; i < texels.Length; i += 4)
                    c.CopyTo(texels.Slice(i, 4));
                return;
            }

            if (!DecodeBlockMode(mode, out int gridW, out int gridH, out int weightLevels, out bool dualPlane))
            {
                FillError(texels);
                return;
            }

            int weightCount = gridW * gridH * (dualPlane ? 2 : 1);
            int weightBits = IseBitCount(weightCount, weightLevels);
            if (weightCount > 64 || weightBits < 24 || weightBits > 96 || gridW > bw || gridH > bh)
            {
                FillError(texels);
                return;
            }

            int partitions = (int)bits.Get(11, 2) + 1;
            if (dualPlane && partitions == 4)
            {
                FillError(texels);
                return;
            }

            Span<int> cem = stackalloc int[4];
            int colorStart;
            int extraCemBits = 0;
            int partitionSeed = 0;

            if (partitions == 1)
            {
                cem[0] = (int)bits.Get(13, 4);
                colorStart = 17;
            }
            else
            {
                partitionSeed = (int)bits.Get(13, 10);
                uint cemLow = bits.Get(23, 6);
                colorStart = 29;

                if ((cemLow & 3) == 0)
                {
                    for (int p = 0; p < partitions; p++)
                        cem[p] = (int)(cemLow >> 2);
                }
                else
                {
                    extraCemBits = 3 * partitions - 4;
                    uint cemHigh = bits.Get(128 - weightBits - extraCemBits, extraCemBits);
                    uint encoded = cemLow | (cemHigh << 6);
                    int baseClass = (int)(encoded & 3) - 1;
                    int pos = 2;
                    Span<int> classes = stackalloc int[4];
                    for (int p = 0; p < partitions; p++)
                        classes[p] = (int)((encoded >> pos++) & 1);
                    for (int p = 0; p < partitions; p++)
                    {
                        cem[p] = ((baseClass + classes[p]) << 2) | (int)((encoded >> pos) & 3);
                        pos += 2;
                    }
                }
            }

            int ccsPosition = 128 - weightBits - extraCemBits - (dualPlane ? 2 : 0);
            int ccs = dualPlane ? (int)bits.Get(ccsPosition, 2) : -1;

            int colorValueCount = 0;
            for (int p = 0; p < partitions; p++)
                colorValueCount += ((cem[p] >> 2) + 1) * 2;
            int colorBits = ccsPosition - colorStart;
            if (colorValueCount > 18 || colorBits <= 0)
            {
                FillError(texels);
                return;
            }

            int colorLevels = 0;
            foreach (int levels in ColorLevels)
            {
                if (IseBitCount(colorValueCount, levels) <= colorBits)
                {
                    colorLevels = levels;
                    break;
                }
            }
            if (colorLevels == 0)
            {
                FillError(texels);
                return;
            }

            Span<int> raw = stackalloc int[18];
            Span<int> rawTq = stackalloc int[18];
            Span<int> rawLow = stackalloc int[18];
            DecodeIse(bits, colorStart, colorBits, colorValueCount, colorLevels, raw, rawTq, rawLow);
            Span<int> colorValues = stackalloc int[18];
            for (int i = 0; i < colorValueCount; i++)
                colorValues[i] = UnquantizeColor(raw[i], rawTq[i], rawLow[i], colorLevels);

            // Endpoints per partition, 8-bit RGBA
            Span<int> endpoints = stackalloc int[4 * 8];
            int valueIndex = 0;
            for (int p = 0; p < partitions; p++)
            {
                int count = ((cem[p] >> 2) + 1) * 2;
                if (!DecodeEndpoints(cem[p], colorValues.Slice(valueIndex, count), endpoints.Slice(p * 8, 8)))
                {
                    FillError(texels);
                    return;
                }
                valueIndex += count;
            }

            // Weights are stored bit-reversed from the top of the block
            var reversed = bits.Reversed();
            Span<int> wRaw = stackalloc int[64];
            Span<int> wTq = stackalloc int[64];
            Span<int> wLow = stackalloc int[64];
            DecodeIse(reversed, 0, weightBits, weightCount, weightLevels, wRaw, wTq, wLow);
            Span<int> gridWeights = stackalloc int[64];
            for (int i = 0; i < weightCount; i++)
                gridWeights[i] = UnquantizeWeight(wRaw[i], wTq[i], wLow[i], weightLevels);

            int planes = dualPlane ? 2 : 1;
            bool smallBlock = bw * bh < 31;
            int ds = (1024 + bw / 2) / (bw - 1);
            int dt = (1024 + bh / 2) / (bh - 1);

            for (int t = 0; t < bh; t++)
            {
                for (int s = 0; s < bw; s++)
                {
                    int partition = partitions == 1 ? 0 : SelectPartition(partitionSeed, s, t, partitions, smallBlock);

                    // Bilinear infill of the weight grid onto the texel
                    int gs = (ds * s * (gridW - 1) + 32) >> 6;
                    int gt = (dt * t * (gridH - 1) + 32) >> 6;
                    int js = gs >> 4, fs = gs & 0xF;
                    int jt = gt >> 4, ft = gt & 0xF;
                    int w11 = (fs * ft + 8) >> 4;
                    int w10 = ft - w11;
                    int w01 = fs - w11;
                    int w00 = 16 - fs - ft + w11;
                    int v0 = js + jt * gridW;

                    Span<int> weight = stackalloc int[2];
                    for (int plane = 0; plane < planes; plane++)
                    {
                        int p00 = GridWeight(gridWeights, v0, plane, planes, weightCount);
                        int p01 = GridWeight(gridWeights, v0 + 1, plane, planes, weightCount);
                        int p10 = GridWeight(gridWeights, v0 + gridW, plane, planes, weightCount);
                        int p11 = GridWeight(gridWeights, v0 + gridW + 1, plane, planes, weightCount);
                        weight[plane] = (p00 * w00 + p01 * w01 + p10 * w10 + p11 * w11 + 8) >> 4;
                    }

                    int texel = (t * bw + s) * 4;
                    for (int c = 0; c < 4; c++)
                    {
                        int w = c == ccs ? weight[1] : weight[0];
                        int e0 = endpoints[partition * 8 + c];
                        int e1 = endpoints[partition * 8 + 4 + c];
                        int c0 = srgb ? (e0 << 8) | 0x80 : (e0 << 8) | e0;
                        int c1 = srgb ? (e1 << 8) | 0x80 : (e1 << 8) | e1;
                        int value = (c0 * (64 - w) + c1 * w + 32) >> 6;
                        texels[texel + c] = (byte)(value >> 8);
                    }
                }
            }
        }

        private static int GridWeight(Span<int> weights, int index, int plane, int planes, int weightCount)
        {
            int i = index * planes + plane;
            return i < weightCount ? weights[i] : 0;
        }

        private static bool DecodeBlockMode(uint mode, out int width, out int height, out int levels, out bool dualPlane)
        {
            width = height = levels = 0;
            dualPlane = false;
            int r;
            bool highPrecision;
            int a = (int)((mode >> 5) & 3);

            if ((mode & 3) != 0)
            {
                r = (int)(((mode & 3) << 1) | ((mode >> 4) & 1));
                highPrecision = ((mode >> 9) & 1) != 0;
                dualPlane = ((mode >> 10) & 1) != 0;
                switch ((mode >> 2) & 3)
                {
                    case 0: width = (int)((mode >> 7) & 3) + 4; height = a + 2; break;
                    case 1: width = (int)((mode >> 7) & 3) + 8; height = a + 2; break;
                    case 2: width = a + 2; height = (int)((mode >> 7) & 3) + 8; break;
                    default:
                        if ((mode & 0x100) == 0) { width = a + 2; height = (int)((mode >> 7) & 1) + 6; }
                        else { width = (int)((mode >> 7) & 1) + 2; height = a + 2; }
                        break;
                }
            }
            else
            {
                r = (int)(((mode >> 2) & 3) << 1 | ((mode >> 4) & 1));
                if (r == 0)
                    return false;
                highPrecision = ((mode >> 9) & 1) != 0;
                dualPlane = ((mode >> 10) & 1) != 0;
                switch ((mode >> 7) & 3)
                {
                    case 0: width = 12; height = a + 2; break;
                    case 1: width = a + 2; height = 12; break;
                    case 3:
                        if (a == 0) { width = 6; height = 10; }
                        else if (a == 1) { width = 10; height = 6; }
                        else return false;
                        break;
                    default:
                        width = a + 6;
                        height = (int)((mode >> 9) & 3) + 6;
                        highPrecision = false;
                        dualPlane = false;
                        break;
                }
            }

            if (r < 2)
                return false;
            ReadOnlySpan<int> low = new[] { 0, 0, 2, 3, 4, 5, 6, 8 };
            ReadOnlySpan<int> high = new[] { 0, 0, 10, 12, 16, 20, 24, 32 };
            levels = highPrecision ? high[r] : low[r];
            return true;
        }

        private static void BitTransferSigned(ref int a, ref int b)
        {
            b >>= 1;
            b |= a & 0x80;
            a >>= 1;
            a &= 0x3F;
            if ((a & 0x20) != 0)
                a -= 0x40;
        }

        private static int Clamp(int v) => v < 0 ? 0 : v > 255 ? 255 : v;

        private static void Set(Span<int> e, int index, int r, int g, int b, int a)
        {
            e[index * 4] = Clamp(r);
            e[index * 4 + 1] = Clamp(g);
            e[index * 4 + 2] = Clamp(b);
            e[index * 4 + 3] = Clamp(a);
        }

        private static void SetBlueContract(Span<int> e, int index, int r, int g, int b, int a)
        {
            Set(e, index, (r + b) >> 1, (g + b) >> 1, b, a);
        }

        private static bool DecodeEndpoints(int mode, Span<int> v, Span<int> e)
        {
            switch (mode)
            {
                case 0:
                    Set(e, 0, v[0], v[0], v[0], 0xFF);
                    Set(e, 1, v[1], v[1], v[1], 0xFF);
                    return true;
                case 1:
                {
                    int l0 = (v[0] >> 2) | (v[1] & 0xC0);
                    int l1 = Math.Min(l0 + (v[1] & 0x3F), 0xFF);
                    Set(e, 0, l0, l0, l0, 0xFF);
                    Set(e, 1, l1, l1, l1, 0xFF);
                    return true;
                }
                case 4:
                    Set(e, 0, v[0], v[0], v[0], v[2]);
                    Set(e, 1, v[1], v[1], v[1], v[3]);
                    return true;
                case 5:
                {
                    int v0 = v[0], v1 = v[1], v2 = v[2], v3 = v[3];
                    BitTransferSigned(ref v1, ref v0);
                    BitTransferSigned(ref v3, ref v2);
                    Set(e, 0, v0, v0, v0, v2);
                    Set(e, 1, v0 + v1, v0 + v1, v0 + v1, v2 + v3);
                    return true;
                }
                case 6:
                    Set(e, 0, (v[0] * v[3]) >> 8, (v[1] * v[3]) >> 8, (v[2] * v[3]) >> 8, 0xFF);
                    Set(e, 1, v[0], v[1], v[2], 0xFF);
                    return true;
                case 8:
                case 12:
                {
                    int a0 = mode == 12 ? v[6] : 0xFF;
                    int a1 = mode == 12 ? v[7] : 0xFF;
                    if (v[1] + v[3] + v[5] >= v[0] + v[2] + v[4])
                    {
                        Set(e, 0, v[0], v[2], v[4], a0);
                        Set(e, 1, v[1], v[3], v[5], a1);
                    }
                    else
                    {
                        SetBlueContract(e, 0, v[1], v[3], v[5], a1);
                        SetBlueContract(e, 1, v[0], v[2], v[4], a0);
                    }
                    return true;
                }
                case 9:
                case 13:
                {
                    int v0 = v[0], v1 = v[1], v2 = v[2], v3 = v[3], v4 = v[4], v5 = v[5];
                    int v6 = mode == 13 ? v[6] : 0xFF, v7 = mode == 13 ? v[7] : 0;
                    BitTransferSigned(ref v1, ref v0);
                    BitTransferSigned(ref v3, ref v2);
                    BitTransferSigned(ref v5, ref v4);
                    if (mode == 13)
                        BitTransferSigned(ref v7, ref v6);
                    if (v1 + v3 + v5 >= 0)
                    {
                        Set(e, 0, v0, v2, v4, v6);
                        Set(e, 1, v0 + v1, v2 + v3, v4 + v5, v6 + v7);
                    }
                    else
                    {
                        SetBlueContract(e, 0, v0 + v1, v2 + v3, v4 + v5, v6 + v7);
                        SetBlueContract(e, 1, v0, v2, v4, v6);
                    }
                    return true;
                }
                case 10:
                    Set(e, 0, (v[0] * v[3]) >> 8, (v[1] * v[3]) >> 8, (v[2] * v[3]) >> 8, v[4]);
                    Set(e, 1, v[0], v[1], v[2], v[5]);
                    return true;
                default:
                    // HDR endpoint modes (2, 3, 7, 11, 14, 15)
                    return false;
            }
        }

        private static int SelectPartition(int seed, int x, int y, int partitionCount, bool smallBlock)
        {
            if (smallBlock)
            {
                x <<= 1;
                y <<= 1;
            }

            seed += (partitionCount - 1) * 1024;
            uint rnum = Hash52((uint)seed);

            int s1 = (int)(rnum & 0xF);
            int s2 = (int)((rnum >> 4) & 0xF);
            int s3 = (int)((rnum >> 8) & 0xF);
            int s4 = (int)((rnum >> 12) & 0xF);
            int s5 = (int)((rnum >> 16) & 0xF);
            int s6 = (int)((rnum >> 20) & 0xF);
            int s7 = (int)((rnum >> 24) & 0xF);
            int s8 = (int)((rnum >> 28) & 0xF);
            int s9 = (int)((rnum >> 18) & 0xF);
            int s10 = (int)((rnum >> 22) & 0xF);
            int s11 = (int)((rnum >> 26) & 0xF);
            int s12 = (int)(((rnum >> 30) | (rnum << 2)) & 0xF);

            s1 *= s1; s2 *= s2; s3 *= s3; s4 *= s4; s5 *= s5; s6 *= s6;
            s7 *= s7; s8 *= s8; s9 *= s9; s10 *= s10; s11 *= s11; s12 *= s12;

            int sh1, sh2;
            if ((seed & 1) != 0)
            {
                sh1 = (seed & 2) != 0 ? 4 : 5;
                sh2 = partitionCount == 3 ? 6 : 5;
            }
            else
            {
                sh1 = partitionCount == 3 ? 6 : 5;
                sh2 = (seed & 2) != 0 ? 4 : 5;
            }
            int sh3 = (seed & 0x10) != 0 ? sh1 : sh2;

            s1 >>= sh1; s2 >>= sh2; s3 >>= sh1; s4 >>= sh2;
            s5 >>= sh1; s6 >>= sh2; s7 >>= sh1; s8 >>= sh2;
            s9 >>= sh3; s10 >>= sh3; s11 >>= sh3; s12 >>= sh3;

            // z is 0 for 2D blocks, so the s9..s12 terms drop out except as written by the spec
            int a = (int)((s1 * x + s2 * y + (rnum >> 14)) & 0x3F);
            int b = (int)((s3 * x + s4 * y + (rnum >> 10)) & 0x3F);
            int c = (int)((s5 * x + s6 * y + (rnum >> 6)) & 0x3F);
            int d = (int)((s7 * x + s8 * y + (rnum >> 2)) & 0x3F);

            if (partitionCount < 4) d = 0;
            if (partitionCount < 3) c = 0;

            if (a >= b && a >= c && a >= d) return 0;
            if (b >= c && b >= d) return 1;
            if (c >= d) return 2;
            return 3;
        }

        private static uint Hash52(uint p)
        {
            p ^= p >> 15;
            p -= p << 17;
            p += p << 7;
            p += p << 4;
            p ^= p >> 5;
            p += p << 16;
            p ^= p >> 7;
            p ^= p >> 3;
            p ^= p << 6;
            p ^= p >> 17;
            return p;
        }

        private static byte HalfToUnorm8(ushort half)
        {
            float f = (float)BitConverter.UInt16BitsToHalf(half);
            if (!(f > 0))
                return 0;
            return f >= 1 ? (byte)255 : (byte)(f * 255 + 0.5f);
        }

        #endregion
    }
}
