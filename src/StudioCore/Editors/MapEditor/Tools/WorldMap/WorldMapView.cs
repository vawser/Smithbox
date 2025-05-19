using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Formats;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.MapEditor.Tools.WorldMap;

public class WorldMapView : IResourceEventListener
{
    private MapEditorScreen Editor;

    private Task _loadingTask;

    private WorldMapLayout VanillaLayout = null;
    private WorldMapLayout SoteLayout = null;

    private bool IsMapTextureLoaded { get; set; }
    private bool IsMapWindowOpen { get; set; }

    public MapSource CurrentMapSource = MapSource.LandsBetween;

    private Vector2 MapZoomFactor;
    private float MapZoomFactorStep = 0.1f;

    private Vector2 TextureViewWindowPosition = new Vector2(0, 0);
    private Vector2 TextureViewScrollPosition = new Vector2(0, 0);

    private Vector2 MapTextureTrueSize = new Vector2();
    private Vector2 MapTextureSize = new Vector2();
    private Vector2 MapCurseRelativePosition = new Vector2();
    private Vector2 MapCurseRelativePositionInWindow = new Vector2();

    private List<string> HoveredMapTiles = new List<string>();
    private List<string> SelectedMapTiles = new List<string>();

    private bool HasSetupMaps = false;

    private bool _isDraggingMap = false;
    private Vector2 _lastMousePos = Vector2.Zero;

    public WorldMapView(MapEditorScreen screen)
    {
        Editor = screen;

        IsMapWindowOpen = false;
        IsMapTextureLoaded = false;
        MapZoomFactor = GetDefaultZoomLevel();
        DPI.UIScaleChanged += (_, _) =>
        {
            MapZoomFactor = GetDefaultZoomLevel();
        };
    }

    /// <summary>
    /// Load the map textures and generate the layouts.
    /// </summary>
    public void InitializeWorldMap()
    {
        if (!HasSetupMaps)
        {
            LoadWorldMapTexture();
            GenerateWorldMapLayout_Vanilla();
            GenerateWorldMapLayout_SOTE();

            HasSetupMaps = true;
        }
    }

    /// <summary>
    /// The Lands Between menu option
    /// </summary>
    public void DisplayMenuOption()
    {
        if (!ResourceManager.IsResourceLoaded("smithbox/worldmap/world_map_vanilla", AccessLevel.AccessGPUOptimizedOnly))
            LoadWorldMapTexture();

        if (!ResourceManager.IsResourceLoaded("smithbox/worldmap/world_map_sote", AccessLevel.AccessGPUOptimizedOnly))
            LoadWorldMapTexture();

        IsMapWindowOpen = !IsMapWindowOpen;
    }

