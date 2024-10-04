using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions;
using StudioCore.Editors.MapEditor.LightmapAtlasEditor;
using StudioCore.Editors.MapEditor.MapQuery;
using StudioCore.Editors.MapEditor.Tools;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Viewport = StudioCore.Interface.Viewport;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Main interface for the MSB Editor.
/// </summary>
public class MapEditorScreen : EditorScreen, SceneTreeEventHandler
{
    /// <summary>
    /// Lock variable used to handle pauses to the Update() function.
    /// </summary>
    private static readonly object _lock_PauseUpdate = new();
    
    /// <summary>
    /// Current entity selection within the viewport.
    /// </summary>
    public ViewportSelection _selection = new();

    /// <summary>
    /// Active modal window.
    /// </summary>
    private IModal _activeModal;

    private bool _PauseUpdate;

    public bool AltHeld;
    public bool CtrlHeld;

    public ViewportActionManager EditorActionManager = new();

    public DisplayGroupEditor DispGroupEditor;
    public MapAssetSelectionView MapAssetSelectionView;
    public SelectionGroupEditor SelectionGroupEditor;
    public PrefabEditor PrefabEditor;
    public LightmapAtlasScreen LightmapAtlasEditor;

    public GranularRegionToggleHandler GranularRegionHandler;

    private bool GCNeedsCollection;

    public Rectangle ModelViewerBounds;
    public NavmeshEditor NavMeshEditor;
    public MapPropertyEditor PropEditor;
    public MapSearchProperties PropSearch;
    private readonly MapPropertyCache _propCache = new();

    public Rectangle Rect;
    public RenderScene RenderScene;

    public MapSceneTree SceneTree;
    public bool ShiftHeld;

    public Universe Universe;
    public IViewport Viewport;

    private bool ViewportUsingKeyboard;

    private Sdl2Window Window;

    public List<string> WorldMap_ClickedMapZone = new List<string>();

    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;

    public ActionHandler ActionHandler;
    public ActionSubMenu ActionSubMenu;

    public MapQuerySearchEngine MapQueryHandler;

    public EditorFocusManager FocusManager;

