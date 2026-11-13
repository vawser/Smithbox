using System.Buffers.Binary;
using System.Numerics;

namespace Havok.Shared;

public enum HavokQuantizedCodec
{
    /// <summary>hkaDeltaCompressedSkeletalAnimation: per block, a full value then quantized frame-to-frame deltas.</summary>
    Delta,

    /// <summary>hkaWaveletSkeletalAnimation: per block, quantized CDF 5/3 wavelet coefficients.</summary>
    Wavelet
}

/// <summary>
/// The fields of a delta or wavelet clip that its data buffer is laid out by. Offsets are byte indices into
/// <see cref="Data" />.
/// </summary>
public sealed class HavokQuantizedLayout
{
    public HavokQuantizedCodec Codec;
    public int TransformTracks;
    public int FloatTracks;
    public int Poses;
    public int BlockSize;

    // QuantizationFormat
    public int Preserved;
    public int DynamicCount;
    public int OffsetIdx;
    public int ScaleIdx;
    public int BitWidthIdx;

    public int StaticMaskIdx;
    public int StaticDofsIdx;
    public int QuantizedDataIdx;
    public int QuantizedDataSize;

    // Wavelet only: the start of each block within the quantized data
    public int BlockIndexIdx;
    public int BlockIndexSize;

    public byte[] Data = Array.Empty<byte>();
}

/// <summary>
/// Decodes the Havok 5.x delta and wavelet compressed clips that Demon's Souls uses. The layout was worked out from the
/// game's files (Claude/Tools/DesAnimationSurvey): every block of every DES clip parses to its stored boundary.
/// <list type="bullet">
/// <item>The data buffer is little endian, whatever the packfile's byte order.</item>
/// <item>Each track has a 16-bit mask: bits 0-1, 2-3 and 4-5 give the position, rotation and scale type (0 dynamic,
/// 1 static, 2 identity); bits 8, 7, 6 mark position X, Y, Z dynamic, 12, 11, 10, 9 rotation X, Y, Z, W, and 15, 14,
/// 13 scale X, Y, Z.</item>
/// <item>Static values are floats, in track order then X, Y, Z(, W). A static rotation W of ±2 means "rebuild W from
/// X, Y and Z, with this sign".</item>
/// <item>Dynamic DOFs are numbered in the same order. Each has an offset, a scale and a bit width; a quantized value q
/// stands for offset + q × scale / (2^width − 1).</item>
/// </list>
/// </summary>
public static class HavokQuantizedAnimationCodec
{
    private const int TypeDynamic = 0;
    private const int TypeStatic = 1;

    private static readonly int[] PositionBits = [8, 7, 6];
    private static readonly int[] RotationBits = [12, 11, 10, 9];
    private static readonly int[] ScaleBits = [15, 14, 13];

    public static HavokQsTransform[] Decode(HavokQuantizedLayout layout)
    {
        var poses = Math.Max(layout.Poses, 0);
        var tracks = Math.Max(layout.TransformTracks, 0);

        float[][] dynamic = layout.Codec == HavokQuantizedCodec.Wavelet ? DecodeWavelet(layout) : DecodeDelta(layout);

        var result = new HavokQsTransform[poses * tracks];
        var staticCursor = layout.StaticDofsIdx;
        var dof = 0;

        var position = new float[3];
        var rotation = new float[4];
        var scale = new float[3];

        // Per component: the dynamic DOF that drives it, or -1 when its value is constant
        var positionDof = new int[3];
        var rotationDof = new int[4];
        var scaleDof = new int[3];

        for (var track = 0; track < tracks; track++)
        {
            int mask = BinaryPrimitives.ReadUInt16LittleEndian(layout.Data.AsSpan(layout.StaticMaskIdx + track * 2));

            ReadComponents(layout.Data, mask & 3, mask, PositionBits, 0, position, positionDof, ref staticCursor, ref dof);
            ReadComponents(layout.Data, (mask >> 2) & 3, mask, RotationBits, 0, rotation, rotationDof, ref staticCursor, ref dof);
            if (((mask >> 2) & 3) == 2)
            {
                rotation[3] = 1;
            }

            ReadComponents(layout.Data, (mask >> 4) & 3, mask, ScaleBits, 1, scale, scaleDof, ref staticCursor, ref dof);

            for (var frame = 0; frame < poses; frame++)
            {
                Vector3 t = new(Sample(dynamic, positionDof[0], frame, position[0]),
                    Sample(dynamic, positionDof[1], frame, position[1]),
                    Sample(dynamic, positionDof[2], frame, position[2]));

                Quaternion r = RebuildRotation(Sample(dynamic, rotationDof[0], frame, rotation[0]),
                    Sample(dynamic, rotationDof[1], frame, rotation[1]),
                    Sample(dynamic, rotationDof[2], frame, rotation[2]),
                    Sample(dynamic, rotationDof[3], frame, rotation[3]));

                Vector3 s = new(Sample(dynamic, scaleDof[0], frame, scale[0]),
                    Sample(dynamic, scaleDof[1], frame, scale[1]),
                    Sample(dynamic, scaleDof[2], frame, scale[2]));

                result[frame * tracks + track] = new HavokQsTransform(t, r, s);
            }
        }

        return result;
    }

