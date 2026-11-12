#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Havok.Builders;

/// <summary>
/// Havok's MOPP byte code (hkpMoppCode): the bounding volume tree of the older (hkp) games' mesh collision. The code
/// works in a quantised space: a point p is (p - offset.xyz) * offset.w, offset.w being 2^24 over the largest extent,
/// so every coordinate fits 24 bits. Instructions compare one byte of it at a time, the top byte at first; a rescale
/// moves the region's origin and takes a finer byte. Opcodes, as found in DES, DS1, DS1R and DS2S files
/// (MoppSurvey ops):
/// <list type="bullet">
/// <item>01-04 RESCALE k: x y z region offsets (in the current byte), then k more bits of precision</item>
/// <item>05/06 JUMP8/16: relative to the instruction's end. 70: JUMP to an absolute 32-bit address (a chunk)</item>
/// <item>09/0A/0B TERM_REOFFSET8/16/32: the base added to the terminals after it</item>
/// <item>10-1C SPLIT dir hi lo jump8: left (next) if the query reaches below hi, right (end + jump) if above lo;
///   10/11/12 are the X/Y/Z axes, 13-1C the diagonals</item>
/// <item>20-22 SPLIT X/Y/Z plane jump8, one plane value for both sides</item>
/// <item>23-25 SPLIT X/Y/Z hi lo left16 right16: both children by 16-bit offsets</item>
/// <item>26-28 CUT X/Y/Z lo hi: the query is dropped if it is outside</item>
/// <item>30-4F TERM 0-31; 50/51/52 TERM8/16/24: a terminal (shape key) of base + value</item>
/// </list>
/// Big-endian operands throughout. The code is padded with 0xCD to 16 bytes.
/// </summary>
public static class MoppCode
{
    // The first byte compared is a coordinate's top 8 of 24 bits
    private const int TopShift = 16;

    /// <summary>A point's quantised coordinate, floored and clamped to 24 bits.</summary>
    public static (int X, int Y, int Z) Quantise(Vector3 p, Vector4 offset)
    {
        static int Q(float v) => (int)Math.Clamp(MathF.Floor(v), 0f, (1 << 24) - 1);
        return (Q((p.X - offset.X) * offset.W), Q((p.Y - offset.Y) * offset.W), Q((p.Z - offset.Z) * offset.W));
    }

    /// <summary>
    /// The terminals (shape keys) whose region the query box reaches. Diagonal splits are taken both ways, so this is
    /// a superset for codes that use them, exact for the axis-only codes the compiler writes.
    /// </summary>
    public static List<uint> Query(byte[] code, Vector4 offset, Vector3 min, Vector3 max)
    {
        var lo = Quantise(min, offset);
        var hi = Quantise(max, offset);
        var result = new List<uint>();
        Walk(code, 0, new State(0, 0, 0, TopShift, 0), new[] { lo.X, lo.Y, lo.Z }, new[] { hi.X, hi.Y, hi.Z }, result, 0);
        return result;
    }

    /// <summary>Every terminal of the code, by walking both sides of every split.</summary>
    public static List<uint> AllTerminals(byte[] code)
    {
        var result = new List<uint>();
        Walk(code, 0, new State(0, 0, 0, TopShift, 0), new[] { int.MinValue / 4, int.MinValue / 4, int.MinValue / 4 },
            new[] { int.MaxValue / 4, int.MaxValue / 4, int.MaxValue / 4 }, result, 0);
        return result;
    }