    public void DisplayWorldMap()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ToggleWorldMap))
        {
            IsMapWindowOpen = !IsMapWindowOpen;
        }

        if (Editor.Project.ProjectType != ProjectType.ER)
            return;

        if (!IsMapWindowOpen)
            return;

        DisplayMap();
    }

    public void ControlsMenu()
    {
        if (ImGui.MenuItem("Controls"))
        {
            ImGui.OpenPopup("controlHint");
        }

        if (ImGui.BeginPopup("controlHint"))
        {
            ImGui.Text($"Left click to navigate the map.");
            ImGui.Text($"Right click on the map to filter the map list to the map tiles underneath your click.");
            ImGui.Text($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
            ImGui.Text($"Press {KeyBindings.Current.TEXTURE_ResetZoomLevel.HintText} to reset zoom level to 100%.");

            ImGui.EndPopup();
        }
    }

    public void MapSourceMenu()
    {
        if (ImGui.BeginMenu("Map Source"))
        {
            if (ImGui.MenuItem("Lands Between", KeyBindings.Current.MAP_ToggleWorldMap.HintText))
            {
                CurrentMapSource = MapSource.LandsBetween;
            }
            UIHelper.Tooltip($"Switch the map image to this.");

            if (ImGui.MenuItem("Shadow of the Erdtree", KeyBindings.Current.MAP_ToggleWorldMap.HintText))
            {
                CurrentMapSource = MapSource.ShadowOfTheErdtree;
            }
            UIHelper.Tooltip($"Switch the map image to this.");

            ImGui.EndMenu();
        }
    }
    public void SettingsMenu()
    {
        if (ImGui.BeginMenu("Settings"))
        {
            ImGui.Checkbox("Display Tiles", ref CFG.Current.WorldMapDisplayTiles);
            UIHelper.Tooltip($"If enabled, the tile shapes will overlay the map.");

            ImGui.Checkbox("Display Small Tiles", ref CFG.Current.WorldMapDisplaySmallTiles);
            UIHelper.Tooltip($"If enabled, the small tile shapes will overlay the map.");

            ImGui.Checkbox("Display Medium Tiles", ref CFG.Current.WorldMapDisplayMediumTiles);
            UIHelper.Tooltip($"If enabled, the medium tile shapes will overlay the map.");

            ImGui.Checkbox("Display Large Tiles", ref CFG.Current.WorldMapDisplayLargeTiles);
            UIHelper.Tooltip($"If enabled, the large tile shapes will overlay the map.");

            var viewport = ImGui.GetMainViewport();
            float width = viewport.Size.X;
            float height = viewport.Size.Y;

            ImGui.DragFloat("Window Width", ref CFG.Current.WorldMapWidth, 1.0f, 100.0f, width);
            ImGui.DragFloat("Window Height", ref CFG.Current.WorldMapHeight, 1.0f, 100.0f, height);

            ImGui.EndMenu();
        }
    }

    public void DisplayMap()
    {
        var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollWithMouse;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowSize(new Vector2(CFG.Current.WorldMapWidth, CFG.Current.WorldMapHeight));
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.Begin("World Map##WorldMapImage", flags);


        if (ImGui.BeginMenuBar())
        {
            ControlsMenu();
            MapSourceMenu();
            SettingsMenu();

            ImGui.EndMenuBar();
        }

        Editor.FocusManager.SwitchWindowContext(MapEditorContext.WorldMap);

        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        var mousePos = ImGui.GetMousePos();

        // Store scroll position for potential modification
        float currentScrollX = ImGui.GetScrollX();
        float currentScrollY = ImGui.GetScrollY();

        Vector2 currentMousePos = ImGui.GetMousePos();
        Vector2 windowPos = ImGui.GetWindowPos();
        Vector2 windowSize = ImGui.GetWindowSize();
        bool isMouseInMapWindow = currentMousePos.X >= windowPos.X &&
                                  currentMousePos.Y >= windowPos.Y &&
                                  currentMousePos.X < windowPos.X + windowSize.X &&
                                  currentMousePos.Y < windowPos.Y + windowSize.Y;

        // Start drag on left click in window
        if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            _isDraggingMap = true;
            _lastMousePos = currentMousePos;
        }

        // Stop drag on release
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            _isDraggingMap = false;
        }

        if (_isDraggingMap && isMouseInMapWindow)
        {
            Vector2 mouseDelta = currentMousePos - _lastMousePos;

            // Reverse scroll direction: drag down → texture up
            ImGui.SetScrollX(currentScrollX - mouseDelta.X);
            ImGui.SetScrollY(currentScrollY - mouseDelta.Y);

            _lastMousePos = currentMousePos;
        }

        // Map
        TextureViewWindowPosition = ImGui.GetWindowPos();
        TextureViewScrollPosition = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle("smithbox/worldmap/world_map_vanilla");

        if (CurrentMapSource is MapSource.ShadowOfTheErdtree)
        {
            resHandle = GetImageTextureHandle("smithbox/worldmap/world_map_sote");
        }

        if (resHandle != null)
        {
            TextureResource texRes = resHandle.Get();

            if (texRes != null)
            {
                MapTextureTrueSize = GetImageSize(texRes, false);
                MapTextureSize = GetImageSize(texRes, true);
                MapCurseRelativePosition = GetRelativePosition(TextureViewWindowPosition, TextureViewScrollPosition);
                MapCurseRelativePositionInWindow = GetRelativePositionWindowOnly(TextureViewWindowPosition);

                var textureId = new ImTextureID(texRes.GPUTexture.TexHandle);
                ImGui.Image(textureId, MapTextureSize);
            }
        }

        // Tile Overlay
        if (CFG.Current.WorldMapDisplayTiles)
        {
            var drawList = ImGui.GetWindowDrawList();
            var tileList = CurrentMapSource == MapSource.LandsBetween ? VanillaLayout.Tiles : SoteLayout.Tiles;

            foreach (var tile in tileList)
            {
                if (tile.TileType is MapTileType.Small && !CFG.Current.WorldMapDisplaySmallTiles)
                    continue;

                if (tile.TileType is MapTileType.Medium && !CFG.Current.WorldMapDisplayMediumTiles)
                    continue;

                if (tile.TileType is MapTileType.Large && !CFG.Current.WorldMapDisplayLargeTiles)
                    continue;

                var tileSize = new Vector2(tile.Width, tile.Height);

                // Image-space coordinates of tile (unscaled, unscrolled)
                Vector2 tileTopLeft = new Vector2(tile.X, tile.Y);
                Vector2 tileBottomRight = tileTopLeft + new Vector2(tile.Width, tile.Height);

                // Apply zoom
                tileTopLeft *= MapZoomFactor;
                tileBottomRight *= MapZoomFactor;

                // Apply scroll and window offsets (convert to screen space)
                Vector2 imageTopLeftScreen = TextureViewWindowPosition - TextureViewScrollPosition + new Vector2(3 * DPI.GetUIScale(), 24 * DPI.GetUIScale());

                Vector2 drawStart = imageTopLeftScreen + tileTopLeft;
                Vector2 drawEnd = imageTopLeftScreen + tileBottomRight;

                // Highlight hovered or selected
                uint color = ImGui.GetColorU32(new Vector4(1, 1, 0, 0.5f)); // Yellow default
                if (HoveredMapTiles.Contains(tile.Name))
                    color = ImGui.GetColorU32(new Vector4(0, 1, 0, 0.5f)); // Green for hover
                else if (SelectedMapTiles.Contains(tile.Name))
                    color = ImGui.GetColorU32(new Vector4(1, 0, 0, 0.5f)); // Red for selected

                // Draw filled semi-transparent rect (optional)
                drawList.AddRectFilled(drawStart, drawEnd, ImGui.GetColorU32(new Vector4(1, 1, 0, 0.05f)));

                // Draw outline
                drawList.AddRect(drawStart, drawEnd, color, 0, ImDrawFlags.None, 2.0f);
            }
        }

        ImGui.End();

        HoveredMapTiles = GetMatchingMaps(MapCurseRelativePosition);

        if (HoveredMapTiles != null && HoveredMapTiles.Count > 0)
        {
            if (MapCurseRelativePositionInWindow.X > 0 && MapCurseRelativePositionInWindow.X < MapTextureSize.X &&
                MapCurseRelativePositionInWindow.Y > 0 && MapCurseRelativePositionInWindow.Y < MapTextureSize.Y)
            {
                ImGui.BeginTooltip();

                foreach (var tile in HoveredMapTiles)
                {
                    ImGui.Text($"{tile} ({AliasUtils.GetMapNameAlias(Editor.Project, tile)})");
                }

                ImGui.EndTooltip();
            }
        }

        HandleZoom();

        // Select Maps under Point
        if (InputTracker.GetMouseButtonDown(MouseButton.Right))
        {
            if (MapCurseRelativePositionInWindow.X > 0 && MapCurseRelativePositionInWindow.X < windowWidth && MapCurseRelativePositionInWindow.Y > 0 && MapCurseRelativePositionInWindow.Y < windowHeight)
            {
                if (HoveredMapTiles != null && HoveredMapTiles.Count > 0)
                {
                    SelectedMapTiles = HoveredMapTiles;

                    Editor.MapListView.UpdateMapList(HoveredMapTiles);
                }
            }
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXTURE_ResetZoomLevel))
        {
            MapZoomFactor = GetDefaultZoomLevel();
        }

        if (InputTracker.GetKey(Key.Escape))
        {
            IsMapWindowOpen = false;
        }
    }

    private void LoadWorldMapTexture()
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob($@"Loading World Map textures");
        ResourceDescriptor ad = new ResourceDescriptor();
        ad.AssetVirtualPath = "smithbox/worldmap";

        if (!ResourceManager.IsResourceLoaded(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
        {
            if (ad.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly, true);
            }

            _loadingTask = job.Complete();
        }

        ResourceManager.AddResourceListener<TextureResource>(ad.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);

        IsMapTextureLoaded = true;
    }

    private void GenerateWorldMapLayout_Vanilla()
    {
        var smallRows = new List<int>() { 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 };
        var smallCols = new List<int>() { 63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30 };
        var mediumRows = new List<int>() { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 };
        var mediumCols = new List<int>() { 31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15 };
        var largeRows = new List<int>() { 8, 9, 10, 11, 12, 13, 14 };
        var largeCols = new List<int>() { 15, 14, 13, 12, 11, 10, 9, 8, 7 };


        VanillaLayout = new WorldMapLayout(Editor, "60", 480, 55);
        VanillaLayout.GenerateTiles(smallRows, smallCols, "00", 124, MapTileType.Small);
        VanillaLayout.GenerateTiles(mediumRows, mediumCols, "01", 248, MapTileType.Medium);
        VanillaLayout.GenerateTiles(largeRows, largeCols, "02", 496, MapTileType.Large);
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

        SoteLayout = new WorldMapLayout(Editor, "61", SOTE_xOffset, SOTE_yOffset);
        SoteLayout.GenerateTiles(smallRows, smallCols, "00", 256, MapTileType.Small);
        SoteLayout.GenerateTiles(mediumRows, mediumCols, "01", 528, MapTileType.Medium);
        SoteLayout.GenerateTiles(largeRows, largeCols, "02", 1056, MapTileType.Large);
    }

    private List<string> GetMatchingMaps(Vector2 pos)
    {
        List<string> matches = new List<string>();

        var tiles = VanillaLayout.Tiles;

        if (CurrentMapSource is MapSource.ShadowOfTheErdtree)
        {
            tiles = SoteLayout.Tiles;
        }

        foreach (var tile in tiles)
        {
            var tileName = "";
            var match = false;

            (tileName, match) = MatchMousePosToIcon(tile, pos);

            if (match && !matches.Contains(tileName))
            {
                matches.Add(tileName);
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

    private Vector2 GetRelativePositionWindowOnly(Vector2 windowPos)
    {
        var scale = DPI.GetUIScale();

        Vector2 relativePos = new Vector2(0, 0);

        var fixedX = 3 * scale;
        var fixedY = 24 * scale;
        var cursorPos = ImGui.GetMousePos();

        // Account for window position
        relativePos.X = cursorPos.X - (windowPos.X + fixedX);
        relativePos.Y = cursorPos.Y - (windowPos.Y + fixedY);

        return relativePos;
    }

    private (string, bool) MatchMousePosToIcon(WorldMapTile tile, Vector2 relativePos)
    {
        var cursorPos = relativePos;

        var Name = tile.Name;

        var success = false;

        float Xmin = tile.X;
        float Xmax = Xmin + tile.Width;
        float Ymin = tile.Y;
        float Ymax = Ymin + tile.Height;

        if (cursorPos.X > Xmin && cursorPos.X < Xmax && cursorPos.Y > Ymin && cursorPos.Y < Ymax)
        {
            success = true;
        }

        return (Name, success);
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

    public ResourceHandle<TextureResource> GetImageTextureHandle(string path)
    {
        var virtName = $@"{path}".ToLower();

        var resources = ResourceManager.GetResourceDatabase();

        if (resources.ContainsKey(virtName))
        {
            return (ResourceHandle<TextureResource>)resources[virtName];
        }

        return null;
    }

    private void HandleZoom()
    {
        var delta = InputTracker.GetMouseWheelDelta();

        if (delta > 0)
        {
            ZoomIn();
        }
        if (delta < 0)
        {
            ZoomOut();
        }
    }

    private void ZoomIn()
    {
        MapZoomFactor.X = MapZoomFactor.X + MapZoomFactorStep;
        MapZoomFactor.Y = MapZoomFactor.Y + MapZoomFactorStep;

        if (MapZoomFactor.X > 10.0f)
        {
            MapZoomFactor.X = 10.0f;
        }
        if (MapZoomFactor.Y > 10.0f)
        {
            MapZoomFactor.Y = 10.0f;
        }
    }
    private void ZoomOut()
    {
        MapZoomFactor.X = MapZoomFactor.X - MapZoomFactorStep;
        MapZoomFactor.Y = MapZoomFactor.Y - MapZoomFactorStep;

        if (MapZoomFactor.X < 0.1f)
        {
            MapZoomFactor.X = 0.1f;
        }
        if (MapZoomFactor.Y < 0.1f)
        {
            MapZoomFactor.Y = 0.1f;
        }
    }
    private Vector2 GetDefaultZoomLevel()
    {
        var scale = DPI.GetUIScale();
        return new Vector2(float.Round(0.2f * scale, 1), float.Round(0.2f * scale, 1));
    }

    private Vector2 GetRelativePosition(Vector2 windowPos, Vector2 scrollPos)
    {
        var scale = DPI.GetUIScale();

        Vector2 relativePos = new Vector2(0, 0);

        // Offsets to account for imgui spacing between window and image texture
        var fixedX = 8 * scale;
        var fixedY = 29 * scale;
        var cursorPos = ImGui.GetMousePos();

        // Account for window position and scroll
        relativePos.X = cursorPos.X - (windowPos.X + fixedX - scrollPos.X);
        relativePos.Y = cursorPos.Y - (windowPos.Y + fixedY - scrollPos.Y);

        // Account for zoom
        relativePos.X = relativePos.X / MapZoomFactor.X;
        relativePos.Y = relativePos.Y / MapZoomFactor.Y;

        return relativePos;
    }
}

public enum MapSource
{
    LandsBetween,
    ShadowOfTheErdtree
}