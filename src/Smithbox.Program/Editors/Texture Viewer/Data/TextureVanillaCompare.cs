#nullable enable annotations
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The TPFs inside a texture container, and the textures inside a TPF, against the game's: for the TPF list and the
/// texture list colours. Works on the saved files (unsaved edits are the yellow "*" marker): every item of an added
/// container is Added, of an unchanged one Unchanged; in a modified container, a TPF is Added when the game's
/// container lacks it and Modified when its bytes differ, and a texture likewise by its name and bytes.
/// Both containers are read and indexed in the background, once per container; a write to the project's copy (any
/// ProjectFS write) drops that container's index. Plan: Claude/Documentation/Vanilla Modified-Added Parity.md.
/// </summary>
public class TextureVanillaCompare
{
    private readonly ProjectEntry Project;

    // A container's TPFs by name: the TPF's bytes and its textures' bytes by name
    private sealed class TpfIndex
    {
        public byte[] Bytes;
        public Dictionary<string, byte[]> Textures;
    }

    private sealed class ContainerCompare
    {
        public Dictionary<string, TpfIndex> Saved;
        public Dictionary<string, TpfIndex> Vanilla;
    }

    // Null while the compare runs
    private readonly ConcurrentDictionary<string, ContainerCompare?> Compares = new(StringComparer.OrdinalIgnoreCase);

    public TextureVanillaCompare(ProjectEntry project)
    {
        Project = project;
        Project.VFS.ProjectFS.FileWritten += OnFileWritten;
    }

    private void OnFileWritten(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var key = Normalize(path);
        Compares.TryRemove(key, out _);

        // A written .tpfbdt belongs to the .tpfbhd's index
        if (key.EndsWith(".tpfbdt", StringComparison.OrdinalIgnoreCase))
            Compares.TryRemove(key[..^3] + "bhd", out _);
    }

    /// <summary>The saved container against the game's (for a .tpfbhd, Modified when its .tpfbdt is)</summary>
    public VanillaStatus ContainerStatus(FileDictionaryEntry container)
    {
        if (container == null)
            return VanillaStatus.None;

        var status = Project.VFS.FileStatus(container.Path);

        if (status == VanillaStatus.Unchanged && IsBhd(container.Path) &&
            Project.VFS.FileStatus(BdtPath(container.Path)) == VanillaStatus.Modified)
            return VanillaStatus.Modified;

        return status;
    }

    /// <summary>A TPF inside a container (for a loose .tpf, the container's own entry) against the game's</summary>
    public VanillaStatus TpfStatus(FileDictionaryEntry container, bool loose, string tpfName)
    {
        var compare = GetCompare(container, loose, out var status);
        if (compare == null)
            return status;

        if (!compare.Vanilla.TryGetValue(tpfName, out var vanilla))
            return VanillaStatus.Added;

        if (!compare.Saved.TryGetValue(tpfName, out var saved))
            return VanillaStatus.None;

        return saved.Bytes.AsSpan().SequenceEqual(vanilla.Bytes) ? VanillaStatus.Unchanged : VanillaStatus.Modified;
    }

    /// <summary>A texture of a TPF inside a container against the game's texture of the same name in the same TPF</summary>
    public VanillaStatus TextureStatus(FileDictionaryEntry container, bool loose, string tpfName, string textureName)
    {
        var compare = GetCompare(container, loose, out var status);
        if (compare == null)
            return status;

        if (!compare.Vanilla.TryGetValue(tpfName, out var vanillaTpf) || !vanillaTpf.Textures.TryGetValue(textureName, out var vanilla))
            return VanillaStatus.Added;

        if (!compare.Saved.TryGetValue(tpfName, out var savedTpf) || !savedTpf.Textures.TryGetValue(textureName, out var saved))
            return VanillaStatus.None;

        return saved.AsSpan().SequenceEqual(vanilla) ? VanillaStatus.Unchanged : VanillaStatus.Modified;
    }

    /// <summary>
    /// The container's compare when the container is Modified (else null, with <paramref name="status" /> the
    /// status every item inherits: Added, Unchanged, or None while unknown or being compared)
    /// </summary>
    private ContainerCompare? GetCompare(FileDictionaryEntry container, bool loose, out VanillaStatus status)
    {
        status = ContainerStatus(container);

        if (status != VanillaStatus.Modified)
            return null;

        var key = Normalize(container.Path);

        if (!Compares.TryGetValue(key, out var compare))
        {
            if (Compares.TryAdd(key, null))
                Task.Run(() => Compare(container, loose, key));

            status = VanillaStatus.None;
            return null;
        }

        if (compare == null)
            status = VanillaStatus.None;

        return compare;
    }

    private void Compare(FileDictionaryEntry container, bool loose, string key)
    {
        var compare = new ContainerCompare { Saved = new(), Vanilla = new() };

        try
        {
            var projectFS = Project.VFS.FS;
            var vanillaFS = Project.VFS.VanillaFS;

            compare.Saved = Index(container, loose, p => projectFS.FileExists(p) ? projectFS.ReadFile(p) : null);
            compare.Vanilla = Index(container, loose, p => vanillaFS.FileExists(p) ? vanillaFS.ReadFile(p) : null);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"Failed to compare {container.Path} with the game's copy", LogPriority.Low, e);
        }

        // Only if no write dropped it meanwhile (that write's compare starts afresh)
        if (Compares.TryGetValue(key, out var current) && current == null)
            Compares[key] = compare;
    }

    /// <summary>The TPFs of a container (named as the bank names them) with their textures</summary>
    private static Dictionary<string, TpfIndex> Index(FileDictionaryEntry container, bool loose, Func<string, Memory<byte>?> read)
    {
        var result = new Dictionary<string, TpfIndex>(StringComparer.OrdinalIgnoreCase);

        void Add(string name, Memory<byte> bytes)
        {
            if (bytes.Length == 0 || result.ContainsKey(name))
                return;

            var index = new TpfIndex { Bytes = bytes.ToArray(), Textures = new(StringComparer.OrdinalIgnoreCase) };

            try
            {
                foreach (var tex in TPF.Read(bytes).Textures)
                    index.Textures.TryAdd(tex.Name, tex.Bytes);
            }
            catch
            {
                // Not a readable TPF: compared by its bytes only
            }

            result[name] = index;
        }

        // A loose .tpf: the bank holds it as a binder of one file, named after the container
        if (loose)
        {
            if (read(container.Path) is { } data)
                Add(container.Filename, data);
            return result;
        }

        IBinder binder = null;

        if (IsBhd(container.Path))
        {
            var bhd = read(container.Path);
            var bdt = read(BdtPath(container.Path));
            if (bhd == null || bdt == null)
                return result;

            binder = BXF3.IsBHD(bhd.Value) ? BXF3.Read(bhd.Value, bdt.Value) : BXF4.Read(bhd.Value, bdt.Value);
        }
        else if (read(container.Path) is { } data)
        {
            binder = BND3.Is(data.ToArray()) ? BND3.Read(data) : BND4.Read(data);
        }

        if (binder == null)
            return result;

        foreach (var file in binder.Files.Where(f => f.Name.Contains(".tpf", StringComparison.OrdinalIgnoreCase)))
            Add(file.Name, file.Bytes);

        return result;
    }

    private static bool IsBhd(string path) => path.EndsWith(".tpfbhd", StringComparison.OrdinalIgnoreCase);

    private static string BdtPath(string bhdPath) => bhdPath[..^".tpfbhd".Length] + ".tpfbdt";

    private static string Normalize(string path) => path.Replace('\\', '/').TrimStart('/');
}