    /// <summary>
    /// Fills one of a track's position, rotation or scale from the static values, and notes which components are
    /// dynamic.
    /// </summary>
    private static void ReadComponents(byte[] data, int type, int mask, int[] bits, float identity, float[] values,
        int[] dofs, ref int staticCursor, ref int dof)
    {
        for (var k = 0; k < bits.Length; k++)
        {
            dofs[k] = -1;
            values[k] = identity;

            if (type == TypeStatic || (type == TypeDynamic && ((mask >> bits[k]) & 1) == 0))
            {
                values[k] = BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(staticCursor));
                staticCursor += 4;
            }
            else if (type == TypeDynamic)
            {
                dofs[k] = dof++;
            }
        }
    }

    private static float Sample(float[][] dynamic, int dof, int frame, float constant)
    {
        if (dof < 0 || dof >= dynamic.Length)
        {
            return constant;
        }

        float[] series = dynamic[dof];
        return series.Length == 0 ? constant : series[Math.Min(frame, series.Length - 1)];
    }

    private static Quaternion RebuildRotation(float x, float y, float z, float w)
    {
        // W was left out when it can be recovered: its stored value is 2 with the sign it takes
        if (MathF.Abs(MathF.Abs(w) - 2) < 1e-4f)
        {
            var rest = 1 - (x * x + y * y + z * z);
            w = MathF.CopySign(rest > 0 ? MathF.Sqrt(rest) : 0, w);
        }

        Quaternion q = new(x, y, z, w);
        return q.LengthSquared() > 1e-12f ? Quaternion.Normalize(q) : Quaternion.Identity;
    }

    private static (float[] Offsets, float[] Scales, byte[] Widths) ReadQuantization(HavokQuantizedLayout layout)
    {
        var count = Math.Max(layout.DynamicCount, 0);
        var offsets = new float[count];
        var scales = new float[count];
        var widths = new byte[count];

        for (var d = 0; d < count; d++)
        {
            offsets[d] = BinaryPrimitives.ReadSingleLittleEndian(layout.Data.AsSpan(layout.OffsetIdx + d * 4));
            scales[d] = BinaryPrimitives.ReadSingleLittleEndian(layout.Data.AsSpan(layout.ScaleIdx + d * 4));
            widths[d] = layout.Data[layout.BitWidthIdx + d];
        }

        return (offsets, scales, widths);
    }

    /// <summary>
    /// Per block and dynamic DOF: a block-size bit header with a set bit for each coefficient left out (truncated to
    /// zero), then each kept coefficient, coarse to fine, at the DOF's bit width; then padding to a byte. The block is
    /// rebuilt with an inverse CDF 5/3 lifting transform with mirrored edges, and blocks follow on without overlap.
    /// </summary>
    private static float[][] DecodeWavelet(HavokQuantizedLayout layout)
    {
        var (offsets, scales, widths) = ReadQuantization(layout);
        var blockSize = Math.Max(layout.BlockSize, 1);
        var poses = Math.Max(layout.Poses, 0);

        var series = new float[offsets.Length][];
        for (var d = 0; d < series.Length; d++)
        {
            series[d] = new float[poses];
        }

        var coefficients = new float[blockSize];
        var headerBits = (blockSize + 7) / 8 * 8;

        for (var block = 0; block < layout.BlockIndexSize; block++)
        {
            var start = (int)BinaryPrimitives.ReadUInt32LittleEndian(layout.Data.AsSpan(layout.BlockIndexIdx + block * 4));
            BitReader reader = new(layout.Data, layout.QuantizedDataIdx + start);

            for (var d = 0; d < series.Length; d++)
            {
                var header = reader.Read(headerBits);
                var max = widths[d] > 0 ? (1UL << widths[d]) - 1 : 1;

                for (var c = 0; c < blockSize; c++)
                {
                    coefficients[c] = ((header >> c) & 1) != 0
                        ? 0
                        : offsets[d] + reader.Read(widths[d]) * scales[d] / max;
                }

                reader.AlignToByte();

                float[] samples = InverseCdf53(coefficients);
                var first = block * blockSize;
                for (var i = 0; i < samples.Length && first + i < poses; i++)
                {
                    series[d][first + i] = samples[i];
                }
            }
        }

        return series;
    }

    /// <summary>
    /// The coefficients run from the approximation through each detail band, coarsest first. Each level undoes
    /// update (even = a − (d[i−1] + d[i]) / 4) then predict (odd = d + (even[i] + even[i+1]) / 2), with the band's
    /// first and last values repeated past its edges.
    /// </summary>
    public static float[] InverseCdf53(float[] coefficients)
    {
        var n = coefficients.Length;
        var levels = 0;
        while (1 << (levels + 1) <= n)
        {
            levels++;
        }

        var length = n >> levels;
        var current = coefficients[..length];
        var position = length;

        for (var level = 0; level < levels; level++)
        {
            float[] detail = coefficients[position..(position + length)];
            position += length;

            var even = new float[length];
            for (var i = 0; i < length; i++)
            {
                even[i] = current[i] - (detail[Math.Max(i - 1, 0)] + detail[i]) / 4;
            }

            var output = new float[length * 2];
            for (var i = 0; i < length; i++)
            {
                output[2 * i] = even[i];
                output[2 * i + 1] = detail[i] + (even[i] + even[Math.Min(i + 1, length - 1)]) / 2;
            }

            current = output;
            length *= 2;
        }

        return current;
    }

    /// <summary>
    /// Per block and dynamic DOF: <see cref="HavokQuantizedLayout.Preserved" /> full floats, then the remaining frames
    /// of the block as quantized deltas, each added to the frame before; then padding to a byte. Blocks hold
    /// block-size frames, the last one what is left.
    /// </summary>
    private static float[][] DecodeDelta(HavokQuantizedLayout layout)
    {
        var (offsets, scales, widths) = ReadQuantization(layout);
        var blockSize = Math.Max(layout.BlockSize, 1);
        var poses = Math.Max(layout.Poses, 0);

        var series = new float[offsets.Length][];
        for (var d = 0; d < series.Length; d++)
        {
            series[d] = new float[poses];
        }

        var position = layout.QuantizedDataIdx;

        for (var first = 0; first < poses; first += blockSize)
        {
            var items = Math.Min(blockSize, poses - first);

            for (var d = 0; d < series.Length; d++)
            {
                float value = 0;
                var preserved = Math.Min(layout.Preserved, items);

                for (var i = 0; i < preserved; i++)
                {
                    value = BinaryPrimitives.ReadSingleLittleEndian(layout.Data.AsSpan(position));
                    series[d][first + i] = value;
                    position += 4;
                }

                BitReader reader = new(layout.Data, position);
                var max = widths[d] > 0 ? (1UL << widths[d]) - 1 : 1;

                for (var i = preserved; i < items; i++)
                {
                    value += offsets[d] + reader.Read(widths[d]) * scales[d] / max;
                    series[d][first + i] = value;
                }

                position += (widths[d] * (items - preserved) + 7) / 8;
            }
        }

        return series;
    }

    /// <summary>Reads values least significant bit first.</summary>
    private struct BitReader
    {
        private readonly byte[] _data;
        private long _bit;

        public BitReader(byte[] data, int byteOffset)
        {
            _data = data;
            _bit = byteOffset * 8L;
        }

        public ulong Read(int count)
        {
            ulong value = 0;
            for (var i = 0; i < count; i++, _bit++)
            {
                var index = (int)(_bit >> 3);
                if (index >= _data.Length)
                {
                    throw new InvalidDataException("The quantized animation data ends early.");
                }

                value |= (ulong)((_data[index] >> (int)(_bit & 7)) & 1) << i;
            }

            return value;
        }

        public void AlignToByte()
        {
            _bit = (_bit + 7) & ~7L;
        }
    }
}
