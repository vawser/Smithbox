using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Smithbox.Core.MapEditorNS;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Interfaces;
using StudioCore.Tasks;
using StudioCore.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.MapEditorNS.MapActionHandler;

namespace StudioCore.Editors.MapEditorNS;

/// <summary>
/// Main interface for the MSB Editor.
/// </summary>
public class MapEditor : IEditor
{
    public int ID;

    public BaseEditor BaseEditor;
    public Project Project;

    public Sdl2Window Window;
    public GraphicsDevice Device;
    public RenderScene RenderScene;

    /// <summary>
    /// Lock variable used to handle pauses to the Update() function.
    /// </summary>
    private readonly object _lock_PauseUpdate = new();
    private bool GCNeedsCollection;
    private bool _PauseUpdate;

    private IModal _activeModal;

    public MapEditorFocus EditorFocus;

    public ViewportActionManager EditorActionManager;
    public MapActionHandler ActionHandler;
    public ViewportSelection Selection;
    public Universe Universe;
    public MapEntityTypeCache EntityTypeCache;
    public MapPropertyCache MapPropertyCache;
    public MapCommandQueue CommandQueue;

    // Core Views
    public MapViewport MapViewport;
    public MapListView MapListView;
    public MapPropertyView MapPropertyView;

    public MapEditorDecorator MapEditorDecorator;

    // Optional Views
    public AssetBrowserView AssetBrowserView;
    public DisplayGroupView DisplayGroupView;
    public SelectionGroupView SelectionGroupView;
    public PrefabView PrefabView;
    public NavmeshBuilderView NavmeshBuilderView;
    public MapQueryView MapQueryView;
    public WorldMapView WorldMapView;
    public MapLocalPropSearch LocalSearchView;
    public EntityInformationView EntityInformationView;
    public EntityIdentifierOverview EntityIdentifierOverview;
    public PatrolDrawManager PatrolDrawManager;

    // Menubar
    public MapRegionFilters RegionFilters;
    public MapContentFilters MapContentFilter;

    // Tools
    public ToolWindow ToolWindow;
    public MassEditHandler MassEditHandler;

    public KeyboardMovement KeyboardMovement;
    public RotationIncrement RotationIncrement;

    public bool IsFocused = false;
    public bool IsSetup = false;

    // Viewport

    public MapEditor(int id, BaseEditor baseEditor, Project project)
    {
        ID = id;
        BaseEditor = baseEditor;
        Project = project;

        // Viewport
        Window = baseEditor.GraphicsContext.Window;
        Device = baseEditor.GraphicsContext.Device;
        RenderScene = new RenderScene();
        RenderScene.DrawFilter = CFG.Current.LastSceneFilter;
        MapViewport = new MapViewport(this);
        Universe = new Universe(this);

        EditorActionManager = new();
        Selection = new();
        EntityTypeCache = new(this);
        MapPropertyCache = new();

        EditorFocus = new MapEditorFocus(this);
        MapListView = new MapListView(this);
        MapPropertyView = new MapPropertyView(this);

        MapEditorDecorator = new MapEditorDecorator(this);

        // Optional Views
        DisplayGroupView = new DisplayGroupView(this);
        LocalSearchView = new MapLocalPropSearch(this);
        AssetBrowserView = new AssetBrowserView(this);
        PrefabView = new PrefabView(this);
        SelectionGroupView = new SelectionGroupView(this);
        NavmeshBuilderView = new NavmeshBuilderView(this);
        EntityInformationView = new EntityInformationView(this);
        EntityIdentifierOverview = new EntityIdentifierOverview(this);

        RegionFilters = new MapRegionFilters(this);
        MapContentFilter = new MapContentFilters(this);

        // Framework
        ActionHandler = new MapActionHandler(this);
        MapQueryView = new MapQueryView(this);
        WorldMapView = new WorldMapView(this);
        CommandQueue = new MapCommandQueue(this);

        // Tools
        ToolWindow = new ToolWindow(this);
        MassEditHandler = new MassEditHandler(this);

        KeyboardMovement = new KeyboardMovement(this);
        RotationIncrement = new RotationIncrement(this);
        PatrolDrawManager = new PatrolDrawManager(this);

        EditorFocus.SetFocus("Properties##mapEditorPropertyView");

        EditorActionManager.AddEventHandler(MapListView);
        
        Universe.PopulateMapList();

        ActionHandler.PopulateClassNames();

        IsSetup = true;
    }