    public MapEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
        Window = window;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport(ViewportType.MapEditor, "Mapeditvp", device, RenderScene, EditorActionManager, _selection, Rect.Width, Rect.Height);
            RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
        }
        else
        {
            Viewport = new NullViewport(ViewportType.MapEditor, "Mapeditvp", EditorActionManager, _selection, Rect.Width, Rect.Height);
        }

        Universe = new Universe(RenderScene, _selection);

        SceneTree = new MapSceneTree(this, MapSceneTree.Configuration.MapEditor, this, "mapedittree", Universe, _selection, EditorActionManager, Viewport);
        DispGroupEditor = new DisplayGroupEditor(RenderScene, _selection, EditorActionManager);
        PropSearch = new MapSearchProperties(Universe, _propCache);
        NavMeshEditor = new NavmeshEditor(RenderScene, _selection);
        MapAssetSelectionView = new MapAssetSelectionView(this);
        GranularRegionHandler = new GranularRegionToggleHandler(Universe);

        PropEditor = new MapPropertyEditor(this, EditorActionManager, _propCache, Viewport);

        LightmapAtlasEditor = new LightmapAtlasScreen(this);
        SelectionGroupEditor = new SelectionGroupEditor(Universe, RenderScene, _selection, EditorActionManager, this, Viewport);
        PrefabEditor = new() { universe = Universe, scene = RenderScene, actionManager = EditorActionManager };

        ActionHandler = new ActionHandler(this);
        ToolWindow = new ToolWindow(this, ActionHandler);
        ToolSubMenu = new ToolSubMenu(this, ActionHandler);
        ActionSubMenu = new ActionSubMenu(this, ActionHandler);

        MapQueryHandler = new MapQuerySearchEngine(this);

        FocusManager = new EditorFocusManager(this);
        FocusManager.SetDefaultFocusElement("Properties##mapeditprop");

        EditorActionManager.AddEventHandler(SceneTree);
    }

    private bool PauseUpdate
    {
        get
        {
            lock (_lock_PauseUpdate)
            {
                return _PauseUpdate;
            }
        }
        set
        {
            lock (_lock_PauseUpdate)
            {
                _PauseUpdate = value;
            }
        }
    }

    public string EditorName => "Map Editor";
    public string CommandEndpoint => "map";
    public string SaveType => "Maps";

    public void OnDefocus()
    {
        FocusManager.ResetFocus();
    }

    public void Update(float dt)
    {
        if (GCNeedsCollection)
        {
            GC.Collect();
            GCNeedsCollection = false;
        }

        if (PauseUpdate)
        {
            return;
        }

        ViewportUsingKeyboard = Viewport.Update(Window, dt);

        // Throw any exceptions that ocurred during async map loading.
        if (Universe.LoadMapExceptions != null)
        {
            Universe.LoadMapExceptions.Throw();
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Window = window;
        Rect = window.Bounds;
        //Viewport.ResizeViewport(device, new Rectangle(0, 0, window.Width, window.Height));
    }

    public void DrawEditorMenu()
    {
        ImGui.Separator();

        // Dropdown: Edit
        if (ImGui.BeginMenu("Edit"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}", false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Scissors}");
            if (ImGui.MenuItem("Remove", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText, false, _selection.IsSelection()))
            {
                DeleteMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.FilesO}");
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText, false,
                    _selection.IsSelection()))
            {
                CloneMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            ImGui.EndMenu();
        }

        // Actions
        ImGui.Separator();

        ActionSubMenu.DisplayMenu();

        ImGui.Separator();

        ToolSubMenu.DisplayMenu();

        ImGui.Separator();

        // Dropdown: View
        if (ImGui.BeginMenu("View"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport"))
            {
                UI.Current.Interface_Editor_Viewport = !UI.Current.Interface_Editor_Viewport;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Viewport);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Map Object List"))
            {
                UI.Current.Interface_MapEditor_MapObjectList = !UI.Current.Interface_MapEditor_MapObjectList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_MapObjectList);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_MapEditor_ToolWindow = !UI.Current.Interface_MapEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_ToolWindow);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Properties"))
            {
                UI.Current.Interface_MapEditor_Properties = !UI.Current.Interface_MapEditor_Properties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_Properties);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Asset Browser"))
            {
                UI.Current.Interface_MapEditor_AssetBrowser = !UI.Current.Interface_MapEditor_AssetBrowser;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_AssetBrowser);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Render Groups"))
            {
                UI.Current.Interface_MapEditor_RenderGroups = !UI.Current.Interface_MapEditor_RenderGroups;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_RenderGroups);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Profiling"))
            {
                UI.Current.Interface_Editor_Profiling = !UI.Current.Interface_Editor_Profiling;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Profiling);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Resource List"))
            {
                UI.Current.Interface_MapEditor_ResourceList = !UI.Current.Interface_MapEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_ResourceList);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport Grid"))
            {
                UI.Current.Interface_MapEditor_Viewport_Grid = !UI.Current.Interface_MapEditor_Viewport_Grid;
                CFG.Current.MapEditor_Viewport_RegenerateMapGrid = true;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_Viewport_Grid);

            if (Smithbox.ProjectType is ProjectType.DS3)
            {
                UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
                if (ImGui.MenuItem("Lightmap Atlas Editor"))
                {
                    UI.Current.Interface_MapEditor_Viewport_LightmapAtlas = !UI.Current.Interface_MapEditor_Viewport_LightmapAtlas;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_Viewport_LightmapAtlas);
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("General Filters", RenderScene != null && Viewport != null))
        {
            bool ticked;

            // Map Piece
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Map Piece"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
            UIHelper.ShowActiveStatus(ticked);

            // Collision
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Collision"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Collision);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
            UIHelper.ShowActiveStatus(ticked);

            // Object
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Object"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Object);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
            UIHelper.ShowActiveStatus(ticked);

            // Character
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Character"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Character);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
            UIHelper.ShowActiveStatus(ticked);

            // Navmesh
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Navmesh"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
            UIHelper.ShowActiveStatus(ticked);

            // Region
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Region"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Region);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
            UIHelper.ShowActiveStatus(ticked);

            // Light
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Light"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Light);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
            UIHelper.ShowActiveStatus(ticked);

            // Debug
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Debug"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Debug);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Debug);
            UIHelper.ShowActiveStatus(ticked);

            ImGui.Separator();

            GranularRegionHandler.DisplayOptions();

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Viewport", RenderScene != null && Viewport != null))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Filter}");
            if (ImGui.BeginMenu("Filter Presets"))
            {
                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_01.Name, "Ctrl+1"))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_01.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_02.Name, "Ctrl+2"))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_02.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_03.Name, "Ctrl+3"))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_03.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_04.Name, "Ctrl+4"))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_04.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_05.Name, "Ctrl+5"))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_05.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_06.Name, "Ctrl+6"))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_06.Filters;
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Cloud}");
            if (ImGui.BeginMenu("Environment Map"))
            {
                if (ImGui.MenuItem("Default"))
                {
                    Viewport.SetEnvMap(0);
                }

                foreach (var map in Universe.EnvMapTextures)
                {
                    if (ImGui.MenuItem(map))
                    {
                        /*var tex = ResourceManager.GetTextureResource($@"tex/{map}".ToLower());
                        if (tex.IsLoaded && tex.Get() != null && tex.TryLock())
                        {
                            if (tex.Get().GPUTexture.Resident)
                            {
                                Viewport.SetEnvMap(tex.Get().GPUTexture.TexHandle);
                            }
                            tex.Unlock();
                        }*/
                    }
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.LightbulbO}");
            if (ImGui.BeginMenu("Scene Lighting"))
            {
                Viewport.SceneParamsGui();
                ImGui.EndMenu();
            }

            if (Smithbox.ProjectType is ProjectType.ER)
            {
                UIHelper.ShowMenuIcon($"{ForkAwesome.Cube}");
                if (ImGui.BeginMenu("Collision Type"))
                {
                    UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
                    if (ImGui.MenuItem("Low"))
                    {
                        HavokCollisionManager.VisibleCollisionType = HavokCollisionType.Low;
                    }
                    UIHelper.ShowHoverTooltip("Visible collision will use the low-detail mesh.\nUsed for standard collision.\nMap must be reloaded after change to see difference.");
                    UIHelper.ShowActiveStatus(HavokCollisionManager.VisibleCollisionType == HavokCollisionType.Low);

                    UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
                    if (ImGui.MenuItem("High"))
                    {
                        HavokCollisionManager.VisibleCollisionType = HavokCollisionType.High;
                    }
                    UIHelper.ShowHoverTooltip("Visible collision will use the high-detail mesh.\nUsed for IK.\nMap must be reloaded after change to see difference.");
                    UIHelper.ShowActiveStatus(HavokCollisionManager.VisibleCollisionType == HavokCollisionType.High);

                    ImGui.EndMenu();
                }
            }

            CFG.Current.LastSceneFilter = RenderScene.DrawFilter;
            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Gizmos"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Compass}");
            if (ImGui.BeginMenu("Mode"))
            {
                if (ImGui.MenuItem("Translate", KeyBindings.Current.VIEWPORT_GizmoTranslationMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Translate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Translate;
                }

                if (ImGui.MenuItem("Rotate", KeyBindings.Current.VIEWPORT_GizmoRotationMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Rotate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Cube}");
            if (ImGui.BeginMenu("Space"))
            {
                if (ImGui.MenuItem("Local", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.Local))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }

                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.World))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Cubes}");
            if (ImGui.BeginMenu("Origin"))
            {
                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText,
                        Gizmos.Origin == Gizmos.GizmosOrigin.World))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }

                if (ImGui.MenuItem("Bounding Box", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText,
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
        var scale = DPI.GetUIScale();

        // Docking setup
        //var vp = ImGui.GetMainViewport();
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0.0f);
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                 ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        flags |= ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
        flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
        flags |= ImGuiWindowFlags.NoBackground;
        //ImGui.Begin("DockSpace_MapEdit", flags);
        ImGui.PopStyleVar(4);
        var dsid = ImGui.GetID("DockSpace_MapEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0));

        MapEditorShortcuts();
        MapEditorCommandLine(initcmd);

        if (ImGui.BeginPopup("##DupeToTargetMapPopup"))
        {
            ActionHandler.DisplayDuplicateToMapMenu();

            ImGui.EndPopup();
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
        //ImGui.Text($@"Viewport size: {Viewport.Width}x{Viewport.Height}");
        //ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

        Viewport.OnGui();

        SceneTree.OnGui();
        PropSearch.OnGui(propSearchCmd);

        if (Smithbox.FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        PropEditor.OnGui(_selection, "mapeditprop", Viewport.Width, Viewport.Height);

        // Not usable yet
        if (FeatureFlags.EnableNavmeshBuilder)
        {
            NavMeshEditor.OnGui();
        }

        if (LightmapAtlasEditor != null)
        {
            LightmapAtlasEditor.OnGui();
        }

        ResourceLoadWindow.DisplayWindow(Viewport.Width, Viewport.Height);
        if (UI.Current.Interface_MapEditor_ResourceList)
        {
            ResourceListWindow.DisplayWindow("mapResourceList");
        }

        DispGroupEditor.OnGui(Universe._dispGroupCount);
        MapAssetSelectionView.OnGui();
        SelectionGroupEditor.OnGui();

        if (UI.Current.Interface_MapEditor_ToolWindow)
        {
            ToolWindow.OnGui();
        }

        if (_activeModal != null)
        {
            if (_activeModal.IsClosed)
            {
                _activeModal.OpenModal();
            }

            _activeModal.OnGui();
            if (_activeModal.IsClosed)
            {
                _activeModal = null;
            }
        }

        ImGui.PopStyleColor(1);

        FocusManager.OnFocus();
    }

    private string[] propSearchCmd = null;

    public void MapEditorCommandLine(string[] initcmd)
    {
        // Parse select commands
        if (initcmd != null && initcmd.Length > 1)
        {
            if (initcmd[0] == "propsearch")
            {
                propSearchCmd = initcmd.Skip(1).ToArray();
                PropSearch.Property = PropEditor.RequestedSearchProperty;
                PropEditor.RequestedSearchProperty = null;
            }

            // Support loading maps through commands.
            // Probably don't support unload here, as there may be unsaved changes.
            ISelectable target = null;
            if (initcmd[0] == "load")
            {
                var mapid = initcmd[1];
                if (Universe.GetLoadedMap(mapid) is MapContainer m)
                {
                    target = m.RootObject;
                }
                else
                {
                    Universe.LoadMap(mapid, true);
                }
            }

            if (initcmd[0] == "select")
            {
                var mapid = initcmd[1];
                if (initcmd.Length > 2)
                {
                    if (Universe.GetLoadedMap(mapid) is MapContainer m)
                    {
                        var name = initcmd[2];
                        if (initcmd.Length > 3 && Enum.TryParse(initcmd[3], out MsbEntity.MsbEntityType entityType))
                        {
                            target = m.GetObjectsByName(name)
                                .Where(ent => ent is MsbEntity me && me.Type == entityType)
                                .FirstOrDefault();
                        }
                        else
                        {
                            target = m.GetObjectByName(name);
                        }
                    }
                }
                else
                {
                    target = new ObjectContainerReference(mapid, Universe).GetSelectionTarget();
                }
            }

            if (initcmd[0] == "idselect")
            {
                var type = initcmd[1];
                var mapid = initcmd[2];
                var entityID = initcmd[3];

                if (initcmd.Length > 3)
                {
                    if (Universe.GetLoadedMap(mapid) is MapContainer m)
                    {
                        if (type == "enemy")
                        {
                            target = m.GetEnemyByID(entityID);
                        }
                        if (type == "asset")
                        {
                            target = m.GetAssetByID(entityID);
                        }
                        if (type == "region")
                        {
                            target = m.GetRegionByID(entityID);
                        }
                    }
                }
            }

            if (initcmd[0] == "emevd_select")
            {
                var mapid = initcmd[1];
                var entityID = initcmd[2];

                if (initcmd.Length > 2)
                {
                    if (Universe.GetLoadedMap(mapid) is MapContainer m)
                    {
                        if (target == null)
                            target = m.GetEnemyByID(entityID, true);

                        if(target == null)
                            target = m.GetAssetByID(entityID);

                        if (target == null)
                            target = m.GetRegionByID(entityID);

                        if (target == null)
                            target = m.GetCollisionByID(entityID);
                    }
                }
            }

            if (target != null)
            {
                Universe.Selection.ClearSelection();
                Universe.Selection.AddSelection(target);
                Universe.Selection.GotoTreeTarget = target;
                ActionHandler.ApplyFrameInViewport();
            }
        }
    }

    public void MapEditorShortcuts()
    {
        // Keyboard shortcuts
        if (!ViewportUsingKeyboard && !ImGui.IsAnyItemActive())
        {
            var type = CFG.Current.MapEditor_Viewport_GridType;

            if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
            {
                EditorActionManager.UndoAction();
            }

            if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
            {
                EditorActionManager.UndoAction();
            }

            if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
            {
                EditorActionManager.RedoAction();
            }

            if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
            {
                EditorActionManager.RedoAction();
            }

            // Viewport Grid
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_LowerGrid))
            {
                var offset = CFG.Current.MapEditor_Viewport_Grid_Height;
                var increment = CFG.Current.MapEditor_Viewport_Grid_Height_Increment;
                offset = offset - increment;
                CFG.Current.MapEditor_Viewport_Grid_Height = offset;
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RaiseGrid))
            {
                var offset = CFG.Current.MapEditor_Viewport_Grid_Height;
                var increment = CFG.Current.MapEditor_Viewport_Grid_Height_Increment;
                offset = offset + increment;
                CFG.Current.MapEditor_Viewport_Grid_Height = offset;
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_SetGridToSelectionHeight))
            {
                var tempList = _selection.GetFilteredSelection<MsbEntity>().ToList();
                if (tempList != null && tempList.Count > 0)
                {
                    MsbEntity sel = tempList.First();
                    Vector3 pos = (Vector3)sel.GetPropertyValue("Position");
                    CFG.Current.MapEditor_Viewport_Grid_Height = pos.Y;
                }
            }

            ActionSubMenu.Shortcuts();
            ToolSubMenu.Shortcuts();

            // Gizmos
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoTranslationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Translate;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoRotationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Rotate;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoOriginMode))
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

            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_GizmoSpaceMode))
            {
                if (Gizmos.Space == Gizmos.GizmosSpace.Local)
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }
                else if (Gizmos.Space == Gizmos.GizmosSpace.World)
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }
            }

            // Render settings
            if (RenderScene != null)
            {
                if (InputTracker.GetControlShortcut(Key.Number1))
                {
                    RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number2))
                {
                    RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number3))
                {
                    RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.Object | RenderFilter.Character |
                                             RenderFilter.Region;
                }
                else if (InputTracker.GetControlShortcut(Key.Number4))
                {
                    RenderScene.DrawFilter = RenderFilter.MapPiece | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number5))
                {
                    RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Light;
                }
                else if (InputTracker.GetControlShortcut(Key.Number6))
                {
                    RenderScene.DrawFilter = RenderFilter.Collision | RenderFilter.Navmesh |
                                             RenderFilter.MapPiece | RenderFilter.Collision |
                                             RenderFilter.Navmesh | RenderFilter.Object |
                                             RenderFilter.Character | RenderFilter.Region |
                                             RenderFilter.Light;
                }

                CFG.Current.LastSceneFilter = RenderScene.DrawFilter;
            }
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Viewport != null)
        {
            Viewport.Draw(device, cl);
        }
    }

    public bool InputCaptured()
    {
        return Viewport.ViewportSelected;
    }

    public void OnProjectChanged()
    {
        _selection.ClearSelection();
        EditorActionManager.Clear();

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            MapQueryHandler.OnProjectChanged();
            SelectionGroupEditor.OnProjectChanged();
            MapAssetSelectionView.OnProjectChanged();
            SceneTree.OnProjectChanged();
            GranularRegionHandler.OnProjectChanged();
            PrefabEditor.OnProjectChanged();
            ToolWindow.OnProjectChanged();
            ToolSubMenu.OnProjectChanged();
            ActionSubMenu.OnProjectChanged();
        }

        ReloadUniverse();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        try
        {
            Universe.SaveAllMaps();
            LightmapAtlasEditor.Save();
        }
        catch (SavingFailedException e)
        {
            HandleSaveException(e);
        }
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        try
        {
            Universe.SaveAllMaps();
        }
        catch (SavingFailedException e)
        {
            HandleSaveException(e);
        }
    }

    public void OnEntityContextMenu(Entity ent)
    {
        /*
        if (ImGui.Selectable("Create prefab"))
        {
            _activeModal = new CreatePrefabModal(Universe, ent);
        }
        */
    }

    public void ReloadUniverse()
    {
        Universe.UnloadAllMaps();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Universe.PopulateMapList();

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ActionHandler.PopulateClassNames();
        }
    }

    public void HandleSaveException(SavingFailedException e)
    {
        if (e.Wrapped is MSB.MissingReferenceException eRef)
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, TaskLogs.LogPriority.Normal, e.Wrapped);

            DialogResult result = PlatformUtils.Instance.MessageBox($"{eRef.Message}\nSelect referring map entity?",
                "Failed to save map",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);
            if (result == DialogResult.Yes)
            {
                foreach (KeyValuePair<string, ObjectContainer> map in Universe.LoadedObjectContainers.Where(e =>
                             e.Value != null))
                {
                    foreach (Entity obj in map.Value.Objects)
                    {
                        if (obj.WrappedObject == eRef.Referrer)
                        {
                            _selection.ClearSelection();
                            _selection.AddSelection(obj);
                            ActionHandler.ApplyFrameInViewport();
                            return;
                        }
                    }
                }

                TaskLogs.AddLog($"Unable to find map entity \"{eRef.Referrer.Name}\"",
                    LogLevel.Error, TaskLogs.LogPriority.High);
            }
        }
        else
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, TaskLogs.LogPriority.High, e.Wrapped);
        }
    }
}
