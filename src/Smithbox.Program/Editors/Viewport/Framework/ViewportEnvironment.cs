using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// The lighting the Model and Time Act Editors' viewports borrow from a map (AC6, ER, NR), as they have no map of
/// their own: its GPARAM lighting, resolved as the Map Editor resolves it (<see cref="GparamLightingBank" />), and
/// one volume of its irradiance bake (<see cref="IrradianceVolumeBank" />), stretched over a box centred on the
/// origin, where these editors draw their models. The volume's placement lives in the map's EnvMapEffectBox, which
/// isn't read, so its scale isn't the game's, but its light and the direction of it are.
/// The banks are this environment's own, so they don't need the Map Editor and don't disturb its maps; the time of
/// day, weather and bake settings are the Map Editor's, so both light alike.
/// </summary>
public class ViewportEnvironment : IDisposable
{
    public ProjectEntry Project;

    public GparamLightingBank LightingBank;

    public IrradianceVolumeBank VolumeBank;

    /// <summary>The volume the viewports light with, or null if the renderer has no material buffer</summary>
    public IrradianceVolumeTable Table;

    /// <summary>The map lit with this frame, null if the project has none to borrow from</summary>
    public string MapId { get; private set; }

    /// <summary>The map's GPARAM lighting this frame, null if there is no map</summary>
    public GparamLighting ActiveLighting { get; private set; }

    /// <summary>The volume of the map's bake lit with, -1 if the map has none</summary>
    public int VolumeId { get; private set; } = -1;

    /// <summary>The volume lit with when the settings name none: the one of median light</summary>
    public int MedianVolumeId { get; private set; } = -1;

    /// <summary>The bake lit with this frame, the bake blended towards (-1 for none) and how far</summary>
    public (int Bake, int BlendBake, float BlendRatio) Bakes { get; private set; }

    /// <summary>Whether the volume's textures are on the GPU, so the viewports are lit by it</summary>
    public bool VolumeShaded { get; private set; }

    // The settings the map was last resolved from, so the file lists aren't searched every frame
    private string ResolvedFor;

    private readonly IrradianceVolumeData[] Entries = new IrradianceVolumeData[1];

    // The volume textures held, so the Model and Time Act Editors' clearing of unused resources keeps them loaded
    private readonly Dictionary<string, IResourceHandle> HeldTextures = new(StringComparer.OrdinalIgnoreCase);

    // When a bake whose textures went missing was last queued again, and how many times, so a file that fails to
    // load isn't queued every frame
    private readonly Dictionary<(string MapId, int Bake), (DateTime Time, int Count)> Retries = new();

    private const int MaxRetries = 3;
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(5);

    public ViewportEnvironment(ProjectEntry project)
    {
        Project = project;
        LightingBank = new GparamLightingBank(project);
        VolumeBank = new IrradianceVolumeBank(project);

        if (CanUse() && SceneRenderer.MaterialBufferAllocator != null)
        {
            Table = new IrradianceVolumeTable(SceneRenderer.MaterialBufferAllocator, 1);
        }
    }

    public bool CanUse()
    {
        return LightingBank.CanUse();
    }

    /// <summary>
    /// The map each project type lights its Model and Time Act viewports with by default: one with its own GPARAM
    /// and a baked irradiance volume, so every setting has something to act on
    /// </summary>
    public static string GetDefaultMap(ProjectType type) => type switch
    {
        ProjectType.AC6 => "m10_00_00_00",
        // Stormveil Castle, whose bakes follow the GPARAM timezones
        ProjectType.ER => "m10_00_00_00",
        ProjectType.NR => "m10_00_00_00",
        _ => null
    };

    /// <summary>
    /// The maps that can be lit with: those with baked irradiance volumes
    /// </summary>
    public List<string> ListMaps()
    {
        return VolumeBank.ListMapsWithVolumes();
    }

