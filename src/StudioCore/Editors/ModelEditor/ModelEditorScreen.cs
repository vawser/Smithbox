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
using Microsoft.Extensions.Logging;
using System.IO;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.UserProject;
using StudioCore.AssetLocator;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen, AssetBrowserEventHandler, IResourceEventListener
{
    private ModelAssetBrowser _assetBrowser;

    private readonly ModelPropertyEditor _propEditor;
    private readonly ModelPropertyCache _propCache = new();

    private readonly ModelSceneTree _sceneTree;
    private readonly ViewportSelection _selection = new();

    private readonly Universe _universe;
    private string _currentModel;

    private ResourceHandle<FlverResource> _flverhandle;

    private Task _loadingTask;
    private MeshRenderableProxy _renderMesh;

    public MapEditor.EntityActionManager EditorActionManager = new();
    public Rectangle Rect;
    public RenderScene RenderScene;
    public IViewport Viewport;

    private bool ViewportUsingKeyboard;
    private Sdl2Window Window;

    public ModelEditorModelType CurrentlyLoadedModelType;

    public LoadedModelInfo _loadedModelInfo;

    public ModelEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
        Window = window;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport(ViewportType.ModelEditor,"Modeleditvp", device, RenderScene, EditorActionManager, _selection, Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport(ViewportType.ModelEditor, "Modeleditvp", EditorActionManager, _selection, Rect.Width, Rect.Height);
        }

        _universe = new Universe(RenderScene, _selection);

        _sceneTree = new ModelSceneTree(this, "modeledittree", _universe, _selection, EditorActionManager, Viewport);
        _propEditor = new ModelPropertyEditor(EditorActionManager, _propCache, Viewport, null);
        _assetBrowser = new ModelAssetBrowser(this, "modelEditorBrowser");
    }

    public void UpdateLoadedModelInfo(string modelName, string mapID = "")
    {
        _loadedModelInfo = new LoadedModelInfo(modelName, CurrentlyLoadedModelType, mapID);
    }

    public void OnInstantiateChr(string chrid)
    {
        CurrentlyLoadedModelType = ModelEditorModelType.Character;
        LoadModel(chrid, ModelEditorModelType.Character);
        UpdateLoadedModelInfo(chrid);
    }

    public void OnInstantiateObj(string objid)
    {
        CurrentlyLoadedModelType = ModelEditorModelType.Object;
        LoadModel(objid, ModelEditorModelType.Object);
        UpdateLoadedModelInfo(objid);
    }

    public void OnInstantiateParts(string partsid)
    {
        CurrentlyLoadedModelType = ModelEditorModelType.Parts;
        LoadModel(partsid, ModelEditorModelType.Parts);
        UpdateLoadedModelInfo(partsid);
    }

    public void OnInstantiateMapPiece(string mapid, string modelid)
    {
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
            FlverResource r = _flverhandle.Get();
            _universe.LoadFlver(r.Flver, _renderMesh, _currentModel);
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

        if (ImGui.BeginMenu("Model"))
        {
            if (ImGui.MenuItem("Unload"))
            {
                _loadedModelInfo = null;
                _universe.UnloadAll(true);
            }

            // TODO: Select other Flvers within the same container (e.g. _1, _2, etc)

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Dummy Polygons", "", CFG.Current.ModelEditor_ViewDummyPolys, true))
            {
                CFG.Current.ModelEditor_ViewDummyPolys = !CFG.Current.ModelEditor_ViewDummyPolys;
                CFG.Current.ModelEditor_RenderingUpdate = true;
            }
            if (ImGui.MenuItem("Bones", "", CFG.Current.Model_ViewBones, true))
            {
                CFG.Current.Model_ViewBones = !CFG.Current.Model_ViewBones;
                CFG.Current.ModelEditor_RenderingUpdate = true;
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

        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
        //ImGui.Text($@"Viewport size: {Viewport.Width}x{Viewport.Height}");
        //ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

        Viewport.OnGui();
        _assetBrowser.Display();
        _sceneTree.OnGui();
        _propEditor.OnGui(_selection, "modeleditprop", Viewport.Width, Viewport.Height);
        ResourceManager.OnGuiDrawTasks(Viewport.Width, Viewport.Height);
    }

    public bool InputCaptured()
    {
        return Viewport.ViewportSelected;
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        if (Project.Type != ProjectType.Undefined)
        {
            _assetBrowser.OnProjectChanged();
        }

        _loadedModelInfo = null;
        _universe.UnloadAll(true);
    }

    public void Save()
    {
        if(_loadedModelInfo != null)
        {
            // Copy the binder to the mod directory if it does not already exist.

            var exists = _loadedModelInfo.CopyBinderToMod();

            if (exists)
            {
                if (Project.Type == ProjectType.DS1 || Project.Type == ProjectType.DS1R)
                {
                    if (_loadedModelInfo.Type == ModelEditorModelType.MapPiece)
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
        LoadedModelInfo info = _loadedModelInfo;
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

            switch (Project.Type)
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
        LoadedModelInfo info = _loadedModelInfo;
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

            switch (Project.Type)
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
        LoadedModelInfo info = _loadedModelInfo;
        FlverResource flvResource = _flverhandle.Get();
        byte[] fileBytes = null;

        FLVER2 flver = FLVER2.Read(DCX.Decompress(info.ModBinderPath));
        flver = flvResource.Flver;
        flver.Write(DCX.Type.DCX_DFLT_10000_24_9);

        TaskLogs.AddLog($"Saved model at: {info.ModBinderPath}");
    }

    public void SaveAll()
    {
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
                _universe.UnloadAll(true);
                _universe.LoadFlver(r.Flver, _renderMesh, _currentModel);
            }
        }
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        _flverhandle = null;
    }

    public void OnEntityContextMenu(Entity ent)
    {
        
    }

    public AssetDescription loadedAsset;

    public void LoadModel(string modelid, ModelEditorModelType modelType, string mapid = null)
    {
        AssetDescription asset;
        AssetDescription assettex;
        var filt = RenderFilter.All;
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");
        switch (modelType)
        {
            case ModelEditorModelType.Character:
                asset = ModelAssetLocator.GetChrModel(modelid);
                assettex = TextureAssetLocator.GetChrTextures(modelid);
                break;
            case ModelEditorModelType.Object:
                asset = ModelAssetLocator.GetObjModel(modelid);
                assettex = TextureAssetLocator.GetObjTexture(modelid);
                break;
            case ModelEditorModelType.Parts:
                asset = ModelAssetLocator.GetPartsModel(modelid);
                assettex = TextureAssetLocator.GetPartTextures(modelid);
                break;
            case ModelEditorModelType.MapPiece:
                asset = ModelAssetLocator.GetMapModel(mapid, modelid);
                assettex = ModelAssetLocator.GetNullAsset();
                break;
            default:
                //Uh oh
                asset = ModelAssetLocator.GetNullAsset();
                assettex = ModelAssetLocator.GetNullAsset();
                break;
        }

        if (_renderMesh != null)
        {
            _renderMesh.Dispose();
        }

        _renderMesh = MeshRenderableProxy.MeshRenderableFromFlverResource(
            RenderScene, asset.AssetVirtualPath, ModelMarkerType.None);
        //_renderMesh.DrawFilter = filt;
        _renderMesh.World = Matrix4x4.Identity;
        _currentModel = modelid;
        if (!ResourceManager.IsResourceLoadedOrInFlight(asset.AssetVirtualPath, AccessLevel.AccessFull))
        {
            if (asset.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessFull, false,
                    ResourceManager.ResourceType.Flver);
            }
            else if (asset.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessFull);
            }

            if (assettex.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(assettex.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                    ResourceManager.ResourceType.Texture);
            }
            else if (assettex.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(assettex.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }

            _loadingTask = job.Complete();
        }

        ResourceManager.AddResourceListener<FlverResource>(asset.AssetVirtualPath, this, AccessLevel.AccessFull);
    }
}
