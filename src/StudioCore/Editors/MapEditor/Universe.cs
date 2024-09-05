using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Numerics;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors;
using StudioCore.Editor;
using StudioCore.Locators;
using StudioCore.Core;
using HKLib.Serialization.hk2018.Binary;
using Google.Protobuf.Reflection;
using Silk.NET.SDL;
using Silk.NET.OpenGL;
using StudioCore.Havok;
using System.ComponentModel;

namespace StudioCore.MsbEditor;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(List<BTL.Light>))]
internal partial class BtlLightSerializerContext : JsonSerializerContext
{
}

/// <summary>
///     A universe is a collection of loaded maps with methods to load, serialize,
///     and unload individual maps.
/// </summary>
public class Universe
{
    public RenderScene _renderScene;
    public int _dispGroupCount = 8;

    public ExceptionDispatchInfo LoadMapExceptions = null;

    public bool postLoad;

    // This handles all rendering-specific stuff. Used to allow Map Editor and Model Editor usage without viewport if needed.
    public static bool IsRendering = true;

    public Universe(RenderScene scene, ViewportSelection sel)
    {
        _renderScene = scene;
        Selection = sel;

        if(_renderScene == null)
        {
            IsRendering = false;
        }
    }

    public Dictionary<string, ObjectContainer> LoadedObjectContainers { get; } = new();
    public Dictionary<string, ModelContainer> LoadedModelContainers { get; } = new();
    public ViewportSelection Selection { get; }

    public List<string> EnvMapTextures { get; private set; } = new();

    public ProjectType GameType => Smithbox.ProjectType;

    public List<MapContainer> GetLoadedMaps()
    {
        List<MapContainer> maps = new List<MapContainer>();

        foreach(var entry in LoadedObjectContainers)
        {
            if(entry.Value is MapContainer m)
            {
                maps.Add(m);
            }
        }

        return maps;
    }

    public MapContainer GetLoadedMap(string id)
    {
        if (id != null)
        {
            if (LoadedObjectContainers.ContainsKey(id) && LoadedObjectContainers[id] is MapContainer m)
            {
                return m;
            }
        }

        return null;
    }

    public int GetLoadedMapCount()
    {
        var i = 0;
        foreach (KeyValuePair<string, ObjectContainer> map in LoadedObjectContainers)
        {
            if (map.Value != null)
            {
                i++;
            }
        }

        return i;
    }

    private static RenderFilter GetRenderFilter(string type)
    {
        RenderFilter filter;

        switch (type)
        {
            case "Enemy":
            case "DummyEnemy":
                filter = RenderFilter.Character;
                break;
            case "Asset":
            case "Object":
            case "DummyObject":
                filter = RenderFilter.Object;
                break;
            case "Player":
                filter = RenderFilter.Region;
                break;
            case "MapPiece":
                filter = RenderFilter.MapPiece;
                break;
            case "Collision":
                filter = RenderFilter.Collision;
                break;
            case "Navmesh":
                filter = RenderFilter.Navmesh;
                break;
            case "Region":
                filter = RenderFilter.Region;
                break;
            case "Light":
                filter = RenderFilter.Light;
                break;
            default:
                filter = RenderFilter.All;
                break;
        }

        return filter;
    }

    private static ModelMarkerType GetModelMarkerType(string type)
    {
        ModelMarkerType modelMarker;

        switch (type)
        {
            case "Enemy":
            case "DummyEnemy":
                modelMarker = ModelMarkerType.Enemy;
                break;
            case "Asset":
            case "Object":
            case "DummyObject":
                modelMarker = ModelMarkerType.Object;
                break;
            case "Player":
                modelMarker = ModelMarkerType.Player;
                break;
            case "MapPiece":
            case "Collision":
            case "Navmesh":
            case "Region":
                modelMarker = ModelMarkerType.Other;
                break;
            default:
                modelMarker = ModelMarkerType.None;
                break;
        }

        return modelMarker;
    }

    public RenderableProxy GetRegionDrawable(MapContainer map, Entity obj)
    {
        if (obj.WrappedObject is IMsbRegion r && r.Shape is MSB.Shape.Box)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetBoxRegionProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Region;
            return mesh;
        }

