using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;
using Andre.Core;
using SoulsFormats;

/// <summary>
/// Phase 5's acceptance test: for the 32-bit packfiles (DS1, DeS), every object SoulsFormats decodes into a class of its
/// own (skeletons, animations, bindings, collision mesh storage, MOPP code) is compared, value by value, with the object
/// the unified engine read at the same data-section offset. SoulsFormats reads every object in the section; the engine
/// only those reachable from the root, so an object it didn't reach is counted apart.
/// </summary>
public static class DecodeCompare
{
    private sealed class Tally
    {
        public int Compared, Equal, NotReached, SoulsFormatsErrors;
        public readonly ConcurrentQueue<string> Examples = new();
    }

    private static readonly ConcurrentDictionary<string, Tally> Tallies = new();
    private static readonly ConcurrentDictionary<string, string> Errors = new();

    public static void Reset()
    {
        Tallies.Clear();
        Errors.Clear();
    }

    public static void Collect(byte[] data, SniffInfo sniff, Game game, string inner)
    {
        if (sniff.Container != "Packfile" || sniff.PointerSize != 4 || game is not (Game.DS1 or Game.DES)) return;
        if (!(sniff.Sdk.StartsWith("hk_2010") || sniff.Sdk.StartsWith("Havok-5.5") || sniff.Sdk.StartsWith("Havok-5.1"))) return;

        HKX hkx;
        try { hkx = HKX.Read(data, game == Game.DES ? HKX.HKXVariation.HKXDeS : HKX.HKXVariation.HKXDS1); }
        catch { return; } // SoulsFormats can't read it: nothing to compare with

        IReadOnlyDictionary<uint, object> objects;
        try
        {
            var profile = Havok.HavokProfile.For(game.ToString(), sniff.Sdk)!;
            var serializer = new Havok.Formats.Packfile.HavokPackfileSerializer(profile.Sdk == Havok.HavokSdk.Hk2010
                ? Havok.hk2010.ClassSet.For(profile)
                : Havok.hk55.ClassSet.For(profile));
            serializer.Read(data);
            objects = serializer.LastObjects;
        }
        catch (Exception ex)
        {
            Errors.TryAdd($"{ex.GetType().Name}: {ex.Message}", inner);
            return;
        }

        foreach (var sf in hkx.DataSection.Objects)
        {
            string? kind = sf switch
            {
                HKX.HKASkeleton => "skeleton",
                HKX.HKASplineCompressedAnimation => "spline animation",
                HKX.HKAInterleavedUncompressedAnimation => "interleaved animation",
                HKX.HKAAnimationBinding => "animation binding",
                HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage => "collision mesh storage",
                HKX.hkpMoppCode => "MOPP code",
                _ => null
            };
            if (kind == null) continue;

            var tally = Tallies.GetOrAdd(kind, _ => new Tally());
            if (!objects.TryGetValue(sf.SectionOffset, out object? ours))
            {
                Interlocked.Increment(ref tally.NotReached);
                if (tally.Examples.Count < 6) tally.Examples.Enqueue($"`{inner}` @0x{sf.SectionOffset:X}: not reached from the root");
                continue;
            }

            var diffs = new List<string>();
            try { Compare(sf, ours, diffs); }
            catch (Exception ex) { diffs.Add($"{ex.GetType().Name}: {ex.Message}"); }
            Interlocked.Increment(ref tally.Compared);
            if (diffs.Remove(SoulsFormatsError)) Interlocked.Increment(ref tally.SoulsFormatsErrors);
            if (diffs.Count == 0) Interlocked.Increment(ref tally.Equal);
            else if (tally.Examples.Count < 6)
                tally.Examples.Enqueue($"`{inner}` @0x{sf.SectionOffset:X} ({ours.GetType().Name}): {string.Join("; ", diffs.Take(3))}");
        }
    }

    // ---- per class ------------------------------------------------------------------------------------------------

