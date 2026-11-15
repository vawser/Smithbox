using System.Buffers.Binary;
using System.Numerics;

namespace Havok.Shared;

/// <summary>
/// Decodes hkaQuantizedAnimation (Havok 2014+; Sekiro and DS3 cutscenes) into whole poses. There is no public
/// description of the format: the layout below was worked out from the game files and is checked by
/// Claude/Tools/HavokTrackProbe, which predicts every section from the header for all SDT and DS3 cutscene clips.
/// <code>
/// header (0x28 bytes, u16 unless noted)
///   0x00 header size (where frame 0 starts)   0x02 transform tracks   0x04 float tracks   0x06 frames
///   0x08 f32 duration
///   0x0C static scalars   0x0E static rotations   0x10 (0)   0x12 static floats
///   0x14 dynamic scalars  0x16 dynamic rotations  0x18 (0)   0x1A dynamic floats
///   0x1C frame stride     0x1E..0x26 the ends of sections 0-4 (section 5 ends at the header size)
/// section 0  one byte per transform track (0xFF), to 2 bytes
/// section 1  u16 indices of the static scalars, rotations, floats
/// section 2  u16 indices of the dynamic scalars, rotations, floats, to 16
/// section 3  static values: f32 per scalar, 48-bit quaternion per rotation, f32 per float, to 16
/// section 4  f32 minimum per dynamic scalar, to 16, then per dynamic float, to 16
/// section 5  f32 step per dynamic scalar, to 16, then per dynamic float, to 16
/// frames     u16 per dynamic scalar, 48-bit quaternion per dynamic rotation, u16 per dynamic float, to 16
/// </code>
/// A scalar index is a float of the track's hkQsTransform (track × 12 + 0-2 translation, 4-7 rotation, 8-10 scale), a
/// rotation index the transform's vec4 (track × 3 + 1), a float index the float track. A dynamic value is
/// minimum + u16 × step. Components no index names keep the reference pose.
/// </summary>
public static class HavokQuantizedAnimation2014Codec
{
    public const int HeaderSize = 0x28;

    public sealed class Layout
    {
        public int DataStart;
        public int TransformTracks;
        public int FloatTracks;
        public int Frames;
        public float Duration;
        public int StaticScalars, StaticRotations, StaticFloats;
        public int DynamicScalars, DynamicRotations, DynamicFloats;
        public int Stride;
        public int[] SectionEnds = new int[6];
    }

