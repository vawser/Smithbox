using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The scene of a Gparam Editor view's viewport: the map pieces and assets of one map, for its GPARAM to light.
/// They are only drawn, not entities, so the map can't be edited here, and it is read on its own, so the Map Editor's
/// copy of the map, loaded or not, isn't touched. The map is placed at the origin, without the offset of a tile.
/// </summary>
public class GparamUniverse : IUniverse
{
    public GparamEditorView View;
    public ProjectEntry Project;
    public RenderScene RenderScene;

    public ViewportSelection Selection { get; }

    /// <summary>The map drawn, null if none</summary>
    public string LoadedMapId { get; private set; }

    /// <summary>The map being read, null if none</summary>
    public string LoadingMapId { get; private set; }

    /// <summary>How many parts of the map are drawn</summary>
    public int PartCount => Meshes.Count;

    private readonly List<MeshRenderableProxy> Meshes = new();

    // The map piece meshes, which the camera is framed on, as assets can be placed far out
    private readonly List<MeshRenderableProxy> PieceMeshes = new();

    // The mesh providers of the map's parts are cached under this, so they can be dropped when it is unloaded
    private string MeshUid;

    // Counts the loads, so a load overtaken by another, or by an unload, is ignored when it finishes
    private int Generation;

    // The map read off the main thread, built into meshes on it
    private volatile PendingMap Pending;

    // Set once the map's resources are loaded, for the camera to be framed on it
    private volatile bool ResourcesLoaded;
    private int ResourcesLoadedGeneration = -1;
    private DateTime ResourcesLoadedTime;
    private bool PendingFrame;

    // How long the meshes are waited for after their files load, before the camera is framed on those that did
    private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(3);

    // The map each GPARAM is shown with, see FindMapForFile
    private readonly Dictionary<string, string> MapsForFiles = new(StringComparer.OrdinalIgnoreCase);

    private sealed record PendingMap(int Generation, string MapId, IMsb Msb, List<BTL> Btls);

    /// <summary>The map's irradiance volumes, lightmaps and BTL lights</summary>
    public GparamMapEnvironment Environment;

    public GparamUniverse(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        RenderScene = view.RenderScene;
        Selection = view.ViewportSelection;

        Environment = new GparamMapEnvironment(project);
    }

    /// <summary>
    /// Whether the project's maps can be read here: the Map Editor's data has to be loaded
    /// </summary>
    public bool CanLoadMaps()
    {
        return Project.Handler.MapData?.PrimaryBank?.Maps != null &&
               MsbGameAdapter.For(Project.Descriptor.ProjectType) != null &&
               Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan &&
               RenderScene != null;
    }

