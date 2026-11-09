using StudioCore.Editors.MaterialEditor;
using StudioCore.Renderer;
using System.Collections.Concurrent;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The textures and models the viewport's particles draw with (FX Editor Plan §10.4).
/// - Textures: an id and type become a TPF name (s88610_a, or s88610 when the game has no typed one), found in the
///   binder index the bank built. Until a texture has loaded its handle is 0, which the shader draws as a soft spot.
/// - Models (phase V4): s&lt;id&gt;.flver, from the same index. When one is read, the textures its materials name
///   are asked for too, so the model draws with its own textures.
/// Only what is asked for is read: binders are read on a task, one at a time, taking everything waiting in each, and
/// the entries are handed to the resource pool. Resources stay loaded while the editor is open.
/// </summary>
public class FxTextureLibrary : IResourceEventListener
{
    private readonly FxEditorScreen Editor;

    private enum Kind { Texture, Model }

    private readonly Dictionary<string, ResourceHandle<TextureResource>> Loaded = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ResourceHandle<FlverResource>> LoadedModels = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> Requested = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> Missing = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Name, Kind Kind)> Tags = new();
    private readonly ConcurrentQueue<(string Name, Kind Kind, byte[] Data, List<string> ModelTextures)> Read = new();
    private readonly Dictionary<(int Id, string Type), string> Names = new();

    /// <summary>Textures a model asked for: when one arrives, the models' textures are refreshed.</summary>
    private readonly HashSet<string> ModelTextures = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Texture and model names that the effects ask for but no binder holds, for the Preview tool.</summary>
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
            StartLoad(name, Kind.Texture);
        return 0;
    }

    /// <summary>A model's resource once it has loaded, null until then (or when no binder holds it).</summary>
    public ResourceHandle<FlverResource> GetModel(int id)
    {
        if (id <= 0)
            return null;

        var name = $"s{id:D5}";
        if (LoadedModels.TryGetValue(name, out var handle))
            return handle.IsLoaded ? handle : null;

        if (!Editor.Data.Bank.Models.ContainsKey(name))
        {
            if (Editor.Data.Bank.IsIndexed)
                Missing.Add($"{name}.flver");
            return null;
        }

        if (Requested.Add($"{name}.flver"))
            StartLoad(name, Kind.Model);
        return null;
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

    // Entries waiting to be read, by binder. A binder is decompressed whole to read one entry (ER's largest is
    // ~350 MB), so each binder is read once for everything waiting in it, and one binder at a time.
    private readonly Dictionary<string, List<(string Name, string InternalName, Kind Kind)>> Pending = new(StringComparer.OrdinalIgnoreCase);
    private Task Reading;

    private void StartLoad(string name, Kind kind)
    {
        var index = kind == Kind.Model ? Editor.Data.Bank.Models : Editor.Data.Bank.Textures;
        if (!index.TryGetValue(name, out var binders) || binders.Count == 0)
            return;

        // The first binder that holds it; copies in other binders are the same file
        var source = binders[0];
        if (!Pending.TryGetValue(source.Binder.Path, out var list))
            Pending[source.Binder.Path] = list = new();
        list.Add((name, source.InternalName, kind));
    }

    /// <summary>A texture path's file name, the texture's name: N:\GR\data\Sfx\Tex\s88300_a.tif to s88300_a.</summary>
    private static string Stem(string path) => Path.GetFileNameWithoutExtension(path.Replace('\\', '/'));

    /// <summary>
    /// A texture a model's material names: from the sfx binders when one holds it, otherwise from the part it
    /// belongs to (ER sfx MATBINs point at weapon and AAT textures, e.g. Model\parts\Weapon\WP_A_0880\tex\ and
    /// Model\parts\tex\AAT500_Hair_02_a.tif).
    /// </summary>
    private void RequestModelTexture(string path)
    {
        var name = Stem(path);
        if (Editor.Data.Bank.Textures.ContainsKey(name))
            RequestTextureByName(name);
        else
            RequestPartTextures(path);
    }

    // Part containers asked for, each once; and loads still running, to refresh the models when one finishes
    private readonly HashSet<string> RequestedContainers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Task> ContainerLoads = new();

    /// <summary>
    /// Loads the textures of the part container a texture path is under, into the pool the FX models use, where
    /// FlverResource finds them by name. ...\parts\tex\ is the shared AAT textures (common_body); ...\parts\Type\Id\tex\
    /// is that part's partsbnd. The pool skips a container that is loaded or loading already.
    /// </summary>
    private void RequestPartTextures(string path)
    {
        var parts = path.Replace('/', '\\').Split('\\', StringSplitOptions.RemoveEmptyEntries);
        int partsIndex = Array.FindLastIndex(parts, p => p.Equals("parts", StringComparison.OrdinalIgnoreCase));
        int texIndex = Array.FindLastIndex(parts, p => p.Equals("tex", StringComparison.OrdinalIgnoreCase));
        var stem = Stem(path);
        var project = Editor.Project;

        ResourceDescriptor descriptor;
        if (partsIndex >= 0 && texIndex > partsIndex)
        {
            descriptor = texIndex == partsIndex + 1
                ? TextureLocator.GetCharacterCommonTextureVirtualPath(project, "common_body")
                : TextureLocator.GetPartTextureVirtualPath(project, parts[texIndex - 1].ToLowerInvariant());
        }
        else if (Array.FindLastIndex(parts, p => p.Equals("chr", StringComparison.OrdinalIgnoreCase)) is var chrIndex
                 && chrIndex >= 0 && texIndex == chrIndex + 2)
        {
            // Character textures (DS3 s-models use Model\chr\c5180\tex\c5180_Fruit_a.tif): the character's texbnd
            descriptor = TextureLocator.GetCharacterTextureVirtualPath(project, parts[chrIndex + 1].ToLowerInvariant());
        }
        else if (stem.Length >= 10 && stem.StartsWith("AET", StringComparison.OrdinalIgnoreCase))
        {
            // Asset environment textures (Asset\Environment\texture\AET250\AET250_041_a.tif): asset/aet/aet250/aet250_041.tpf
            descriptor = TextureLocator.GetAssetTextureVirtualPath(project, stem[..10].ToLowerInvariant());
        }
        else
        {
            return;
        }

        var container = descriptor.IsValid() ? descriptor.GetContainerPath(project) : null;
        if (container == null || !RequestedContainers.Add(container))
            return;

        var vfsPath = "/" + container.Replace('\\', '/').TrimStart('/');
        if (!project.VFS.FS.FileExists(vfsPath))
        {
            Missing.Add(container);
            return;
        }

        ContainerLoads.Add(ResourceManager.DefaultPool.LoadContainer(container, new LoadOptions
        {
            Types = ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            HideFromLoadWindow = true,
            JobName = $"FX part textures {container}",
        }));
    }

    /// <summary>A texture by its name (as a model's material names it), if a binder holds it.</summary>
    private void RequestTextureByName(string name)
    {
        if (Loaded.ContainsKey(name) || !Editor.Data.Bank.Textures.ContainsKey(name))
            return;
        ModelTextures.Add(name);
        if (Requested.Add(name))
            StartLoad(name, Kind.Texture);
    }

    private void ReadNextBinder()
    {
        if (Reading is { IsCompleted: false } || Pending.Count == 0)
            return;

        var (path, wanted) = Pending.First();
        Pending.Remove(path);
        var fs = Editor.Project.VFS.FS;
        var materials = Editor.Project.Handler.MaterialData?.PrimaryBank;

        Reading = Task.Run(() =>
        {
            try
            {
                var reader = new SoulsFormats.BND4Reader(fs.ReadFile(path).Value);
                foreach (var (name, internalName, kind) in wanted)
                {
                    var file = reader.Files.FirstOrDefault(f => f.Name == internalName);
                    if (file == null)
                        continue;

                    var bytes = reader.ReadFile(file).ToArray();
                    List<string> textures = null;
                    if (kind == Kind.Model)
                        textures = MaterialTextures(bytes, materials);
                    Read.Enqueue((name, kind, bytes, textures));
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[FX Editor] Couldn't read textures or models from {path}: {e.Message}", e);
            }
        });
    }

    /// <summary>
    /// The texture paths a model's materials use (e.g. N:\GR\data\Sfx\Tex\s88300_a.tif; their file names are the
    /// texture names).
    /// ER, NR and AC6 sfx models leave most texture paths empty (ER: 1,353 of 1,708) and name them in the
    /// material's MATBIN instead, which is where FlverResource looks them up. Those are asked for here too, from
    /// all of the MATBIN's samplers, as the renderer also matches samplers loosely
    /// (FxRuntimeSurvey --modelmaterials: ER 1,011, NR 952, AC6 598 of the empty slots name an sfx TPF).
    /// </summary>
    private static List<string> MaterialTextures(byte[] bytes, MaterialBank materials)
    {
        var names = new List<string>();
        try
        {
            var data = SoulsFormats.DCX.Is(bytes) ? SoulsFormats.DCX.Decompress(bytes).ToArray() : bytes;
            var flver = SoulsFormats.FLVER2.Read(data);
            foreach (var material in flver.Materials)
            {
                bool fromMatbin = false;
                foreach (var texture in material.Textures)
                {
                    if (!string.IsNullOrEmpty(texture.Path))
                        names.Add(texture.Path);
                    else
                        fromMatbin = true;
                }

                var matbin = fromMatbin && !string.IsNullOrEmpty(material.MTD) ? materials?.GetMatbin(Stem(material.MTD)) : null;
                if (matbin != null)
                    foreach (var sampler in matbin.Samplers)
                        if (!string.IsNullOrEmpty(sampler.Path))
                            names.Add(sampler.Path);
            }
        }
        catch
        {
            // Not a FLVER2: drawn with whatever textures are loaded
        }
        return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Reads waiting binders and hands what was read to the resource pool. Call on the main thread each frame.</summary>
    public void Update()
    {
        ReadNextBinder();

        // A part container's textures arrived: have the models pick them up
        if (ContainerLoads.RemoveAll(t => t.IsCompleted) > 0)
            ResourceManager.SchedulePostTextureRefresh();

        while (Read.TryDequeue(out var item))
        {
            int tag = Tags.Count;
            Tags.Add((item.Name, item.Kind));

            if (item.Kind == Kind.Model)
            {
                ResourceManager.DefaultPool.AddListener<FlverResource>(item.Name, this, AccessLevel.AccessGPUOptimizedOnly, tag);
                ResourceManager.DefaultPool.LoadContainerData($"{item.Name}.flver", item.Data, new LoadOptions
                {
                    Types = ResourceType.Flver,
                    JobName = $"FX model {item.Name}",
                    HideFromLoadWindow = true,
                });
                foreach (var path in item.ModelTextures ?? new())
                    RequestModelTexture(path);
            }
            else
            {
                ResourceManager.DefaultPool.AddListener<TextureResource>(item.Name, this, AccessLevel.AccessGPUOptimizedOnly, tag);
                ResourceManager.DefaultPool.LoadContainerData($"{item.Name}.tpf", item.Data, new LoadOptions
                {
                    Types = ResourceType.Texture,
                    JobName = $"FX texture {item.Name}",
                    HideFromLoadWindow = true,
                });
            }
        }
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        if (tag < 0 || tag >= Tags.Count)
            return;

        var (name, kind) = Tags[tag];
        if (kind == Kind.Model)
        {
            if (LoadedModels.ContainsKey(name))
                return;
            var model = (ResourceHandle<FlverResource>)handle;
            model.Acquire();
            LoadedModels[name] = model;
            return;
        }

        if (Loaded.ContainsKey(name))
            return;
        var texture = (ResourceHandle<TextureResource>)handle;
        texture.Acquire();
        Loaded[name] = texture;

        // A model's texture came after the model: have the models pick it up
        if (ModelTextures.Contains(name))
            ResourceManager.SchedulePostTextureRefresh();
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        if (tag < 0 || tag >= Tags.Count)
            return;

        var (name, kind) = Tags[tag];
        if (kind == Kind.Model)
        {
            if (LoadedModels.Remove(name, out var model))
                model.Release();
            Requested.Remove($"{name}.flver");
            return;
        }

        if (Loaded.Remove(name, out var texture))
            texture.Release();
        Requested.Remove(name);
    }

    public void Clear()
    {
        foreach (var texture in Loaded.Values)
            texture.Release();
        foreach (var model in LoadedModels.Values)
            model.Release();
        Loaded.Clear();
        LoadedModels.Clear();
        Requested.Clear();
        Missing.Clear();
        Names.Clear();
        Pending.Clear();
        ModelTextures.Clear();
        RequestedContainers.Clear();
        ContainerLoads.Clear();
    }
}