    private static void Compare(HKX.HKXObject sf, object ours, List<string> d)
    {
        switch (sf)
        {
            case HKX.HKASkeleton s:
                Name(d, "name", s.Name?.GetString(), Get(ours, "m_name"));
                Seq(d, "parentIndices", Elements(s.ParentIndices).Select(x => (object)x.data), List(ours, "m_parentIndices"));
                var bones = Elements(s.Bones).ToList();
                var ourBones = List(ours, "m_bones").ToList();
                Same(d, "bones.Count", bones.Count, ourBones.Count);
                for (int i = 0; i < Math.Min(bones.Count, ourBones.Count) && d.Count < 3; i++)
                {
                    Name(d, $"bones[{i}].name", bones[i].Name?.GetString(), Get(ourBones[i]!, "m_name"));
                    Same(d, $"bones[{i}].lockTranslation", bones[i].LockTranslation != 0, Get(ourBones[i]!, "m_lockTranslation"));
                }
                var pose = Elements(s.Transforms).ToList();
                var ourPose = List(ours, "m_referencePose").ToList();
                Same(d, "referencePose.Count", pose.Count, ourPose.Count);
                for (int i = 0; i < Math.Min(pose.Count, ourPose.Count) && d.Count < 3; i++)
                {
                    Same(d, $"referencePose[{i}].translation", pose[i].Position.Vector, Get(ourPose[i]!, "m_translation"));
                    Same(d, $"referencePose[{i}].rotation", pose[i].Rotation.Vector, AsVector(Get(ourPose[i]!, "m_rotation")));
                    Same(d, $"referencePose[{i}].scale", pose[i].Scale.Vector, Get(ourPose[i]!, "m_scale"));
                }
                break;

            case HKX.HKASplineCompressedAnimation a:
                Same(d, "duration", a.Duration, Get(ours, "m_duration"));
                Same(d, "numberOfTransformTracks", a.TransformTrackCount, Get(ours, "m_numberOfTransformTracks"));
                Same(d, "numberOfFloatTracks", a.FloatTrackCount, Get(ours, "m_numberOfFloatTracks"));
                Same(d, "numFrames", a.FrameCount, Get(ours, "m_numFrames"));
                Same(d, "numBlocks", a.BlockCount, Get(ours, "m_numBlocks"));
                Same(d, "maxFramesPerBlock", a.FramesPerBlock, Get(ours, "m_maxFramesPerBlock"));
                Same(d, "maskAndQuantizationSize", (int)a.MaskAndQuantization, Get(ours, "m_maskAndQuantizationSize"));
                Same(d, "blockDuration", a.BlockDuration, Get(ours, "m_blockDuration"));
                Same(d, "blockInverseDuration", a.InverseBlockDuration, Get(ours, "m_blockInverseDuration"));
                Same(d, "frameDuration", a.FrameDuration, Get(ours, "m_frameDuration"));
                Seq(d, "blockOffsets", Elements(a.BlockOffsets).Select(x => (object)x.data), List(ours, "m_blockOffsets"));
                Seq(d, "floatBlockOffsets", Elements(a.FloatBlockOffsets).Select(x => (object)x.data), List(ours, "m_floatBlockOffsets"));
                Seq(d, "transformOffsets", Elements(a.TransformBlockOffsets).Select(x => (object)x.data), List(ours, "m_transformOffsets"));
                Seq(d, "floatOffsets", Elements(a.FloatOffsets).Select(x => (object)x.data), List(ours, "m_floatOffsets"));
                Seq(d, "data", Elements(a.Data).Select(x => (object)x.data), List(ours, "m_data"));
                Same(d, "endian", a.Endian, Get(ours, "m_endian"));
                break;

            case HKX.HKAInterleavedUncompressedAnimation a:
                Same(d, "duration", a.Duration, Get(ours, "m_duration"));
                Same(d, "numberOfTransformTracks", a.TransformTrackCount, Get(ours, "m_numberOfTransformTracks"));
                var transforms = Elements(a.Transforms).ToList();
                var ourTransforms = List(ours, "m_transforms").ToList();
                Same(d, "transforms.Count", transforms.Count, ourTransforms.Count);
                for (int i = 0; i < Math.Min(transforms.Count, ourTransforms.Count) && d.Count < 3; i++)
                {
                    Same(d, $"transforms[{i}].translation", transforms[i].Position.Vector, Get(ourTransforms[i]!, "m_translation"));
                    Same(d, $"transforms[{i}].rotation", transforms[i].Rotation.Vector, AsVector(Get(ourTransforms[i]!, "m_rotation")));
                }
                Seq(d, "floats", Elements(a.Floats).Select(x => (object)x.data), List(ours, "m_floats"));
                break;

            case HKX.HKAAnimationBinding b:
                Seq(d, "transformTrackToBoneIndices", Elements(b.TransformTrackToBoneIndices).Select(x => (object)x.data),
                    List(ours, "m_transformTrackToBoneIndices"));
                Seq(d, "floatTrackToFloatSlotIndices", Elements(b.FloatTrackToFloatSlotIndices).Select(x => (object)x.data),
                    List(ours, "m_floatTrackToFloatSlotIndices"));
                // SoulsFormats' DS1 path reads the name inline after blendHint, where only padding is: the name is the string
                // the binding's first member points at (a local fixup), which is what the engine reads
                if (Has(ours, "m_originalSkeletonName"))
                {
                    if (string.IsNullOrEmpty(b.OriginalSkeletonName) && Get(ours, "m_originalSkeletonName") is string { Length: > 0 })
                        d.Add(SoulsFormatsError);
                    else Same(d, "originalSkeletonName", b.OriginalSkeletonName, Get(ours, "m_originalSkeletonName"));
                }
                break;

            case HKX.HKPStorageExtendedMeshShapeMeshSubpartStorage m:
                Seq(d, "vertices", Elements(m.Vertices).Select(x => (object)x.Vector), List(ours, "m_vertices"));
                Seq(d, "indices16", Elements(m.Indices16).Select(x => (object)x.data), List(ours, "m_indices16"));
                Seq(d, "indices32", Elements(m.Indices32).Select(x => (object)x.data), List(ours, "m_indices32"));
                Seq(d, "materialIndices", Elements(m.MaterialIndices).Select(x => (object)x.data), List(ours, "m_materialIndices"));
                // SoulsFormats reads materials as 32-bit words: each one's filterInfo (DeS: the whole hkpMeshMaterial)
                var materials = List(ours, "m_materials").ToList();
                Same(d, "materials.Count", Elements(m.Materials).Count(), materials.Count);
                break;

            case HKX.hkpMoppCode c:
                Seq(d, "data", Elements(c.MoppCode).Select(x => (object)x.data), List(ours, "m_data"));
                Same(d, "info.offset", new Vector4(c.Unk10, c.Unk14, c.Unk18, c.Unk1C), Get(Get(ours, "m_info")!, "m_offset"));
                break;
        }
    }