    /// <summary>
    /// The project's maps, by name
    /// </summary>
    public List<string> ListMaps()
    {
        var maps = Project.Handler.MapData?.PrimaryBank?.Maps;

        if (maps == null)
            return new List<string>();

        // The list is drawn every frame, so it is only sorted again when the maps change
        if (CachedMapList == null || CachedMapCount != maps.Count)
        {
            CachedMapCount = maps.Count;
            CachedMapList = maps.Keys
                .Select(e => e.Filename)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return CachedMapList;
    }

    private List<string> CachedMapList;
    private int CachedMapCount = -1;

    /// <summary>
    /// The map a GPARAM is shown with: for mAA_BB_CCWW, the area's mAA_BB_00_00 if there is one, else the first
    /// map of the area whose name ends in _00 (the small tile of an open world map), else the first map whose
    /// GPARAMs are the area's (see <see cref="GparamLightingBank.GetArea" />). Null for a shared GPARAM, such as
    /// mapbase, which is no map's.
    /// </summary>
    public string FindMapForFile(string file)
    {
        if (string.IsNullOrEmpty(file) || file.Length < 6 || file[0] is not ('m' or 'M'))
            return null;

        if (MapsForFiles.TryGetValue(file, out var cached))
            return cached;

        var area = file.Substring(0, 6);
        var maps = ListMaps();

        var preferred = $"{area}_00_00";

        var map = maps.FirstOrDefault(m => m.Equals(preferred, StringComparison.OrdinalIgnoreCase))
            ?? maps.FirstOrDefault(m => m.StartsWith($"{area}_", StringComparison.OrdinalIgnoreCase) &&
                                        m.EndsWith("_00", StringComparison.OrdinalIgnoreCase))
            ?? maps.FirstOrDefault(m => m.StartsWith($"{area}_", StringComparison.OrdinalIgnoreCase));

        if (map == null)
        {
            var bank = View.ViewportWindow?.LightingBank;

            if (bank != null)
            {
                var inArea = maps.Where(m => string.Equals(bank.GetArea(m), area, StringComparison.OrdinalIgnoreCase)).ToList();

                map = inArea.FirstOrDefault(m => m.EndsWith("_00", StringComparison.OrdinalIgnoreCase)) ?? inArea.FirstOrDefault();
            }
        }

        MapsForFiles[file] = map;

        return map;
    }

    /// <summary>
    /// Reads a map in place of the one drawn. Its MSB is read off the main thread, and its meshes built on it, in
    /// <see cref="Update" />.
    /// </summary>
    public void LoadMap(string mapId)
    {
        if (string.IsNullOrEmpty(mapId) || !CanLoadMaps())
            return;

        var maps = Project.Handler.MapData.PrimaryBank.Maps;
        var entry = maps.FirstOrDefault(e => e.Key.Filename.Equals(mapId, StringComparison.OrdinalIgnoreCase));

        if (entry.Value == null)
        {
            Smithbox.LogError(this, LOC.Get("GPARAM_Viewport_Map_Not_Found", mapId));
            return;
        }

        UnloadMap();

        var generation = Generation;
        var wrapper = entry.Value;

        // The Map Editor's copy if it has read the map, which is only read from here, else a copy of our own,
        // so the Map Editor's isn't replaced
        var existing = wrapper.MSB;
        var path = wrapper.Path;
        var fs = wrapper.TargetFS;
        var type = Project.Descriptor.ProjectType;

        LoadingMapId = mapId;

        Task.Run(() =>
        {
            IMsb msb = existing;
            List<BTL> btls = new();

            try
            {
                if (msb == null && fs.FileExists(path))
                {
                    msb = MsbGameAdapter.For(type).Read(fs.ReadFile(path).Value);
                }

                // What its lighting needs: the volumes' IVINFOs and the BTL lights
                if (msb != null)
                {
                    btls = Environment.ReadFiles(mapId);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("GPARAM_Viewport_Map_Read_Failed", mapId), e);
                msb = null;
            }

            Pending = new PendingMap(generation, mapId, msb, btls);
        });
    }

    /// <summary>
    /// Removes the map drawn, and stops any map being read from being drawn
    /// </summary>
    public void UnloadMap()
    {
        Generation++;

        Pending = null;
        LoadingMapId = null;
        PendingFrame = false;
        ResourcesLoaded = false;

        // Before the meshes go, so their lightmaps are taken off them
        Environment.Clear();

        View.ViewportSelection.ClearSelection();

        foreach (var mesh in Meshes)
        {
            mesh.Dispose();
        }

        Meshes.Clear();
        PieceMeshes.Clear();

        if (MeshUid != null)
        {
            MeshProviderCache.InvalidateUidEntries(MeshUid);
            MeshUid = null;
        }

        if (LoadedMapId != null)
        {
            LoadedMapId = null;

            ResourceManager.ClearUnusedResources();
        }
    }

    /// <summary>
    /// Builds the meshes of a map once it is read, and frames the camera on it once they have loaded. Called on
    /// the main thread every frame.
    /// </summary>
    public void Update()
    {
        var pending = Pending;

        if (pending != null)
        {
            Pending = null;

            if (pending.Generation == Generation)
            {
                LoadingMapId = null;

                if (pending.Msb != null)
                {
                    BuildMap(pending.MapId, pending.Msb, pending.Btls);
                }
            }
        }

        UpdatePendingFrame();
    }

    /// <summary>
    /// Moves the camera so the whole map is in view
    /// </summary>
    public void FrameMap()
    {
        if (TryGetBounds(includeUnavailable: true, out var bounds))
        {
            View.ViewportWindow.Viewport?.FitBox(bounds);
        }
    }

    private void BuildMap(string mapId, IMsb msb, List<BTL> btls)
    {
        var amapid = PathBuilder.GetAssetMapID(Project, mapId);
        var type = Project.Descriptor.ProjectType;

        MeshUid = $"GparamPreview_{View.ViewIndex}_{Generation}";
        LoadedMapId = mapId;

        // Instances of a model share its provider, as there are no model masks here
        var pieceAssets = new HashSet<ResourceDescriptor>();
        var assetAssets = new HashSet<ResourceDescriptor>();
        var mapTextures = new HashSet<ResourceDescriptor>();
        var assetTextures = new HashSet<ResourceDescriptor>();
        var otherTextures = new HashSet<ResourceDescriptor>();

        // DS3's map pieces, which its lightmaps go on
        var lightmapPieces = new List<(MSB3.Part.MapPiece Piece, MeshRenderableProxy Proxy)>();

        var speedTrees = Project.Handler.MapData?.SpeedTreeList?.Entries
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in msb.Parts.GetEntries())
        {
            var isPiece = EntityHelper.IsPartMapPiece(part);
            var isAsset = !isPiece && CFG.Current.GparamEditor_Viewport_Load_Assets && EntityHelper.IsPartAsset(part);

            if (!isPiece && !isAsset)
                continue;

            var modelName = part.ModelName;

            if (string.IsNullOrEmpty(modelName))
                continue;

            ResourceDescriptor resource;
            RenderFilter filter;

            if (isPiece)
            {
                var name = ModelLocator.MapModelNameToAssetName(Project, amapid, modelName);
                resource = ModelLocator.GetMapModel(Project, amapid, name, name);
                filter = RenderFilter.MapPiece;
            }
            else
            {
                var isSpeedTree = speedTrees.Contains(modelName);

                // SpeedTree vertices can only be read for these games; the Map Editor draws a placeholder for the others
                if (isSpeedTree && type is not (ProjectType.ER or ProjectType.NR))
                    continue;

                resource = ModelLocator.GetObjModel(Project, modelName, modelName);
                filter = isSpeedTree ? RenderFilter.SpeedTree : RenderFilter.Object;

                foreach (var texture in MapResourceHandler.GetModelTextureAssets(Project, modelName))
                {
                    assetTextures.Add(texture);
                }
            }

            if (resource == null || !resource.IsValid())
                continue;

            var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(RenderScene,
                ResourceManager.DefaultPool.Find<FlverResource>(resource.Name), ModelMarkerType.None, MeshUid, null);

            mesh.World = GparamMapEnvironment.GetWorldMatrix(part, part.Scale);
            mesh.DrawFilter = filter;

            ApplyShadowFlags(mesh, part);

            Meshes.Add(mesh);

            if (isPiece)
            {
                PieceMeshes.Add(mesh);
                pieceAssets.Add(resource);

                if (part is MSB3.Part.MapPiece ds3Piece)
                    lightmapPieces.Add((ds3Piece, mesh));
            }
            else
            {
                assetAssets.Add(resource);
            }
        }

        // The textures the Map Editor loads with a map, other than those of characters, collisions and volumes
        foreach (var texture in TextureLocator.GetMapTextureVirtualPaths(Project, amapid))
        {
            if (texture.IsValid())
                mapTextures.Add(texture);
        }

        if (type is ProjectType.ER or ProjectType.AC6 or ProjectType.NR or ProjectType.DB_NT or ProjectType.DB)
        {
            var aat = TextureLocator.GetCharacterCommonTextureVirtualPath(Project, "common_body");

            if (aat.IsValid())
                otherTextures.Add(aat);
        }

        if (type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.DS3 or ProjectType.BB
            or ProjectType.NR or ProjectType.DB_NT or ProjectType.DB)
        {
            var systex = TextureLocator.GetSystexTextureVirtualPath(Project, "systex");

            if (systex.IsValid())
                otherTextures.Add(systex);
        }

        // The volumes' bakes and the lightmaps, and the lights, volume boxes, probes and atlas entries they need
        foreach (var texture in Environment.SetMap(mapId, msb, btls, lightmapPieces))
        {
            otherTextures.Add(texture);
        }

        // Temporary diagnostics until the viewport is checked in the app
        Smithbox.Log(this, $"[Gparam Viewport] {mapId}: {Meshes.Count} meshes ({PieceMeshes.Count} map pieces) from " +
            $"{msb.Parts.GetEntries().Count} parts, {pieceAssets.Count} piece models, {assetAssets.Count} asset models, " +
            $"{mapTextures.Count + assetTextures.Count + otherTextures.Count} texture containers");

        _ = LoadResources(Generation, mapId, pieceAssets, assetAssets, mapTextures, assetTextures, otherTextures);
    }