    /// <summary>
    /// Resolves the lighting and the volume for this frame. Called by the Model and Time Act Editors every frame.
    /// </summary>
    public void Update()
    {
        if (!CanUse() || Table == null)
            return;

        var chosen = CFG.Current.Viewport_Environment_Map ?? "";

        if (ResolvedFor != chosen)
        {
            ResolvedFor = chosen;
            SwitchMap(ResolveMap(chosen));
        }

        if (MapId == null)
        {
            ActiveLighting = null;
            UploadVolume(null, -1, default);
            return;
        }

        ActiveLighting = LightingBank.ResolveCached(MapId, CFG.Current.Viewport_Environment_Lighting_File);

        var volumes = VolumeBank.GetMap(MapId);

        VolumeId = volumes != null && CFG.Current.Viewport_Environment_Volume >= 0 &&
                   volumes.HasVolume(CFG.Current.Viewport_Environment_Volume)
            ? CFG.Current.Viewport_Environment_Volume
            : MedianVolumeId;

        if (volumes == null || VolumeId < 0)
        {
            UploadVolume(null, -1, default);
            return;
        }

        // The bakes of the Global Illumination tab, or of the time of day, as in the Map Editor; a volume without the
        // bake asked for takes its first
        var (bake, blendBake, blendRatio) = VolumeBank.ResolveBakes(new[] { MapId });

        if (!volumes.HasVolume(VolumeId, bake))
        {
            bake = volumes.Infos.Keys.Where(k => k.Id == VolumeId).Select(k => k.Bake).DefaultIfEmpty(-1).Min();
            blendBake = -1;
        }

        Bakes = (bake, blendBake, blendRatio);

        if (bake < 0)
        {
            UploadVolume(null, -1, default);
            return;
        }

        // ResolveBakes only queues the bakes when they follow the time of day; these only queue once
        VolumeBank.RequestBake(MapId, bake);

        if (blendBake >= 0 && volumes.HasVolume(VolumeId, blendBake))
            VolumeBank.RequestBake(MapId, blendBake);

        RetryIfMissing(volumes, bake);

        if (blendBake >= 0)
            RetryIfMissing(volumes, blendBake);

        HoldTextures(bake, blendBake);

        UploadVolume(volumes, bake, (blendBake, blendRatio));
    }

    /// <summary>
    /// The map to light with: the chosen one if the project has its volumes or its GPARAM, else the project type's
    /// default, else the first with volumes
    /// </summary>
    private string ResolveMap(string chosen)
    {
        bool Usable(string mapId) => !string.IsNullOrEmpty(mapId) &&
            (VolumeBank.HasVolumes(mapId) || LightingBank.HasFile(LightingBank.GetDefaultFile(mapId)));

        if (Usable(chosen))
            return chosen;

        var fallback = GetDefaultMap(Project.Descriptor.ProjectType);

        if (Usable(fallback))
            return fallback;

        return ListMaps().FirstOrDefault();
    }

    private void SwitchMap(string mapId)
    {
        if (string.Equals(mapId, MapId, StringComparison.OrdinalIgnoreCase) && MapId != null)
            return;

        if (MapId != null)
            VolumeBank.OnUnloadMap(MapId);

        ReleaseTextures(new HashSet<string>());
        Retries.Clear();

        MapId = mapId;
        VolumeId = -1;
        MedianVolumeId = -1;

        if (mapId == null)
            return;

        var volumes = VolumeBank.LoadVolumes(mapId);
        MedianVolumeId = FindMedianVolume(volumes);
    }

    /// <summary>
    /// The volume whose light is the median of the map's, among those baked the most times, so it is neither one of
    /// the dark interiors nor the open sky, and has every time of day the map has
    /// </summary>
    private static int FindMedianVolume(IrradianceVolumeBank.MapVolumes volumes)
    {
        var byId = volumes.Infos
            .GroupBy(i => i.Key.Id)
            .Select(g => (Id: g.Key, Bakes: g.Count(), Light: g.Average(i => i.Value.ShLuminance.W)))
            .Where(v => v.Light > 0)
            .ToList();

        if (byId.Count == 0)
            return volumes.Infos.Keys.Select(k => k.Id).DefaultIfEmpty(-1).Min();

        var mostBakes = byId.Max(v => v.Bakes);
        var candidates = byId.Where(v => v.Bakes == mostBakes).OrderBy(v => v.Light).ToList();

        return candidates[candidates.Count / 2].Id;
    }