    /// <summary>
    /// Main graphical loop for this editor
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="cmd"></param>
    public void Display(float dt, string[] cmd)
    {
        UIHelper.ApplyMainStyle();
        ImGui.Begin($"Map Editor##MapEditor{ID}", Project.BaseEditor.MainWindowFlags);

        uint dockspaceID = ImGui.GetID($"MapEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();
        WorldMapView.Shortcuts();

        CommandQueue.Parse(cmd);

        ImGui.End();
        UIHelper.UnpplyMainStyle();

        // GUI
        MapViewport.Display();
        MapListView.Display();
        MapPropertyView.Display();
        EntityInformationView.Display();
        EntityIdentifierOverview.Display();
        DisplayGroupView.Display();
        AssetBrowserView.Display();
        ToolWindow.Display();
        NavmeshBuilderView.Display();
        MapViewport.DisplayResourceLoadWindow();
        MapViewport.DisplayResourceList();

        // Behavior
        SelectionGroupView.Update();
        LocalSearchView.Update();
        EditorFocus.Update();
        MapViewport.Update(dt);

        // Throw any exceptions that ocurred during async map loading.
        if (Universe.LoadMapExceptions != null)
        {
            Universe.LoadMapExceptions.Throw();
        }

        if (GCNeedsCollection)
        {
            GC.Collect();
            GCNeedsCollection = false;
        }
    }

    /// <summary>
    /// Viewport draw loop for this editor
    /// </summary>
    /// <param name="device"></param>
    /// <param name="cl"></param>
    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (MapViewport != null)
        {
            MapViewport.Draw(device, cl);
        }
    }

    public void Resized(Sdl2Window window, GraphicsDevice device)
    {
        MapViewport.EditorResized(window, device);
    }

    /// <summary>
    /// Editor menubar
    /// </summary>
    public void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
                {
                    Save();
                }

