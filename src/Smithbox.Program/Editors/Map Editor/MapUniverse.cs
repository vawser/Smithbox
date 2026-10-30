using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tracy;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A universe is a collection of loaded maps with methods to load, serialize,
/// and unload individual maps.
/// </summary>
public class MapUniverse : IUniverse
{
    public MapEditorView View;
    public ProjectEntry Project;

    public ExceptionDispatchInfo LoadMapExceptions = null;

    public volatile bool HasProcessedMapLoad;
    public volatile bool IsLoading = false;

    public string ModelDataMapID { get; set; }


    public MapUniverse(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public RenderScene GetCurrentScene()
    {
        return View.ViewportHandler.ActiveViewport.RenderScene;
    }

    public bool LoadMap(string mapid, bool selectOnLoad = false, bool ignoreHavokLoad = false, bool allowConcurrentMapLoad = false)
    {
        if(!allowConcurrentMapLoad && IsLoading)
        {
            Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Universe_Log_Map_Already_Loading"));
            return false;
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            if (Project.Handler.ParamEditor == null)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Universe_Log_DS2_Params_Not_Ready_for_Map"));
                return false;
            }
        }

        ResourceManager.ClearUnusedResources();

        View.ViewportSelection.ClearSelection();

        // Load collisions before async map loading
        Profiler.TracyFiberEnter("loadMapCollision");

        if (!ignoreHavokLoad)
        {
            if (!CFG.Current.MapEditor_SkipHavokLoad)
            {
                using var __havokZone = Profiler.TracyZoneAuto("HavokZone");

                View.HavokCollisionBank.OnLoadMap(mapid);
                View.HavokNavmeshBank.OnLoadMap(mapid);

                __havokZone.Dispose();
            }
        }

        Profiler.TracyFiberLeave();

        _ = LoadMapAsync(mapid, selectOnLoad);

        return true;
    }

    public async Task<bool> LoadMapAsync(string mapid, bool selectOnLoad = false)
    {
        IsLoading = true;

        Profiler.TracyFiberEnter("loadMapAsync");
        using var __loadMapZone = Profiler.TracyZoneAuto();

        var fileEntry = View.Selection.GetFileEntryFromMapID(mapid);
        var existingMap = View.Selection.GetMapContainerFromMapID(mapid);

        if (existingMap != null && 
            existingMap.LoadState is MapContentLoadState.Loaded)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Universe_Log_Map_Already_Loaded", mapid));

            IsLoading = false;
            return false;
        }

        MapContainer newMap = null;

        ResourceLoadTracker.BeginSession(mapid);

        try
        {
            HasProcessedMapLoad = false;

            newMap = new MapContainer(View, mapid);

            ModelDataMapID = newMap.Name;

            View.ModelInsightTool.AddEntry(newMap);
            View.DisplayGroupTool.SetupDrawgroupCount();

            MapResourceHandler resourceHandler = new MapResourceHandler(View, mapid);

            Profiler.TracyFiberLeave();
            await resourceHandler.ReadMap(mapid);
            Profiler.TracyFiberEnter("loadMapAsync");

            if (resourceHandler.Msb != null)
            {
                newMap.LoadMSB(resourceHandler.Msb);

                if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan || CFG.Current.Viewport_Enable_Rendering)
                {
                    resourceHandler.SetupHumanEnemySubstitute();

                    using var __resScope = Profiler.TracyZoneAuto();
                    resourceHandler.SetupModelLoadLists();
                    __resScope.Dispose();

                    using var __texScope = Profiler.TracyZoneAuto();
                    resourceHandler.SetupTextureLoadLists();
                    __texScope.Dispose();

                    if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
                    {
                        resourceHandler.ReserveLoadCategories();
                    }
                }

                using var __lightScope = Profiler.TracyZoneAuto();
                LoadLights(newMap);

                View.AutoInvadeBank.LoadAIP(newMap);
                View.LightAtlasBank.LoadBTAB(newMap);
                View.LightProbeBank.LoadBTPB(newMap);
                View.AssetConfigurationBank.LoadACB(newMap);

                __lightScope.Dispose();

                using var __navScope = Profiler.TracyZoneAuto();
                View.HavokNavmeshBank.LoadNVA(newMap, resourceHandler);
                __navScope.Dispose();

                if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
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

                    if (View.Project.Descriptor.ProjectType is ProjectType.DB_NT)
                    {
                        // TODO: Duskbloods
                    }

                    if (View.Project.Descriptor.ProjectType is ProjectType.DB)
                    {
                        // TODO: Duskbloods
                    }
                }

