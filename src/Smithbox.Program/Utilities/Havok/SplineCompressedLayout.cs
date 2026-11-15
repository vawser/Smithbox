#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace StudioCore.Utilities;

/// <summary>
///     A lossless model of the data of an hkaSplineCompressedAnimation: masks, knots and the quantized values as they
///     are stored, with a reader and a writer. <see cref="SplineCompressedAnimation" /> decodes the same bytes into
///     floats for playback; this keeps the raw form, so that a clip can be written back (the camera authoring of the
///     Cutscene Editor). Little-endian only, like every game that uses it.
///     <para>
///         A block is: one 4-byte mask per transform track (quantization, position, rotation, scale flags), one mask
///         byte per float track, padding to 4, then each transform track's position, rotation and scale, each padded
///         to 4. The float tracks start at the block's float offset, each padded to 4 before it.
///     </para>
/// </summary>
public static class SplineCompressedLayout
{
    // Component flags of a position/scale group (and of a float track spline, X only)
    public const byte StaticX = 0x01, StaticY = 0x02, StaticZ = 0x04;
    public const byte SplineX = 0x10, SplineY = 0x20, SplineZ = 0x40;

    // Float track masks: 0x01 a static value, 0x10 a spline, 0x02 16-bit spline values
    public const byte FloatStatic = 0x01, FloatBits16 = 0x02, FloatSpline = 0x10;

    /// <summary>
    ///     A position, scale or float spline: up to three components, each a spline, a static value or absent.
    /// </summary>
    public sealed class ScalarGroup
    {
        public byte Flags;
        public bool Bits16;
        public byte Degree;
        public byte[] Knots = Array.Empty<byte>();
        public readonly float[] Static = new float[3];
        public readonly float[] Min = new float[3];
        public readonly float[] Max = new float[3];

        // [control point, component]; only the spline components are used
        public ushort[,] Values = new ushort[0, 3];

        public int ControlPointCount => Values.GetLength(0);

        public bool IsSpline => (Flags & (SplineX | SplineY | SplineZ)) != 0;

        public bool HasSpline(int component) => (Flags & (SplineX << component)) != 0;

        public bool HasStatic(int component) => (Flags & (StaticX << component)) != 0;
    }

    public sealed class RotationGroup
    {
        public byte Flags;
        public int Quantization;
        public byte Degree;
        public byte[] Knots = Array.Empty<byte>();

        // The quantized quaternions, one per control point (or the single static one)
        public List<byte[]> Points = new();

        public bool IsSpline => (Flags & 0xF0) != 0;

        public bool IsStatic => !IsSpline && (Flags & 0x0F) != 0;
    }

    public sealed class TransformTrack
    {
        public ScalarGroup Position = new();
        public RotationGroup Rotation = new();
        public ScalarGroup Scale = new();

        public byte QuantizationByte =>
            (byte)((Position.Bits16 ? 1 : 0) | ((Rotation.Quantization & 0xF) << 2) | ((Scale.Bits16 ? 1 : 0) << 6));
    }

    public sealed class FloatTrack
    {
        public byte Mask;
        public float Static;
        public ScalarGroup? Spline;
    }

    public sealed class Block
    {
        public TransformTrack[] Transforms = Array.Empty<TransformTrack>();
        public FloatTrack[] Floats = Array.Empty<FloatTrack>();

        // Where the transform and float data ended when read, from the start of the data (diagnostics)
        public int ReadTransformEnd;
        public int ReadFloatEnd;
    }

    public static int RotationAlign(int quantization) => quantization switch
    {
        0 => 4, // POLAR32
        1 => 1, // THREECOMP40
        2 => 2, // THREECOMP48
        3 => 1, // THREECOMP24
        4 => 2, // STRAIGHT16
        5 => 4, // UNCOMPRESSED
        _ => throw new NotSupportedException($"Rotation quantization {quantization}")
    };

    public static int RotationSize(int quantization) => quantization switch
    {
        0 => 4,
        1 => 5,
        2 => 6,
        3 => 3,
        4 => 2,
        5 => 16,
        _ => throw new NotSupportedException($"Rotation quantization {quantization}")
    };

    // ---------------------------------------------------------------- reading

    public static List<Block> Read(byte[] data, int transformTrackCount, int floatTrackCount,
        IReadOnlyList<uint> blockOffsets, IReadOnlyList<uint> floatBlockOffsets)
    {
        List<Block> blocks = new();
        for (var b = 0; b < blockOffsets.Count; b++)
        {
            var floatOffset = b < floatBlockOffsets.Count ? (int)floatBlockOffsets[b] : 0;
            blocks.Add(ReadBlock(data, (int)blockOffsets[b], transformTrackCount, floatTrackCount, floatOffset));
        }

        return blocks;
    }

