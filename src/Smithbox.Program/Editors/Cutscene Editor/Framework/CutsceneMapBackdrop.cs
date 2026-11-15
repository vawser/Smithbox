#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The map behind a cut: the map pieces and assets of one map, drawn but not entities, so nothing of the map can be
/// changed here. The map is placed at the origin, as offset.sib places cuts in the space of their map (Godrick's cut
/// s10_00_0020 stands where m10_00_00_00's c4750 enemy does; see Cutscene Editor Analysis, phase 3).
/// <para>
/// A copy of the Gparam Editor's map loader (GparamUniverse), without its lighting environment, kept apart so the
/// two editors don't depend on each other. The MSB is the Map Editor's copy if it has read the map, else one read here.
/// </para>
/// </summary>
public class CutsceneMapBackdrop
{
    private readonly ProjectEntry Project;
    private readonly Func<RenderScene?> GetScene;

    /// <summary>The map drawn, null if none</summary>
    public string? LoadedMapId { get; private set; }

    /// <summary>The map being read, null if none</summary>
    public string? LoadingMapId { get; private set; }

    public int PartCount => Meshes.Count;
    public int PieceCount { get; private set; }

    private readonly List<MeshRenderableProxy> Meshes = new();

    // Only the latest map read is built: reading another one, or unloading, moves this on
    private int Generation;
    private PendingMap? Pending;
    private string? MeshUid;

    private sealed record PendingMap(int Generation, string MapId, IMsb? Msb);

    public CutsceneMapBackdrop(ProjectEntry project, Func<RenderScene?> getScene)
    {
        Project = project;
        GetScene = getScene;
    }

    /// <summary>Whether maps can be read: the Map Editor's data has to be loaded, and the viewport be Vulkan</summary>
    public bool CanLoadMaps()
    {
        return Project.Handler.MapData?.PrimaryBank?.Maps != null &&
               MsbGameAdapter.For(Project.Descriptor.ProjectType) != null &&
               Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan &&
               GetScene() != null;
    }

    private List<string>? CachedMapList;
    private int CachedMapCount = -1;

    /// <summary>The project's maps, by name</summary>
    public List<string> ListMaps()
    {
        var maps = Project.Handler.MapData?.PrimaryBank?.Maps;
        if (maps == null)
            return new List<string>();

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

    public bool HasMap(string mapId) => ListMaps().Contains(mapId, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The map a cutscene belongs to by its name: sAA_BB_CCCC plays in mAA_BB_00_00, the map whose EMEVD plays it.
    /// Null for the open world (s60, s61), whose cutscenes common.emevd plays: their tile has to be chosen.
    /// </summary>
    public static string? ImpliedMap(string cutsceneName)
    {
        var parts = cutsceneName.Split('_');
        if (parts.Length < 3 || parts[0].Length != 3 || !parts[0].StartsWith('s'))
            return null;

        var area = parts[0][1..];
        if (area is "60" or "61")
            return null;

        return $"m{area}_{parts[1]}_00_00";
    }

    /// <summary>Reads a map in place of the one drawn. The MSB is read off the main thread, the meshes made in Update.</summary>
    public void LoadMap(string mapId)
    {
        if (string.IsNullOrEmpty(mapId) || !CanLoadMaps())
            return;

        if (mapId.Equals(LoadedMapId, StringComparison.OrdinalIgnoreCase) || mapId.Equals(LoadingMapId, StringComparison.OrdinalIgnoreCase))
            return;

        var entry = Project.Handler.MapData.PrimaryBank.Maps
            .FirstOrDefault(e => e.Key.Filename.Equals(mapId, StringComparison.OrdinalIgnoreCase));

        if (entry.Value == null)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_Map_Not_Found", mapId));
            return;
        }

        UnloadMap();

        var generation = Generation;
        var wrapper = entry.Value;
        var existing = wrapper.MSB;
        var path = wrapper.Path;
        var fs = wrapper.TargetFS;
        var type = Project.Descriptor.ProjectType;

        LoadingMapId = mapId;

        Task.Run(() =>
        {
            IMsb? msb = existing;

            try
            {
                if (msb == null && fs.FileExists(path))
                    msb = MsbGameAdapter.For(type).Read(fs.ReadFile(path).Value);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("CUTSCENE_Map_Read_Failed", mapId), e);
                msb = null;
            }

            Pending = new PendingMap(generation, mapId, msb);
        });
    }