        if (obj.WrappedObject is IMsbRegion r2 && r2.Shape is MSB.Shape.Sphere s)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetSphereRegionProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Region;
            return mesh;
        }

        if (obj.WrappedObject is IMsbRegion r3 && r3.Shape is MSB.Shape.Point p)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetPointRegionProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Region;
            return mesh;
        }

        if (obj.WrappedObject is IMsbRegion r4 && r4.Shape is MSB.Shape.Cylinder c)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetCylinderRegionProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Region;
            return mesh;
        }

        if (obj.WrappedObject is IMsbRegion r5 && r5.Shape is MSB.Shape.Composite co)
        {
            // Not fully implemented. Temporarily uses point region marker.
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetPointRegionProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Region;
            return mesh;
        }

        if (obj.WrappedObject is IMsbRegion r6 && r6.Shape is MSB.Shape.Rectangle re)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetBoxRegionProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Region;
            return mesh;
        }

        if (obj.WrappedObject is IMsbRegion r7 && r7.Shape is MSB.Shape.Circle ci)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetCylinderRegionProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Region;
            return mesh;
        }

        throw new NotSupportedException($"No region model proxy was specified for {obj.WrappedObject.GetType()}");
    }

    public RenderableProxy GetLightDrawable(MapContainer map, Entity obj)
    {
        var light = (BTL.Light)obj.WrappedObject;
        if (light.Type is BTL.LightType.Directional)
        {
            DebugPrimitiveRenderableProxy mesh =
                DebugPrimitiveRenderableProxy.GetDirectionalLightProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Light;
            return mesh;
        }

        if (light.Type is BTL.LightType.Point)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetPointLightProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Light;
            return mesh;
        }

        if (light.Type is BTL.LightType.Spot)
        {
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetSpotLightProxy(_renderScene);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Light;
            return mesh;
        }

        throw new Exception($"Unexpected BTL LightType: {light.Type}");
    }

    public RenderableProxy GetDS2EventLocationDrawable(MapContainer map, Entity obj)
    {
        DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetBoxRegionProxy(_renderScene);
        mesh.World = obj.GetWorldMatrix();
        obj.RenderSceneMesh = mesh;
        mesh.DrawFilter = RenderFilter.Region;
        mesh.SetSelectable(obj);
        return mesh;
    }

    public RenderableProxy GetDummyPolyDrawable(ObjectContainer map, Entity obj)
    {
        DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetDummyPolyForwardUpProxy(_renderScene);
        mesh.World = obj.GetWorldMatrix();
        obj.RenderSceneMesh = mesh;
        mesh.SetSelectable(obj);
        return mesh;
    }

    public RenderableProxy GetBoneDrawable(ObjectContainer map, Entity obj)
    {
        SkeletonBoneRenderableProxy mesh = new(_renderScene);
        mesh.World = obj.GetWorldMatrix();
        obj.RenderSceneMesh = mesh;
        mesh.SetSelectable(obj);
        return mesh;
    }

    public RenderableProxy GetPatrolLineDrawable(Entity selectable, Entity obj, List<Vector3> points, List<Vector3> looseStartPoints, bool endAtStart, bool random)
    {
        if (points.Count + looseStartPoints.Count < 2)
        {
            return null;
        }

        DebugPrimitives.DbgPrimWireChain line = new(points, looseStartPoints, System.Drawing.Color.Red, endAtStart, random);
        DebugPrimitiveRenderableProxy mesh = new(_renderScene.OpaqueRenderables, line)
        {
            BaseColor = System.Drawing.Color.Red,
            HighlightedColor = System.Drawing.Color.Red,
            World = obj.GetWorldMatrix(),
            DrawFilter = RenderFilter.Region,
        };
        mesh.SetSelectable(selectable);
        return mesh;
    }

    /// <summary>
    ///     Creates a drawable for a model and registers it with the scene. Will load
    ///     the required assets in the background if they aren't already loaded.
    /// </summary>
    /// <param name="modelname"></param>
    /// <returns></returns>
    public RenderableProxy GetModelDrawable(MapContainer map, Entity obj, string modelname, bool load, IEnumerable<int>? masks)
    {
        ResourceDescriptor asset;
        var loadcol = false;
        var loadnav = false;
        var loadflver = false;
        var filt = RenderFilter.All;

        var amapid = MapLocator.GetAssetMapID(map.Name);

        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");
        if (modelname.ToLower().StartsWith("m"))
        {
            loadflver = true;
            asset = ModelLocator.GetMapModel(amapid, ModelLocator.MapModelNameToAssetName(amapid, modelname));
            filt = RenderFilter.MapPiece;
        }
        else if (modelname.ToLower().StartsWith("c"))
        {
            loadflver = true;
            asset = ModelLocator.GetChrModel(modelname);
            filt = RenderFilter.Character;
        }
        else if (modelname.ToLower().StartsWith("o") || modelname.StartsWith("AEG"))
        {
            loadflver = true;
            asset = ModelLocator.GetObjModel(modelname);
            filt = RenderFilter.Object;
        }
        else if (modelname.ToLower().StartsWith("h"))
        {
            loadcol = true;
            asset = ModelLocator.GetMapCollisionModel(amapid,
                ModelLocator.MapModelNameToAssetName(amapid, modelname), false);

            if (asset == null || asset.AssetPath == null) loadcol = false;

            filt = RenderFilter.Collision;
        }
        else if (modelname.ToLower().StartsWith("n"))
        {
            loadnav = true;
            asset = ModelLocator.GetMapNVMModel(amapid, ModelLocator.MapModelNameToAssetName(amapid, modelname));
            filt = RenderFilter.Navmesh;
        }
        else
        {
            asset = ModelLocator.GetNullAsset();
        }

        ModelMarkerType modelMarkerType =
            GetModelMarkerType(obj.WrappedObject.GetType().ToString().Split("+").Last());

        if (loadcol)
        {
            MeshRenderableProxy mesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(
                _renderScene, asset.AssetVirtualPath, modelMarkerType);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Collision;
            obj.RenderSceneMesh = mesh;

            if (load && !ResourceManager.IsResourceLoadedOrInFlight(asset.AssetVirtualPath,
                    AccessLevel.AccessGPUOptimizedOnly))
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

                ResourceManager.MarkResourceInFlight(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                Task task = job.Complete();
                if (obj.Universe.postLoad)
                {
                    task.Wait();
                }
            }

            return mesh;
        }

        if (loadnav && Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
        {
            var mesh = MeshRenderableProxy.MeshRenderableFromNVMResource(
                _renderScene, asset.AssetVirtualPath, modelMarkerType);
            mesh.World = obj.GetWorldMatrix();
            obj.RenderSceneMesh = mesh;
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Navmesh;
            if (load && !ResourceManager.IsResourceLoadedOrInFlight(asset.AssetVirtualPath,
                    AccessLevel.AccessGPUOptimizedOnly))
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

                ResourceManager.MarkResourceInFlight(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                Task task = job.Complete();
                if (obj.Universe.postLoad)
                {
                    task.Wait();
                }
            }

            return mesh;
        }

        if (!loadflver)
        {
            return null;
        }

        if (CFG.Current.MapEditor_Substitute_PseudoPlayer_Model)
        {
            if (asset.AssetName == "c0000")
            {
                asset = ModelLocator.GetChrModel(CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID);
            }
        }

        var model = MeshRenderableProxy.MeshRenderableFromFlverResource(_renderScene, asset.AssetVirtualPath, modelMarkerType, masks);
        model.DrawFilter = filt;
        model.World = obj.GetWorldMatrix();
        obj.RenderSceneMesh = model;
        model.SetSelectable(obj);

        if (load && !ResourceManager.IsResourceLoadedOrInFlight(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
        {
            if (asset.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                    ResourceManager.ResourceType.Flver);
            }
            else if (asset.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }

            ResourceManager.MarkResourceInFlight(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            Task task = job.Complete();
            if (obj.Universe.postLoad)
            {
                task.Wait();
            }
        }

        return model;
    }

    public void LoadDS2Generators(string mapid, MapContainer map)
    {
        Dictionary<long, Param.Row> registParams = new();
        Dictionary<long, MergedParamRow> generatorParams = new();
        Dictionary<long, Entity> generatorObjs = new();
        Dictionary<long, Param.Row> eventParams = new();
        Dictionary<long, Param.Row> eventLocationParams = new();
        Dictionary<long, Param.Row> objectInstanceParams = new();

        Param regparam = ParamBank.PrimaryBank.Params[$"generatorregistparam_{mapid}"];
        foreach (Param.Row row in regparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "regist_" + row.ID;
            }

            registParams.Add(row.ID, row);

            MsbEntity obj = new(map, row, MsbEntity.MsbEntityType.DS2GeneratorRegist);
            map.AddObject(obj);
        }

        Param locparam = ParamBank.PrimaryBank.Params[$"generatorlocation_{mapid}"];
        foreach (Param.Row row in locparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "generator_" + row.ID;
            }


            MergedParamRow mergedRow = new();
            mergedRow.AddRow("generator-loc", row);
            generatorParams.Add(row.ID, mergedRow);

            MsbEntity obj = new(map, mergedRow, MsbEntity.MsbEntityType.DS2Generator);
            generatorObjs.Add(row.ID, obj);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);
        }

        HashSet<ResourceDescriptor> chrsToLoad = new();
        Param genparam = ParamBank.PrimaryBank.Params[$"generatorparam_{mapid}"];
        foreach (Param.Row row in genparam.Rows)
        {
            if (row.Name == null || row.Name == "")
            {
                row.Name = "generator_" + row.ID;
            }

            if (generatorParams.ContainsKey(row.ID))
            {
                generatorParams[row.ID].AddRow("generator", row);
            }
            else
            {
                MergedParamRow mergedRow = new();
                mergedRow.AddRow("generator", row);
                generatorParams.Add(row.ID, mergedRow);
                MsbEntity obj = new(map, mergedRow, MsbEntity.MsbEntityType.DS2Generator);
                generatorObjs.Add(row.ID, obj);
                map.AddObject(obj);
            }

            var registid = (uint)row.GetCellHandleOrThrow("GeneratorRegistParam").Value;
            if (registParams.ContainsKey(registid))
            {
                Param.Row regist = registParams[registid];
                var chrid = ParamBank.PrimaryBank.GetChrIDForEnemy(
                    (int)regist.GetCellHandleOrThrow("EnemyParamID").Value);
                if (chrid != null)
                {
                    ResourceDescriptor asset = ModelLocator.GetChrModel($@"c{chrid}");
                    MeshRenderableProxy model = MeshRenderableProxy.MeshRenderableFromFlverResource(
                        _renderScene, asset.AssetVirtualPath, ModelMarkerType.Enemy, null);
                    model.DrawFilter = RenderFilter.Character;
                    generatorObjs[row.ID].RenderSceneMesh = model;
                    model.SetSelectable(generatorObjs[row.ID]);
                    chrsToLoad.Add(asset);

                    if (CFG.Current.Viewport_Enable_Texturing)
                    {
                        ResourceDescriptor tasset = TextureLocator.GetChrTextures($@"c{chrid}");
                        if (tasset.AssetVirtualPath != null || tasset.AssetArchiveVirtualPath != null)
                        {
                            chrsToLoad.Add(tasset);
                        }
                    }
                }
            }
        }

        Param evtparam = ParamBank.PrimaryBank.Params[$"eventparam_{mapid}"];
        foreach (Param.Row row in evtparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "event_" + row.ID;
            }

            eventParams.Add(row.ID, row);

            MsbEntity obj = new(map, row, MsbEntity.MsbEntityType.DS2Event);
            map.AddObject(obj);
        }

        Param evtlparam = ParamBank.PrimaryBank.Params[$"eventlocation_{mapid}"];
        foreach (Param.Row row in evtlparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "eventloc_" + row.ID;
            }

            eventLocationParams.Add(row.ID, row);

            MsbEntity obj = new(map, row, MsbEntity.MsbEntityType.DS2EventLocation);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);

            // Try rendering as a box for now
            DebugPrimitiveRenderableProxy mesh = DebugPrimitiveRenderableProxy.GetBoxRegionProxy(_renderScene);
            mesh.World = obj.GetLocalTransform().WorldMatrix;
            obj.RenderSceneMesh = mesh;
            mesh.DrawFilter = RenderFilter.Region;
            mesh.SetSelectable(obj);
        }

        Param objparam = ParamBank.PrimaryBank.Params[$"mapobjectinstanceparam_{mapid}"];
        foreach (Param.Row row in objparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "objinstance_" + row.ID;
            }

            objectInstanceParams.Add(row.ID, row);

            MsbEntity obj = new(map, row, MsbEntity.MsbEntityType.DS2ObjectInstance);
            map.AddObject(obj);
        }

        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading chrs");
        foreach (ResourceDescriptor chr in chrsToLoad)
        {
            if (chr.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(chr.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                    ResourceManager.ResourceType.Flver);
            }
            else if (chr.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(chr.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }
        }

        job.Complete();
    }

    public void PopulateMapList()
    {
        LoadedObjectContainers.Clear();
        foreach (var m in MapLocator.GetFullMapList())
        {
            LoadedObjectContainers.Add(m, null);
        }
    }

    public void LoadRelatedMapsER(string mapid, Dictionary<string, ObjectContainer> maps)
    {
        IReadOnlyDictionary<string, SpecialMapConnections.RelationType> relatedMaps =
            SpecialMapConnections.GetRelatedMaps(mapid, maps.Keys);
        foreach (KeyValuePair<string, SpecialMapConnections.RelationType> map in relatedMaps)
        {
            LoadMap(map.Key);
        }
    }

    public bool LoadMap(string mapid, bool selectOnLoad = false)
    {
        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2S)
        {
            if (ParamBank.PrimaryBank.Params == null)
            {
                // ParamBank must be loaded for DS2 maps
                TaskLogs.AddLog("Cannot load DS2 maps when params are not loaded.",
                    LogLevel.Warning, TaskLogs.LogPriority.High);
                return false;
            }
        }

        ResourceDescriptor ad = MapLocator.GetMapMSB(mapid);
        if (ad.AssetPath == null)
        {
            return false;
        }

        LoadMapAsync(mapid, selectOnLoad);


        return true;
    }

    public BTL ReturnBTL(ResourceDescriptor ad)
    {
        try
        {
            BTL btl;

            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                using var bdt = BXF4.Read(ad.AssetPath, ad.AssetPath[..^3] + "bdt");
                BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));
                if (file == null)
                {
                    return null;
                }

                btl = BTL.Read(file.Bytes);
            }
            else
            {
                btl = BTL.Read(ad.AssetPath);
            }

            return btl;
        }
        catch (InvalidDataException e)
        {
            TaskLogs.AddLog($"Failed to load {ad.AssetName}",
                LogLevel.Error, TaskLogs.LogPriority.Normal, e);
            return null;
        }
    }

    private void SetupDrawgroupCount()
    {
        switch (Smithbox.ProjectType)
        {
            // imgui checkbox click seems to break at some point after 8 (8*32) checkboxes, so let's just hope that never happens, yeah?
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
                _dispGroupCount = 4;
                break;
            case ProjectType.BB:
            case ProjectType.DS3:
                _dispGroupCount = 8;
                break;
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.AC6:
                _dispGroupCount = 8; //?
                break;
            default:
                throw new Exception($"Error: Did not expect Gametype {Smithbox.ProjectType}");
                //break;
        }
    }

    private List<Task> tasks = new();

    public async void LoadMapAsync(string mapid, bool selectOnLoad = false)
    {
        if (LoadedObjectContainers.TryGetValue(mapid, out var m) && m != null)
        {
            TaskLogs.AddLog($"Map \"{mapid}\" is already loaded",
                LogLevel.Information, TaskLogs.LogPriority.Normal);
            return;
        }

        HavokUtils.OnLoadMap(mapid);

        try
        {
            postLoad = false;

            SetupDrawgroupCount();

            MapContainer map = new(this, mapid);

            MapResourceHandler resourceHandler = new MapResourceHandler(mapid);

            // Get the Map MSB resource
            bool exists = resourceHandler.GetMapMSB();

            // If MSB resource doesn't exist, quit
            if (!exists)
                return;

            // Read the MSB
            resourceHandler.ReadMap();

            // Load the map into the MapContainer
            map.LoadMSB(resourceHandler.Msb);

            if (IsRendering)
            {
                resourceHandler.SetupHumanEnemySubstitute();
                resourceHandler.SetupModelLoadLists();
                resourceHandler.SetupTexturelLoadLists();
                resourceHandler.SetupModelMasks(map);
            }

            resourceHandler.LoadLights(map);

            if (IsRendering)
            {
                if (Smithbox.ProjectType == ProjectType.ER && CFG.Current.Viewport_Enable_ER_Auto_Map_Offset)
                {
                    if (SpecialMapConnections.GetEldenMapTransform(mapid, LoadedObjectContainers) is Transform
                        loadTransform)
                    {
                        map.RootObject.GetUpdateTransformAction(loadTransform).Execute();
                    }
                }
            }

            if (!LoadedObjectContainers.ContainsKey(mapid))
            {
                LoadedObjectContainers.Add(mapid, map);
            }
            else
            {
                LoadedObjectContainers[mapid] = map;
            }

            if (IsRendering)
            {
                // Intervene in the UI to change selection if requested.
                // We want to do this as soon as the RootObject is available, rather than at the end of all jobs.
                if (selectOnLoad)
                {
                    Selection.ClearSelection();
                    Selection.AddSelection(map.RootObject);
                }
            }

            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                LoadDS2Generators(resourceHandler.AdjustedMapID, map);
            }

            if (IsRendering)
            {
                tasks = resourceHandler.LoadTextures(tasks, map);
                await Task.WhenAll(tasks);
                tasks = resourceHandler.LoadModels(tasks, map);
                await Task.WhenAll(tasks);

                resourceHandler.SetupNavmesh(map);

                // Real bad hack
                EnvMapTextures = TextureLocator.GetEnvMapTextureNames(resourceHandler.AdjustedMapID);

                ScheduleTextureRefresh();
            }

            // After everything loads, do some additional checks:
            await Task.WhenAll(tasks);
            postLoad = true;

            if (IsRendering)
            {
                // Update models (For checking meshes for Model Markers. & updates `CollisionName` field reference info)
                foreach (Entity obj in map.Objects)
                {
                    obj.UpdateRenderModel();
                }
            }

            // Check for duplicate EntityIDs
            CheckDupeEntityIDs(map);
        }
        catch (Exception e)
        {
#if DEBUG
            TaskLogs.AddLog("Map Load Failed (debug build)",
                LogLevel.Error, TaskLogs.LogPriority.High, e);
            throw;
#else
                // Store async exception so it can be caught by crash handler.
                LoadMapExceptions = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e);