    public static Block ReadBlock(byte[] data, int blockStart, int transformTrackCount, int floatTrackCount,
        int floatBlockOffset)
    {
        var r = new Reader(data, blockStart);
        var block = new Block
        {
            Transforms = new TransformTrack[transformTrackCount],
            Floats = new FloatTrack[floatTrackCount]
        };

        var masks = new byte[transformTrackCount, 4];
        for (var t = 0; t < transformTrackCount; t++)
        {
            for (var i = 0; i < 4; i++)
            {
                masks[t, i] = r.U8();
            }
        }

        var floatMasks = r.Bytes(floatTrackCount);
        r.Align(4);

        for (var t = 0; t < transformTrackCount; t++)
        {
            byte q = masks[t, 0];
            var track = new TransformTrack();

            track.Position = ReadScalar(ref r, masks[t, 1], (q & 3) != 0, 3);
            r.Align(4);

            track.Rotation = ReadRotation(ref r, masks[t, 2], (q >> 2) & 0xF);
            r.Align(4);

            track.Scale = ReadScalar(ref r, masks[t, 3], ((q >> 6) & 3) != 0, 3);
            r.Align(4);

            block.Transforms[t] = track;
        }

        block.ReadTransformEnd = r.Position;

        if (floatTrackCount > 0)
        {
            r.Position = blockStart + floatBlockOffset;
            for (var f = 0; f < floatTrackCount; f++)
            {
                r.Align(4);
                byte mask = floatMasks[f];
                var track = new FloatTrack { Mask = mask };
                if ((mask & FloatSpline) != 0)
                {
                    track.Spline = ReadScalar(ref r, SplineX, (mask & FloatBits16) != 0, 1);
                }
                else if ((mask & FloatStatic) != 0)
                {
                    track.Static = r.F32();
                }

                block.Floats[f] = track;
            }
        }

        block.ReadFloatEnd = r.Position;
        return block;
    }

    private static ScalarGroup ReadScalar(ref Reader r, byte flags, bool bits16, int components)
    {
        var g = new ScalarGroup { Flags = flags, Bits16 = bits16 };
        if (!g.IsSpline)
        {
            for (var c = 0; c < components; c++)
            {
                if (g.HasStatic(c))
                {
                    g.Static[c] = r.F32();
                }
            }

            return g;
        }

        int items = r.I16();
        g.Degree = r.U8();
        g.Knots = r.Bytes(items + g.Degree + 2);
        r.Align(4);

        for (var c = 0; c < components; c++)
        {
            if (g.HasSpline(c))
            {
                g.Min[c] = r.F32();
                g.Max[c] = r.F32();
            }
            else if (g.HasStatic(c))
            {
                g.Static[c] = r.F32();
            }
        }

        g.Values = new ushort[items + 1, 3];
        for (var i = 0; i <= items; i++)
        {
            for (var c = 0; c < components; c++)
            {
                if (g.HasSpline(c))
                {
                    g.Values[i, c] = bits16 ? r.U16() : r.U8();
                }
            }
        }

        return g;
    }

    private static RotationGroup ReadRotation(ref Reader r, byte flags, int quantization)
    {
        var g = new RotationGroup { Flags = flags, Quantization = quantization };
        if (g.IsSpline)
        {
            int items = r.I16();
            g.Degree = r.U8();
            g.Knots = r.Bytes(items + g.Degree + 2);
            r.Align(RotationAlign(quantization));
            for (var i = 0; i <= items; i++)
            {
                g.Points.Add(r.Bytes(RotationSize(quantization)));
            }
        }
        else if (g.IsStatic)
        {
            r.Align(RotationAlign(quantization));
            g.Points.Add(r.Bytes(RotationSize(quantization)));
        }

        return g;
    }

    // ---------------------------------------------------------------- writing

    /// <summary>The data, the start of each block and each block's float data offset (from the block's start).</summary>
    public sealed record Written(byte[] Data, uint[] BlockOffsets, uint[] FloatBlockOffsets, int MaskAndQuantizationSize);

    /// <summary>
    ///     Writes blocks one after another: each block's masks and transforms, then its float tracks, padded to 16.
    ///     Byte-identical to the vanilla data of every ER cutscene clip. The mask size is m_maskAndQuantizationSize
    ///     (4 per transform track and 1 per float track, padded to 4).
    /// </summary>
    public static Written Write(IReadOnlyList<Block> blocks)
    {
        var w = new Writer();
        var blockOffsets = new uint[blocks.Count];
        var floatOffsets = new uint[blocks.Count];
        var maskSize = 0;

        for (var b = 0; b < blocks.Count; b++)
        {
            w.Align(16);
            Block block = blocks[b];
            var start = w.Position;
            blockOffsets[b] = (uint)start;

            foreach (TransformTrack t in block.Transforms)
            {
                w.U8(t.QuantizationByte);
                w.U8(t.Position.Flags);
                w.U8(t.Rotation.Flags);
                w.U8(t.Scale.Flags);
            }

            foreach (FloatTrack f in block.Floats)
            {
                w.U8(f.Mask);
            }

            w.Align(4);
            maskSize = w.Position - start;

            foreach (TransformTrack t in block.Transforms)
            {
                WriteScalar(w, t.Position, 3);
                w.Align(4);
                WriteRotation(w, t.Rotation);
                w.Align(4);
                WriteScalar(w, t.Scale, 3);
                w.Align(4);
            }

            // The float tracks follow straight on (the transforms already end 4-aligned)
            floatOffsets[b] = (uint)(w.Position - start);

            foreach (FloatTrack f in block.Floats)
            {
                w.Align(4);
                if ((f.Mask & FloatSpline) != 0 && f.Spline != null)
                {
                    WriteScalar(w, f.Spline, 1);
                }
                else if ((f.Mask & FloatStatic) != 0)
                {
                    w.F32(f.Static);
                }
            }
        }

        w.Align(16);
        return new Written(w.ToArray(), blockOffsets, floatOffsets, maskSize);
    }

