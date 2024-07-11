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
using StudioCore.Core;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ModelAssetSelectionView ModelSelectionView;

    private readonly ModelPropertyEditor _propEditor;
    private readonly ModelPropertyCache _propCache = new();

    public static ModelSceneTree _sceneTree;
    public static ViewportSelection _selection = new();

    public Universe _universe;

    public static MapEditor.ViewportActionManager EditorActionManager = new();
    public Rectangle Rect;
    public RenderScene RenderScene;
    public IViewport Viewport;

    private bool ViewportUsingKeyboard;
    private Sdl2Window Window;

    public ModelToolbar _modelToolbar;
    public ModelToolbar_ActionList _modelToolbar_ActionList;
    public ModelToolbar_Configuration _modelToolbar_Configuration;

    public ModelResourceHandler ResourceHandler;

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

        ResourceHandler = new ModelResourceHandler(this, Viewport);

        ModelSelectionView = new ModelAssetSelectionView(this);

        _universe = new Universe(RenderScene, _selection);

        _sceneTree = new ModelSceneTree(this, "modeledittree", _universe, _selection, EditorActionManager, Viewport);
        _propEditor = new ModelPropertyEditor(EditorActionManager, _propCache, Viewport, null);

        _modelToolbar = new ModelToolbar(EditorActionManager, this);
        _modelToolbar_ActionList = new ModelToolbar_ActionList();
        _modelToolbar_Configuration = new ModelToolbar_Configuration();
    }

    public void Init()
    {
        ShowSaveOption = true;
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

            if (ResourceHandler._flverhandle != null)
            {
                FlverResource r = ResourceHandler._flverhandle.Get();
                _universe.LoadFlverInModelEditor(r.Flver, ResourceHandler._renderMesh, ResourceHandler.CurrentFLVERInfo.ModelName);
            }

            if (CFG.Current.Viewport_Enable_Texturing)
            {
                _universe.ScheduleTextureRefresh();
            }
        }

        if (ResourceHandler._loadingTask != null && ResourceHandler._loadingTask.IsCompleted)
        {
            ResourceHandler._loadingTask = null;
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

    public void OnGUI(string[] initcmd)
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

        if (initcmd != null && initcmd.Length > 1)
        {
            if (initcmd[0] == "load")
            {
                var modelName = initcmd[1];
                var assetType = initcmd[2];

                if (assetType == "Character")
                {
                    ResourceHandler.LoadCharacter(modelName);
                }

                if (assetType == "Asset")
                {
                    ResourceHandler.LoadAsset(modelName);
                }

                if (assetType == "Part")
                {
                    ResourceHandler.LoadPart(modelName);
                }

                if(initcmd.Length > 3)
                {
                    var mapId = initcmd[3];

                    if (assetType == "MapPiece")
                    {
                        ResourceHandler.LoadMapPiece(mapId, modelName);
                    }
                }
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
        //ImGui.Text($@"Viewport size: {Viewport.Width}x{Viewport.Height}");
        //ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

        Viewport.OnGui();
        ModelSelectionView.OnGui();

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

        // Focus on Properties by default when this editor is made focused
        if (FirstFrame)
        {
            ImGui.SetWindowFocus("Properties##modeleditprop");

            FirstFrame = false;
        }
    }

    public bool InputCaptured()
    {
        return Viewport.ViewportSelected;
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ModelSelectionView.OnProjectChanged();
        }

        ResourceHandler.CurrentFLVERInfo = null;
        _universe.UnloadAll(true);
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ResourceHandler.SaveModel();
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        Save(); // Just call save.
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
}