    public static Layout ReadLayout(byte[] data)
    {
        if (data.Length < HeaderSize)
            throw new InvalidDataException("hkaQuantizedAnimation data is shorter than its header.");

        ushort U16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset));

        Layout layout = new()
        {
            DataStart = U16(0x00),
            TransformTracks = U16(0x02),
            FloatTracks = U16(0x04),
            Frames = U16(0x06),
            Duration = BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(0x08)),
            StaticScalars = U16(0x0C),
            StaticRotations = U16(0x0E),
            StaticFloats = U16(0x12),
            DynamicScalars = U16(0x14),
            DynamicRotations = U16(0x16),
            DynamicFloats = U16(0x1A),
            Stride = U16(0x1C)
        };

        for (int i = 0; i < 5; i++)
            layout.SectionEnds[i] = U16(0x1E + i * 2);
        layout.SectionEnds[5] = layout.DataStart;

        return layout;
    }

    /// <summary>
    /// Frame-major poses (frame f's track t at f × tracks + t) and floats (frame f's float track t at f × float tracks + t).
    /// </summary>
    /// <param name="referencePose">Each transform track's reference pose, for the components the clip doesn't store.</param>
    public static (HavokQsTransform[] Poses, float[] Floats) Decode(byte[] data,
        Func<int, HavokQsTransform>? referencePose = null)
    {
        Layout l = ReadLayout(data);
        int tracks = l.TransformTracks, frames = l.Frames, floatTracks = l.FloatTracks;

        // The pose every frame starts from: the reference pose with the static values over it
        var basePose = new float[tracks * 12];
        for (int t = 0; t < tracks; t++)
        {
            HavokQsTransform r = referencePose?.Invoke(t) ?? new HavokQsTransform(Vector3.Zero, Quaternion.Identity, Vector3.One);
            Write(basePose, t, r);
        }

        var baseFloats = new float[floatTracks];

        ushort U16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset));
        float F32(int offset) => BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(offset));
        static int Align16(int x) => (x + 15) & ~15;

        // Section 1: static indices; section 3: their values
        int indices = l.SectionEnds[0];
        int values = l.SectionEnds[2];
        for (int i = 0; i < l.StaticScalars; i++, indices += 2, values += 4)
            SetScalar(basePose, U16(indices), F32(values));
        for (int i = 0; i < l.StaticRotations; i++, indices += 2, values += 6)
            SetRotation(basePose, U16(indices), ReadQuat48(data.AsSpan(values)));
        for (int i = 0; i < l.StaticFloats; i++, indices += 2, values += 4)
        {
            int slot = U16(indices);
            if (slot < floatTracks) baseFloats[slot] = F32(values);
        }

        // Section 2: dynamic indices; sections 4 and 5: the minimum and step of each dynamic scalar and float
        int dynamic = l.SectionEnds[1];
        var scalarIndex = new int[l.DynamicScalars];
        var rotationIndex = new int[l.DynamicRotations];
        var floatIndex = new int[l.DynamicFloats];
        for (int i = 0; i < scalarIndex.Length; i++, dynamic += 2) scalarIndex[i] = U16(dynamic);
        for (int i = 0; i < rotationIndex.Length; i++, dynamic += 2) rotationIndex[i] = U16(dynamic);
        for (int i = 0; i < floatIndex.Length; i++, dynamic += 2) floatIndex[i] = U16(dynamic);

        int mins = l.SectionEnds[3], steps = l.SectionEnds[4];
        int floatMins = Align16(mins + 4 * l.DynamicScalars), floatSteps = Align16(steps + 4 * l.DynamicScalars);

        var poses = new HavokQsTransform[frames * tracks];
        var floats = new float[frames * floatTracks];
        var pose = new float[basePose.Length];

        for (int f = 0; f < frames; f++)
        {
            Array.Copy(basePose, pose, pose.Length);
            int o = l.DataStart + f * l.Stride;

            for (int i = 0; i < scalarIndex.Length; i++, o += 2)
                SetScalar(pose, scalarIndex[i], F32(mins + i * 4) + U16(o) * F32(steps + i * 4));
            for (int i = 0; i < rotationIndex.Length; i++, o += 6)
                SetRotation(pose, rotationIndex[i], ReadQuat48(data.AsSpan(o)));

            for (int t = 0; t < tracks; t++)
                poses[f * tracks + t] = Read(pose, t);

            for (int t = 0; t < floatTracks; t++)
                floats[f * floatTracks + t] = baseFloats[t];
            for (int i = 0; i < floatIndex.Length; i++, o += 2)
            {
                if (floatIndex[i] < floatTracks)
                    floats[f * floatTracks + floatIndex[i]] = F32(floatMins + i * 4) + U16(o) * F32(floatSteps + i * 4);
            }
        }

        return (poses, floats);
    }

    private static void SetScalar(float[] pose, int index, float value)
    {
        if (index >= 0 && index < pose.Length) pose[index] = value;
    }

    private static void SetRotation(float[] pose, int vec4Index, Quaternion q)
    {
        int at = vec4Index * 4;
        if (at < 0 || at + 4 > pose.Length) return;
        pose[at] = q.X;
        pose[at + 1] = q.Y;
        pose[at + 2] = q.Z;
        pose[at + 3] = q.W;
    }

    private static void Write(float[] pose, int track, HavokQsTransform t)
    {
        int b = track * 12;
        pose[b] = t.Translation.X; pose[b + 1] = t.Translation.Y; pose[b + 2] = t.Translation.Z; pose[b + 3] = 0;
        pose[b + 4] = t.Rotation.X; pose[b + 5] = t.Rotation.Y; pose[b + 6] = t.Rotation.Z; pose[b + 7] = t.Rotation.W;
        pose[b + 8] = t.Scale.X; pose[b + 9] = t.Scale.Y; pose[b + 10] = t.Scale.Z; pose[b + 11] = 0;
    }

    private static HavokQsTransform Read(float[] pose, int track)
    {
        int b = track * 12;
        var q = new Quaternion(pose[b + 4], pose[b + 5], pose[b + 6], pose[b + 7]);
        // Rotations come whole from 48-bit quaternions, but a scalar index could name one component
        float length = q.Length();
        if (length > 1e-6f && MathF.Abs(length - 1) > 1e-4f) q = Quaternion.Normalize(q);
        return new HavokQsTransform(new Vector3(pose[b], pose[b + 1], pose[b + 2]), q,
            new Vector3(pose[b + 8], pose[b + 9], pose[b + 10]));
    }

    /// <summary>
    /// Havok's THREECOMP48: three 15-bit components and the index of the dropped (largest) one, which is rebuilt from
    /// unit length, in the top bits of the first two; the sign of the dropped one in the top bit of the third.
    /// </summary>
    public static Quaternion ReadQuat48(ReadOnlySpan<byte> data)
    {
        const int mask = (1 << 15) - 1;
        const float fractal = 0.000043161f;

        ushort x = BinaryPrimitives.ReadUInt16LittleEndian(data);
        ushort y = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
        ushort z = BinaryPrimitives.ReadUInt16LittleEndian(data[4..]);

        int dropped = ((y >> 14) & 2) | ((x >> 15) & 1);
        bool negative = (z >> 15) != 0;

        Span<float> kept = stackalloc float[3];
        kept[0] = ((x & mask) - (mask >> 1)) * fractal;
        kept[1] = ((y & mask) - (mask >> 1)) * fractal;
        kept[2] = ((z & mask) - (mask >> 1)) * fractal;

        Span<float> q = stackalloc float[4];
        for (int i = 0, k = 0; i < 4; i++)
        {
            if (i != dropped) q[i] = kept[k++];
        }

        float w = 1 - kept[0] * kept[0] - kept[1] * kept[1] - kept[2] * kept[2];
        q[dropped] = w > 0 ? MathF.Sqrt(w) * (negative ? -1 : 1) : 0;

        return new Quaternion(q[0], q[1], q[2], q[3]);
    }
}
