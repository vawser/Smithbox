using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Tools.MapConnections;
using StudioCore.Editors.ParamEditor;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.MsbEditor;

/// <summary>
/// A universe is a collection of loaded maps with methods to load, serialize,
/// and unload individual maps.
/// </summary>
public class Universe
{
    /// <summary>
    /// The rendering scene context
    /// </summary>
    public RenderScene RenderScene;

    /// <summary>
    /// Holds exception dispatches that can occur during map loading
    /// </summary>
    public ExceptionDispatchInfo LoadMapExceptions = null;

    /// <summary>
    /// True after or before a map load. False if a map load is on-going.
    /// </summary>
    public bool HasProcessedMapLoad;

    /// <summary>
    /// The map entity containers, sorted by map ID
    /// </summary>
    public Dictionary<string, ObjectContainer> LoadedObjectContainers { get; } = new();

    /// <summary>
    /// The entity selection context
    /// </summary>
    public ViewportSelection Selection { get; }

    /// <summary>
    /// Task list for the async map loads
    /// </summary>
    private List<Task> Tasks = new();

    public Universe(RenderScene scene, ViewportSelection sel)
    {
        RenderScene = scene;
        Selection = sel;

        if (RenderScene == null)
        {
            CFG.Current.Viewport_Enable_Rendering = false;
        }
    }

    /// <summary>
    /// Get the list of map containers available.
    /// </summary>
    public IOrderedEnumerable<KeyValuePair<string, ObjectContainer>> GetMapContainerList()
    {
        return LoadedObjectContainers
            .Where(k => k.Key is not null)
            .OrderBy(k => k.Key);
    }

    /// <summary>
    /// Get the count of map containers available.
    /// </summary>
    public int GetMapContainerCount()
    {
        return LoadedObjectContainers.Count;
    }

    /// <summary>
    /// Get the object container for the passed Map ID if possible, or return null.
    /// </summary>
    public ObjectContainer GetObjectContainerForMap(string mapId)
    {
        if (LoadedObjectContainers.ContainsKey(mapId))
        {
            return LoadedObjectContainers[mapId];
        }

        return null;
    }

    /// <summary>
    /// Get the list of map containers available that are loaded.
    /// </summary>
    public List<MapContainer> GetLoadedMapContainerList()
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

    /// <summary>
    /// Get the loaded map container for the passed Map ID if possible, or return null.
    /// </summary>
    public MapContainer GetLoadedMapContainer(string id)
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

    /// <summary>
    /// Load a map asynchronously based on the passed map ID
    /// </summary>
    public async void LoadMapAsync(string mapid, bool selectOnLoad = false, bool fastLoad = false)
    {
        if (LoadedObjectContainers.TryGetValue(mapid, out var m) && m != null)
        {
            TaskLogs.AddLog($"Map \"{mapid}\" is already loaded",
                LogLevel.Information, StudioCore.Tasks.LogPriority.Normal);
            return;
        }

        if (!fastLoad)
        {
            HavokCollisionManager.OnLoadMap(mapid);
        }

        try
        {
            HasProcessedMapLoad = false;

            Smithbox.EditorHandler.MapEditor.DisplayGroupView.SetupDrawgroupCount();

            MapContainer map = new(mapid);

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

            if (CFG.Current.Viewport_Enable_Rendering)
            {
                if (Smithbox.ProjectType != ProjectType.AC4 &&
                    Smithbox.ProjectType != ProjectType.ACFA &&
                    Smithbox.ProjectType != ProjectType.ACV &&
                    Smithbox.ProjectType != ProjectType.ACVD)
                    resourceHandler.SetupHumanEnemySubstitute();

                resourceHandler.SetupModelLoadLists();
                resourceHandler.SetupTexturelLoadLists();
                resourceHandler.SetupModelMasks(map);
            }

            resourceHandler.LoadLights(map);

            if (CFG.Current.Viewport_Enable_Rendering)
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

            if (CFG.Current.Viewport_Enable_Rendering)
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

            if (CFG.Current.Viewport_Enable_Rendering)
            {
                Tasks = resourceHandler.LoadTextures(Tasks, map);
                await Task.WhenAll(Tasks);
                Tasks = resourceHandler.LoadModels(Tasks, map);
                await Task.WhenAll(Tasks);

                resourceHandler.SetupNavmesh(map);

                ScheduleTextureRefresh();
            }

            // After everything loads, do some additional checks:
            await Task.WhenAll(Tasks);
            HasProcessedMapLoad = true;

            if (CFG.Current.Viewport_Enable_Rendering)
            {
                // Update models (For checking meshes for Model Markers. & updates `CollisionName` field reference info)
                foreach (Entity obj in map.Objects)
                {
                    obj.UpdateRenderModel();
                }
            }

            // Check for duplicate EntityIDs
            CheckDupeEntityIDs(map);

            // Set the map transform to the saved position, rotation and scale.
            //map.LoadMapTransform();
        }
        catch (Exception e)
        {
#if DEBUG
            TaskLogs.AddLog("Map Load Failed (debug build)",
                LogLevel.Error, StudioCore.Tasks.LogPriority.High, e);
            throw;
#else
                // Store async exception so it can be caught by crash handler.
                LoadMapExceptions = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e);
#endif
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
            //TODO ACFA
            else if (Smithbox.ProjectType == ProjectType.ACFA)
            {
                MSBFA prev = MSBFA.Read(ad.AssetPath);
                MSBFA n = new();
                n.Models.Version = prev.Models.Version;
                n.Events.Version = prev.Events.Version;
                n.Parts.Version = prev.Parts.Version;
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                n.DrawingTree = prev.DrawingTree;
                n.CollisionTree = prev.CollisionTree;
                msb = n;
            }
            else if (Smithbox.ProjectType == ProjectType.ACV)
            {
                MSBV prev = MSBV.Read(ad.AssetPath);
                MSBV n = new();
                n.Models.Version = prev.Models.Version;
                n.Events.Version = prev.Events.Version;
                n.Parts.Version = prev.Parts.Version;
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                n.DrawingTree = prev.DrawingTree;
                n.CollisionTree = prev.CollisionTree;
                msb = n;
            }
            else if (Smithbox.ProjectType == ProjectType.ACVD)
            {
                MSBVD prev = MSBVD.Read(ad.AssetPath);
                MSBVD n = new();
                n.Models.Version = prev.Models.Version;
                n.Events.Version = prev.Events.Version;
                n.Parts.Version = prev.Parts.Version;
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                n.DrawingTree = prev.DrawingTree;
                n.CollisionTree = prev.CollisionTree;
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
            if (!CFG.Current.MapEditor_IgnoreSaveExceptions)
            {
                throw new SavingFailedException(Path.GetFileName(map.Name), e);
            }
        }

        // Save the current map transform for this map
        //map.SaveMapTransform();
    }


