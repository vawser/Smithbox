using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Aliases;
using StudioCore.Browsers;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Utilities;
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

namespace StudioCore.MsbEditor;

/// <summary>
/// Main interface for the MSB Editor.
/// </summary>
public class MsbEditorScreen : EditorScreen, SceneTreeEventHandler
{
    /// <summary>
    /// Lock variable used to handle pauses to the Update() function.
    /// </summary>
    private static readonly object _lock_PauseUpdate = new();

    /// <summary>
    /// Current entity selection within the viewport.
    /// </summary>
    private Selection _selection = new();

    /// <summary>
    /// Asset locator for game files.
    /// </summary>
    public readonly AssetLocator AssetLocator;

    /// <summary>
    /// Active modal window.
    /// </summary>
    private IModal _activeModal;

    private int _createEntityMapIndex;
    private (string, ObjectContainer) _dupeSelectionTargetedMap = ("None", null);
    private (string, Entity) _dupeSelectionTargetedParent = ("None", null);

    private bool _PauseUpdate;
    private ProjectSettings _projectSettings;

    public bool AltHeld;
    public bool CtrlHeld;

    public ActionManager EditorActionManager = new();

    public DisplayGroupsEditor DispGroupEditor;
    public MapAssetBrowser AssetBrowser;
    public MsbToolbar Toolbar;

    private bool GCNeedsCollection;

    public Rectangle ModelViewerBounds;
    public NavmeshEditor NavMeshEditor;
    public PropertyEditor PropEditor;
    public SearchProperties PropSearch;
    private readonly PropertyCache _propCache = new();

    public Rectangle Rect;
    public RenderScene RenderScene;

    public SceneTree SceneTree;
    public bool ShiftHeld;

    public Universe Universe;
    public IViewport Viewport;

    private bool ViewportUsingKeyboard;

    private Sdl2Window Window;

    private AliasBank _modelAliasBank;
    private AliasBank _mapAliasBank;

    public MsbEditorScreen(Sdl2Window window, GraphicsDevice device, AssetLocator locator, AliasBank modelAliasBank, AliasBank mapAliasBank)
    {
        Rect = window.Bounds;
        AssetLocator = locator;
        ResourceManager.Locator = AssetLocator;
        Window = window;

        _modelAliasBank = modelAliasBank;
        _mapAliasBank = mapAliasBank;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport("Mapeditvp", device, RenderScene, EditorActionManager, _selection, Rect.Width, Rect.Height);
            RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
        }
        else
        {
            Viewport = new NullViewport("Mapeditvp", EditorActionManager, _selection, Rect.Width, Rect.Height);
        }

        Universe = new Universe(AssetLocator, RenderScene, _selection);

