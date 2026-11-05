using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The baked and placed light of the map a Gparam Editor view shows, lit as the Map Editor lights its maps:
/// <list type="bullet">
/// <item>the irradiance volumes of its EnvMapEffectBoxes (AC6, ER, NR) and its light probe grids (DS3, BB), in a
/// volume table of this view's own, so the Map Editor's isn't touched (<see cref="IrradianceVolumeBank" />)</item>
/// <item>DS3's lightmaps of its map pieces, placed in their atlases by its BTAB (<see cref="DolLightmapBank" />)</item>
/// <item>its BTL lights near the camera (see <see cref="MapLightCollector" />)</item>
/// </list>
/// The banks are this view's own, so its map can be one the Map Editor has loaded too. The light and lightmap tables
/// are shared by every viewport, as each entry is a part's or a light's: the editor in focus fills them each frame,
/// and only its viewports are drawn.
/// </summary>
public class GparamMapEnvironment : IDisposable
{
    public ProjectEntry Project;

    public IrradianceVolumeBank VolumeBank;

    /// <summary>The volume table the viewport lights with, null if the renderer has no material buffer</summary>
    public IrradianceVolumeTable VolumeTable;

    public DolLightmapBank LightmapBank;

    /// <summary>The map lit, null if none</summary>
    public string MapId { get; private set; }

    /// <summary>How many volumes and probe grids lit the viewport last frame</summary>
    public int ShadedVolumeCount => VolumeBank.ShadedVolumeCount;

    /// <summary>How many of the map's BTL lights it has, and how many were in range and lit the viewport last frame</summary>
    public int LightCount => Lights.Count;
    public int InRangeCount { get; private set; }
    public int ShadedLightCount { get; private set; }

    /// <summary>How many map pieces are lit with lightmaps</summary>
    public int LightmappedPartCount => LightmapBank.LightmappedPartCount;

    // The map's EnvMapEffectBoxes, light probe groups, BTL lights, map pieces and BTAB entries
    private readonly List<(object Region, Func<Matrix4x4> GetWorld)> Regions = new();
    private readonly List<(Matrix4x4 World, BTPB.Group Group)> ProbeGroups = new();
    private readonly List<(BTL.Light Light, Matrix4x4 World)> Lights = new();
    private readonly List<DolLightmapBank.LightmapPart> LightmapParts = new();
    private Dictionary<string, List<BTAB.Entry>> Atlas = new(StringComparer.OrdinalIgnoreCase);

    private readonly MapLightData[] LightEntries = new MapLightData[SceneLightTable.Capacity];

    /// <summary>The file of each bake read, as the Map Editor reads them: _0000 when a map has several</summary>
    private const string Bake = "_0000";

    public GparamMapEnvironment(ProjectEntry project)
    {
        Project = project;

        VolumeBank = new IrradianceVolumeBank(project);
        LightmapBank = new DolLightmapBank(project);

        if (SceneRenderer.MaterialBufferAllocator != null)
        {
            VolumeTable = new IrradianceVolumeTable(SceneRenderer.MaterialBufferAllocator);
        }
    }

    /// <summary>
    /// What a map's lighting needs read from files, done off the main thread with the map's MSB: its IVINFOs, kept
    /// in <see cref="VolumeBank" />, and its BTL files
    /// </summary>
    public List<BTL> ReadFiles(string mapId)
    {
        VolumeBank.LoadVolumes(mapId);

        var btls = new List<BTL>();

        var isDs2 = Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S;
        var files = isDs2 ? Project.Locator.DS2_LightFiles : Project.Locator.LightFiles;

        // As the Map Editor finds them: DS2's by the map's world block, the others by its name
        var match = isDs2 ? mapId.Substring(1) : mapId;

        foreach (var entry in files.Entries)
        {
            if (!entry.Filename.Contains(match, StringComparison.OrdinalIgnoreCase))
                continue;

            if (MapUniverse.ReadBTL(Project, entry) is { } btl)
                btls.Add(btl);
        }

        return btls;
    }

