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
            return $@"{Utils.ImGuiEscape(Name, null)}";
        }
    }
    public override bool HasTransform => 
        Type != MsbEntityType.Event && 
        Type != MsbEntityType.Models &&
        Type != MsbEntityType.DS2GeneratorRegist &&
        Type != MsbEntityType.DS2Event;

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

        switch (Type)
        {
            case MsbEntityType.Part:
                AssignPartDrawable();
                AssignObjectCollisionDrawable();
                RefreshGrass();
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

            case MsbEntityType.Navmesh:
                AssignNavmeshDrawable();
                break;

            case MsbEntityType.LightProbeVolume:
                //AssignLightProbeDrawable();
                break;

            default: break;
        }

        // The mesh now exists, so UpdateRenderModel must not build it a second time
        SetupRenderMesh = true;
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

        // Collision
        if (EntityHelper.IsPartCollision(this))
        {
            var name = ModelLocator.MapModelNameToAssetName(curProject, amapid, modelName);
            asset = ModelLocator.GetMapCollisionModel(curProject, amapid, name);

            RenderSceneMesh = CreateCollisionMesh(asset, false);
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
    /// Assigns the collision mesh of an object or asset, which is displayed alongside its model.
    /// Only done while object collision is displayed, as it is rarely needed and has to be read per model.
    /// </summary>
    public void AssignObjectCollisionDrawable()
    {
        if (!EntityHelper.IsPartAsset(this) && !EntityHelper.IsPartDummyAsset(this))
            return;

        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;

        CollisionSceneMesh = null;

        if (!CFG.Current.MapEditor_ModelLoad_Collisions ||
            !curUniverse.GetCurrentScene().DrawFilter.HasFlag(RenderFilter.ObjectCollision))
            return;

        var modelProp = GetProperty("ModelName");

        if (modelProp == null)
            return;

        var modelName = (string)modelProp.GetValue(WrappedObject);
        var asset = ModelLocator.GetMapObjCollision(curProject, modelName,
            curUniverse.View.HavokCollisionBank.VisibleCollisionType);

        if (asset.GetContainerPath(curProject) == null)
            return;

        CollisionSceneMesh = CreateObjectCollisionMesh(asset);
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

        if (!EntityHelper.IsPartMapPiece(this))
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
        mesh.DrawFilter = RenderFilter.Character;

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
        mesh.DrawFilter = RenderFilter.Object;

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

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Character;

        return mesh;
    }

    public DebugPrimitiveRenderableProxy CreateInteractableProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetInteractableSphereProxy(scene);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Character;

        return mesh;
    }

    public DebugPrimitiveRenderableProxy CreatePlayerProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetPlayerBoxProxy(scene);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Character;

        return mesh;
    }

    public DebugPrimitiveRenderableProxy CreateObjectProxyMesh()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = RenderableHelper.GetDefaultBoxProxy(scene);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Object;

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
        mesh.DrawFilter = RenderFilter.SpeedTree;

        return mesh;
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

    public bool IsSpeedTreePlaceholder(string modelName)
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
        mesh.DrawFilter = RenderFilter.Character;

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

        if (IsAssetPlaceholder(modelName, ProjectAliasType.Assets))
        {
            SetRenderSceneMesh(CreateObjectProxyMesh());
            return;
        }

        var meshProxy = CreateObjectMesh(asset);
        SetRenderSceneMesh(meshProxy);

        meshProxy.MeshAvailabilityChanged += proxy =>
        {
            if (ReferenceEquals(RenderSceneMesh, proxy) && !proxy.HasRenderableGeometry())
            {
                SetRenderSceneMesh(CreateObjectProxyMesh());
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
