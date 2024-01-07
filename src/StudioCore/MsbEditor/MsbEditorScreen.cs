using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Viewport = StudioCore.Gui.Viewport;

namespace StudioCore.MsbEditor;

public class MsbEditorScreen : EditorScreen, SceneTreeEventHandler
{
    private const int RECENT_FILES_MAX = 32;

    private static readonly object _lock_PauseUpdate = new();
    private readonly Selection _selection = new();

    public readonly AssetLocator AssetLocator;

    private IModal _activeModal;

    private int _createEntityMapIndex;
    private (string, ObjectContainer) _dupeSelectionTargetedMap = ("None", null);
    private (string, Entity) _dupeSelectionTargetedParent = ("None", null);
    private List<(string, Type)> _eventClasses = new();

    private List<(string, Type)> _partsClasses = new();
    private bool _PauseUpdate;
    private ProjectSettings _projectSettings;
    private List<(string, Type)> _regionClasses = new();
    public bool AltHeld;

    public bool CtrlHeld;
    public DisplayGroupsEditor DispGroupEditor;
    public ActionManager EditorActionManager = new();

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

    public MsbEditorScreen(Sdl2Window window, GraphicsDevice device, AssetLocator locator)
    {
        Rect = window.Bounds;
        AssetLocator = locator;
        ResourceManager.Locator = AssetLocator;
        Window = window;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport("Mapeditvp", device, RenderScene, EditorActionManager, _selection, Rect.Width,
                Rect.Height);
            RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
        }
        else
        {
            Viewport = new NullViewport("Mapeditvp", EditorActionManager, _selection, Rect.Width, Rect.Height);
        }

        Universe = new Universe(AssetLocator, RenderScene, _selection);

