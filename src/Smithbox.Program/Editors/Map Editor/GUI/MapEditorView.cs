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
    public MultiPlayerWallBank MultiPlayerWallBank => Project.Handler.MapData.MultiPlayerWallBank;
    public FogVolumeBank FogVolumeBank => Project.Handler.MapData.FogVolumeBank;
    public IrradianceVolumeBank IrradianceVolumeBank => Project.Handler.MapData.IrradianceVolumeBank;
    public GparamLightingBank GparamLightingBank => Project.Handler.MapData.GparamLightingBank;
    public DrawParamLightingBank DrawParamLightingBank => Project.Handler.MapData.DrawParamLightingBank;
    public MapLimitBank MapLimitBank => Project.Handler.MapData.MapLimitBank;
    public HavokCollisionBank HavokCollisionBank => Project.Handler.MapData.HavokCollisionBank;
    public GrassBank GrassBank => Project.Handler.MapData.GrassBank;
    public EdgeBank EdgeBank => Project.Handler.MapData.EdgeBank;
    public WallBank WallBank => Project.Handler.MapData.WallBank;
    public HavokNavmeshBank HavokNavmeshBank => Project.Handler.MapData.HavokNavmeshBank;
    public NavigationBank NavigationBank => Project.Handler.MapData.NavigationBank;
    public NvmBank NvmBank => Project.Handler.MapData.NvmBank;
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
    public SelectionGroupsTool SelectionGroupsTool;
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
    public SnapToMeshAction SnapToMeshAction;
    public TranslateAction TranslateAction;
    public RotateAction RotateAction;
    public ScrambleAction ScrambleAction;
    public ReplicateAction ReplicateAction;
    public SelectAllAction SelectAllAction;
    public EditorVisibilityAction EditorVisibilityAction;
    public GameVisibilityAction GameVisibilityAction;
    public LowDetailVisibilityAction LowDetailVisibilityAction;
    public SelectionOutlineAction SelectionOutlineAction;
    public AdjustToGridAction AdjustToGridAction;
    public EntityInfoAction EntityInfoAction;
    public EntityIdCheckAction EntityIdCheckAction;
    public EntityRenameAction EntityRenameAction;
    public SelectCollisionRefAction SelectCollisionRefAction;
    public ViewportFiltersAction ViewportFiltersAction;
    public BoxSelectionAction BoxSelectionAction;

    // Tools
    public MsbMassEditTool MassEditTool;
    public ModelSelectorTool ModelBrowserTool;
    public AssetBrowserTool AssetBrowserTool;
    public DisplayGroupTool DisplayGroupTool;
    public PrefabTool PrefabTool;
    public MsbSearchTool LocalSearchTool;
    public MsbSearchTool GlobalSearchTool;
    public WorldMapTool WorldMapTool;
    public EntityIdentifierTool EntityIdentifierTool;
    public ViewportGridTool GridTool;
    public ViewportRenderingTool RenderingTool;
    public MapAnimationTool MapAnimationTool;
    public WorldMapLayoutTool WorldMapLayoutTool;
    public MapListFilterTool MapListFilterTool;
    public MapValidatorTool MapValidatorTool;
    public VanillaCompareTool VanillaCompareTool;
    public IrradianceVolumeIdTool IrradianceVolumeIdTool;
    public MapModelInsightView MapModelInsightTool;
    public MapModelInsightHelper ModelInsightTool;
    public AutomaticPreviewTool AutomaticPreviewTool;
    public PatrolDrawManager PatrolDrawManager;
    public CompassTool CompassTool;
    public EnemyAIVisualizationTool EnemyAIVisualizationTool;
    public WallPaintTool WallPaintTool;
    public CollisionEditorTool CollisionEditorTool;
    public NavigationTool NavigationTool;
    public MapRulerTool RulerTool;
    public FirstPersonCollisionWorld FirstPersonCollision;
    public FirstPersonTool FirstPersonTool;
    public MapExtractionTool MapExtractionTool;
    public MapTextureTransferTool TextureTransferTool;
    public MapPartTransferTool PartTransferTool;
    public DestructionGenerationTool DestructionGenerationTool;
    public LoadListTool LoadListTool;
    public EntryFileListTool EntryFileListTool;
    public PartsTokenIdListTool PartsTokenIdListTool;

    public ResourceListTool ResourceListTool;

    public MapEditorView(MapEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;
        MapPropertyCache.HavokProfile = StudioCore.Editors.HavokEditor.HavokTypeUtils.GetProfile(project);

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
        SelectionGroupsTool = new SelectionGroupsTool(this, project);
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
        SnapToMeshAction = new SnapToMeshAction(this, project);
        TranslateAction = new TranslateAction(this, project);
        RotateAction = new RotateAction(this, project);
        ScrambleAction = new ScrambleAction(this, project);
        ReplicateAction = new ReplicateAction(this, project);
        SelectAllAction = new SelectAllAction(this, project);
        EditorVisibilityAction = new EditorVisibilityAction(this, project);
        GameVisibilityAction = new GameVisibilityAction(this, project);
        LowDetailVisibilityAction = new LowDetailVisibilityAction(this, project);
        SelectionOutlineAction = new SelectionOutlineAction(this, project);
        AdjustToGridAction = new AdjustToGridAction(this, project);
        EntityInfoAction = new EntityInfoAction(this, project);
        EntityIdCheckAction = new EntityIdCheckAction(this, project);
        EntityRenameAction = new EntityRenameAction(this, project);
        SelectCollisionRefAction = new SelectCollisionRefAction(this, project);
        ViewportFiltersAction = new ViewportFiltersAction(this, project);
        BoxSelectionAction = new BoxSelectionAction(this, project);

        // Tools
        MassEditTool = new MsbMassEditTool(this, project);
        AutomaticPreviewTool = new AutomaticPreviewTool(this, project);
        DisplayGroupTool = new DisplayGroupTool(this, project);
        GlobalSearchTool = new MsbSearchTool(this, project, MsbSearchMode.Global);
        LocalSearchTool = new MsbSearchTool(this, project, MsbSearchMode.Local);
        ModelBrowserTool = new ModelSelectorTool(this, project);
        AssetBrowserTool = new AssetBrowserTool(project,
            "Double-click an asset to switch the selected map objects to it.",
            e => ModelBrowserTool.ApplyMapAssetSelection(e.ID, FileSelectionType.Asset));
        PrefabTool = new PrefabTool(this, project);
        EntityIdentifierTool = new EntityIdentifierTool(this, project);
        GridTool = new ViewportGridTool(ViewportGridEditor.Map, GetCurrentViewport);
        RenderingTool = new ViewportRenderingTool(GetCurrentViewport);
        MapAnimationTool = new MapAnimationTool(this, project);
        WorldMapTool = new WorldMapTool(this, project);
        WorldMapLayoutTool = new WorldMapLayoutTool(this, project);
        MapListFilterTool = new MapListFilterTool(this, project);
        MapValidatorTool = new MapValidatorTool(this, project);
        VanillaCompareTool = new VanillaCompareTool(this, project);
        IrradianceVolumeIdTool = new IrradianceVolumeIdTool(this, project);
        MapModelInsightTool = new MapModelInsightView(this, project);
        ModelInsightTool = new MapModelInsightHelper(this, project);
        PatrolDrawManager = new PatrolDrawManager(this);
        CompassTool = new CompassTool(this, project);
        EnemyAIVisualizationTool = new EnemyAIVisualizationTool(this, project);
        WallPaintTool = new WallPaintTool(this, project);
        CollisionEditorTool = new CollisionEditorTool(this, project);
        NavigationTool = new NavigationTool(this, project);
        RulerTool = new MapRulerTool(this, project);
        FirstPersonCollision = new FirstPersonCollisionWorld(this, project);
        FirstPersonTool = new FirstPersonTool(this, project);
        MapExtractionTool = new MapExtractionTool(this, project);
        TextureTransferTool = new MapTextureTransferTool(this, project);
        PartTransferTool = new MapPartTransferTool(this, project);
        DestructionGenerationTool = new DestructionGenerationTool(this, project);
        LoadListTool = new LoadListTool(this, project);
        EntryFileListTool = new EntryFileListTool(this, project);
        PartsTokenIdListTool = new PartsTokenIdListTool(this, project);

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

        Universe.ProcessLoadQueue();

        SyncActiveMapToSelection();

        MapListFilterTool.Update();
        MapAnimationTool.Update();

        // Group shortcuts, so they work while the Tools window section is hidden
        SelectionGroupsTool.Update();

        // After the Tools window, which marks the frames the Snap to Mesh section is shown
        SnapToMeshAction.Update();

        WorldMapTool.Update();

        // Picks up a finished first person collision build, and checks the walk is still where it can run
        FirstPersonCollision.Update();
        FirstPersonTool.UpdateSession();

        // The popups that context menus, shortcuts and the Tools window open. Drawn here rather than in the
        // Tools window so they don't depend on it being shown.
        DuplicateToMapAction.OnGui();
        MoveToMapAction.OnGui();
        SelectAllAction.OnGui();
        AdjustToGridAction.OnGui();

        ViewportSelection.ClearGotoTarget();
    }

    private void DisplayWindows(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Map List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"{LOC.Get("MAP_Window_Map_List")}##mapEditor_MapList_{viewIndex}", GUI.GetInnerWindowFlags()))
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
        if (ImGui.Begin($@"{LOC.Get("MAP_Window_Map_Contents")}##mapEditor_MapContents_{viewIndex}", GUI.GetInnerWindowFlags()))
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

        // Viewport or Map Data
        DisplayCenterWindow(dockspaceId, viewIndex);

        // Properties
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"{LOC.Get("MAP_Window_Properties")}##mapEditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
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
            if (ImGui.Begin($@"{LOC.Get("MAP_Window_Tools")}##mapEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
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

        // Map Information
        WorldMapTool.Display(dockspaceId, viewIndex, GetCurrentViewport()?.LastDockId ?? 0);
    }


    private void DisplayTabbedPanels(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Left Panel
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);
        if (ImGui.Begin($@"{LOC.Get("MAP_Window_Left_Panel")}##mapEditor_LeftPanel_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            // Only the hovered panel takes the active view, as in Windows mode; otherwise every open view
            // claims it each frame and the last one drawn always wins
            var hovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);

            ImGui.BeginTabBar("leftPanelTabs");

            if(ImGui.BeginTabItem(LOC.Get("MAP_Window_Map_List")))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_FileList);

                if (hovered)
                    Editor.ViewHandler.ActiveView = this;

                MapListView.Display(width, height);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(LOC.Get("MAP_Window_Map_Contents")))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_ContentTree);

                if (hovered)
                    Editor.ViewHandler.ActiveView = this;

                MapContentView.Display(width, height);

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
        if (ImGui.Begin($@"{LOC.Get("MAP_Window_Right_Panel")}##mapEditor_RightPanel_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            var hovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows);

            ImGui.BeginTabBar("rightPanelTabs");

            if (ImGui.BeginTabItem(LOC.Get("MAP_Window_Properties")))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_Properties);

                if (hovered)
                    Editor.ViewHandler.ActiveView = this;

                MapPropertyView.Display();

                ImGui.EndTabItem();
            }

            if (CFG.Current.Interface_MapEditor_ToolWindow)
            {
                if (ImGui.BeginTabItem(LOC.Get("MAP_Window_Tools")))
                {
                    FocusManager.SetFocus(EditorFocusContext.MapEditor_Tools);

                    if (hovered)
                        Editor.ViewHandler.ActiveView = this;

                    ToolView.Display();

                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        ImGui.End();

        // Map Information: its own window here too, as it needs the room of a viewport
        WorldMapTool.Display(dockspaceId, viewIndex, GetCurrentViewport()?.LastDockId ?? 0);
    }

    private Entity _lastSyncedSelection;

    /// <summary>
    /// Makes the map of a newly selected object the active map (the one the content tree shows and the
    /// tools default to). Only done when the selection changes, so picking another map in the Map List
    /// while an object is still selected isn't undone on the next frame.
    /// </summary>
    private void SyncActiveMapToSelection()
    {
        var first = ViewportSelection.GetSelection().OfType<Entity>().FirstOrDefault();

        if (first == _lastSyncedSelection)
            return;

        _lastSyncedSelection = first;

        if (first?.Container is MapContainer map &&
            map.LoadState is MapContentLoadState.Loaded &&
            map != Selection.SelectedMapContainer)
        {
            Selection.SelectedMapID = map.Name;
            Selection.SelectedMapContainer = map;
        }
    }

    /// <summary>
    /// Releases what the view holds once it has been closed and its maps unloaded: its viewports' render
    /// pipelines (GPU targets, passes and queues), the placement orb, its patrol routes, selection and history.
    /// The view must not be displayed or updated afterwards.
    /// </summary>
    public void Dispose()
    {
        ViewportSelection.ClearSelection();
        ViewportActionManager.Clear();

        PatrolDrawManager.Clear();

        ViewportWindow.PlacementOrb?.Dispose();
        ViewportWindow.PlacementOrb = null;

        ViewportHandler.Dispose();
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
        if (ImGui.Begin($@"{LOC.Get("MAP_Window_Map_Data")}##mapEditor_MapData_{viewIndex}", GUI.GetInnerWindowFlags()))
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
