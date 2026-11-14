using Andre.Formats;
using DotNext.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StudioCore.Editors.Common;


/// <summary>
/// Entity used within MSB.
/// </summary>
public class MsbEntity : Entity
{

    protected IUniverse Owner;

    protected int CurrentNPCParamID = 0;
    protected int[] ModelMasks = null;

    public MsbEntityType Type { get; set; }

    /// <summary>
    /// The parts of the render mesh, if this is a character built from part models.
    /// </summary>
    [XmlIgnore]
    public CharacterAppearance CharacterAppearance { get; private set; }

    public MapContainer ContainingMap
    {
        get => (MapContainer)Container;
        set => Container = value;
    }
    public override string PrettyName
    {
        get
        {
            // MSLD areas have no name of their own, so they go by the collision they were built from
            if (WrappedObject is MSLD.Area area)
                return $"h{area.CollisionNumber:D6}";

            return $@"{Utils.ImGuiEscape(Name, null)}";
        }
    }

    // MSLD areas are only a group for their edges, which are already in the space of the map
    public override bool HasTransform =>
        Type != MsbEntityType.Event &&
        Type != MsbEntityType.Models &&
        Type != MsbEntityType.DS2GeneratorRegist &&
        Type != MsbEntityType.DS2Event &&
        Type != MsbEntityType.MapLimitArea;

    [XmlIgnore]
    public string MapID { get; set; }

    public MsbEntity(IUniverse owner, ObjectContainer map, object msbo, MsbEntityType type) : base(owner, map, msbo)
    {
        Owner = owner;
        Container = map;
        WrappedObject = msbo;
        Type = type;
        MapID = map.Name;
    }

    public void AssignDrawable()
    {
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            return;

        MeshProviderCache.InvalidateUidEntries(EntityCacheUID);

        // A part can go from a placeholder to a real model, which has no fill
        SelectionFillMesh = null;
        SelectionWireMesh = null;
        CollisionLayerMeshes = null;
        PrefabMemberMeshes = null;
        PrefabMemberCollisionMeshes = null;

        switch (Type)
        {
            case MsbEntityType.Part:
                AssignPartDrawable();
                AssignObjectCollisionDrawable();
                RefreshGrass();
                RefreshEdgePoints();
                RefreshWalls();
                break;

            case MsbEntityType.Region:
                AssignRegionDrawable();
                break;

            case MsbEntityType.Light:
                AssignLightDrawable();
                break;

            case MsbEntityType.DS2Generator:
                AssignGeneratorDrawable();
                break;

            case MsbEntityType.DS2EventLocation:
                AssignEventDrawable();
                break;

            case MsbEntityType.AutoInvadePoint:
                AssignInvasionPointDrawable();
                break;

            case MsbEntityType.MultiPlayerWall:
                AssignMultiPlayerWallDrawable();
                break;

            case MsbEntityType.MapLimitEdge:
                AssignMapLimitEdgeDrawable();
                break;

            case MsbEntityType.Navmesh:
                AssignNavmeshDrawable();
                break;

            case MsbEntityType.LightProbeVolume:
                //AssignLightProbeDrawable();
                break;

            case MsbEntityType.FogVolume:
                AssignFogVolumeDrawable();
                break;

            default: break;
        }

        // The mesh now exists, so UpdateRenderModel must not build it a second time
        SetupRenderMesh = true;

        RefreshShadowFlags();
    }

    /// <summary>
    /// Has the part's mesh cast and receive the sun's shadow as its MSB flags say: IsShadowSrc (ShadowSource in DS3
    /// and BB) or IsShadowOnly to cast, IsShadowDest (ShadowDest) to receive. In Elden Ring each flag only counts when
    /// its Override byte is set (IsDirLightShadowSrc for the sun); otherwise the part's draw param decides, taken here
    /// as its usual value. A format without the flags, and every part when Viewport_Shadows_Use_Msb_Flags is off, does both.
    /// </summary>
    private void RefreshShadowFlags()
    {
        if (RenderSceneMesh is not MeshRenderableProxy mesh)
            return;

        if (!CFG.Current.Viewport_Shadows_Use_Msb_Flags || Type != MsbEntityType.Part)
        {
            mesh.CastsShadow = true;
            mesh.ReceivesShadow = true;
            return;
        }

        // Elden Ring: an override byte, then the value it applies
        bool? Overridden(string over, string value, bool fallback)
            => ReadShadowFlag(over) is bool on ? (on ? ReadShadowFlag(value) ?? fallback : fallback) : null;

        var source = Overridden("OverrideDirLightShadowSrc", "IsDirLightShadowSrc", true)
            ?? ReadShadowFlag("IsShadowSrc") ?? ReadShadowFlag("ShadowSource") ?? true;
        var only = Overridden("OverrideShadowOnly", "IsShadowOnly", false) ?? ReadShadowFlag("IsShadowOnly") ?? false;
        var dest = Overridden("OverrideShadowDest", "IsShadowDest", true)
            ?? ReadShadowFlag("IsShadowDest") ?? ReadShadowFlag("ShadowDest") ?? true;

        mesh.CastsShadow = source || only;
        mesh.ReceivesShadow = dest;
    }

    /// <summary>A shadow flag of the part, which is a bool or a byte depending on the format, or null if it has none</summary>
    private bool? ReadShadowFlag(string name)
    {
        return GetPropertyValue(name) switch
        {
            bool b => b,
            byte b => b != 0,
            sbyte b => b != 0,
            int i => i != 0,
            _ => null
        };
    }

    private void SetRenderSceneMesh(RenderableProxy newMesh)
    {
        if (RenderSceneMesh != null && !ReferenceEquals(RenderSceneMesh, newMesh))
        {
            RenderSceneMesh.Dispose();
        }

        RenderSceneMesh = newMesh;
    }

    public void AssignPartDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        ResourceDescriptor asset;

        var modelProp = GetProperty("ModelName");

        if (modelProp == null)
            return;

        var modelName = (string)modelProp.GetValue(WrappedObject);
        var amapid = PathBuilder.GetAssetMapID(curProject, MapID);

        // A newly created part has no model yet, and map models are found from the name, so it gets a box
        // until one is set
        if (string.IsNullOrEmpty(modelName) && (EntityHelper.IsPartMapPiece(this)
            || EntityHelper.IsPartCollision(this) || EntityHelper.IsPartConnectCollision(this)
            || EntityHelper.IsPartNavmesh(this)))
        {
            SetRenderSceneMesh(CreateObjectProxyMesh());
            return;
        }

        // Map Piece
        if (EntityHelper.IsPartMapPiece(this))
        {
            var name = ModelLocator.MapModelNameToAssetName(curProject, amapid, modelName);
            asset = ModelLocator.GetMapModel(curProject, amapid, name, name);

            RenderSceneMesh = CreateMapPieceMesh(asset);
        }

        // Enemy
        if (EntityHelper.IsPartEnemy(this) || EntityHelper.IsPartDummyEnemy(this))
        {
            CharacterAppearance = null;

            if (!TryAssignCompositeCharacter(modelName))
            {
                asset = ModelLocator.GetChrModel(curProject, modelName, modelName);
                AssignCharacterOrPlaceholder(asset, modelName);
            }
        }

