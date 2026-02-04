using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Main interface for the MSB Editor.
/// </summary>
public class MapEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    /// <summary>
    /// Lock variable used to handle pauses to the Update() function.
    /// </summary>
    private static readonly object _lock_PauseUpdate = new();
    private bool GCNeedsCollection;
    private bool _PauseUpdate;

    public ViewportActionManager EditorActionManager = new();
    public MapActionHandler ActionHandler;
    public ViewportSelection ViewportSelection = new();
    public MapSelection Selection;
    public Universe Universe;
    public MapEntityTypeCache EntityTypeCache;
    public MapPropertyCache MapPropertyCache = new();
    public MapCommandQueue CommandQueue;
    public MapShortcuts Shortcuts;

    public AutoInvadeBank AutoInvadeBank;
    public HavokCollisionBank HavokCollisionBank;
    public HavokNavmeshBank HavokNavmeshBank;
    public LightAtlasBank LightAtlasBank;
    public LightProbeBank LightProbeBank;

    // Core
    public MapViewportView MapViewportView;
    public MapListView MapListView;
    public MapContentView MapContentView;
    public MapPropertyView MapPropertyView;

    // Menubar
    public BasicFilters BasicFilters;
    public RegionFilters RegionFilters;
    public MapContentFilters MapContentFilter;

    // Tools
    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;

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

    // Special Tools
    public AutomaticPreviewTool AutomaticPreviewTool;

    public MapEditorScreen(ProjectEntry project)
    {
        Project = project;

        MapViewportView = new MapViewportView(this, project);
        MapViewportView.Setup();

        Universe = new Universe(this, project);
        EntityTypeCache = new(this);

        Selection = new(this, project);

        // Core Views
        MapListView = new MapListView(this, project);
        MapContentView = new MapContentView(this, project);
        MapPropertyView = new MapPropertyView(this);

        // Optional Views
        BasicFilters = new BasicFilters(this);
        RegionFilters = new RegionFilters(this);
        MapContentFilter = new MapContentFilters(this);

        // Framework
        ActionHandler = new MapActionHandler(this, project);
        CommandQueue = new MapCommandQueue(this);
        Shortcuts = new MapShortcuts(this);

        HavokCollisionBank = new HavokCollisionBank(this, project);
        HavokNavmeshBank = new HavokNavmeshBank(this, project);
        AutoInvadeBank = new AutoInvadeBank(this, project);
        LightAtlasBank = new LightAtlasBank(this, project);
        LightProbeBank = new LightProbeBank(this, project);

        // Tools
        ToolWindow = new ToolWindow(this, ActionHandler);
        ToolSubMenu = new ToolSubMenu(this, ActionHandler);

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

        MapModelInsightHelper.Setup(this, project);

        // Focus
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
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] initcmd)
    {
        if (Project.IsInitializing)
            return;

        var scale = DPI.UIScale();

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
        ToolSubMenu.Shortcuts();
        CommandQueue.Parse(initcmd);

        DuplicateToMapAction.OnGui();
        MoveToMapAction.OnGui();
        SelectAllAction.OnGui();
        AdjustToGridAction.OnGui();

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
        //ImGui.Text($@"Viewport size: {Viewport.Width}x{Viewport.Height}");
        //ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        MapViewportView.OnGui();
        MapListView.OnGui();
        MapContentView.OnGui();

        if (Smithbox.FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        if (MapPropertyView.Focus)
        {
            MapPropertyView.Focus = false;
            ImGui.SetNextWindowFocus();
        }

        MapPropertyView.OnGui(ViewportSelection, "mapeditprop", MapViewportView.Viewport.Width, MapViewportView.Viewport.Height);

        SelectionGroupTool.OnGui();
        LocalSearchView.OnGui();
        WorldMapTool.DisplayWorldMap();
        ResourceLoadWindow.DisplayWindow(MapViewportView.Viewport.Width, MapViewportView.Viewport.Height);
        if (CFG.Current.Interface_MapEditor_ResourceList)
        {
            ResourceListWindow.DisplayWindow("mapResourceList", this);
        }

        if (CFG.Current.Interface_MapEditor_ToolWindow)
        {
            ToolWindow.OnGui();
        }

        ImGui.PopStyleColor(1);
    }

    public void OnDefocus()
    {
    }

    public void Update(float dt)
    {
        if (Project.IsInitializing)
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

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"MSB"))
                {
                    CFG.Current.MapEditor_ManualSave_IncludeMSB = !CFG.Current.MapEditor_ManualSave_IncludeMSB;
                }
                UIHelper.Tooltip("If enabled, the map files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeMSB);

                if (ImGui.MenuItem($"BTL"))
                {
                    CFG.Current.MapEditor_ManualSave_IncludeBTL = !CFG.Current.MapEditor_ManualSave_IncludeBTL;
                }
                UIHelper.Tooltip("If enabled, the light files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTL);

                if (AutoInvadeBank.CanUse())
                {
                    if (ImGui.MenuItem($"AIP"))
                    {
                        CFG.Current.MapEditor_ManualSave_IncludeAIP = !CFG.Current.MapEditor_ManualSave_IncludeAIP;
                    }
                    UIHelper.Tooltip("If enabled, the auto invade point files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeAIP);
                }

                if (HavokNavmeshBank.CanUse())
                {
                    if (ImGui.MenuItem($"NVA"))
                    {
                        CFG.Current.MapEditor_ManualSave_IncludeNVA = !CFG.Current.MapEditor_ManualSave_IncludeNVA;
                    }
                    UIHelper.Tooltip("If enabled, the navmesh configuration files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeNVA);
                }

                if (LightAtlasBank.CanUse())
                {
                    if (ImGui.MenuItem($"BTAB"))
                    {
                        CFG.Current.MapEditor_ManualSave_IncludeBTAB = !CFG.Current.MapEditor_ManualSave_IncludeBTAB;
                    }
                    UIHelper.Tooltip("If enabled, the light atlas files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTAB);
                }

                if (LightProbeBank.CanUse())
                {
                    if (ImGui.MenuItem($"BTPB"))
                    {
                        CFG.Current.MapEditor_ManualSave_IncludeBTPB = !CFG.Current.MapEditor_ManualSave_IncludeBTPB;
                    }
                    UIHelper.Tooltip("If enabled, the light probe files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTPB);
                }

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"MSB"))
                {
                    CFG.Current.MapEditor_AutomaticSave_IncludeMSB = !CFG.Current.MapEditor_AutomaticSave_IncludeMSB;
                }
                UIHelper.Tooltip("If enabled, the map files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeMSB);

                if (ImGui.MenuItem($"BTL"))
                {
                    CFG.Current.MapEditor_AutomaticSave_IncludeBTL = !CFG.Current.MapEditor_AutomaticSave_IncludeBTL;
                }
                UIHelper.Tooltip("If enabled, the light files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTL);

                if (AutoInvadeBank.CanUse())
                {
                    if (ImGui.MenuItem($"AIP"))
                    {
                        CFG.Current.MapEditor_AutomaticSave_IncludeAIP = !CFG.Current.MapEditor_AutomaticSave_IncludeAIP;
                    }
                    UIHelper.Tooltip("If enabled, the auto invade point files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeAIP);
                }

                if (HavokNavmeshBank.CanUse())
                {
                    if (ImGui.MenuItem($"NVA"))
                    {
                        CFG.Current.MapEditor_AutomaticSave_IncludeNVA = !CFG.Current.MapEditor_AutomaticSave_IncludeNVA;
                    }
                    UIHelper.Tooltip("If enabled, the navmesh configuration files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeNVA);
                }

                if (LightAtlasBank.CanUse())
                {
                    if (ImGui.MenuItem($"BTAB"))
                    {
                        CFG.Current.MapEditor_AutomaticSave_IncludeBTAB = !CFG.Current.MapEditor_AutomaticSave_IncludeBTAB;
                    }
                    UIHelper.Tooltip("If enabled, the light atlas files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTAB);
                }

                if (LightProbeBank.CanUse())
                {
                    if (ImGui.MenuItem($"BTPB"))
                    {
                        CFG.Current.MapEditor_AutomaticSave_IncludeBTPB = !CFG.Current.MapEditor_AutomaticSave_IncludeBTPB;
                    }
                    UIHelper.Tooltip("If enabled, the light probe files are outputted on save.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTPB);
                }

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the automatic saving process.");

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
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
            if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            DuplicateAction.OnMenu();
            DeleteAction.OnMenu();
            RotateAction.OnMenu();
            ScrambleAction.OnMenu();
            ReplicateAction.OnMenu();
            RenderTypeAction.OnMenu();

            ImGui.Separator();

            CreateAction.OnMenu();
            DuplicateToMapAction.OnMenu();
            MoveToMapAction.OnMenu();

            ImGui.Separator();

            GotoAction.OnMenu();
            FrameAction.OnMenu();   
            PullToCameraAction.OnMenu();

            ImGui.Separator();

            ReorderAction.OnMenu();

            ImGui.Separator();

            EditorVisibilityAction.OnMenu();
            GameVisibilityAction.OnMenu();

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        // Dropdown: View
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Viewport"))
            {
                CFG.Current.Viewport_Display = !CFG.Current.Viewport_Display;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Viewport_Display);

            if (ImGui.MenuItem("Map List"))
            {
                CFG.Current.Interface_MapEditor_MapList = !CFG.Current.Interface_MapEditor_MapList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_MapList);

            if (ImGui.MenuItem("Map Contents"))
            {
                CFG.Current.Interface_MapEditor_MapContents = !CFG.Current.Interface_MapEditor_MapContents;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_MapContents);

            if (ImGui.MenuItem("Properties"))
            {
                CFG.Current.Interface_MapEditor_Properties = !CFG.Current.Interface_MapEditor_Properties;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Properties);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_MapEditor_ToolWindow = !CFG.Current.Interface_MapEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_ToolWindow);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_MapEditor_ResourceList = !CFG.Current.Interface_MapEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_ResourceList);

            ImGui.Separator();

            // Quick toggles for some of the Field Editor field visibility options

            if (ImGui.MenuItem("Field: Community Names"))
            {
                CFG.Current.MapEditor_Properties_Enable_Commmunity_Names = !CFG.Current.MapEditor_Properties_Enable_Commmunity_Names;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_Properties_Enable_Commmunity_Names);

            if (ImGui.MenuItem("Field: Unknowns"))
            {
                CFG.Current.MapEditor_Properties_Display_Unknown_Properties = !CFG.Current.MapEditor_Properties_Display_Unknown_Properties;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_Properties_Display_Unknown_Properties);

            ImGui.EndMenu();
        }
    }

    public void ToolMenu()
    {
        var validViewportState = MapViewportView.RenderScene != null &&
            MapViewportView.Viewport != null;

        // Tools
        ToolSubMenu.DisplayMenu();
    }

    public void FilterMenu()
    {
        var validViewportState = MapViewportView.RenderScene != null &&
            MapViewportView.Viewport != null;

        // General Filters
        if (ImGui.BeginMenu("General Filters", validViewportState))
        {
            BasicFilters.Display();

            ImGui.Separator();

            if (ImGui.BeginMenu("Filter Presets"))
            {
                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_1.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_1.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_2.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_2.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_3.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_3.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_4.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_4.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_5.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_5.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_6.Name))
                {
                    MapViewportView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_6.Filters;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        // Region Filters
        if (ImGui.BeginMenu("Region Filters", validViewportState))
        {
            RegionFilters.DisplayOptions();

            ImGui.EndMenu();
        }
    }

    public void CollisionMenu()
    {
        var validViewportState = MapViewportView.RenderScene != null &&
            MapViewportView.Viewport != null;

        if (ImGui.BeginMenu("Collision Type", validViewportState))
        {
            if (ImGui.MenuItem("Low"))
            {
                HavokCollisionBank.VisibleCollisionType = HavokCollisionType.Low;
                CFG.Current.CurrentHavokCollisionType = HavokCollisionType.Low;

                HavokCollisionBank.RefreshCollision();
            }
            UIHelper.Tooltip("Visible collision will use the low-detail mesh.\nUsed for standard collision.");
            UIHelper.ShowActiveStatus(HavokCollisionBank.VisibleCollisionType == HavokCollisionType.Low);

            if (ImGui.MenuItem("High"))
            {
                HavokCollisionBank.VisibleCollisionType = HavokCollisionType.High;
                CFG.Current.CurrentHavokCollisionType = HavokCollisionType.High;

                HavokCollisionBank.RefreshCollision();
            }
            UIHelper.Tooltip("Visible collision will use the high-detail mesh.\nUsed for IK.");
            UIHelper.ShowActiveStatus(HavokCollisionBank.VisibleCollisionType == HavokCollisionType.High);

            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                if (ImGui.MenuItem("Fall Protection"))
                {
                    HavokCollisionBank.VisibleCollisionType = HavokCollisionType.FallProtection;
                    CFG.Current.CurrentHavokCollisionType = HavokCollisionType.FallProtection;

                    HavokCollisionBank.RefreshCollision();
                }
                UIHelper.Tooltip("Visible collision will use the fall-protection mesh.\nUsed for enemy fall protection.");
                UIHelper.ShowActiveStatus(HavokCollisionBank.VisibleCollisionType == HavokCollisionType.FallProtection);
            }

            ImGui.EndMenu();
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.IsInitializing)
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

    public void Save(bool autoSave = false)
    {
        if (Project.Descriptor.ProjectType == ProjectType.Undefined)
            return;

        try
        {
            Universe.SaveAllMaps(autoSave);
        }
        catch (SavingFailedException e)
        {
            HandleSaveException(e);
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }


    public void ReloadUniverse()
    {
        Universe.UnloadAllMaps();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        CreateAction.PopulateClassNames();
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
                foreach (var entry in Project.Locator.MapFiles.Entries)
                {
                    var currentContainer = Selection.GetMapContainerFromMapID(entry.Filename);

                    if (currentContainer != null)
                    {
                        foreach (Entity obj in currentContainer.Objects)
                        {
                            if (obj.WrappedObject == eRef.Referrer)
                            {
                                ViewportSelection.ClearSelection(this);
                                ViewportSelection.AddSelection(this, obj);
                                FrameAction.ApplyViewportFrame();
                                return;
                            }
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