    /// <summary>Removes the map drawn, and stops any map being read from being drawn</summary>
    public void UnloadMap()
    {
        Generation++;

        Pending = null;
        LoadingMapId = null;

        foreach (var mesh in Meshes)
            mesh.Dispose();

        Meshes.Clear();
        PieceCount = 0;

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

    /// <summary>Builds the meshes of a map once it is read. Call on the main thread every frame.</summary>
    public void Update()
    {
        var pending = Pending;
        if (pending == null)
            return;

        Pending = null;

        if (pending.Generation != Generation)
            return;

        LoadingMapId = null;

        if (pending.Msb != null && GetScene() is { } scene)
            BuildMap(scene, pending.MapId, pending.Msb);
    }

    private void BuildMap(RenderScene scene, string mapId, IMsb msb)
    {
        var amapid = PathBuilder.GetAssetMapID(Project, mapId);
        var type = Project.Descriptor.ProjectType;

        MeshUid = $"CutsceneMap_{Generation}";
        LoadedMapId = mapId;

        var pieceAssets = new HashSet<ResourceDescriptor>();
        var assetAssets = new HashSet<ResourceDescriptor>();
        var textures = new HashSet<ResourceDescriptor>();

        var speedTrees = Project.Handler.MapData?.SpeedTreeList?.Entries
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in msb.Parts.GetEntries())
        {
            var isPiece = EntityHelper.IsPartMapPiece(part);
            var isAsset = !isPiece && CFG.Current.CutsceneEditor_Map_Load_Assets && EntityHelper.IsPartAsset(part);

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

                // SpeedTree vertices can only be read for these games
                if (isSpeedTree && type is not (ProjectType.ER or ProjectType.NR))
                    continue;

                resource = ModelLocator.GetObjModel(Project, modelName, modelName);
                filter = isSpeedTree ? RenderFilter.SpeedTree : RenderFilter.Object;

                foreach (var texture in MapResourceHandler.GetModelTextureAssets(Project, modelName))
                    textures.Add(texture);
            }

            if (resource == null || !resource.IsValid())
                continue;

            var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
                ResourceManager.DefaultPool.Find<FlverResource>(resource.Name), ModelMarkerType.None, MeshUid, null);

            mesh.World = GetWorldMatrix(part, part.Scale);
            mesh.DrawFilter = filter;
            ApplyShadowFlags(mesh, part);

            Meshes.Add(mesh);

            if (isPiece)
            {
                PieceCount++;
                pieceAssets.Add(resource);
            }
            else
            {
                assetAssets.Add(resource);
            }
        }

        foreach (var texture in TextureLocator.GetMapTextureVirtualPaths(Project, amapid))
        {
            if (texture.IsValid())
                textures.Add(texture);
        }

        if (type is ProjectType.ER or ProjectType.AC6 or ProjectType.NR or ProjectType.DB_NT or ProjectType.DB)
        {
            var aat = TextureLocator.GetCharacterCommonTextureVirtualPath(Project, "common_body");
            if (aat.IsValid())
                textures.Add(aat);
        }

        if (type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT or ProjectType.DS3 or ProjectType.BB
            or ProjectType.NR or ProjectType.DB_NT or ProjectType.DB)
        {
            var systex = TextureLocator.GetSystexTextureVirtualPath(Project, "systex");
            if (systex.IsValid())
                textures.Add(systex);
        }

        _ = LoadResources(mapId, pieceAssets, assetAssets, textures);
    }

    private async Task LoadResources(string mapId, HashSet<ResourceDescriptor> pieceAssets,
        HashSet<ResourceDescriptor> assetAssets, HashSet<ResourceDescriptor> textures)
    {
        var session = $"{mapId} (Cutscene)";

        var pieceJob = LOC.Get("MAP_ResourceManager_Map_Piece_Models");
        var assetJob = MapResourceHandler.GetObjectModelsName(Project);
        var textureJob = LOC.Get("MAP_ResourceManager_Map_Textures");
        var texturing = CFG.Current.Viewport_Enable_Texturing;

        ResourceLoadTracker.BeginSession(session);

        try
        {
            if (pieceAssets.Count > 0)
                ResourceLoadTracker.Reserve(pieceJob);
            if (assetAssets.Count > 0)
                ResourceLoadTracker.Reserve(assetJob);
            if (texturing && textures.Count > 0)
                ResourceLoadTracker.Reserve(textureJob);

            // Model archives can also contain texture packs, so those are loaded along with the models
            var modelTypes = ResourceType.Flver | ResourceType.Texture;

            await Task.WhenAll(
                LoadAssets(pieceAssets, pieceJob, modelTypes),
                LoadAssets(assetAssets, assetJob, modelTypes));

            if (texturing)
            {
                await LoadAssets(textures, textureJob, ResourceType.Texture);
                await ResourceManager.LoadMissingTexturesAsync();
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_Map_Load_Failed", mapId), e);
        }
        finally
        {
            ResourceLoadTracker.EndSession(session);
        }
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

    /// <summary>A part's placement, read as the Map Editor reads it: degrees unless marked radians, XZY if marked.</summary>
    private static Matrix4x4 GetWorldMatrix(object placed, Vector3 scale)
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
                rotation = new Vector3(float.DegreesToRadians(rotation.X), float.DegreesToRadians(rotation.Y), float.DegreesToRadians(rotation.Z));

            if (isXzy)
                transform.EulerRotationXZY = rotation;
            else
                transform.EulerRotation = rotation;
        }

        transform.Scale = scale;
        return transform.WorldMatrix;
    }

    /// <summary>Has the mesh cast and receive the sun's shadow as the part's MSB flags say, as the Map Editor does</summary>
    private static void ApplyShadowFlags(MeshRenderableProxy mesh, IMsbPart part)
    {
        if (!CFG.Current.Viewport_Shadows_Use_Msb_Flags)
        {
            mesh.CastsShadow = true;
            mesh.ReceivesShadow = true;
            return;
        }

        bool? ReadFlag(string name) => part.GetType().GetProperty(name)?.GetValue(part) switch
        {
            bool b => b,
            byte b => b != 0,
            sbyte b => b != 0,
            int i => i != 0,
            _ => null
        };

        // Elden Ring: an override byte, then the value it applies
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