    /// <summary>
    /// For checking: the instructions on the way to a terminal (walked with no bounds), with the query's bytes at
    /// each and whether it passes there. Null if the code has no such terminal.
    /// </summary>
    public static List<string> Explain(byte[] code, Vector4 offset, Vector3 min, Vector3 max, uint key)
    {
        var lo = Quantise(min, offset);
        var hi = Quantise(max, offset);
        var qMin = new[] { lo.X, lo.Y, lo.Z };
        var qMax = new[] { hi.X, hi.Y, hi.Z };
        List<string> found = null;
        var path = new List<string>();
        var big = new[] { int.MinValue / 4, int.MinValue / 4, int.MinValue / 4 };
        var bigMax = new[] { int.MaxValue / 4, int.MaxValue / 4, int.MaxValue / 4 };

        void Trace(int pc, State s)
        {
            var origin = new[] { s.OX, s.OY, s.OZ };
            string Bytes() => string.Join(" ", Enumerable.Range(0, 3).Select(a => $"{"xyz"[a]} {Byte(qMin[a], origin[a], s.Shift)}-{Byte(qMax[a], origin[a], s.Shift)}"));
            path.Add($"@{pc} {string.Join(" ", code.Skip(pc).Take(7).Select(b => b.ToString("X2")))} | shift {s.Shift} | {Bytes()}");
        }

        void Visit(int pc, State s, int depth)
        {
            if (found != null || depth > 4096) return;
            var start = path.Count;
            var single = new List<uint>();
            // One instruction at a time: Walk's own branching is reproduced here with both sides taken
            while (found == null)
            {
                var op = code[pc];
                Trace(pc, s);

                switch (op)
                {
                    case >= 0x01 and <= 0x04:
                        s = new State(s.OX + (code[pc + 1] << s.Shift), s.OY + (code[pc + 2] << s.Shift), s.OZ + (code[pc + 3] << s.Shift), s.Shift - op, s.Base);
                        pc += 4; continue;
                    case 0x05: pc += 2 + code[pc + 1]; continue;
                    case 0x06: pc += 3 + (code[pc + 1] << 8 | code[pc + 2]); continue;
                    case 0x07: pc += 4 + (code[pc + 1] << 16 | code[pc + 2] << 8 | code[pc + 3]); continue;
                    case 0x08: pc += 5 + (code[pc + 1] << 24 | code[pc + 2] << 16 | code[pc + 3] << 8 | code[pc + 4]); continue;
                    case 0x70: pc = code[pc + 1] << 24 | code[pc + 2] << 16 | code[pc + 3] << 8 | code[pc + 4]; continue;
                    case 0x09: s = s with { Base = s.Base + code[pc + 1] }; pc += 2; continue;
                    case 0x0A: s = s with { Base = s.Base + (uint)(code[pc + 1] << 8 | code[pc + 2]) }; pc += 3; continue;
                    case 0x0B: s = s with { Base = s.Base + (uint)(code[pc + 1] << 24 | code[pc + 2] << 16 | code[pc + 3] << 8 | code[pc + 4]) }; pc += 5; continue;
                    case >= 0x10 and <= 0x1C: Visit(pc + 4 + code[pc + 3], s, depth + 1); pc += 4; continue;
                    case >= 0x20 and <= 0x22: Visit(pc + 3 + code[pc + 2], s, depth + 1); pc += 3; continue;
                    case >= 0x23 and <= 0x25: Visit(pc + 7 + (code[pc + 5] << 8 | code[pc + 6]), s, depth + 1); pc += 7 + (code[pc + 3] << 8 | code[pc + 4]); continue;
                    case >= 0x26 and <= 0x28: pc += 3; continue;
                    case >= 0x30 and <= 0x4F: single.Add(s.Base + (uint)(op - 0x30)); break;
                    case 0x50: single.Add(s.Base + code[pc + 1]); break;
                    case 0x51: single.Add(s.Base + (uint)(code[pc + 1] << 8 | code[pc + 2])); break;
                    case 0x52: single.Add(s.Base + (uint)(code[pc + 1] << 16 | code[pc + 2] << 8 | code[pc + 3])); break;
                    default: return;
                }

                if (single.Contains(key)) found = new List<string>(path);
                break;
            }

            path.RemoveRange(start, path.Count - start);
        }

        Visit(0, new State(0, 0, 0, TopShift, 0), 0);
        return found;
    }

    private readonly record struct State(int OX, int OY, int OZ, int Shift, uint Base);

    private static int Byte(int q, int origin, int shift) => (q - origin) >> shift;

