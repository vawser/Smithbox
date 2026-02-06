using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A universe is a collection of loaded maps with methods to load, serialize,
/// and unload individual maps.
/// </summary>
public class MapUniverse : IUniverse
{
    public MapEditorView View;
    public ProjectEntry Project;

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
    /// The entity selection context
    /// </summary>
    public ViewportSelection Selection { get; }

    /// <summary>
    /// Task list for the async map loads
    /// </summary>
    private List<Task> Tasks = new();

    public MapUniverse(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        RenderScene = view.RenderScene;
        Selection = view.ViewportSelection;

        if (RenderScene == null)
        {
            CFG.Current.Viewport_Enable_Rendering = false;
        }
        else
        {
            CFG.Current.Viewport_Enable_Rendering = true;
        }
    }
    public bool LoadMap(string mapid, bool selectOnLoad = false, bool fastLoad = false)
    {
        if (View.Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            if (Project.Handler.ParamEditor == null)
            {
                // ParamBank must be loaded for DS2 maps
                TaskLogs.AddLog("Cannot load DS2 maps when params are not loaded.",
                    LogLevel.Warning, LogPriority.High);
                return false;
            }
        }

        ResourceManager.ClearUnusedResources();

        View.ViewportSelection.ClearSelection();

        LoadMapAsync(mapid, selectOnLoad, fastLoad);

        return true;
    }