                // This method continues on a thread pool thread after its first await, so build
                // every object of the map before publishing it, where the UI thread can iterate it
                if (View.Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    LoadDS2Generators(resourceHandler.AdjustedMapID, newMap);
                }

                Project.Handler.MapData.PrimaryBank.Maps[fileEntry].MapContainer = newMap;
                newMap.LoadState = MapContentLoadState.Loaded;

                if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
                {
                    // Intervene in the UI to change selection if requested.
                    // We want to do this as soon as the RootObject is available, rather than at the end of all jobs.
                    if (selectOnLoad)
                    {
                        View.ViewportSelection.ClearSelection();
                        View.ViewportSelection.AddSelection(newMap.RootObject);
                    }
                }

                var tasks = new List<Task>();

                if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
                {
                    tasks = resourceHandler.LoadModels(tasks, newMap);

                    Profiler.TracyFiberLeave();
                    await Task.WhenAll(tasks);
                    Profiler.TracyFiberEnter("loadMapAsync");

                    if (CFG.Current.Viewport_Enable_Texturing)
                    {
                        tasks = resourceHandler.LoadTextures(tasks, newMap);

                        Profiler.TracyFiberLeave();
                        await Task.WhenAll(tasks);
                        Profiler.TracyFiberEnter("loadMapAsync");
                    }
                }

                // After everything loads, do some additional checks:
                Profiler.TracyFiberLeave();
                await Task.WhenAll(tasks);
                Profiler.TracyFiberEnter("loadMapAsync");
                HasProcessedMapLoad = true;

                if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
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

                if (selectOnLoad)
                {
                    View.Selection.SelectedMapID = mapid;
                    View.Selection.SelectedMapContainer = newMap;
                }

                ScheduleTextureRefresh();
                ResourceManager.ScheduleCompassRefresh();
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Universe_Map_Load_Failed"), e);

            // A map that never got published should not linger in the model insight tool
            if (newMap != null && fileEntry != null &&
                Project.Handler.MapData.PrimaryBank.Maps[fileEntry].MapContainer != newMap)
            {
                View.ModelInsightTool.ClearEntry(newMap);
            }