    private static void WriteScalar(Writer w, ScalarGroup g, int components)
    {
        if (!g.IsSpline)
        {
            for (var c = 0; c < components; c++)
            {
                if (g.HasStatic(c))
                {
                    w.F32(g.Static[c]);
                }
            }

            return;
        }

        var items = g.ControlPointCount - 1;
        CheckKnots(g.Knots, items, g.Degree);
        w.I16((short)items);
        w.U8(g.Degree);
        w.Bytes(g.Knots);
        w.Align(4);

        for (var c = 0; c < components; c++)
        {
            if (g.HasSpline(c))
            {
                w.F32(g.Min[c]);
                w.F32(g.Max[c]);
            }
            else if (g.HasStatic(c))
            {
                w.F32(g.Static[c]);
            }
        }

        for (var i = 0; i <= items; i++)
        {
            for (var c = 0; c < components; c++)
            {
                if (!g.HasSpline(c))
                {
                    continue;
                }

                if (g.Bits16)
                {
                    w.U16(g.Values[i, c]);
                }
                else
                {
                    w.U8((byte)g.Values[i, c]);
                }
            }
        }
    }

    private static void WriteRotation(Writer w, RotationGroup g)
    {
        var size = RotationSize(g.Quantization);
        if (g.IsSpline)
        {
            var items = g.Points.Count - 1;
            CheckKnots(g.Knots, items, g.Degree);
            w.I16((short)items);
            w.U8(g.Degree);
            w.Bytes(g.Knots);
            w.Align(RotationAlign(g.Quantization));
            foreach (var p in g.Points)
            {
                if (p.Length != size)
                {
                    throw new InvalidOperationException("A rotation point has the wrong size for its quantization.");
                }

                w.Bytes(p);
            }
        }
        else if (g.IsStatic)
        {
            w.Align(RotationAlign(g.Quantization));
            w.Bytes(g.Points[0]);
        }
    }

    private static void CheckKnots(byte[] knots, int items, int degree)
    {
        if (knots.Length != items + degree + 2)
        {
            throw new InvalidOperationException(
                $"A spline with {items + 1} control points of degree {degree} needs {items + degree + 2} knots, not {knots.Length}.");
        }
    }

    // ---------------------------------------------------------------- byte access

    private struct Reader
    {
        private readonly byte[] _data;
        public int Position;

        public Reader(byte[] data, int position)
        {
            _data = data;
            Position = position;
        }

        public byte U8() => _data[Position++];

        public short I16()
        {
            var v = BinaryPrimitives.ReadInt16LittleEndian(_data.AsSpan(Position));
            Position += 2;
            return v;
        }

        public ushort U16()
        {
            var v = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(Position));
            Position += 2;
            return v;
        }

        public float F32()
        {
            var v = BinaryPrimitives.ReadSingleLittleEndian(_data.AsSpan(Position));
            Position += 4;
            return v;
        }

        public byte[] Bytes(int count)
        {
            var v = _data.AsSpan(Position, count).ToArray();
            Position += count;
            return v;
        }

        public void Align(int alignment)
        {
            Position = (Position + alignment - 1) / alignment * alignment;
        }
    }

    private sealed class Writer
    {
        private readonly List<byte> _data = new();

        public int Position => _data.Count;

        public void U8(byte v) => _data.Add(v);

        public void I16(short v)
        {
            Span<byte> b = stackalloc byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(b, v);
            _data.AddRange(b);
        }

        public void U16(ushort v)
        {
            Span<byte> b = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(b, v);
            _data.AddRange(b);
        }

        public void F32(float v)
        {
            Span<byte> b = stackalloc byte[4];
            BinaryPrimitives.WriteSingleLittleEndian(b, v);
            _data.AddRange(b);
        }

        public void Bytes(byte[] v) => _data.AddRange(v);

        public void Align(int alignment)
        {
            while (_data.Count % alignment != 0)
            {
                _data.Add(0);
            }
        }

        public byte[] ToArray() => _data.ToArray();
    }
}