        SceneTree = new SceneTree(SceneTree.Configuration.MapEditor, this, "mapedittree", Universe, _selection, EditorActionManager, Viewport, AssetLocator, _modelAliasBank, _mapAliasBank);
        PropEditor = new PropertyEditor(EditorActionManager, _propCache, _mapAliasBank);
        DispGroupEditor = new DisplayGroupsEditor(RenderScene, _selection, EditorActionManager);
        PropSearch = new SearchProperties(Universe, _propCache);
        NavMeshEditor = new NavmeshEditor(locator, RenderScene, _selection);
        AssetBrowser = new MapAssetBrowser(Universe, RenderScene, _selection, EditorActionManager, AssetLocator, this, Viewport, _modelAliasBank, _mapAliasBank);
        Toolbar = new MsbToolbar(RenderScene, _selection, EditorActionManager, Universe, AssetLocator, this, Viewport);

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

            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            if (ImGui.MenuItem("Delete", KeyBindings.Current.Core_Delete.HintText, false, _selection.IsSelection()))
            {
                DeleteMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MapEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.Core_Duplicate.HintText, false,
                    _selection.IsSelection()))
            {
                CloneMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MapEntity>().ToList(), true, AssetLocator);
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
            /*
            // Does nothing at the moment. Maybe add to settings menu if this is ever implemented
            if (ImGui.MenuItem("Grid", "", Viewport.DrawGrid))
            {
                Viewport.DrawGrid = !Viewport.DrawGrid;
            }
            */
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

        if (ImGui.BeginMenu("Tools"))
        {
            var loadedMaps = Universe.LoadedObjectContainers.Values.Where(x => x != null);

            if (ImGui.MenuItem("Check loaded maps for duplicate Entity IDs", loadedMaps.Any()))
            {
                HashSet<uint> vals = new();
                string badVals = "";
                foreach (var loadedMap in loadedMaps)
                {
                    foreach (var e in loadedMap?.Objects)
                    {
                        var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);
                        if (val == null)
                            continue;

                        uint entUint;
                        if (val is int entInt)
                            entUint = (uint)entInt;
                        else
                            entUint = (uint)val;

                        if (entUint == 0 || entUint == uint.MaxValue)
                            continue;
                        if (!vals.Add(entUint))
                            badVals += $"   Duplicate entity ID: {entUint}\n";
                    }
                }
                if (badVals != "")
                {
                    TaskLogs.AddLog("Duplicate entity IDs found across loaded maps (see logger)", LogLevel.Information, TaskLogs.LogPriority.High);
                    TaskLogs.AddLog("Duplicate entity IDs found:\n" + badVals[..^1], LogLevel.Information, TaskLogs.LogPriority.Low);
                }
                else
                {
                    TaskLogs.AddLog("No duplicate entity IDs found", LogLevel.Information, TaskLogs.LogPriority.Normal);
                }
            }

            if (AssetLocator.Type is GameType.DemonsSouls or
                GameType.DarkSoulsPTDE or GameType.DarkSoulsRemastered)
            {
                if (ImGui.BeginMenu("Regenerate MCP and MCG"))
                {
                    GenerateMCGMCP(Universe.LoadedObjectContainers);

                    ImGui.EndMenu();
                }
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

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate) && _selection.IsSelection())
            {
                CloneMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MapEntity>().ToList(), true, AssetLocator);
                EditorActionManager.ExecuteAction(action);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_DuplicateToMap) && _selection.IsSelection())
            {
                ImGui.OpenPopup("##DupeToTargetMapPopup");
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete) && _selection.IsSelection())
            {
                DeleteMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MapEntity>().ToList(), true);
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
                Toolbar.ForceVisibilityState(false, false, true);
                Toolbar.ToggleEntityVisibility();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Enabled) && _selection.IsSelection())
            {
                Toolbar.ForceVisibilityState(true, false, false);
                Toolbar.ToggleEntityVisibility();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Selection_Visibility_Disabled) && _selection.IsSelection())
            {
                Toolbar.ForceVisibilityState(false, true, false);
                Toolbar.ToggleEntityVisibility();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Flip))
            {
                Toolbar.ForceVisibilityState(false, false, true);
                Toolbar.ToggleEntityVisibility();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Enabled))
            {
                Toolbar.ForceVisibilityState(true, false, false);
                Toolbar.ToggleEntityVisibility();
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Toggle_Map_Visibility_Disabled))
            {
                Toolbar.ForceVisibilityState(false, true, false);
                Toolbar.ToggleEntityVisibility();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport))
            {
                Toolbar.FrameSelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Go_to_Selection_in_Object_List))
            {
                Toolbar.GoToInObjectList();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Rotate_X))
            {
                Toolbar.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Rotate_Y))
            {
                Toolbar.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Rotate_Y_Pivot))
            {
                Toolbar.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Reset_Rotation))
            {
                Toolbar.SetSelectionToFixedRotation();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Dummify) && _selection.IsSelection())
            {
                if(CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                    Toolbar.ER_UnDummySelection();
                else
                    Toolbar.UnDummySelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Undummify) && _selection.IsSelection())
            {
                if (CFG.Current.Toolbar_Presence_Dummy_Type_ER)
                    Toolbar.ER_DummySelection();
                else
                    Toolbar.DummySelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Move_Selection_to_Camera) && _selection.IsSelection())
            {
                Toolbar.MoveSelectionToCamera();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_RenderEnemyPatrolRoutes))
            {
                PatrolDrawManager.Generate(Universe);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Set_to_Grid) && _selection.IsSelection())
            {
                Toolbar.MoveSelectionToGrid();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Scramble) && _selection.IsSelection())
            {
                Toolbar.ScambleSelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Replicate) && _selection.IsSelection())
            {
                Toolbar.ReplicateSelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Toolbar_Create) && _selection.IsSelection())
            {
                Toolbar.CreateNewMapObject();
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
                if (Universe.GetLoadedMap(mapid) is Map m)
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
                    if (Universe.GetLoadedMap(mapid) is Map m)
                    {
                        var name = initcmd[2];
                        if (initcmd.Length > 3 && Enum.TryParse(initcmd[3], out MapEntity.MapEntityType entityType))
                        {
                            target = m.GetObjectsByName(name)
                                .Where(ent => ent is MapEntity me && me.Type == entityType)
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
                Toolbar.FrameSelection();
            }
        }

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
            NavMeshEditor.OnGui(AssetLocator.Type);
        }

        ResourceManager.OnGuiDrawTasks(Viewport.Width, Viewport.Height);
        ResourceManager.OnGuiDrawResourceList();

        DispGroupEditor.OnGui(Universe._dispGroupCount);
        AssetBrowser.OnGui();
        Toolbar.OnGui();

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

        ReloadUniverse();
    }

    public void Save()
    {
        if (AssetLocator.Type == GameType.ArmoredCoreVI && FeatureFlags.AC6_MSB_Saving == false)
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
        if (AssetLocator.Type == GameType.ArmoredCoreVI && FeatureFlags.AC6_MSB_Saving == false)
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
        if (ImGui.Selectable("Create prefab"))
        {
            _activeModal = new CreatePrefabModal(Universe, ent);
        }
    }

    /// <summary>
    ///     Adds a new entity to the targeted map. If no parent is specified, RootObject will be used.
    /// </summary>
    private void AddNewEntity(Type typ, MapEntity.MapEntityType etype, Map map, Entity parent = null)
    {
        var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        MapEntity obj = new(map, newent, etype);

        parent ??= map.RootObject;

        AddMapObjectsAction act = new(Universe, map, RenderScene, new List<MapEntity> { obj }, true, parent);
        EditorActionManager.ExecuteAction(act);
    }  

    private void DuplicateToTargetMapUI()
    {
        ImGui.Text("Duplicate selection to specific map");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.5f),
            $" <{KeyBindings.Current.Map_DuplicateToMap.HintText}>");

        if (ImGui.BeginCombo("Targeted Map", _dupeSelectionTargetedMap.Item1))
        {
            foreach (KeyValuePair<string, ObjectContainer> obj in Universe.LoadedObjectContainers)
            {
                if (obj.Value != null)
                {
                    if (ImGui.Selectable(obj.Key))
                    {
                        _dupeSelectionTargetedMap = (obj.Key, obj.Value);
                        break;
                    }
                }
            }

            ImGui.EndCombo();
        }

        if (_dupeSelectionTargetedMap.Item2 == null)
        {
            return;
        }

        var targetMap = (Map)_dupeSelectionTargetedMap.Item2;

        List<MapEntity> sel = _selection.GetFilteredSelection<MapEntity>().ToList();

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
            {
                return;
            }
        }

        if (ImGui.Button("Duplicate"))
        {
            Entity? targetParent = _dupeSelectionTargetedParent.Item2;

            CloneMapObjectsAction action = new(Universe, RenderScene, sel, true, AssetLocator, targetMap, targetParent);
            EditorActionManager.ExecuteAction(action);
            _dupeSelectionTargetedMap = ("None", null);
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

        if (AssetLocator.Type != GameType.Undefined)
        {
            Toolbar.PopulateClassNames(AssetLocator.Type);
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
                            Toolbar.FrameSelection();
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

    private void GenerateMCGMCP(Dictionary<string, ObjectContainer> orderedMaps)
    {
        if (ImGui.BeginCombo("Regenerate MCP and MCG", "Maps"))
        {
            HashSet<string> idCache = new();
            foreach (var map in orderedMaps)
            {
                string mapid = map.Key;
                if (AssetLocator.Type is GameType.DemonsSouls)
                {
                    if (mapid != "m03_01_00_99" && !mapid.StartsWith("m99"))
                    {
                        var areaId = mapid.Substring(0, 3);
                        if (idCache.Contains(areaId))
                            continue;
                        idCache.Add(areaId);

                        if (ImGui.Selectable($"{areaId}"))
                        {
                            List<string> areaDirectories = new List<string>();
                            foreach (var orderMap in orderedMaps)
                            {
                                if (orderMap.Key.StartsWith(areaId) && orderMap.Key != "m03_01_00_99")
                                {
                                    areaDirectories.Add(Path.Combine(AssetLocator.GameRootDirectory, "map", orderMap.Key));
                                }
                            }
                            SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, AssetLocator, toBigEndian: true);
                        }
                    }
                    else
                    {
                        if (ImGui.Selectable($"{mapid}"))
                        {
                            List<string> areaDirectories = new List<string>
                            {
                                Path.Combine(AssetLocator.GameRootDirectory, "map", mapid)
                            };
                            SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, AssetLocator, toBigEndian: true);
                        }
                    }
                }
                else if (AssetLocator.Type is GameType.DarkSoulsPTDE or GameType.DarkSoulsRemastered)
                {
                    if (ImGui.Selectable($"{mapid}"))
                    {
                        List<string> areaDirectories = new List<string>
                        {
                            Path.Combine(AssetLocator.GameRootDirectory, "map", mapid)
                        };
                        SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, AssetLocator, toBigEndian: false);
                    }
                }
            }
            ImGui.EndCombo();
        }
    }
}
