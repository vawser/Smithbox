using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor;

public class MapEditorView
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

    public AutoInvadeBank AutoInvadeBank;
    public HavokCollisionBank HavokCollisionBank;
    public HavokNavmeshBank HavokNavmeshBank;
    public LightAtlasBank LightAtlasBank;
    public LightProbeBank LightProbeBank;

    public MapSelection Selection;
    public ViewportSelection ViewportSelection = new();

    public MapActionHandler ActionHandler;
    public MapEntityTypeCache EntityTypeCache;
    public MapPropertyCache MapPropertyCache = new();

    public MapViewportView ViewportWindow;
    public MapListView MapListView;
    public MapContentView MapContentView;
    public MapPropertyView MapPropertyView;

    public BasicFilters BasicFilters;
    public RegionFilters RegionFilters;
    public MapContentFilters MapContentFilter;

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
    public RotateAction RotateAction;
    public ScrambleAction ScrambleAction;
    public ReplicateAction ReplicateAction;
    public RenderTypeAction RenderTypeAction;
    public SelectAllAction SelectAllAction;
    public EditorVisibilityAction EditorVisibilityAction;
    public GameVisibilityAction GameVisibilityAction;
    public SelectionOutlineAction SelectionOutlineAction;
    public AdjustToGridAction AdjustToGridAction;
    public EntityInfoAction EntityInfoAction;
    public EntityIdCheckAction EntityIdCheckAction;
    public EntityRenameAction EntityRenameAction;

    // Tools
    public MassEditTool MassEditTool;
    public RotationIncrementTool RotationIncrementTool;
    public PositionIncrementTool PositionIncrementTool;
    public ModelSelectorTool ModelSelectorTool;
    public DisplayGroupTool DisplayGroupTool;
    public SelectionGroupTool SelectionGroupTool;
    public PrefabTool PrefabTool;
    public NavmeshBuilderTool NavmeshBuilderTool;
    public LocalSearchTool LocalSearchView;
    public GlobalSearchTool GlobalSearchTool;
    public WorldMapTool WorldMapTool;
    public EntityIdentifierTool EntityIdentifierTool;
    public MapGridTool MapGridTool;
    public WorldMapLayoutTool WorldMapLayoutTool;
    public MapListFilterTool MapListFilterTool;
    public MapValidatorTool MapValidatorTool;
    public MapModelInsightView MapModelInsightTool;
    public MapModelInsightHelper ModelInsightTool;
    public AutomaticPreviewTool AutomaticPreviewTool;
    public PatrolDrawManager PatrolDrawManager;

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

        HavokCollisionBank = new HavokCollisionBank(this, project);
        HavokNavmeshBank = new HavokNavmeshBank(this, project);
        AutoInvadeBank = new AutoInvadeBank(this, project);
        LightAtlasBank = new LightAtlasBank(this, project);
        LightProbeBank = new LightProbeBank(this, project);

        Selection = new(this, project);

        ViewportWindow = new MapViewportView(this, project);

        // Core Views
        MapListView = new MapListView(this, project);
        MapContentView = new MapContentView(this, project);
        MapPropertyView = new MapPropertyView(this, project);

        // Optional Views
        BasicFilters = new BasicFilters(this);
        RegionFilters = new RegionFilters(this);
        MapContentFilter = new MapContentFilters(this);

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
        RotateAction = new RotateAction(this, project);
        ScrambleAction = new ScrambleAction(this, project);
        ReplicateAction = new ReplicateAction(this, project);
        RenderTypeAction = new RenderTypeAction(this, project);
        SelectAllAction = new SelectAllAction(this, project);
        EditorVisibilityAction = new EditorVisibilityAction(this, project);
        GameVisibilityAction = new GameVisibilityAction(this, project);
        SelectionOutlineAction = new SelectionOutlineAction(this, project);
        AdjustToGridAction = new AdjustToGridAction(this, project);
        EntityInfoAction = new EntityInfoAction(this, project);
        EntityIdCheckAction = new EntityIdCheckAction(this, project);
        EntityRenameAction = new EntityRenameAction(this, project);

        // Tools
        MassEditTool = new MassEditTool(this, project);
        RotationIncrementTool = new RotationIncrementTool(this, project);
        PositionIncrementTool = new PositionIncrementTool(this, project);
        AutomaticPreviewTool = new AutomaticPreviewTool(this, project);
        DisplayGroupTool = new DisplayGroupTool(this, project);
        GlobalSearchTool = new GlobalSearchTool(this, project);
        LocalSearchView = new LocalSearchTool(this, project);
        ModelSelectorTool = new ModelSelectorTool(this, project);
        PrefabTool = new PrefabTool(this, project);
        SelectionGroupTool = new SelectionGroupTool(this, project);
        NavmeshBuilderTool = new NavmeshBuilderTool(this, project);
        EntityIdentifierTool = new EntityIdentifierTool(this, project);
        MapGridTool = new MapGridTool(this, project);
        WorldMapTool = new WorldMapTool(this, project);
        WorldMapLayoutTool = new WorldMapLayoutTool(this, project);
        MapListFilterTool = new MapListFilterTool(this, project);
        MapValidatorTool = new MapValidatorTool(this, project);
        MapModelInsightTool = new MapModelInsightView(this, project);
        ModelInsightTool = new MapModelInsightHelper(this, project);
        PatrolDrawManager = new PatrolDrawManager(this);

        ViewportActionManager.AddEventHandler(MapListView);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        // MSB
        if (ImGui.Begin($@"MSB##MsbWindow{ViewIndex}", UIHelper.GetMainWindowFlags()))
        {
            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            MapListView.Display(width, height * 0.25f);
            MapContentView.Display(width, height * 0.75f);
        }

        ImGui.End();

        // Viewport
        ViewportWindow.Display();

        // Properties
        if (ImGui.Begin($@"Properties##MapPropertiesWindow{ViewIndex}", UIHelper.GetMainWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            MapPropertyView.Display();
        }

        ImGui.End();

        MapListFilterTool.Update();
        LocalSearchView.Update();

        SelectionGroupTool.DisplayPopup();
        WorldMapTool.DisplayPopup();

        ViewportSelection.ClearGotoTarget();
    }
}
