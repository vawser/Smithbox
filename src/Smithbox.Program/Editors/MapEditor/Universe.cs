using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
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
    private MapEditorScreen Editor;
    private ProjectEntry Project;

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

    public Universe(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        RenderScene = editor.MapViewportView.RenderScene;
        Selection = editor.ViewportSelection;

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
        if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            if (Project.ParamEditor == null)
            {
                // ParamBank must be loaded for DS2 maps
                TaskLogs.AddLog("Cannot load DS2 maps when params are not loaded.",
                    LogLevel.Warning, StudioCore.Tasks.LogPriority.High);
                return false;
            }
        }

        LoadMapAsync(mapid, selectOnLoad, fastLoad);

        return true;
    }

    /// <summary>
    /// Load a map asynchronously based on the passed map ID
    /// </summary>
    public async void LoadMapAsync(string mapid, bool selectOnLoad = false, bool fastLoad = false)
    {
        var map = Editor.GetMapContainerFromMapID(mapid);

        if (map != null)
        {
            TaskLogs.AddLog($"Map \"{mapid}\" is already loaded",
                LogLevel.Information, StudioCore.Tasks.LogPriority.Normal);
            return;
        }

        var targetMap = Project.MapData.PrimaryBank.Maps.FirstOrDefault(e => e.Key.Filename == mapid);
        targetMap.Value.MapContainer = new MapContainer(Editor, mapid);

        map = targetMap.Value.MapContainer;

        if (!fastLoad)
        {
            Editor.CollisionManager.OnLoadMap(mapid);
        }

        try
        {
            HasProcessedMapLoad = false;

            Editor.DisplayGroupView.SetupDrawgroupCount();

            MapResourceHandler resourceHandler = new MapResourceHandler(Editor, mapid);

            // Read the MSB
            await resourceHandler.ReadMap(mapid);

            // Load the map into the MapContainer
            map.LoadMSB(resourceHandler.Msb);

            if (CFG.Current.Viewport_Enable_Rendering)
            {
                if (Editor.Project.ProjectType != ProjectType.AC4 &&
                    Editor.Project.ProjectType != ProjectType.ACFA &&
                    Editor.Project.ProjectType != ProjectType.ACV &&
                    Editor.Project.ProjectType != ProjectType.ACVD)
                    resourceHandler.SetupHumanEnemySubstitute();

                resourceHandler.SetupModelLoadLists();
                resourceHandler.SetupTexturelLoadLists();
                resourceHandler.SetupModelMasks(map);
            }

            resourceHandler.LoadLights(map);

            if (CFG.Current.Viewport_Enable_Rendering)
            {
                if (Editor.Project.ProjectType == ProjectType.ER && CFG.Current.Viewport_Enable_ER_Auto_Map_Offset)
                {
                    if (SpecialMapConnections.GetEldenMapTransform(Editor, mapid) is Transform
                        loadTransform)
                    {
                        map.RootObject.GetUpdateTransformAction(loadTransform).Execute();
                    }
                }
            }

            if (CFG.Current.Viewport_Enable_Rendering)
            {
                // Intervene in the UI to change selection if requested.
                // We want to do this as soon as the RootObject is available, rather than at the end of all jobs.
                if (selectOnLoad)
                {
                    Selection.ClearSelection(Editor);
                    Selection.AddSelection(Editor, map.RootObject);
                }
            }

            if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
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
                    obj.UpdateRenderModel(Editor);
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
        SaveBTL(Editor, map);

        try
        {
            ResourceDescriptor ad = MapLocator.GetMapMSB(Project, map.Name);
            ResourceDescriptor adw = MapLocator.GetMapMSB(Project, map.Name, true);
            IMsb msb;
            DCX.Type compressionType = GetCompressionType();
            if (Editor.Project.ProjectType == ProjectType.DS3)
            {
                var prev = MSB3.Read(ad.AssetPath);
                MSB3 n = new();
                n.PartsPoses = prev.PartsPoses;
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Editor.Project.ProjectType == ProjectType.ER)
            {
                var prev = MSBE.Read(ad.AssetPath);
                MSBE n = new();
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Editor.Project.ProjectType == ProjectType.AC6)
            {
                var prev = MSB_AC6.Read(ad.AssetPath);
                MSB_AC6 n = new();
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
            {
                var prev = MSB2.Read(ad.AssetPath);
                MSB2 n = new();
                n.PartPoses = prev.PartPoses;
                msb = n;
            }
            else if (Editor.Project.ProjectType == ProjectType.SDT)
            {
                var prev = MSBS.Read(ad.AssetPath);
                MSBS n = new();
                n.PartsPoses = prev.PartsPoses;
                n.Layers = prev.Layers;
                n.Routes = prev.Routes;
                msb = n;
            }
            else if (Editor.Project.ProjectType == ProjectType.BB)
            {
                msb = new MSBB();
            }
            else if (Editor.Project.ProjectType == ProjectType.DES)
            {
                var prev = MSBD.Read(ad.AssetPath);
                MSBD n = new();
                n.Trees = prev.Trees;
                msb = n;
            }
            //TODO ACFA
            else if (Editor.Project.ProjectType == ProjectType.ACFA)
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
            else if (Editor.Project.ProjectType == ProjectType.ACV)
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
            else if (Editor.Project.ProjectType == ProjectType.ACVD)
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

            map.SerializeToMSB(msb, Editor.Project.ProjectType);

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

            if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
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

        Param regparam = Project.ParamData.PrimaryBank.Params[$"generatorregistparam_{mapid}"];
        foreach (Param.Row row in regparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "regist_" + row.ID;
            }

            registParams.Add(row.ID, row);

            MsbEntity obj = new(Editor, map, row, MsbEntityType.DS2GeneratorRegist);
            map.AddObject(obj);
        }

        Param locparam = Project.ParamData.PrimaryBank.Params[$"generatorlocation_{mapid}"];
        foreach (Param.Row row in locparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "generator_" + row.ID;
            }


            MergedParamRow mergedRow = new("GENERATOR_MERGED_PARAM");
            mergedRow.AddRow("generator-loc", row);
            generatorParams.Add(row.ID, mergedRow);

            MsbEntity obj = new(Editor, map, mergedRow, MsbEntityType.DS2Generator);
            generatorObjs.Add(row.ID, obj);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);
        }

        HashSet<ResourceDescriptor> chrsToLoad = new();
        Param genparam = Project.ParamData.PrimaryBank.Params[$"generatorparam_{mapid}"];
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
                MsbEntity obj = new(Editor, map, mergedRow, MsbEntityType.DS2Generator);
                generatorObjs.Add(row.ID, obj);
                map.AddObject(obj);
            }

            var registid = (uint)row.GetCellHandleOrThrow("GeneratorRegistParam").Value;
            if (registParams.ContainsKey(registid))
            {
                Param.Row regist = registParams[registid];
                var chrid = Project.ParamData.PrimaryBank.GetChrIDForEnemy(
                    (int)regist.GetCellHandleOrThrow("EnemyParamID").Value);
                if (chrid != null)
                {
                    ResourceDescriptor asset = ModelLocator.GetChrModel(Editor.Project, $@"c{chrid}", $@"c{chrid}");
                    MeshRenderableProxy model = MeshRenderableProxy.MeshRenderableFromFlverResource(
                        RenderScene, asset.AssetVirtualPath, ModelMarkerType.Enemy, null);
                    model.DrawFilter = RenderFilter.Character;
                    generatorObjs[row.ID].RenderSceneMesh = model;
                    model.SetSelectable(generatorObjs[row.ID]);
                    chrsToLoad.Add(asset);

                    if (CFG.Current.Viewport_Enable_Texturing)
                    {
                        ResourceDescriptor tasset = TextureLocator.GetChrTextures(Editor.Project, $@"c{chrid}");
                        if (tasset.AssetVirtualPath != null || tasset.AssetArchiveVirtualPath != null)
                        {
                            chrsToLoad.Add(tasset);
                        }
                    }
                }
            }
        }

        Param evtparam = Project.ParamData.PrimaryBank.Params[$"eventparam_{mapid}"];
        foreach (Param.Row row in evtparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "event_" + row.ID;
            }

            eventParams.Add(row.ID, row);

            MsbEntity obj = new(Editor, map, row, MsbEntityType.DS2Event);
            map.AddObject(obj);
        }

        Param evtlparam = Project.ParamData.PrimaryBank.Params[$"eventlocation_{mapid}"];
        foreach (Param.Row row in evtlparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "eventloc_" + row.ID;
            }

            eventLocationParams.Add(row.ID, row);

            MsbEntity obj = new(Editor, map, row, MsbEntityType.DS2EventLocation);
            map.AddObject(obj);
            map.MapOffsetNode.AddChild(obj);

            // Try rendering as a box for now
            DebugPrimitiveRenderableProxy mesh = RenderableHelper.GetBoxRegionProxy(RenderScene);
            mesh.World = obj.GetLocalTransform().WorldMatrix;
            obj.RenderSceneMesh = mesh;
            mesh.DrawFilter = RenderFilter.Region;
            mesh.SetSelectable(obj);
        }

        Param objparam = Project.ParamData.PrimaryBank.Params[$"mapobjectinstanceparam_{mapid}"];
        foreach (Param.Row row in objparam.Rows)
        {
            if (string.IsNullOrEmpty(row.Name))
            {
                row.Name = "objinstance_" + row.ID;
            }

            objectInstanceParams.Add(row.ID, row);

            MsbEntity obj = new(Editor, map, row, MsbEntityType.DS2ObjectInstance);
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

    public void LoadRelatedMapsER(string mapid)
    {
        IReadOnlyDictionary<string, SpecialMapConnections.RelationType> relatedMaps =
            SpecialMapConnections.GetRelatedMaps(Editor, mapid);

        foreach (KeyValuePair<string, SpecialMapConnections.RelationType> map in relatedMaps)
        {
            Editor.MapListView.TriggerMapLoad(map.Key);
        }
    }

    public BTL ReturnBTL(ResourceDescriptor ad)
    {
        try
        {
            BTL btl;

            if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
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
        ResourceDescriptor regparamad = ParamLocator.GetDS2GeneratorRegistParam(Editor.Project, map.Name);
        ResourceDescriptor regparamadw = ParamLocator.GetDS2GeneratorRegistParam(Editor.Project, map.Name, true);
        Param regparam = Param.Read(regparamad.AssetPath);
        PARAMDEF reglayout = ParamLocator.GetParamdefForParam(Editor.Project, regparam.ParamType);
        regparam.ApplyParamdef(reglayout);

        ResourceDescriptor locparamad = ParamLocator.GetDS2GeneratorLocationParam(Editor.Project, map.Name);
        ResourceDescriptor locparamadw = ParamLocator.GetDS2GeneratorLocationParam(Editor.Project, map.Name, true);
        Param locparam = Param.Read(locparamad.AssetPath);
        PARAMDEF loclayout = ParamLocator.GetParamdefForParam(Editor.Project, locparam.ParamType);
        locparam.ApplyParamdef(loclayout);

        ResourceDescriptor genparamad = ParamLocator.GetDS2GeneratorParam(Editor.Project, map.Name);
        ResourceDescriptor genparamadw = ParamLocator.GetDS2GeneratorParam(Editor.Project, map.Name, true);
        Param genparam = Param.Read(genparamad.AssetPath);
        PARAMDEF genlayout = ParamLocator.GetParamdefForParam(Editor.Project, genparam.ParamType);
        genparam.ApplyParamdef(genlayout);

        ResourceDescriptor evtparamad = ParamLocator.GetDS2EventParam(Editor.Project, map.Name);
        ResourceDescriptor evtparamadw = ParamLocator.GetDS2EventParam(Editor.Project, map.Name, true);
        Param evtparam = Param.Read(evtparamad.AssetPath);
        PARAMDEF evtlayout = ParamLocator.GetParamdefForParam(Editor.Project, evtparam.ParamType);
        evtparam.ApplyParamdef(evtlayout);

        ResourceDescriptor evtlparamad = ParamLocator.GetDS2EventLocationParam(Editor.Project, map.Name);
        ResourceDescriptor evtlparamadw = ParamLocator.GetDS2EventLocationParam(Editor.Project, map.Name, true);
        Param evtlparam = Param.Read(evtlparamad.AssetPath);
        PARAMDEF evtllayout = ParamLocator.GetParamdefForParam(Editor.Project, evtlparam.ParamType);
        evtlparam.ApplyParamdef(evtllayout);

        ResourceDescriptor objparamad = ParamLocator.GetDS2ObjInstanceParam(Editor.Project, map.Name);
        ResourceDescriptor objparamadw = ParamLocator.GetDS2ObjInstanceParam(Editor.Project, map.Name, true);
        Param objparam = Param.Read(objparamad.AssetPath);
        PARAMDEF objlayout = ParamLocator.GetParamdefForParam(Editor.Project, objparam.ParamType);
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
        if (Editor.Project.ProjectType == ProjectType.DS3)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }

        if (Editor.Project.ProjectType == ProjectType.ER)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }

        if (Editor.Project.ProjectType == ProjectType.AC6)
        {
            return DCX.Type.DCX_KRAK_MAX;
        }
        else if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
        {
            return DCX.Type.None;
        }
        else if (Editor.Project.ProjectType == ProjectType.SDT)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (Editor.Project.ProjectType == ProjectType.BB)
        {
            return DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (Editor.Project.ProjectType == ProjectType.DES)
        {
            return DCX.Type.None;
        }

        return DCX.Type.None;
    }

    /// <summary>
    ///     Save BTL light data
    /// </summary>
    public void SaveBTL(MapEditorScreen editor, MapContainer map)
    {
        List<ResourceDescriptor> BTLs = MapLocator.GetMapBTLs(Project, map.Name);
        List<ResourceDescriptor> BTLs_w = MapLocator.GetMapBTLs(Project, map.Name, true);
        DCX.Type compressionType = GetCompressionType();
        if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
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

                        Utils.WriteWithBackup(editor.Project, Utils.GetLocalAssetPath(editor.Project, bdtPath), bdt,
                            Utils.GetLocalAssetPath(editor.Project, BTLs_w[i].AssetPath));
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

                        Utils.WriteWithBackup(editor.Project, Utils.GetLocalAssetPath(editor.Project, BTLs_w[i].AssetPath), btl);
                    }
                }
            }
        }
    }

    public void SaveAllMaps()
    {
        foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                SaveMap(entry.Value.MapContainer);
            }
        }
    }

    public void UnloadMapContainer(string mapID, bool clearFromList = false)
    {
        foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
        {
            if (entry.Key.Filename != mapID)
                continue;

            Editor.CollisionManager.OnUnloadMap(entry.Key.Filename);

            foreach (Entity obj in entry.Value.MapContainer.Objects)
            {
                if (obj != null)
                {
                    obj.Dispose();
                }
            }

            entry.Value.MapContainer.Clear();
            entry.Value.MapContainer = null;
        }
    }

    public void UnloadAllMaps()
    {
        foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                UnloadMapContainer(entry.Key.Filename);
            }
        }
    }

    public void UnloadAll(bool clearFromList = false)
    {
        foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                UnloadMapContainer(entry.Key.Filename);
            }
        }
    }

    public void ScheduleTextureRefresh()
    {
        if (Editor.Project.ProjectType == ProjectType.DS1)
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