    public void UnloadMap(string mapID, bool clearFromList = false)
    {
        View.ViewportSelection.ClearSelection();
        View.ViewportActionManager.Clear();

        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            var curMapID = entry.Key.Filename;

            if (curMapID == mapID)
            {
                var wrapper = entry.Value;

                ResourceManager.ClearUnusedResources();
                View.ModelInsightTool.ClearEntry(wrapper.MapContainer);

                View.EntityTypeCache.RemoveMapFromCache(wrapper.MapContainer);

                View.HavokCollisionBank.OnUnloadMap(curMapID);
                View.HavokNavmeshBank.OnUnloadMap(curMapID);

                if (View.Selection.SelectedMapContainer == wrapper.MapContainer)
                {
                    View.Selection.SelectedMapID = "";
                    View.Selection.SelectedMapContainer = null;
                }

                wrapper.MapContainer.LoadState = MapContentLoadState.Unloaded;
                wrapper.MapContainer.Unload();
                wrapper.MapContainer.Clear();
                wrapper.MapContainer = null;
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    public string ModelDataMapID { get; set; }

    /// <summary>
    /// Load a map asynchronously based on the passed map ID
    /// </summary>
    public async void LoadMapAsync(string mapid, bool selectOnLoad = false, bool fastLoad = false)
    {
        var fileEntry = View.Selection.GetFileEntryFromMapID(mapid);
        var existingMap = View.Selection.GetMapContainerFromMapID(mapid);

        if (existingMap != null && existingMap.LoadState is MapContentLoadState.Loaded)
        {
            TaskLogs.AddLog($"Map \"{mapid}\" is already loaded",
                LogLevel.Information, LogPriority.Normal);
            return;
        }

        if (!fastLoad)
        {
            View.HavokCollisionBank.OnLoadMap(mapid);
            View.HavokNavmeshBank.OnLoadMap(mapid);
        }

        try
        {
            HasProcessedMapLoad = false;

            var newMap = new MapContainer(View, mapid);

            ModelDataMapID = newMap.Name;
            View.ModelInsightTool.AddEntry(newMap);

            View.DisplayGroupTool.SetupDrawgroupCount();

            MapResourceHandler resourceHandler = new MapResourceHandler(View, mapid);

            await resourceHandler.ReadMap(mapid);

            if (resourceHandler.Msb != null)
            {
                newMap.LoadMSB(resourceHandler.Msb);

                if (CFG.Current.Viewport_Enable_Rendering)
                {
                    resourceHandler.SetupHumanEnemySubstitute();
                    resourceHandler.SetupModelLoadLists();
                    resourceHandler.SetupTexturelLoadLists();
                    resourceHandler.SetupModelMasks(newMap);
                }

                LoadLights(newMap);

                View.AutoInvadeBank.LoadAIP(newMap);
                View.LightAtlasBank.LoadBTAB(newMap);
                View.LightProbeBank.LoadBTPB(newMap);

                View.HavokNavmeshBank.LoadHavokNVA(newMap, resourceHandler);

                if (CFG.Current.Viewport_Enable_Rendering)
                {
                    // Handle the map offsets for games that use a tile system
                    // This is what adjusts the map position/rotation so they are presented in the same way they appear in-game.
                    // By default, maps assume 0,0,0 as their origin, which means without this they overlap.

                    if (View.Project.Descriptor.ProjectType is ProjectType.ER)
                    {
                        if (MapConnections_ER.GetMapTransform(View, mapid) is Transform
                            loadTransform)
                        {
                            newMap.RootObject.GetUpdateTransformAction(loadTransform).Execute();
                        }
                    }

                    if (View.Project.Descriptor.ProjectType is ProjectType.NR)
                    {
                        if (MapConnections_NR.GetMapTransform(View, mapid) is Transform
                            loadTransform)
                        {
                            newMap.RootObject.GetUpdateTransformAction(loadTransform).Execute();
                        }
                    }
                }

                Project.Handler.MapData.PrimaryBank.Maps[fileEntry].MapContainer = newMap;
                newMap.LoadState = MapContentLoadState.Loaded;

                if (CFG.Current.Viewport_Enable_Rendering)
                {
                    // Intervene in the UI to change selection if requested.
                    // We want to do this as soon as the RootObject is available, rather than at the end of all jobs.
                    if (selectOnLoad)
                    {
                        Selection.ClearSelection();
                        Selection.AddSelection(newMap.RootObject);
                    }
                }

                if (View.Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    LoadDS2Generators(resourceHandler.AdjustedMapID, newMap);
                }

                if (CFG.Current.Viewport_Enable_Rendering)
                {
                    Tasks = resourceHandler.LoadTextures(Tasks, newMap);
                    await Task.WhenAll(Tasks);
                    Tasks = resourceHandler.LoadModels(Tasks, newMap);
                    await Task.WhenAll(Tasks);

                    ScheduleTextureRefresh();
                }

                // After everything loads, do some additional checks:
                await Task.WhenAll(Tasks);
                HasProcessedMapLoad = true;

                if (CFG.Current.Viewport_Enable_Rendering)
                {
                    // Update models (For checking meshes for Model Markers. & updates `CollisionName` field reference info)
                    foreach (Entity obj in newMap.Objects)
                    {
                        obj.UpdateRenderModel();
                    }
                }

                newMap.LightAtlasResolver.BuildReferenceMaps();

                // Check for duplicate EntityIDs
                CheckDupeEntityIDs(newMap);

                // Set the map transform to the saved position, rotation and scale.
                //map.LoadMapTransform();

                // HACK: this fixes the weird ghost state between the viewport and content list
                CloneMapObjectsAction action = new(
                    View,
                    new List<MsbEntity>() { (MsbEntity)newMap.RootObject }, false,
                    null, null, true);

                View.ViewportActionManager.ExecuteAction(action);

                if (selectOnLoad)
                {
                    View.Selection.SelectedMapID = mapid;
                    View.Selection.SelectedMapContainer = newMap;
                }
            }
        }
        catch (Exception e)
        {
#if DEBUG
            TaskLogs.AddLog("Map Load Failed (debug build)",
                LogLevel.Error, LogPriority.High, e);
            throw;
#else
                // Store async exception so it can be caught by crash handler.
                LoadMapExceptions = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e);
#endif
        }
    }

    public void LoadLights(MapContainer map)
    {
        if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            foreach (var entry in View.Project.Locator.DS2_LightFiles.Entries)
            {
                if (entry.Filename.Contains(map.Name))
                {
                    BTL btl = LoadBTL(entry);
                    if (btl != null)
                    {
                        map.LoadBTL(entry, btl);
                    }
                }
            }
        }
        else
        {
            foreach (var entry in View.Project.Locator.LightFiles.Entries)
            {
                if (entry.Filename.Contains(map.Name))
                {
                    BTL btl = LoadBTL(entry);

                    if (btl != null)
                    {
                        map.LoadBTL(entry, btl);
                    }
                }
            }
        }
    }

