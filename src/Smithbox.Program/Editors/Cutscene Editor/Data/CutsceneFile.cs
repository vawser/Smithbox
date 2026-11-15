#nullable enable
using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// One cutscene binder (cutscenebnd in Sekiro, ER, AC6 and NR; remobnd in DS3 and Bloodborne): the MQB that describes
/// the scene, and one Havok file per cut holding the skeletons and clips of its actors and camera. Read on first use.
/// </summary>
public class CutsceneFile
{
    public FileDictionaryEntry Entry;

    /// <summary>The cutscene's name, as EMEVD plays it: s10_00_0020</summary>
    public string Name;

    public MQB? Mqb { get; private set; }

    /// <summary>Why the file couldn't be read, null if it was (or hasn't been tried)</summary>
    public string? LoadError { get; private set; }

    public bool IsLoaded => Mqb != null;

    // The cut Havok files, by "cutNNNN/aNNNN" (lower case): the folder of the cut and the file's stem. An HKT resource
    // of the MQB names its file by the same two parts (...\cut0010\hkx\a0000.HKT).
    private readonly Dictionary<string, byte[]> HavokFiles = new(StringComparer.OrdinalIgnoreCase);

    // Each cut Havok file's DCX compression inside the binder (DCX_NONE if none), to write an edited one back alike
    private readonly Dictionary<string, DCX.Type> HavokCompression = new(StringComparer.OrdinalIgnoreCase);

    // The cut Havok files edited since the last save (the camera written back by camera authoring)
    private readonly HashSet<string> ModifiedHavok = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Bumped whenever a cut Havok file changes, so the viewport reads its cameras again</summary>
    public int HavokVersion { get; private set; }

    // The cut Havok files as they were read from the project, and as the game has them (read when first needed)
    private readonly Dictionary<string, byte[]> LoadedHavokFiles = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, byte[]>? VanillaHavokFiles;

    /// <summary>The camera keys of this cutscene, from its sidecar (<see cref="CutsceneCameraKeyStore" />)</summary>
    public CutsceneCameraKeyStore CameraKeys { get; private set; } = new();

    // The cameras whose keys were baked into the project's files (in the sidecar when read, or at the last save): one
    // that has no keys any more is put back as the game has it on the next save
    private readonly HashSet<string> BakedCameras = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Bumped whenever a camera's keys change, so the viewport plays them</summary>
    public int KeysVersion { get; private set; }

    /// <summary>"cutNNNN/aNNNN/camNNNN": a camera's id in the key store, from its HKT</summary>
    public static string KeyIdOf(MQB.Resource hkt, string cameraName) => $"{HavokKey(hkt.Path ?? "")}/{cameraName}";

    /// <summary>Gives a camera new keys, or takes them away (null). Use through <see cref="CutsceneCameraKeysAction" />.</summary>
    public void SetCameraKeys(string keyId, CutsceneCameraKeys? keys)
    {
        if (keys == null)
            CameraKeys.Cameras.Remove(keyId);
        else
            CameraKeys.Cameras[keyId] = keys;

        KeysVersion++;
    }

    /// <summary>
    /// The cut Havok file as the game has it, which camera keys apply to; the project's file as it was read if the
    /// game has no such cutscene.
    /// </summary>
    public byte[]? GetBaseHavokFile(ProjectEntry project, MQB.Resource hktResource)
    {
        var key = HavokKey(hktResource.Path ?? "");
        if (key == null)
            return null;

        VanillaHavokFiles ??= ReadVanillaHavokFiles(project);
        return VanillaHavokFiles.TryGetValue(key, out var vanilla) ? vanilla
            : LoadedHavokFiles.TryGetValue(key, out var loaded) ? loaded : null;
    }

    /// <summary>Whether the game has its own copy of this cutscene, for Offset keys to apply to</summary>
    public bool HasVanilla(ProjectEntry project)
    {
        VanillaHavokFiles ??= ReadVanillaHavokFiles(project);
        return VanillaHavokFiles.Count > 0;
    }