    /// <summary>
    /// Takes the map's lighting from its MSB, its BTLs and the Map Editor's banks of BTAB and BTPB files, which are
    /// only read from. The map is at the origin, as the viewport draws it. Returns the textures to load with the
    /// map: the volumes' bakes and DS3's lightmaps.
    /// </summary>
    /// <param name="pieces">The map's DS3 map pieces and their meshes, which the lightmaps go on</param>
    public List<ResourceDescriptor> SetMap(string mapId, IMsb msb, List<BTL> btls,
        IEnumerable<(MSB3.Part.MapPiece Piece, MeshRenderableProxy Proxy)> pieces)
    {
        Clear();

        MapId = mapId;

        var textures = new List<ResourceDescriptor>();

        // Irradiance volumes: the boxes linked to them, of which those the bank links are lit with
        foreach (var region in msb.Regions.GetEntries())
        {
            if (IrradianceVolumeBank.GetVolumeIdProperty(region) == null)
                continue;

            var world = GetRegionWorldMatrix(region);
            Regions.Add((region, () => world));
        }

        // The bakes the Map Editor would load with the map; others are queued when the volumes change to them
        if (VolumeBank.CanUse())
        {
            var bakeFiles = VolumeBank.GetBakeFiles(mapId);

            foreach (var bake in VolumeBank.GetBakesToLoad(mapId))
            {
                if (!bakeFiles.TryGetValue(bake, out var files))
                    continue;

                VolumeBank.MarkBakeRequested(mapId, bake);

                textures.AddRange(files.Select(f => TextureLocator.GetEnvMapTextureVirtualPath(mapId, f.Filename)));
            }
        }

        var mapData = Project.Handler.MapData;

        // Light probe grids (DS3, BB), placed by the map's offset, which is none here
        if (mapData?.LightProbeBank is { } probeBank && probeBank.CanUse() &&
            ChooseFile(probeBank.Files, mapId) is { } btpb)
        {
            foreach (var group in btpb.Groups)
            {
                ProbeGroups.Add((Matrix4x4.Identity, group));
            }
        }

        // DS3's lightmaps: the area's binder, and the map's atlas entries by part
        if (LightmapBank.CanUse())
        {
            var amapid = PathBuilder.GetAssetMapID(Project, mapId);
            var lightmaps = TextureLocator.GetDolLightmapVirtualPath(amapid);

            if (lightmaps.IsValid())
                textures.Add(lightmaps);

            if (mapData?.LightAtlasBank is { } atlasBank && atlasBank.CanUse() &&
                ChooseFile(atlasBank.Files, mapId) is { } btab)
            {
                foreach (var entry in btab.Entries)
                {
                    if (string.IsNullOrEmpty(entry.PartName))
                        continue;

                    if (!Atlas.TryGetValue(entry.PartName, out var list))
                    {
                        list = new List<BTAB.Entry>();
                        Atlas[entry.PartName] = list;
                    }

                    list.Add(entry);
                }
            }

            foreach (var (piece, proxy) in pieces)
            {
                LightmapParts.Add(new DolLightmapBank.LightmapPart(piece, piece, proxy));
            }
        }

        // BTL lights
        foreach (var btl in btls)
        {
            foreach (var light in btl.Lights)
            {
                Lights.Add((light, GetWorldMatrix(light, Vector3.One)));
            }
        }

        return textures;
    }