#endif
        }
    }

    public static void CheckDupeEntityIDs(MapContainer map)
    {
        /* Notes about dupe Entity ID behavior in-game:
         * Entity ID dupes exist in vanilla (including dupe regions)
         * Duplicate Entity IDs for regions causes all regions later in the map object list to not function properly (only confirmed for DS1).
         * Currently unknown if dupes cause issues outside of regions.
         * Unique behavior can be seen when using dupe IDs with objects, and all objects with the same ID can be affected by single commands.
         * * This behavior is probably unintentional and may secretly cause issues. Unknown.
         *
         * At the moment, only region ID checking is necessary.
         */
        Dictionary<int, string> entityIDList = new();
        foreach (Entity obj in map.Objects)
        {
            var objType = obj.WrappedObject.GetType().ToString();
            if (objType.Contains("Region"))
            {
                PropertyInfo entityIDProp = obj.GetProperty("EntityID");
                if (entityIDProp != null)
                {
                    var idObj = entityIDProp.GetValue(obj.WrappedObject);
                    if (idObj is not int entityID)
                    {
                        // EntityID is uint in Elden Ring. Only <2^31 is used in practice.
                        // If really desired, a separate routine could be created.
                        if (idObj is uint uID)
                        {
                            entityID = unchecked((int)uID);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (entityID > 0)
                    {
                        var entryExists = entityIDList.TryGetValue(entityID, out var name);
                        if (entryExists)
                        {
                            TaskLogs.AddLog(
                                $"Duplicate EntityID: \"{entityID}\" is being used by multiple regions \"{obj.PrettyName}\" and \"{name}\"",
                                LogLevel.Warning);
                        }
                        else
                        {
                            entityIDList.Add(entityID, obj.PrettyName);
                        }
                    }
                }
            }
        }
    }

    public void LoadFlverInModelEditor(FLVER2 flver, MeshRenderableProxy proxy, string name)
    {
        ModelContainer container = new(this, name);

        container.LoadFlver(flver, proxy);

        if (!LoadedModelContainers.ContainsKey(name))
        {
            LoadedModelContainers.Add(name, container);
        }
        else
        {
            LoadedModelContainers[name] = container;
        }
    }

    private void SaveDS2Generators(MapContainer map)
    {
        // Load all the params
        ResourceDescriptor regparamad = ParamLocator.GetDS2GeneratorRegistParam(map.Name);
        ResourceDescriptor regparamadw = ParamLocator.GetDS2GeneratorRegistParam(map.Name, true);
        Param regparam = Param.Read(regparamad.AssetPath);
        PARAMDEF reglayout = ParamLocator.GetParamdefForParam(regparam.ParamType);
        regparam.ApplyParamdef(reglayout);

        ResourceDescriptor locparamad = ParamLocator.GetDS2GeneratorLocationParam(map.Name);
        ResourceDescriptor locparamadw = ParamLocator.GetDS2GeneratorLocationParam(map.Name, true);
        Param locparam = Param.Read(locparamad.AssetPath);
        PARAMDEF loclayout = ParamLocator.GetParamdefForParam(locparam.ParamType);
        locparam.ApplyParamdef(loclayout);

        ResourceDescriptor genparamad = ParamLocator.GetDS2GeneratorParam(map.Name);
        ResourceDescriptor genparamadw = ParamLocator.GetDS2GeneratorParam(map.Name, true);
        Param genparam = Param.Read(genparamad.AssetPath);
        PARAMDEF genlayout = ParamLocator.GetParamdefForParam(genparam.ParamType);
        genparam.ApplyParamdef(genlayout);

        ResourceDescriptor evtparamad = ParamLocator.GetDS2EventParam(map.Name);
        ResourceDescriptor evtparamadw = ParamLocator.GetDS2EventParam(map.Name, true);
        Param evtparam = Param.Read(evtparamad.AssetPath);
        PARAMDEF evtlayout = ParamLocator.GetParamdefForParam(evtparam.ParamType);
        evtparam.ApplyParamdef(evtlayout);

        ResourceDescriptor evtlparamad = ParamLocator.GetDS2EventLocationParam(map.Name);
        ResourceDescriptor evtlparamadw = ParamLocator.GetDS2EventLocationParam(map.Name, true);
        Param evtlparam = Param.Read(evtlparamad.AssetPath);
        PARAMDEF evtllayout = ParamLocator.GetParamdefForParam(evtlparam.ParamType);
        evtlparam.ApplyParamdef(evtllayout);

        ResourceDescriptor objparamad = ParamLocator.GetDS2ObjInstanceParam(map.Name);
        ResourceDescriptor objparamadw = ParamLocator.GetDS2ObjInstanceParam(map.Name, true);
        Param objparam = Param.Read(objparamad.AssetPath);
        PARAMDEF objlayout = ParamLocator.GetParamdefForParam(objparam.ParamType);
        objparam.ApplyParamdef(objlayout);

        // Clear them out
        regparam.ClearRows();
        locparam.ClearRows();
        genparam.ClearRows();
        evtparam.ClearRows();
        evtlparam.ClearRows();
        objparam.ClearRows();

        // Serialize objects
        if (!map.SerializeDS2Generators(locparam, genparam))
        {
            return;
        }

        if (!map.SerializeDS2Regist(regparam))
        {
            return;
        }

        if (!map.SerializeDS2Events(evtparam))
        {
            return;
        }

        if (!map.SerializeDS2EventLocations(evtlparam))
        {
            return;
        }

        if (!map.SerializeDS2ObjInstances(objparam))
        {
            return;
        }

        // Create a param directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(regparamadw.AssetPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(regparamadw.AssetPath));
        }

        // Save all the params
        if (File.Exists(regparamadw.AssetPath + ".temp"))
        {
            File.Delete(regparamadw.AssetPath + ".temp");
        }

        regparam.Write(regparamadw.AssetPath + ".temp", DCX.Type.None);
        if (File.Exists(regparamadw.AssetPath))
        {
            if (!File.Exists(regparamadw.AssetPath + ".bak"))
            {
                File.Copy(regparamadw.AssetPath, regparamadw.AssetPath + ".bak", true);
            }

            File.Copy(regparamadw.AssetPath, regparamadw.AssetPath + ".prev", true);
            File.Delete(regparamadw.AssetPath);
        }

        File.Move(regparamadw.AssetPath + ".temp", regparamadw.AssetPath);

        if (File.Exists(locparamadw.AssetPath + ".temp"))
        {
            File.Delete(locparamadw.AssetPath + ".temp");
        }

        locparam.Write(locparamadw.AssetPath + ".temp", DCX.Type.None);
        if (File.Exists(locparamadw.AssetPath))
        {
            if (!File.Exists(locparamadw.AssetPath + ".bak"))
            {
                File.Copy(locparamadw.AssetPath, locparamadw.AssetPath + ".bak", true);
            }

            File.Copy(locparamadw.AssetPath, locparamadw.AssetPath + ".prev", true);
            File.Delete(locparamadw.AssetPath);
        }

        File.Move(locparamadw.AssetPath + ".temp", locparamadw.AssetPath);

        if (File.Exists(genparamadw.AssetPath + ".temp"))
        {
            File.Delete(genparamadw.AssetPath + ".temp");
        }

        genparam.Write(genparamadw.AssetPath + ".temp", DCX.Type.None);
        if (File.Exists(genparamadw.AssetPath))
        {
            if (!File.Exists(genparamadw.AssetPath + ".bak"))
            {
                File.Copy(genparamadw.AssetPath, genparamadw.AssetPath + ".bak", true);
            }

            File.Copy(genparamadw.AssetPath, genparamadw.AssetPath + ".prev", true);
            File.Delete(genparamadw.AssetPath);
        }

        File.Move(genparamadw.AssetPath + ".temp", genparamadw.AssetPath);

        // Events
        if (File.Exists(evtparamadw.AssetPath + ".temp"))
        {
            File.Delete(evtparamadw.AssetPath + ".temp");
        }

        evtparam.Write(evtparamadw.AssetPath + ".temp", DCX.Type.None);
        if (File.Exists(evtparamadw.AssetPath))
        {
            if (!File.Exists(evtparamadw.AssetPath + ".bak"))
            {
                File.Copy(evtparamadw.AssetPath, evtparamadw.AssetPath + ".bak", true);
            }

            File.Copy(evtparamadw.AssetPath, evtparamadw.AssetPath + ".prev", true);
            File.Delete(evtparamadw.AssetPath);
        }

        File.Move(evtparamadw.AssetPath + ".temp", evtparamadw.AssetPath);

        // Event regions
        if (File.Exists(evtlparamadw.AssetPath + ".temp"))
        {
            File.Delete(evtlparamadw.AssetPath + ".temp");
        }

        evtlparam.Write(evtlparamadw.AssetPath + ".temp", DCX.Type.None);
        if (File.Exists(evtlparamadw.AssetPath))
        {
            if (!File.Exists(evtlparamadw.AssetPath + ".bak"))
            {
                File.Copy(evtlparamadw.AssetPath, evtlparamadw.AssetPath + ".bak", true);
            }

            File.Copy(evtlparamadw.AssetPath, evtlparamadw.AssetPath + ".prev", true);
            File.Delete(evtlparamadw.AssetPath);
        }

        File.Move(evtlparamadw.AssetPath + ".temp", evtlparamadw.AssetPath);

        // Object instances
        if (File.Exists(objparamadw.AssetPath + ".temp"))
        {
            File.Delete(objparamadw.AssetPath + ".temp");
        }

        objparam.Write(objparamadw.AssetPath + ".temp", DCX.Type.None);
        if (File.Exists(objparamadw.AssetPath))
        {
            if (!File.Exists(objparamadw.AssetPath + ".bak"))
            {
                File.Copy(objparamadw.AssetPath, objparamadw.AssetPath + ".bak", true);
            }

            File.Copy(objparamadw.AssetPath, objparamadw.AssetPath + ".prev", true);
            File.Delete(objparamadw.AssetPath);
        }

        File.Move(objparamadw.AssetPath + ".temp", objparamadw.AssetPath);
    }

    private DCX.Type GetCompressionType()
    {
        if (Smithbox.ProjectType == ProjectType.DS3)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }

        if (Smithbox.ProjectType == ProjectType.AC6)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            return DCX.Type.None;
        }
        else if (Smithbox.ProjectType == ProjectType.SDT)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (Smithbox.ProjectType == ProjectType.BB)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (Smithbox.ProjectType == ProjectType.DES)
        {
            return DCX.Type.None;
        }

        return DCX.Type.None;
    }

    /// <summary>
    ///     Save BTL light data
    /// </summary>
    public void SaveBTL(MapContainer map)
    {
        List<ResourceDescriptor> BTLs = MapLocator.GetMapBTLs(map.Name);
        List<ResourceDescriptor> BTLs_w = MapLocator.GetMapBTLs(map.Name, true);
        DCX.Type compressionType = GetCompressionType();
        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
        {
            for (var i = 0; i < BTLs.Count; i++)
            {
                using var bdt = BXF4.Read(BTLs[i].AssetPath, BTLs[i].AssetPath[..^3] + "bdt");
                BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));
                var btl = BTL.Read(file.Bytes);
                if (btl != null)
                {
                    List<BTL.Light> newLights = map.SerializeBtlLights(BTLs_w[i].AssetName);

                    // Only save BTL if it has been modified
                    if (JsonSerializer.Serialize(btl.Lights, BtlLightSerializerContext.Default.ListLight) !=
                        JsonSerializer.Serialize(newLights, BtlLightSerializerContext.Default.ListLight))
                    {
                        btl.Lights = newLights;
                        file.Bytes = btl.Write(DCX.Type.DCX_DFLT_10000_24_9);
                        var bdtPath = BTLs_w[i].AssetPath[..^3] + "bdt";

                        Utils.WriteWithBackup(Utils.GetLocalAssetPath(bdtPath), bdt,
                            Utils.GetLocalAssetPath(BTLs_w[i].AssetPath));
                    }
                }
            }
        }
        else
        {
            for (var i = 0; i < BTLs.Count; i++)
            {
                BTL btl = ReturnBTL(BTLs[i]);
                if (btl != null)
                {
                    List<BTL.Light> newLights = map.SerializeBtlLights(BTLs_w[i].AssetName);

                    // Only save BTL if it has been modified
                    if (JsonSerializer.Serialize(btl.Lights, BtlLightSerializerContext.Default.ListLight) !=
                        JsonSerializer.Serialize(newLights, BtlLightSerializerContext.Default.ListLight))
                    {
                        btl.Lights = newLights;

                        Utils.WriteWithBackup(Utils.GetLocalAssetPath(BTLs_w[i].AssetPath), btl);
                    }
                }
            }
        }
    }

    public void SaveMap(MapContainer map)
    {
        SaveBTL(map);
        try
        {
            ResourceDescriptor ad = MapLocator.GetMapMSB(map.Name);
            ResourceDescriptor adw = MapLocator.GetMapMSB(map.Name, true);
            IMsb msb;
            DCX.Type compressionType = GetCompressionType();
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                var prev = MSB3.Read(ad.AssetPath);
                MSB3 n = new();
                n.PartsPoses = prev.PartsPoses;
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Smithbox.ProjectType == ProjectType.ER)
            {
                var prev = MSBE.Read(ad.AssetPath);
                MSBE n = new();
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Smithbox.ProjectType == ProjectType.AC6)
            {
                var prev = MSB_AC6.Read(ad.AssetPath);
                MSB_AC6 n = new();
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                var prev = MSB2.Read(ad.AssetPath);
                MSB2 n = new();
                n.PartPoses = prev.PartPoses;
                msb = n;
            }
            else if (Smithbox.ProjectType == ProjectType.SDT)
            {
                var prev = MSBS.Read(ad.AssetPath);
                MSBS n = new();
                n.PartsPoses = prev.PartsPoses;
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Smithbox.ProjectType == ProjectType.BB)
            {
                msb = new MSBB();
            }
            else if (Smithbox.ProjectType == ProjectType.DES)
            {
                var prev = MSBD.Read(ad.AssetPath);
                MSBD n = new();
                n.Trees = prev.Trees;
                msb = n;
            }
            else
            {
                msb = new MSB1();
                //var t = MSB1.Read(ad.AssetPath);
                //((MSB1)msb).Models = t.Models;
            }

            map.SerializeToMSB(msb, Smithbox.ProjectType);

            // Create the map directory if it doesn't exist
            if (!Directory.Exists(Path.GetDirectoryName(adw.AssetPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(adw.AssetPath));
            }

            // Write as a temporary file to make sure there are no errors before overwriting current file 
            var mapPath = adw.AssetPath;
            //if (GetModProjectPathForFile(mapPath) != null)
            //{
            //    mapPath = GetModProjectPathForFile(mapPath);
            //}

            // If a backup file doesn't exist of the original file create it
            if (!File.Exists(mapPath + ".bak") && File.Exists(mapPath))
            {
                File.Copy(mapPath, mapPath + ".bak", true);
            }

            if (File.Exists(mapPath + ".temp"))
            {
                File.Delete(mapPath + ".temp");
            }

            msb.Write(mapPath + ".temp", compressionType);

            // Make a copy of the previous map
            if (File.Exists(mapPath))
            {
                File.Copy(mapPath, mapPath + ".prev", true);
            }

            // Move temp file as new map file
            if (File.Exists(mapPath))
            {
                File.Delete(mapPath);
            }

            File.Move(mapPath + ".temp", mapPath);

            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
            {
                SaveDS2Generators(map);
            }

            CheckDupeEntityIDs(map);
            map.HasUnsavedChanges = false;
            TaskLogs.AddLog($"Saved map {map.Name}");
        }
        catch (Exception e)
        {
            throw new SavingFailedException(Path.GetFileName(map.Name), e);
        }
    }

    public void SaveAllMaps()
    {
        foreach (KeyValuePair<string, ObjectContainer> m in LoadedObjectContainers)
        {
            if (m.Value != null)
            {
                if (m.Value is MapContainer ma)
                {
                    SaveMap(ma);
                }
            }
        }
    }

    public void UnloadModels(bool clearFromList = false)
    {
        List<ModelContainer> toUnload = new();
        foreach (var key in LoadedModelContainers.Keys)
        {
            if (LoadedModelContainers[key] != null)
            {
                toUnload.Add(LoadedModelContainers[key]);
            }
        }

        foreach (ModelContainer un in toUnload)
        {
            UnloadModelContainer(un, clearFromList);
        }
    }

    // This unloads only the Dummy and Node selectables
    public void UnloadTransformableEntities(bool clearFromList = false)
    {
        List<ModelContainer> toUnload = new();
        foreach (var key in LoadedModelContainers.Keys)
        {
            if (LoadedModelContainers[key] != null)
            {
                toUnload.Add(LoadedModelContainers[key]);
            }
        }

        foreach (ModelContainer container in toUnload)
        {
            if (LoadedModelContainers.ContainsKey(container.Name))
            {
                foreach (Entity obj in container.Objects)
                {
                    if (obj is TransformableNamedEntity)
                    {
                        if (obj != null)
                        {
                            obj.Dispose();
                        }
                    }
                }

                container.Clear();
                LoadedModelContainers[container.Name] = null;
                if (clearFromList)
                {
                    LoadedModelContainers.Remove(container.Name);
                }
            }
        }
    }

    public void UnloadModelContainer(ObjectContainer container, bool clearFromList = false)
    {
        if (LoadedModelContainers.ContainsKey(container.Name))
        {
            foreach (Entity obj in container.Objects)
            {
                if (obj != null)
                {
                    obj.Dispose();
                }
            }

            container.Clear();
            LoadedModelContainers[container.Name] = null;
            if (clearFromList)
            {
                LoadedModelContainers.Remove(container.Name);
            }
        }
    }

    public void UnloadContainer(ObjectContainer container, bool clearFromList = false)
    {
        HavokUtils.OnUnloadMap(container.Name);

        if (LoadedObjectContainers.ContainsKey(container.Name))
        {
            foreach (Entity obj in container.Objects)
            {
                if (obj != null)
                {
                    obj.Dispose();
                }
            }

            container.Clear();
            LoadedObjectContainers[container.Name] = null;
            if (clearFromList)
            {
                LoadedObjectContainers.Remove(container.Name);
            }
        }
    }

    public void UnloadAllMaps()
    {
        List<ObjectContainer> toUnload = new();
        foreach (var key in LoadedObjectContainers.Keys)
        {
            if (LoadedObjectContainers[key] != null)
            {
                toUnload.Add(LoadedObjectContainers[key]);
            }
        }

        foreach (ObjectContainer un in toUnload)
        {
            if (un is MapContainer ma)
            {
                UnloadContainer(ma);
            }
        }
    }

    public void UnloadAll(bool clearFromList = false)
    {
        List<ObjectContainer> toUnload = new();
        foreach (var key in LoadedObjectContainers.Keys)
        {
            if (LoadedObjectContainers[key] != null)
            {
                toUnload.Add(LoadedObjectContainers[key]);
            }
        }

        foreach (ObjectContainer un in toUnload)
        {
            UnloadContainer(un, clearFromList);
        }
    }

    public void ScheduleTextureRefresh()
    {
        if (GameType == ProjectType.DS1)
        {
            ResourceManager.ScheduleUDSMFRefresh();
        }

        ResourceManager.ScheduleUnloadedTexturesRefresh();
    }
}
