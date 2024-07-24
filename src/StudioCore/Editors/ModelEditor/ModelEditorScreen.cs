﻿using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.Resource;
using StudioCore.Scene;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Viewport = StudioCore.Gui.Viewport;
using StudioCore.Configuration;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor.Actions;

namespace StudioCore.Editors.ModelEditor;

// DESIGN:
// Actual Model is loaded as CurrentFLVER. This is the object that all edits apply to.
// Viewport Model is loaded via the Resource Manager system and is represented via the Model Container instance.
// Viewport Model is never edited, and instead it is discarded and re-generated from saved actual model when required.

// PITFALLS:
// Actual Model and Viewport Model must be manually kept in sync when entries are added/removed.
// Default method is to add to actual model, force actual model save and then re-load.
public class ModelEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public MapEditor.ViewportActionManager EditorActionManager = new();

    public ModelSelectionView ModelSelectionView;

    public ModelPropertyEditor ModelPropertyEditor;
    public ModelPropertyCache _propCache = new();

    public ModelHierarchyView ModelHierarchy;
    public ViewportSelection _selection = new();

    public Universe _universe;

    public Rectangle Rect;
    public RenderScene RenderScene;
    public IViewport Viewport;

    public bool ViewportUsingKeyboard;
    public Sdl2Window Window;

    public ModelResourceHandler ResourceHandler;
    public ModelViewportHandler ViewportHandler;
    public SkeletonHandler SkeletonHandler;

    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;

    public ActionSubMenu ActionSubMenu;

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

        ResourceHandler = new ModelResourceHandler(this, Viewport);
        ViewportHandler = new ModelViewportHandler(this, Viewport);
        ModelSelectionView = new ModelSelectionView(this);
        ModelHierarchy = new ModelHierarchyView(this);
        ModelPropertyEditor = new ModelPropertyEditor(this);
        SkeletonHandler = new SkeletonHandler(this, _universe);

        ToolWindow = new ToolWindow(this);
        ToolSubMenu = new ToolSubMenu(this);
        ActionSubMenu = new ActionSubMenu(this);
    }

    public void Init()
    {
        ShowSaveOption = true;
    }

    public string EditorName => "Model Editor";
    public string Discription => "模型编辑器 Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";

    public void Update(float dt)
    {
        ViewportUsingKeyboard = Viewport.Update(Window, dt);

        /*
        if (ViewportHandler._flverhandle != null)
        {
            FlverResource r = ViewportHandler._flverhandle.Get();
            _universe.LoadFlverInModelEditor(r.Flver, ViewportHandler._renderMesh, ResourceHandler.CurrentFLVERInfo.ModelName);

            if (CFG.Current.Viewport_Enable_Texturing)
            {
                _universe.ScheduleTextureRefresh();
            }
        }
        */

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
        if (ImGui.BeginMenu("编辑 Edit"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"撤销 Undo", KeyBindings.Current.Core_Undo.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("撤销全部 Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("返回 Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ActionSubMenu.DisplayMenu();
        ToolSubMenu.DisplayMenu();

        if (ImGui.BeginMenu("显示 View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("视图 Viewport"))
            {
                CFG.Current.Interface_Editor_Viewport = !CFG.Current.Interface_Editor_Viewport;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_Editor_Viewport);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("模型 Model Hierarchy"))
            {
                CFG.Current.Interface_ModelEditor_ModelHierarchy = !CFG.Current.Interface_ModelEditor_ModelHierarchy;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ModelHierarchy);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("属性 Properties"))
            {
                CFG.Current.Interface_ModelEditor_Properties = !CFG.Current.Interface_ModelEditor_Properties;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Properties);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("资源浏览器 Asset Browser"))
            {
                CFG.Current.Interface_ModelEditor_AssetBrowser = !CFG.Current.Interface_ModelEditor_AssetBrowser;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_AssetBrowser);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("工具配置 Tool Configuration"))
            {
                CFG.Current.Interface_ModelEditor_ToolConfigurationWindow = !CFG.Current.Interface_ModelEditor_ToolConfigurationWindow;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ToolConfigurationWindow);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("分析 Profiling"))
            {
                CFG.Current.Interface_Editor_Profiling = !CFG.Current.Interface_Editor_Profiling;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_Editor_Profiling);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("资源列表 Resource List"))
            {
                CFG.Current.Interface_ModelEditor_ResourceList = !CFG.Current.Interface_ModelEditor_ResourceList;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ResourceList);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("视图网格 Viewport Grid"))
            {
                CFG.Current.Interface_ModelEditor_Viewport_Grid = !CFG.Current.Interface_ModelEditor_Viewport_Grid;
                CFG.Current.ModelEditor_Viewport_RegenerateMapGrid = true;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Viewport_Grid);

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("过滤器 Filters", RenderScene != null && Viewport != null))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("网 Meshes"))
            {
                CFG.Current.ModelEditor_ViewMeshes = !CFG.Current.ModelEditor_ViewMeshes;
                var container = _universe.LoadedModelContainers[ViewportHandler.ContainerID];
                foreach (var entry in container.Mesh_RootNode.Children)
                {
                    entry.EditorVisible = CFG.Current.ModelEditor_ViewMeshes;
                }
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.ModelEditor_ViewMeshes);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("占位 Dummy Polygons"))
            {
                CFG.Current.ModelEditor_ViewDummyPolys = !CFG.Current.ModelEditor_ViewDummyPolys;

                var container = _universe.LoadedModelContainers[ViewportHandler.ContainerID];
                foreach(var entry in container.DummyPoly_RootNode.Children)
                {
                    entry.EditorVisible = CFG.Current.ModelEditor_ViewDummyPolys;
                }
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.ModelEditor_ViewDummyPolys);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("骨骼 Bones"))
            {
                CFG.Current.ModelEditor_ViewBones = !CFG.Current.ModelEditor_ViewBones;
                var container = _universe.LoadedModelContainers[ViewportHandler.ContainerID];
                foreach (var entry in container.Bone_RootNode.Children)
                {
                    entry.EditorVisible = CFG.Current.ModelEditor_ViewBones;
                }
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.ModelEditor_ViewBones);

            /*
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Skeleton"))
            {
                CFG.Current.ModelEditor_ViewSkeleton = !CFG.Current.ModelEditor_ViewSkeleton;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.ModelEditor_ViewSkeleton);
            */

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("视图 Viewport"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.LightbulbO}");
            if (ImGui.BeginMenu("场景亮度 Scene Lighting"))
            {
                Viewport.SceneParamsGui();
                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("线框 Gizmos"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Compass}");
            if (ImGui.BeginMenu("模式 Mode"))
            {
                if (ImGui.MenuItem("平移 Translate", KeyBindings.Current.Viewport_TranslateMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Translate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Translate;
                }

                if (ImGui.MenuItem("旋转 Rotate", KeyBindings.Current.Viewport_RotationMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Rotate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Cube}");
            if (ImGui.BeginMenu("空间 Space"))
            {
                if (ImGui.MenuItem("本地 Local", KeyBindings.Current.Viewport_ToggleGizmoSpace.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.Local))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }

                if (ImGui.MenuItem("世界 World", KeyBindings.Current.Viewport_ToggleGizmoSpace.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.World))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Cubes}");
            if (ImGui.BeginMenu("原始 Origin"))
            {
                if (ImGui.MenuItem("世界 World", KeyBindings.Current.Viewport_ToggleGizmoOrigin.HintText,
                        Gizmos.Origin == Gizmos.GizmosOrigin.World))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }

                if (ImGui.MenuItem("绑定 Bounding Box", KeyBindings.Current.Viewport_ToggleGizmoOrigin.HintText,
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

        ActionSubMenu.Shortcuts();
        ToolSubMenu.Shortcuts();

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
        ModelHierarchy.OnGui();
        ModelPropertyEditor.OnGui();
        SkeletonHandler.OnGui();

        if (CFG.Current.Interface_ModelEditor_ToolConfigurationWindow)
        {
            ToolWindow.OnGui();
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
            ImGui.SetWindowFocus("Properties##ModelEditorProperties");

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
            ModelHierarchy.OnProjectChanged();
            ToolWindow.OnProjectChanged();
            ToolSubMenu.OnProjectChanged();
            ActionSubMenu.OnProjectChanged();
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

}
