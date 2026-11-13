using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Concurrent;
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

    // A count, not a flag: "Load Related Maps" runs several loads at once, and the first to finish
    // must not report that loading is over
    private int _activeLoads;

    public bool IsLoading => Volatile.Read(ref _activeLoads) > 0;

    // The maps being loaded right now
    private readonly ConcurrentDictionary<string, byte> _loadingMaps = new();

    // Loads requested while another was running, started in order once it finishes (UI thread only)
    private readonly List<(string MapID, bool SelectOnLoad)> _loadQueue = new();

    public bool IsMapLoading(string mapID) => mapID != null && _loadingMaps.ContainsKey(mapID);

    public bool IsMapQueued(string mapID) => _loadQueue.Any(e => e.MapID == mapID);

    public int QueuedLoadCount => _loadQueue.Count;

    /// <summary>
    /// The maps being loaded right now, for display
    /// </summary>
    public IReadOnlyCollection<string> LoadingMaps => _loadingMaps.Keys.ToList();

    /// <summary>
    /// True while a load is running or waiting in the queue
    /// </summary>
    public bool HasPendingLoads => IsLoading || _loadQueue.Count > 0;

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
            // Queued rather than refused, and started when the running load finishes
            if (!IsMapLoading(mapid) && !IsMapQueued(mapid) && View.Selection.GetMapContainerFromMapID(mapid) == null)
            {
                _loadQueue.Add((mapid, selectOnLoad));
                Smithbox.Log(this, LOC.Get("MAP_Universe_Log_Map_Load_Queued", mapid));
            }

            return true;
        }

        // Checked here as well as in LoadMapAsync, so the collision below isn't loaded again for a map
        // that is already loaded or loading
        if (IsMapLoading(mapid))
        {
            return true;
        }

        if (View.Selection.GetMapContainerFromMapID(mapid) is { LoadState: MapContentLoadState.Loaded })
        {
            Smithbox.LogError(this, LOC.Get("MAP_Universe_Log_Map_Already_Loaded", mapid));
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

        // Only a load that will select its map replaces the selection; any other (a queued load, each map of
        // Load Related Maps) leaves what the user has selected alone
        if (selectOnLoad)
        {
            View.ViewportSelection.ClearSelection();
        }

        // Load collisions before async map loading
        Profiler.TracyFiberEnter("loadMapCollision");

        if (!ignoreHavokLoad)
        {
            if (!CFG.Current.MapEditor_SkipHavokLoad)
            {
                using var __havokZone = Profiler.TracyZoneAuto("HavokZone");

                View.HavokCollisionBank.OnLoadMap(mapid);
                View.HavokNavmeshBank.OnLoadMap(mapid);
                View.FirstPersonCollision?.OnMapChanged(mapid);

                __havokZone.Dispose();
            }
        }

        Profiler.TracyFiberLeave();

        _ = LoadMapAsync(mapid, selectOnLoad);

        return true;
    }

    public async Task<bool> LoadMapAsync(string mapid, bool selectOnLoad = false)
    {
        var fileEntry = View.Selection.GetFileEntryFromMapID(mapid);
        var existingMap = View.Selection.GetMapContainerFromMapID(mapid);

        if (existingMap != null &&
            existingMap.LoadState is MapContentLoadState.Loaded)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Universe_Log_Map_Already_Loaded", mapid));
            return false;
        }

        // A load of this map is already running
        if (!_loadingMaps.TryAdd(mapid, 0))
        {
            return false;
        }

        // Before the first await, so the load counts as running as soon as LoadMap returns
        Interlocked.Increment(ref _activeLoads);

        Profiler.TracyFiberEnter("loadMapAsync");
        using var __loadMapZone = Profiler.TracyZoneAuto();

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
                View.MultiPlayerWallBank.LoadMPW(newMap);
                View.FogVolumeBank.LoadFVB(newMap);
                View.IrradianceVolumeBank.LoadMap(newMap);
                View.MapLimitBank.LoadMSLD(newMap);
                View.LightAtlasBank.LoadBTAB(newMap);
                View.LightProbeBank.LoadBTPB(newMap);
                View.AssetConfigurationBank.LoadACB(newMap);

                __lightScope.Dispose();

                using var __navScope = Profiler.TracyZoneAuto();
                View.HavokNavmeshBank.LoadNVA(newMap, resourceHandler);
                View.HavokNavmeshBank.LoadNVVA(newMap);
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
                    // Select the map as soon as its RootObject is available, rather than at the end of all jobs.
                    // This runs on a thread pool thread, and the selection is read by the UI every frame, so it is
                    // handed to the UI thread (ApplyPendingSelections) instead of being changed here.
                    if (selectOnLoad)
                    {
                        _pendingSelections.Enqueue((newMap, SelectRoot: true));
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

                // Only the last of the loads running at once says loading is over
                if (Volatile.Read(ref _activeLoads) <= 1)
                {
                    HasProcessedMapLoad = true;
                }

                if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.Viewport_Enable_Rendering)
                {
                    // Update models (For checking meshes for Model Markers. & updates `CollisionName` field reference info)
                    foreach (Entity obj in newMap.Objects)
                    {
                        obj.UpdateRenderModel();
                    }

                    View.MapAnimationTool.OnMapLoaded(newMap);
                }

                newMap.LightAtlasResolver.BuildReferenceMaps();

                // Check for duplicate EntityIDs
                CheckDupeEntityIDs(newMap);

                if (selectOnLoad)
                {
                    // Made the active map on the UI thread too
                    _pendingSelections.Enqueue((newMap, SelectRoot: false));
                }

                ScheduleTextureRefresh();
                ResourceManager.ScheduleCompassRefresh();
            }
            else
            {
                // Nothing was published, so the map stays unloaded; say why, and drop what was set up for it
                Smithbox.LogError(this, LOC.Get("MAP_Universe_Map_Load_No_MSB", mapid));
                View.ModelInsightTool.ClearEntry(newMap);
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
            // Loads can fail at once, so the first is kept rather than replaced (each is logged above).
            Interlocked.CompareExchange(ref LoadMapExceptions, ExceptionDispatchInfo.Capture(e), null);
        }
        finally
        {
            __loadMapZone.Dispose();
            Profiler.TracyFiberLeave();

            ResourceLoadTracker.EndSession(mapid);

            _loadingMaps.TryRemove(mapid, out _);

            // A failed or empty load must not leave the editor waiting for a load that is over
            if (Interlocked.Decrement(ref _activeLoads) == 0)
            {
                HasProcessedMapLoad = true;
            }
        }

        return true;
    }

    // Selections asked for by loads, applied on the UI thread: the root to select, or the map to make active
    private readonly ConcurrentQueue<(MapContainer Map, bool SelectRoot)> _pendingSelections = new();

    private void ApplyPendingSelections()
    {
        while (_pendingSelections.TryDequeue(out var pending))
        {
            // Unloaded again before this frame
            if (pending.Map.LoadState is not MapContentLoadState.Loaded || View.Selection.GetMapContainerFromMapID(pending.Map.Name) != pending.Map)
                continue;

            if (pending.SelectRoot)
            {
                View.ViewportSelection.ClearSelection();
                View.ViewportSelection.AddSelection(pending.Map.RootObject);
            }
            else
            {
                View.Selection.SelectedMapID = pending.Map.Name;
                View.Selection.SelectedMapContainer = pending.Map;
            }
        }
    }

    /// <summary>
    /// Starts the next queued load once nothing is loading, and applies the selections finished loads asked
    /// for. Called every frame, on the UI thread.
    /// </summary>
    public void ProcessLoadQueue()
    {
        ApplyPendingSelections();

        if (IsLoading || _loadQueue.Count == 0)
            return;

        var (mapID, selectOnLoad) = _loadQueue[0];
        _loadQueue.RemoveAt(0);

        // It may have been loaded some other way while it waited
        if (View.Selection.GetMapContainerFromMapID(mapID) != null)
            return;

        LoadMap(mapID, selectOnLoad);
    }

    /// <summary>
    /// Before maps are unloaded (or their view closed): if any have unsaved changes, asks whether to save them
    /// (Yes), discard them (No) or keep the maps loaded (Cancel). Returns true if the unload should go ahead,
    /// which after Yes is only if every save succeeded.
    /// </summary>
    public bool ConfirmUnload(IEnumerable<MapContainer> maps, string dialogKey = "MAP_Universe_Dialog_Unload_Unsaved")
    {
        var unsaved = maps.Where(m => m != null && m.HasUnsavedChanges).ToList();

        if (unsaved.Count == 0)
            return true;

        DialogResult result = PlatformUtils.Instance.MessageBox(
            LOC.Get(dialogKey, string.Join(", ", unsaved.Select(m => m.Name))),
            LOC.Get("MAP_Options_Dialog_Confirm"),
            MessageBoxButtons.YesNoCancel);

        if (result == DialogResult.No)
            return true;

        if (result != DialogResult.Yes)
            return false;

        foreach (var map in unsaved)
        {
            try
            {
                // Saved by the view that owns it, as its banks hold its collision and navmesh data
                (map.OwnerView?.Universe ?? this).SaveMap(map);
            }
            catch (SavingFailedException e)
            {
                View.Editor.HandleSaveException(e);
                return false;
            }

            // A save that failed without throwing (exceptions ignored, or a non-MSB file failed) leaves it dirty
            if (map.HasUnsavedChanges)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Universe_Unload_Save_Incomplete", map.Name));
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Unloads the map, offering to save it first if it has unsaved changes
    /// </summary>
    public void RequestUnloadMap(string mapID)
    {
        if (!ConfirmUnload(new[] { View.Selection.GetMapContainerFromMapID(mapID) }))
            return;

        UnloadMap(mapID);
    }

    /// <summary>
    /// Unloads every loaded map, offering to save those with unsaved changes first
    /// </summary>
    public void RequestUnloadAllMaps()
    {
        var loaded = Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null)
            .ToList();

        if (loaded.Count == 0)
            return;

        if (loaded.Any(m => m.HasUnsavedChanges))
        {
            if (!ConfirmUnload(loaded))
                return;
        }
        else
        {
            // Nothing to lose, but reloading many maps takes a while
            DialogResult result = PlatformUtils.Instance.MessageBox(
                LOC.Get("MAP_Options_Dialog_Unload_All"),
                LOC.Get("MAP_Options_Dialog_Confirm"),
                MessageBoxButtons.YesNo);

            if (result != DialogResult.Yes)
                return;
        }

        UnloadAllMaps();
    }

    public void UnloadMap(string mapID, bool clearFromList = false, bool collectGarbage = true)
    {
        // The map's selection, undo history and havok data live in the view that loaded it
        var ownerView = View.Selection.GetMapContainerFromMapID(mapID)?.OwnerView;
        if (ownerView != null && ownerView != View)
        {
            ownerView.Universe.UnloadMap(mapID, clearFromList, collectGarbage);
            return;
        }

        // A queued load of it is no longer wanted
        _loadQueue.RemoveAll(e => e.MapID == mapID);

        var unloading = View.Selection.GetMapContainerFromMapID(mapID);

        // Only this map's objects leave the selection, and only the history that touched it is dropped,
        // so work on the other loaded maps can still be undone
        if (unloading != null)
        {
            foreach (var sel in View.ViewportSelection.GetSelection().OfType<Entity>().Where(e => e.Container == unloading).ToList())
            {
                View.ViewportSelection.RemoveSelection(sel);
            }
        }

        // Said in the log, as the steps can't be got back (the History tool shows the count before unloading)
        var droppedSteps = View.ViewportActionManager.CountActionsTouching(mapID);

        View.ViewportActionManager.RemoveActionsTouching(mapID);

        if (droppedSteps > 0)
        {
            Smithbox.Log(this, LOC.Get("MAP_History_Unload_Dropped", mapID, droppedSteps));
        }

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
                View.FirstPersonCollision?.OnMapChanged(curMapID);
                View.WallBank.OnUnloadMap(curMapID);
                View.HavokNavmeshBank.OnUnloadMap(curMapID);
                View.NavigationBank.OnUnloadMap(curMapID, Project.Handler.MapData.PrimaryBank.Maps.Values
                    .Where(w => w.MapContainer != null).Select(w => w.MapContainer.Name));
                View.NvmBank.OnUnloadMap(curMapID);
                View.IrradianceVolumeBank.OnUnloadMap(curMapID);

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
                    View.EditorVisibilityAction.OnMapUnloaded(wrapper.MapContainer);
                    View.LowDetailVisibilityAction.OnMapUnloaded(wrapper.MapContainer);

                    wrapper.MapContainer.LoadState = MapContentLoadState.Unloaded;
                    wrapper.MapContainer.Unload();
                    wrapper.MapContainer.Clear();
                    wrapper.MapContainer = null;
                }
            }
        }

        if (collectGarbage)
        {
            CollectGarbage();
        }
    }

    private static void CollectGarbage()
    {
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

    private BTL LoadBTL(FileDictionaryEntry curEntry) => ReadBTL(View.Project, curEntry);

    /// <summary>
    /// Reads a map's BTL file (in DS2, the light.btl in its gibhd binder). Null if it can't be read.
    /// </summary>
    public static BTL ReadBTL(ProjectEntry project, FileDictionaryEntry curEntry)
    {
        BTL btl = null;

        var fs = project.Handler.MapData.PrimaryBank.TargetFS;

        if (project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var bhdPath = curEntry.Path;
            var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

            try
            {
                if (fs.FileExists(bdtPath) && fs.FileExists(bhdPath))
                {
                    var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
                    var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

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
                                Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Read_BTL", file.Name), e);
                            }
                        }
                        else
                        {
                            Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Find_GI_Light_BTL_File", bhdPath));
                        }
                    }
                    else
                    {
                        Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Read_GI_Binder", bhdPath));
                    }
                }
                else
                {
                    Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Find_GI_File", bhdPath));
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Read_GI", bhdPath), e);
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
                        Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Read_BTL", curEntry.Path), e);
                    }
                }
                else
                {
                    Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Read_GI", curEntry.Path));
                }
            }
            else
            {
                Smithbox.LogError<MapUniverse>(LOC.Get("MAP_Data_Failed_Find_GI_File", curEntry.Path));
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
    /// Save the map file if it should be saved, clearing its modified state if the save succeeded.
    /// True if the file had edits and they were saved.
    /// </summary>
    private bool SaveFile(MapContainer map, MapFileType fileType, Func<bool> save)
    {
        if (!ShouldSaveFile(map, fileType))
            return false;

        var wasDirty = map.IsDirty(fileType);

        if (save())
        {
            map.ClearDirty(fileType);
            return wasDirty;
        }

        return false;
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
        SaveFile(map, MapFileType.MPW, () => View.MultiPlayerWallBank.SaveMPW(map));
        SaveFile(map, MapFileType.FVB, () => View.FogVolumeBank.SaveFVB(map));
        SaveFile(map, MapFileType.MSLD, () => View.MapLimitBank.SaveMSLD(map));
        SaveFile(map, MapFileType.BTAB, () => View.LightAtlasBank.SaveBTAB(map));
        SaveFile(map, MapFileType.BTPB, () => View.LightProbeBank.SaveBTPB(map));
        SaveFile(map, MapFileType.ACB, () => View.AssetConfigurationBank.SaveACB(map));
        // Navigation and collision files saved with edits, each reported once the map's save is done
        var navSaved = false;
        var collisionSaved = false;

        navSaved |= SaveFile(map, MapFileType.NVA, () => View.HavokNavmeshBank.SaveNVA(map));
        navSaved |= SaveFile(map, MapFileType.NVVA, () => View.HavokNavmeshBank.SaveNVVA(map));

        // Only these projects are supported for collision editing within the Map Editor
        if (Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            collisionSaved |= SaveFile(map, MapFileType.Collision, () => View.HavokCollisionBank.SaveMapCollisionFiles(map.Name));
            navSaved |= SaveFile(map, MapFileType.Navmesh, () => View.HavokNavmeshBank.SaveHavokNavmeshModels(map.Name));
        }

        // The NVC and ONAV files are only read for ER, by the navigation bank
        if (Project.Descriptor.ProjectType is ProjectType.ER)
        {
            navSaved |= SaveFile(map, MapFileType.NVC, () => View.NavigationBank.SaveNVC(map));
            navSaved |= SaveFile(map, MapFileType.ONAV, () => View.NavigationBank.SaveONAV(map));
        }

        // DS3, SDT, BB and the hkp games' collision is edited by the Collision Editor (their navmesh isn't edited
        // here). BB and the hkp games only write the files it changed.
        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.BB
            or ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2S)
        {
            collisionSaved |= SaveFile(map, MapFileType.Collision, () => View.HavokCollisionBank.SaveMapCollisionFiles(map.Name));
        }

        // The Navigation tool edits DS3, SDT and BB navmeshes (Phase N5) in the same nvmhktbnd binders as ER's
        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.BB)
        {
            navSaved |= SaveFile(map, MapFileType.Navmesh, () => View.HavokNavmeshBank.SaveHavokNavmeshModels(map.Name));
        }

        // DeS, DS1 and DS1R navigation (NVM binder, loose DeS NVMs, MCP, MCG), edited by the Navigation tool's NVM mode
        if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            navSaved |= SaveFile(map, MapFileType.Navmesh, () => View.NvmBank.SaveMap(map));
        }

        // The WALL files belong to the collisions
        if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            collisionSaved |= SaveFile(map, MapFileType.Wall, () => View.WallBank.SaveWalls(map.Name));
        }

        if (navSaved)
        {
            Smithbox.Log(this, LOC.Get("MAP_Universe_Save_Navmesh_PASS", map.Name));
        }

        if (collisionSaved)
        {
            Smithbox.Log(this, LOC.Get("MAP_Universe_Save_Collision_PASS", map.Name));
        }

        if (ShouldSaveFile(map, MapFileType.MSB))
        {
            SaveMSB(map);

            // Maps loaded after this place the saved contents of a prefab
            Project.Handler.MapData.PrefabAssetBank?.Invalidate(map.Name);
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

            // DS2's generators, events and object instances are serialized before anything is written, so a
            // failure (a duplicate ID) leaves the map unsaved and still marked as modified, rather than writing
            // the MSB without its params
            List<(Param Param, string Path)> ds2Params = null;

            if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2 && map.DS2GeneratorsLoaded)
            {
                ds2Params = SerializeDS2Generators(map)
                    ?? throw new InvalidOperationException(LOC.Get("MAP_Universe_DS2_Generator_Save_Failed", map.Name));
            }

            var msb = adapter.CreateForWrite(mapData);
            map.SerializeToMSB(msb, Project.Descriptor.ProjectType);

            var newMapData = msb.Write(adapter.Compression);
            Project.VFS.ProjectFS.WriteFile(curEntry.Path, newMapData);

            if (ds2Params != null)
            {
                WriteDS2Generators(ds2Params);
            }

            CheckDupeEntityIDs(map);

            map.ClearDirty(MapFileType.MSB);
            map.SaveError = null;
            Smithbox.Log(this, LOC.Get("MAP_Universe_Save_Map_PASS", curEntry.Filename));
        }
        catch (Exception e)
        {
            // Kept until the next successful save, so the Map List can show which map failed and why
            map.SaveError = e.Message;

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

        // The six per-map params are all written back on save, so without any one of them the generators are not
        // loaded at all (and SaveDS2Generators leaves the files alone) rather than saved with a param emptied
        var paramNames = new[] { "generatorregistparam", "generatorlocation", "generatorparam",
            "eventparam", "eventlocation", "mapobjectinstanceparam" };

        var missing = paramNames.Where(p => !Project.Handler.ParamData.PrimaryBank.Params.ContainsKey($"{p}_{mapid}")).ToList();

        if (missing.Count > 0)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Universe_DS2_Generator_Params_Missing", mapid, string.Join(", ", missing.Select(p => $"{p}_{mapid}"))));
            map.DS2GeneratorsLoaded = false;
            return;
        }

        map.DS2GeneratorsLoaded = true;

        // Duplicate row IDs are kept as objects of their own, so nothing is lost; the save reports them
        // (SerializeDS2*) and refuses to write until they are made unique
        var duplicateIds = new List<string>();

        Param regparam = Project.Handler.ParamData.PrimaryBank.Params[$"generatorregistparam_{mapid}"];
        foreach (Param.Row row in regparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "regist_" + row.ID;
            }

            if (!registParams.TryAdd(row.ID, row))
            {
                duplicateIds.Add($"generatorregistparam {row.ID}");
            }

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

            MsbEntity obj = new(View.Universe, map, mergedRow, MsbEntityType.DS2Generator);

            // A duplicate location gets its own object, but the generator rows of that ID join the first one
            if (generatorParams.TryAdd(row.ID, mergedRow))
            {
                generatorObjs.Add(row.ID, obj);
            }
            else
            {
                duplicateIds.Add($"generatorlocation {row.ID}");
            }

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

            if (generatorParams.TryGetValue(row.ID, out var existing) && existing.GetRow("generator") == null)
            {
                existing.AddRow("generator", row);
            }
            else
            {
                if (existing != null)
                {
                    duplicateIds.Add($"generatorparam {row.ID}");
                }

                MergedParamRow mergedRow = new("GENERATOR_MERGED_PARAM");
                mergedRow.AddRow("generator", row);
                MsbEntity obj = new(View.Universe, map, mergedRow, MsbEntityType.DS2Generator);

                if (existing == null)
                {
                    generatorParams.Add(row.ID, mergedRow);
                    generatorObjs.Add(row.ID, obj);
                }

                map.AddObject(obj);

                // Only the first generator of an ID gets the character model below
                if (existing != null)
                    continue;
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

        if (duplicateIds.Count > 0)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Universe_DS2_Generator_Duplicate_IDs", mapid, string.Join(", ", duplicateIds)));
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

    /// <summary>
    /// Builds the map's six DS2 per-map params from its objects, ready to write. Null if any of them failed to
    /// serialize (the reason is logged), in which case nothing should be written.
    /// </summary>
    private List<(Param Param, string Path)> SerializeDS2Generators(MapContainer map)
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

        // Serialize objects; every step runs, so all the ID collisions are logged at once
        var ok = map.SerializeDS2Generators(locparam, genparam);
        ok &= map.SerializeDS2Regist(regparam);
        ok &= map.SerializeDS2Events(evtparam);
        ok &= map.SerializeDS2EventLocations(evtlparam);
        ok &= map.SerializeDS2ObjInstances(objparam);

        if (!ok)
        {
            return null;
        }

        return new List<(Param, string)>
        {
            (regparam, regparamPath),
            (locparam, locparamPath),
            (genparam, genparamPath),
            (evtparam, evtparamPath),
            (evtlparam, evtlparamPath),
            (objparam, objparamPath)
        };
    }

    private static void WriteDS2Generators(List<(Param Param, string Path)> ds2Params)
    {
        foreach (var (param, path) in ds2Params)
        {
            // Create the param directory if it does not exist
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            WriteDS2Param(param, path);
        }
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
        _loadQueue.Clear();

        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                // One collection at the end rather than two blocking ones per map
                UnloadMap(entry.Key.Filename, collectGarbage: false);
            }
        }

        CollectGarbage();
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
