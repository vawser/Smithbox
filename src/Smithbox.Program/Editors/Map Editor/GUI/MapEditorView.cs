using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor;

public class MapEditorView : IEditorView
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public Sdl2Window Window;
    public GraphicsDevice Device;

    public MapViewportHandler ViewportHandler;

    public ViewportActionManager ViewportActionManager = new();
    public ActionManager ActionManager = new();

    public int ViewIndex;

    public MapUniverse Universe;

    // Project-level banks, shared by all views
    public AutoInvadeBank AutoInvadeBank => Project.Handler.MapData.AutoInvadeBank;
    public HavokCollisionBank HavokCollisionBank => Project.Handler.MapData.HavokCollisionBank;
    public GrassBank GrassBank => Project.Handler.MapData.GrassBank;
    public EdgeBank EdgeBank => Project.Handler.MapData.EdgeBank;
    public HavokNavmeshBank HavokNavmeshBank => Project.Handler.MapData.HavokNavmeshBank;
    public LightAtlasBank LightAtlasBank => Project.Handler.MapData.LightAtlasBank;
    public LightProbeBank LightProbeBank => Project.Handler.MapData.LightProbeBank;
    public AssetConfigurationBank AssetConfigurationBank => Project.Handler.MapData.AssetConfigurationBank;


    public MapSelection Selection;
    public ViewportSelection ViewportSelection = new();

    public MapActionHandler ActionHandler;
    public MapEntityTypeCache EntityTypeCache;
    public MapPropertyCache MapPropertyCache = new();

    public MapViewportView ViewportWindow;
    public MapListView MapListView;
    public MapContentView MapContentView;
    public MapGroupsView MapGroupsView;
    public MapPropertyView MapPropertyView;
    public MapToolWindow ToolView;
    public MapDataView MapDataView;

    /// <summary>
    /// When true, the Map Data window is displayed in place of the viewport.
    /// </summary>
    public bool ShowMapData { get; private set; } = false;

    private bool _focusCenterWindow = false;

    public BasicFilters BasicFilters;
    public RegionFilters RegionFilters;
    public MapContentTreeSearch MapContentFilter;

    // Actions
    public CreateAction CreateAction;
    public DuplicateAction DuplicateAction;
    public DeleteAction DeleteAction;
    public DuplicateToMapAction DuplicateToMapAction;
    public MoveToMapAction MoveToMapAction;
    public ReorderAction ReorderAction;
    public GotoAction GotoAction;
    public FrameAction FrameAction;
    public PullToCameraAction PullToCameraAction;
    public TranslateAction TranslateAction;
    public RotateAction RotateAction;
    public ScrambleAction ScrambleAction;
    public ReplicateAction ReplicateAction;
    public SelectAllAction SelectAllAction;
    public EditorVisibilityAction EditorVisibilityAction;
    public GameVisibilityAction GameVisibilityAction;
    public SelectionOutlineAction SelectionOutlineAction;
    public AdjustToGridAction AdjustToGridAction;
    public EntityInfoAction EntityInfoAction;
    public EntityIdCheckAction EntityIdCheckAction;
    public EntityRenameAction EntityRenameAction;
    public SelectCollisionRefAction SelectCollisionRefAction;
    public ViewportFiltersAction ViewportFiltersAction;
    public BoxSelectionAction BoxSelectionAction;

    // Tools
    public MassEditTool MassEditTool;
    public ModelSelectorTool ModelSelectorTool;
    public AssetBrowserTool AssetBrowserTool;
    public DisplayGroupTool DisplayGroupTool;
    public PrefabTool PrefabTool;
    public NavmeshBuilderTool NavmeshBuilderTool;
    public LocalSearchTool LocalSearchView;
    public GlobalSearchTool GlobalSearchTool;
    public WorldMapTool WorldMapTool;
    public EntityIdentifierTool EntityIdentifierTool;
    public MapGridTool MapGridTool;
    public MapAnimationTool MapAnimationTool;
    public WorldMapLayoutTool WorldMapLayoutTool;
    public MapListFilterTool MapListFilterTool;
    public MapValidatorTool MapValidatorTool;
    public MapModelInsightView MapModelInsightTool;
    public MapModelInsightHelper ModelInsightTool;
    public AutomaticPreviewTool AutomaticPreviewTool;
    public PatrolDrawManager PatrolDrawManager;
    public CompassTool CompassTool;

    public ResourceListTool ResourceListTool;

    public MapEditorView(MapEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        Window = Smithbox.Instance._context.Window;
        Device = Smithbox.Instance._context.Device;

        ViewIndex = imguiId;

        Universe = new MapUniverse(this, project);

        ViewportHandler = new(this);

        EntityTypeCache = new MapEntityTypeCache(this, project);

        Selection = new(this, project);

        ViewportWindow = new MapViewportView(this, project);

        // Core Views
        MapListView = new MapListView(this, project);
        MapContentView = new MapContentView(this, project);
        MapGroupsView = new MapGroupsView(this, project);
        MapPropertyView = new MapPropertyView(this, project);
        ToolView = new MapToolWindow(this, project);
        MapDataView = new MapDataView(this, project);

        // Optional Views
        BasicFilters = new BasicFilters(this);
        RegionFilters = new RegionFilters(this);
        MapContentFilter = new MapContentTreeSearch(this);

        // Framework
        ActionHandler = new MapActionHandler(this, project);

        // Actions
        CreateAction = new CreateAction(this, project);
        DuplicateAction = new DuplicateAction(this, project);
        DeleteAction = new DeleteAction(this, project);
        DuplicateToMapAction = new DuplicateToMapAction(this, project);
        MoveToMapAction = new MoveToMapAction(this, project);
        ReorderAction = new ReorderAction(this, project);
        GotoAction = new GotoAction(this, project);
        FrameAction = new FrameAction(this, project);
        PullToCameraAction = new PullToCameraAction(this, project);
        TranslateAction = new TranslateAction(this, project);
        RotateAction = new RotateAction(this, project);
        ScrambleAction = new ScrambleAction(this, project);
        ReplicateAction = new ReplicateAction(this, project);
        SelectAllAction = new SelectAllAction(this, project);
        EditorVisibilityAction = new EditorVisibilityAction(this, project);
        GameVisibilityAction = new GameVisibilityAction(this, project);
        SelectionOutlineAction = new SelectionOutlineAction(this, project);
        AdjustToGridAction = new AdjustToGridAction(this, project);
        EntityInfoAction = new EntityInfoAction(this, project);
        EntityIdCheckAction = new EntityIdCheckAction(this, project);
        EntityRenameAction = new EntityRenameAction(this, project);
        SelectCollisionRefAction = new SelectCollisionRefAction(this, project);
        ViewportFiltersAction = new ViewportFiltersAction(this, project);
        BoxSelectionAction = new BoxSelectionAction(this, project);

        // Tools
        MassEditTool = new MassEditTool(this, project);
        AutomaticPreviewTool = new AutomaticPreviewTool(this, project);
        DisplayGroupTool = new DisplayGroupTool(this, project);
        GlobalSearchTool = new GlobalSearchTool(this, project);
        LocalSearchView = new LocalSearchTool(this, project);
        ModelSelectorTool = new ModelSelectorTool(this, project);
        AssetBrowserTool = new AssetBrowserTool(project,
            "Double-click an asset to switch the selected map objects to it.",
            e => ModelSelectorTool.ApplyMapAssetSelection(e.ID, FileSelectionType.Asset));
        PrefabTool = new PrefabTool(this, project);
        NavmeshBuilderTool = new NavmeshBuilderTool(this, project);
        EntityIdentifierTool = new EntityIdentifierTool(this, project);
        MapGridTool = new MapGridTool(this, project);
        MapAnimationTool = new MapAnimationTool(this, project);
        WorldMapTool = new WorldMapTool(this, project);
        WorldMapLayoutTool = new WorldMapLayoutTool(this, project);
        MapListFilterTool = new MapListFilterTool(this, project);
        MapValidatorTool = new MapValidatorTool(this, project);
        MapModelInsightTool = new MapModelInsightView(this, project);
        ModelInsightTool = new MapModelInsightHelper(this, project);
        PatrolDrawManager = new PatrolDrawManager(this);
        CompassTool = new CompassTool(this, project);

        ResourceListTool = new ResourceListTool();

        ViewportActionManager.AddEventHandler(MapListView);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        if (CFG.Current.Interface_MapEditor_EditorDisplayType is EditorDisplayType.Windows)
        {
            DisplayWindows(dockspaceId, viewIndex, doFocus, isActiveView);
        }

        if (CFG.Current.Interface_MapEditor_EditorDisplayType is EditorDisplayType.TabbedPanels)
        {
            DisplayTabbedPanels(dockspaceId, viewIndex, doFocus, isActiveView);
        }

        MapListFilterTool.Update();
        LocalSearchView.Update();
        MapAnimationTool.Update();

        WorldMapTool.DisplayPopup();

        ViewportSelection.ClearGotoTarget();
    }

    private void DisplayWindows(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Map List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"Map List##mapEditor_MapList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            MapListView.Display(width, height);
        }

        ImGui.End();

        // Map Contents
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"Map Contents##mapEditor_MapContents_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_ContentTree);
                Editor.ViewHandler.ActiveView = this;
            }

            MapContentView.Display(width, height);
        }

        ImGui.End();

        // Map Groups
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"Map Content Groups##mapEditor_MapGroups_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_MapGroups);
                Editor.ViewHandler.ActiveView = this;
            }

            MapGroupsView.Display(width, height);
        }

        ImGui.End();

        // Viewport or Map Data
        DisplayCenterWindow(dockspaceId, viewIndex);

        // Properties
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"Properties##mapEditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            MapPropertyView.Display();
        }

        ImGui.End();

        if (CFG.Current.Interface_MapEditor_ToolWindow)
        {
            // Tools
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
            if (ImGui.Begin($@"Tools##mapEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.MapEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;
                }

                ToolView.Display();
            }

            ImGui.End();
        }
    }


    private void DisplayTabbedPanels(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Left Panel
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"Left Panel##mapEditor_LeftPanel_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            ImGui.BeginTabBar("leftPanelTabs");

            if(ImGui.BeginTabItem("Map List"))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_FileList);
                Editor.ViewHandler.ActiveView = this;

                MapListView.Display(width, height);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Map Contents"))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_ContentTree);
                Editor.ViewHandler.ActiveView = this;

                MapContentView.Display(width, height);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Map Content Groups"))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_MapGroups);
                Editor.ViewHandler.ActiveView = this;

                MapGroupsView.Display(width, height);

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();

        // Viewport or Map Data
        DisplayCenterWindow(dockspaceId, viewIndex);

        // Right Panel
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"Right Panel##mapEditor_RightPanel_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            ImGui.BeginTabBar("rightPanelTabs");

            if (ImGui.BeginTabItem("Properties"))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_Properties);
                Editor.ViewHandler.ActiveView = this;

                MapPropertyView.Display();

                ImGui.EndTabItem();
            }

            if (CFG.Current.Interface_MapEditor_ToolWindow)
            {
                if (ImGui.BeginTabItem("Tools"))
                {
                    FocusManager.SetFocus(EditorFocusContext.MapEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;

                    ToolView.Display();

                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    public void ToggleMapData()
    {
        ShowMapData = !ShowMapData;
        _focusCenterWindow = true;
    }

    private void DisplayCenterWindow(uint dockspaceId, int viewIndex)
    {
        // Bring the newly shown window to the front of its dock node
        if (_focusCenterWindow)
        {
            _focusCenterWindow = false;
            ImGui.SetNextWindowFocus();
        }

        if (!ShowMapData)
        {
            ViewportWindow.Display(dockspaceId);
            return;
        }

        // Follow the viewport into its dock node, so the two windows swap in place
        var viewportDockId = GetCurrentViewport()?.LastDockId ?? 0;

        if (viewportDockId != 0)
        {
            ImGui.SetNextWindowDockID(viewportDockId, ImGuiCond.Appearing);
        }
        else
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        }

        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"Map Data##mapEditor_MapData_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_MapData);
                Editor.ViewHandler.ActiveView = this;
            }

            MapDataView.Display();
        }

        ImGui.End();
    }

    public VulkanViewport GetCurrentViewport()
    {
        if (ViewportHandler.ActiveViewport.Viewport is VulkanViewport vulkanViewport)
        {
            return vulkanViewport;
        }

        return null;
    }

    public void DelayPicking()
    {
        // Delay picking since the menu can be over the viewport,
        // so a user might click the menu action, and then the click registers in the viewport,
        // wiping the select all selection.
        if (ViewportHandler.ActiveViewport.Viewport is VulkanViewport vulkanViewport)
        {
            vulkanViewport.ClickSelection.TriggerCooldown();
        }
    }
}
