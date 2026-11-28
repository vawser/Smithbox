#nullable enable
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>One clip's work as read from its work files: the clip (base frames with its keys) and its manifest entry</summary>
public sealed class AnimationWorkClip
{
    public string Name = "";

    /// <summary>For a clip added under a new ID: the existing clip it was made from (its anibnd, binding and settings)</summary>
    public string? TemplateName;

    /// <summary>Whether the anibnd had this work baked in when it was saved (Export since the last edit)</summary>
    public bool Exported;

    /// <summary>Null when the files couldn't be read (the entry is kept in the manifest, so the files aren't lost)</summary>
    public EditableClip? Clip;
}

/// <summary>
/// The Animation Editor's work files, kept beside the project in .smithbox/Animations/&lt;set&gt;/ (Animation Editor
/// Analysis.md §4.8). Save writes them; Export bakes the work into the anibnds and leaves them, so the keys can be
/// edited again and exported again.
/// <list type="bullet">
/// <item><c>Work.json</c>: the clips with work (and, for a new ID, the clip it was made from) and the clips deleted.</item>
/// <item><c>&lt;clip&gt;.keys.json</c>: the offset keys, by bone name, and their ease.</item>
/// <item><c>&lt;clip&gt;.base.bin</c>: the frames the keys are on (gzip), so whole-clip operations and Export don't
/// move them.</item>
/// </list>
/// </summary>
public static class AnimationWorkStore
{
    public const string ManifestName = "Work.json";
    private const string KeysSuffix = ".keys.json";
    private const string BaseSuffix = ".base.bin";
    private const string RootBoneName = "(root motion)";

    private static readonly byte[] Magic = "SBAW"u8.ToArray();
    private const int BaseVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>The work folder of a character's animation set (variants sharing a set share its work)</summary>
    public static string FolderFor(ProjectEntry project, string setName) =>
        Path.Join(project.Descriptor.ProjectPath, ".smithbox", "Animations", setName);

    // ---- reading

    /// <summary>The set's work, or null when it has none. Unreadable clips are logged and listed with no clip.</summary>
    public static (List<AnimationWorkClip> Clips, List<string> Deleted)? Load(string folder, HavokSkeletonData skeleton)
    {
        var manifestPath = Path.Join(folder, ManifestName);
        if (!File.Exists(manifestPath))
            return null;

        ManifestDto? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<ManifestDto>(File.ReadAllText(manifestPath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(AnimationWorkStore), LOC.Get("ANIM_Work_Load_Failed", manifestPath), e);
            return null;
        }

        if (manifest == null)
            return null;

        var clips = new List<AnimationWorkClip>();
        foreach (var entry in manifest.Clips)
        {
            var work = new AnimationWorkClip
            {
                Name = entry.Name,
                TemplateName = string.IsNullOrEmpty(entry.Template) ? null : entry.Template,
                Exported = entry.Exported
            };

            try
            {
                work.Clip = ReadClip(folder, entry.Name, skeleton);
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AnimationWorkStore), LOC.Get("ANIM_Work_Clip_Load_Failed", entry.Name, folder), e);
            }