        // Player
        if (EntityHelper.IsPartPlayer(this))
        {
            SetRenderSceneMesh(CreatePlayerProxyMesh());
        }

        // Asset
        if (EntityHelper.IsPartAsset(this) || EntityHelper.IsPartDummyAsset(this))
        {
            asset = ModelLocator.GetObjModel(curProject, modelName, modelName);
            AssignAssetOrPlaceholder(asset, modelName);
        }

        // Prefab Asset
        if (EntityHelper.IsPartPrefabAsset(this))
        {
            AssignPrefabAssetDrawables(curProject, modelName);
        }

        // Collision
        if (EntityHelper.IsPartCollision(this))
        {
            var name = ModelLocator.MapModelNameToAssetName(curProject, amapid, modelName);

            AssignCollisionDrawables(curProject, amapid, name);
        }

        // Connect Collision
        if (EntityHelper.IsPartConnectCollision(this))
        {
            var name = ModelLocator.MapModelNameToAssetName(curProject, amapid, modelName);
            asset = ModelLocator.GetMapCollisionModel(curProject, amapid, name);

            RenderSceneMesh = CreateCollisionMesh(asset, true);
        }

        // Navmesh
        if (EntityHelper.IsPartNavmesh(this))
        {
            var name = ModelLocator.MapModelNameToAssetName(curProject, amapid, modelName);
            asset = ModelLocator.GetMapNVMModel(curProject, amapid, name);

            RenderSceneMesh = CreateNavmeshMesh(asset);
        }
    }

    /// <summary>
    /// Assigns a box at the origin of a prefab asset, with the models of the parts it places displayed alongside it.
    /// The parts all belong to this entity, so the prefab is selected and moved as one. They can't be edited
    /// here, as they are stored in the prefab's own MSB.
    /// </summary>
    private void AssignPrefabAssetDrawables(ProjectEntry curProject, string modelName)
    {
        var curUniverse = Owner as MapUniverse;
        var scene = curUniverse.GetCurrentScene();

        // The box is what the prefab is placed by, and is all that shows if its MSB can't be read
        var marker = RenderableHelper.GetDefaultBoxProxy(scene);
        marker.World = GetWorldMatrix();
        marker.SetSelectable(this);
        marker.DrawFilter = RenderFilter.Object;

        SetRenderSceneMesh(marker);

        var fill = RenderableHelper.GetDefaultBoxFillProxy(scene);
        fill.World = GetWorldMatrix();
        fill.SetSelectable(this);
        fill.DrawFilter = RenderFilter.Object;

        SelectionFillMesh = fill;

        List<PrefabMemberMesh> meshes = new();
        Dictionary<string, ResourceDescriptor> memberAssets = new(StringComparer.OrdinalIgnoreCase);

        foreach (var member in curProject.Handler.MapData.PrefabAssetBank.GetMembers(modelName))
        {
            if (!memberAssets.TryGetValue(member.ModelName, out var asset))
            {
                // SpeedTree vertices can't be read for this game, and members have no placeholder of their own
                asset = IsSpeedTreePlaceholder(member.ModelName)
                    ? null
                    : ModelLocator.GetObjModel(curProject, member.ModelName, member.ModelName);

                memberAssets[member.ModelName] = asset;

                if (asset != null)
                {
                    LoadAsset<FlverResource>(asset, MapResourceHandler.GetObjectModelsName(curProject), ResourceType.Flver | ResourceType.Texture);
                }
            }

            if (asset == null)
                continue;

            // Members share the provider of their model, as they have no model masks of their own
            var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
                ResourceManager.DefaultPool.Find<FlverResource>(asset.Name), ModelMarkerType.None, "", null);

            mesh.SetSelectable(this);
            mesh.DrawFilter = RenderFilter.Object;

            meshes.Add(new PrefabMemberMesh(mesh, member.LocalTransform));
        }

        PrefabMemberMeshes = meshes;
    }

    /// <summary>
    /// Assigns a mesh for each displayed collision type of a collision part. The first is the render scene mesh,
    /// the others are displayed alongside it, so the part is still selected and moved as one.
    /// </summary>
    private void AssignCollisionDrawables(ProjectEntry curProject, string amapid, string name)
    {
        var curUniverse = Owner as MapUniverse;

        List<ResourceDescriptor> assets = new();

        foreach (var type in curUniverse.View.HavokCollisionBank.GetVisibleCollisionTypes())
        {
            var asset = ModelLocator.GetMapCollisionModel(curProject, amapid, name, false, type);

            // Some games store a single collision for every type, or don't have the type at all
            if (!asset.IsValid() || assets.Any(e => e.Name == asset.Name))
                continue;

            assets.Add(asset);
        }

        if (assets.Count == 0)
        {
            assets.Add(ModelLocator.GetMapCollisionModel(curProject, amapid, name));
        }

        RenderSceneMesh = CreateCollisionMesh(assets[0], false);

        if (assets.Count > 1)
        {
            CollisionLayerMeshes = assets.Skip(1)
                .Select(e => (RenderableProxy)CreateCollisionMesh(e, false))
                .ToList();
        }
    }

    /// <summary>
    /// Assigns the collision mesh of an object or asset, which is displayed alongside its model.
    /// Only done while object collision is displayed, as it is rarely needed and has to be read per model.
    /// </summary>
    public void AssignObjectCollisionDrawable()
    {
        var isPrefab = EntityHelper.IsPartPrefabAsset(this);

        if (!isPrefab && !EntityHelper.IsPartAsset(this) && !EntityHelper.IsPartDummyAsset(this))
            return;

        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        CollisionSceneMesh = null;
        PrefabMemberCollisionMeshes = null;

        if (!CFG.Current.MapEditor_ModelLoad_Collisions ||
            !curUniverse.GetCurrentScene().DrawFilter.HasFlag(RenderFilter.ObjectCollision))
            return;

        var modelProp = GetProperty("ModelName");

        if (modelProp == null)
            return;

        var modelName = (string)modelProp.GetValue(WrappedObject);
        var collisionType = curUniverse.View.HavokCollisionBank.PrimaryCollisionType;

        if (isPrefab)
        {
            AssignPrefabCollisionDrawables(curProject, modelName, collisionType);
            return;
        }

        var asset = ModelLocator.GetMapObjCollision(curProject, modelName, collisionType);

        if (asset.GetContainerPath(curProject) == null)
            return;

        CollisionSceneMesh = CreateObjectCollisionMesh(asset);
    }

    /// <summary>
    /// Assigns the collision of each part placed by a prefab asset, placed as its member model is.
    /// </summary>
    private void AssignPrefabCollisionDrawables(ProjectEntry curProject, string modelName, HavokCollisionType collisionType)
    {
        List<PrefabMemberMesh> meshes = new();
        Dictionary<string, ResourceDescriptor> memberAssets = new(StringComparer.OrdinalIgnoreCase);

        foreach (var member in curProject.Handler.MapData.PrefabAssetBank.GetMembers(modelName))
        {
            if (!memberAssets.TryGetValue(member.ModelName, out var asset))
            {
                asset = ModelLocator.GetMapObjCollision(curProject, member.ModelName, collisionType);

                // Many members (e.g. decoration) have no collision of their own
                if (asset.GetContainerPath(curProject) == null)
                    asset = null;

                memberAssets[member.ModelName] = asset;
            }

            if (asset == null)
                continue;

            meshes.Add(new PrefabMemberMesh(CreateObjectCollisionMesh(asset), member.LocalTransform));
        }

        if (meshes.Count > 0)
        {
            PrefabMemberCollisionMeshes = meshes;
        }
    }

    /// <summary>
    /// What the displayed (or requested) grass was built from. Null if there is none.
    /// </summary>
    [XmlIgnore]
    public string GrassKey { get; set; }

    /// <summary>
    /// Requests the grass of this part. Only does anything while grass is displayed and when it has changed.
    /// </summary>
    public void RefreshGrass()
    {
        if (Type is not MsbEntityType.Part || Owner is not MapUniverse curUniverse)
            return;

        curUniverse.Project.Handler.MapData.GrassBank?.Request(this);
    }

    public void ClearGrass()
    {
        GrassKey = null;
        GrassSceneMeshes = null;
    }

    /// <summary>
    /// The GrassTypeParam rows of this part, one for each density in the GRASS file of its model.
    /// Null if this part doesn't have any.
    /// </summary>
    public int[] GetGrassParamIds()
    {
        var property = GetProperty("GrassConfigStruct") ?? GetProperty("GrassData");

        return property?.GetValue(WrappedObject) switch
        {
            MSBE.Part.GrassConfigStruct e => new[] { e.GrassParamId0, e.GrassParamId1, e.GrassParamId2, e.GrassParamId3, e.GrassParamId4, e.GrassParamId5 },
            MSBS.Part.GrassConfigStruct s => new[] { s.GrassTypeParamID0, s.GrassTypeParamID1, s.GrassTypeParamID2, s.GrassTypeParamID3, s.GrassTypeParamID4, s.GrassTypeParamID5 },
            MSB_AC6.Part.GrassConfigStruct ac6 => ac6.GrassTypeParamIds,
            MSB_NR.Part.GrassStruct nr => nr.GrassParamIds,
            MSB_DB_NT.Part.GrassStruct db => db.GrassParamIds,
            _ => null
        };
    }

    public string GetPartModelName()
    {
        return GetProperty("ModelName")?.GetValue(WrappedObject) as string;
    }

    /// <summary>
    /// The name of the model file of this part, without its extension.
    /// </summary>
    public string GetGrassModelFile()
    {
        var curProject = ((MapUniverse)Owner).Project;
        var modelName = GetPartModelName();

        if (!EntityHelper.IsPartMapPiece(this) || string.IsNullOrEmpty(modelName))
            return modelName;

        return ModelLocator.MapModelNameToAssetName(curProject, PathBuilder.GetAssetMapID(curProject, MapID), modelName);
    }

    /// <summary>
    /// Displays finished grass. The models are requested here, as most of them aren't placed in any map.
    /// </summary>
    public void ApplyGrass(IReadOnlyList<GrassLayer> layers, HashSet<string> loadedModels)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        List<RenderableProxy> meshes = new();

        foreach (GrassLayer layer in layers)
        {
            var asset = ModelLocator.GetObjModel(curProject, layer.ModelName, layer.ModelName);

            var handle = ResourceManager.DefaultPool.Find<FlverResource>(asset.Name);

            curUniverse.Project.Handler.MapData.GrassBank.HoldModel(handle);

            var mesh = MeshRenderableProxy.FromHandle(scene, handle);

            mesh.InstanceTransforms = layer.Transforms;
            mesh.World = GetWorldMatrix();
            mesh.SetSelectable(this);
            mesh.DrawFilter = RenderFilter.Grass;

            meshes.Add(mesh);

            if (loadedModels.Add(layer.ModelName))
            {
                LoadAsset<FlverResource>(asset, MapResourceHandler.GetObjectModelsName(curProject), ResourceType.Flver | ResourceType.Texture);
                LoadGrassTextures(layer.ModelName);
            }
        }

        GrassSceneMeshes = meshes;
    }

    // The EDGE markers reuse region meshes, which are a unit in size, so they are scaled by the
    // Viewport_Edge_Point_Size and Viewport_Edge_Segment_Radius settings

    // MPW panels have no thickness in the game, but are given some so they can be seen and clicked edge on
    private const float MultiPlayerWallDepth = 0.2f;

    // As are MSLD edges, which are far larger, so they are given more
    private const float MapLimitEdgeDepth = 1.0f;

    /// <summary>
    /// What the displayed (or requested) EDGE points were built from. Null if there are none.
    /// </summary>
    [XmlIgnore]
    public string EdgeKey { get; set; }

    /// <summary>
    /// Requests the EDGE points of this part. Only does anything while they are displayed and when they have changed.
    /// </summary>
    public void RefreshEdgePoints()
    {
        if (Type is not MsbEntityType.Part || Owner is not MapUniverse curUniverse)
            return;

        curUniverse.Project.Handler.MapData.EdgeBank?.Request(this);
    }

    public void ClearEdgePoints()
    {
        EdgeKey = null;
        EdgePointMeshes = null;
    }

    /// <summary>
    /// Displays finished EDGE points. These are purely visual, so they can't be selected.
    /// </summary>
    public void ApplyEdgePoints(EdgeMarkers markers)
    {
        var scene = (Owner as MapUniverse).GetCurrentScene();

        List<EdgePointMesh> meshes = new(markers.Markers.Count * 2);

        var pointSize = Math.Max(CFG.Current.Viewport_Edge_Point_Size, 0.01f);
        var radius = Math.Max(CFG.Current.Viewport_Edge_Segment_Radius, 0.005f);

        foreach (var marker in markers.Markers)
        {
            // The selected entry is drawn a little larger, in its own colour and over the geometry
            var grow = marker.Selected ? 1.4f : 1.0f;
            var color = marker.Selected ? CFG.Current.Viewport_Edge_Selected_Color : GetEdgeColor(marker);

            var (cylinder, local) = marker.Kind switch
            {
                EdgeMarkerKind.Point => (false, Matrix4x4.CreateScale(pointSize * grow) * Matrix4x4.CreateTranslation(marker.A)),
                EdgeMarkerKind.Pull => (false, Matrix4x4.CreateScale(pointSize * 0.6f * grow) * Matrix4x4.CreateTranslation(marker.A)),
                EdgeMarkerKind.Segment => (true, GetEdgeSegmentTransform(marker.A, marker.B, radius * grow)),
                _ => (true, GetEdgeSegmentTransform(marker.A, marker.B, radius * 0.4f * grow))
            };

            AddEdgePointMesh(meshes, RenderableHelper.GetEdgeMarkerWireProxy(scene, cylinder, color, marker.Selected), local);
            AddEdgePointMesh(meshes, RenderableHelper.GetEdgeMarkerFillProxy(scene, cylinder, color), local);
        }

        EdgePointMeshes = meshes;
    }

    public static Vector3 GetEdgeColor(EdgeMarker marker)
    {
        if (marker.Kind is EdgeMarkerKind.Pull or EdgeMarkerKind.PullLine)
            return CFG.Current.Viewport_Edge_Pull_Color;

        return marker.Type switch
        {
            EDGE.EdgeType.Grapple => CFG.Current.Viewport_Edge_Grapple_Color,
            EDGE.EdgeType.Hang => CFG.Current.Viewport_Edge_Hang_Color,
            _ => CFG.Current.Viewport_Edge_Hug_Color
        };
    }

    private static void AddEdgePointMesh(List<EdgePointMesh> meshes, DebugPrimitiveRenderableProxy mesh, Matrix4x4 local)
    {
        mesh.DrawFilter = RenderFilter.EdgePoint;
        meshes.Add(new EdgePointMesh(mesh, local));
    }

    /// <summary>
    /// The model name the displayed (or requested) walls were built from. Null if there are none.
    /// </summary>
    [XmlIgnore]
    public string WallKey { get; set; }

    /// <summary>
    /// Requests the walls of this collision part. Only does anything while they are displayed and when they have changed.
    /// </summary>
    public void RefreshWalls()
    {
        if (Type is not MsbEntityType.Part || Owner is not MapUniverse curUniverse)
            return;

        curUniverse.Project.Handler.MapData.WallBank?.Request(this);
    }

    public void ClearWalls()
    {
        WallKey = null;
        WallMeshes = null;
    }

    /// <summary>
    /// Displays the cells of a WALL file, in a batch of meshes for each color. They can't be selected,
    /// so clicks reach the collision behind them.
    /// </summary>
    public void ApplyWalls(WallCells cells)
    {
        var scene = (Owner as MapUniverse).GetCurrentScene();

        List<WallMesh> meshes = new();

        if (CFG.Current.MapEditor_Wall_Display_Cells)
        {
            AddWallMeshes(meshes, scene, cells.Walls, CFG.Current.Viewport_Wall_Fill_Color);
            AddWallMeshes(meshes, scene, cells.OtherCollisionWalls, CFG.Current.Viewport_Wall_Other_Collision_Color);
            AddWallMeshes(meshes, scene, cells.None, CFG.Current.Viewport_Wall_None_Color);
        }

        if (CFG.Current.MapEditor_Wall_Display_NoJump)
        {
            AddWallMeshes(meshes, scene, cells.NoJump, CFG.Current.Viewport_Wall_NoJump_Color);
        }

        WallMeshes = meshes;
    }

    private void AddWallMeshes(List<WallMesh> meshes, RenderScene scene,
        List<(Vector3 A, Vector3 B, Vector3 C)> triangles, Vector3 color)
    {
        for (var start = 0; start < triangles.Count; start += DbgPrimWallCells.MaxTriangles)
        {
            var count = Math.Min(DbgPrimWallCells.MaxTriangles, triangles.Count - start);
            var primitive = new DbgPrimWallCells(triangles.GetRange(start, count));

            meshes.Add(new WallMesh(CreateWallProxy(scene, primitive, color), primitive));
        }
    }

    /// <summary>
    /// A translucent, unselectable mesh for wall cells, which only shows while walls are displayed.
    /// </summary>
    public static DebugPrimitiveRenderableProxy CreateWallProxy(RenderScene scene, IDbgPrim primitive, Vector3 color)
    {
        var fill = ColorHelper.GetTransparencyColor(color / 255.0f, CFG.Current.Viewport_Wall_Alpha);

        return new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
        {
            Pickable = false,
            RenderTranslucent = true,
            BaseColor = fill,
            HighlightedColor = fill,
            DrawFilter = RenderFilter.Wall
        };
    }

    /// <summary>
    /// Fits the unit cylinder, which stands on the origin, between two points with its top at the start.
    /// </summary>
    public static Matrix4x4 GetEdgeSegmentTransform(Vector3 start, Vector3 end, float radius)
    {
        var axis = start - end;
        var length = axis.Length();
        var direction = axis / length;
        var dot = Vector3.Dot(Vector3.UnitY, direction);

        // The cross product used for the rotation is empty when pointing straight down
        var rotation = dot < -0.9999f
            ? Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI)
            : Quaternion.Normalize(new Quaternion(Vector3.Cross(Vector3.UnitY, direction), 1 + dot));

        return Matrix4x4.CreateScale(radius, length, radius)
            * Matrix4x4.CreateFromQuaternion(rotation)
            * Matrix4x4.CreateTranslation(end);
    }

    private void LoadGrassTextures(string modelName)
    {
        if (!CFG.Current.Viewport_Enable_Texturing || !CFG.Current.MapEditor_TextureLoad_Objects)
            return;

        var curUniverse = Owner as MapUniverse;

        ResourceManager.DefaultPool.LoadAssets(curUniverse.Project, MapResourceHandler.GetModelTextureAssets(curUniverse.Project, modelName), new LoadOptions
        {
            JobName = LOC.Get("REND_Job_Additional_Textures"),
            Types = ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly
        });

        curUniverse.ScheduleTextureRefresh();
    }

    public MeshRenderableProxy CreateMapPieceMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            ResourceManager.DefaultPool.Find<FlverResource>(asset.Name), ModelMarkerType.None, EntityCacheUID, GetModelMasks());

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.MapPiece;

        LoadAsset<FlverResource>(asset, LOC.Get("MAP_ResourceManager_Map_Piece_Models"), ResourceType.Flver | ResourceType.Texture);

        return mesh;
    }

    public MeshRenderableProxy CreateCharacterMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            ResourceManager.DefaultPool.Find<FlverResource>(asset.Name), ModelMarkerType.Enemy, EntityCacheUID, GetModelMasks());

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = GetPartDrawFilter(RenderFilter.Character);

        LoadAsset<FlverResource>(asset, LOC.Get("MAP_ResourceManager_Character_Models"), ResourceType.Flver | ResourceType.Texture);

        return mesh;
    }

    public MeshRenderableProxy CreateObjectMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            ResourceManager.DefaultPool.Find<FlverResource>(asset.Name), ModelMarkerType.Object, EntityCacheUID, GetModelMasks());

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = GetPartDrawFilter(RenderFilter.Object);

        LoadAsset<FlverResource>(asset, MapResourceHandler.GetObjectModelsName(curProject), ResourceType.Flver | ResourceType.Texture);

        return mesh;
    }

    public MeshRenderableProxy CreateCollisionMesh(ResourceDescriptor asset, bool isConnectCollision)
    {
        var curUniverse = Owner as MapUniverse;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(
                scene, ResourceManager.DefaultPool.Find<HavokCollisionResource>(asset.Name), ModelMarkerType.None);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Collision;

        if (isConnectCollision)
        {
            mesh.DrawFilter = RenderFilter.ConnectCollision;
        }

        var jobName = LOC.Get(isConnectCollision ? "MAP_ResourceManager_Connect_Collision_Models" : "MAP_ResourceManager_Collision_Models");

        LoadAsset<HavokCollisionResource>(asset, jobName, ResourceType.CollisionHKX);

        return mesh;
    }

    public MeshRenderableProxy CreateObjectCollisionMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(
                scene, ResourceManager.DefaultPool.Find<HavokCollisionResource>(asset.Name), ModelMarkerType.None);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.ObjectCollision;

        // Binders can hold other hkx files (e.g. animations and cloth), so only the collision itself is loaded
        LoadAsset<HavokCollisionResource>(asset, LOC.Get("MAP_ResourceManager_Collision_Models"), ResourceType.CollisionHKX, asset.Name);

        return mesh;
    }

    public MeshRenderableProxy CreateNavmeshMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            return null;

        var mesh = MeshRenderableProxy.MeshRenderableFromNVMResource(
                scene, ResourceManager.DefaultPool.Find<NVMNavmeshResource>(asset.Name), ModelMarkerType.None);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Navmesh;

        LoadAsset<NVMNavmeshResource>(asset, LOC.Get("MAP_ResourceManager_Navmesh_Models"), ResourceType.Navmesh);

        return mesh;
    }

    /// <summary>
    ///     Makes sure the container holding the asset is loaded. Does nothing if the asset is already loaded.
    /// </summary>
    public void LoadAsset<T>(ResourceDescriptor asset, string jobMessage, ResourceType types, string onlyName = null)
        where T : class, IResource, System.IDisposable, new()
    {
        var curUniverse = Owner as MapUniverse;

        ResourceManager.DefaultPool.LoadAsset<T>(curUniverse.Project, asset, new LoadOptions
        {
            JobName = jobMessage,
            Types = types,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            OnlyNames = onlyName == null ? null : new[] { onlyName }
        });
    }

    public DebugPrimitiveRenderableProxy CreateCharacterProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetEnemyBoxProxy(scene);
        var fill = RenderableHelper.GetEnemyBoxFillProxy(scene);

        return SetupPlaceholderMesh(mesh, fill, GetPartDrawFilter(RenderFilter.Character));
    }

    public DebugPrimitiveRenderableProxy CreateInteractableProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetInteractableSphereProxy(scene);
        var fill = RenderableHelper.GetInteractableSphereFillProxy(scene);

        return SetupPlaceholderMesh(mesh, fill, GetPartDrawFilter(RenderFilter.Character));
    }

    public DebugPrimitiveRenderableProxy CreatePlayerProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetPlayerBoxProxy(scene);
        var fill = RenderableHelper.GetPlayerBoxFillProxy(scene);

        return SetupPlaceholderMesh(mesh, fill, RenderFilter.Character);
    }

    public DebugPrimitiveRenderableProxy CreateObjectProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetDefaultBoxProxy(scene);
        var fill = RenderableHelper.GetDefaultBoxFillProxy(scene);

        return SetupPlaceholderMesh(mesh, fill, GetPartDrawFilter(RenderFilter.Object));
    }

    /// <summary>
    /// Placeholders are a wireframe with a fill that is always displayed, as they have no model to show.
    /// </summary>
    private DebugPrimitiveRenderableProxy SetupPlaceholderMesh(DebugPrimitiveRenderableProxy mesh, DebugPrimitiveRenderableProxy fill, RenderFilter filter)
    {
        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = filter;

        fill.World = GetWorldMatrix();
        fill.SetSelectable(this);
        fill.DrawFilter = filter;

        FillAlwaysVisible = true;
        SelectionFillMesh = fill;

        return mesh;
    }

    public DebugPrimitiveRenderableProxy CreateSpeedTreeProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetSpeedTreeProxy(scene);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = GetPartDrawFilter(RenderFilter.SpeedTree);

        return mesh;
    }

    /// <summary>
    /// Dummy parts have their own filters, whatever kind of mesh they display. Other parts use the passed filter.
    /// </summary>
    private RenderFilter GetPartDrawFilter(RenderFilter filter)
    {
        if (EntityHelper.IsPartDummyEnemy(this))
            return RenderFilter.DummyEnemy;

        if (EntityHelper.IsPartDummyAsset(this))
            return RenderFilter.DummyAsset;

        return filter;
    }

    public void AssignRegionDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        DebugPrimitiveRenderableProxy mesh = null;
        DebugPrimitiveRenderableProxy fill = null;

        if (WrappedObject is IMsbRegion r)
        {
            // BOX
            if (r.Shape is MSB.Shape.Box)
            {
                mesh = RenderableHelper.GetBoxRegionProxy(scene);
            }
            // SPHERE
            else if (r.Shape is MSB.Shape.Sphere)
            {
                mesh = RenderableHelper.GetSphereRegionProxy(scene);
            }
            // CYLINDER
            else if (r.Shape is MSB.Shape.Cylinder)
            {
                mesh = RenderableHelper.GetCylinderRegionProxy(scene);
            }
            // POINT
            else if (r.Shape is MSB.Shape.Point)
            {
                mesh = RenderableHelper.GetPointRegionProxy(scene);
            }
            // RECTANGLE
            else if (r.Shape is MSB.Shape.Rectangle)
            {
                mesh = RenderableHelper.GetBoxRegionProxy(scene);
            }
            // CIRCLE
            else if (r.Shape is MSB.Shape.Circle)
            {
                mesh = RenderableHelper.GetCylinderRegionProxy(scene);
            }
            // COMPOSITE
            else if (r.Shape is MSB.Shape.Composite)
            {
                mesh = RenderableHelper.GetPointRegionProxy(scene);
            }

            // A wireframe only has a volume to show once selected
            if (r.Shape is MSB.Shape.Box or MSB.Shape.Rectangle)
            {
                fill = RenderableHelper.GetBoxRegionFillProxy(scene);
            }
            else if (r.Shape is MSB.Shape.Sphere)
            {
                fill = RenderableHelper.GetSphereRegionFillProxy(scene);
            }
            else if (r.Shape is MSB.Shape.Cylinder or MSB.Shape.Circle)
            {
                fill = RenderableHelper.GetCylinderRegionFillProxy(scene);
            }
            else if (r.Shape is MSB.Shape.Point or MSB.Shape.Composite)
            {
                fill = RenderableHelper.GetPointRegionFillProxy(scene);
            }
        }

        if (mesh == null)
        {
            throw new NotSupportedException(
                LOC.Get("REND_No_Region_Model_Proxy_For_Type", WrappedObject.GetType()));
        }

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Region;

        RenderSceneMesh = mesh;

        if (fill != null)
        {
            fill.World = GetWorldMatrix();
            fill.DrawFilter = RenderFilter.Region;
        }

        SelectionFillMesh = fill;
    }

    public void AssignLightDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var light = (BTL.Light)WrappedObject;

        DebugPrimitiveRenderableProxy mesh = null;

        if (light.Type is BTL.LightType.Directional)
        {
            mesh = RenderableHelper.GetDirectionalLightProxy(scene);
        }

        if (light.Type is BTL.LightType.Point)
        {
            mesh = RenderableHelper.GetPointLightProxy(scene);
        }

        if (light.Type is BTL.LightType.Spot)
        {
            mesh = RenderableHelper.GetSpotLightProxy(scene);
        }

        if (mesh == null)
        {
            throw new Exception(
                LOC.Get("REND_Invalid_BTL_LightType", light.Type));
        }

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Light;

        RenderSceneMesh = mesh;

        var fill = RenderableHelper.GetLightFillProxy(scene);
        fill.World = GetWorldMatrix();
        fill.DrawFilter = RenderFilter.Light;

        SelectionFillMesh = fill;
    }

    // DS2
    public void AssignGeneratorDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        if (ModelName != "")
        {
            var asset = ModelLocator.GetChrModel(curProject, ModelName, ModelName);
            AssignCharacterOrPlaceholder(asset, ModelName);
        }
    }

    // DS2
    public void AssignEventDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        DebugPrimitiveRenderableProxy mesh = RenderableHelper.GetBoxRegionProxy(scene);

        mesh.World =GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Region;

        RenderSceneMesh = mesh;
    }

    public void AssignInvasionPointDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var aip = (AIP.AutoInvadePointInstance)WrappedObject;

        DebugPrimitiveRenderableProxy mesh = null;

        mesh = RenderableHelper.GetAutoInvadeSphereProxy(scene);

        if (mesh == null)
        {
            throw new Exception(
                LOC.Get("REND_Invalid_Mesh"));
        }

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.AutoInvade;

        RenderSceneMesh = mesh;
    }

    /// <summary>
    /// A wireframe of the panel with a fill that is always displayed, so the boundary shows at a glance.
    /// The panel is sized by its transform; see GetLocalTransform.
    /// </summary>
    public void AssignMultiPlayerWallDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetMultiPlayerWallProxy(scene);
        var fill = RenderableHelper.GetMultiPlayerWallFillProxy(scene);

        RenderSceneMesh = SetupPlaceholderMesh(mesh, fill, RenderFilter.MultiPlayerWall);
    }

    // The fog ID and textures the fog of a volume was last drawn with, so it is only updated when they change
    private (int FogID, string HeightMap, string ColorMap, int SettingsVersion)? _fogVolumeState;

    /// <summary>
    /// The fog of the volume, which is always displayed, with a wireframe and fill of its box displayed while it
    /// is selected. The box is sized by its transform, as the volume's scale is its size.
    /// </summary>
    public void AssignFogVolumeDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var scene = curUniverse.GetCurrentScene();

        var fog = RenderableHelper.GetFogVolumeProxy(scene);
        fog.World = GetWorldMatrix();
        fog.SetSelectable(this);
        fog.DrawFilter = RenderFilter.FogVolume;

        var wire = RenderableHelper.GetFogVolumeWireProxy(scene);
        wire.World = GetWorldMatrix();
        wire.SetSelectable(this);
        wire.DrawFilter = RenderFilter.FogVolume;

        var fill = RenderableHelper.GetFogVolumeFillProxy(scene);
        fill.World = GetWorldMatrix();
        fill.SetSelectable(this);
        fill.DrawFilter = RenderFilter.FogVolume;

        _fogVolumeState = null;

        RenderSceneMesh = fog;
        SelectionWireMesh = wire;
        SelectionFillMesh = fill;

        RefreshFogVolume();
    }

    /// <summary>
    /// Updates the fog of a volume after its fog ID or textures have been edited, or the GPARAM settings it is
    /// looked up with have changed (<see cref="FogVolumeBank.SettingsVersion" />)
    /// </summary>
    public void RefreshFogVolume()
    {
        if (Type != MsbEntityType.FogVolume || RenderSceneMesh is not FogVolumeRenderableProxy fog
            || WrappedObject is not FVB.Volume volume)
            return;

        var curUniverse = Owner as MapUniverse;
        var bank = curUniverse.View.FogVolumeBank;

        var state = (volume.FogID, volume.HeightMapName, volume.ColorMapName, bank.SettingsVersion);

        if (_fogVolumeState == state)
            return;

        _fogVolumeState = state;

        fog.Settings = bank.GetSettings(Container?.Name, volume.FogID);
        fog.SetTextures(volume.HeightMapName, volume.ColorMapName);
    }

    /// <summary>
    /// A wireframe of the wall with a fill that is always displayed, so the mission area shows at a glance.
    /// The wall is placed and sized by its transform; see GetMapLimitEdgeTransform.
    /// </summary>
    public void AssignMapLimitEdgeDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetMapLimitEdgeProxy(scene);
        var fill = RenderableHelper.GetMapLimitEdgeFillProxy(scene);

        RenderSceneMesh = SetupPlaceholderMesh(mesh, fill, RenderFilter.MapLimit);
    }

    /// <summary>
    /// The transform of the unit box that stands on the origin, so it rises from the middle of the edge's
    /// bottom, runs along it, and is as tall as the wall. Only the yaw of the edge is used, as the walls are
    /// always vertical and the ends of the vanilla edges are always level.
    /// </summary>
    private static Transform GetMapLimitEdgeTransform(MSLD.Edge edge)
    {
        var t = Transform.Default;
        var dir = edge.End - edge.Start;

        t.Position = (edge.Start + edge.End) / 2;
        t.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.Atan2(-dir.Z, dir.X));
        t.Scale = new Vector3(new Vector2(dir.X, dir.Z).Length(), edge.Height, MapLimitEdgeDepth);

        return t;
    }

    /// <summary>
    /// Moves the ends of an MSLD edge with its transform: they follow the position and the yaw, and the
    /// length and height follow the scale if it is included. Edges have no position or rotation of their own,
    /// so the base action, which sets those properties, would do nothing.
    /// </summary>
    public override ViewportAction GetUpdateTransformAction(Transform newt, bool includeScale = false)
    {
        if (Type != MsbEntityType.MapLimitEdge || WrappedObject is not MSLD.Edge edge)
            return base.GetUpdateTransformAction(newt, includeScale);

        var oldt = GetMapLimitEdgeTransform(edge);
        var mid = oldt.Position;

        // The yaw of the new transform, from where it turns the edge's direction
        var forward = Vector3.Transform(Vector3.UnitX, newt.Rotation);
        var newYaw = MathF.Atan2(-forward.Z, forward.X);
        var oldDir = edge.End - edge.Start;
        var oldYaw = MathF.Atan2(-oldDir.Z, oldDir.X);
        var turn = Quaternion.CreateFromAxisAngle(Vector3.UnitY, newYaw - oldYaw);

        // The ends keep their offsets from the middle, so ends that aren't level stay as they were
        var lengthScale = includeScale && oldt.Scale.X > 0 ? newt.Scale.X / oldt.Scale.X : 1.0f;
        var startOffset = Vector3.Transform(edge.Start - mid, turn) * new Vector3(lengthScale, 1, lengthScale);
        var endOffset = Vector3.Transform(edge.End - mid, turn) * new Vector3(lengthScale, 1, lengthScale);

        PropChangeAction act = new(this, WrappedObject);
        act.AddPropertyChange(GetProperty("Start"), newt.Position + startOffset);
        act.AddPropertyChange(GetProperty("End"), newt.Position + endOffset);

        if (includeScale)
            act.AddPropertyChange(GetProperty("Height"), newt.Scale.Y);

        act.SetPostExecutionAction(undo =>
        {
            UpdateRenderModel();
        });

        return act;
    }

    public void AssignNavmeshDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();
    }

    public void AssignLightProbeDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene(); 
        
        var lightProbe = (BTPB.Probe)WrappedObject;

        DebugPrimitiveRenderableProxy mesh = null;

        mesh = RenderableHelper.GetLightProbeSphereProxy(scene);

        if (mesh == null)
        {
            throw new Exception(
                LOC.Get("REND_Invalid_Mesh"));
        }

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.LightProbe;

        RenderSceneMesh = mesh;
    }

    public int[] GetModelMasks()
    {
        int[] callback(Param.Row row, int count = 32)
        {
            if (row == null) return null;
            int[] enabledMasks = new int[count];

            for (int i = 0; i < count; i++)
            {
                var fieldName = $"modelDispMask{i}";
                if (Convert.ToBoolean((byte)row[fieldName]!.Value.Value))
                    enabledMasks[i] = 1;
            }
            return enabledMasks;
        }

        ParamEditorScreen paramEditor = null;
        var curProjectType = ProjectType.Undefined;

        if (Owner is MapUniverse)
        {
            var curOwner = (MapUniverse)Owner;

            curProjectType = curOwner.Project.Descriptor.ProjectType;
            paramEditor = curOwner.Project.Handler.ParamEditor;
        }

        if (paramEditor == null)
            return null;

        if (paramEditor.Project.Handler.ParamData.PrimaryBank.Params.ContainsKey("NpcParam"))
        {
            var npcParam = paramEditor.Project.Handler.ParamData.PrimaryBank.Params?["NpcParam"];

            switch (curProjectType)
            {
                case ProjectType.DB_NT:
                case ProjectType.DB:
                    break;
                case ProjectType.DS3:
                    if (WrappedObject is MSB3.Part.EnemyBase ds3e)
                    {
                        var npcParamId = ds3e.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.BB:
                    if (WrappedObject is MSBB.Part.EnemyBase bbe)
                    {
                        var npcParamId = bbe.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.SDT:
                    if (WrappedObject is MSB3.Part.EnemyBase sdte)
                    {
                        var npcParamId = sdte.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.ER:
                    if (WrappedObject is MSBE.Part.EnemyBase ere)
                    {
                        var npcParamId = ere.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.NR:
                    if (WrappedObject is MSB_NR.Part.EnemyBase nre)
                    {
                        var npcParamId = nre.NpcParamId;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.DES:
                    if (WrappedObject is MSBD.Part.EnemyBase dese)
                    {
                        var npcParamId = dese.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId], 16);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    if (WrappedObject is MSB1.Part.EnemyBase ds1e)
                    {
                        var npcParamId = ds1e.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId], 16);
                        }
                        else
                        {
                            return null;
                        }
                    }

                    break;
                case ProjectType.DS2S:
                case ProjectType.AC6:
                case ProjectType.DS2:
                case ProjectType.Undefined:
                default:
                    return null;
            }
        }

        return null;
    }

    public override void BuildReferenceMap()
    {
        if (Owner is MapUniverse)
        {
            var universe = (MapUniverse)Owner;

            if (Type == MsbEntityType.MapRoot && universe != null)
            {
                // Special handling for map itself, as it references objects outside of the map.
                // This depends on Type, which is only defined in MapEntity.
                List<byte[]> connects = new();
                foreach (Entity child in Children)
                {
                    // This could use an annotation, but it would require both a custom type and field annotation.
                    if (child.WrappedObject?.GetType().Name != "ConnectCollision")
                    {
                        continue;
                    }

                    PropertyInfo mapProp = child.WrappedObject.GetType().GetProperty("MapID");
                    if (mapProp == null || mapProp.PropertyType != typeof(byte[]))
                    {
                        continue;
                    }

                    var mapId = (byte[])mapProp.GetValue(child.WrappedObject);
                    if (mapId != null)
                    {
                        connects.Add(mapId);
                    }
                }

                // For now, the map relationship type is not given here (dictionary values), just all related maps.
                foreach (var mapRef in MapConnections_ER.GetRelatedMaps(universe.View, Name))
                {
                    References[mapRef.Key] = new[] { new ObjectContainerReference(mapRef.Key) };
                }
            }
            else
            {
                base.BuildReferenceMap();
            }
        }
    }

    public override Transform GetLocalTransform()
    {
        // MSLD edges are placed by their ends rather than a position and rotation
        if (Type == MsbEntityType.MapLimitEdge && WrappedObject is MSLD.Edge edge)
        {
            Transform edgeTransform = GetMapLimitEdgeTransform(edge);

            // Prevent zero scale since it won't render
            if (edgeTransform.Scale.X == 0)
                edgeTransform.Scale.X = 0.1f;
            if (edgeTransform.Scale.Y == 0)
                edgeTransform.Scale.Y = 0.1f;

            return edgeTransform;
        }

        Transform t = base.GetLocalTransform();

        // If this is a region scale the region primitive by its respective parameters
        if (Type == MsbEntityType.Region)
        {
            var shape = GetPropertyValue("Shape");
            if (shape != null && shape is MSB.Shape.Box b2)
            {
                t.Scale = new Vector3(b2.Width, b2.Height, b2.Depth);
            }
            else if (shape != null && shape is MSB.Shape.Sphere s)
            {
                t.Scale = new Vector3(s.Radius);
            }
            else if (shape != null && shape is MSB.Shape.Cylinder c)
            {
                t.Scale = new Vector3(c.Radius, c.Height, c.Radius);
            }
            else if (shape != null && shape is MSB.Shape.Rectangle re)
            {
                t.Scale = new Vector3(re.Width, 0.0f, re.Depth);
            }
            else if (shape != null && shape is MSB.Shape.Circle ci)
            {
                t.Scale = new Vector3(ci.Radius, 0.0f, ci.Radius);
            }
        }
        // DS2 event regions
        else if (Type == MsbEntityType.DS2EventLocation)
        {
            var sx = GetPropertyValue("ScaleX");
            var sy = GetPropertyValue("ScaleY");
            var sz = GetPropertyValue("ScaleZ");
            if (sx != null)
            {
                t.Scale.X = (float)sx;
            }

            if (sy != null)
            {
                t.Scale.Y = (float)sy;
            }

            if (sz != null)
            {
                t.Scale.Z = (float)sz;
            }
        }
        // MPW panels have a fixed size, so the unit box is scaled to it
        else if (Type == MsbEntityType.MultiPlayerWall)
        {
            t.Scale = new Vector3(MPW.PanelWidth, MPW.PanelHeight, MultiPlayerWallDepth);
        }

        // Prevent zero scale since it won't render
        if (t.Scale == Vector3.Zero)
        {
            t.Scale = new Vector3(0.1f);
        }

        return t;
    }

    internal override Entity DuplicateEntity(object clone)
    {
        var curClone = clone;

        return new MsbEntity(Owner, Container, curClone, Type);
    }

    public override Entity Clone()
    {
        var c = (MsbEntity)base.Clone();
        c.Type = Type;
        c.ModelName = ModelName;
        return c;
    }

    public bool SetupRenderMesh = false;

    /// <summary>
    /// Re-syncs the cached model name from the WrappedObject and rebuilds the render mesh.
    /// Call this after the ModelName property on the WrappedObject has changed (including undo/redo).
    /// </summary>
    public void RefreshModel()
    {
        // Keep the cached field in sync with the WrappedObject (also covers undo)
        var modelProp = GetProperty("ModelName");
        if (modelProp != null && modelProp.GetValue(WrappedObject) is string modelName)
        {
            ModelName = modelName;
        }

        Name = null;
        CachedAliasName = null;

        AssignDrawable();
        UpdateRenderModel();
        LoadModelTextures();
    }

    /// <summary>
    /// Requests the textures of the current model. Models only load their own container, whereas the
    /// textures of a newly assigned model can be in a separate one that the map load didn't know about.
    /// </summary>
    private void LoadModelTextures()
    {
        if (!CFG.Current.Viewport_Enable_Texturing || string.IsNullOrEmpty(ModelName))
            return;

        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        var loadTextures = ModelName.StartsWith('c')
            ? CFG.Current.MapEditor_TextureLoad_Characters
            : CFG.Current.MapEditor_TextureLoad_Objects;

        if (loadTextures)
        {
            ResourceManager.DefaultPool.LoadAssets(curProject, MapResourceHandler.GetModelTextureAssets(curProject, ModelName), new LoadOptions
            {
                JobName = LOC.Get("REND_Job_Additional_Textures"),
                Types = ResourceType.Texture,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly
            });
        }

        // Runs once the jobs are done, for textures that aren't in the containers above
        curUniverse.ScheduleTextureRefresh();
    }

    public override void UpdateRenderModel()
    {
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            return;

        if (!SetupRenderMesh)
        {
            AssignDrawable();
            SetupRenderMesh = true;
        }

        // Editing the grass fields of a part only reaches here
        RefreshGrass();
        RefreshEdgePoints();
        RefreshFogVolume();
        RefreshShadowFlags();

        base.UpdateRenderModel();
    }

    public bool IsCharacterPlaceholder(string modelName, ProjectAliasType aliasType)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        curProject.Handler.ProjectData.Aliases.TryGetValue(aliasType, out var list);

        if(list != null)
        {
            foreach(var alias in list)
            {
                if (alias.ID == modelName)
                {
                    if (alias.Tags.Contains("character_placeholder"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool IsInteractablePlaceholder(string modelName, ProjectAliasType aliasType)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        curProject.Handler.ProjectData.Aliases.TryGetValue(aliasType, out var list);

        if(list != null)
        {
            foreach(var alias in list)
            {
                if (alias.ID == modelName)
                {
                    if (alias.Tags.Contains("interactable_placeholder"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool IsAssetPlaceholder(string modelName, ProjectAliasType aliasType)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        curProject.Handler.ProjectData.Aliases.TryGetValue(aliasType, out var list);

        if (list != null)
        {
            foreach (var alias in list)
            {
                if (alias.ID == modelName)
                {
                    if (alias.Tags.Contains("asset_placeholder"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool IsSpeedTree(string modelName)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        if(curProject.Handler.MapData.SpeedTreeList.Entries.Any(e => e.ToLower() == modelName.ToLower()))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// The vertices of SpeedTree models can only be read for some games, the others show a placeholder.
    /// See FlverResource.HasSpeedTreeVertices
    /// </summary>
    public bool IsSpeedTreePlaceholder(string modelName)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        return IsSpeedTree(modelName) && curProject.Descriptor.ProjectType is not (ProjectType.ER or ProjectType.NR);
    }

    /// <summary>
    /// Assigns a mesh built from the part models of a player-like character, dressed by its CharaInitParam row.
    /// Returns false if the model isn't one of these, or there are no parts to display for it.
    /// </summary>
    private bool TryAssignCompositeCharacter(string modelName)
    {
        if (!CFG.Current.MapEditor_Enable_Character_Composition || !CharacterComposition.IsComposite(modelName))
            return false;

        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        var charaInitProp = GetProperty("CharaInitID") ?? GetProperty("CharaInitParamId");
        var charaInitId = charaInitProp?.GetValue(WrappedObject) as int? ?? -1;

        var appearance = CharacterComposition.Resolve(curProject, charaInitId);

        var mesh = CharacterComposition.CreateMesh(curProject, curUniverse.GetCurrentScene(), appearance,
            EntityCacheUID, LOC.Get("MAP_ResourceManager_Character_Models"));

        if (mesh == null)
            return false;

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = GetPartDrawFilter(RenderFilter.Character);

        CharacterAppearance = appearance;
        SetRenderSceneMesh(mesh);

        return true;
    }

    private void AssignCharacterOrPlaceholder(ResourceDescriptor asset, string modelName)
    {
        if (IsCharacterPlaceholder(modelName, ProjectAliasType.Characters))
        {
            SetRenderSceneMesh(CreateCharacterProxyMesh());
            return;
        }

        if (IsInteractablePlaceholder(modelName, ProjectAliasType.Characters))
        {
            SetRenderSceneMesh(CreateInteractableProxyMesh());
            return;
        }

        var meshProxy = CreateCharacterMesh(asset);
        SetRenderSceneMesh(meshProxy);

        // Fall back to a generic marker if the model turns out to have no real geometry once loaded
        // (missing/corrupt FLVER), instead of silently rendering nothing.
        meshProxy.MeshAvailabilityChanged += proxy =>
        {
            if (ReferenceEquals(RenderSceneMesh, proxy) && !proxy.HasRenderableGeometry())
            {
                SetRenderSceneMesh(CreateCharacterProxyMesh());
            }
        };
    }

    private void AssignAssetOrPlaceholder(ResourceDescriptor asset, string modelName)
    {
        if (IsSpeedTreePlaceholder(modelName))
        {
            SetRenderSceneMesh(CreateSpeedTreeProxyMesh());
            return;
        }

        var isSpeedTree = IsSpeedTree(modelName);

        if (!isSpeedTree && IsAssetPlaceholder(modelName, ProjectAliasType.Assets))
        {
            SetRenderSceneMesh(CreateObjectProxyMesh());
            return;
        }

        var meshProxy = CreateObjectMesh(asset);

        if (isSpeedTree)
        {
            meshProxy.DrawFilter = GetPartDrawFilter(RenderFilter.SpeedTree);
        }

        SetRenderSceneMesh(meshProxy);

        meshProxy.MeshAvailabilityChanged += proxy =>
        {
            if (ReferenceEquals(RenderSceneMesh, proxy) && !proxy.HasRenderableGeometry())
            {
                SetRenderSceneMesh(isSpeedTree ? CreateSpeedTreeProxyMesh() : CreateObjectProxyMesh());
            }
        };
    }

    #region Dispose
    protected override void Dispose(bool disposing)
    {
        SetupRenderMesh = false;
        base.Dispose(disposing);
    }
    #endregion
}