    private static void Walk(byte[] c, int pc, State s, int[] qMin, int[] qMax, List<uint> result, int depth)
    {
        if (depth > 4096)
            throw new InvalidOperationException("MOPP code nests too deep");

        while (true)
        {
            if (pc < 0 || pc >= c.Length)
                throw new InvalidOperationException($"MOPP code jumps outside to {pc}");

            var op = c[pc];
            int Min(int axis) => Byte(qMin[axis], axis == 0 ? s.OX : axis == 1 ? s.OY : s.OZ, s.Shift);
            int Max(int axis) => Byte(qMax[axis], axis == 0 ? s.OX : axis == 1 ? s.OY : s.OZ, s.Shift);

            switch (op)
            {
                case >= 0x01 and <= 0x04:
                {
                    var k = op;
                    s = new State(s.OX + (c[pc + 1] << s.Shift), s.OY + (c[pc + 2] << s.Shift), s.OZ + (c[pc + 3] << s.Shift), s.Shift - k, s.Base);
                    if (s.Shift < 0) throw new InvalidOperationException($"MOPP shift below 0 at {pc}");
                    pc += 4;
                    break;
                }
                case 0x05: pc += 2 + c[pc + 1]; break;
                case 0x06: pc += 3 + (c[pc + 1] << 8 | c[pc + 2]); break;
                case 0x07: pc += 4 + (c[pc + 1] << 16 | c[pc + 2] << 8 | c[pc + 3]); break;
                case 0x08: pc += 5 + (c[pc + 1] << 24 | c[pc + 2] << 16 | c[pc + 3] << 8 | c[pc + 4]); break;
                case 0x70:
                    // The region and base carry into the chunk (resetting either loses triangles in the games' codes)
                    pc = c[pc + 1] << 24 | c[pc + 2] << 16 | c[pc + 3] << 8 | c[pc + 4];
                    break;
                // Re-offsets add up: a 32-bit one gives the subpart, 8- and 16-bit ones step within it
                case 0x09: s = s with { Base = s.Base + c[pc + 1] }; pc += 2; break;
                case 0x0A: s = s with { Base = s.Base + (uint)(c[pc + 1] << 8 | c[pc + 2]) }; pc += 3; break;
                case 0x0B: s = s with { Base = s.Base + (uint)(c[pc + 1] << 24 | c[pc + 2] << 16 | c[pc + 3] << 8 | c[pc + 4]) }; pc += 5; break;
                case >= 0x10 and <= 0x1C:
                {
                    var end = pc + 4;
                    bool left, right;

                    if (op <= 0x12)
                    {
                        var axis = op - 0x10;
                        left = Min(axis) <= c[pc + 1];
                        right = Max(axis) >= c[pc + 2];
                    }
                    else
                    {
                        left = right = true;
                    }

                    if (left && right)
                    {
                        Walk(c, end + c[pc + 3], s, qMin, qMax, result, depth + 1);
                        pc = end;
                    }
                    else if (left) pc = end;
                    else if (right) pc = end + c[pc + 3];
                    else return;
                    break;
                }
                case >= 0x20 and <= 0x22:
                {
                    var axis = op - 0x20;
                    var end = pc + 3;
                    var left = Min(axis) <= c[pc + 1];
                    var right = Max(axis) >= c[pc + 1];

                    if (left && right)
                    {
                        Walk(c, end + c[pc + 2], s, qMin, qMax, result, depth + 1);
                        pc = end;
                    }
                    else if (left) pc = end;
                    else if (right) pc = end + c[pc + 2];
                    else return;
                    break;
                }
                case >= 0x23 and <= 0x25:
                {
                    var axis = op - 0x23;
                    var end = pc + 7;
                    var leftAt = end + (c[pc + 3] << 8 | c[pc + 4]);
                    var rightAt = end + (c[pc + 5] << 8 | c[pc + 6]);
                    var left = Min(axis) <= c[pc + 1];
                    var right = Max(axis) >= c[pc + 2];

                    if (left && right)
                    {
                        Walk(c, rightAt, s, qMin, qMax, result, depth + 1);
                        pc = leftAt;
                    }
                    else if (left) pc = leftAt;
                    else if (right) pc = rightAt;
                    else return;
                    break;
                }
                case >= 0x26 and <= 0x28:
                {
                    var axis = op - 0x26;

                    if (Max(axis) < c[pc + 1] || Min(axis) > c[pc + 2])
                        return;

                    pc += 3;
                    break;
                }
                case >= 0x30 and <= 0x4F: result.Add(s.Base + (uint)(op - 0x30)); return;
                case 0x50: result.Add(s.Base + c[pc + 1]); return;
                case 0x51: result.Add(s.Base + (uint)(c[pc + 1] << 8 | c[pc + 2])); return;
                case 0x52: result.Add(s.Base + (uint)(c[pc + 1] << 16 | c[pc + 2] << 8 | c[pc + 3])); return;
                default:
                    throw new InvalidOperationException($"MOPP opcode {op:X2} at {pc} isn't known");
            }
        }
    }
}