    // a difference that is SoulsFormats' mistake (kept apart in the tally)
    private const string SoulsFormatsError = "sf-error";

    // ---- helpers --------------------------------------------------------------------------------------------------

    private static IEnumerable<T> Elements<T>(HKX.HKArray<T> array) where T : HKX.IHKXSerializable, new() =>
        array?.GetArrayData()?.Elements ?? Enumerable.Empty<T>();

    private static bool Has(object o, string field) => o.GetType().GetField(field) != null;

    private static object? Get(object o, string field) =>
        (o.GetType().GetField(field) ?? throw new MissingFieldException(o.GetType().Name, field)).GetValue(o);

    private static IEnumerable<object?> List(object o, string field) =>
        Get(o, field) is IEnumerable e ? e.Cast<object?>() : Enumerable.Empty<object?>();

    private static object? AsVector(object? q) => q is Quaternion r ? new Vector4(r.X, r.Y, r.Z, r.W) : q;

    // SoulsFormats reads these names as ASCII: a Shift-JIS name comes back with each non-ASCII byte as '?'
    private static readonly System.Text.Encoding ShiftJis = GetShiftJis();
    private static System.Text.Encoding GetShiftJis()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        return System.Text.Encoding.GetEncoding(932);
    }

    private static void Name(List<string> d, string what, string? theirs, object? ours)
    {
        if (theirs != null && ours is string s && theirs != s && System.Text.Encoding.ASCII.GetString(ShiftJis.GetBytes(s)) == theirs)
            d.Add(SoulsFormatsError);
        else Same(d, what, theirs, ours);
    }

    private static void Same(List<string> d, string what, object? theirs, object? ours)
    {
        if (!Equal(theirs, ours)) d.Add($"{what}: SoulsFormats {Show(theirs)} vs engine {Show(ours)}");
    }

    private static void Seq(List<string> d, string what, IEnumerable<object?> theirs, IEnumerable<object?> ours)
    {
        var a = theirs.ToList();
        var b = ours.ToList();
        if (a.Count != b.Count) { d.Add($"{what}: {a.Count} vs {b.Count} elements"); return; }
        for (int i = 0; i < a.Count; i++)
            if (!Equal(a[i], b[i])) { d.Add($"{what}[{i}]: SoulsFormats {Show(a[i])} vs engine {Show(b[i])}"); return; }
    }

    // bit-exact for floats (both readers decode the same bytes), numeric across integer widths and enums
    private static bool Equal(object? a, object? b)
    {
        if (a is null || b is null) return (a ?? "") .Equals(b ?? "") || (a is null && b is string { Length: 0 }) || (b is null && a is string { Length: 0 });
        if (a is float fa && b is float fb) return BitConverter.SingleToInt32Bits(fa) == BitConverter.SingleToInt32Bits(fb);
        if (a is Vector4 va && b is Vector4 vb)
            return Equal(va.X, vb.X) && Equal(va.Y, vb.Y) && Equal(va.Z, vb.Z) && Equal(va.W, vb.W);
        if (a is bool || b is bool) return Convert.ToBoolean(a) == Convert.ToBoolean(b);
        if (IsInteger(a) && IsInteger(b)) return Convert.ToInt64(a) == Convert.ToInt64(b) || unchecked((uint)Convert.ToInt64(a) == (uint)Convert.ToInt64(b));
        return a.Equals(b);
    }

    private static bool IsInteger(object o) => o is sbyte or byte or short or ushort or int or uint or long or ulong || o.GetType().IsEnum;

    private static string Show(object? o) => o switch
    {
        null => "null",
        string s => $"\"{s}\"",
        Vector4 v => $"({v.X}, {v.Y}, {v.Z}, {v.W})",
        _ => o.ToString() ?? ""
    };

    public static List<string> Report()
    {
        var lines = new List<string>();
        if (Tallies.IsEmpty && Errors.IsEmpty) return lines;
        lines.Add("");
        lines.Add("## Decode comparison with SoulsFormats");
        lines.Add("");
        lines.Add("Every object SoulsFormats decodes into a class of its own, against the unified engine's object at the same offset (value by value, floats bit-exact).");
        lines.Add("");
        lines.Add("| Class | Compared | Equal | Not reached by the engine | Of the equal: SoulsFormats wrong |");
        lines.Add("|---|---|---|---|---|");
        foreach (var (kind, t) in Tallies.OrderBy(x => x.Key))
            lines.Add($"| {kind} | {t.Compared} | {t.Equal} | {t.NotReached} | {t.SoulsFormatsErrors} |");
        lines.Add("");
        lines.Add("SoulsFormats wrong: DS1 animation bindings, whose `originalSkeletonName` SoulsFormats reads inline after `blendHint` (it gets the padding, \"\"); the name is the string the binding's first member points at, which the engine reads. Skeletons: Shift-JIS bone names, which SoulsFormats decodes as ASCII (`?G?N?X?g` for `エクストラ`).");
        lines.Add("");
        foreach (var (kind, t) in Tallies.OrderBy(x => x.Key))
            foreach (var e in t.Examples) lines.Add($"- {kind}: {e}");
        foreach (var (error, inner) in Errors.Take(5)) lines.Add($"- engine read failed: {error} (e.g. `{inner}`)");
        return lines;
    }
}