            clips.Add(work);
        }

        return (clips, manifest.Deleted.ToList());
    }

    private static EditableClip ReadClip(string folder, string name, HavokSkeletonData skeleton)
    {
        EditableClip clip;
        using (var file = File.OpenRead(Path.Join(folder, name + BaseSuffix)))
        using (var gzip = new GZipStream(file, CompressionMode.Decompress))
        using (var reader = new BinaryReader(gzip))
            clip = ReadBase(reader, skeleton);

        var keysPath = Path.Join(folder, name + KeysSuffix);
        if (File.Exists(keysPath) && JsonSerializer.Deserialize<KeysDto>(File.ReadAllText(keysPath)) is { } keys)
            ApplyKeys(clip, keys, skeleton, name);

        return clip;
    }

    private static EditableClip ReadBase(BinaryReader r, HavokSkeletonData skeleton)
    {
        if (!r.ReadBytes(4).AsSpan().SequenceEqual(Magic))
            throw new InvalidDataException("Not an Animation Editor base file.");

        var version = r.ReadInt32();
        if (version != BaseVersion)
            throw new InvalidDataException($"Base file version {version} isn't supported.");

        var clip = new EditableClip
        {
            FrameDuration = r.ReadSingle(),
            BlendMode = (HavokBlendMode)r.ReadInt32(),
            TrackCount = r.ReadInt32(),
            FloatTrackCount = r.ReadInt32()
        };

        var frameCount = r.ReadInt32();
        if (frameCount < 2 || clip.TrackCount < 0 || clip.FloatTrackCount < 0)
            throw new InvalidDataException("The base file's frame or track count is out of range.");

        clip.TrackToBone = ReadShorts(r);
        clip.FloatTrackToSlot = ReadShorts(r);

        if (clip.TrackToBone.Any(b => b >= skeleton.BoneCount))
            throw new InvalidDataException("The base file's tracks are for another skeleton.");

        clip.Frames = new HavokTransformData[frameCount][];
        for (var f = 0; f < frameCount; f++)
        {
            var pose = new HavokTransformData[clip.TrackCount];
            for (var t = 0; t < clip.TrackCount; t++)
            {
                pose[t] = new HavokTransformData
                {
                    Translation = ReadVector3(r),
                    Rotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                    Scale = ReadVector3(r)
                };
            }

            clip.Frames[f] = pose;
        }

        clip.Floats = new float[frameCount][];
        for (var f = 0; f < frameCount; f++)
        {
            var floats = new float[clip.FloatTrackCount];
            for (var t = 0; t < clip.FloatTrackCount; t++)
                floats[t] = r.ReadSingle();
            clip.Floats[f] = floats;
        }

        clip.Up = ReadVector4(r);
        clip.Forward = ReadVector4(r);

        var rootCount = r.ReadInt32();
        if (rootCount > 0)
        {
            clip.RootMotion = new Vector4[rootCount];
            for (var i = 0; i < rootCount; i++)
                clip.RootMotion[i] = ReadVector4(r);
        }

        return clip;
    }

    /// <summary>
    /// The keys onto the clip's tracks: by bone name where the skeleton has it (so a key file still fits after the
    /// binding's order changes), else by the track it was saved on
    /// </summary>
    private static void ApplyKeys(EditableClip clip, KeysDto keys, HavokSkeletonData skeleton, string clipName)
    {
        clip.OffsetEase = Enum.TryParse<BoneOffsetEase>(keys.Ease, out var ease) ? ease : BoneOffsetEase.Smooth;

        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);

        // A track by bone name where the skeleton has it, else the track it was saved on; null when neither fits
        int? TrackOf(int saved, string boneName)
        {
            if (saved == EditableClip.RootTrack || boneName == RootBoneName)
                return EditableClip.RootTrack;

            var bone = Array.FindIndex(skeleton.Names, n => n.Equals(boneName, StringComparison.Ordinal));
            if (bone >= 0 && boneToTrack[bone] >= 0)
                return boneToTrack[bone];

            return saved >= 0 && saved < clip.TrackCount ? saved : null;
        }

        Dictionary<int, List<BoneOffsetKey>> Read(List<TrackDto> tracks)
        {
            var result = new Dictionary<int, List<BoneOffsetKey>>();
            foreach (var track in tracks)
            {
                if (TrackOf(track.Track, track.Bone) is not { } index)
                {
                    Smithbox.LogError(typeof(AnimationWorkStore), LOC.Get("ANIM_Work_Bone_Missing", track.Bone, clipName));
                    continue;
                }

                var list = track.Keys
                    .Select(k => new BoneOffsetKey(k.Frame, Quaternion.Normalize(Q(k.Rotation)), V(k.Translation),
                        Enum.TryParse<BoneOffsetEase>(k.Ease, out var own) ? own : null))
                    .GroupBy(k => k.Frame).Select(g => g.Last())
                    .OrderBy(k => k.Frame)
                    .ToList();

                if (list.Count > 0)
                    result[index] = list;
            }

            return result;
        }

        // Version 2: layers. Version 1: one set of tracks, read as one layer
        if (keys.Layers is { Count: > 0 } layers)
        {
            clip.Layers = layers.Select(l => new OffsetLayer
            {
                Name = string.IsNullOrWhiteSpace(l.Name) ? "Layer" : l.Name,
                Weight = l.Weight,
                Mute = l.Mute,
                Solo = l.Solo,
                Mask = l.Mask?.Select(n => TrackOf(int.MinValue, n)).OfType<int>().ToHashSet(),
                Keys = Read(l.Tracks)
            }).ToList();
            clip.ActiveLayer = Math.Clamp(keys.ActiveLayer, 0, clip.Layers.Count - 1);
        }
        else
        {
            clip.Layers = new List<OffsetLayer> { new() { Keys = Read(keys.Tracks ?? new()) } };
            clip.ActiveLayer = 0;
        }
    }

    // ---- writing

    /// <summary>Writes a clip's work files: its base frames (without keys) and its keys</summary>
    public static void WriteClip(string folder, string name, EditableClip clip, HavokSkeletonData skeleton)
    {
        Directory.CreateDirectory(folder);

        var basePath = Path.Join(folder, name + BaseSuffix);
        var tempPath = basePath + ".tmp";
        using (var file = File.Create(tempPath))
        using (var gzip = new GZipStream(file, CompressionLevel.Fastest))
        using (var writer = new BinaryWriter(gzip))
            WriteBase(writer, clip);
        File.Move(tempPath, basePath, true);

        string NameOf(int track) => track == EditableClip.RootTrack ? RootBoneName : BoneName(clip, skeleton, track);

        var keys = new KeysDto
        {
            Version = 2,
            Clip = name,
            FrameCount = clip.FrameCount,
            Ease = clip.OffsetEase.ToString(),
            ActiveLayer = clip.ActiveLayer,
            Layers = clip.Layers.Select(l => new LayerDto
            {
                Name = l.Name,
                Weight = l.Weight,
                Mute = l.Mute,
                Solo = l.Solo,
                Mask = l.Mask?.OrderBy(t => t).Select(NameOf).ToList(),
                Tracks = l.Keys.OrderBy(o => o.Key).Select(o => new TrackDto
                {
                    Track = o.Key,
                    Bone = NameOf(o.Key),
                    Keys = o.Value.Select(k => new KeyDto
                    {
                        Frame = k.Frame,
                        Rotation = [k.Rotation.X, k.Rotation.Y, k.Rotation.Z, k.Rotation.W],
                        Translation = [k.Translation.X, k.Translation.Y, k.Translation.Z],
                        Ease = k.Ease?.ToString()
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        File.WriteAllText(Path.Join(folder, name + KeysSuffix), JsonSerializer.Serialize(keys, JsonOptions));
    }

    private static string BoneName(EditableClip clip, HavokSkeletonData skeleton, int track)
    {
        var bone = clip.BoneOf(track);
        return bone >= 0 && bone < skeleton.BoneCount ? skeleton.Names[bone] : "";
    }

    private static void WriteBase(BinaryWriter w, EditableClip clip)
    {
        w.Write(Magic);
        w.Write(BaseVersion);
        w.Write(clip.FrameDuration);
        w.Write((int)clip.BlendMode);
        w.Write(clip.TrackCount);
        w.Write(clip.FloatTrackCount);
        w.Write(clip.FrameCount);
        WriteShorts(w, clip.TrackToBone);
        WriteShorts(w, clip.FloatTrackToSlot);

        foreach (var pose in clip.Frames)
        {
            for (var t = 0; t < clip.TrackCount; t++)
            {
                var x = pose[t];
                Write(w, x.Translation);
                w.Write(x.Rotation.X);
                w.Write(x.Rotation.Y);
                w.Write(x.Rotation.Z);
                w.Write(x.Rotation.W);
                Write(w, x.Scale);
            }
        }

        foreach (var floats in clip.Floats)
        {
            for (var t = 0; t < clip.FloatTrackCount; t++)
                w.Write(floats[t]);
        }

        Write(w, clip.Up);
        Write(w, clip.Forward);

        w.Write(clip.RootMotion?.Length ?? 0);
        if (clip.RootMotion != null)
        {
            foreach (var sample in clip.RootMotion)
                Write(w, sample);
        }
    }

    /// <summary>Takes a clip's work files out of the folder (its work was reverted, or the clip deleted)</summary>
    public static void RemoveClip(string folder, string name)
    {
        foreach (var path in new[] { Path.Join(folder, name + BaseSuffix), Path.Join(folder, name + KeysSuffix) })
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    /// <summary>Writes the manifest: the clips with work, in order, and the clips deleted (not exported yet)</summary>
    public static void WriteManifest(string folder, IEnumerable<AnimationWorkClip> clips, IEnumerable<string> deleted)
    {
        Directory.CreateDirectory(folder);
        var manifest = new ManifestDto
        {
            Clips = clips.Select(c => new ManifestClipDto
            {
                Name = c.Name,
                Template = c.TemplateName ?? "",
                Exported = c.Exported
            }).ToList(),
            Deleted = deleted.ToList()
        };

        File.WriteAllText(Path.Join(folder, ManifestName), JsonSerializer.Serialize(manifest, JsonOptions));
    }

    // ---- helpers

    private static short[] ReadShorts(BinaryReader r)
    {
        var count = r.ReadInt32();
        var result = new short[count];
        for (var i = 0; i < count; i++)
            result[i] = r.ReadInt16();
        return result;
    }

    private static void WriteShorts(BinaryWriter w, short[] values)
    {
        w.Write(values.Length);
        foreach (var v in values)
            w.Write(v);
    }

    private static Vector3 ReadVector3(BinaryReader r) => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    private static Vector4 ReadVector4(BinaryReader r) => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

    private static void Write(BinaryWriter w, Vector3 v)
    {
        w.Write(v.X);
        w.Write(v.Y);
        w.Write(v.Z);
    }

    private static void Write(BinaryWriter w, Vector4 v)
    {
        w.Write(v.X);
        w.Write(v.Y);
        w.Write(v.Z);
        w.Write(v.W);
    }

    private static Vector3 V(float[]? a) => a is { Length: >= 3 } ? new Vector3(a[0], a[1], a[2]) : Vector3.Zero;

    private static Quaternion Q(float[]? a) =>
        a is { Length: >= 4 } ? new Quaternion(a[0], a[1], a[2], a[3]) : Quaternion.Identity;

    private sealed class ManifestDto
    {
        public int Version { get; set; } = 1;
        public List<ManifestClipDto> Clips { get; set; } = new();
        public List<string> Deleted { get; set; } = new();
    }

    private sealed class ManifestClipDto
    {
        public string Name { get; set; } = "";
        public string Template { get; set; } = "";
        public bool Exported { get; set; }
    }

    private sealed class KeysDto
    {
        public int Version { get; set; } = 1;
        public string Clip { get; set; } = "";
        public int FrameCount { get; set; }
        public string Ease { get; set; } = "";

        // Version 1 only: the keys, before keys had layers
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<TrackDto>? Tracks { get; set; }

        // Version 2: the layers in order, and the one keying acts on
        public int ActiveLayer { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<LayerDto>? Layers { get; set; }
    }

    private sealed class LayerDto
    {
        public string Name { get; set; } = "";
        public float Weight { get; set; } = 1;
        public bool Mute { get; set; }
        public bool Solo { get; set; }

        // Bone names (and <root>) the layer acts on; left out for every bone
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Mask { get; set; }

        public List<TrackDto> Tracks { get; set; } = new();
    }

    private sealed class TrackDto
    {
        public int Track { get; set; }
        public string Bone { get; set; } = "";
        public List<KeyDto> Keys { get; set; } = new();
    }

    private sealed class KeyDto
    {
        public int Frame { get; set; }
        public float[] Rotation { get; set; } = [];
        public float[] Translation { get; set; } = [];

        // The key's own ease (Linear, Smooth, Hold); left out for a key that uses the clip's Ease, as every key
        // saved before keys had their own did
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Ease { get; set; }
    }
}
