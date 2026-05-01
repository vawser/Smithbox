using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
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
    public SelectCollisionRefAction SelectCollisionRefAction;

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
        SelectCollisionRefAction = new SelectCollisionRefAction(this, project);

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
        DisplayMenubar();

        // MSB
        if (ImGui.Begin($@"MSB##MsbWindow{ViewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y * CFG.Current.Interace_Editor_Display_Inner_Height_Percent;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            MapListView.Display(width, height * CFG.Current.MapEditor_Display_MapList_Percentage);
            MapContentView.Display(width, height * CFG.Current.MapEditor_Display_Contents_Percentage);
        }

        ImGui.End();

        // Viewport
        ViewportWindow.Display();

        // Properties
        if (ImGui.Begin($@"Properties##MapPropertiesWindow{ViewIndex}", UIHelper.GetInnerWindowFlags()))
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

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.BeginMenu("Map List"))
                {
                    if (ImGui.MenuItem("Unload Current"))
                    {
                        DialogResult result = PlatformUtils.Instance.MessageBox("Unload current?", "Confirm", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            Universe.UnloadMap(Selection.SelectedMapID);
                        }
                    }
                    UIHelper.Tooltip("Unload the currently loaded and selected map.");

                    if (ImGui.MenuItem("Unload All"))
                    {
                        DialogResult result = PlatformUtils.Instance.MessageBox("Unload all maps?", "Confirm", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            Universe.UnloadAllMaps();
                        }
                    }
                    UIHelper.Tooltip("Unload all loaded maps.");

                    if (ImGui.BeginMenu("List Filters"))
                    {
                        if (ImGui.BeginMenu("Select"))
                        {
                            MapListFilterTool.SelectionMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Select an existing list filter to apply to the map list.");

                        if (ImGui.MenuItem("Clear"))
                        {
                            MapListFilterTool.Clear();
                        }
                        UIHelper.Tooltip("Clear the current list filter, resetting the filtering of the map list.");

                        ImGui.Separator();

                        if (ImGui.BeginMenu("Create"))
                        {
                            MapListFilterTool.CreationMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Create a new list filter. The filter terms support regular expressions.");

                        if (ImGui.BeginMenu("Edit"))
                        {
                            MapListFilterTool.EditMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Edit an existing list filter.");

                        if (ImGui.BeginMenu("Delete"))
                        {
                            MapListFilterTool.DeleteMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Delete an existing list filter.");

                        ImGui.EndMenu();
                    }
                    UIHelper.Tooltip("Select a list filter to narrow the map list down to a pre-defined set of maps.");

                    if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
                    {
                        if (ImGui.MenuItem("World Map"))
                        {
                            WorldMapTool.DisplayMenuOption();
                        }
                        UIHelper.Tooltip($"Open a world map with a visual representation of the map tiles.\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)}");

                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Contents"))
                {
                    if (ImGui.BeginMenu("Content Display"))
                    {
                        if (ImGui.MenuItem("Tree"))
                        {
                            MapContentView.ContentViewType = MapContentViewType.ObjectType;
                        }
                        UIHelper.Tooltip("Display the content in the object type tree form.");
                        UIHelper.ShowActiveStatus(MapContentView.ContentViewType == MapContentViewType.ObjectType);

                        if (ImGui.MenuItem("Flat"))
                        {
                            MapContentView.ContentViewType = MapContentViewType.Flat;
                        }
                        UIHelper.Tooltip("Display the content in the flat form.");
                        UIHelper.ShowActiveStatus(MapContentView.ContentViewType == MapContentViewType.Flat);

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Name Display"))
                    {
                        var curType = CFG.Current.MapEditor_MapContentList_EntryNameDisplayType;

                        if (ImGui.MenuItem("Internal"))
                        {
                            CFG.Current.MapEditor_MapContentList_EntryNameDisplayType = EntityNameDisplayType.Internal;
                        }
                        UIHelper.Tooltip("Display the internal map object name only.");
                        UIHelper.ShowActiveStatus(curType == EntityNameDisplayType.Internal);

                        if (ImGui.MenuItem("Internal + Text"))
                        {
                            CFG.Current.MapEditor_MapContentList_EntryNameDisplayType = EntityNameDisplayType.Internal_FMG;
                        }
                        UIHelper.Tooltip("Display the internal map object name with the associated FMG name as the alias.");
                        UIHelper.ShowActiveStatus(curType == EntityNameDisplayType.Internal_FMG);

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                if (LightAtlasBank.CanUse())
                {
                    if (ImGui.BeginMenu("Light Atlases"))
                    {
                        //if (ImGui.BeginMenu("Light Atlases"))
                        //{
                        //    if (ImGui.MenuItem("Automatically adjust entries"))
                        //    {
                        //        CFG.Current.MapEditor_LightAtlas_AutomaticAdjust = !CFG.Current.MapEditor_LightAtlas_AutomaticAdjust;
                        //    }
                        //    UIHelper.Tooltip("If enabled, when a part is renamed, if a light atlas entry points to it, the name reference within the entry is updated to the new name.");
                        //    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdjust);


                        //    if (ImGui.MenuItem("Automatically add entries"))
                        //    {
                        //        CFG.Current.MapEditor_LightAtlas_AutomaticAdd = !CFG.Current.MapEditor_LightAtlas_AutomaticAdd;
                        //    }
                        //    UIHelper.Tooltip("If enabled, when new parts are duplicated, the a new light atlas entry pointing to the newly duplicated part is created (deriving the other properties from the source part).");
                        //    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdd);

                        //    if (ImGui.MenuItem("Automatically delete entries"))
                        //    {
                        //        CFG.Current.MapEditor_LightAtlas_AutomaticDelete = !CFG.Current.MapEditor_LightAtlas_AutomaticDelete;
                        //    }
                        //    UIHelper.Tooltip("If enabled, when parts are deleted, if there is a light atlas entry pointing to that part, the entry is deleted.");
                        //    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticDelete);

                        //    ImGui.EndMenu();
                        //}

                        ImGui.EndMenu();
                    }
                }

                if (ImGui.BeginMenu("Display"))
                {
                    ImGui.SliderFloat("Map List##mapListDisplayPercentage", ref CFG.Current.MapEditor_Display_MapList_Percentage, 0.01f, 0.99f);
                    if(ImGui.IsItemDeactivatedAfterEdit())
                    {
                        // Auto-adjust the other var so the ratio remains 100%
                        CFG.Current.MapEditor_Display_Contents_Percentage = 1 - CFG.Current.MapEditor_Display_MapList_Percentage;
                    }
                    UIHelper.Tooltip("The percentage of the window the Map List section occupies.");

                    ImGui.SliderFloat("Contents##mapContentsDisplayPercentage", ref CFG.Current.MapEditor_Display_Contents_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        // Auto-adjust the other var so the ratio remains 100%
                        CFG.Current.MapEditor_Display_MapList_Percentage = 1 - CFG.Current.MapEditor_Display_Contents_Percentage;
                    }
                    UIHelper.Tooltip("The percentage of the window the Contents section occupies.");

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
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
