using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
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

    public bool IsSwitchingRenderType = false;
    private RenderModelType EntityRenderType = RenderModelType.Wireframe;

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
        switch(Type)
        {
            case MsbEntityType.Part:
                AssignPartDrawable();
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
            asset = ModelLocator.GetChrModel(curProject, modelName, modelName);

            RenderSceneMesh = CreateCharacterMesh(asset);

            if(RenderSceneMesh is MeshRenderableProxy meshProxy)
            {
                if(IsCharacterPlaceholder(meshProxy, modelName, ProjectAliasType.Characters))
                {
                    RenderSceneMesh = CreateCharacterProxyMesh();
                }
                if (IsInteractablePlaceholder(meshProxy, modelName, ProjectAliasType.Characters))
                {
                    RenderSceneMesh = CreateInteractableProxyMesh();
                }
            }
        }

        // Player
        if (EntityHelper.IsPartPlayer(this))
        {
            RenderSceneMesh = CreatePlayerProxyMesh();
        }

        // Asset
        if (EntityHelper.IsPartAsset(this) || EntityHelper.IsPartDummyAsset(this))
        {
            asset = ModelLocator.GetObjModel(curProject, modelName, modelName);

            RenderSceneMesh = CreateObjectMesh(asset);

            if (RenderSceneMesh is MeshRenderableProxy meshProxy)
            {
                if (IsAssetPlaceholder(meshProxy, modelName, ProjectAliasType.Assets))
                {
                    RenderSceneMesh = CreateObjectProxyMesh();
                }
            }
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

    public MeshRenderableProxy CreateMapPieceMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene, 
            asset.AssetVirtualPath, ModelMarkerType.None, EntityCacheUID, GetModelMasks());

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.MapPiece;

        LoadAsset(asset, "Loading map piece...");

        return mesh;
    }

    public MeshRenderableProxy CreateCharacterMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            asset.AssetVirtualPath, ModelMarkerType.Enemy, EntityCacheUID, GetModelMasks());

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Character;

        LoadAsset(asset, "Loading character...");

        return mesh;
    }

    public MeshRenderableProxy CreateObjectMesh(ResourceDescriptor asset)
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(scene,
            asset.AssetVirtualPath, ModelMarkerType.Object, EntityCacheUID, GetModelMasks());

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Object;

        LoadAsset(asset, "Loading object...");

        return mesh;
    }

    public MeshRenderableProxy CreateCollisionMesh(ResourceDescriptor asset, bool isConnectCollision)
    {
        var curUniverse = Owner as MapUniverse;
        var scene = curUniverse.GetCurrentScene();

        var mesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(
                scene, asset.AssetVirtualPath, ModelMarkerType.None);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Collision;

        if (isConnectCollision)
        {
            mesh.DrawFilter = RenderFilter.ConnectCollision;
        }

        LoadAsset(asset, "Loading collision...");

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
                scene, asset.AssetVirtualPath, ModelMarkerType.None);

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Navmesh;

        LoadAsset(asset, "Loading navmesh...");

        return mesh;
    }

    public void LoadAsset(ResourceDescriptor asset, string jobMessage)
    {
        ResourceJobBuilder job = ResourceManager.CreateNewJob(jobMessage);

        if (!ResourceManager.IsResourceLoaded(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
        {
            if (asset.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                    false);
            }
            else if (asset.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }

            Task task = job.Complete();
        }
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

    public void AssignRegionDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        DebugPrimitiveRenderableProxy mesh = null;

        if (WrappedObject is IMsbRegion r)
        {
            // SOLID
            if (EntityRenderType is RenderModelType.Solid)
            {
                // BOX
                if (r.Shape is MSB.Shape.Box)
                {
                    mesh = RenderableHelper.GetSolidBoxRegionProxy(scene);
                }
                // SPHERE
                else if (r.Shape is MSB.Shape.Sphere)
                {
                    mesh = RenderableHelper.GetSolidSphereRegionProxy(scene);
                }
                // CYLINDER
                else if (r.Shape is MSB.Shape.Cylinder)
                {
                    mesh = RenderableHelper.GetSolidCylinderRegionProxy(scene);
                }
                // POINT
                else if (r.Shape is MSB.Shape.Point)
                {
                    mesh = RenderableHelper.GetSolidPointRegionProxy(scene);
                }
                // RECTANGLE
                else if (r.Shape is MSB.Shape.Rectangle)
                {
                    mesh = RenderableHelper.GetSolidBoxRegionProxy(scene);
                }
                // CIRCLE
                else if (r.Shape is MSB.Shape.Circle)
                {
                    mesh = RenderableHelper.GetSolidCylinderRegionProxy(scene);
                }
                // COMPOSITE
                else if (r.Shape is MSB.Shape.Composite)
                {
                    mesh = RenderableHelper.GetSolidPointRegionProxy(scene);
                }
            }
            // WIREFRAME
            else if (EntityRenderType is RenderModelType.Wireframe)
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
            }
        }

        if (mesh == null)
            throw new NotSupportedException($"No region model proxy was specified for {WrappedObject.GetType()}");

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Region;

        RenderSceneMesh = mesh;
    }

    public void AssignLightDrawable()
    {
        var curUniverse = Owner as MapUniverse;
        var curProject = curUniverse.Project;
        var scene = curUniverse.GetCurrentScene();

        var light = (BTL.Light)WrappedObject;

        DebugPrimitiveRenderableProxy mesh = null;

        // SOLID
        if (EntityRenderType is RenderModelType.Solid)
        {
            if (light.Type is BTL.LightType.Directional)
            {
                mesh = RenderableHelper.GetSolidDirectionalLightProxy(this, scene);
            }

            if (light.Type is BTL.LightType.Point)
            {
                mesh = RenderableHelper.GetSolidPointLightProxy(this, scene);
            }

            if (light.Type is BTL.LightType.Spot)
            {
                mesh = RenderableHelper.GetSolidSpotLightProxy(this, scene);
            }
        }
        // WIREFRAME
        else if (EntityRenderType is RenderModelType.Wireframe)
        {
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
        }

        if (mesh == null)
            throw new Exception($"Unexpected BTL LightType: {light.Type}");

        mesh.World = GetWorldMatrix();
        mesh.SetSelectable(this);
        mesh.DrawFilter = RenderFilter.Light;

        RenderSceneMesh = mesh;
    }

    // DS2
    public void AssignGeneratorDrawable()
    {

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
            throw new Exception($"Unexpected AIP type");

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
            throw new Exception($"Unexpected BTPB type");

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

    public void SwitchRenderType()
    {
        IsSwitchingRenderType = true;

        switch (EntityRenderType)
        {
            case RenderModelType.Solid:
                EntityRenderType = RenderModelType.Wireframe;
                break;
            case RenderModelType.Wireframe:
                EntityRenderType = RenderModelType.Solid;
                break;
        }

        SetupRenderMesh = false;
        IsSwitchingRenderType = false;
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
        else if (Type == MsbEntityType.Light)
        {
            var lightType = GetPropertyValue("Type");
            if (lightType != null)
            {
                if (lightType is BTL.LightType.Directional)
                {
                }
                else if (lightType is BTL.LightType.Spot)
                {
                    var rad = (float)GetPropertyValue("Radius");
                    t.Scale = new Vector3(rad);

                    // TODO: Better transformation (or update primitive) using BTL ConeAngle
                    /*
                        var ang = (float)GetPropertyValue("ConeAngle");
                        t.Scale.X += ang;
                        t.Scale.Y += ang;
                    */
                }
                else if (lightType is BTL.LightType.Point)
                {
                    var rad = (float)GetPropertyValue("Radius");
                    t.Scale = new Vector3(rad);
                }
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
        return c;
    }

    public bool SetupRenderMesh = false;

    public override void UpdateRenderModel()
    {
        if(!SetupRenderMesh)
        {
            SetupRenderMesh = true;

            AssignDrawable();
        }

        base.UpdateRenderModel();
    }

    public bool IsCharacterPlaceholder(MeshRenderableProxy meshProxy, string modelName, ProjectAliasType aliasType)
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

    public bool IsInteractablePlaceholder(MeshRenderableProxy meshProxy, string modelName, ProjectAliasType aliasType)
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

    public bool IsAssetPlaceholder(MeshRenderableProxy meshProxy, string modelName, ProjectAliasType aliasType)
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
}