                ImGui.EndMenu();
            }

            EditMenu();
            ViewMenu();
            EditorMenu();

            ImGui.EndMenuBar();
        }
    }

    /// <summary>
    /// Menubar dropdowns relating to discrete actions
    /// </summary>
    public void EditMenu()
    {
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
            UIHelper.Tooltip($"Duplicate the currently selected map objects.");

            ///--------------------
            // Delete
            ///--------------------
            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.ApplyDelete();
            }
            UIHelper.Tooltip($"Delete the currently selected map objects.");

            ///--------------------
            // Scramble
            ///--------------------
            if (ImGui.MenuItem("Scramble", KeyBindings.Current.MAP_ScrambleSelection.HintText))
            {
                ActionHandler.ApplyScramble();
            }
            UIHelper.Tooltip($"Apply the scramble configuration to the currently selected map objects.");

            ///--------------------
            // Replicate
            ///--------------------
            if (ImGui.MenuItem("Replicate", KeyBindings.Current.MAP_ReplicateSelection.HintText))
            {
                ActionHandler.ApplyReplicate();
            }
            UIHelper.Tooltip($"Apply the replicate configuration to the currently selected map objects.");

            ImGui.Separator();

            ///--------------------
            // Duplicate to Map
            ///--------------------
            if (ImGui.BeginMenu("Duplicate Selected to Map"))
            {
                ActionHandler.DisplayDuplicateToMapMenu(MapDuplicateToMapType.Menubar);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip($"Duplicate the selected map objects into another map.");

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
                    UIHelper.Tooltip("Create a Part object.");

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
                        UIHelper.Tooltip("Create a Region object.");
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
                    UIHelper.Tooltip("Create an Event object.");

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
                    UIHelper.Tooltip("Create a BTL Light object.");
                }

                ImGui.EndMenu();
            }
            UIHelper.Tooltip($"Create a new map object.");

            ImGui.Separator();

            ///--------------------
            // Frame in Viewport
            ///--------------------
            if (ImGui.MenuItem("Frame Selected in Viewport", KeyBindings.Current.MAP_FrameSelection.HintText))
            {
                ActionHandler.ApplyFrameInViewport();
            }
            UIHelper.Tooltip("Frames the current selection in the viewport.");

            ///--------------------
            // Move to Grid
            ///--------------------
            if (ImGui.MenuItem("Move Selected to Grid", KeyBindings.Current.MAP_SetSelectionToGrid.HintText))
            {
                ActionHandler.ApplyMovetoGrid();
            }
            UIHelper.Tooltip("Move the current selection to the nearest grid point.");

            ///--------------------
            // Move to Camera
            ///--------------------
            if (ImGui.MenuItem("Move Selected to Camera", KeyBindings.Current.MAP_MoveToCamera.HintText))
            {
                ActionHandler.ApplyMoveToCamera();
            }
            UIHelper.Tooltip("Move the current selection to the camera position.");

            ///--------------------
            // Toggle Render Type
            ///--------------------
            if (ImGui.MenuItem("Toggle Render Type", KeyBindings.Current.VIEWPORT_ToggleRenderType.HintText))
            {
                MapEditorUtils.ToggleRenderType(Selection);
            }
            UIHelper.Tooltip("Toggle the render type of the current selection.");

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

                if (Project.ProjectType is ProjectType.ER)
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

    /// <summary>
    /// Menubar dropdowns relating to visibility
    /// </summary>
    public void ViewMenu()
    {
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

            if (Project.ProjectType is ProjectType.DS3)
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

    /// <summary>
    /// Menubar dropdowns specific to this editor
    /// </summary>
    public void EditorMenu()
    {
        var validViewportState = RenderScene != null && 
            MapViewport != null;

        if (ImGui.BeginMenu("Tools"))
        {
            ///--------------------
            /// Color Picker
            ///--------------------
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            ImGui.Separator();

            ///--------------------
            /// Toggle Editor Visibility by Tag
            ///--------------------
            if (ImGui.BeginMenu("Toggle Editor Visibility by Tag"))
            {
                ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
                UIHelper.Tooltip("Specific which tag the map objects will be filtered by.");

                if (ImGui.MenuItem("Enable Visibility"))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = true;
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = false;

                    ActionHandler.ApplyEditorVisibilityChangeByTag();
                }
                if (ImGui.MenuItem("Disable Visibility"))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = true;

                    ActionHandler.ApplyEditorVisibilityChangeByTag();
                }

                ImGui.EndMenu();
            }

            ///--------------------
            /// Patrol Route Visualisation
            ///--------------------
            if (Project.ProjectType != ProjectType.DS2S && Project.ProjectType != ProjectType.DS2)
            {
                if (ImGui.BeginMenu("Patrol Route Visualisation"))
                {
                    if (ImGui.MenuItem("Display"))
                    {
                        PatrolDrawManager.Generate();
                    }
                    if (ImGui.MenuItem("Clear"))
                    {
                        PatrolDrawManager.Clear();
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Generate Navigation Data
            ///--------------------
            if (Project.ProjectType is ProjectType.DES || Project.ProjectType is ProjectType.DS1 || Project.ProjectType is ProjectType.DS1R)
            {
                if (ImGui.BeginMenu("Navigation Data"))
                {
                    if (ImGui.MenuItem("Generate"))
                    {
                        ActionHandler.GenerateNavigationData();
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Entity ID Checker
            ///--------------------
            if (Project.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
            {
                if (ImGui.BeginMenu("Entity ID Checker"))
                {
                    if (Universe.LoadedObjectContainers != null && Universe.LoadedObjectContainers.Any())
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

                        if (ImGui.MenuItem("Check"))
                        {
                            ActionHandler.ApplyEntityChecker();
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Name Map Objects
            ///--------------------
            // Tool for AC6 since its maps come with unnamed Regions and Events
            if (Project.ProjectType is ProjectType.AC6)
            {
                if (ImGui.BeginMenu("Rename Map Objects"))
                {
                    if (Universe.LoadedObjectContainers != null && Universe.LoadedObjectContainers.Any())
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

                        if (ImGui.MenuItem("Apply Japanese Names"))
                        {
                            DialogResult result = MessageBox.Show(
                            $"This will apply the developer map object names (in Japanese) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object",
                            "Warning",
                            MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                ActionHandler.ApplyMapObjectNames(true);
                            }
                        }

                        if (ImGui.MenuItem("Apply English Names"))
                        {
                            DialogResult result = MessageBox.Show(
                            $"This will apply the developer map object names (in machine translated English) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object",
                            "Warning",
                            MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                ActionHandler.ApplyMapObjectNames(false);
                            }
                        }
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("Applies descriptive name for map objects from developer name list.");
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Filters
        if (ImGui.BeginMenu("Filters", validViewportState))
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

            // Speed Tree
            if (ImGui.MenuItem("Speed Tree"))
            {
                RenderScene.ToggleDrawFilter(RenderFilter.SpeedTree);
            }
            ticked = RenderScene.DrawFilter.HasFlag(RenderFilter.SpeedTree);
            UIHelper.ShowActiveStatus(ticked);

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
                    MapViewport.SetEnvMap(0);
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Scene Lighting"))
            {
                MapViewport.SceneParamsGui();
                ImGui.EndMenu();
            }

            if (Project.ProjectType is ProjectType.ER)
            {
                if (ImGui.BeginMenu("Collision Type"))
                {
                    if (ImGui.MenuItem("Low"))
                    {
                        HavokCollisionManager.VisibleCollisionType = HavokCollisionType.Low;
                    }
                    UIHelper.Tooltip("Visible collision will use the low-detail mesh.\nUsed for standard collision.\nMap must be reloaded after change to see difference.");
                    UIHelper.ShowActiveStatus(HavokCollisionManager.VisibleCollisionType == HavokCollisionType.Low);

                    if (ImGui.MenuItem("High"))
                    {
                        HavokCollisionManager.VisibleCollisionType = HavokCollisionType.High;
                    }
                    UIHelper.Tooltip("Visible collision will use the high-detail mesh.\nUsed for IK.\nMap must be reloaded after change to see difference.");
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
                UIHelper.Tooltip($"Set the gizmo to Translation mode.");

                if (ImGui.MenuItem("Rotate", KeyBindings.Current.VIEWPORT_GizmoRotationMode.HintText))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }
                UIHelper.Tooltip($"Set the gizmo to Rotation mode.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Space"))
            {
                if (ImGui.MenuItem("Local", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }
                UIHelper.Tooltip($"Place the gizmo origin based on the selection's local position.");

                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }
                UIHelper.Tooltip($"Place the gizmo origin based on the selection's world position.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Origin"))
            {
                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }
                UIHelper.Tooltip($"Orient the gizmo origin based on the world position.");

                if (ImGui.MenuItem("Bounding Box", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }
                UIHelper.Tooltip($"Orient the gizmo origin based on the bounding box.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Save process for currently selected map
    /// </summary>
    public void Save()
    {
        if (Project.ProjectType == ProjectType.Undefined)
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

    /// <summary>
    /// Save process for all touched maps
    /// </summary>
    public void SaveAll()
    {
        if (Project.ProjectType == ProjectType.Undefined)
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

    /// <summary>
    /// EXception handling for map save
    /// </summary>
    /// <param name="e"></param>
    public void HandleSaveException(SavingFailedException e)
    {
        if (e.Wrapped is MSB.MissingReferenceException eRef)
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, LogPriority.Normal, e.Wrapped);

            var result = MessageBox.Show($"{eRef.Message}\nSelect referring map entity?",
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

    /// <summary>
    /// Shortcuts for this editor
    /// </summary>
    public void Shortcuts()
    {
        ActionHandler.HandleDuplicateToMapMenuPopup();

        // Keyboard shortcuts
        if (!MapViewport.ViewportUsingKeyboard && !ImGui.IsAnyItemActive())
        {
            var type = CFG.Current.MapEditor_Viewport_GridType;

            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
            {
                Save();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAll))
            {
                SaveAll();
            }

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
                var tempList = Selection.GetFilteredSelection<MsbEntity>().ToList();
                if (tempList != null && tempList.Count > 0)
                {
                    MsbEntity sel = tempList.First();
                    Vector3 pos = (Vector3)sel.GetPropertyValue("Position");
                    CFG.Current.MapEditor_Viewport_Grid_Height = pos.Y;
                }
            }

            // Create
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateMapObject) && Selection.IsSelection())
            {
                ActionHandler.ApplyObjectCreation();
            }

            // Duplicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry) && Selection.IsSelection())
            {
                ActionHandler.ApplyDuplicate();
            }

            // Duplicate to Map
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DuplicateToMap) && Selection.IsSelection())
            {
                ImGui.OpenPopup("##DupeToTargetMapPopup");
            }

            // Delete
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry) && Selection.IsSelection())
            {
                ActionHandler.ApplyDelete();
            }

            // Frame in Viewport
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FrameSelection) && Selection.IsSelection())
            {
                ActionHandler.ApplyFrameInViewport();
            }

            // Go to in Map Object List
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_GoToInList) && Selection.IsSelection())
            {
                ActionHandler.ApplyGoToInObjectList();
            }

            // Move to Camera
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveToCamera) && Selection.IsSelection())
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

            // Negative Rotate (X-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativeRotateSelectionXAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(-1, 0, 0), false);
            }

            // Negative Rotate (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativeRotateSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), false);
            }

            // Negative Rotate Pivot (Y-axis)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_NegativePivotSelectionYAxis))
            {
                ActionHandler.ArbitraryRotation_Selection(new Vector3(0, -1, 0), true);
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
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectUp) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
            }

            // Order (Down)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectDown) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
            }

            // Order (Top)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectTop) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
            }

            // Order (Bottom)
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MoveObjectBottom) && Selection.IsSelection())
            {
                ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
            }

            // Scramble
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ScrambleSelection) && Selection.IsSelection())
            {
                ActionHandler.ApplyScramble();
            }

            // Replicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ReplicateSelection) && Selection.IsSelection())
            {
                ActionHandler.ApplyReplicate();
            }

            // Move to Grid
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetSelectionToGrid) && Selection.IsSelection())
            {
                ActionHandler.ApplyMovetoGrid();
            }

            // Toggle Editor Visibility
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_FlipSelectionVisibility) && Selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableSelectionVisibility) && Selection.IsSelection())
            {
                ActionHandler.ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableSelectionVisibility) && Selection.IsSelection())
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
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeDummyObject) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeNormalObject) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableGamePresence) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
            }
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableGamePresence) && Selection.IsSelection())
            {
                ActionHandler.ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
            }

            // Toggle Selection Outline
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_RenderOutline))
            {
                CFG.Current.Viewport_Enable_Selection_Outline = !CFG.Current.Viewport_Enable_Selection_Outline;
            }

            // Toggle Render Type
            if (InputTracker.GetKeyDown(KeyBindings.Current.VIEWPORT_ToggleRenderType))
            {
                MapEditorUtils.ToggleRenderType(Selection);
            }

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

        /// Toggle Patrol Route Visualisation
        if (Project.ProjectType != ProjectType.DS2S && Project.ProjectType != ProjectType.DS2)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_TogglePatrolRouteRendering))
            {
                if (!PatrolDrawManager.VisualizePatrolRoutes)
                {
                    PatrolDrawManager.VisualizePatrolRoutes = true;
                    PatrolDrawManager.Generate();
                }
                else
                {
                    PatrolDrawManager.Clear();
                    PatrolDrawManager.VisualizePatrolRoutes = false;
                }
            }
        }

        RotationIncrement.Shortcuts();
        KeyboardMovement.Shortcuts();

        //Selection Groups
        SelectionGroupView.SelectionGroupShortcuts();
    }
}
