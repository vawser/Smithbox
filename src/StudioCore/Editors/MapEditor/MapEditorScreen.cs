using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Framework.MassEdit;
using StudioCore.Editors.MapEditor.Helpers;
using StudioCore.Editors.MapEditor.PropertyEditor;
using StudioCore.Editors.MapEditor.Tools;
using StudioCore.Editors.MapEditor.Tools.AssetBrowser;
using StudioCore.Editors.MapEditor.Tools.DisplayGroups;
using StudioCore.Editors.MapEditor.Tools.EntityIdentifierOverview;
using StudioCore.Editors.MapEditor.Tools.EntityInformation;
using StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;
using StudioCore.Editors.MapEditor.Tools.MapQuery;
using StudioCore.Editors.MapEditor.Tools.NavmeshEdit;
using StudioCore.Editors.MapEditor.Tools.SelectionGroups;
using StudioCore.Editors.MapEditor.Tools.WorldMap;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Scene.Interfaces;
using StudioCore.Settings;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.MapEditor.Framework.MapActionHandler;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Main interface for the MSB Editor.
/// </summary>
public class MapEditorScreen : EditorScreen
{
    /// <summary>
    /// Lock variable used to handle pauses to the Update() function.
    /// </summary>
    private static readonly object _lock_PauseUpdate = new();
    private bool GCNeedsCollection;
    private bool _PauseUpdate;

    private IModal _activeModal;

    public ViewportActionManager EditorActionManager = new();
    public MapActionHandler ActionHandler;
    public ViewportSelection Selection = new();
    public Universe Universe;
    public MapEntityTypeCache EntityTypeCache = new();
    public EditorFocusManager FocusManager;
    public MapPropertyCache MapPropertyCache = new();
    public MapCommandQueue CommandQueue;
    public MapShortcuts Shortcuts;

    // Core Views
    public MapViewportView MapViewportView;
    public MapListView MapListView;
    public MapPropertyView MapPropertyView;

    // Optional Views
    public AssetBrowserView AssetBrowserView;
    public DisplayGroupView DisplayGroupView;
    public SelectionGroupView SelectionGroupView;
    public PrefabView PrefabView;
    public LightmapAtlasView LightmapAtlasView;
    public NavmeshBuilderView NavmeshBuilderView;
    public MapQueryView MapQueryView;
    public WorldMapView WorldMapView;
    public LocalSearchView LocalSearchView;
    public EntityInformationView EntityInformationView;
    public EntityIdentifierOverview EntityIdentifierOverview;

    // Menubar
    public BasicFilters BasicFilters;
    public RegionFilters RegionFilters;
    public MapContentFilters MapContentFilter;

    // Tools
    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;
    public MassEditHandler MassEditHandler;

    // Viewport

    public MapEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        MapViewportView = new MapViewportView(this, window, device);
        Universe = new Universe(MapViewportView.RenderScene, Selection);
        FocusManager = new EditorFocusManager(this);

        // Core Views
        MapListView = new MapListView(this);
        MapPropertyView = new MapPropertyView(this);

        // Optional Views
        DisplayGroupView = new DisplayGroupView(this);
        LocalSearchView = new LocalSearchView(this);

        AssetBrowserView = new AssetBrowserView(this);
        LightmapAtlasView = new LightmapAtlasView(this);
        PrefabView = new PrefabView(this);
        SelectionGroupView = new SelectionGroupView(this);
        NavmeshBuilderView = new NavmeshBuilderView(this);
        EntityInformationView = new EntityInformationView(this);
        EntityIdentifierOverview = new EntityIdentifierOverview(this);

        BasicFilters = new BasicFilters(this);
        RegionFilters = new RegionFilters(this);
        MapContentFilter = new MapContentFilters(this);

        // Framework
        ActionHandler = new MapActionHandler(this);
        MapQueryView = new MapQueryView(this);
        WorldMapView = new WorldMapView(this);
        CommandQueue = new MapCommandQueue(this);
        Shortcuts = new MapShortcuts(this);

        // Tools
        ToolWindow = new ToolWindow(this, ActionHandler);
        ToolSubMenu = new ToolSubMenu(this, ActionHandler);
        MassEditHandler = new MassEditHandler(this);

        // Focus
        FocusManager.SetDefaultFocusElement("Properties##mapeditprop");
        EditorActionManager.AddEventHandler(MapListView);
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