            // This task is fire-and-forget, so a rethrow here would be lost (even in debug builds).
            // Store the exception instead; it is rethrown on the main thread in MapEditorScreen.Update.
            LoadMapExceptions = ExceptionDispatchInfo.Capture(e);
        }
        finally
        {
            __loadMapZone.Dispose();
            Profiler.TracyFiberLeave();

            ResourceLoadTracker.EndSession(mapid);
            IsLoading = false;
        }

        return true;
    }

    /// <summary>
    /// Asks the user to confirm discarding unsaved changes, if the map has any
    /// </summary>
    public bool ConfirmDiscardChanges(MapContainer map)
    {
        if (map == null || !map.HasUnsavedChanges)
            return true;

        DialogResult result = PlatformUtils.Instance.MessageBox(
            LOC.Get("MAP_Universe_Dialog_Unload_Unsaved", map.Name),
            LOC.Get("MAP_Options_Dialog_Confirm"),
            MessageBoxButtons.YesNo);

        return result == DialogResult.Yes;
    }

    /// <summary>
    /// Unloads the map, asking first if it has unsaved changes
    /// </summary>
    public void RequestUnloadMap(string mapID)
    {
        if (!ConfirmDiscardChanges(View.Selection.GetMapContainerFromMapID(mapID)))
            return;

        UnloadMap(mapID);
    }

    public void UnloadMap(string mapID, bool clearFromList = false)
    {
        // The map's selection, undo history and havok data live in the view that loaded it
        var ownerView = View.Selection.GetMapContainerFromMapID(mapID)?.OwnerView;
        if (ownerView != null && ownerView != View)
        {
            ownerView.Universe.UnloadMap(mapID, clearFromList);
            return;
        }

        View.ViewportSelection.ClearSelection();
        View.ViewportActionManager.Clear();

        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            var curMapID = entry.Key.Filename;

            if (curMapID == mapID)
            {
                var wrapper = entry.Value;

                ResourceManager.ClearUnusedResources();

                if (wrapper != null)
                {
                    View.ModelInsightTool.ClearEntry(wrapper.MapContainer);

                    View.EntityTypeCache.RemoveMapFromCache(wrapper.MapContainer);
                }

                View.HavokCollisionBank.OnUnloadMap(curMapID);
                View.HavokNavmeshBank.OnUnloadMap(curMapID);

                if (wrapper != null)
                {
                    if (View.Selection.SelectedMapContainer == wrapper.MapContainer)
                    {
                        View.Selection.SelectedMapID = "";
                        View.Selection.SelectedMapContainer = null;
                    }
                }

                View.MapContentView.OnMapUnloaded();

                if (wrapper?.MapContainer != null)
                {
                    wrapper.MapContainer.LoadState = MapContentLoadState.Unloaded;
                    wrapper.MapContainer.Unload();
                    wrapper.MapContainer.Clear();
                    wrapper.MapContainer = null;
                }
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    public void LoadLights(MapContainer map)
    {
        if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            foreach (var entry in View.Project.Locator.DS2_LightFiles.Entries)
            {
                var worldBlock = map.Name.Substring(1);

                if (entry.Filename.Contains(worldBlock))
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

        var fs = View.Project.Handler.MapData.PrimaryBank.TargetFS;

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var bhdPath = curEntry.Path;
            var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

            try
            {
                if (fs.FileExists(bdtPath) && fs.FileExists(bhdPath))
                {
                    var bdtFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bdtPath);
                    var bhdFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bhdPath);

                    using var bdt = BXF4.Read(bhdFile, bdtFile);

                    if (bdt != null)
                    {
                        BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));

                        if (file != null)
                        {
                            try
                            {
                                btl = BTL.Read(file.Bytes);
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_BTL", file.Name), e);
                            }
                        }
                        else
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_GI_Light_BTL_File", bhdPath));
                        }
                    }
                    else
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_GI_Binder", bhdPath));
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_GI_File", bhdPath));
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_GI", bhdPath), e);
            }
        }
        else
        {
            if (fs.FileExists(curEntry.Path))
            {
                Memory<byte>? btlFile = (Memory<byte>)fs.ReadFile(curEntry.Path);

                if (btlFile.HasValue)
                {
                    try
                    {
                        btl = BTL.Read(btlFile.Value);
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_BTL", curEntry.Path), e);
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_GI", curEntry.Path));
                }
            }
            else
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_GI_File", curEntry.Path));
            }
        }

        return btl;
    }

    /// <summary>
    /// Whether the map file should be saved: it has been modified, or unmodified saves are allowed
    /// </summary>
    private bool ShouldSaveFile(MapContainer map, MapFileType fileType)
    {
        return CFG.Current.MapEditor_Allow_Unmodified_Save || map.IsDirty(fileType);
    }

    /// <summary>
    /// Save the map file if it should be saved, clearing its modified state if the save succeeded
    /// </summary>
    private void SaveFile(MapContainer map, MapFileType fileType, Func<bool> save)
    {
        if (!ShouldSaveFile(map, fileType))
            return;

        if (save())
        {
            map.ClearDirty(fileType);
        }
    }

    /// <summary>
    /// Save the files of the map that have been modified
    /// </summary>
    public void SaveMap(MapContainer map)
    {
        if (!CFG.Current.MapEditor_Allow_Unmodified_Save && map.DirtyFiles is MapFileType.None)
        {
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", map.Name));
            return;
        }

        SaveFile(map, MapFileType.BTL, () => SaveBTL(View, map));
        SaveFile(map, MapFileType.AIP, () => View.AutoInvadeBank.SaveAIP(map));
        SaveFile(map, MapFileType.BTAB, () => View.LightAtlasBank.SaveBTAB(map));
        SaveFile(map, MapFileType.BTPB, () => View.LightProbeBank.SaveBTPB(map));
        SaveFile(map, MapFileType.ACB, () => View.AssetConfigurationBank.SaveACB(map));
        SaveFile(map, MapFileType.NVA, () => View.HavokNavmeshBank.SaveNVA(map));

        // Only these projects are supported for collision editing within the Map Editor
        if (Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            SaveFile(map, MapFileType.Collision, () => View.HavokCollisionBank.SaveMapCollisionFiles(map.Name));
            SaveFile(map, MapFileType.Navmesh, () => View.HavokNavmeshBank.SaveHavokNavmeshModels(map.Name));
        }

        if (ShouldSaveFile(map, MapFileType.MSB))
        {
            SaveMSB(map);
        }
    }

    /// <summary>
    /// Serialize the map into a new MSB and write it to the project.
    /// Every failure is logged here and, unless ignored, thrown as one SavingFailedException
    /// that wraps the original exception (e.g. MissingReferenceException).
    /// </summary>
    private void SaveMSB(MapContainer map)
    {
        try
        {
            var adapter = MsbGameAdapter.For(Project.Descriptor.ProjectType)
                ?? throw new NotSupportedException(LOC.Get("MAP_Universe_Save_Map_Unsupported", Project.Descriptor.ProjectType));

            var curEntry = Project.Locator.MapFiles.Entries.FirstOrDefault(e => e.Filename == map.Name)
                ?? throw new FileNotFoundException(LOC.Get("MAP_Data_Failed_Find_VPS", map.Name));

            var mapData = (Memory<byte>)Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(curEntry.Path);

            var msb = adapter.CreateForWrite(mapData);
            map.SerializeToMSB(msb, Project.Descriptor.ProjectType);

            var newMapData = msb.Write(adapter.Compression);
            Project.VFS.ProjectFS.WriteFile(curEntry.Path, newMapData);

            if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            {
                SaveDS2Generators(map);
            }

            CheckDupeEntityIDs(map);

            map.ClearDirty(MapFileType.MSB);
            Smithbox.Log(this, LOC.Get("MAP_Universe_Save_Map_PASS", curEntry.Filename));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Universe_Save_Map_FAIL", map.Name), e);

            if (!CFG.Current.MapEditor_IgnoreSaveExceptions)
            {
                throw new SavingFailedException(Path.GetFileName(map.Name), e);
            }
        }
    }

    public void LoadDS2Generators(string mapid, MapContainer map)
    {
        Dictionary<long, Param.Row> registParams = new();
        Dictionary<long, MergedParamRow> generatorParams = new();
        Dictionary<long, Entity> generatorObjs = new();

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

            var registid = (int)row.GetCellHandleOrThrow("GeneratorRegistParam").Value;
            if (registParams.ContainsKey(registid))
            {
                Param.Row regist = registParams[registid];
                var chrid = Project.Handler.ParamData.PrimaryBank.GetChrIDForEnemy(
                    (int)regist.GetCellHandleOrThrow("EnemyId").Value);


                if (chrid != null)
                {
                    var chrName = $"c{chrid}";

                    // Queue the character's model and textures so the generator's drawable has something to show
                    var modelAsset = ModelLocator.GetChrModel(Project, chrName, chrName);
                    if (modelAsset.IsValid())
                        chrsToLoad.Add(modelAsset);

                    var textureAsset = TextureLocator.GetCharacterTextureVirtualPath(Project, chrName, false);
                    if (textureAsset.IsValid())
                        chrsToLoad.Add(textureAsset);

                    textureAsset = TextureLocator.GetCharacterTextureVirtualPath(Project, chrName, true);
                    if (textureAsset.IsValid())
                        chrsToLoad.Add(textureAsset);

                    generatorObjs[row.ID].ModelName = chrName;
                    if(generatorObjs[row.ID] is MsbEntity msbEnt)
                    {
                        msbEnt.AssignDrawable();
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

            MsbEntity obj = new(View.Universe, map, row, MsbEntityType.DS2EventLocation);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);

            // Try rendering as a box for now
            DebugPrimitiveRenderableProxy mesh = RenderableHelper.GetBoxRegionProxy(View.ViewportHandler.ActiveViewport.RenderScene);
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

            MsbEntity obj = new(View.Universe, map, row, MsbEntityType.DS2ObjectInstance);
            map.AddObject(obj);
        }

        ResourceManager.DefaultPool.LoadAssets(Project, chrsToLoad, new LoadOptions
        {
            JobName = LOC.Get("MAP_ResourceManager_Character_Models"),
            Types = ResourceType.Flver | ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly
        });
    }

    public void LoadRelatedMapsER(string mapid)
    {
        IReadOnlyDictionary<string, MapConnectionRelationType> relatedMaps =
            MapConnections_ER.GetRelatedMaps(View, mapid);

        foreach (KeyValuePair<string, MapConnectionRelationType> map in relatedMaps)
        {
            View.Universe.LoadMap(map.Key, false, false, true);
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
                            Smithbox.LogError<MapUniverse>(
                                LOC.Get("MAP_Universe_DuplicateCheck_Hit", entityID, obj.PrettyName, name));
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
        var (regparam, regparamPath) = ReadDS2Param(ParamLocator.GetDS2GeneratorRegistParam, map.Name);
        var (locparam, locparamPath) = ReadDS2Param(ParamLocator.GetDS2GeneratorLocationParam, map.Name);
        var (genparam, genparamPath) = ReadDS2Param(ParamLocator.GetDS2GeneratorParam, map.Name);
        var (evtparam, evtparamPath) = ReadDS2Param(ParamLocator.GetDS2EventParam, map.Name);
        var (evtlparam, evtlparamPath) = ReadDS2Param(ParamLocator.GetDS2EventLocationParam, map.Name);
        var (objparam, objparamPath) = ReadDS2Param(ParamLocator.GetDS2ObjInstanceParam, map.Name);

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
        if (!Directory.Exists(Path.GetDirectoryName(regparamPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(regparamPath));
        }

        // Save all the params
        WriteDS2Param(regparam, regparamPath);
        WriteDS2Param(locparam, locparamPath);
        WriteDS2Param(genparam, genparamPath);
        WriteDS2Param(evtparam, evtparamPath);
        WriteDS2Param(evtlparam, evtlparamPath);
        WriteDS2Param(objparam, objparamPath);
    }

    /// <summary>
    /// Reads a DS2 per-map param with its paramdef applied, returning it with the path it should be written to
    /// </summary>
    private (Param Param, string WritePath) ReadDS2Param(Func<ProjectEntry, string, bool, ResourceDescriptor> locate, string mapId)
    {
        ResourceDescriptor readDescriptor = locate(View.Project, mapId, false);
        ResourceDescriptor writeDescriptor = locate(View.Project, mapId, true);

        Param param = Param.Read(readDescriptor.AssetPath);
        PARAMDEF layout = ParamLocator.GetParamdefForParam(View.Project, param.ParamType);
        param.ApplyParamdef(layout);

        return (param, writeDescriptor.AssetPath);
    }

    /// <summary>
    /// Writes a DS2 per-map param via a temp file, keeping .bak (first save) and .prev backups if enabled
    /// </summary>
    private static void WriteDS2Param(Param param, string path)
    {
        var tempPath = path + ".temp";

        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }

        param.Write(tempPath, DCX.Type.None);

        if (File.Exists(path))
        {
            if (CFG.Current.Project_Enable_Backup_Saves)
            {
                if (!File.Exists(path + ".bak"))
                {
                    File.Copy(path, path + ".bak", true);
                }

                File.Copy(path, path + ".prev", true);
            }

            File.Delete(path);
        }

        File.Move(tempPath, path);
    }

    public bool SaveBTL(MapEditorView view, MapContainer map)
    {
        var success = true;

        var fileEntries = View.Project.Locator.LightFiles.Entries;

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            fileEntries = View.Project.Locator.DS2_LightFiles.Entries;

        foreach (var entry in fileEntries)
        {
            if (View.Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            {
                var worldBlock = map.Name.Substring(1);

                if (!entry.Filename.Contains(worldBlock))
                    continue;

                var bhdPath = entry.Path;
                var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

                try
                {
                    var bdtFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bdtPath);
                    var bhdFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(bhdPath);

                    using var packedBinder = BXF4.Read(bhdFile, bdtFile);
                    BinderFile file = packedBinder.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));

                    if (file != null)
                    {
                        var btl = BTL.Read(file.Bytes);

                        if (btl != null)
                        {
                            List<BTL.Light> newLights = map.SerializeBtlLights(file.Name, true);

                            // Only save BTL if it has been modified
                            if (JsonSerializer.Serialize(btl.Lights, BtlLightSerializerContext.Default.ListLight) !=
                                JsonSerializer.Serialize(newLights, BtlLightSerializerContext.Default.ListLight))
                            {
                                btl.Lights = newLights;
                                file.Bytes = btl.Write();

                                var bhdData = new byte[0];
                                var bdtData = new byte[0];

                                packedBinder.Write(out bhdData, out bdtData);

                                Project.VFS.ProjectFS.WriteFile(bhdPath, bhdData);
                                Project.VFS.ProjectFS.WriteFile(bdtPath, bdtData);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Universe_Save_BTL_FAIL", entry.Path), e);
                    success = false;
                }
            }
            else
            {
                if (!entry.Filename.Contains(map.Name))
                    continue;

                try
                {
                    var btlFile = (Memory<byte>)View.Project.Handler.MapData.PrimaryBank.TargetFS.ReadFile(entry.Path);

                    var btl = BTL.Read(btlFile);

                    if (btl != null)
                    {
                        List<BTL.Light> newLights = map.SerializeBtlLights(entry.Filename);

                        // Only save BTL if it has been modified, so unedited BTLs are not added to the project
                        if (JsonSerializer.Serialize(btl.Lights, BtlLightSerializerContext.Default.ListLight) !=
                            JsonSerializer.Serialize(newLights, BtlLightSerializerContext.Default.ListLight))
                        {
                            btl.Lights = newLights;
                            btlFile = btl.Write();

                            Project.VFS.ProjectFS.WriteFile(entry.Path, btlFile.ToArray());
                            Smithbox.Log(this, LOC.Get("MAP_Universe_Save_BTL_PASS", entry.Path));
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Universe_Save_BTL_FAIL", entry.Path), e);
                    success = false;
                }
            }
        }

        return success;
    }


    /// <summary>
    /// Save the modified files of all loaded maps
    /// </summary>
    public void SaveAllMaps()
    {
        // Keep saving the remaining maps if one fails, then report the first failure
        SavingFailedException firstFailure = null;

        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                try
                {
                    SaveMap(entry.Value.MapContainer);
                }
                catch (SavingFailedException e)
                {
                    firstFailure ??= e;
                }
            }
        }

        if (firstFailure != null)
        {
            ExceptionDispatchInfo.Capture(firstFailure).Throw();
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
