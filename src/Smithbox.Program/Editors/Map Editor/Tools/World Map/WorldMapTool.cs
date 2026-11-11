using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Map Information window: the world map with the map tiles over it (World Map mode), and the editor for the
/// ER mapinfotexbnd layers (Map Information Editor mode). See Claude/Documentation/World Map Assessment.md.
/// </summary>
public class WorldMapTool : IResourceEventListener
{
    public enum WorldMapMode
    {
        WorldMap,
        MapInformation
    }

    private MapEditorView View;
    private ProjectEntry Project;

    private WorldMapLayout VanillaLayout = null;
    private WorldMapLayout SoteLayout = null;
    private WorldMapLayout LimveldLayout = null;

    private bool LayoutsBuilt = false;


    public WorldMapImageSource CurrentMapSource = WorldMapImageSource.LandsBetween;

    public WorldMapMode Mode = WorldMapMode.WorldMap;

    public MapInfoEditor MapInfo { get; }

    /// <summary>The game's map tiles (ER); null for games still using the bundled image</summary>
    private readonly WorldMapTileSet GameTiles;

    /// <summary>The game's map of a map source: M00 Lands Between, M10 Shadow of the Erdtree</summary>
    private static string GameMapOf(WorldMapImageSource source) => source switch
    {
        WorldMapImageSource.LandsBetween => "M00",
        WorldMapImageSource.ShadowOfTheErdtree => "M10",
        _ => null
    };

    private Vector2 MapZoomFactor;

    private Vector2 MapTextureSize = new Vector2();

    /// <summary>The screen position of the map image's top left this frame</summary>
    private Vector2 ImageMin = new Vector2();

    /// <summary>The cursor on the map image, before zoom</summary>
    private Vector2 MouseImage = new Vector2();

    private bool CanvasHovered = false;

    private List<string> HoveredMapTiles = new List<string>();
    private List<string> SelectedMapTiles = new List<string>();

    private bool _isDraggingMap = false;
    private Vector2 _lastMousePos = Vector2.Zero;

    /// <summary>The screen offset of the map image's top left from the canvas's top left</summary>
    private Vector2 MapPan;

    /// <summary>Centre the map on the next draw (first show, reset, map source change)</summary>
    private bool _centerPending = true;

    public static List<string> TextureExlusions = new List<string>()
    {
        "world_map_vanilla",
        "world_map_sote",
        "world_map_limveld",
        "world_map_limveld_mountaintops",
        "world_map_limveld_crater",
        "world_map_limveld_rotted_woods",
        "world_map_limveld_noklateo",
        "world_map_forsaken_hollows"
    };

    public WorldMapTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;


        MapInfo = new MapInfoEditor(view, project);

        if (WorldMapTileSet.IsSupported(project))
            GameTiles = new WorldMapTileSet(project);

        MapZoomFactor = GetDefaultZoomLevel();
        DPI.UIScaleChanged += (_, _) =>
        {
            ResetView();
        };

