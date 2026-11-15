#nullable enable
using SoulsFormats;
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

            var data = DCX.Is(raw.Value) ? DCX.Decompress(raw.Value) : raw.Value;
            List<BinderFile> files = data.Length >= 4 && data.Span[..4].SequenceEqual("BND4"u8)
                ? BND4.Read(data).Files
                : BND3.Read(data).Files;

            foreach (var file in files)
            {
                var name = (file.Name ?? "").Replace('\\', '/');
                var bytes = DCX.Is(file.Bytes) ? DCX.Decompress(file.Bytes) : file.Bytes;

                if (name.EndsWith(".mqb", StringComparison.OrdinalIgnoreCase))
                {
                    Mqb = MQB.Read(bytes);
                }
                else if (name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase))
                {
                    var key = HavokKey(name);
                    if (key != null)
                        HavokFiles[key] = bytes.ToArray();
                }
            }

            if (Mqb == null)
            {
                LoadError = $"{Entry.Path} has no MQB.";
                return false;
            }

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