        MapViewportView.Update(dt);

        // Throw any exceptions that ocurred during async map loading.
        if (Universe.LoadMapExceptions != null)
        {
            Universe.LoadMapExceptions.Throw();
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        MapViewportView.EditorResized(window, device);
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
                ActionHandler.DisplayDuplicateToMapMenu(MapDuplicateToMapType.Menubar);

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
            UIHelper.ShowHoverTooltip("Frames the current selection in the viewport.");

            ///--------------------
            // Move to Grid
            ///--------------------
            if (ImGui.MenuItem("Move Selected to Grid", KeyBindings.Current.MAP_SetSelectionToGrid.HintText))
            {
                ActionHandler.ApplyMovetoGrid();
            }
            UIHelper.ShowHoverTooltip("Move the current selection to the nearest grid point.");

            ///--------------------
            // Move to Camera
            ///--------------------
            if (ImGui.MenuItem("Move Selected to Camera", KeyBindings.Current.MAP_MoveToCamera.HintText))
            {
                ActionHandler.ApplyMoveToCamera();
            }
            UIHelper.ShowHoverTooltip("Move the current selection to the camera position.");

            ///--------------------
            // Toggle Render Type
            ///--------------------
            if (ImGui.MenuItem("Toggle Render Type", KeyBindings.Current.VIEWPORT_ToggleRenderType.HintText))
            {
                VisualizationHelper.ToggleRenderType(Selection);
            }
            UIHelper.ShowHoverTooltip("Toggle the render type of the current selection.");

            ImGui.Separator();

            ///--------------------
            // Rotate (X-axis)
            ///--------------------
            if (ImGui.MenuItem("Positive Rotate Selected (X-axis)", KeyBindings.Current.MAP_RotateSelectionXAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }

            ///--------------------
            // Negative Rotate (X-axis)
            ///--------------------
            if (ImGui.MenuItem("Negative Rotate Selected (X-axis)", KeyBindings.Current.MAP_NegativeRotateSelectionXAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(-1, 0, 0), false);
            }

            ///--------------------
            // Rotate (Y-axis)
            ///--------------------
            if (ImGui.MenuItem("Positive Rotate Selected (Y-axis)", KeyBindings.Current.MAP_RotateSelectionYAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }

            ///--------------------
            // Negative Rotate (Y-axis)
            ///--------------------
            if (ImGui.MenuItem("Negative Rotate Selected (Y-axis)", KeyBindings.Current.MAP_NegativeRotateSelectionYAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), false);
            }

            ///--------------------
            // Rotate Pivot (Y-axis)
            ///--------------------
            if (ImGui.MenuItem("Positive Rotate Selected with Pivot (Y-axis)", KeyBindings.Current.MAP_PivotSelectionYAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }

            ///--------------------
            // Negative Rotate Pivot (Y-axis)
            ///--------------------
            if (ImGui.MenuItem("Negative Rotate Selected with Pivot (Y-axis)", KeyBindings.Current.MAP_NegativePivotSelectionYAxis.HintText))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), true);
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

            if (ImGui.MenuItem("Map List"))
            {
                UI.Current.Interface_MapEditor_MapList = !UI.Current.Interface_MapEditor_MapList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_MapList);

            if (ImGui.MenuItem("Map Contents"))
            {
                UI.Current.Interface_MapEditor_MapContents = !UI.Current.Interface_MapEditor_MapContents;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_MapContents);

            if (ImGui.MenuItem("Properties"))
            {
                UI.Current.Interface_MapEditor_Properties = !UI.Current.Interface_MapEditor_Properties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_Properties);

            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_MapEditor_ToolWindow = !UI.Current.Interface_MapEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_ToolWindow);

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

            if (ImGui.MenuItem("Viewport Information Panel"))
            {
                CFG.Current.Viewport_Enable_ViewportInfoPanel = !CFG.Current.Viewport_Enable_ViewportInfoPanel;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Viewport_Enable_ViewportInfoPanel);

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

            if (ImGui.MenuItem("Entity Identifier Overview"))
            {
                UI.Current.Interface_MapEditor_EntityIdentifierOverview = !UI.Current.Interface_MapEditor_EntityIdentifierOverview;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_EntityIdentifierOverview);