    private Dictionary<string, byte[]> ReadVanillaHavokFiles(ProjectEntry project)
    {
        var result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var raw = project.VFS.VanillaFS.ReadFile(Entry.Path);
            if (raw == null)
                return result;

            var data = DCX.Is(raw.Value) ? DCX.Decompress(raw.Value) : raw.Value;
            var files = data.Length >= 4 && data.Span[..4].SequenceEqual("BND4"u8) ? BND4.Read(data).Files : BND3.Read(data).Files;
            foreach (var file in files)
            {
                var name = (file.Name ?? "").Replace('\\', '/');
                if (!name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase) || HavokKey(name) is not { } key)
                    continue;

                result[key] = (DCX.Is(file.Bytes) ? DCX.Decompress(file.Bytes) : file.Bytes).ToArray();
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_Vanilla_Read_Failed", Entry.Path), e);
        }

        return result;
    }

    public CutsceneFile(FileDictionaryEntry entry)
    {
        Entry = entry;
        Name = entry.Filename;
    }

    private readonly object LoadLock = new();

    /// <summary>Reads the binder, once. Safe to call from worker threads.</summary>
    public bool Load(ProjectEntry project)
    {
        lock (LoadLock)
            return LoadCore(project);
    }

    private bool LoadCore(ProjectEntry project)
    {
        if (IsLoaded)
            return true;

        try
        {
            var raw = project.VFS.FS.ReadFile(Entry.Path);
            if (raw == null)
            {
                LoadError = $"{Entry.Path} could not be read.";
                return false;
            }

            var binder = ReadBinder(project, raw.Value, out var repaired);
            if (repaired)
            {
                // Written encrypted by an earlier save: marked changed, so the next save writes it readable
                IsDirty = true;
                Smithbox.LogError(this, LOC.Get("CUTSCENE_File_Repaired", Entry.Path));
            }

            foreach (var file in binder.Files)
            {
                var name = (file.Name ?? "").Replace('\\', '/');
                var compression = DCX.Type.None;
                var bytes = DCX.Is(file.Bytes) ? DCX.Decompress(file.Bytes, out compression) : file.Bytes;

                if (name.EndsWith(".mqb", StringComparison.OrdinalIgnoreCase))
                {
                    Mqb = MQB.Read(bytes);
                }
                else if (name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase))
                {
                    var key = HavokKey(name);
                    if (key != null)
                    {
                        HavokFiles[key] = bytes.ToArray();
                        LoadedHavokFiles[key] = HavokFiles[key];
                        HavokCompression[key] = compression;
                    }
                }
            }

            if (Mqb == null)
            {
                LoadError = $"{Entry.Path} has no MQB.";
                return false;
            }

            CameraKeys = CutsceneCameraKeyStore.Load(project, Name);
            BakedCameras.Clear();
            BakedCameras.UnionWith(CameraKeys.Cameras.Keys);

            LoadError = null;
            return true;
        }
        catch (Exception e)
        {
            LoadError = $"{Entry.Path}: {e.Message}";
            Smithbox.LogError(this, LOC.Get("CUTSCENE_File_Load_Failed", Entry.Path), e);
            return false;
        }
    }

    /// <summary>
    /// The Havok file an HKT resource names (its path ends cutNNNN\hkx\aNNNN.HKT), or null if the binder has none.
    /// </summary>
    public byte[]? GetHavokFile(MQB.Resource hktResource)
    {
        var key = HavokKey(hktResource.Path ?? "");
        return key != null && HavokFiles.TryGetValue(key, out var bytes) ? bytes : null;
    }

    public int HavokFileCount => HavokFiles.Count;

    /// <summary>
    /// Replaces the Havok file an HKT resource names (uncompressed bytes), to be written into the binder on save. Use
    /// through <see cref="CutsceneHavokEditAction" />, so it can be undone.
    /// </summary>
    public void SetHavokFile(MQB.Resource hktResource, byte[] bytes)
    {
        var key = HavokKey(hktResource.Path ?? "")
                  ?? throw new InvalidOperationException($"{hktResource.Name} names no cut Havok file.");
        if (!HavokFiles.ContainsKey(key))
            throw new InvalidOperationException($"The binder has no Havok file for {hktResource.Path}.");

        HavokFiles[key] = bytes;
        ModifiedHavok.Add(key);
        HavokVersion++;
    }

    /// <summary>
    /// Reads only the MQB of the binder, without keeping its Havok files: for indexing every cutscene. The file's own
    /// MQB if it is loaded, so its unsaved edits count.
    /// </summary>
    public MQB? ReadMqbOnly(ProjectEntry project)
    {
        if (Mqb != null)
            return Mqb;

        try
        {
            var raw = project.VFS.FS.ReadFile(Entry.Path);
            if (raw == null)
                return null;

            var files = ReadBinder(project, raw.Value, out _).Files;
            var mqb = files.FirstOrDefault(f => (f.Name ?? "").EndsWith(".mqb", StringComparison.OrdinalIgnoreCase));
            return mqb != null ? MQB.Read(DCX.Is(mqb.Bytes) ? DCX.Decompress(mqb.Bytes) : mqb.Bytes) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>The MQB or a cut Havok file has changes that aren't written to the project yet</summary>
    public bool IsDirty;

    /// <summary>
    /// Writes the MQB, and the cut Havok files that were edited, back into the project's copy of the binder, leaving
    /// its other files as they are. Returns false, with the error logged, if it couldn't.
    /// </summary>
    public bool Save(ProjectEntry project)
    {
        if (Mqb == null)
            return false;

        try
        {
            var raw = project.VFS.FS.ReadFile(Entry.Path)
                      ?? throw new InvalidOperationException($"{Entry.Path} could not be read.");
            var bytes = Mqb.Write();

            BakeCameras(project);

            var binder = ReadBinder(project, raw, out _);
            MqbFileOf(binder.Files).Bytes = bytes;
            PutHavokFiles(binder.Files);

            // Written as it is, with its own DCX compression, as the other editors write their binders.
            // ProjectUtils.WriteWithBackup isn't used: for DS3, ER, NR and AC6 it encrypts every BND4 as a regulation.bin.
            var output = binder switch
            {
                BND4 bnd4 => bnd4.Write(),
                BND3 bnd3 => bnd3.Write(),
                _ => throw new NotSupportedException($"{Entry.Path} is neither a BND3 nor a BND4.")
            };
            project.VFS.ProjectFS.WriteFile(Entry.Path, output);

            IsDirty = false;
            ModifiedHavok.Clear();

            // The keys stay with the project; a camera with neither keys nor a mode needs no entry
            foreach (var id in CameraKeys.Cameras.Where(p => !p.Value.Applies && p.Value.Keys.Count == 0).Select(p => p.Key).ToList())
                CameraKeys.Cameras.Remove(id);
            CameraKeys.Save(project, Name);

            BakedCameras.Clear();
            BakedCameras.UnionWith(CameraKeys.Cameras.Where(p => p.Value.Applies).Select(p => p.Key));
            Smithbox.Log(this, LOC.Get("CUTSCENE_Saved", Entry.Path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_Save_Failed", Entry.Path), e);
            return false;
        }
    }

    /// <summary>
    /// Bakes every camera with keys into its cut Havok file, starting from the game's own file so keys never stack on
    /// an earlier bake; a cut whose cameras were baked before and have no keys now gets the game's file back.
    /// </summary>
    private void BakeCameras(ProjectEntry project)
    {
        var ids = CameraKeys.Cameras.Keys.Union(BakedCameras, StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var group in ids.GroupBy(id => id[..Math.Max(0, id.LastIndexOf('/'))], StringComparer.OrdinalIgnoreCase))
        {
            var hkt = Mqb!.Resources.FirstOrDefault(r => r.Type == CutsceneResourceTypes.Animation
                                                         && string.Equals(HavokKey(r.Path ?? ""), group.Key, StringComparison.OrdinalIgnoreCase));
            var baseFile = hkt != null ? GetBaseHavokFile(project, hkt) : null;
            if (hkt == null || baseFile == null || !HavokFiles.TryGetValue(group.Key, out var current))
            {
                Smithbox.LogError(this, LOC.Get("CUTSCENE_Keys_No_Hkt", group.Key));
                continue;
            }

            var keyed = group
                .Select(id => (Name: id[(id.LastIndexOf('/') + 1)..], Keys: CameraKeys.Cameras.GetValueOrDefault(id)))
                .Where(x => x.Keys is { Applies: true })
                .Select(x => (x.Name, x.Keys!))
                .ToList();

            var bytes = baseFile;
            if (keyed.Count > 0)
            {
                var (baked, results) = CutsceneCameraEdit.Bake(project.Descriptor.ProjectType, baseFile, keyed);
                bytes = baked;
                foreach (var (camera, r) in results)
                {
                    var text = r.Format == StudioCore.Utilities.HavokClipFormat.Spline
                        ? LOC.Get("CUTSCENE_Camera_Written", camera, LOC.Get("CUTSCENE_Camera_Format_Spline"), r.DataBytes, r.MaxFramesPerBlock, r.WorstErrorRatio)
                        : LOC.Get("CUTSCENE_Camera_Written_Interleaved", camera, r.DataBytes);
                    Smithbox.Log(this, $"{Name} {group.Key}: {text}");
                }
            }

            if (!current.AsSpan().SequenceEqual(bytes))
                SetHavokFile(hkt, bytes);
        }
    }

    // The edited cut Havok files, in their binder entries, compressed as they were
    private void PutHavokFiles(List<BinderFile> files)
    {
        foreach (var key in ModifiedHavok)
        {
            var file = files.FirstOrDefault(f => (f.Name ?? "").EndsWith(".hkx", StringComparison.OrdinalIgnoreCase)
                                                 && string.Equals(HavokKey(f.Name!), key, StringComparison.OrdinalIgnoreCase))
                       ?? throw new InvalidOperationException($"The binder has no Havok file {key}.");

            var bytes = HavokFiles[key];
            var compression = HavokCompression.GetValueOrDefault(key, DCX.Type.None);
            file.Bytes = compression == DCX.Type.None ? bytes : DCX.Compress(bytes, compression);
        }
    }

    /// <summary>
    /// The cutscene binder, BND4 or BND3, DCX-compressed or not. Before this fix, saves wrote DS3, ER, NR and AC6
    /// binders encrypted like a regulation.bin (ProjectUtils.WriteWithBackup does that to every BND4 of those games):
    /// such a file is decrypted with its game's regulation key, and <paramref name="repaired" /> says so.
    /// </summary>
    private static IBinder ReadBinder(ProjectEntry project, Memory<byte> raw, out bool repaired)
    {
        repaired = false;
        var data = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
        if (data.Length >= 4 && data.Span[..4].SequenceEqual("BND4"u8))
            return BND4.Read(raw);
        if (data.Length >= 4 && data.Span[..4].SequenceEqual("BND3"u8))
            return BND3.Read(raw);

        var bytes = raw.ToArray();
        BND4? decrypted = project.Descriptor.ProjectType switch
        {
            ProjectType.DS3 => SFUtil.DecryptDS3Regulation(bytes),
            ProjectType.ER => SFUtil.DecryptERRegulation(bytes),
            ProjectType.NR => SFUtil.DecryptNightreignRegulation(bytes),
            ProjectType.AC6 => SFUtil.DecryptAC6Regulation(bytes),
            _ => null
        };

        repaired = decrypted != null;
        return decrypted ?? throw new InvalidDataException("The file is neither a BND3 nor a BND4.");
    }

    private static BinderFile MqbFileOf(List<BinderFile> files) =>
        files.FirstOrDefault(f => (f.Name ?? "").EndsWith(".mqb", StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException("The binder has no MQB.");

    // "cut0010/a0000" from N:\GR\data\INTERROOT_win64\cutscene\s10_00_0020\cut0010\a0000.hkx or from
    // N:\GR\data\Model\cutscene\scn\s10_00_0020\cut0010\hkx\a0000.HKT: the nearest folder named cut*, and the stem
    private static string? HavokKey(string path)
    {
        var parts = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return null;

        var stem = Path.GetFileNameWithoutExtension(parts[^1]);
        var cut = parts.Take(parts.Length - 1).LastOrDefault(p => p.StartsWith("cut", StringComparison.OrdinalIgnoreCase));

        return cut == null ? null : $"{cut}/{stem}".ToLowerInvariant();
    }
}
