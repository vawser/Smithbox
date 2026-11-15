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

    // ---- draw groups: what the player would see standing where the cut's camera starts

    /// <summary>A mesh of the map, the part it is, and the draw groups it is drawn in (null: always drawn)</summary>
    private sealed record BackdropMesh(MeshRenderableProxy Mesh, string PartName, bool IsCharacter, DrawGroup? Groups);

    private readonly List<BackdropMesh> Placed = new();

    // The player's collision (HitFilterID 8) of the map drawn, to stand on, with each collision's display groups
    private FirstPersonCollisionSet? Collision;
    private Dictionary<string, uint[]> CollisionDisplay = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Bumped when the map's collision is ready, or let go: the groups are worked out again</summary>
    public int CollisionVersion { get; private set; }
    public bool HasCollision => Collision is { Meshes.Count: > 0 };
    public int CollisionPartCount => Collision?.Meshes.Count ?? 0;

    /// <summary>The collision the groups were last taken from (chosen, or under the point), null if none</summary>
    public string? StandingOn { get; private set; }

    /// <summary>The collision under the point, whether or not another was chosen; null if none was hit</summary>
    public string? CollisionUnderPoint { get; private set; }

    /// <summary>Whether <see cref="StandingOn" /> was chosen by the user rather than found under the point</summary>
    public bool StandingOnChosen { get; private set; }

    /// <summary>The map's player collisions, by name, with how many display groups each shows: to choose from</summary>
    public IReadOnlyList<(string Name, int Groups)> Collisions { get; private set; } = Array.Empty<(string, int)>();

    /// <summary>The display groups in use (the collision's), null when everything is drawn</summary>
    public uint[]? ActiveDisplay { get; private set; }

    public int ShownCount { get; private set; }
    public int CharacterCount { get; private set; }

    // Only the latest map read is built: reading another one, or unloading, moves this on
    private int Generation;
    private PendingMap? Pending;
    private string? MeshUid;

    private sealed record PendingMap(int Generation, string MapId, IMsb? Msb, FirstPersonCollisionSet? Collision,
        Dictionary<string, uint[]> CollisionDisplay);

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
        var project = Project;

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

            // The collision to stand on, read here too: decoding it takes a while on a big map
            FirstPersonCollisionSet? collision = null;
            var display = new Dictionary<string, uint[]>(StringComparer.OrdinalIgnoreCase);
            if (msb != null)
            {
                try
                {
                    collision = ReadCollision(project, mapId, msb, display);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("CUTSCENE_Map_Collision_Failed", mapId), e);
                }
            }

            Pending = new PendingMap(generation, mapId, msb, collision, display);
        });
    }

    /// <summary>The loader state draw groups are resolved for: GameMan's starting state, alternate groups merged in (ER/NR)</summary>
    private const DisplayGroupLoaderMode LoaderMode = DisplayGroupLoaderMode.Merged;

    /// <summary>A part's draw and display groups, as the game loads them (alternate groups merged in where it has them)</summary>
    private static (uint[]? Draw, uint[]? Display) GroupsOf(object part)
    {
        var draw = DisplayGroupFields.GetArray(part, "DrawGroups");

        // DisplayGroups before DispGroups: in SDT/ER/NR "DispGroups" also names an unrelated nested struct's array
        var display = DisplayGroupFields.GetArray(part, "DisplayGroups") ?? DisplayGroupFields.GetArray(part, "DispGroups");

        var resolved = DisplayGroupFields.ApplyLoader(part, EntityHelper.IsPartCollision(part), draw, display, LoaderMode);
        return resolved is { } r ? (r.draw, r.display) : (draw, display);
    }

    /// <summary>
    /// The map's player collision (HitFilterID 8, as First Person mode reads it), placed in the map, and each
    /// collision's display groups: what is shown while the player stands on it. Off the main thread.
    /// </summary>
    private static FirstPersonCollisionSet ReadCollision(ProjectEntry project, string mapId, IMsb msb, Dictionary<string, uint[]> display)
    {
        var game = project.Descriptor.ProjectType.ToString();
        var vfs = project.VFS;
        byte[]? ReadFile(string path)
        {
            try
            {
                if (vfs.ProjectFS.FileExists(path))
                    return vfs.ProjectFS.ReadFile(path)?.ToArray();
                return vfs.FS.FileExists(path) ? vfs.FS.ReadFile(path)?.ToArray() : null;
            }
            catch
            {
                return null;
            }
        }

        // High collision first, as the player stands on it; low when a part has no high one
        var binders = new Dictionary<string, FirstPersonCollisionReader.MapBinder?>();
        FirstPersonCollisionReader.MapBinder? Binder(string kind)
        {
            if (!binders.TryGetValue(kind, out var b))
                binders[kind] = b = FirstPersonCollisionReader.MapBinder.Read(ReadFile!, game, mapId, kind);
            return b;
        }

        var set = new FirstPersonCollisionSet();

        foreach (var part in msb.Parts.GetEntries())
        {
            if (!EntityHelper.IsPartCollision(part) || string.IsNullOrEmpty(part.ModelName) || part.ModelName.Length < 2)
                continue;

            // Only the collision the player stands on: the others are for enemies, objects and cameras
            if (DisplayGroupFields.Get(part, "HitFilterID") is { } filter &&
                !FirstPersonCollisionReader.IsPlayerHitFilter(game, Convert.ToInt32(filter)))
                continue;

            List<Vector3>? local = null;
            foreach (var kind in new[] { "h", "l" })
            {
                var (_, entry) = FirstPersonCollisionReader.MapCollisionSource(game, mapId, part.ModelName, kind);
                local = entry != null ? Binder(kind)?.ReadTriangles(entry) : null;
                if (local is { Count: > 0 })
                    break;
            }

            if (local is not { Count: > 0 })
                continue;

            var world = GetWorldMatrix(part, part.Scale);
            set.Add(new FirstPersonMesh
            {
                Kind = FirstPersonMeshKind.Map,
                MapId = mapId,
                PartName = part.Name,
                Source = $"cutscene|{mapId}|{part.Name}",
                World = world,
                Tree = new FirstPersonTriangleTree(FirstPersonCollisionReader.Place(local, world))
            });

            if (GroupsOf(part).Display is { } groups)
                display[part.Name] = groups;
        }

        return set;
    }

    /// <summary>
    /// Takes the display groups from a collision, and draws only the map's parts in them; parts that cutscene actors
    /// stand for are hidden too. The collision is <paramref name="chosenCollision" /> when one of the map's is chosen,
    /// else the one under a point (the cut camera's start, cast straight down). With no collision, or with
    /// <paramref name="useGroups" /> off, every part is drawn.
    /// </summary>
    public void ApplyGroups(Vector3? from, bool useGroups, IReadOnlySet<string> excludedParts, string? chosenCollision = null)
    {
        StandingOn = null;
        StandingOnChosen = false;
        CollisionUnderPoint = null;
        ActiveDisplay = null;

        // Found even when one is chosen, so the list can say which the camera would give
        if (from is { } point && Collision != null &&
            Collision.RayCast(point + Vector3.UnitY * 0.5f, -Vector3.UnitY, 2000f, out var hit) && hit.Mesh != null)
            CollisionUnderPoint = hit.Mesh.PartName;

        if (useGroups)
        {
            if (chosenCollision != null && Collisions.Any(c => c.Name.Equals(chosenCollision, StringComparison.OrdinalIgnoreCase)))
            {
                StandingOn = chosenCollision;
                StandingOnChosen = true;
            }
            else
            {
                StandingOn = CollisionUnderPoint;
            }

            if (StandingOn != null)
                ActiveDisplay = CollisionDisplay.GetValueOrDefault(StandingOn);
        }

        var active = ActiveDisplay is { } d && d.Any(w => w != 0) ? new DrawGroup(d) : null;
        var shown = 0;

        foreach (var placed in Placed)
        {
            var visible = !excludedParts.Contains(placed.PartName) &&
                          (active == null || placed.Groups == null || placed.Groups.IsInDisplayGroup(active));
            if (placed.Mesh.Visible != visible)
                placed.Mesh.Visible = visible;
            if (visible)
                shown++;
        }

        ShownCount = shown;
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
        Placed.Clear();
        PieceCount = 0;
        CharacterCount = 0;
        ShownCount = 0;

        if (Collision != null)
        {
            Collision = null;
            CollisionDisplay = new(StringComparer.OrdinalIgnoreCase);
            Collisions = Array.Empty<(string, int)>();
            StandingOn = null;
            CollisionUnderPoint = null;
            StandingOnChosen = false;
            ActiveDisplay = null;
            CollisionVersion++;
        }

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
        {
            BuildMap(scene, pending.MapId, pending.Msb);

            Collision = pending.Collision;
            CollisionDisplay = pending.CollisionDisplay;
            Collisions = (Collision?.Meshes ?? new List<FirstPersonMesh>())
                .Select(m => m.PartName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .Select(n => (n, CollisionDisplay.TryGetValue(n, out var g) ? g.Sum(w => System.Numerics.BitOperations.PopCount(w)) : 0))
                .ToList();
            CollisionVersion++;
        }
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

        // Enemies take their groups from the collision they name (as the Map Editor does), so look those up by name
        var parts = msb.Parts.GetEntries().ToList();
        var byName = new Dictionary<string, IMsbPart>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in parts)
            byName.TryAdd(part.Name, part);

        foreach (var part in parts)
        {
            var isPiece = EntityHelper.IsPartMapPiece(part);
            var isAsset = !isPiece && CFG.Current.CutsceneEditor_Map_Load_Assets && EntityHelper.IsPartAsset(part);
            var isCharacter = !isPiece && !isAsset && CFG.Current.CutsceneEditor_Map_Load_Characters && EntityHelper.IsPartEnemy(part);

            if (isCharacter)
            {
                AddCharacter(scene, part, byName, textures);
                continue;
            }

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
            Placed.Add(new BackdropMesh(mesh, part.Name, false, DrawGroupOf(GroupsOf(part).Draw)));

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
        if (CharacterAssets.Count > 0)
        {
            _ = LoadAssets(new HashSet<ResourceDescriptor>(CharacterAssets), LOC.Get("MAP_ResourceManager_Character_Models"),
                ResourceType.Flver | ResourceType.Texture);
            CharacterAssets.Clear();
        }

        ShownCount = Placed.Count;
    }

    // A part's draw groups as a DrawGroup, null when it has none (drawn whatever is shown, as the Map Editor treats it)
    private static DrawGroup? DrawGroupOf(uint[]? draw) => draw is { } d && d.Any(w => w != 0) ? new DrawGroup(d) : null;

    private readonly List<ResourceDescriptor> CharacterAssets = new();

    /// <summary>
    /// One of the map's characters, standing as it is placed (not animated). Its NpcParam masks apply, a c0000 NPC wears
    /// its CharaInit equipment, and its groups are those of the collision it names when it has one.
    /// </summary>
    private void AddCharacter(RenderScene scene, IMsbPart part, Dictionary<string, IMsbPart> byName, HashSet<ResourceDescriptor> textures)
    {
        var model = part.ModelName;
        if (string.IsNullOrEmpty(model) || !model.StartsWith('c'))
            return;

        int Id(params string[] names)
        {
            foreach (var name in names)
            {
                if (part.GetType().GetProperty(name)?.GetValue(part) is { } v)
                    return Convert.ToInt32(v);
            }
            return -1;
        }

        MeshRenderableProxy? mesh;
        var uid = $"{MeshUid}_{part.Name}";

        if (CharacterComposition.IsComposite(model))
        {
            var appearance = CharacterComposition.Resolve(Project, Id("CharaInitID", "CharaInitParamId"));
            mesh = CharacterComposition.CreateMesh(Project, scene, appearance, uid, LOC.Get("MAP_ResourceManager_Character_Models"));
            if (CharacterComposition.GetCommonTexture(Project) is { } common)
                textures.Add(common);
        }
        else
        {
            var resource = ModelLocator.GetChrModel(Project, model, model);
            if (resource == null || !resource.IsValid())
                return;

            mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
                ResourceManager.DefaultPool.Find<FlverResource>(resource.Name), ModelMarkerType.None, uid,
                CutsceneCast.Masks(Project, Id("NPCParamID", "NpcParamId")));
            CharacterAssets.Add(resource);
        }

        if (mesh == null)
            return;

        foreach (var texture in new[] { TextureLocator.GetCharacterTextureVirtualPath(Project, model, false),
                     TextureLocator.GetCharacterTextureVirtualPath(Project, model, true) })
        {
            if (texture.IsValid())
                textures.Add(texture);
        }

        mesh.World = GetWorldMatrix(part, part.Scale);
        mesh.DrawFilter = RenderFilter.Character;
        Meshes.Add(mesh);
        CharacterCount++;

        // The collision it names gives its groups, unless the field is marked not to (as the Map Editor reads it)
        var groupsFrom = part;
        var collisionProp = part.GetType().GetProperty("CollisionName") ?? part.GetType().GetProperty("CollisionPartName");
        if (collisionProp != null && collisionProp.GetCustomAttribute<NoRenderGroupInheritence>() == null &&
            collisionProp.GetValue(part) is string collisionName && byName.TryGetValue(collisionName, out var collision))
            groupsFrom = collision;

        Placed.Add(new BackdropMesh(mesh, part.Name, true, DrawGroupOf(GroupsOf(groupsFrom).Draw)));
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