            // Debug only
            if (CFG.Current.DisplayDebugTools)
            {
                if (ImGui.MenuItem("Entity Information"))
                {
                    UI.Current.Interface_MapEditor_EntityInformation = !UI.Current.Interface_MapEditor_EntityInformation;
                }
                UIHelper.ShowActiveStatus(UI.Current.Interface_MapEditor_EntityInformation);
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Map Object List: Categories"))
            {
                CFG.Current.MapEditor_DisplayMapCategories = !CFG.Current.MapEditor_DisplayMapCategories;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_DisplayMapCategories);

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

        var validViewportState = MapViewportView.RenderScene != null && 
            MapViewportView.Viewport != null;

        // Tools
        ToolSubMenu.DisplayMenu();

        ImGui.Separator();

        // Filters
        if (ImGui.BeginMenu("Filters", validViewportState))
        {
            BasicFilters.Display();

            ImGui.Separator();

            RegionFilters.DisplayOptions();

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Viewport
        if (ImGui.BeginMenu("Viewport", validViewportState))
        {
            if (ImGui.BeginMenu("Filter Presets"))
            {
                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_01.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_01.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_02.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_02.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_03.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_03.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_04.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_04.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_05.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_05.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.SceneFilter_Preset_06.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.SceneFilter_Preset_06.Filters;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Environment Map"))
            {
                if (ImGui.MenuItem("Default"))
                {
                    MapViewportView.Viewport.SetEnvMap(0);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Scene Lighting"))
            {
                MapViewportView.Viewport.SceneParamsGui();
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

            CFG.Current.LastSceneFilter = MapViewportView.RenderScene.DrawFilter;
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

        Shortcuts.Monitor();
        WorldMapView.Shortcuts();
        ToolSubMenu.Shortcuts();
        CommandQueue.Parse(initcmd);
        ActionHandler.HandleDuplicateToMapMenuPopup();

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
        //ImGui.Text($@"Viewport size: {Viewport.Width}x{Viewport.Height}");
        //ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

        MapViewportView.OnGui();
        MapListView.OnGui();

        if (Smithbox.FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        if (MapPropertyView.Focus)
        {
            MapPropertyView.Focus = false;
            ImGui.SetNextWindowFocus();
        }

        MapPropertyView.OnGui(Selection, "mapeditprop", MapViewportView.Viewport.Width, MapViewportView.Viewport.Height);

        LocalSearchView.OnGui();

        // Not usable yet
        if (FeatureFlags.EnableNavmeshBuilder)
        {
            NavmeshBuilderView.OnGui();
        }

        if (LightmapAtlasView != null)
        {
            LightmapAtlasView.OnGui();
        }

        if (EntityInformationView != null)
        {
            EntityInformationView.OnGui();
        }

        if (EntityIdentifierOverview != null)
        {
            EntityIdentifierOverview.OnGui();
        }

        ResourceLoadWindow.DisplayWindow(MapViewportView.Viewport.Width, MapViewportView.Viewport.Height);
        if (UI.Current.Interface_MapEditor_ResourceList)
        {
            ResourceListWindow.DisplayWindow("mapResourceList");
        }

        DisplayGroupView.OnGui();
        AssetBrowserView.OnGui();
        SelectionGroupView.OnGui();

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

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (!CFG.Current.EnableEditor_MSB)
            return;

        if (MapViewportView.Viewport != null)
        {
            MapViewportView.Draw(device, cl);
        }
    }

    public bool InputCaptured()
    {
        return MapViewportView.InputCaptured();
    }

    public void OnProjectChanged()
    {
        if (!CFG.Current.EnableEditor_MSB)
            return;

        Selection.ClearSelection();
        EditorActionManager.Clear();

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            MapListView.OnProjectChanged();

            MapQueryView.OnProjectChanged();
            SelectionGroupView.OnProjectChanged();
            AssetBrowserView.OnProjectChanged();
            RegionFilters.OnProjectChanged();
            PrefabView.OnProjectChanged();
            ToolWindow.OnProjectChanged();
            ToolSubMenu.OnProjectChanged();
            WorldMapView.OnProjectChanged();
        }

        MapViewportView.OnProjectChanged();
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
            LightmapAtlasView.Save();
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
                            Selection.ClearSelection();
                            Selection.AddSelection(obj);
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