    private async Task LoadResources(int generation, string mapId,
        HashSet<ResourceDescriptor> pieceAssets, HashSet<ResourceDescriptor> assetAssets,
        HashSet<ResourceDescriptor> mapTextures, HashSet<ResourceDescriptor> assetTextures,
        HashSet<ResourceDescriptor> otherTextures)
    {
        var session = $"{mapId} (GPARAM)";

        var pieceJob = LOC.Get("MAP_ResourceManager_Map_Piece_Models");
        var assetJob = MapResourceHandler.GetObjectModelsName(Project);
        var mapTextureJob = LOC.Get("MAP_ResourceManager_Map_Textures");
        var assetTextureJob = MapResourceHandler.GetObjectTexturesName(Project);
        var otherTextureJob = LOC.Get("MAP_ResourceManager_Miscellaneous_Textures");

        var texturing = CFG.Current.Viewport_Enable_Texturing;

        ResourceLoadTracker.BeginSession(session);

        try
        {
            void Reserve(bool enabled, HashSet<ResourceDescriptor> assets, string name)
            {
                if (enabled && assets.Count > 0)
                    ResourceLoadTracker.Reserve(name);
            }

            Reserve(true, pieceAssets, pieceJob);
            Reserve(true, assetAssets, assetJob);
            Reserve(texturing, mapTextures, mapTextureJob);
            Reserve(texturing, assetTextures, assetTextureJob);
            Reserve(texturing, otherTextures, otherTextureJob);

            // Model archives can also contain texture packs, so those are loaded along with the models
            var modelTypes = ResourceType.Flver | ResourceType.Texture;

            await Task.WhenAll(
                LoadAssets(pieceAssets, pieceJob, modelTypes),
                LoadAssets(assetAssets, assetJob, modelTypes));

            if (texturing)
            {
                await Task.WhenAll(
                    LoadAssets(mapTextures, mapTextureJob, ResourceType.Texture),
                    LoadAssets(assetTextures, assetTextureJob, ResourceType.Texture),
                    LoadAssets(otherTextures, otherTextureJob, ResourceType.Texture));

                // Inside the session, so the textures found for the models are part of this load
                await ResourceManager.LoadMissingTexturesAsync();
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("GPARAM_Viewport_Map_Load_Failed", mapId), e);
        }
        finally
        {
            ResourceLoadTracker.EndSession(session);
        }

        ResourcesLoadedGeneration = generation;
        ResourcesLoaded = true;
    }

