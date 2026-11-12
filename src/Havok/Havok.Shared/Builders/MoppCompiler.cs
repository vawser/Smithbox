#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Havok.Builders;

/// <summary>
/// Builds MOPP code (see <see cref="MoppCode"/>) for a triangle mesh: a tree of median splits written with the
/// axis-aligned instructions only (cuts, axis splits, rescales, jumps, re-offsets and terminals), so every instruction
/// it writes is one whose meaning was checked against the games' own code by MoppSurvey's decode mode.
/// A query box reaches every triangle it overlaps: each triangle's bounds are rounded outwards by a few quanta, and
/// every comparison keeps a triangle when its byte range touches the query's.
/// </summary>
public static class MoppCompiler
{
    /// <summary>One triangle to place, by its shape key (the terminal the code gives for it).</summary>
    public readonly record struct Triangle(Vector3 A, Vector3 B, Vector3 C, uint Key);

    /// <summary>The code and its quantisation (offset xyz, scale in w), as hkpMoppCode.m_info.m_offset holds it.</summary>
    public sealed record Result(byte[] Code, Vector4 Offset);

    private const int Quanta = 1 << 24;

    // Triangle bounds grow by this many quanta, so float rounding of a query never drops a triangle
    private const int Pad = 4;

    private sealed class Item
    {
        public int[] Lo = new int[3];
        public int[] Hi = new int[3];
        public uint Key;
    }

