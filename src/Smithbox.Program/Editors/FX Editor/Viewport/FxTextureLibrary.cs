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
    private readonly IFxDisplayHost Host;

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

    public FxTextureLibrary(IFxDisplayHost host)
    {
        Host = host;
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

        // Read as data, not colour (BB names many normal maps without "_n", e.g. s02600)
        if (type == "n")
            NormalNames.Add(name);

        if (Loaded.TryGetValue(name, out var handle))
            return handle.IsLoaded ? handle.Get()?.GPUTexture?.TexHandle ?? 0 : 0;

        if (Requested.Add(name))
            StartLoad(name, Kind.Texture);
        return 0;
    }

    /// <summary>s&lt;id&gt; with five digits (FXR3), else the older games' name for the id (DES s0000, DS2 s000010).</summary>
    private string ModelName(int id)
    {
        var name = $"s{id:D5}";
        if (Host.Data.Bank.Models.ContainsKey(name) || !Host.Data.IsFfx)
            return name;
        return Host.Data.Bank.ResourceName(Host.Data.Bank.Models, id) ?? name;
    }

    /// <summary>The first binder holding a model (its anibnd sits beside it), null when none does or indexing isn't done.</summary>
    public string ModelBinderPath(int id) =>
        id > 0 && Host.Data.Bank.Models.TryGetValue(ModelName(id), out var list) && list.Count > 0 ? list[0].Binder.Path : null;

    /// <summary>A model's resource once it has loaded, null until then (or when no binder holds it).</summary>
    public ResourceHandle<FlverResource> GetModel(int id)
    {
        if (id <= 0)
            return null;

        var name = ModelName(id);
        if (LoadedModels.TryGetValue(name, out var handle))
            return handle.IsLoaded ? handle : null;

        if (!Host.Data.Bank.Models.ContainsKey(name))
        {
            if (Host.Data.Bank.IsIndexed)
                Missing.Add($"{name}.flver");
            return null;
        }

        if (Requested.Add($"{name}.flver"))
            StartLoad(name, Kind.Model);
        return null;
    }

    // Entries a binder was found not to hold, or couldn't be read from: they'll never load (models as s12345.flver)
    private readonly HashSet<string> Unreadable = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<string> FailedReads = new();

    /// <summary>
    /// Whether a texture is loaded, or never will be (no binder holds it, or it couldn't be read), for
    /// <see cref="FxPreload"/>. The first call starts loading it, as <see cref="Get"/> does.
    /// </summary>
    public bool Prefetch(int id, string type)
    {
        if (id <= 0)
            return true;

        var name = Resolve(id, type);
        if (name == null)
            return Host.Data.Bank.IsIndexed;

        return Get(id, type) != 0 || Unreadable.Contains(name);
    }

    /// <summary>Whether a model is loaded, or never will be, for <see cref="FxPreload"/>. The first call starts loading it.</summary>
    public bool PrefetchModel(int id)
    {
        if (id <= 0)
            return true;

        if (GetModel(id) != null)
            return true;

        var name = ModelName(id);
        if (!Host.Data.Bank.Models.ContainsKey(name))
            return Host.Data.Bank.IsIndexed;

        return Unreadable.Contains($"{name}.flver");
    }

    /// <summary>Whether textures a loaded model's materials name are still on their way (sfx TPFs or part containers).</summary>
    public bool IsLoadingModelTextures =>
        ContainerLoads.Count > 0 || ModelTextures.Any(n => !Loaded.ContainsKey(n) && !Unreadable.Contains(n));

    /// <summary>s&lt;id&gt;_&lt;type&gt; if a binder holds it, else s&lt;id&gt; (DS3 and SDT have both kinds), else null.</summary>
    private string Resolve(int id, string type)
    {
        if (Names.TryGetValue((id, type), out var name))
            return name;

        var textures = Host.Data.Bank.Textures;
        var typed = string.IsNullOrEmpty(type) ? null : $"s{id:D5}_{type}";
        var plain = $"s{id:D5}";

        if (typed != null && textures.ContainsKey(typed))
            name = typed;
        else if (textures.ContainsKey(plain))
            name = plain;
        else if (Host.Data.IsFfx && Host.Data.Bank.ResourceName(textures, id) is string any)
            name = any; // the older games name textures with other digit counts (DES s0003, DS2 s000002)
        else
        {
            name = null;
            // Indexing may not have finished; only remember a miss once it has
            if (Host.Data.Bank.IsIndexed)
                Missing.Add(typed ?? plain);
        }

        if (name != null || Host.Data.Bank.IsIndexed)
            Names[(id, type)] = name;
        return name;
    }

    // Entries waiting to be read, by binder. A binder is decompressed whole to read one entry (ER's largest is
    // ~350 MB), so each binder is read once for everything waiting in it, and one binder at a time.
    private readonly Dictionary<string, List<(string Name, string InternalName, Kind Kind)>> Pending = new(StringComparer.OrdinalIgnoreCase);
    private Task Reading;

    // Textures asked for as normal maps: kept UNORM by AsColour
    private readonly HashSet<string> NormalNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// BB's PS4 TPFs give their colour textures UNORM DXGI formats (BC1 71, BC3 77, BC7 98), so the pool samples them
    /// without sRGB decoding, while the other games' sfx textures (DDS DXT1 / DXT5) are decoded as sRGB, as the
    /// billboard shader and the tone map expect. Read as UNORM, BB effects were too bright and their textures' dark
    /// rims stayed visible: f000000117's 2.5 × 7.5 additive glow (s01000) showed its rectangle (FFX plan §20.35).
    /// Here a PS4 TPF's formats are switched to their _SRGB twins, for the FX textures only; maps and characters are
    /// unchanged. Normal maps (asked for as "n") are left as they are.
    /// </summary>
    private static byte[] AsColour(byte[] bytes)
    {
        try
        {
            var raw = SoulsFormats.DCX.Is(bytes) ? SoulsFormats.DCX.Decompress(bytes).ToArray() : bytes;
            var tpf = SoulsFormats.TPF.Read(raw);
            if (tpf.Platform != SoulsFormats.TPF.TPFPlatform.PS4)
                return bytes;
            bool changed = false;
            foreach (var t in tpf.Textures)
            {
                if (t.Header == null)
                    continue;
                int srgb = (SoulsFormats.DDS.DXGI_FORMAT)t.Header.DXGIFormat switch
                {
                    SoulsFormats.DDS.DXGI_FORMAT.BC1_UNORM => (int)SoulsFormats.DDS.DXGI_FORMAT.BC1_UNORM_SRGB,
                    SoulsFormats.DDS.DXGI_FORMAT.BC2_UNORM => (int)SoulsFormats.DDS.DXGI_FORMAT.BC2_UNORM_SRGB,
                    SoulsFormats.DDS.DXGI_FORMAT.BC3_UNORM => (int)SoulsFormats.DDS.DXGI_FORMAT.BC3_UNORM_SRGB,
                    SoulsFormats.DDS.DXGI_FORMAT.BC7_UNORM => (int)SoulsFormats.DDS.DXGI_FORMAT.BC7_UNORM_SRGB,
                    _ => t.Header.DXGIFormat,
                };
                if (srgb != t.Header.DXGIFormat)
                {
                    t.Header.DXGIFormat = srgb;
                    changed = true;
                }
            }
            return changed ? tpf.Write() : bytes;
        }
        catch
        {
            return bytes;
        }
    }

    private void StartLoad(string name, Kind kind)
    {
        var index = kind == Kind.Model ? Host.Data.Bank.Models : Host.Data.Bank.Textures;
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
        if (Host.Data.Bank.Textures.ContainsKey(name))
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
        var project = Host.Project;

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
        if (Loaded.ContainsKey(name) || !Host.Data.Bank.Textures.ContainsKey(name))
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
        var fs = Host.Project.VFS.FS;
        var materials = Host.Project.Handler.MaterialData?.PrimaryBank;
        var normals = new HashSet<string>(NormalNames, StringComparer.OrdinalIgnoreCase);

        Reading = Task.Run(() =>
        {
            try
            {
                // BND4 (FXR3 games, DS2, BB) is read entry by entry; BND3 (DES, DS1, DS1R) is read whole
                var raw = fs.ReadFile(path).Value;
                var unpacked = SoulsFormats.DCX.Is(raw) ? SoulsFormats.DCX.Decompress(raw) : raw;
                bool bnd3 = unpacked.Length >= 4 && System.Text.Encoding.ASCII.GetString(unpacked.Span[..4]) == "BND3";
                var reader = bnd3 ? null : new SoulsFormats.BND4Reader(raw);
                var bnd3Files = bnd3 ? SoulsFormats.BND3.Read(unpacked).Files : null;
                foreach (var (name, internalName, kind) in wanted)
                {
                    byte[] bytes;
                    if (bnd3)
                    {
                        var entry = bnd3Files.FirstOrDefault(f => f.Name == internalName);
                        bytes = entry?.Bytes.ToArray();
                    }
                    else
                    {
                        var file = reader.Files.FirstOrDefault(f => f.Name == internalName);
                        bytes = file == null ? null : reader.ReadFile(file).ToArray();
                    }
                    if (bytes == null)
                    {
                        FailedReads.Enqueue(kind == Kind.Model ? $"{name}.flver" : name);
                        continue;
                    }

                    List<string> textures = null;
                    if (kind == Kind.Model)
                        textures = MaterialTextures(bytes, materials);
                    else if (!normals.Contains(name))
                        bytes = AsColour(bytes);
                    Read.Enqueue((name, kind, bytes, textures));
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[FX Editor] Couldn't read textures or models from {path}: {e.Message}", e);
                foreach (var (name, _, kind) in wanted)
                    FailedReads.Enqueue(kind == Kind.Model ? $"{name}.flver" : name);
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

        while (FailedReads.TryDequeue(out var failed))
            Unreadable.Add(failed);

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
        Unreadable.Clear();
        FailedReads.Clear();
    }
}