    /// <summary>
    /// Queues a bake again if its textures were loaded and have been unloaded since, or never arrived, once no load
    /// is running, a few times at most
    /// </summary>
    private void RetryIfMissing(IrradianceVolumeBank.MapVolumes volumes, int bake)
    {
        if (!volumes.HasVolume(VolumeId, bake) || !ResourceManager.ActiveJobProgress.IsEmpty ||
            IrradianceVolumeBank.TryGetBakeTextures(MapId, VolumeId, bake, out _))
            return;

        var key = (MapId, bake);
        var now = DateTime.UtcNow;

        if (Retries.TryGetValue(key, out var retry) && (retry.Count >= MaxRetries || now - retry.Time < RetryInterval))
            return;

        Retries[key] = (now, retry.Count + 1);

        VolumeBank.ForgetBake(MapId, bake);
        VolumeBank.RequestBake(MapId, bake);
    }

    /// <summary>
    /// Takes a reference on the volume's textures of the bakes lit with, and lets go of any others
    /// </summary>
    private void HoldTextures(int bake, int blendBake)
    {
        var wanted = new HashSet<string>(IrradianceVolumeBank.GetTextureNames(MapId, VolumeId, bake),
            StringComparer.OrdinalIgnoreCase);

        if (blendBake >= 0)
            wanted.UnionWith(IrradianceVolumeBank.GetTextureNames(MapId, VolumeId, blendBake));

        ReleaseTextures(wanted);

        foreach (var name in wanted)
        {
            if (HeldTextures.ContainsKey(name))
                continue;

            if (ResourceManager.DefaultPool.TryFind<TextureResource>(name, out var handle) && handle?.Get() != null)
            {
                handle.Acquire();
                HeldTextures[name] = handle;
            }
        }
    }

    private void ReleaseTextures(HashSet<string> keep)
    {
        foreach (var name in HeldTextures.Keys.Where(n => !keep.Contains(n)).ToList())
        {
            HeldTextures[name].Release();
            HeldTextures.Remove(name);
        }
    }

    /// <summary>
    /// Gives the shaders the volume stretched over the box, or none
    /// </summary>
    private void UploadVolume(IrradianceVolumeBank.MapVolumes volumes, int bake, (int Bake, float Ratio) blend)
    {
        VolumeShaded = false;

        if (volumes != null && bake >= 0)
        {
            // The region's unit box has its position at the bottom centre; it starts a little below the origin, so
            // models standing on it are inside
            var size = Math.Max(CFG.Current.Viewport_Environment_Volume_Size, 1.0f);
            var world = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(0.0f, -size * 0.1f, 0.0f);

            if (IrradianceVolumeBank.TryBuildEntry(MapId, volumes, VolumeId, bake, blend.Bake, blend.Ratio, world,
                    size * 0.1f, 1.0f, 1.0f, out var data))
            {
                Entries[0] = data;
                VolumeShaded = true;

                // As in the Map Editor, the volume's average light becomes the viewport's default ambient light
                var luminance = volumes.Infos[(VolumeId, bake)].ShLuminance.W;
                Table.AutoExposure = luminance > 0 ? (float)(IrradianceVolumeBank.AmbientTarget / luminance) : 1.0f;
            }
        }

        if (!VolumeShaded)
            Table.AutoExposure = 1.0f;

        Table.Upload(Entries, VolumeShaded ? 1 : 0);
    }

    public void Dispose()
    {
        ReleaseTextures(new HashSet<string>());
        Table?.Dispose();
        Table = null;
    }
}