    /// <summary>Builds the code. The triangles may be empty, which gives code with no terminals.</summary>
    public static Result Compile(IReadOnlyList<Triangle> triangles)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var t in triangles)
        {
            min = Vector3.Min(min, Vector3.Min(t.A, Vector3.Min(t.B, t.C)));
            max = Vector3.Max(max, Vector3.Max(t.A, Vector3.Max(t.B, t.C)));
        }

        if (triangles.Count == 0)
        {
            min = Vector3.Zero;
            max = Vector3.One;
        }

        // A small margin around the mesh, as the games' own codes have
        var extent = max - min;
        var largest = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z));
        var margin = 0.01f + largest * 0.002f;
        var origin = min - new Vector3(margin);
        var scale = (Quanta - 1) / (largest + 2 * margin);
        var offset = new Vector4(origin, scale);

        var items = new List<Item>(triangles.Count);

        foreach (var t in triangles)
        {
            var lo = Vector3.Min(t.A, Vector3.Min(t.B, t.C));
            var hi = Vector3.Max(t.A, Vector3.Max(t.B, t.C));
            var item = new Item { Key = t.Key };

            for (var a = 0; a < 3; a++)
            {
                item.Lo[a] = Math.Clamp((int)MathF.Floor((Axis(lo, a) - Axis(origin, a)) * scale) - Pad, 0, Quanta - 1);
                item.Hi[a] = Math.Clamp((int)MathF.Floor((Axis(hi, a) - Axis(origin, a)) * scale) + Pad, 0, Quanta - 1);
            }

            items.Add(item);
        }

        var code = new List<byte>();

        if (items.Count > 0)
        {
            var known = new[] { int.MinValue, int.MinValue, int.MinValue, int.MaxValue, int.MaxValue, int.MaxValue };
            code.AddRange(Emit(items, new[] { 0, 0, 0 }, 16, known, 0));
        }
        else
        {
            // Nothing to reach: a cut no query passes
            code.AddRange(new byte[] { 0x26, 0xFF, 0x00 });
        }

        // Padded to 16 bytes as the games' codes are
        while (code.Count % 16 != 0)
            code.Add(0xCD);

        return new Result(code.ToArray(), offset);
    }

    private static float Axis(Vector3 v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

    private static int ByteOf(int q, int origin, int shift) => (q - origin) >> shift;

    /// <summary>
    /// The code of a node: its cuts, a rescale if its bounds allow a finer byte, then a terminal or a split into two
    /// halves. known holds the cut ranges (lo xyz, hi xyz) the query already passed, in this frame's bytes.
    /// </summary>
    private static List<byte> Emit(List<Item> items, int[] origin, int shift, int[] known, uint keyBase)
    {
        var code = new List<byte>();
        known = (int[])known.Clone();

        while (true)
        {
            var lo = new int[3];
            var hi = new int[3];

            for (var a = 0; a < 3; a++)
            {
                lo[a] = items.Min(i => ByteOf(i.Lo[a], origin[a], shift));
                hi[a] = items.Max(i => ByteOf(i.Hi[a], origin[a], shift));
            }

            // Cuts, where the node is tighter than what the query already passed
            for (var a = 0; a < 3; a++)
            {
                if (lo[a] > known[a] || hi[a] < known[3 + a])
                {
                    code.Add((byte)(0x26 + a));
                    code.Add((byte)lo[a]);
                    code.Add((byte)hi[a]);
                    known[a] = lo[a];
                    known[3 + a] = hi[a];
                }
            }

            // A finer byte once the node fits in half the range or less on every axis
            var span = Math.Max(hi[0] - lo[0], Math.Max(hi[1] - lo[1], hi[2] - lo[2])) + 1;
            var k = 0;

            while (k < 4 && k < shift && (span << (k + 1)) <= 256)
                k++;

            if (k == 0)
                break;

            code.Add((byte)k);
            code.Add((byte)lo[0]);
            code.Add((byte)lo[1]);
            code.Add((byte)lo[2]);

            origin = new[] { origin[0] + (lo[0] << shift), origin[1] + (lo[1] << shift), origin[2] + (lo[2] << shift) };
            shift -= k;

            // The query passed the cut at the coarser byte: in the finer one it can reach the region's edges
            for (var a = 0; a < 3; a++)
            {
                known[a] = 0;
                known[3 + a] = ((hi[a] - lo[a] + 1) << k) - 1;
            }
        }

        // Terminals relative to a base: re-offset once the keys are far from it
        var minKey = items.Min(i => i.Key);

        if (minKey - keyBase >= 256)
        {
            var delta = minKey - keyBase;

            if (delta < 256)
            {
                code.Add(0x09);
                code.Add((byte)delta);
            }
            else if (delta < 65536)
            {
                code.Add(0x0A);
                code.Add((byte)(delta >> 8));
                code.Add((byte)delta);
            }
            else
            {
                code.Add(0x0B);
                code.Add((byte)(delta >> 24));
                code.Add((byte)(delta >> 16));
                code.Add((byte)(delta >> 8));
                code.Add((byte)delta);
            }

            keyBase = minKey;
        }

        if (items.Count == 1)
        {
            var value = items[0].Key - keyBase;

            if (value < 32)
            {
                code.Add((byte)(0x30 + value));
            }
            else if (value < 256)
            {
                code.Add(0x50);
                code.Add((byte)value);
            }
            else if (value < 65536)
            {
                code.Add(0x51);
                code.Add((byte)(value >> 8));
                code.Add((byte)value);
            }
            else
            {
                // Within 2^24 of the base by the re-offset above
                code.Add(0x52);
                code.Add((byte)(value >> 16));
                code.Add((byte)(value >> 8));
                code.Add((byte)value);
            }

            return code;
        }

        // Split on the axis the node is longest along, at the median of the triangles' middles
        var nodeLo = new int[3];
        var nodeHi = new int[3];

        for (var a = 0; a < 3; a++)
        {
            nodeLo[a] = items.Min(i => ByteOf(i.Lo[a], origin[a], shift));
            nodeHi[a] = items.Max(i => ByteOf(i.Hi[a], origin[a], shift));
        }

        var axis = 0;

        for (var a = 1; a < 3; a++)
        {
            if (nodeHi[a] - nodeLo[a] > nodeHi[axis] - nodeLo[axis])
                axis = a;
        }

        List<Item> left, right;
        int leftHi, rightLo;
        byte op;

        if (nodeHi[axis] == nodeLo[axis])
        {
            // Every triangle in one byte and no finer one left: both halves are always visited
            var byKey = items.OrderBy(i => i.Key).ToList();
            left = byKey.Take(byKey.Count / 2).ToList();
            right = byKey.Skip(byKey.Count / 2).ToList();
            leftHi = 255;
            rightLo = 0;
            axis = 0;
        }
        else
        {
            var sorted = items.OrderBy(i => (long)ByteOf(i.Lo[axis], origin[axis], shift) + ByteOf(i.Hi[axis], origin[axis], shift))
                .ThenBy(i => i.Key).ToList();
            var half = sorted.Count / 2;
            left = sorted.Take(half).ToList();
            right = sorted.Skip(half).ToList();
            leftHi = left.Max(i => ByteOf(i.Hi[axis], origin[axis], shift));
            rightLo = right.Min(i => ByteOf(i.Lo[axis], origin[axis], shift));
        }

        var leftCode = Emit(left, origin, shift, known, keyBase);
        var rightCode = Emit(right, origin, shift, known, keyBase);

        if (leftCode.Count <= 255)
        {
            op = (byte)(0x10 + axis);
            code.Add(op);
            code.Add((byte)leftHi);
            code.Add((byte)rightLo);
            code.Add((byte)leftCode.Count);
        }
        else if (leftCode.Count <= 65535)
        {
            // Left right after, right past it
            code.Add((byte)(0x23 + axis));
            code.Add((byte)leftHi);
            code.Add((byte)rightLo);
            code.Add(0);
            code.Add(0);
            code.Add((byte)(leftCode.Count >> 8));
            code.Add((byte)leftCode.Count);
        }
        else
        {
            // Right goes through a 24-bit jump over the left, which starts after it
            code.Add((byte)(0x23 + axis));
            code.Add((byte)leftHi);
            code.Add((byte)rightLo);
            code.Add(0);
            code.Add(4);
            code.Add(0);
            code.Add(0);
            code.Add(0x07);
            code.Add((byte)(leftCode.Count >> 16));
            code.Add((byte)(leftCode.Count >> 8));
            code.Add((byte)leftCode.Count);
        }

        code.AddRange(leftCode);
        code.AddRange(rightCode);
        return code;
    }
}
