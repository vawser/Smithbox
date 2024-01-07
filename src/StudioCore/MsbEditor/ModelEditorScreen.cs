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

namespace StudioCore.MsbEditor;

public class ModelEditorScreen : EditorScreen, AssetBrowserEventHandler, SceneTreeEventHandler,
    IResourceEventListener
{
    private readonly AssetBrowser _assetBrowser;
    private readonly PropertyEditor _propEditor;
    private readonly PropertyCache _propCache = new();

    private readonly SceneTree _sceneTree;
    private readonly Selection _selection = new();

    private readonly Universe _universe;
    private string _currentModel;

    private ResourceHandle<FlverResource> _flverhandle;

    private Task _loadingTask;
    private MeshRenderableProxy _renderMesh;

    public AssetLocator AssetLocator;
    public ActionManager EditorActionManager = new();
    public Rectangle Rect;
    public RenderScene RenderScene;
    public IViewport Viewport;

    private bool ViewportUsingKeyboard;
    private Sdl2Window Window;

    public ModelEditorScreen(Sdl2Window window, GraphicsDevice device, AssetLocator locator)
    {
        Rect = window.Bounds;
        AssetLocator = locator;
        ResourceManager.Locator = AssetLocator;
        Window = window;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport("Modeleditvp", device, RenderScene, EditorActionManager, _selection,
                Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport("Modeleditvp", EditorActionManager, _selection, Rect.Width, Rect.Height);
        }

        _universe = new Universe(AssetLocator, RenderScene, _selection);

        _sceneTree = new SceneTree(SceneTree.Configuration.ModelEditor, this, "modeledittree", _universe,
            _selection, EditorActionManager, Viewport, AssetLocator);
        _propEditor = new PropertyEditor(EditorActionManager, _propCache);
        _assetBrowser = new AssetBrowser(this, "modelEditorBrowser", AssetLocator);
    }

    public void OnInstantiateChr(string chrid)
    {
        LoadModel(chrid, ModelEditorModelType.Character);
    }

    public void OnInstantiateObj(string objid)
    {
        LoadModel(objid, ModelEditorModelType.Object);
    }

    public void OnInstantiateParts(string partsid)
    {
        LoadModel(partsid, ModelEditorModelType.Parts);
    }

    public void OnInstantiateMapPiece(string mapid, string modelid)
    {
        LoadModel(modelid, ModelEditorModelType.MapPiece, mapid);
    }

    public string EditorName => "Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";

    public void Update(float dt)
    {
        ViewportUsingKeyboard = Viewport.Update(Window, dt);

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
    }

    public void OnGUI(string[] commands)
    {
        var scale = MapStudioNew.GetUIScale();
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
            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_FrameSelection))
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
        _assetBrowser.OnGui();
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
        ReloadAssetBrowser();
    }

    public void Save()
    {
    }

    public void SaveAll()
    {
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

    public void LoadModel(string modelid, ModelEditorModelType modelType, string mapid = null)
    {
        AssetDescription asset;
        AssetDescription assettex;
        var filt = RenderFilter.All;
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");
        switch (modelType)
        {
            case ModelEditorModelType.Character:
                asset = AssetLocator.GetChrModel(modelid);
                assettex = AssetLocator.GetChrTextures(modelid);
                break;
            case ModelEditorModelType.Object:
                asset = AssetLocator.GetObjModel(modelid);
                assettex = AssetLocator.GetObjTexture(modelid);
                break;
            case ModelEditorModelType.Parts:
                asset = AssetLocator.GetPartsModel(modelid);
                assettex = AssetLocator.GetPartTextures(modelid);
                break;
            case ModelEditorModelType.MapPiece:
                asset = AssetLocator.GetMapModel(mapid, modelid);
                assettex = AssetLocator.GetNullAsset();
                break;
            default:
                //Uh oh
                asset = AssetLocator.GetNullAsset();
                assettex = AssetLocator.GetNullAsset();
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

    public void ReloadAssetBrowser()
    {
        if (AssetLocator.Type != GameType.Undefined)
        {
            _assetBrowser.ClearCaches();
        }
    }
}