    private BTL LoadBTL(FileDictionaryEntry curEntry)
    {
        BTL btl = null;

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var bhdPath = curEntry.Path;
            var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

            try
            {
                var bdtFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bdtPath);
                var bhdFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bhdPath);

                using var bdt = BXF4.Read(bhdFile, bdtFile);
                BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));

                if (file == null)
                {
                    return null;
                }

                btl = BTL.Read(file.Bytes);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Map Editor] Failed to load BTL file.", LogLevel.Error, LogPriority.High, e);
            }
        }
        else
        {
            Memory<byte>? btlFile = (Memory<byte>)View.Project.Handler.MapData?.PrimaryBank.TargetFS.ReadFile(curEntry.Path);

            if (btlFile.HasValue)
                btl = BTL.Read(btlFile.Value);
        }

        return btl;
    }

    public void SaveMap(MapContainer map, bool autoSave = false)
    {
        if (!autoSave && CFG.Current.MapEditor_ManualSave_IncludeBTL ||
            autoSave && CFG.Current.MapEditor_AutomaticSave_IncludeBTL)
        {
            SaveBTL(View, map);
        }

        if (!autoSave && CFG.Current.MapEditor_ManualSave_IncludeAIP ||
            autoSave && CFG.Current.MapEditor_AutomaticSave_IncludeAIP)
        {
            View.AutoInvadeBank.SaveAIP(map);
        }

        if (!autoSave && CFG.Current.MapEditor_ManualSave_IncludeBTAB ||
            autoSave && CFG.Current.MapEditor_AutomaticSave_IncludeBTAB)
        {
            View.LightAtlasBank.SaveBTAB(map);
        }

        if (!autoSave && CFG.Current.MapEditor_ManualSave_IncludeBTPB ||
            autoSave && CFG.Current.MapEditor_AutomaticSave_IncludeBTPB)
        {
            View.LightProbeBank.SaveBTPB(map);
        }

        if (!autoSave && CFG.Current.MapEditor_ManualSave_IncludeNVA ||
            autoSave && CFG.Current.MapEditor_AutomaticSave_IncludeNVA)
        {
            View.HavokNavmeshBank.SaveHavokNVA(map);
        }

        if (!autoSave && CFG.Current.MapEditor_ManualSave_IncludeMSB ||
            autoSave && CFG.Current.MapEditor_AutomaticSave_IncludeMSB)
        {
            try
            {
                var curEntry = Project.Locator.MapFiles.Entries.FirstOrDefault(e => e.Filename == map.Name);
                var mapData = (Memory<byte>)Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(curEntry.Path);

                IMsb msb;
                DCX.Type compressionType = GetCompressionType();
                if (View.Project.Descriptor.ProjectType == ProjectType.DS3)
                {
                    var prev = MSB3.Read(mapData);
                    MSB3 n = new();
                    //n.PartsPoses = prev.PartsPoses;
                    n.Layers = prev.Layers;
                    n.Routes = prev.Routes;
                    msb = n;
                }
                else if (View.Project.Descriptor.ProjectType == ProjectType.ER)
                {
                    var prev = MSBE.Read(mapData);
                    MSBE n = new();
                    n.Layers = prev.Layers;
                    n.Routes = prev.Routes;
                    msb = n;
                }
                else if (View.Project.Descriptor.ProjectType == ProjectType.NR)
                {
                    var prev = MSB_NR.Read(mapData);
                    MSB_NR n = new();
                    n.Layers = prev.Layers;
                    n.Routes = prev.Routes;
                    msb = n;
                }
                else if (View.Project.Descriptor.ProjectType == ProjectType.AC6)
                {
                    var prev = MSB_AC6.Read(mapData);
                    MSB_AC6 n = new();
                    n.Layers = prev.Layers;
                    n.Routes = prev.Routes;
                    msb = n;
                }
                else if (View.Project.Descriptor.ProjectType == ProjectType.DS2S || View.Project.Descriptor.ProjectType == ProjectType.DS2)
                {
                    var prev = MSB2.Read(mapData);
                    MSB2 n = new();
                    //n.PartPoses = prev.PartPoses;
                    msb = n;
                }
                else if (View.Project.Descriptor.ProjectType == ProjectType.SDT)
                {
                    var prev = MSBS.Read(mapData);
                    MSBS n = new();
                    n.PartsPoses = prev.PartsPoses;
                    n.Layers = prev.Layers;
                    n.Routes = prev.Routes;
                    msb = n;
                }
                else if (View.Project.Descriptor.ProjectType == ProjectType.BB)
                {
                    msb = new MSBB();
                }
                else if (View.Project.Descriptor.ProjectType == ProjectType.DES)
                {
                    var prev = MSBD.Read(mapData);
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

                map.SerializeToMSB(msb, View.Project.Descriptor.ProjectType);

                try
                {
                    var newMapData = msb.Write(compressionType);
                    Project.VFS.ProjectFS.WriteFile(curEntry.Path, newMapData);

                    if (View.Project.Descriptor.ProjectType == ProjectType.DS2S || View.Project.Descriptor.ProjectType == ProjectType.DS2)
                    {
                        SaveDS2Generators(map);
                    }

                    CheckDupeEntityIDs(map);

                    map.HasUnsavedChanges = false;
                    TaskLogs.AddLog($"[Map Editor] Saved map: {curEntry.Filename}");
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Map Editor] Failed to save map: {curEntry.Filename}", LogLevel.Error, LogPriority.High, e);

                    if (!CFG.Current.MapEditor_IgnoreSaveExceptions)
                    {
                        throw new SavingFailedException(Path.GetFileName(map.Name), e);
                    }
                }
            }
            catch (Exception e)
            {
                if (!CFG.Current.MapEditor_IgnoreSaveExceptions)
                {
                    throw new SavingFailedException(Path.GetFileName(map.Name), e);
                }
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

        Param regparam = Project.Handler.ParamData.PrimaryBank.Params[$"generatorregistparam_{mapid}"];
        foreach (Param.Row row in regparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "regist_" + row.ID;
            }

            registParams.Add(row.ID, row);

            MsbEntity obj = new(View.Universe, map, row, MsbEntityType.DS2GeneratorRegist);
            map.AddObject(obj);
        }

        Param locparam = Project.Handler.ParamData.PrimaryBank.Params[$"generatorlocation_{mapid}"];
        foreach (Param.Row row in locparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "generator_" + row.ID;
            }


            MergedParamRow mergedRow = new("GENERATOR_MERGED_PARAM");
            mergedRow.AddRow("generator-loc", row);
            generatorParams.Add(row.ID, mergedRow);

            MsbEntity obj = new(View.Universe, map, mergedRow, MsbEntityType.DS2Generator);
            generatorObjs.Add(row.ID, obj);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);
        }

        HashSet<ResourceDescriptor> chrsToLoad = new();
        Param genparam = Project.Handler.ParamData.PrimaryBank.Params[$"generatorparam_{mapid}"];
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
                MsbEntity obj = new(View.Universe, map, mergedRow, MsbEntityType.DS2Generator);
                generatorObjs.Add(row.ID, obj);
                map.AddObject(obj);
            }

            var registid = (uint)row.GetCellHandleOrThrow("GeneratorRegistParam").Value;
            if (registParams.ContainsKey(registid))
            {
                Param.Row regist = registParams[registid];
                var chrid = Project.Handler.ParamData.PrimaryBank.GetChrIDForEnemy(
                    (int)regist.GetCellHandleOrThrow("EnemyParamID").Value);
                if (chrid != null)
                {
                    ResourceDescriptor asset = ModelLocator.GetChrModel(View.Project, $@"c{chrid}", $@"c{chrid}");
                    MeshRenderableProxy model = MeshRenderableProxy.MeshRenderableFromFlverResource(
                        RenderScene, asset.AssetVirtualPath, ModelMarkerType.Enemy, null);
                    model.DrawFilter = RenderFilter.Character;
                    generatorObjs[row.ID].RenderSceneMesh = model;
                    model.SetSelectable(generatorObjs[row.ID]);
                    chrsToLoad.Add(asset);

                    if (CFG.Current.Viewport_Enable_Texturing)
                    {
                        // TPF
                        var textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, $@"c{chrid}", false);

                        if (textureAsset.IsValid())
                            chrsToLoad.Add(textureAsset);

                        // BND
                        textureAsset = TextureLocator.GetCharacterTextureVirtualPath(View.Project, $@"c{chrid}", true);

                        if (textureAsset.IsValid())
                            chrsToLoad.Add(textureAsset);
                    }
                }
            }
        }

        Param evtparam = Project.Handler.ParamData.PrimaryBank.Params[$"eventparam_{mapid}"];
        foreach (Param.Row row in evtparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "event_" + row.ID;
            }

            eventParams.Add(row.ID, row);

            MsbEntity obj = new(View.Universe, map, row, MsbEntityType.DS2Event);
            map.AddObject(obj);
        }

        Param evtlparam = Project.Handler.ParamData.PrimaryBank.Params[$"eventlocation_{mapid}"];
        foreach (Param.Row row in evtlparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "eventloc_" + row.ID;
            }

            eventLocationParams.Add(row.ID, row);

            MsbEntity obj = new(View.Universe, map, row, MsbEntityType.DS2EventLocation);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);

            // Try rendering as a box for now
            DebugPrimitiveRenderableProxy mesh = RenderableHelper.GetBoxRegionProxy(RenderScene);
            mesh.World = obj.GetLocalTransform().WorldMatrix;
            obj.RenderSceneMesh = mesh;
            mesh.DrawFilter = RenderFilter.Region;
            mesh.SetSelectable(obj);
        }

        Param objparam = Project.Handler.ParamData.PrimaryBank.Params[$"mapobjectinstanceparam_{mapid}"];
        foreach (Param.Row row in objparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "objinstance_" + row.ID;
            }

            objectInstanceParams.Add(row.ID, row);

            MsbEntity obj = new(View.Universe, map, row, MsbEntityType.DS2ObjectInstance);
            map.AddObject(obj);
        }

        ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading chrs");
        foreach (ResourceDescriptor chr in chrsToLoad)
        {
            if (chr.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(chr.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceType.Flver);
            }
            else if (chr.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(chr.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }
        }

        job.Complete();
    }

    public void LoadRelatedMapsER(string mapid)
    {
        IReadOnlyDictionary<string, MapConnectionRelationType> relatedMaps =
            MapConnections_ER.GetRelatedMaps(View, mapid);

        foreach (KeyValuePair<string, MapConnectionRelationType> map in relatedMaps)
        {
            View.Universe.LoadMap(map.Key);
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
        ResourceDescriptor regparamad = ParamLocator.GetDS2GeneratorRegistParam(View.Project, map.Name);
        ResourceDescriptor regparamadw = ParamLocator.GetDS2GeneratorRegistParam(View.Project, map.Name, true);
        Param regparam = Param.Read(regparamad.AssetPath);
        PARAMDEF reglayout = ParamLocator.GetParamdefForParam(View.Project, regparam.ParamType);
        regparam.ApplyParamdef(reglayout);

        ResourceDescriptor locparamad = ParamLocator.GetDS2GeneratorLocationParam(View.Project, map.Name);
        ResourceDescriptor locparamadw = ParamLocator.GetDS2GeneratorLocationParam(View.Project, map.Name, true);
        Param locparam = Param.Read(locparamad.AssetPath);
        PARAMDEF loclayout = ParamLocator.GetParamdefForParam(View.Project, locparam.ParamType);
        locparam.ApplyParamdef(loclayout);

        ResourceDescriptor genparamad = ParamLocator.GetDS2GeneratorParam(View.Project, map.Name);
        ResourceDescriptor genparamadw = ParamLocator.GetDS2GeneratorParam(View.Project, map.Name, true);
        Param genparam = Param.Read(genparamad.AssetPath);
        PARAMDEF genlayout = ParamLocator.GetParamdefForParam(View.Project, genparam.ParamType);
        genparam.ApplyParamdef(genlayout);

        ResourceDescriptor evtparamad = ParamLocator.GetDS2EventParam(View.Project, map.Name);
        ResourceDescriptor evtparamadw = ParamLocator.GetDS2EventParam(View.Project, map.Name, true);
        Param evtparam = Param.Read(evtparamad.AssetPath);
        PARAMDEF evtlayout = ParamLocator.GetParamdefForParam(View.Project, evtparam.ParamType);
        evtparam.ApplyParamdef(evtlayout);

        ResourceDescriptor evtlparamad = ParamLocator.GetDS2EventLocationParam(View.Project, map.Name);
        ResourceDescriptor evtlparamadw = ParamLocator.GetDS2EventLocationParam(View.Project, map.Name, true);
        Param evtlparam = Param.Read(evtlparamad.AssetPath);
        PARAMDEF evtllayout = ParamLocator.GetParamdefForParam(View.Project, evtlparam.ParamType);
        evtlparam.ApplyParamdef(evtllayout);

        ResourceDescriptor objparamad = ParamLocator.GetDS2ObjInstanceParam(View.Project, map.Name);
        ResourceDescriptor objparamadw = ParamLocator.GetDS2ObjInstanceParam(View.Project, map.Name, true);
        Param objparam = Param.Read(objparamad.AssetPath);
        PARAMDEF objlayout = ParamLocator.GetParamdefForParam(View.Project, objparam.ParamType);
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
            if (CFG.Current.Project_Enable_Backup_Saves)
            {
                if (!File.Exists(regparamadw.AssetPath + ".bak"))
                {
                    File.Copy(regparamadw.AssetPath, regparamadw.AssetPath + ".bak", true);
                }

                File.Copy(regparamadw.AssetPath, regparamadw.AssetPath + ".prev", true);
            }
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
            if (CFG.Current.Project_Enable_Backup_Saves)
            {
                if (!File.Exists(locparamadw.AssetPath + ".bak"))
                {
                    File.Copy(locparamadw.AssetPath, locparamadw.AssetPath + ".bak", true);
                }

                File.Copy(locparamadw.AssetPath, locparamadw.AssetPath + ".prev", true);
            }
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
            if (CFG.Current.Project_Enable_Backup_Saves)
            {
                if (!File.Exists(genparamadw.AssetPath + ".bak"))
                {
                    File.Copy(genparamadw.AssetPath, genparamadw.AssetPath + ".bak", true);
                }

                File.Copy(genparamadw.AssetPath, genparamadw.AssetPath + ".prev", true);
            }
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
            if (CFG.Current.Project_Enable_Backup_Saves)
            {
                if (!File.Exists(evtparamadw.AssetPath + ".bak"))
                {
                    File.Copy(evtparamadw.AssetPath, evtparamadw.AssetPath + ".bak", true);
                }

                File.Copy(evtparamadw.AssetPath, evtparamadw.AssetPath + ".prev", true);
            }
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
            if (CFG.Current.Project_Enable_Backup_Saves)
            {
                if (!File.Exists(evtlparamadw.AssetPath + ".bak"))
                {
                    File.Copy(evtlparamadw.AssetPath, evtlparamadw.AssetPath + ".bak", true);
                }

                File.Copy(evtlparamadw.AssetPath, evtlparamadw.AssetPath + ".prev", true);
            }
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
            if (CFG.Current.Project_Enable_Backup_Saves)
            {
                if (!File.Exists(objparamadw.AssetPath + ".bak"))
                {
                    File.Copy(objparamadw.AssetPath, objparamadw.AssetPath + ".bak", true);
                }

                File.Copy(objparamadw.AssetPath, objparamadw.AssetPath + ".prev", true);
            }
            File.Delete(objparamadw.AssetPath);
        }

        File.Move(objparamadw.AssetPath + ".temp", objparamadw.AssetPath);
    }

    private DCX.Type GetCompressionType()
    {
        if (View.Project.Descriptor.ProjectType == ProjectType.DS3)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }

        if (View.Project.Descriptor.ProjectType == ProjectType.ER)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }

        if (View.Project.Descriptor.ProjectType == ProjectType.NR)
        {
            return DCX.Type.DCX_DFLT_11000_44_9_15;
        }

        if (View.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            return DCX.Type.DCX_KRAK_MAX;
        }
        else if (View.Project.Descriptor.ProjectType == ProjectType.DS2S || View.Project.Descriptor.ProjectType == ProjectType.DS2)
        {
            return DCX.Type.None;
        }
        else if (View.Project.Descriptor.ProjectType == ProjectType.SDT)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (View.Project.Descriptor.ProjectType == ProjectType.BB)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (View.Project.Descriptor.ProjectType == ProjectType.DES)
        {
            return DCX.Type.None;
        }

        return DCX.Type.None;
    }

    /// <summary>
    ///     Save BTL light data
    /// </summary>
    public void SaveBTL(MapEditorView view, MapContainer map)
    {
        var fileEntries = View.Project.Locator.LightFiles.Entries;

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            fileEntries = View.Project.Locator.DS2_LightFiles.Entries;
                
        foreach (var entry in fileEntries)
        {
            if (!entry.Filename.Contains(map.Name))
                continue;

            if (View.Project.Descriptor.ProjectType == ProjectType.DS2S || View.Project.Descriptor.ProjectType == ProjectType.DS2)
            {
                var bhdPath = entry.Path;
                var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

                try
                {
                    var bdtFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bdtPath);
                    var bhdFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bhdPath);

                    using var bdt = BXF4.Read(bhdFile, bdtFile);
                    BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));

                    if (file != null)
                    {
                        var btl = BTL.Read(file.Bytes);

                        if (btl != null)
                        {
                            List<BTL.Light> newLights = map.SerializeBtlLights(file.Name);

                            // Only save BTL if it has been modified
                            if (JsonSerializer.Serialize(btl.Lights, BtlLightSerializerContext.Default.ListLight) !=
                                JsonSerializer.Serialize(newLights, BtlLightSerializerContext.Default.ListLight))
                            {
                                btl.Lights = newLights;
                                file.Bytes = btl.Write();
                            }

                            Project.VFS.ProjectFS.WriteFile(bhdPath, bhdFile.ToArray());
                            Project.VFS.ProjectFS.WriteFile(bdtPath, bdtFile.ToArray());
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Map Editor] Failed to load BTL file.", LogLevel.Error, LogPriority.High, e);
                }
            }
            else
            {
                var btlFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(entry.Path);

                var btl = BTL.Read(btlFile);

                if (btl != null)
                {
                    List<BTL.Light> newLights = map.SerializeBtlLights(entry.Filename);

                    // Only save BTL if it has been modified
                    if (JsonSerializer.Serialize(btl.Lights, BtlLightSerializerContext.Default.ListLight) !=
                        JsonSerializer.Serialize(newLights, BtlLightSerializerContext.Default.ListLight))
                    {
                        btl.Lights = newLights;
                        btlFile = btl.Write();
                    }
                }

                Project.VFS.ProjectFS.WriteFile(entry.Path, btlFile.ToArray());
            }
        }
    }


    public void SaveAllMaps(bool autoSave = false)
    {
        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                SaveMap(entry.Value.MapContainer, autoSave);
            }
        }
    }

    public void UnloadAllMaps()
    {
        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                UnloadMap(entry.Key.Filename);
            }
        }
    }

    public void UnloadAll(bool clearFromList = false)
    {
        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                UnloadMap(entry.Key.Filename);
            }
        }
    }

    public void ScheduleTextureRefresh()
    {
        ResourceManager.SchedulePostTextureRefresh();
    }

    public void ScheduleWorldMapRefresh()
    {
        ResourceManager.ScheduleWorldMapRefresh();
    }
}

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(List<BTL.Light>))]
internal partial class BtlLightSerializerContext : JsonSerializerContext
{
}