    /// <summary>
    /// 
    /// </summary>
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

            MsbEntity obj = new(map, row, MsbEntityType.DS2GeneratorRegist);
            map.AddObject(obj);
        }

        Param locparam = ParamBank.PrimaryBank.Params[$"generatorlocation_{mapid}"];
        foreach (Param.Row row in locparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "generator_" + row.ID;
            }


            MergedParamRow mergedRow = new("GENERATOR_MERGED_PARAM");
            mergedRow.AddRow("generator-loc", row);
            generatorParams.Add(row.ID, mergedRow);

            MsbEntity obj = new(map, mergedRow, MsbEntityType.DS2Generator);
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
                MergedParamRow mergedRow = new("GENERATOR_MERGED_PARAM");
                mergedRow.AddRow("generator", row);
                generatorParams.Add(row.ID, mergedRow);
                MsbEntity obj = new(map, mergedRow, MsbEntityType.DS2Generator);
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
                    ResourceDescriptor asset = ModelLocator.GetChrModel($@"c{chrid}", $@"c{chrid}");
                    MeshRenderableProxy model = MeshRenderableProxy.MeshRenderableFromFlverResource(
                        RenderScene, asset.AssetVirtualPath, ModelMarkerType.Enemy, null);
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

            MsbEntity obj = new(map, row, MsbEntityType.DS2Event);
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

            MsbEntity obj = new(map, row, MsbEntityType.DS2EventLocation);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);

            // Try rendering as a box for now
            DebugPrimitiveRenderableProxy mesh = RenderableHelper.GetBoxRegionProxy(RenderScene);
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

            MsbEntity obj = new(map, row, MsbEntityType.DS2ObjectInstance);
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

    public void LoadRelatedMapsER(string mapid)
    {
        IReadOnlyDictionary<string, SpecialMapConnections.RelationType> relatedMaps =
            SpecialMapConnections.GetRelatedMaps(mapid, LoadedObjectContainers.Keys);

        foreach (KeyValuePair<string, SpecialMapConnections.RelationType> map in relatedMaps)
        {
            LoadMap(map.Key);
            Smithbox.EditorHandler.MapEditor.MapListView.SignalLoad(map.Key);
        }
    }

    public bool LoadMap(string mapid, bool selectOnLoad = false, bool fastLoad = false)
    {
        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            if (ParamBank.PrimaryBank.Params == null)
            {
                // ParamBank must be loaded for DS2 maps
                TaskLogs.AddLog("Cannot load DS2 maps when params are not loaded.",
                    LogLevel.Warning, StudioCore.Tasks.LogPriority.High);
                return false;
            }
        }

        ResourceDescriptor ad = MapLocator.GetMapMSB(mapid);
        if (ad.AssetPath == null)
        {
            return false;
        }

        LoadMapAsync(mapid, selectOnLoad, fastLoad);
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
                LogLevel.Error, StudioCore.Tasks.LogPriority.Normal, e);
            return null;
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
            return DCX.Type.DCX_KRAK_MAX;
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

    public void UnloadContainer(ObjectContainer container, bool clearFromList = false)
    {
        HavokCollisionManager.OnUnloadMap(container.Name);

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
        if (Smithbox.ProjectType == ProjectType.DS1)
        {
            ResourceManager.ScheduleUDSMFRefresh();
        }

        ResourceManager.ScheduleUnloadedTexturesRefresh();
    }

}

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(List<BTL.Light>))]
internal partial class BtlLightSerializerContext : JsonSerializerContext
{
}
