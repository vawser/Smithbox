using StudioCore.Renderer;
using System.Collections.Concurrent;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The textures the viewport's particles draw with (FX Editor Plan §10.4). A texture id and type become a TPF name
/// (s88610_a, or s88610 when the game has no typed one), found in the binder index the bank built. Only the TPFs
/// asked for are read: the binder is read on a task, the TPF taken out of it, and handed to the resource pool,
/// where the texture is named as in the TPF. Until it has loaded its handle is 0, which the shader draws as a soft
/// spot. Textures stay loaded while the editor is open.
/// </summary>
public class FxTextureLibrary : IResourceEventListener
{
    private readonly FxEditorScreen Editor;

    private readonly Dictionary<string, ResourceHandle<TextureResource>> Loaded = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> Requested = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> Missing = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> Tags = new();
    private readonly ConcurrentQueue<(string Name, byte[] Data)> Read = new();
    private readonly Dictionary<(int Id, string Type), string> Names = new();

    /// <summary>Texture names that the effects ask for but no binder holds, for the Preview tool.</summary>
    public IReadOnlyCollection<string> MissingTextures => Missing;

    public FxTextureLibrary(FxEditorScreen editor)
    {
        Editor = editor;
    }

    /// <summary>
    /// The GPU handle of a texture, 0 while it loads or when there is none. The first call starts loading it.
    /// </summary>
    public uint Get(int id, string type)
    {
        if (id <= 0)
            return 0;

        var name = Resolve(id, type);
        if (name == null)
            return 0;

        if (Loaded.TryGetValue(name, out var handle))
            return handle.IsLoaded ? handle.Get()?.GPUTexture?.TexHandle ?? 0 : 0;

        if (Requested.Add(name))
            StartLoad(name);
        return 0;
    }

    /// <summary>s&lt;id&gt;_&lt;type&gt; if a binder holds it, else s&lt;id&gt; (DS3 and SDT have both kinds), else null.</summary>
    private string Resolve(int id, string type)
    {
        if (Names.TryGetValue((id, type), out var name))
            return name;

        var textures = Editor.Data.Bank.Textures;
        var typed = string.IsNullOrEmpty(type) ? null : $"s{id:D5}_{type}";
        var plain = $"s{id:D5}";

        if (typed != null && textures.ContainsKey(typed))
            name = typed;
        else if (textures.ContainsKey(plain))
            name = plain;
        else
        {
            name = null;
            // Indexing may not have finished; only remember a miss once it has
            if (Editor.Data.Bank.IsIndexed)
                Missing.Add(typed ?? plain);
        }

        if (name != null || Editor.Data.Bank.IsIndexed)
            Names[(id, type)] = name;
        return name;
    }

    // Textures waiting to be read, by binder. A binder is decompressed whole to read one entry (ER's largest is
    // ~350 MB), so each binder is read once for everything waiting in it, and one binder at a time.
    private readonly Dictionary<string, List<(string Name, string InternalName)>> Pending = new(StringComparer.OrdinalIgnoreCase);
    private Task Reading;

    private void StartLoad(string name)
    {
        if (!Editor.Data.Bank.Textures.TryGetValue(name, out var binders) || binders.Count == 0)
            return;

        // The first binder that holds it; copies in other binders are the same texture
        var source = binders[0];
        if (!Pending.TryGetValue(source.Binder.Path, out var list))
            Pending[source.Binder.Path] = list = new();
        list.Add((name, source.InternalName));
    }

    private void ReadNextBinder()
    {
        if (Reading is { IsCompleted: false } || Pending.Count == 0)
            return;

        var (path, wanted) = Pending.First();
        Pending.Remove(path);
        var fs = Editor.Project.VFS.FS;

        Reading = Task.Run(() =>
        {
            try
            {
                var reader = new SoulsFormats.BND4Reader(fs.ReadFile(path).Value);
                foreach (var (name, internalName) in wanted)
                {
                    var file = reader.Files.FirstOrDefault(f => f.Name == internalName);
                    if (file != null)
                        Read.Enqueue((name, reader.ReadFile(file).ToArray()));
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[FX Editor] Couldn't read textures from {path}: {e.Message}", e);
            }
        });
    }

    /// <summary>Reads waiting binders and hands read TPFs to the resource pool. Call on the main thread each frame.</summary>
    public void Update()
    {
        ReadNextBinder();

        while (Read.TryDequeue(out var item))
        {
            int tag = Tags.Count;
            Tags.Add(item.Name);
            ResourceManager.DefaultPool.AddListener<TextureResource>(item.Name, this, AccessLevel.AccessGPUOptimizedOnly, tag);

            ResourceManager.DefaultPool.LoadContainerData($"{item.Name}.tpf", item.Data, new LoadOptions
            {
                Types = ResourceType.Texture,
                JobName = $"FX texture {item.Name}",
                HideFromLoadWindow = true,
            });
        }
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        if (tag < 0 || tag >= Tags.Count)
            return;

        var name = Tags[tag];
        var texture = (ResourceHandle<TextureResource>)handle;
        if (Loaded.ContainsKey(name))
            return;

        texture.Acquire();
        Loaded[name] = texture;
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        if (tag < 0 || tag >= Tags.Count)
            return;

        var name = Tags[tag];
        if (Loaded.Remove(name, out var texture))
            texture.Release();
        Requested.Remove(name);
    }

    public void Clear()
    {
        foreach (var texture in Loaded.Values)
            texture.Release();
        Loaded.Clear();
        Requested.Clear();
        Missing.Clear();
        Names.Clear();
        Pending.Clear();
    }
}
