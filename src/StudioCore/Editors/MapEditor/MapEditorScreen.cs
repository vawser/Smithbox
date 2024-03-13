using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.AssetLocator;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Viewport = StudioCore.Gui.Viewport;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor.Toolbar;

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
    public bool FirstFrame { get; set; }

    /// <summary>
    /// Current entity selection within the viewport.
    /// </summary>
    private ViewportSelection _selection = new();

    /// <summary>
    /// Active modal window.
    /// </summary>
    private IModal _activeModal;

    private int _createEntityMapIndex;
    private (string, ObjectContainer) _comboTargetMap = ("None", null);
    private (string, Entity) _dupeSelectionTargetedParent = ("None", null);

    private bool _PauseUpdate;
    private ProjectSettings _projectSettings;

    public bool AltHeld;
    public bool CtrlHeld;

    public ViewportActionManager EditorActionManager = new();

    public DisplayGroupEditor DispGroupEditor;
    public MapAssetBrowser AssetBrowser;
    public MapEditorToolbar MapEditorToolbar;
    public PrefabToolbar PrefabToolbar;

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

        SceneTree = new MapSceneTree(MapSceneTree.Configuration.MapEditor, this, "mapedittree", Universe, _selection, EditorActionManager, Viewport);
        DispGroupEditor = new DisplayGroupEditor(RenderScene, _selection, EditorActionManager);
        PropSearch = new MapSearchProperties(Universe, _propCache);
        NavMeshEditor = new NavmeshEditor(RenderScene, _selection);
        AssetBrowser = new MapAssetBrowser(Universe, RenderScene, _selection, EditorActionManager, this, Viewport);
        MapEditorToolbar = new MapEditorToolbar(RenderScene, _selection, EditorActionManager, Universe, Viewport);
        PrefabToolbar = new PrefabToolbar(RenderScene, _selection, EditorActionManager, Universe, Viewport, _comboTargetMap);
        PropEditor = new MapPropertyEditor(EditorActionManager, _propCache, Viewport);

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

    public void Init()
    {

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
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Undo", KeyBindings.Current.Core_Undo.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            if (ImGui.MenuItem("Delete", KeyBindings.Current.Core_Delete.HintText, false, _selection.IsSelection()))
            {
                DeleteMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.Core_Duplicate.HintText, false,
                    _selection.IsSelection()))
            {
                CloneMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            if (ImGui.BeginMenu("Duplicate to Map", _selection.IsSelection()))
            {
                DuplicateToTargetMapUI();
                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Display", RenderScene != null && Viewport != null))
        {
            if (ImGui.BeginMenu("Object Types"))
            {
                bool ticked;
                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Debug);
                if (ImGui.Checkbox("Debug", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.Debug);
                }

                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
                if (ImGui.Checkbox("Map Piece", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
                }

                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
                if (ImGui.Checkbox("Collision", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.Collision);
                }

                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
                if (ImGui.Checkbox("Object", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.Object);
                }

                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
                if (ImGui.Checkbox("Character", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.Character);
                }

                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
                if (ImGui.Checkbox("Navmesh", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
                }

                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
                if (ImGui.Checkbox("Region", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.Region);
                }

                ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
                if (ImGui.Checkbox("Light", ref ticked))
                {
                    RenderScene.ToggleDrawFilter(RenderFilter.Light);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Display Presets"))
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

            if (ImGui.BeginMenu("Scene Lighting"))
            {
                Viewport.SceneParamsGui();
                ImGui.EndMenu();
            }

            CFG.Current.LastSceneFilter = RenderScene.DrawFilter;
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Gizmos"))
        {
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

        // Keyboard shortcuts
        if (!ViewportUsingKeyboard && !ImGui.IsAnyItemActive())
        {
            var type = CFG.Current.Viewport_GridType;

            if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
            {
                EditorActionManager.UndoAction();
            }

            if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
            {
                EditorActionManager.RedoAction();
            }

            // Viewport Grid
            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_ViewportGrid_Lower))
            {
                var offset = CFG.Current.Viewport_Grid_Height;
                var increment = CFG.Current.Viewport_Grid_Height_Increment;
                offset = offset - increment;
                CFG.Current.Viewport_Grid_Height = offset;
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_ViewportGrid_Raise))
            {
                var offset = CFG.Current.Viewport_Grid_Height;
                var increment = CFG.Current.Viewport_Grid_Height_Increment;
                offset = offset + increment;
                CFG.Current.Viewport_Grid_Height = offset;
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_ViewportGrid_Bring_to_Selection))
            {
                MsbEntity sel = _selection.GetFilteredSelection<MsbEntity>().ToList().First();
                Vector3 pos = (Vector3)sel.GetPropertyValue("Position");
                CFG.Current.Viewport_Grid_Height = pos.Y;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate) && _selection.IsSelection())
            {
                CloneMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_DuplicateToMap) && _selection.IsSelection())
            {
                ImGui.OpenPopup("##DupeToTargetMapPopup");
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete) && _selection.IsSelection())
            {
                DeleteMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MsbEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_TranslateMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Translate;
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_RotationMode))
            {
                Gizmos.Mode = Gizmos.GizmosMode.Rotate;
            }

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

            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_ToggleGizmoSpace))
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

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Flip) && _selection.IsSelection())
            {
                MapAction_ToggleVisibility.ForceVisibilityState(false, false, true);
                MapAction_ToggleVisibility.Act(_selection);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Enabled) && _selection.IsSelection())
            {
                MapAction_ToggleVisibility.ForceVisibilityState(true, false, false);
                MapAction_ToggleVisibility.Act(_selection);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Disabled) && _selection.IsSelection())
            {
                MapAction_ToggleVisibility.ForceVisibilityState(false, true, false);
                MapAction_ToggleVisibility.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Flip))
            {
                MapAction_ToggleVisibility.ForceVisibilityState(false, false, true);
                MapAction_ToggleVisibility.Act(_selection);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Enabled))
            {
                MapAction_ToggleVisibility.ForceVisibilityState(true, false, false);
                MapAction_ToggleVisibility.Act(_selection);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Disabled))
            {
                MapAction_ToggleVisibility.ForceVisibilityState(false, true, false);
                MapAction_ToggleVisibility.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport))
            {
                MapAction_FrameInViewport.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Go_to_Selection_in_Object_List))
            {
                MapAction_GoToInObjectList.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Rotate_X))
            {
                MapAction_Rotate.ArbitraryRotation_Selection(_selection, new Vector3(1, 0, 0), false);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Rotate_Y))
            {
                MapAction_Rotate.ArbitraryRotation_Selection(_selection, new Vector3(0, 1, 0), false);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Rotate_Y_Pivot))
            {
                MapAction_Rotate.ArbitraryRotation_Selection(_selection, new Vector3(0, 1, 0), true);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Reset_Rotation))
            {
                MapAction_Rotate.SetSelectionToFixedRotation(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Dummify) && _selection.IsSelection())
            {
                if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                {
                    MapAction_TogglePresence.ER_UnDummySelection(_selection);
                }
                else
                {
                    MapAction_TogglePresence.UnDummySelection(_selection);
                }
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Undummify) && _selection.IsSelection())
            {
                if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                {
                    MapAction_TogglePresence.ER_DummySelection(_selection);
                }
                else
                {
                    MapAction_TogglePresence.DummySelection(_selection);
                }
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Move_Selection_to_Camera) && _selection.IsSelection())
            {
                MapAction_MoveToCamera.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_RenderEnemyPatrolRoutes))
            {
                PatrolDrawManager.Generate(Universe);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Set_to_Grid) && _selection.IsSelection())
            {
                MapAction_MoveToGrid.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Scramble) && _selection.IsSelection())
            {
                MapAction_Scramble.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Replicate) && _selection.IsSelection())
            {
                MapAction_Replicate.Act(_selection);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Create) && _selection.IsSelection())
            {
                MapAction_Create.Act(_selection);
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

        if (ImGui.BeginPopup("##DupeToTargetMapPopup"))
        {
            DuplicateToTargetMapUI();
            ImGui.EndPopup();
        }

        // Parse select commands
        string[] propSearchCmd = null;
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

            if (target != null)
            {
                Universe.Selection.ClearSelection();
                Universe.Selection.AddSelection(target);
                Universe.Selection.GotoTreeTarget = target;
                MapAction_FrameInViewport.Act(_selection);
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
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

        ResourceManager.OnGuiDrawTasks(Viewport.Width, Viewport.Height);
        ResourceManager.OnGuiDrawResourceList();

        DispGroupEditor.OnGui(Universe._dispGroupCount);
        AssetBrowser.OnGui();
        MapEditorToolbar.OnGui();
        PrefabToolbar.OnGui();

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

        // Focus on Properties by default when this editor is made focused
        if (FirstFrame)
        {
            ImGui.SetWindowFocus("Properties##mapeditprop");

            FirstFrame = false;
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

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;
        _selection.ClearSelection();
        EditorActionManager.Clear();
        PrefabToolbar.OnProjectChanged();

        ReloadUniverse();
    }

    public void Save()
    {
        if (Project.Type == ProjectType.AC6 && FeatureFlags.AC6_MSB_Saving == false)
        {
            TaskLogs.AddLog("AC6 map saving has been disabled.", LogLevel.Warning, TaskLogs.LogPriority.Normal);
        }
        else
        {
            try
            {
                Universe.SaveAllMaps();
            }
            catch (SavingFailedException e)
            {
                HandleSaveException(e);
            }
        }
    }

    public void SaveAll()
    {
        if (Project.Type == ProjectType.AC6 && FeatureFlags.AC6_MSB_Saving == false)
        {
            TaskLogs.AddLog("AC6 map saving has been disabled.", LogLevel.Warning, TaskLogs.LogPriority.Normal);
        }
        else
        {
            try
            {
                Universe.SaveAllMaps();
            }
            catch (SavingFailedException e)
            {
                HandleSaveException(e);
            }
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

    /// <summary>
    ///     Adds a new entity to the targeted map. If no parent is specified, RootObject will be used.
    /// </summary>
    private void AddNewEntity(Type typ, MsbEntity.MsbEntityType etype, MapContainer map, Entity parent = null)
    {
        var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        MsbEntity obj = new(map, newent, etype);

        parent ??= map.RootObject;

        AddMapObjectsAction act = new(Universe, map, RenderScene, new List<MsbEntity> { obj }, true, parent);
        EditorActionManager.ExecuteAction(act);
    }

    private void ComboTargetMapUI()
    {
        if (ImGui.BeginCombo("Targeted Map", _comboTargetMap.Item1))
        {
            foreach (var obj in Universe.LoadedObjectContainers)
            {
                if (obj.Value != null)
                {
                    if (ImGui.Selectable(obj.Key))
                    {
                        _comboTargetMap = (obj.Key, obj.Value);
                        break;
                    }
                }
            }
            ImGui.EndCombo();
        }
    }

    private void DuplicateToTargetMapUI()
    {
        ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), "Duplicate selection to specific map");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.5f), $" <{KeyBindings.Current.Map_DuplicateToMap.HintText}>");

        ComboTargetMapUI();
        if (_comboTargetMap.Item2 == null)
            return;

        MapContainer targetMap = (MapContainer)_comboTargetMap.Item2;

        var sel = _selection.GetFilteredSelection<MsbEntity>().ToList();

        if (sel.Any(e => e.WrappedObject is BTL.Light))
        {
            if (ImGui.BeginCombo("Targeted BTL", _dupeSelectionTargetedParent.Item1))
            {
                foreach (Entity btl in targetMap.BTLParents)
                {
                    var ad = (AssetDescription)btl.WrappedObject;
                    if (ImGui.Selectable(ad.AssetName))
                    {
                        _dupeSelectionTargetedParent = (ad.AssetName, btl);
                        break;
                    }
                }
                ImGui.EndCombo();
            }
            if (_dupeSelectionTargetedParent.Item2 == null)
                return;
        }

        if (ImGui.Button("Duplicate"))
        {
            Entity? targetParent = _dupeSelectionTargetedParent.Item2;

            var action = new CloneMapObjectsAction(Universe, RenderScene, sel, true, targetMap, targetParent);
            EditorActionManager.ExecuteAction(action);
            _comboTargetMap = ("None", null);
            _dupeSelectionTargetedParent = ("None", null);
            // Closes popup/menu bar
            ImGui.CloseCurrentPopup();
        }
    }

    public void ReloadUniverse()
    {
        Universe.UnloadAllMaps();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Universe.PopulateMapList();

        if (Project.Type != ProjectType.Undefined)
        {
            MapAction_Create.PopulateClassNames();
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
                            MapAction_FrameInViewport.Act(_selection);
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