    private Task LoadAssets(HashSet<ResourceDescriptor> assets, string jobName, ResourceType types)
    {
        if (assets.Count == 0)
            return Task.CompletedTask;

        var options = new LoadOptions
        {
            JobName = jobName,
            Types = types,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly
        };

        var load = ResourceManager.DefaultPool.LoadAssets(Project, assets, options);
        ResourceLoadTracker.MarkRequested(jobName);

        return load;
    }

    /// <summary>
    /// The meshes' bounds are only known once the resource manager has committed them, some frames after their
    /// files load, so the camera waits for them, or for the timeout if some never do
    /// </summary>
    private void UpdatePendingFrame()
    {
        if (ResourcesLoaded)
        {
            ResourcesLoaded = false;

            if (ResourcesLoadedGeneration == Generation)
            {
                ResourcesLoadedTime = DateTime.UtcNow;
                PendingFrame = true;
            }
        }

        if (!PendingFrame)
            return;

        var timedOut = DateTime.UtcNow - ResourcesLoadedTime > FrameTimeout;

        if (!timedOut && Meshes.Any(m => !m._meshProvider.IsAvailable()))
            return;

        PendingFrame = false;

        if (TryGetBounds(includeUnavailable: false, out var bounds))
        {
            View.ViewportWindow.Viewport?.FitBox(bounds);

            // Temporary diagnostics until the viewport is checked in the app
            var camera = (View.ViewportWindow.Viewport as VulkanViewport)?.ViewportCamera.CameraTransform.Position;
            Smithbox.Log(this, $"[Gparam Viewport] Framed {LoadedMapId}: {Meshes.Count(m => m._meshProvider.IsAvailable())} of " +
                $"{Meshes.Count} meshes available, bounds {bounds.Min} to {bounds.Max}, camera at {camera}");
        }
        else
        {
            // Temporary diagnostics until the viewport is checked in the app
            Smithbox.Log(this, $"[Gparam Viewport] {LoadedMapId}: no mesh had bounds to frame " +
                $"({Meshes.Count(m => m._meshProvider.IsAvailable())} of {Meshes.Count} available)");
        }
    }

