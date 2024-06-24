using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Viewport = StudioCore.Gui.Viewport;
using StudioCore.Configuration;
using System.IO;
using SoulsFormats;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using StudioCore.Editors.ModelEditor.Toolbar;
using ModelCore.Editors.ModelEditor.Toolbar;
using StudioCore.Locators;
using StudioCore.Editors.AssetBrowser;
using StudioCore.Core;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen, IResourceEventListener
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public AssetBrowserScreen ModelAssetBrowser;

    private readonly ModelPropertyEditor _propEditor;
    private readonly ModelPropertyCache _propCache = new();

    public static ModelSceneTree _sceneTree;
    public static ViewportSelection _selection = new();

    public static Universe _universe;

    public static ResourceHandle<FlverResource> _flverhandle;

    private Task _loadingTask;
    public static MeshRenderableProxy _renderMesh;

    public static MapEditor.ViewportActionManager EditorActionManager = new();
    public Rectangle Rect;
    public static RenderScene RenderScene;
    public IViewport Viewport;

    private bool ViewportUsingKeyboard;
    private Sdl2Window Window;

    public ModelEditorModelType CurrentlyLoadedModelType;

    public ModelToolbar _modelToolbar;
    public ModelToolbar_ActionList _modelToolbar_ActionList;
    public ModelToolbar_Configuration _modelToolbar_Configuration;

    public static string SelectedAssetID;
    public static LoadedModelInfo CurrentModelInfo;

    public ModelEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
        Window = window;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport(ViewportType.ModelEditor, "Modeleditvp", device, RenderScene, EditorActionManager, _selection, Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport(ViewportType.ModelEditor, "Modeleditvp", EditorActionManager, _selection, Rect.Width, Rect.Height);
        }

        _universe = new Universe(RenderScene, _selection);

        _sceneTree = new ModelSceneTree(this, "modeledittree", _universe, _selection, EditorActionManager, Viewport);
        _propEditor = new ModelPropertyEditor(EditorActionManager, _propCache, Viewport, null);

        ModelAssetBrowser = new AssetBrowserScreen(AssetBrowserSource.ModelEditor, _universe, RenderScene, _selection, EditorActionManager, this, Viewport);

        _modelToolbar = new ModelToolbar(EditorActionManager, this);
        _modelToolbar_ActionList = new ModelToolbar_ActionList();
        _modelToolbar_Configuration = new ModelToolbar_Configuration();
    }

    public void Init()
    {
        ShowSaveOption = false;
    }

    public void UpdateLoadedModelInfo(string modelName, string mapID = "")
    {
        CurrentModelInfo = new LoadedModelInfo(modelName, CurrentlyLoadedModelType, mapID);
    }

    public void OnInstantiateChr(string chrid)
    {
        SelectedAssetID = chrid;
        CurrentlyLoadedModelType = ModelEditorModelType.Character;
        LoadModel(chrid, ModelEditorModelType.Character);
        UpdateLoadedModelInfo(chrid);
    }

    public void OnInstantiateObj(string objid)
    {
        SelectedAssetID = objid;
        CurrentlyLoadedModelType = ModelEditorModelType.Object;
        LoadModel(objid, ModelEditorModelType.Object);
        UpdateLoadedModelInfo(objid);
    }

    public void OnInstantiateParts(string partsid)
    {
        SelectedAssetID = partsid;
        CurrentlyLoadedModelType = ModelEditorModelType.Parts;
        LoadModel(partsid, ModelEditorModelType.Parts);
        UpdateLoadedModelInfo(partsid);
    }

    public void OnInstantiateMapPiece(string mapid, string modelid)
    {
        SelectedAssetID = $"{mapid}{modelid}";
        CurrentlyLoadedModelType = ModelEditorModelType.MapPiece;
        LoadModel(modelid, ModelEditorModelType.MapPiece, mapid);
        UpdateLoadedModelInfo(modelid, mapid);
    }

    public string EditorName => "Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";

    public void Update(float dt)
    {
        ViewportUsingKeyboard = Viewport.Update(Window, dt);

        // Reload the flvers if a rendering bool has changed
        if (CFG.Current.ModelEditor_RenderingUpdate)
        {
            CFG.Current.ModelEditor_RenderingUpdate = false;

            if (_flverhandle != null)
            {
                FlverResource r = _flverhandle.Get();
                _universe.LoadFlverInModelEditor(r.Flver, _renderMesh, CurrentModelInfo.ModelName);
            }

            if (CFG.Current.Viewport_Enable_Texturing)
            {
                _universe.ScheduleTextureRefresh();
            }
        }

        if (_loadingTask != null && _loadingTask.IsCompleted)
        {
            _loadingTask = null;
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Window = window;
        Rect = window.Bounds;
        //Viewport.ResizeViewport(device, new Rectangle(0, 0, window.Width, window.Height));
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Viewport != null)
        {
            Viewport.Draw(device, cl);
        }
    }

    public void DrawEditorMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", KeyBindings.Current.Core_Undo.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Scissors}");
            if (ImGui.MenuItem("Remove", KeyBindings.Current.Core_Delete.HintText, false, _selection.IsSelection()))
            {
                ModelAction_DeleteProperty.DeleteFLVERProperty();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.FilesO}");
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.Core_Duplicate.HintText, false,
                    _selection.IsSelection()))
            {
                ModelAction_DuplicateProperty.DuplicateFLVERProperty();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.FilesO}");
            if (ImGui.MenuItem("Load Asset Selection", KeyBindings.Current.ModelEditor_LoadCurrentSelection.HintText, true))
            {
                ModelAssetBrowser.LoadModelAssetSelection();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.WindowClose}");
            if (ImGui.MenuItem("Unload Current Asset", KeyBindings.Current.ModelEditor_UnloadCurrentSelection.HintText, true))
            {
                CurrentModelInfo = null;
                _universe.UnloadModels(true);
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport"))
            {
                CFG.Current.Interface_Editor_Viewport = !CFG.Current.Interface_Editor_Viewport;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_Editor_Viewport);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Model Hierarchy"))
            {
                CFG.Current.Interface_ModelEditor_ModelHierarchy = !CFG.Current.Interface_ModelEditor_ModelHierarchy;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ModelHierarchy);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Properties"))
            {
                CFG.Current.Interface_ModelEditor_Properties = !CFG.Current.Interface_ModelEditor_Properties;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Properties);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Asset Browser"))
            {
                CFG.Current.Interface_ModelEditor_AssetBrowser = !CFG.Current.Interface_ModelEditor_AssetBrowser;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_AssetBrowser);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Toolbar"))
            {
                CFG.Current.Interface_ModelEditor_Toolbar = !CFG.Current.Interface_ModelEditor_Toolbar;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Toolbar);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Profiling"))
            {
                CFG.Current.Interface_Editor_Profiling = !CFG.Current.Interface_Editor_Profiling;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_Editor_Profiling);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_ModelEditor_ResourceList = !CFG.Current.Interface_ModelEditor_ResourceList;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ResourceList);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport Grid"))
            {
                CFG.Current.Interface_ModelEditor_Viewport_Grid = !CFG.Current.Interface_ModelEditor_Viewport_Grid;
                CFG.Current.ModelEditor_Viewport_RegenerateMapGrid = true;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Viewport_Grid);

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Filters", RenderScene != null && Viewport != null))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Dummy Polygons"))
            {
                CFG.Current.ModelEditor_ViewDummyPolys = !CFG.Current.ModelEditor_ViewDummyPolys;
                CFG.Current.ModelEditor_RenderingUpdate = true;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.ModelEditor_ViewDummyPolys);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Bones"))
            {
                CFG.Current.Model_ViewBones = !CFG.Current.Model_ViewBones;
                CFG.Current.ModelEditor_RenderingUpdate = true;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Model_ViewBones);

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Viewport"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.LightbulbO}");
            if (ImGui.BeginMenu("Scene Lighting"))
            {
                Viewport.SceneParamsGui();
                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Gizmos"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Compass}");
            if (ImGui.BeginMenu("Mode"))
            {
                if (ImGui.MenuItem("Translate", KeyBindings.Current.Viewport_TranslateMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Translate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Translate;
                }

                if (ImGui.MenuItem("Rotate", KeyBindings.Current.Viewport_RotationMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Rotate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Cube}");
            if (ImGui.BeginMenu("Space"))
            {
                if (ImGui.MenuItem("Local", KeyBindings.Current.Viewport_ToggleGizmoSpace.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.Local))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }

                if (ImGui.MenuItem("World", KeyBindings.Current.Viewport_ToggleGizmoSpace.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.World))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Cubes}");
            if (ImGui.BeginMenu("Origin"))
            {
                if (ImGui.MenuItem("World", KeyBindings.Current.Viewport_ToggleGizmoOrigin.HintText,
                        Gizmos.Origin == Gizmos.GizmosOrigin.World))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }

                if (ImGui.MenuItem("Bounding Box", KeyBindings.Current.Viewport_ToggleGizmoOrigin.HintText,
                        Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void OnGUI(string[] commands)
    {
        var scale = Smithbox.GetUIScale();
        // Docking setup
        //var vp = ImGui.GetMainViewport();
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);
        var dsid = ImGui.GetID("DockSpace_ModelEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0));

        // Keyboard shortcuts
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
        {
            EditorActionManager.RedoAction();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate))
        {
            ModelAction_DuplicateProperty.DuplicateFLVERProperty();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete))
        {
            ModelAction_DeleteProperty.DeleteFLVERProperty();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.ModelEditor_LoadCurrentSelection))
        {
            ModelAssetBrowser.LoadModelAssetSelection();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.ModelEditor_UnloadCurrentSelection))
        {
            CurrentModelInfo = null;
            _universe.UnloadModels(true);
        }

        if (!ViewportUsingKeyboard && !ImGui.GetIO().WantCaptureKeyboard)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_TranslateMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Translate;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_RotationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Rotate;
            }

            // Use home key to cycle between gizmos origin modes
            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_ToggleGizmoOrigin))
            {
                if (Gizmos.Origin == Gizmos.GizmosOrigin.World)
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }
                else if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox)
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }
            }

            // F key frames the selection
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport))
            {
                HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();
                var first = false;
                BoundingBox box = new();
                foreach (Entity s in selected)
                {
                    if (s.RenderSceneMesh != null)
                    {
                        if (!first)
                        {
                            box = s.RenderSceneMesh.GetBounds();
                            first = true;
                        }
                        else
                        {
                            box = BoundingBox.Combine(box, s.RenderSceneMesh.GetBounds());
                        }
                    }
                }

                if (first)
                {
                    Viewport.FrameBox(box);
                }
            }

            // Render settings
            if (InputTracker.GetControlShortcut(Key.Number1))
            {
                RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character |
                                         RenderFilter.Region;
            }
            else if (InputTracker.GetControlShortcut(Key.Number2))
            {
                RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character |
                                         RenderFilter.Region;
            }
            else if (InputTracker.GetControlShortcut(Key.Number3))
            {
                RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object |
                                         RenderFilter.Character | RenderFilter.Region;
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
        //ImGui.Text($@"Viewport size: {Viewport.Width}x{Viewport.Height}");
        //ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

        Viewport.OnGui();
        ModelAssetBrowser.OnGui();
        _sceneTree.OnGui();
        _propEditor.OnGui(_selection, "modeleditprop", Viewport.Width, Viewport.Height);

        if(CFG.Current.Interface_ModelEditor_Toolbar)
        {
            _modelToolbar_ActionList.OnGui();
            _modelToolbar_Configuration.OnGui();
        }

        ResourceManager.OnGuiDrawTasks(Viewport.Width, Viewport.Height);

        if (CFG.Current.Interface_ModelEditor_ResourceList)
        {
            ResourceManager.OnGuiDrawResourceList("modelResourceList");
        }
        ImGui.PopStyleColor(1);
    }

    public bool InputCaptured()
    {
        return Viewport.ViewportSelected;
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ModelAssetBrowser.OnProjectChanged();
        }

        CurrentModelInfo = null;
        _universe.UnloadAll(true);
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (CurrentModelInfo != null)
        {
            // Copy the binder to the mod directory if it does not already exist.

            var exists = CurrentModelInfo.CopyBinderToMod();

            if (exists)
            {
                if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
                {
                    if (CurrentModelInfo.Type == ModelEditorModelType.MapPiece)
                    {
                        WriteModelFlver(); // DS1 doesn't wrap the mappiece flver within a container
                    }
                    else
                    {
                        WriteModelBinderBND3();
                    }
                }
                else
                {
                    WriteModelBinderBND4();
                }
            }
        }
    }

    public void WriteModelBinderBND4()
    {
        LoadedModelInfo info = CurrentModelInfo;
        FlverResource flvResource = _flverhandle.Get();

        byte[] fileBytes = null;

        using (IBinder binder = BND4.Read(DCX.Decompress(info.ModBinderPath)))
        {
            foreach (var file in binder.Files)
            {
                var curFileName = $"{Path.GetFileName(file.Name)}";

                if (curFileName == info.FlverFileName)
                {
                    try
                    {
                        file.Bytes = flvResource.Flver.Write();
                    }
                    catch (Exception ex)
                    {
                        TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
                    }
                }
            }

            // Then write those bytes to file
            BND4 writeBinder = binder as BND4;

            switch (Smithbox.ProjectType)
            {
                case ProjectType.DS3:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                    break;
                case ProjectType.SDT:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                    break;
                case ProjectType.ER:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                    break;
                case ProjectType.AC6:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
                    break;
                default:
                    TaskLogs.AddLog($"Invalid ProjectType during Model Editor Save");
                    return;
            }
        }

        if (fileBytes != null)
        {
            File.WriteAllBytes(info.ModBinderPath, fileBytes);
            TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
        }
    }

    public void WriteModelBinderBND3()
    {
        LoadedModelInfo info = CurrentModelInfo;
        FlverResource flvResource = _flverhandle.Get();
        byte[] fileBytes = null;

        using (IBinder binder = BND3.Read(DCX.Decompress(info.ModBinderPath)))
        {
            foreach (var file in binder.Files)
            {
                var curFileName = $"{Path.GetFileName(file.Name)}";

                if (curFileName == info.FlverFileName)
                {
                    try
                    {
                        file.Bytes = flvResource.Flver.Write();
                    }
                    catch (Exception ex)
                    {
                        TaskLogs.AddLog($"{file.ID} - Failed to write.\n{ex.ToString()}");
                    }
                }
            }

            // Then write those bytes to file
            BND3 writeBinder = binder as BND3;

            switch (Smithbox.ProjectType)
            {
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                    break;
                default:
                    TaskLogs.AddLog($"Invalid ProjectType during Model Editor Save");
                    return;
            }
        }

        if (fileBytes != null)
        {
            File.WriteAllBytes(info.ModBinderPath, fileBytes);
            TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
        }
    }

    public void WriteModelFlver()
    {
        LoadedModelInfo info = CurrentModelInfo;
        FlverResource flvResource = _flverhandle.Get();
        byte[] fileBytes = null;

        FLVER2 flver = FLVER2.Read(DCX.Decompress(info.ModBinderPath));
        flver = flvResource.Flver;
        flver.Write(DCX.Type.DCX_DFLT_10000_24_9);

        TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        Save(); // Just call save.
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        _flverhandle = (ResourceHandle<FlverResource>)handle;
        _flverhandle.Acquire();

        if (_renderMesh != null)
        {
            BoundingBox box = _renderMesh.GetBounds();
            Viewport.FrameBox(box);

            Vector3 dim = box.GetDimensions();
            var mindim = Math.Min(dim.X, Math.Min(dim.Y, dim.Z));
            var maxdim = Math.Max(dim.X, Math.Max(dim.Y, dim.Z));

            var minSpeed = 1.0f;
            var basespeed = Math.Max(minSpeed, (float)Math.Sqrt(mindim / 3.0f));
            Viewport.WorldView.CameraMoveSpeed_Normal = basespeed;
            Viewport.WorldView.CameraMoveSpeed_Slow = basespeed / 10.0f;
            Viewport.WorldView.CameraMoveSpeed_Fast = basespeed * 10.0f;

            Viewport.NearClip = Math.Max(0.001f, maxdim / 10000.0f);
        }

        if (_flverhandle.IsLoaded && _flverhandle.Get() != null)
        {
            FlverResource r = _flverhandle.Get();
            if (r.Flver != null)
            {
                _universe.UnloadModels(true);
                _universe.LoadFlverInModelEditor(r.Flver, _renderMesh, CurrentModelInfo.ModelName);
            }
        }

        if (CFG.Current.Viewport_Enable_Texturing)
        {
            _universe.ScheduleTextureRefresh();
        }
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        _flverhandle = null;
    }

    public void OnEntityContextMenu(Entity ent)
    {
        if (ImGui.MenuItem("Duplicate"))
        {
            ModelAction_DuplicateProperty.DuplicateFLVERProperty();
        }
        if (ImGui.MenuItem("Delete"))
        {
            ModelAction_DeleteProperty.DeleteFLVERProperty();
        }
    }

    public ResourceDescriptor loadedAsset;

    public void LoadModel(string modelid, ModelEditorModelType modelType, string mapid = null)
    {
        LoadModelInternal(modelid, modelType, mapid);

        // If model ID has additional textures associated with it, load them
        if (Smithbox.BankHandler.AdditionalTextureInfo.HasAdditionalTextures(modelid))
        {
            foreach (var entry in Smithbox.BankHandler.AdditionalTextureInfo.GetAdditionalTextures(modelid))
            {
                LoadModelInternal(entry, modelType, mapid, true);
            }
        }
    }

    // PIPELINE: find resource desciptor for passed parameters and then start a new Resource Job for loading the resource.
    public void LoadModelInternal(string modelid, ModelEditorModelType modelType, string mapid = null, bool skipModel = false)
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");

        ResourceDescriptor modelAsset = GetModelAssetDescriptor(modelid, modelType, mapid);
        ResourceDescriptor textureAsset = GetTextureAssetDescriptor(modelid, modelType, mapid);

        UpdateRenderMesh(modelAsset, skipModel);

        // PIPELINE: resource has not already been loaded
        if (!ResourceManager.IsResourceLoadedOrInFlight(modelAsset.AssetVirtualPath, AccessLevel.AccessFull))
        {
            // Ignore this if we are only loading textures
            if (!skipModel)
            {
                // PIPELINE: resource path is a archive path (MAPBND.DCX or MAPBHD/MAPBDT)
                if (modelAsset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(modelAsset.AssetArchiveVirtualPath, AccessLevel.AccessFull, false, ResourceManager.ResourceType.Flver);
                }
                // PIPELINE: resource path is adirect path (FLVER.DCX)
                else if (modelAsset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(modelAsset.AssetVirtualPath, AccessLevel.AccessFull);
                }
            }

            if (Universe.IsRendering)
            {
                if (CFG.Current.Viewport_Enable_Texturing)
                {
                    if (textureAsset.AssetArchiveVirtualPath != null)
                    {
                        job.AddLoadArchiveTask(textureAsset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.Texture);
                    }
                    else if (textureAsset.AssetVirtualPath != null)
                    {
                        job.AddLoadFileTask(textureAsset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                    }
                }
            }

            _loadingTask = job.Complete();
        }

        ResourceManager.AddResourceListener<FlverResource>(modelAsset.AssetVirtualPath, this, AccessLevel.AccessFull);
    }

    public static void UpdateLoadedRenderMesh()
    {
        ResourceDescriptor asset = GetModelAssetDescriptor(CurrentModelInfo.ModelName, CurrentModelInfo.Type, CurrentModelInfo.MapID);

        if (Universe.IsRendering)
        {
            if (_renderMesh != null)
            {
                _renderMesh.Dispose();
            }

            _renderMesh = MeshRenderableProxy.MeshRenderableFromFlverResource(
                RenderScene, asset.AssetVirtualPath, ModelMarkerType.None);
            _renderMesh.World = Matrix4x4.Identity;
        }
    }

    public void UpdateRenderMesh(ResourceDescriptor modelAsset, bool skipModel = false)
    {
        if (Universe.IsRendering)
        {
            // Ignore this if we are only loading textures
            if (!skipModel)
            {
                if (_renderMesh != null)
                {
                    _renderMesh.Dispose();
                }

                _renderMesh = MeshRenderableProxy.MeshRenderableFromFlverResource(RenderScene, modelAsset.AssetVirtualPath, ModelMarkerType.None);
                _renderMesh.World = Matrix4x4.Identity;
            }
        }
    }

    public static ResourceDescriptor GetModelAssetDescriptor(string modelid, ModelEditorModelType modelType, string mapid = null)
    {
        ResourceDescriptor asset;

        switch (modelType)
        {
            case ModelEditorModelType.Character:
                asset = ResourceModelLocator.GetChrModel(modelid);
                break;
            case ModelEditorModelType.Object:
                asset = ResourceModelLocator.GetObjModel(modelid);
                break;
            case ModelEditorModelType.Parts:
                asset = ResourceModelLocator.GetPartsModel(modelid);
                break;
            case ModelEditorModelType.MapPiece:
                asset = ResourceModelLocator.GetMapModel(mapid, modelid);
                break;
            default:
                asset = ResourceModelLocator.GetNullAsset();
                break;
        }

        return asset;
    }

    public static ResourceDescriptor GetTextureAssetDescriptor(string modelid, ModelEditorModelType modelType, string mapid = null)
    {
        ResourceDescriptor asset;

        switch (modelType)
        {
            case ModelEditorModelType.Character:
                asset = ResourceTextureLocator.GetChrTextures(modelid);
                break;
            case ModelEditorModelType.Object:
                asset = ResourceTextureLocator.GetObjTextureContainer(modelid);
                break;
            case ModelEditorModelType.Parts:
                asset = ResourceTextureLocator.GetPartTextureContainer(modelid);
                break;
            case ModelEditorModelType.MapPiece:
                asset = ResourceModelLocator.GetNullAsset();
                break;
            default:
                asset = ResourceModelLocator.GetNullAsset();
                break;
        }

        return asset;
    }

}
