using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions;
using StudioCore.Editors.MapEditor.LightmapAtlasEditor;
using StudioCore.Editors.MapEditor.MapQuery;
using StudioCore.Editors.MapEditor.PropertyEditor;
using StudioCore.Editors.MapEditor.Tools;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Tasks;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.MapEditor.Actions.ActionHandler;
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

    public MapSelectionManager Selection;

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

    public MapQuerySearchEngine MapQueryHandler;

    public EditorFocusManager FocusManager;

    public MapEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
        Window = window;

        Selection = new MapSelectionManager(this);

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
        if (!CFG.Current.EnableEditor_MSB)
            return;

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

    public void EditDropdown()
    {
        if (!CFG.Current.EnableEditor_MSB)
            return;

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            ///--------------------
            // Duplicate
            ///--------------------
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ActionHandler.ApplyDuplicate();
            }
            UIHelper.ShowHoverTooltip($"Duplicate the currently selected map objects.");

            ///--------------------
            // Delete
            ///--------------------
            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.ApplyDelete();
            }
            UIHelper.ShowHoverTooltip($"Delete the currently selected map objects.");

            ///--------------------
            // Scramble
            ///--------------------
            if (ImGui.MenuItem("Scramble", KeyBindings.Current.MAP_ScrambleSelection.HintText))
            {
                ActionHandler.ApplyScramble();
            }
            UIHelper.ShowHoverTooltip($"Apply the scramble configuration to the currently selected map objects.");

            ///--------------------
            // Replicate
            ///--------------------
            if (ImGui.MenuItem("Replicate", KeyBindings.Current.MAP_ReplicateSelection.HintText))
            {
                ActionHandler.ApplyReplicate();
            }
            UIHelper.ShowHoverTooltip($"Apply the replicate configuration to the currently selected map objects.");

            ImGui.Separator();

            ///--------------------
            // Duplicate to Map
            ///--------------------
            if (ImGui.BeginMenu("Duplicate Selected to Map"))
            {
                ActionHandler.DisplayDuplicateToMapMenu(false, true);

                ImGui.EndMenu();
            }
            UIHelper.ShowHoverTooltip($"Duplicate the selected map objects into another map.");

            ///--------------------
            // Create
            ///--------------------
            if (ImGui.BeginMenu("Create New Object"))
            {
                if (ImGui.BeginCombo("##Targeted Map", ActionHandler._targetMap.Item1))
                {
                    foreach (var obj in Universe.LoadedObjectContainers)
                    {
                        if (obj.Value != null)
                        {
                            if (ImGui.Selectable(obj.Key))
                            {
                                ActionHandler._targetMap = (obj.Key, obj.Value);
                                break;
                            }
                        }
                    }
                    ImGui.EndCombo();
                }

                if (ActionHandler._targetMap != (null, null))
                {
                    var map = (MapContainer)ActionHandler._targetMap.Item2;

                    if (ImGui.BeginMenu("Parts"))
                    {
                        foreach ((string, Type) p in ActionHandler._partsClasses)
                        {
                            if (ImGui.MenuItem($"{p.Item1}"))
                            {
                                CFG.Current.Toolbar_Create_Part = true;
                                CFG.Current.Toolbar_Create_Region = false;
                                CFG.Current.Toolbar_Create_Event = false;

                                ActionHandler._createPartSelectedType = p.Item2;
                                ActionHandler.ApplyObjectCreation();
                            }
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.ShowHoverTooltip("Create a Part object.");

                    if (ActionHandler._regionClasses.Count == 1)
                    {
                        if (ImGui.MenuItem("Region"))
                        {
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Region = true;
                            CFG.Current.Toolbar_Create_Event = false;

                            ActionHandler._createRegionSelectedType = ActionHandler._regionClasses[0].Item2;
                            ActionHandler.ApplyObjectCreation();
                        }
                    }
                    else
                    {
                        if (ImGui.BeginMenu("Regions"))
                        {
                            foreach ((string, Type) p in ActionHandler._regionClasses)
                            {
                                if (ImGui.MenuItem($"{p.Item1}"))
                                {
                                    CFG.Current.Toolbar_Create_Part = false;
                                    CFG.Current.Toolbar_Create_Region = true;
                                    CFG.Current.Toolbar_Create_Event = false;

                                    ActionHandler._createRegionSelectedType = p.Item2;
                                    ActionHandler.ApplyObjectCreation();
                                }
                            }

                            ImGui.EndMenu();
                        }
                        UIHelper.ShowHoverTooltip("Create a Region object.");
                    }

                    if (ImGui.BeginMenu("Events"))
                    {
                        foreach ((string, Type) p in ActionHandler._eventClasses)
                        {
                            if (ImGui.MenuItem($"{p.Item1}"))
                            {
                                CFG.Current.Toolbar_Create_Part = false;
                                CFG.Current.Toolbar_Create_Region = false;
                                CFG.Current.Toolbar_Create_Event = true;

                                ActionHandler._createEventSelectedType = p.Item2;
                                ActionHandler.ApplyObjectCreation();
                            }
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.ShowHoverTooltip("Create an Event object.");

                    if (ImGui.MenuItem("Light"))
                    {
                        CFG.Current.Toolbar_Create_Part = false;
                        CFG.Current.Toolbar_Create_Region = false;
                        CFG.Current.Toolbar_Create_Event = false;

                        if (map.BTLParents.Any())
                        {
                            ActionHandler.ApplyObjectCreation();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Create a BTL Light object.");
                }

                ImGui.EndMenu();
            }
            UIHelper.ShowHoverTooltip($"Create a new map object.");

            ImGui.Separator();

            ///--------------------
            // Frame in Viewport
            ///--------------------
            if (ImGui.MenuItem("Frame Selected in Viewport", KeyBindings.Current.MAP_FrameSelection.HintText))
            {
                ActionHandler.ApplyFrameInViewport();
            }

            ///--------------------
            // Move to Grid
            ///--------------------
            if (ImGui.MenuItem("Move Selected to Grid", KeyBindings.Current.MAP_ReplicateSelection.HintText))
            {
                ActionHandler.ApplyMovetoGrid();
            }

            ///--------------------
            // Move to Camera
            ///--------------------
            if (ImGui.MenuItem("Move Selected to Camera", KeyBindings.Current.MAP_MoveToCamera.HintText))
            {
                ActionHandler.ApplyMoveToCamera();
            }

            ImGui.Separator();

            ///--------------------
            // Rotate (X-axis)
            ///--------------------
            if (ImGui.MenuItem("Rotate Selected (X-axis)", KeyBindings.Current.MAP_RotateSelectionXAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            ///--------------------
            // Rotate (Y-axis)
            ///--------------------
            if (ImGui.MenuItem("Rotate Selected (Y-axis)", KeyBindings.Current.MAP_RotateSelectionYAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            ///--------------------
            // Rotate Pivot (Y-axis)
            ///--------------------
            if (ImGui.MenuItem("Rotate Selected with Pivot (Y-axis)", KeyBindings.Current.MAP_PivotSelectionYAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            ///--------------------
            // Rotate Fixed Increment
            ///--------------------
            if (ImGui.MenuItem("Rotate Selected to Fixed Angle", KeyBindings.Current.MAP_RotateFixedAngle.HintText))
            {
                ActionHandler.SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }

            ///--------------------
            // Reset Rotation
            ///--------------------
            if (ImGui.MenuItem("Reset Selected Rotation", KeyBindings.Current.MAP_ResetRotation.HintText))
            {
                ActionHandler.SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }

            ImGui.Separator();

            ///--------------------
            // Go to in List
            ///--------------------
            if (ImGui.MenuItem("Go to in List", KeyBindings.Current.MAP_GoToInList.HintText))
            {
                ActionHandler.ApplyGoToInObjectList();
            }

            ///--------------------
            // Order (Up)
            ///--------------------
            if (ImGui.MenuItem("Move Selected Up in List", KeyBindings.Current.MAP_MoveObjectUp.HintText))
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
            }

            ///--------------------
            // Order (Down)
            ///--------------------
            if (ImGui.MenuItem("Move Selected Down in List", KeyBindings.Current.MAP_MoveObjectDown.HintText))
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
            }

            ///--------------------
            // Order (Top)
            ///--------------------
            if (ImGui.MenuItem("Move Selected to the List Top", KeyBindings.Current.MAP_MoveObjectTop.HintText))
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
            }

            ///--------------------
            // Order (Bottom)
            ///--------------------
            if (ImGui.MenuItem("Move Selected to the List Bottom", KeyBindings.Current.MAP_MoveObjectBottom.HintText))
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
            }

            ImGui.Separator();

            ///--------------------
            // Toggle Editor Visibility
            ///--------------------
            if (ImGui.BeginMenu("Toggle Editor Visibility"))
            {
                if (ImGui.MenuItem("Flip Visibility for Selected", KeyBindings.Current.MAP_FlipSelectionVisibility.HintText))
                {
                    ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
                }

                if (ImGui.MenuItem("Enable Visibility for Selected", KeyBindings.Current.MAP_EnableSelectionVisibility.HintText))
                {
                    ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
                }

                if (ImGui.MenuItem("Disable Visibility for Selected", KeyBindings.Current.MAP_DisableSelectionVisibility.HintText))
                {
                    ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
                }

                if (ImGui.MenuItem("Flip Visibility for All", KeyBindings.Current.MAP_FlipAllVisibility.HintText))
                {
                    ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
                }

                if (ImGui.MenuItem("Enable Visibility for All", KeyBindings.Current.MAP_EnableAllVisibility.HintText))
                {
                    ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
                }

                if (ImGui.MenuItem("Disable Visibility for All", KeyBindings.Current.MAP_DisableAllVisibility.HintText))
                {
                    ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
                }

                ImGui.EndMenu();
            }

            ///--------------------
            // Toggle In-game Visibility
            ///--------------------
            if (ImGui.BeginMenu("Toggle In-Game Visibility"))
            {
                if (ImGui.MenuItem("Make Selected Normal Object into Dummy Object", KeyBindings.Current.MAP_MakeDummyObject.HintText))
                {
                    ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
                }

                if (ImGui.MenuItem("Make Selected Dummy Object into Normal Object", KeyBindings.Current.MAP_MakeNormalObject.HintText))
                {
                    ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
                }

                if (Smithbox.ProjectType is ProjectType.ER)
                {
                    if (ImGui.MenuItem("Disable Game Presence of Selected", KeyBindings.Current.MAP_DisableGamePresence.HintText))
                    {
                        ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                    }

                    if (ImGui.MenuItem("Enable Game Presence of Selected", KeyBindings.Current.MAP_EnableGamePresence.HintText))
                    {
                        ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ViewDropdown()
    {
        if (!CFG.Current.EnableEditor_MSB)
            return;

        // Dropdown: View
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Viewport"))
            {
                UI.Current.Interface_Editor_Viewport = !UI.Current.Interface_Editor_Viewport;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Viewport);

            if (ImGui.MenuItem("Map Object List"))
            {
                UI.Current.Interface_MapEditor_MapObjectList = !UI.Current.Interface_MapEditor_MapObjectList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_MapObjectList);

            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_MapEditor_ToolWindow = !UI.Current.Interface_MapEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_ToolWindow);

            if (ImGui.MenuItem("Properties"))
            {
                UI.Current.Interface_MapEditor_Properties = !UI.Current.Interface_MapEditor_Properties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_Properties);

            if (ImGui.MenuItem("Asset Browser"))
            {
                UI.Current.Interface_MapEditor_AssetBrowser = !UI.Current.Interface_MapEditor_AssetBrowser;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_AssetBrowser);

            if (ImGui.MenuItem("Render Groups"))
            {
                UI.Current.Interface_MapEditor_RenderGroups = !UI.Current.Interface_MapEditor_RenderGroups;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_RenderGroups);

            if (ImGui.MenuItem("Profiling"))
            {
                UI.Current.Interface_Editor_Profiling = !UI.Current.Interface_Editor_Profiling;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Profiling);

            if (ImGui.MenuItem("Resource List"))
            {
                UI.Current.Interface_MapEditor_ResourceList = !UI.Current.Interface_MapEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_ResourceList);

            if (ImGui.MenuItem("Viewport Grid"))
            {
                UI.Current.Interface_MapEditor_Viewport_Grid = !UI.Current.Interface_MapEditor_Viewport_Grid;
                CFG.Current.MapEditor_Viewport_RegenerateMapGrid = true;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_Viewport_Grid);

            if (Smithbox.ProjectType is ProjectType.DS3)
            {
                if (ImGui.MenuItem("Lightmap Atlas Editor"))
                {
                    UI.Current.Interface_MapEditor_Viewport_LightmapAtlas = !UI.Current.Interface_MapEditor_Viewport_LightmapAtlas;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_Viewport_LightmapAtlas);
            }

            ImGui.Separator();

            // Quick toggles for some of the Field Editor field visibility options

            if (ImGui.MenuItem("Field: Community Names"))
            {
                CFG.Current.MapEditor_Enable_Commmunity_Names = !CFG.Current.MapEditor_Enable_Commmunity_Names;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_Enable_Commmunity_Names);

            if (ImGui.MenuItem("Field: Padding"))
            {
                CFG.Current.MapEditor_Enable_Padding_Fields = !CFG.Current.MapEditor_Enable_Padding_Fields;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_Enable_Padding_Fields);

            if (ImGui.MenuItem("Field: Obsolete"))
            {
                CFG.Current.MapEditor_Enable_Obsolete_Fields = !CFG.Current.MapEditor_Enable_Obsolete_Fields;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_Enable_Obsolete_Fields);

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void EditorUniqueDropdowns()
    {
        if (!CFG.Current.EnableEditor_MSB)
            return;

        ToolSubMenu.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("Filters", RenderScene != null && Viewport != null))
        {
            bool ticked;

            // Map Piece
            if (ImGui.MenuItem("Map Piece"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.MapPiece);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
            UIHelper.ShowActiveStatus(ticked);

            // Collision
            if (ImGui.MenuItem("Collision"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Collision);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
            UIHelper.ShowActiveStatus(ticked);

            // Object
            if (ImGui.MenuItem("Object"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Object);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Object);
            UIHelper.ShowActiveStatus(ticked);

            // Character
            if (ImGui.MenuItem("Character"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Character);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Character);
            UIHelper.ShowActiveStatus(ticked);

            // Navmesh
            if (ImGui.MenuItem("Navmesh"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Navmesh);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Navmesh);
            UIHelper.ShowActiveStatus(ticked);

            // Region
            if (ImGui.MenuItem("Region"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Region);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Region);
            UIHelper.ShowActiveStatus(ticked);

            // Light
            if (ImGui.MenuItem("Light"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.Light);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.Light);
            UIHelper.ShowActiveStatus(ticked);

            // Debug
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
            if (ImGui.BeginMenu("Filter Presets"))
            {
                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_01.Name))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_01.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_02.Name))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_02.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_03.Name))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_03.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_04.Name))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_04.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_05.Name))
                {
                    RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_05.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_06.Name))
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

            if (Smithbox.ProjectType is ProjectType.ER)
            {
                if (ImGui.BeginMenu("Collision Type"))
                {
                    if (ImGui.MenuItem("Low"))
                    {
                        HavokCollisionManager.VisibleCollisionType = HavokCollisionType.Low;
                    }
                    UIHelper.ShowHoverTooltip("Visible collision will use the low-detail mesh.\nUsed for standard collision.\nMap must be reloaded after change to see difference.");
                    UIHelper.ShowActiveStatus(HavokCollisionManager.VisibleCollisionType == HavokCollisionType.Low);

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
            if (ImGui.BeginMenu("Mode"))
            {
                if (ImGui.MenuItem("Translate", KeyBindings.Current.VIEWPORT_GizmoTranslationMode.HintText))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Translate;
                }
                UIHelper.ShowHoverTooltip($"Set the gizmo to Translation mode.");

                if (ImGui.MenuItem("Rotate", KeyBindings.Current.VIEWPORT_GizmoRotationMode.HintText))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }
                UIHelper.ShowHoverTooltip($"Set the gizmo to Rotation mode.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Space"))
            {
                if (ImGui.MenuItem("Local", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }
                UIHelper.ShowHoverTooltip($"Place the gizmo origin based on the selection's local position.");

                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }
                UIHelper.ShowHoverTooltip($"Place the gizmo origin based on the selection's world position.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Origin"))
            {
                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }
                UIHelper.ShowHoverTooltip($"Orient the gizmo origin based on the world position.");

                if (ImGui.MenuItem("Bounding Box", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }
                UIHelper.ShowHoverTooltip($"Orient the gizmo origin based on the bounding box.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void OnGUI(string[] initcmd)
    {
        if (!CFG.Current.EnableEditor_MSB)
            return;

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

        if (UpdatePropSearch)
        {
            UpdatePropSearch = false;

            PropSearch.OnGui(propSearchCmd);
        }

        if (Smithbox.FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        if(PropEditor.Focus)
        {
            PropEditor.Focus = false;
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
    private bool UpdatePropSearch = false;

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
                UpdatePropSearch = true;
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

            // Create
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateMapObject) && _selection.IsSelection())
            {
                ActionHandler.ApplyObjectCreation();
            }

            // Duplicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry) && _selection.IsSelection())
            {
                ActionHandler.ApplyDuplicate();
            }

            // Duplicate to Map
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DuplicateToMap) && _selection.IsSelection())
            {
                ImGui.OpenPopup("##DupeToTargetMapPopup");
            }

            // Delete
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry) && _selection.IsSelection())
            {
                ActionHandler.ApplyDelete();
            }

            // Frame in Viewport
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FrameSelection) && _selection.IsSelection())
            {
                ActionHandler.ApplyFrameInViewport();
            }

            // Go to in Map Object List
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_GoToInList) && _selection.IsSelection())
            {
                ActionHandler.ApplyGoToInObjectList();
            }

            // Move to Camera
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveToCamera) && _selection.IsSelection())
            {
                ActionHandler.ApplyMoveToCamera();
            }

            // Rotate (X-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionXAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            // Rotate (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            // Rotate Pivot (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_PivotSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            // Rotate (Fixed Increment)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_RotateFixedAngle))
            {
                ActionHandler.SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }

            // Reset Rotation
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ResetRotation))
            {
                ActionHandler.SetSelectionToFixedRotation(new Vector3(0, 0, 0));
            }

            // Order (Up)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectUp) && _selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
            }

            // Order (Down)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectDown) && _selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
            }

            // Order (Top)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectTop) && _selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
            }

            // Order (Bottom)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectBottom) && _selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
            }

            // Scramble
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ScrambleSelection) && _selection.IsSelection())
            {
                ActionHandler.ApplyScramble();
            }

            // Replicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ReplicateSelection) && _selection.IsSelection())
            {
                ActionHandler.ApplyReplicate();
            }

            // Move to Grid
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetSelectionToGrid) && _selection.IsSelection())
            {
                ActionHandler.ApplyMovetoGrid();
            }

            // Toggle Editor Visibility
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipSelectionVisibility) && _selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableSelectionVisibility) && _selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableSelectionVisibility) && _selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipAllVisibility))
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableAllVisibility))
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableAllVisibility))
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
            }

            // Toggle In-game Visibility
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeDummyObject) && _selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeNormalObject) && _selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableGamePresence) && _selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableGamePresence) && _selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
            }

            // Toggle Selection Outline
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RenderOutline))
            {
                CFG.Current.Viewport_Enable_Selection_Outline = !CFG.Current.Viewport_Enable_Selection_Outline;
            }

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
        if (!CFG.Current.EnableEditor_MSB)
            return;

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
        if (!CFG.Current.EnableEditor_MSB)
            return;

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
        }

        ReloadUniverse();
    }

    public void Save()
    {
        if (!CFG.Current.EnableEditor_MSB)
            return;

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
        if (!CFG.Current.EnableEditor_MSB)
            return;

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
                LogLevel.Error, LogPriority.Normal, e.Wrapped);

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
                    LogLevel.Error, LogPriority.High);
            }
        }
        else
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, LogPriority.High, e.Wrapped);
        }
    }
}
