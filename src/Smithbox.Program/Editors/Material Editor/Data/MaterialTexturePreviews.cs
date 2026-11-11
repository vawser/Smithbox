using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System.Collections.Concurrent;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Textures loaded only to preview them in the Textures window, for a texture no viewport has loaded. The containers
/// come from the texture index (<see cref="MaterialTextureIndex"/>), so a texture can only be loaded once it has been
/// scanned. The file is read and the texture found on a background task: from a .tpfbhd only that texture's file is
/// read from the .tpfbdt, and in a binder only the TPF whose entry table names it is read in full. The GPU upload is
/// queued on the main thread, as the Texture Viewer does. The last <see cref="MaxLoaded"/> textures used are kept.
/// </summary>
public class MaterialTexturePreviews
{
    private readonly ProjectEntry Project;
    private readonly MaterialTextureIndex Index;

    private const int MaxLoaded = 48;

    // One container read at a time: a .tpfbdt or texture binder can be large
    private readonly SemaphoreSlim ReadGate = new(1, 1);

    public enum PreviewState
    {
        Loading,
        Loaded,
        Failed
    }

    private sealed class Entry
    {
        public PreviewState State = PreviewState.Loading;
        public TextureResource Resource;
        public long LastUsed;
    }

    private readonly Dictionary<string, Entry> Entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<(string name, TPF tpf, int index)> Finished = new();
    private long Frame;

    public MaterialTexturePreviews(ProjectEntry project, MaterialTextureIndex index)
    {
        Project = project;
        Index = index;
    }

    /// <summary>
    /// The preview of a texture, started if it isn't yet. Null while it loads or if it can't be loaded;
    /// <paramref name="state"/> says which.
    /// </summary>
    public TexturePool.TextureHandle Get(string name, out PreviewState state)
    {
        if (!Entries.TryGetValue(name, out var entry))
        {
            entry = new Entry();
            Entries[name] = entry;
            Start(name);
        }

        entry.LastUsed = Frame;
        state = entry.State;

        var texture = entry.Resource?.GPUTexture;

        // The upload runs on a later frame; until then the texture has no size
        if (state == PreviewState.Loaded && (texture == null || !texture.Resident || texture.Width == 0))
        {
            state = PreviewState.Loading;
            return null;
        }

        return state == PreviewState.Loaded ? texture : null;
    }

    /// <summary>
    /// Drops a texture's preview, so it is read again the next time it is drawn (after it was edited, say)
    /// </summary>
    public void Reload(string name)
    {
        if (Entries.Remove(name, out var entry))
            entry.Resource?.Dispose();
    }

    /// <summary>Uploads the textures read since the last frame, and drops the oldest past the limit. Main thread.</summary>
    public void Update()
    {
        Frame++;

        while (Finished.TryDequeue(out var done))
        {
            if (!Entries.TryGetValue(done.name, out var entry))
                continue;

            if (done.tpf == null)
            {
                entry.State = PreviewState.Failed;
                continue;
            }

            try
            {
                var resource = new TextureResource(done.tpf, done.index);

                if (resource._LoadTexture(AccessLevel.AccessFull))
                {
                    entry.Resource = resource;
                    entry.State = PreviewState.Loaded;
                }
                else
                {
                    resource.Dispose();
                    entry.State = PreviewState.Failed;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAT_Texture_Preview_Failed", done.name), e);
                entry.State = PreviewState.Failed;
            }
        }

        if (Entries.Count <= MaxLoaded)
            return;

        // Oldest first, but never one still loading, whose result would have nowhere to go
        foreach (var (name, entry) in Entries.Where(kv => kv.Value.State != PreviewState.Loading)
                     .OrderBy(kv => kv.Value.LastUsed).Take(Entries.Count - MaxLoaded).ToList())
        {
            entry.Resource?.Dispose();
            Entries.Remove(name);
        }
    }

    private void Start(string name)
    {
        var containers = Index.ContainersOf(name);

        if (containers.Count == 0)
        {
            Finished.Enqueue((name, null, -1));
            return;
        }

        var fs = Project.VFS.FS;
        var isPacked3 = Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;

        Task.Run(async () =>
        {
            await ReadGate.WaitAsync();

            try
            {
                foreach (var container in containers)
                {
                    var found = Find(fs, container, name, isPacked3);

                    if (found.tpf != null)
                    {
                        Finished.Enqueue((name, found.tpf, found.index));
                        return;
                    }
                }

                Finished.Enqueue((name, null, -1));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAT_Texture_Preview_Failed", name), e);
                Finished.Enqueue((name, null, -1));
            }
            finally
            {
                ReadGate.Release();
            }
        });
    }

    /// <summary>
    /// The TPF in a container holding a texture of this name, and the texture's index in it
    /// </summary>
    private static (TPF tpf, int index) Find(VirtualFileSystem fs, string container, string name, bool isPacked3)
    {
        if (!fs.FileExists(container))
            return (null, -1);

        // .tpfbhd: one file per texture, named as the texture, in the .tpfbdt
        if (container.EndsWith(".tpfbhd", StringComparison.OrdinalIgnoreCase))
        {
            var bdtPath = container[..^".tpfbhd".Length] + ".tpfbdt";
            if (!fs.FileExists(bdtPath))
                return (null, -1);

            var bhd = fs.ReadFile(container);
            var bdt = fs.ReadFile(bdtPath);
            if (bhd == null || bdt == null)
                return (null, -1);

            using BinderReader reader = isPacked3
                ? new BXF3Reader(bhd.Value, bdt.Value)
                : new BXF4Reader(bhd.Value, bdt.Value);

            var file = reader.Files.FirstOrDefault(f =>
                string.Equals(MaterialTextureIndex.Stem(f.Name), name, StringComparison.OrdinalIgnoreCase));

            return file == null ? (null, -1) : InTpf(reader.ReadFile(file), name);
        }

        // A TPF, or a binder of them
        var data = fs.ReadFile(container);
        if (data == null)
            return (null, -1);

        foreach (var tpfData in MaterialTextureIndex.Tpfs(data.Value, 0))
        {
            var names = TpfTextureNames.Read(tpfData);
            if (names == null || !names.Any(n => string.Equals(MaterialTextureIndex.Stem(n), name, StringComparison.OrdinalIgnoreCase)))
                continue;

            var found = InTpf(tpfData, name);
            if (found.tpf != null)
                return found;
        }

        return (null, -1);
    }

    private static (TPF tpf, int index) InTpf(Memory<byte> data, string name)
    {
        if (data.Length == 0)
            return (null, -1);

        var tpf = TPF.Read(data);
        var index = tpf.Textures.FindIndex(t =>
            string.Equals(MaterialTextureIndex.Stem(t.Name), name, StringComparison.OrdinalIgnoreCase));

        return index < 0 ? (null, -1) : (tpf, index);
    }
}