    /// <summary>
    /// Forgets the map, giving back its lightmap entries while its meshes are still there
    /// </summary>
    public void Clear()
    {
        if (MapId != null)
        {
            VolumeBank.OnUnloadMap(MapId);
        }

        LightmapBank.Reset();

        MapId = null;

        Regions.Clear();
        ProbeGroups.Clear();
        Lights.Clear();
        LightmapParts.Clear();
        Atlas = new Dictionary<string, List<BTAB.Entry>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Fills the volume table, the lightmaps and the lights for this frame. Called on the main thread every frame
    /// the editor is in focus, after the lighting is resolved.
    /// </summary>
    /// <param name="lighting">The lighting the viewport is lit with this frame, or null</param>
    /// <param name="hours">The time of day it is lit at, which ER's bakes follow when set to</param>
    /// <param name="eye">The camera's position, which the lights are turned on around, or null</param>
    public void Update(GparamLighting lighting, float hours, Vector3? eye)
    {
        UpdateVolumes(lighting, hours);
        UpdateLightmaps(lighting);
        UpdateLights(lighting, eye);
    }

    private void UpdateVolumes(GparamLighting lighting, float hours)
    {
        if (VolumeTable == null)
            return;

        var sources = new List<IrradianceVolumeBank.VolumeSource>();
        var (bake, blendBake, blendRatio) = (0, -1, 0.0f);

        if (MapId != null)
        {
            (bake, blendBake, blendRatio) = VolumeBank.ResolveBakes(new[] { MapId }, hours);

            // Only queues the bakes the first time they are asked for
            if (VolumeBank.CanUse())
            {
                VolumeBank.RequestBake(MapId, bake);

                if (blendBake >= 0)
                    VolumeBank.RequestBake(MapId, blendBake);
            }

            sources.Add(new IrradianceVolumeBank.VolumeSource(MapId, Regions, ProbeGroups));
        }

        VolumeBank.UpdateTable(VolumeTable, sources, bake, blendBake, blendRatio, lighting?.ShColor ?? Vector3.One);
    }

    private void UpdateLightmaps(GparamLighting lighting)
    {
        if (MapId == null || LightmapParts.Count == 0)
        {
            LightmapBank.Update(Array.Empty<DolLightmapBank.LightmapMap>(), lighting);
            return;
        }

        var atlas = Atlas;

        LightmapBank.Update(new[] { new DolLightmapBank.LightmapMap(MapId, LightmapParts, () => atlas) }, lighting);
    }

    /// <summary>
    /// The map's BTL lights near the camera, nearest first, up to the viewport's limit, as the Map Editor turns them on
    /// </summary>
    private void UpdateLights(GparamLighting lighting, Vector3? eye)
    {
        var table = SceneRenderer.MapLights;

        if (table == null)
            return;

        var found = new List<(float Distance, MapLightData Data)>();

        var radius = Math.Max(CFG.Current.Viewport_BTL_Light_Radius, 0.0f);
        var intensityScale = MapLightCollector.GetIntensityScale(lighting);

        foreach (var (light, world) in Lights)
        {
            var distance = eye.HasValue ? Vector3.Distance(world.Translation, eye.Value) : float.MaxValue;

            if (!MapLightCollector.IsNearCamera(light, distance, radius))
                continue;

            found.Add((distance, MapLightCollector.CreateLight(light, world, intensityScale).ToMapGpu()));
        }

        found.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        InRangeCount = found.Count;

        var count = Math.Min(found.Count, Math.Clamp(CFG.Current.Viewport_BTL_Light_Max_Count, 0, SceneLightTable.Capacity));

        for (var i = 0; i < count; i++)
            LightEntries[i] = found[i].Data;

        ShadedLightCount = count;

        table.Upload(LightEntries, count);
    }

    /// <summary>
    /// The file of a map in one of the Map Editor's banks of BTAB or BTPB files, named mAA_BB_CC_DD_NNNN: the _0000
    /// one if the map has several, as the lightmaps and probes read, else the first
    /// </summary>
    private T ChooseFile<T>(Dictionary<string, T> files, string mapId) where T : class
    {
        // DS2's are named by the map's world block
        var match = Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S ? mapId.Substring(1) : mapId;

        var names = files.Keys
            .Where(k => k.Contains(match, StringComparison.OrdinalIgnoreCase))
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var name = names.FirstOrDefault(k => k.EndsWith(Bake, StringComparison.OrdinalIgnoreCase)) ?? names.FirstOrDefault();

        return name != null ? files[name] : null;
    }

    /// <summary>
    /// A region's placement, its shape's size as its scale, as the Map Editor draws its regions (MsbEntity's
    /// GetLocalTransform): a Box's unit box is scaled by its width, height and depth
    /// </summary>
    private static Matrix4x4 GetRegionWorldMatrix(IMsbRegion region)
    {
        var scale = region.Shape switch
        {
            MSB.Shape.Box box => new Vector3(box.Width, box.Height, box.Depth),
            MSB.Shape.Sphere sphere => new Vector3(sphere.Radius),
            MSB.Shape.Cylinder cylinder => new Vector3(cylinder.Radius, cylinder.Height, cylinder.Radius),
            _ => Vector3.One
        };

        return GetWorldMatrix(region, scale);
    }

    /// <summary>
    /// The placement of an MSB part or region, or a BTL light, as the Map Editor reads it (Entity.GetLocalTransform):
    /// its Position, and its Rotation in degrees unless marked as radians, in the XZY order where marked so
    /// </summary>
    public static Matrix4x4 GetWorldMatrix(object placed, Vector3 scale)
    {
        var type = placed.GetType();

        var transform = Transform.Default;

        if (type.GetProperty("Position")?.GetValue(placed) is Vector3 position)
            transform.Position = position;

        var property = type.GetProperty("Rotation");

        if (property?.GetValue(placed) is Vector3 rotation)
        {
            var isRadians = property.GetCustomAttribute<RotationRadians>() != null;
            var isXzy = property.GetCustomAttribute<RotationXZY>() != null;

            if (!isRadians)
            {
                rotation = new Vector3(
                    float.DegreesToRadians(rotation.X),
                    float.DegreesToRadians(rotation.Y),
                    float.DegreesToRadians(rotation.Z));
            }

            if (isXzy)
                transform.EulerRotationXZY = rotation;
            else
                transform.EulerRotation = rotation;
        }

        transform.Scale = scale;

        return transform.WorldMatrix;
    }

    public void Dispose()
    {
        Clear();

        VolumeTable?.Dispose();
        VolumeTable = null;
    }
}