    private bool TryGetBounds(bool includeUnavailable, out BoundingBox bounds)
    {
        bounds = new BoundingBox();

        var meshes = PieceMeshes.Count > 0 ? PieceMeshes : Meshes;
        var hasBounds = false;

        foreach (var mesh in meshes)
        {
            if (!includeUnavailable && !mesh._meshProvider.IsAvailable())
                continue;

            if (!mesh._meshProvider.IsAvailable() || !mesh.HasRenderableGeometry())
                continue;

            var meshBounds = mesh.GetBounds();

            bounds = hasBounds ? BoundingBox.Combine(bounds, meshBounds) : meshBounds;
            hasBounds = true;
        }

        return hasBounds;
    }

    /// <summary>
    /// Has the mesh cast and receive the sun's shadow as the part's MSB flags say, as the Map Editor does (see
    /// MsbEntity.RefreshShadowFlags)
    /// </summary>
    private static void ApplyShadowFlags(MeshRenderableProxy mesh, IMsbPart part)
    {
        if (!CFG.Current.Viewport_Shadows_Use_Msb_Flags)
        {
            mesh.CastsShadow = true;
            mesh.ReceivesShadow = true;
            return;
        }

        bool? ReadFlag(string name)
        {
            return part.GetType().GetProperty(name)?.GetValue(part) switch
            {
                bool b => b,
                byte b => b != 0,
                sbyte b => b != 0,
                int i => i != 0,
                _ => null
            };
        }

        // Elden Ring: an override byte, then the value it applies (see MsbEntity.RefreshShadowFlags)
        bool? Overridden(string over, string value, bool fallback)
            => ReadFlag(over) is bool on ? (on ? ReadFlag(value) ?? fallback : fallback) : null;

        var source = Overridden("OverrideDirLightShadowSrc", "IsDirLightShadowSrc", true)
            ?? ReadFlag("IsShadowSrc") ?? ReadFlag("ShadowSource") ?? true;
        var only = Overridden("OverrideShadowOnly", "IsShadowOnly", false) ?? ReadFlag("IsShadowOnly") ?? false;
        var dest = Overridden("OverrideShadowDest", "IsShadowDest", true)
            ?? ReadFlag("IsShadowDest") ?? ReadFlag("ShadowDest") ?? true;

        mesh.CastsShadow = source || only;
        mesh.ReceivesShadow = dest;
    }
}