        SceneTree = new SceneTree(SceneTree.Configuration.MapEditor, this, "mapedittree", Universe, _selection,
            EditorActionManager, Viewport, AssetLocator);
        PropEditor = new PropertyEditor(EditorActionManager, _propCache);
        DispGroupEditor = new DisplayGroupsEditor(RenderScene, _selection, EditorActionManager);
        PropSearch = new SearchProperties(Universe, _propCache);
        NavMeshEditor = new NavmeshEditor(locator, RenderScene, _selection);

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
                    _selection.GetFilteredSelection<MapEntity>().ToList(), true);
                EditorActionManager.ExecuteAction(action);
            }

            if (ImGui.BeginMenu("Duplicate to Map", _selection.IsSelection()))
            {
                DuplicateToTargetMapUI();
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Dummify/Un-Dummify"))
            {
                if (ImGui.MenuItem("Dummify Enemies/Objects/Assets", KeyBindings.Current.Map_Dummify.HintText,
                        false, _selection.IsSelection()))
                {
                    DummySelection();
                }

                if (ImGui.MenuItem("Un-Dummify Enemies/Objects/Assets", KeyBindings.Current.Map_UnDummify.HintText,
                        false, _selection.IsSelection()))
                {
                    UnDummySelection();
                }

                //ImGui.TextColored(new Vector4(1f, .4f, 0f, 1f), "Warning: Converting Assets to Dummy Assets will result in lost property data (Undo will properly restore data)");
                ImGui.EndMenu();
            }

            //
            ImGui.Separator(); // Visual options goes below here

            if (ImGui.BeginMenu("Hide/Unhide"))
            {
                if (ImGui.MenuItem("Hide/Unhide", KeyBindings.Current.Map_HideToggle.HintText, false,
                        _selection.IsSelection()))
                {
                    HideShowSelection();
                }

                ObjectContainer loadedMap = Universe.LoadedObjectContainers.Values.FirstOrDefault(x => x != null);
                if (ImGui.MenuItem("Unhide All", KeyBindings.Current.Map_UnhideAll.HintText, false,
                        loadedMap != null))
                {
                    UnhideAllObjects();
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Frame in Viewport", KeyBindings.Current.Viewport_FrameSelection.HintText, false,
                    _selection.IsSelection()))
            {
                FrameSelection();
            }

            if (ImGui.MenuItem("Goto in Object List", KeyBindings.Current.Map_GotoSelectionInObjectList.HintText,
                    false, _selection.IsSelection()))
            {
                GotoSelection();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Manipulate Selection"))
            {
                if (ImGui.MenuItem("Reset Rotation", KeyBindings.Current.Map_ResetRotation.HintText, false,
                        _selection.IsSelection()))
                {
                    ResetRotationSelection();
                }

                if (ImGui.MenuItem("Arbitrary Rotation: Roll",
                        KeyBindings.Current.Map_ArbitraryRotation_Roll.HintText, false, _selection.IsSelection()))
                {
                    ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
                }

                if (ImGui.MenuItem("Arbitrary Rotation: Yaw",
                        KeyBindings.Current.Map_ArbitraryRotation_Yaw.HintText, false, _selection.IsSelection()))
                {
                    ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
                }

                if (ImGui.MenuItem("Arbitrary Rotation: Yaw Pivot",
                        KeyBindings.Current.Map_ArbitraryRotation_Yaw_Pivot.HintText, false,
                        _selection.IsSelection()))
                {
                    ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
                }

                if (ImGui.MenuItem("Move Selection to Camera",
                        KeyBindings.Current.Map_MoveSelectionToCamera.HintText, false, _selection.IsSelection()))
                {
                    MoveSelectionToCamera();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Create"))
        {
            IEnumerable<ObjectContainer> loadedMaps = Universe.LoadedObjectContainers.Values.Where(x => x != null);
            if (!loadedMaps.Any())
            {
                ImGui.Text("No maps loaded");
            }
            else
            {
                if (_createEntityMapIndex >= loadedMaps.Count())
                {
                    _createEntityMapIndex = 0;
                }

                ImGui.Combo("Target Map", ref _createEntityMapIndex, loadedMaps.Select(e => e.Name).ToArray(),
                    loadedMaps.Count());

                var map = (Map)loadedMaps.ElementAt(_createEntityMapIndex);

                if (ImGui.BeginMenu("BTL Lights"))
                {
                    if (!map.BTLParents.Any())
                    {
                        ImGui.Text("This map has no BTL files.");
                    }
                    else
                    {
                        foreach (Entity btl in map.BTLParents)
                        {
                            var ad = (AssetDescription)btl.WrappedObject;
                            if (ImGui.BeginMenu(ad.AssetName))
                            {
                                if (ImGui.MenuItem("Create Light"))
                                {
                                    AddNewEntity(typeof(BTL.Light), MapEntity.MapEntityType.Light, map, btl);
                                }

                                ImGui.EndMenu();
                            }
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Parts"))
                {
                    foreach ((string, Type) p in _partsClasses)
                    {
                        if (ImGui.MenuItem(p.Item1))
                        {
                            AddNewEntity(p.Item2, MapEntity.MapEntityType.Part, map);
                        }
                    }

                    ImGui.EndMenu();
                }

                // Some games only have one single region class
                if (_regionClasses.Count == 1)
                {
                    if (ImGui.MenuItem("Region"))
                    {
                        AddNewEntity(_regionClasses[0].Item2, MapEntity.MapEntityType.Region, map);
                    }
                }
                else
                {
                    if (ImGui.BeginMenu("Regions"))
                    {
                        foreach ((string, Type) p in _regionClasses)
                        {
                            if (ImGui.MenuItem(p.Item1))
                            {
                                AddNewEntity(p.Item2, MapEntity.MapEntityType.Region, map);
                            }
                        }

                        ImGui.EndMenu();
                    }
                }

                if (ImGui.BeginMenu("Events"))
                {
                    foreach ((string, Type) p in _eventClasses)
                    {
                        if (ImGui.MenuItem(p.Item1))
                        {
                            AddNewEntity(p.Item2, MapEntity.MapEntityType.Event, map);
                        }
                    }

                    ImGui.EndMenu();
                }
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
        var scale = MapStudioNew.GetUIScale();

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
            if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
            {
                EditorActionManager.UndoAction();
            }

            if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
            {
                EditorActionManager.RedoAction();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate) && _selection.IsSelection())
            {
                CloneMapObjectsAction action = new(Universe, RenderScene,
                    _selection.GetFilteredSelection<MapEntity>().ToList(), true);
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

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_HideToggle) && _selection.IsSelection())
            {
                HideShowSelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_UnhideAll))
            {
                UnhideAllObjects();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Viewport_FrameSelection))
            {
                FrameSelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_GotoSelectionInObjectList))
            {
                GotoSelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_ArbitraryRotation_Roll))
            {
                ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_ArbitraryRotation_Yaw))
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_ArbitraryRotation_Yaw_Pivot))
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_Dummify) && _selection.IsSelection())
            {
                UnDummySelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_UnDummify) && _selection.IsSelection())
            {
                DummySelection();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_MoveSelectionToCamera) && _selection.IsSelection())
            {
                MoveSelectionToCamera();
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
                FrameSelection();
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
        if (MapStudioNew.FirstFrame)
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
        try
        {
            Universe.SaveAllMaps();
        }
        catch (SavingFailedException e)
        {
            HandleSaveException(e);
        }
    }

    public void SaveAll()
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

    public void OnEntityContextMenu(Entity ent)
    {
        if (ImGui.Selectable("Create prefab"))
        {
            _activeModal = new CreatePrefabModal(Universe, ent);
        }
    }

    public void FrameSelection()
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
            else if (s.Container.RootObject == s)
            {
                // Selection is transform node
                Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                Vector3 pos = s.GetLocalTransform().Position;
                BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                if (!first)
                {
                    first = true;
                    box = nodeBox;
                }
                else
                {
                    box = BoundingBox.Combine(box, nodeBox);
                }
            }
        }

        if (first)
        {
            Viewport.FrameBox(box);
        }
    }

    private void GotoSelection()
    {
        _selection.GotoTreeTarget = _selection.GetSingleSelection();
    }

    /// <summary>
    ///     Reset the rotation of the selected object to 0, 0, 0
    /// </summary>
    private void ResetRotationSelection()
    {
        List<Action> actlist = new();

        HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);
        foreach (Entity s in selected)
        {
            Vector3 pos = s.GetLocalTransform().Position;
            var rot_x = 0;
            var rot_y = 0;
            var rot_z = 0;

            Transform newRot = new(pos, new Vector3(rot_x, rot_y, rot_z));

            actlist.Add(s.GetUpdateTransformAction(newRot));
        }


        if (actlist.Any())
        {
            CompoundAction action = new(actlist);
            EditorActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    ///     Rotate the selected objects by a fixed amount on the specified axis
    /// </summary>
    private void ArbitraryRotation_Selection(Vector3 axis, bool pivot)
    {
        List<Action> actlist = new();
        HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

        // Get the center position of the selections
        Vector3 accumPos = Vector3.Zero;
        foreach (Entity sel in sels)
        {
            accumPos += sel.GetLocalTransform().Position;
        }

        Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

        foreach (Entity s in sels)
        {
            Transform objT = s.GetLocalTransform();

            var radianRotateAmount = 0.0f;
            var rot_x = objT.EulerRotation.X;
            var rot_y = objT.EulerRotation.Y;
            var rot_z = objT.EulerRotation.Z;

            var newPos = Transform.Default;

            if (axis.X != 0)
            {
                radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Map_ArbitraryRotation_X_Shift;
                rot_x = objT.EulerRotation.X + radianRotateAmount;
            }

            if (axis.Y != 0)
            {
                radianRotateAmount = (float)Math.PI / 180 * CFG.Current.Map_ArbitraryRotation_Y_Shift;
                rot_y = objT.EulerRotation.Y + radianRotateAmount;
            }

            if (pivot)
            {
                newPos = Utils.RotateVectorAboutPoint(objT.Position, centerT.Position, axis, radianRotateAmount);
            }
            else
            {
                newPos.Position = objT.Position;
            }

            newPos.EulerRotation = new Vector3(rot_x, rot_y, rot_z);

            actlist.Add(s.GetUpdateTransformAction(newPos));
        }

        if (actlist.Any())
        {
            CompoundAction action = new(actlist);
            EditorActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    ///     Move current selection to the current camera position
    /// </summary>
    private void MoveSelectionToCamera()
    {
        List<Action> actlist = new();
        HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);

        Vector3 camDir = Vector3.Transform(Vector3.UnitZ, Viewport.WorldView.CameraTransform.RotationMatrix);
        Vector3 camPos = Viewport.WorldView.CameraTransform.Position;
        Vector3 targetCamPos = camPos + (camDir * CFG.Current.Map_MoveSelectionToCamera_Radius);

        // Get the accumulated center position of all selections
        Vector3 accumPos = Vector3.Zero;
        foreach (Entity sel in sels)
        {
            if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
            {
                // Use bounding box origin as center
                accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
            }
            else
            {
                // Use actual position as center
                accumPos += sel.GetRootLocalTransform().Position;
            }
        }

        Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

        // Offset selection positions to place accumulated center in front of camera
        foreach (Entity sel in sels)
        {
            Transform localT = sel.GetLocalTransform();
            Transform rootT = sel.GetRootTransform();
            
            // Get new localized position by applying reversed root offsets to target camera position.  
            Vector3 newPos = Vector3.Transform(targetCamPos, Quaternion.Inverse(rootT.Rotation)) 
                             - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));
            
            // Offset from center of multiple selections.
            Vector3 localCenter = Vector3.Transform(centerT.Position, Quaternion.Inverse(rootT.Rotation))
                                      - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));
            Vector3 offsetFromCenter = localCenter - localT.Position;
            newPos -= offsetFromCenter;
            
            Transform newT = new(newPos, localT.EulerRotation);

            actlist.Add(sel.GetUpdateTransformAction(newT));
        }

        if (actlist.Any())
        {
            CompoundAction action = new(actlist);
            EditorActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    ///     Hides all the selected objects, unless all of them are hidden in which
    ///     they will be unhidden
    /// </summary>
    public void HideShowSelection()
    {
        HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();
        var allhidden = true;
        foreach (Entity s in selected)
        {
            if (s.EditorVisible)
            {
                allhidden = false;
            }
        }

        foreach (Entity s in selected)
        {
            s.EditorVisible = allhidden;
        }
    }

    /// <summary>
    ///     Unhides all objects in every map
    /// </summary>
    public void UnhideAllObjects()
    {
        foreach (ObjectContainer m in Universe.LoadedObjectContainers.Values)
        {
            if (m == null)
            {
                continue;
            }

            foreach (Entity obj in m.Objects)
            {
                obj.EditorVisible = true;
            }
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

    private void DummySelection()
    {
        string[] sourceTypes = { "Enemy", "Object", "Asset" };
        string[] targetTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
        DummyUndummySelection(sourceTypes, targetTypes);
    }

    private void UnDummySelection()
    {
        string[] sourceTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
        string[] targetTypes = { "Enemy", "Object", "Asset" };
        DummyUndummySelection(sourceTypes, targetTypes);
    }

    private void DummyUndummySelection(string[] sourceTypes, string[] targetTypes)
    {
        Type msbclass;
        switch (AssetLocator.Type)
        {
            case GameType.DemonsSouls:
                msbclass = typeof(MSBD);
                break;
            case GameType.DarkSoulsPTDE:
            case GameType.DarkSoulsRemastered:
                msbclass = typeof(MSB1);
                break;
            case GameType.DarkSoulsIISOTFS:
                msbclass = typeof(MSB2);
                //break;
                return; //idk how ds2 dummies should work
            case GameType.DarkSoulsIII:
                msbclass = typeof(MSB3);
                break;
            case GameType.Bloodborne:
                msbclass = typeof(MSBB);
                break;
            case GameType.Sekiro:
                msbclass = typeof(MSBS);
                break;
            case GameType.EldenRing:
                msbclass = typeof(MSBE);
                break;
            case GameType.ArmoredCoreVI:
                msbclass = typeof(MSB_AC6);
                break;
            default:
                throw new ArgumentException("type must be valid");
        }

        List<MapEntity> sourceList = _selection.GetFilteredSelection<MapEntity>().ToList();

        ChangeMapObjectType action = new(Universe, msbclass, sourceList, sourceTypes, targetTypes, "Part", true);
        EditorActionManager.ExecuteAction(action);
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

            CloneMapObjectsAction action = new(Universe, RenderScene, sel, true, targetMap, targetParent);
            EditorActionManager.ExecuteAction(action);
            _dupeSelectionTargetedMap = ("None", null);
            _dupeSelectionTargetedParent = ("None", null);
            // Closes popup/menu bar
            ImGui.CloseCurrentPopup();
        }
    }

    /// <summary>
    ///     Gets all the msb types using reflection to populate editor creation menus
    /// </summary>
    /// <param name="type">The game to collect msb types for</param>
    private void PopulateClassNames(GameType type)
    {
        Type msbclass;
        switch (type)
        {
            case GameType.DemonsSouls:
                msbclass = typeof(MSBD);
                break;
            case GameType.DarkSoulsPTDE:
            case GameType.DarkSoulsRemastered:
                msbclass = typeof(MSB1);
                break;
            case GameType.DarkSoulsIISOTFS:
                msbclass = typeof(MSB2);
                break;
            case GameType.DarkSoulsIII:
                msbclass = typeof(MSB3);
                break;
            case GameType.Bloodborne:
                msbclass = typeof(MSBB);
                break;
            case GameType.Sekiro:
                msbclass = typeof(MSBS);
                break;
            case GameType.EldenRing:
                msbclass = typeof(MSBE);
                break;
            case GameType.ArmoredCoreVI:
                msbclass = typeof(MSB_AC6);
                break;
            default:
                throw new ArgumentException("type must be valid");
        }

        Type partType = msbclass.GetNestedType("Part");
        List<Type> partSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(partType) && !type.IsAbstract).ToList();
        _partsClasses = partSubclasses.Select(x => (x.Name, x)).ToList();

        Type regionType = msbclass.GetNestedType("Region");
        List<Type> regionSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(regionType) && !type.IsAbstract).ToList();
        _regionClasses = regionSubclasses.Select(x => (x.Name, x)).ToList();
        if (_regionClasses.Count == 0)
        {
            _regionClasses.Add(("Region", regionType));
        }

        Type eventType = msbclass.GetNestedType("Event");
        List<Type> eventSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(eventType) && !type.IsAbstract).ToList();
        _eventClasses = eventSubclasses.Select(x => (x.Name, x)).ToList();
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
            PopulateClassNames(AssetLocator.Type);
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
                            FrameSelection();
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