        // ER draws the game's own map tiles (GameTiles), so it needs no bundled image
        if (View.Project.Descriptor.ProjectType is ProjectType.ER)
        {
            CurrentMapSource = WorldMapImageSource.LandsBetween;
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.NR)
        {
            CurrentMapSource = WorldMapImageSource.Limveld;

            RegisterWorldMapListeners();
        }
    }

    public static bool IsAvailable(ProjectEntry project) => project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR;

    /// <summary>
    /// Shows or hides the window. Its visibility is a Map Editor window setting, like the Tools window's.
    /// </summary>
    public void ToggleWindow()
    {
        CFG.Current.Interface_MapEditor_MapInformation = !CFG.Current.Interface_MapEditor_MapInformation;
    }

    private void BuildLayouts()
    {
        if (LayoutsBuilt)
            return;

        LayoutsBuilt = true;

        GenerateWorldMapLayout_Vanilla();
        GenerateWorldMapLayout_SOTE();

        GenerateWorldMapLayout_Limveld(
            smallTile: 256,
            mediumTile: 512,
            largeTile: 1024,
            xLargeOffset: -256,
            yLargeOffset: 256,
            xMediumOffset: 256,
            yMediumOffset: 256,
            xSmallOffset: 256,
            ySmallOffset: 256);
    }

    /// <summary>
    /// The toggle shortcut, checked every frame whether or not the window is shown
    /// </summary>
    public void Update()
    {
        if (!IsAvailable(Project))
            return;

        if (FocusManager.IsFocus(EditorFocusContext.MapEditor_Viewport)
            || FocusManager.IsFocus(EditorFocusContext.MapEditor_WorldMap))
        {
            if (!ImGui.IsAnyItemActive() && InputManager.IsPressed(KeybindID.MapEditor_Toggle_World_Map_Menu))
            {
                ToggleWindow();
            }
        }
    }

    #region Menus

    public void ModeMenu()
    {
        if (ImGui.MenuItem($"{LOC.Get("MAP_MapInfo_Mode_World_Map")}##worldMapModeWorldMap", "", Mode == WorldMapMode.WorldMap))
        {
            Mode = WorldMapMode.WorldMap;
        }
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Mode_World_Map_TT"));

        if (MapInfoEditor.IsSupported(Project))
        {
            if (ImGui.MenuItem($"{LOC.Get("MAP_MapInfo_Mode_Editor")}##worldMapModeEditor", "", Mode == WorldMapMode.MapInformation))
            {
                Mode = WorldMapMode.MapInformation;
            }
            GUI.Tooltip(LOC.Get("MAP_MapInfo_Mode_Editor_TT"));
        }
    }

    public void ControlsMenu()
    {
        if (ImGui.MenuItem($"{LOC.Get("MAP_MapInfo_Controls")}##worldMapControls"))
        {
            ImGui.OpenPopup("controlHint");
        }

        if (ImGui.BeginPopup("controlHint"))
        {
            if (Mode == WorldMapMode.WorldMap)
            {
                ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Drag", InputManager.GetMouseHint(MousebindID.MapEditor_World_Map_Drag_Start)));
                ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Select", InputManager.GetMouseHint(MousebindID.MapEditor_Select_Map_In_World_Map)));
            }
            else
            {
                ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Paint", InputManager.GetMouseHint(MousebindID.MapEditor_Map_Information_Paint)));
                ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Pick", InputManager.GetMouseHint(MousebindID.MapEditor_Map_Information_Pick)));
                ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Drag", InputManager.GetMouseHint(MousebindID.MapEditor_Map_Information_Pan)));
                ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Undo",
                    InputManager.GetHint(KeybindID.Undo), InputManager.GetHint(KeybindID.Redo), InputManager.GetHint(KeybindID.Save)));
            }

            ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Zoom"));
            ImGui.Text(LOC.Get("MAP_MapInfo_Controls_Reset_Zoom", InputManager.GetHint(KeybindID.MapEditor_Reset_World_Map_Zoom_Level)));

            ImGui.EndPopup();
        }
    }

    public void MapSourceMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("MAP_MapInfo_Map_Source")}##worldMapSource"))
        {
            if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                SourceItem(WorldMapImageSource.LandsBetween, "MAP_MapInfo_Source_Lands_Between");
                SourceItem(WorldMapImageSource.ShadowOfTheErdtree, "MAP_MapInfo_Source_SotE");
            }

            if (Project.Descriptor.ProjectType is ProjectType.NR)
            {
                SourceItem(WorldMapImageSource.Limveld, "MAP_MapInfo_Source_Limveld");
                SourceItem(WorldMapImageSource.Limveld_Mountaintops, "MAP_MapInfo_Source_Limveld_Mountaintops");
                SourceItem(WorldMapImageSource.Limveld_Crater, "MAP_MapInfo_Source_Limveld_Crater");
                SourceItem(WorldMapImageSource.Limveld_Rotted_Woods, "MAP_MapInfo_Source_Limveld_Rotted_Woods");
                SourceItem(WorldMapImageSource.Limveld_Noklateo, "MAP_MapInfo_Source_Limveld_Noklateo");
                SourceItem(WorldMapImageSource.Forsaken_Hollows, "MAP_MapInfo_Source_Forsaken_Hollows");
            }

            ImGui.EndMenu();
        }
    }

    private void SourceItem(WorldMapImageSource source, string key)
    {
        if (ImGui.MenuItem($"{LOC.Get(key)}##worldMapSource_{source}", "", CurrentMapSource == source))
        {
            CurrentMapSource = source;
            _centerPending = true;
        }
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Source_TT"));
    }

    /// <summary>
    /// The World Map mode's side panel: the maps picked on the map, with buttons to load them, and which tile
    /// shapes are drawn
    /// </summary>
    private void DisplayWorldMapPanel()
    {
        var pickHint = InputManager.GetMouseHint(MousebindID.MapEditor_Select_Map_In_World_Map);

        GUI.WrappedText(LOC.Get("MAP_MapInfo_WorldMap_Description", pickHint));

        // Selected Tiles
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Selected_Tiles_Header"), LOC.Get("MAP_MapInfo_Selected_Tiles_Header_TT", pickHint));

        if (SelectedMapTiles.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MapInfo_Selected_Tiles_None", pickHint));
        }

        foreach (var tile in SelectedMapTiles)
        {
            GUI.WrappedText($"{tile} ({AliasHelper.GetMapNameAlias(View.Project, tile)})");

            var loaded = IsMapLoaded(tile);
            var mapId = tile;

            GUI.ConditionalMultiButtonInput($"worldMapLoadTable_{tile}",
                $"worldMapLoad_{tile}",
                LOC.Get(loaded ? "MAP_MapInfo_Selected_Tiles_Loaded" : "MAP_MapInfo_Selected_Tiles_Load"),
                LOC.Get("MAP_MapInfo_Selected_Tiles_Load_TT", tile),
                () => View.Universe.LoadMap(mapId, true), !loaded);
        }

        // Tile Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Tile_Display_Header"), LOC.Get("MAP_MapInfo_Tile_Display_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MapInfo_Display_Tiles")}##worldMapTiles", ref CFG.Current.WorldMapDisplayTiles);
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Display_Tiles_TT"));

        if (CFG.Current.WorldMapDisplayTiles)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_MapInfo_Display_Small")}##worldMapTilesSmall", ref CFG.Current.WorldMapDisplaySmallTiles);
            GUI.Tooltip(LOC.Get("MAP_MapInfo_Display_Small_TT"));

            ImGui.Checkbox($"{LOC.Get("MAP_MapInfo_Display_Medium")}##worldMapTilesMedium", ref CFG.Current.WorldMapDisplayMediumTiles);
            GUI.Tooltip(LOC.Get("MAP_MapInfo_Display_Medium_TT"));

            ImGui.Checkbox($"{LOC.Get("MAP_MapInfo_Display_Large")}##worldMapTilesLarge", ref CFG.Current.WorldMapDisplayLargeTiles);
            GUI.Tooltip(LOC.Get("MAP_MapInfo_Display_Large_TT"));
        }

        if (Project.Descriptor.ProjectType is ProjectType.NR)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_MapInfo_Filter_Restrict")}##worldMapFilterRestrict", ref CFG.Current.WorldMapFilter_RestrictBySource);
            GUI.Tooltip(LOC.Get("MAP_MapInfo_Filter_Restrict_TT"));
        }
    }

    #endregion

    /// <summary>
    /// The window, as one of the view's Map Editor windows: in the view's dockspace, so it can be docked beside
    /// the viewport. It first docks into the viewport's node, as a tab next to it.
    /// </summary>
    public void Display(uint dockspaceId, int viewIndex, uint viewportDockId)
    {
        if (!IsAvailable(Project) || !CFG.Current.Interface_MapEditor_MapInformation)
            return;

        BuildLayouts();

        ImGui.SetNextWindowDockID(viewportDockId != 0 ? viewportDockId : dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);

        if (ImGui.Begin($"{LOC.Get("MAP_MapInfo_Window_Title")}##mapEditor_MapInformation_{viewIndex}", GUI.GetMainWindowFlags()))
        {
            DisplayContents();
        }
        else
        {
            MapInfo.CancelPainting();
        }

        ImGui.End();
    }

    private void DisplayContents()
    {
        var editorMode = Mode == WorldMapMode.MapInformation && MapInfoEditor.IsSupported(Project);
        var area = MapInfoEditor.AreaOf(CurrentMapSource);

        if (ImGui.BeginMenuBar())
        {
            ModeMenu();
            ImGui.Separator();
            ControlsMenu();
            MapSourceMenu();

            ImGui.EndMenuBar();
        }

        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows))
        {
            FocusManager.SetFocus(EditorFocusContext.MapEditor_WorldMap);
            View.Editor.ViewHandler.ActiveView = View;
        }

        // Side panel: the mode's options
        if (editorMode)
        {
            ImGui.BeginChild("##mapInfoPanel", new Vector2(320 * DPI.UIScale(), 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
            MapInfo.DisplayPanel(area);
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("##worldMapPanel", new Vector2(260 * DPI.UIScale(), 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
            DisplayWorldMapPanel();
            ImGui.EndChild();
        }

        ImGui.SameLine();

        DisplayCanvas(editorMode, area);

        var focused = FocusManager.IsFocus(EditorFocusContext.MapEditor_WorldMap)
            && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);

        if (focused)
        {
            if (editorMode)
                MapInfo.HandleShortcuts();

            if (!ImGui.IsAnyItemActive() && InputManager.IsPressed(KeybindID.MapEditor_Reset_World_Map_Zoom_Level))
            {
                ResetView();
            }
        }
    }

    private void DisplayCanvas(bool editorMode, string area)
    {
        // The canvas has no scrolling of its own: the map is placed at MapPan, and panning and zooming change
        // MapPan and the zoom before anything is drawn, so a change shows in the same frame. (With ImGui scrolling
        // a zoom drew one frame at the new zoom with the old scroll, and the scroll was clamped to the old size.)
        ImGui.BeginChild("##worldMapCanvas", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

        var canvasMin = ImGui.GetCursorScreenPos();
        var canvasSize = Vector2.Max(ImGui.GetContentRegionAvail(), new Vector2(1));
        var canvasMax = canvasMin + canvasSize;

        // Takes the mouse over the whole canvas, so dragging pans the map instead of moving the window
        ImGui.InvisibleButton("##worldMapCanvasInput", canvasSize,
            ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight | ImGuiButtonFlags.MouseButtonMiddle);
        var canvasHovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);

        var drawList = ImGui.GetWindowDrawList();
        drawList.PushClipRect(canvasMin, canvasMax, true);

        var gameMap = GameMapOf(CurrentMapSource);
        var useGameTiles = GameTiles != null && gameMap != null;
        TextureResource texRes = null;

        // Map size in image units: L0 map pixels for the game's tiles, texels for a bundled image
        Vector2 imageSize;

        if (useGameTiles)
        {
            GameTiles.EnsureLoading();
            imageSize = new Vector2(WorldMapTileSet.ImageSize);
        }
        else
        {
            texRes = GetImageTextureHandle(GetImagePath())?.Get();
            imageSize = texRes != null ? GetImageSize(texRes, false) : Vector2.Zero;
        }

        // Pan and zoom first
        if (_centerPending && imageSize.X > 0)
        {
            MapPan = (canvasSize - imageSize * MapZoomFactor) / 2;
            _centerPending = false;
        }

        HandleDrag(editorMode, canvasHovered);

        if (canvasHovered)
        {
            HandleZoom(canvasMin);
        }

        ClampPan(canvasSize, imageSize);

        ImageMin = canvasMin + MapPan;
        MapTextureSize = imageSize * MapZoomFactor;
        MouseImage = (ImGui.GetMousePos() - ImageMin) / MapZoomFactor;

        var mouse = ImGui.GetMousePos();
        CanvasHovered = canvasHovered
            && mouse.X >= ImageMin.X && mouse.Y >= ImageMin.Y
            && mouse.X < ImageMin.X + MapTextureSize.X && mouse.Y < ImageMin.Y + MapTextureSize.Y;

        // Map
        if (useGameTiles)
        {
            var visibleMin = Vector2.Max(Vector2.Zero, (canvasMin - ImageMin) / MapZoomFactor);
            var visibleMax = (canvasMax - ImageMin) / MapZoomFactor;

            GameTiles.Draw(drawList, gameMap, ImageMin, MapZoomFactor.X, visibleMin, visibleMax);

            if (!GameTiles.Loaded)
            {
                var status = GameTiles.LoadError != null
                    ? LOC.Get("MAP_WorldMapTiles_Load_Error", GameTiles.LoadError)
                    : LOC.Get("MAP_WorldMapTiles_Loading");
                drawList.AddText(canvasMin + new Vector2(12, 12) * DPI.UIScale(), ImGui.GetColorU32(ImGuiCol.Text), status);
            }
        }
        else if (texRes != null)
        {
            drawList.AddImage(new ImTextureID(texRes.GPUTexture.TexHandle), ImageMin, ImageMin + MapTextureSize);
        }

        if (editorMode)
        {
            MapInfo.DisplayCanvas(drawList, ImageMin, MapZoomFactor, GetLayout(area), area, CanvasHovered && !_isDraggingMap, MouseImage);
        }
        else
        {
            DrawTileOverlay(drawList);
        }

        drawList.PopClipRect();

        ImGui.EndChild();

        // Hover and tooltips, only while the image itself is under the cursor
        if (editorMode)
        {
            HoveredMapTiles = new List<string>();

            if (CanvasHovered && !MapInfo.IsPainting)
                MapInfo.DisplayTooltip(area);
        }
        else
        {
            HoveredMapTiles = CanvasHovered ? GetMatchingMaps(MouseImage) : new List<string>();

            if (HoveredMapTiles.Count > 0)
            {
                ImGui.BeginTooltip();

                foreach (var tile in HoveredMapTiles)
                {
                    ImGui.Text($"{tile} ({AliasHelper.GetMapNameAlias(View.Project, tile)})");
                }

                ImGui.EndTooltip();
            }

            // Select Maps under Point
            if (CanvasHovered && HoveredMapTiles.Count > 0 && InputManager.IsMousePressed(MousebindID.MapEditor_Select_Map_In_World_Map))
            {
                SelectTilesUnderCursor();
            }
        }
    }

    private void DrawTileOverlay(ImDrawListPtr drawList)
    {
        if (!CFG.Current.WorldMapDisplayTiles)
            return;

        foreach (var tile in GetTileList())
        {
            if (tile.TileType is WorldMapTileType.Small && !CFG.Current.WorldMapDisplaySmallTiles)
                continue;

            if (tile.TileType is WorldMapTileType.Medium && !CFG.Current.WorldMapDisplayMediumTiles)
                continue;

            if (tile.TileType is WorldMapTileType.Large && !CFG.Current.WorldMapDisplayLargeTiles)
                continue;

            Vector2 drawStart = ImageMin + new Vector2(tile.X, tile.Y) * MapZoomFactor;
            Vector2 drawEnd = ImageMin + new Vector2(tile.X + tile.Width, tile.Y + tile.Height) * MapZoomFactor;

            // Highlight hovered or selected
            uint color = ImGui.GetColorU32(new Vector4(1, 1, 0, 0.5f)); // Yellow default
            if (HoveredMapTiles.Contains(tile.Name))
                color = ImGui.GetColorU32(new Vector4(0, 1, 0, 0.5f)); // Green for hover
            else if (SelectedMapTiles.Contains(tile.Name))
                color = ImGui.GetColorU32(new Vector4(1, 0, 0, 0.5f)); // Red for selected

            drawList.AddRectFilled(drawStart, drawEnd, ImGui.GetColorU32(new Vector4(1, 1, 0, 0.05f)));
            drawList.AddRect(drawStart, drawEnd, color, 0, ImDrawFlags.None, 2.0f);
        }
    }

    /// <summary>
    /// Drags the view. It starts only on the image, and ends as soon as the button is no longer held,
    /// wherever the cursor is.
    /// </summary>
    private void HandleDrag(bool editorMode, bool canvasHovered)
    {
        var startBind = editorMode ? MousebindID.MapEditor_Map_Information_Pan : MousebindID.MapEditor_World_Map_Drag_Start;
        var currentMousePos = ImGui.GetMousePos();

        if (!_isDraggingMap && canvasHovered && !MapInfo.IsPainting && InputManager.IsMousePressed(startBind))
        {
            _isDraggingMap = true;
            _lastMousePos = currentMousePos;
        }

        if (!_isDraggingMap)
            return;

        var released = !InputManager.IsMouseDown(startBind)
            || (!editorMode && InputManager.IsMouseReleased(MousebindID.MapEditor_World_Map_Drag_End));

        if (released)
        {
            _isDraggingMap = false;
            return;
        }

        // The map follows the cursor
        MapPan += currentMousePos - _lastMousePos;

        _lastMousePos = currentMousePos;
    }

    /// <summary>
    /// Keeps at least a margin of the map inside the canvas, so it can't be panned out of sight
    /// </summary>
    private void ClampPan(Vector2 canvasSize, Vector2 imageSize)
    {
        if (imageSize.X <= 0)
            return;

        var margin = 64 * DPI.UIScale();
        var size = imageSize * MapZoomFactor;

        MapPan = Vector2.Clamp(MapPan,
            new Vector2(margin, margin) - size,
            canvasSize - new Vector2(margin, margin));
    }

    private string GetImagePath()
    {
        return CurrentMapSource switch
        {
            WorldMapImageSource.LandsBetween => "smithbox/world_map/world_map_vanilla",
            WorldMapImageSource.ShadowOfTheErdtree => "smithbox/world_map/world_map_sote",
            WorldMapImageSource.Limveld => "smithbox/world_map/world_map_limveld",
            WorldMapImageSource.Limveld_Mountaintops => "smithbox/world_map/world_map_limveld_mountaintops",
            WorldMapImageSource.Limveld_Crater => "smithbox/world_map/world_map_limveld_crater",
            WorldMapImageSource.Limveld_Rotted_Woods => "smithbox/world_map/world_map_limveld_rotted_woods",
            WorldMapImageSource.Limveld_Noklateo => "smithbox/world_map/world_map_limveld_noklateo",
            WorldMapImageSource.Forsaken_Hollows => "smithbox/world_map/world_map_forsaken_hollows",
            _ => ""
        };
    }

    /// <summary>The layout of the map information area (m60 or m61)</summary>
    private WorldMapLayout GetLayout(string area)
    {
        return area switch
        {
            "m60" => VanillaLayout,
            "m61" => SoteLayout,
            _ => null
        };
    }

    private List<WorldMapTile> GetTileList()
    {
        var layout = CurrentMapSource switch
        {
            WorldMapImageSource.LandsBetween => VanillaLayout,
            WorldMapImageSource.ShadowOfTheErdtree => SoteLayout,
            _ => LimveldLayout
        };

        return layout?.Tiles ?? new List<WorldMapTile>();
    }

    /// <summary>
    /// With "Restrict by Map Source" on (NR), whether a tile belongs to the shown map: its last two digits
    /// </summary>
    private bool IsInCurrentSource(WorldMapTile tile)
    {
        if (!CFG.Current.WorldMapFilter_RestrictBySource)
            return true;

        var variant = CurrentMapSource switch
        {
            WorldMapImageSource.Limveld => 0,
            WorldMapImageSource.Limveld_Mountaintops => 1,
            WorldMapImageSource.Limveld_Crater => 2,
            WorldMapImageSource.Limveld_Rotted_Woods => 3,
            WorldMapImageSource.Forsaken_Hollows => 4,
            WorldMapImageSource.Limveld_Noklateo => 5,
            _ => -1
        };

        if (variant < 0)
            return true;

        return tile.Name.EndsWith($"{variant}0") || tile.Name.EndsWith($"{variant}1");
    }

    /// <summary>
    /// Selects the maps under the cursor (small, medium and large tile), for the Selected Tiles section to load
    /// </summary>
    private void SelectTilesUnderCursor()
    {
        SelectedMapTiles = GetTileList()
            .Where(tile => HoveredMapTiles.Contains(tile.Name) && IsInCurrentSource(tile))
            .OrderBy(tile => tile.TileType)
            .Select(tile => tile.Name)
            .Distinct()
            .ToList();
    }

    private bool IsMapLoaded(string mapId)
    {
        return View.Selection.GetMapContainerFromMapID(mapId) is { LoadState: MapContentLoadState.Loaded };
    }

    private void RegisterWorldMapListeners()
    {
        if (Project.Descriptor.ProjectType is ProjectType.ER)
        {
            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_vanilla", this, AccessLevel.AccessGPUOptimizedOnly);

            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_sote", this, AccessLevel.AccessGPUOptimizedOnly);
        }

        if (Project.Descriptor.ProjectType is ProjectType.NR)
        {
            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_limveld", this, AccessLevel.AccessGPUOptimizedOnly);

            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_limveld_mountaintops", this, AccessLevel.AccessGPUOptimizedOnly);

            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_limveld_crater", this, AccessLevel.AccessGPUOptimizedOnly);

            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_limveld_rotted_woods", this, AccessLevel.AccessGPUOptimizedOnly);

            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_limveld_noklateo", this, AccessLevel.AccessGPUOptimizedOnly);

            ResourceManager.DefaultPool.AddListener<TextureResource>("smithbox/world_map/world_map_forsaken_hollows", this, AccessLevel.AccessGPUOptimizedOnly);
        }

        View.Universe.ScheduleWorldMapRefresh();
    }

    private void GenerateWorldMapLayout_Vanilla()
    {
        var smallRows = new List<int>() { 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 };
        var smallCols = new List<int>() { 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30 };

        var mediumRows = new List<int>() { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 };
        var mediumCols = new List<int>() { 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15 };

        var largeRows = new List<int>() { 8, 9, 10, 11, 12, 13, 14 };
        var largeCols = new List<int>() { 15, 14, 13, 12, 11, 10, 9, 8, 7 };

        VanillaLayout = new WorldMapLayout(View, "60", 480, 55);

        // On the game's tiles, the tiles sit exactly where the game's map puts them
        if (GameTiles != null)
        {
            VanillaLayout.GenerateExactTiles(smallRows, smallCols, WorldMapTileType.Small);
            VanillaLayout.GenerateExactTiles(mediumRows, mediumCols, WorldMapTileType.Medium);
            VanillaLayout.GenerateExactTiles(largeRows, largeCols, WorldMapTileType.Large);
            return;
        }

        VanillaLayout.GenerateTiles(smallRows, smallCols, 0, 124, WorldMapTileType.Small);
        VanillaLayout.GenerateTiles(mediumRows, mediumCols, 1, 248, WorldMapTileType.Medium);
        VanillaLayout.GenerateTiles(largeRows, largeCols, 2, 496, WorldMapTileType.Large);
    }

    private int SOTE_xOffset = 540;
    private int SOTE_yOffset = -1075;

    private void GenerateWorldMapLayout_SOTE()
    {
        var smallRows = new List<int>() { 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57 };
        var smallCols = new List<int>() { 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30 };
        var mediumRows = new List<int>() { 21, 22, 23, 24, 25, 26, 27, 28, 29 };
        var mediumCols = new List<int>() { 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17 };
        var largeRows = new List<int>() { 10, 11, 12, 13, 14 };
        var largeCols = new List<int>() { 13, 12, 11, 10, 9, 8 };

        SoteLayout = new WorldMapLayout(View, "61", SOTE_xOffset, SOTE_yOffset);

        if (GameTiles != null)
        {
            SoteLayout.GenerateExactTiles(smallRows, smallCols, WorldMapTileType.Small);
            SoteLayout.GenerateExactTiles(mediumRows, mediumCols, WorldMapTileType.Medium);
            SoteLayout.GenerateExactTiles(largeRows, largeCols, WorldMapTileType.Large);
            return;
        }

        SoteLayout.GenerateTiles(smallRows, smallCols, 0, 256, WorldMapTileType.Small);
        SoteLayout.GenerateTiles(mediumRows, mediumCols, 1, 528, WorldMapTileType.Medium);
        SoteLayout.GenerateTiles(largeRows, largeCols, 2, 1056, WorldMapTileType.Large);
    }

    public void GenerateWorldMapLayout_Limveld(
        int smallTile = 256, int mediumTile = 512, int largeTile = 1024,
        int xLargeOffset = 0, int yLargeOffset = 0,
        int xMediumOffset = 0, int yMediumOffset = 0,
        int xSmallOffset = 0, int ySmallOffset = 0)
    {
        var smallRows = new List<int>() { 42, 43, 44, 45 };
        var smallCols = new List<int>() { 39, 38, 37, 36 };

        var mediumRows = new List<int>() { 21, 22 };
        var mediumCols = new List<int>() { 19, 18 };

        var largeRows = new List<int>() { 10, 11 };
        var largeCols = new List<int>() { 9 };

        var variantTileIds = new List<int>() { 0, 10, 20, 30, 40, 50 };

        LimveldLayout = new WorldMapLayout(View, "60", 0, 0);

        LimveldLayout.GenerateTiles(smallRows, smallCols, 0, smallTile, WorldMapTileType.Small, variantTileIds,
            xLargeOffset, yLargeOffset,
            xMediumOffset, yMediumOffset,
            xSmallOffset, ySmallOffset);

        LimveldLayout.GenerateTiles(mediumRows, mediumCols, 1, mediumTile, WorldMapTileType.Medium, variantTileIds,
            xLargeOffset, yLargeOffset,
            xMediumOffset, yMediumOffset,
            xSmallOffset, ySmallOffset);

        LimveldLayout.GenerateTiles(largeRows, largeCols, 2, largeTile, WorldMapTileType.Large, variantTileIds,
            xLargeOffset, yLargeOffset,
            xMediumOffset, yMediumOffset,
            xSmallOffset, ySmallOffset);
    }

    private List<string> GetMatchingMaps(Vector2 pos)
    {
        List<string> matches = new List<string>();

        foreach (var tile in GetTileList())
        {
            if (!IsInCurrentSource(tile))
                continue;

            if (pos.X > tile.X && pos.X < tile.X + tile.Width && pos.Y > tile.Y && pos.Y < tile.Y + tile.Height
                && !matches.Contains(tile.Name))
            {
                matches.Add(tile.Name);
            }
        }

        return matches;
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
    }

    private Vector2 GetImageSize(TextureResource texRes, bool includeZoomFactor)
    {
        Vector2 size = new Vector2(0, 0);

        if (texRes.GPUTexture != null)
        {
            var Width = texRes.GPUTexture.Width;
            var Height = texRes.GPUTexture.Height;

            if (Height != 0 && Width != 0)
            {
                if (includeZoomFactor)
                {
                    size = new Vector2(Width * MapZoomFactor.X, Height * MapZoomFactor.Y);
                }
                else
                {
                    size = new Vector2(Width, Height);
                }
            }
        }

        return size;
    }

    /// <summary>
    ///     The world map textures are found by name (the last part of the path is the name).
    /// </summary>
    public ResourceHandle<TextureResource> GetImageTextureHandle(string path)
    {
        return ResourceManager.DefaultPool.TryFind<TextureResource>(path, out var handle) ? handle : null;
    }

    /// <summary>
    /// Zooms with the mouse wheel, keeping the point under the cursor in place
    /// </summary>
    private void HandleZoom(Vector2 canvasMin)
    {
        var delta = InputManager.MouseWheelDelta;

        if (delta == 0)
            return;

        var oldZoom = MapZoomFactor;

        if (delta > 0)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }

        if (MapZoomFactor == oldZoom)
            return;

        // The image point under the cursor stays under it: cursor = canvasMin + pan + point * zoom, before and after
        var cursor = ImGui.GetMousePos() - canvasMin;
        MapPan = cursor - (cursor - MapPan) * (MapZoomFactor / oldZoom);
    }

    /// <summary>Back to the default zoom, with the map centred in the canvas</summary>
    private void ResetView()
    {
        MapZoomFactor = GetDefaultZoomLevel();
        _centerPending = true;
    }

    // Zoom steps by a factor, so each wheel step feels the same from the whole map down to single metres
    private const float ZoomStepFactor = 1.15f;
    private const float MinZoom = 0.02f;
    private const float MaxZoom = 10.0f;

    private void ZoomIn()
    {
        MapZoomFactor = Vector2.Min(MapZoomFactor * ZoomStepFactor, new Vector2(MaxZoom));
    }

    private void ZoomOut()
    {
        MapZoomFactor = Vector2.Max(MapZoomFactor / ZoomStepFactor, new Vector2(MinZoom));
    }

    /// <summary>
    /// The whole map at about 800 px wide: the game's tiles are 1 px per metre (10496 px), the bundled images
    /// are about 4500 px
    /// </summary>
    private Vector2 GetDefaultZoomLevel()
    {
        var scale = DPI.UIScale();
        var zoom = GameTiles != null ? 0.08f : 0.2f;
        return new Vector2(zoom * scale);
    }
}
