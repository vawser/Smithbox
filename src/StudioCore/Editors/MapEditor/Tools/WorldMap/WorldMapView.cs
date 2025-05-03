using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
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
    private bool IsMapOpen { get; set; }
    private bool IsSoteMapOpen { get; set; }

    private Vector2 MapZoomFactor;
    private float MapZoomFactorStep = 0.1f;

    private Vector2 TextureViewWindowPosition = new Vector2(0, 0);
    private Vector2 TextureViewScrollPosition = new Vector2(0, 0);

    private Vector2 MapTextureTrueSize = new Vector2();
    private Vector2 MapTextureSize = new Vector2();
    private Vector2 MapCurseRelativePosition = new Vector2();
    private Vector2 MapCurseRelativePositionInWindow = new Vector2();

    List<string> currentHoverMaps = new List<string>();
    public List<string> WorldMap_ClickedMapZone = new List<string>();

    public WorldMapView(MapEditorScreen screen)
    {
        Editor = screen;

        IsMapOpen = false;
        IsMapTextureLoaded = false;
        MapZoomFactor = GetDefaultZoomLevel();
        DPI.UIScaleChanged += (_, _) =>
        {
            MapZoomFactor = GetDefaultZoomLevel();
        };
    }

    public void OnProjectChanged()
    {
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
            LoadWorldMapTexture();
            GenerateWorldMapLayout_Vanilla();
            GenerateWorldMapLayout_SOTE();
        }
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ToggleERMapVanilla))
        {
            IsMapOpen = !IsMapOpen;
            if (IsSoteMapOpen)
            {
                IsSoteMapOpen = false;
                IsMapOpen = true;
            };
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ToggleERMapSOTE))
        {
            IsMapOpen = !IsMapOpen;
            if (!IsSoteMapOpen)
            {
                IsSoteMapOpen = true;
                IsMapOpen = true;
            };
        }

        if (InputTracker.GetKey(Key.LControl))
        {
            HandleZoom();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXTURE_ResetZoomLevel))
        {
            MapZoomFactor = GetDefaultZoomLevel();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DragWorldMap))
        {
            AdjustScrollNextFrame = true;
        }

        if (InputTracker.GetKey(Key.Escape))
        {
            IsSoteMapOpen = false;
            IsMapOpen = false;
        }
    }

    /// <summary>
    /// Load the maps via the ContentViews when appropriate
    /// </summary>
    public void LoadMapsOnClick(List<string> mapIDs)
    {
        if (Editor.MapListView.SetupContentViews)
        {
            foreach (var entry in Editor.MapListView.MapIDs)
            {
                MapContentView curView = null;

                if (Editor.MapListView.ContentViews.ContainsKey(entry))
                {
                    curView = Editor.MapListView.ContentViews[entry];
                }

                if (curView != null)
                {
                    if(mapIDs.Contains(curView.MapID))
                    {
                        curView.Load(true);
                    }
                }
            }
        }
    }

    public void DisplayWorldMapButton()
    {
        if (Editor.Project.ProjectType != ProjectType.ER)
            return;

        var scale = DPI.GetUIScale();

        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        var widthUnit = windowWidth / 100;

        if (IsMapTextureLoaded && CFG.Current.MapEditor_ShowWorldMapButtons)
        {
            if (ImGui.Button("Lands Between", new Vector2(widthUnit * 48, 20 * scale)))
            {
                if (!ResourceManager.IsResourceLoaded("smithbox/worldmap/world_map_vanilla", AccessLevel.AccessGPUOptimizedOnly))
                    LoadWorldMapTexture();

                IsMapOpen = !IsMapOpen;
                if (IsSoteMapOpen)
                {
                    IsSoteMapOpen = false;
                    IsMapOpen = true;
                };
            }
            UIHelper.ShowHoverTooltip($"Open the Lands Between world map for Elden Ring.\nAllows you to easily select open-world tiles.\nShortcut: {KeyBindings.Current.MAP_ToggleERMapVanilla.HintText}");

            ImGui.SameLine();
            if (ImGui.Button("Shadow of the Erdtree", new Vector2(widthUnit * 48, 20 * scale)))
            {
                if (!ResourceManager.IsResourceLoaded("smithbox/worldmap/world_map_sote", AccessLevel.AccessGPUOptimizedOnly))
                    LoadWorldMapTexture();

                IsMapOpen = !IsMapOpen;
                if (!IsSoteMapOpen)
                {
                    IsSoteMapOpen = true;
                    IsMapOpen = true;
                };
            }
            UIHelper.ShowHoverTooltip($"Open the Shadow of the Erdtree world map for Elden Ring.\nAllows you to easily select open-world tiles.\nShortcut: {KeyBindings.Current.MAP_ToggleERMapSOTE.HintText}");
        }
    }

    private float WorldMapScrollX = 0;
    private float WorldMapScrollY = 0;
    private float WorldMapScrollXMax = 0;
    private float WorldMapScrollYMax = 0;

    private bool AdjustScrollNextFrame = false;
    private float NextFrameAdjustmentX = 0;
    private float NextFrameAdjustmentY = 0;

    private Vector2 MouseDelta = new Vector2(0, 0);

    public void DisplayWorldMap()
    {
        if (Editor.Project.ProjectType != ProjectType.ER)
            return;

        if (!IsMapOpen)
            return;

        ImGui.Begin("World Map##WorldMapImage", ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        Editor.FocusManager.SwitchWindowContext(MapEditorContext.WorldMap);

        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        var mousePos = ImGui.GetMousePos();

        // Map Drag
        /*
        WorldMapScrollX = ImGui.GetScrollX();
        WorldMapScrollXMax = ImGui.GetScrollMaxX();
        WorldMapScrollY = ImGui.GetScrollY();
        WorldMapScrollYMax = ImGui.GetScrollMaxY();
        MouseDelta = InputTracker.MouseDelta;

        if (AdjustScrollNextFrame)
        {
            AdjustScrollNextFrame = false;
            ImGui.SetScrollFromPosX(NextFrameAdjustmentX);
        }
        */

        // Map
        TextureViewWindowPosition = ImGui.GetWindowPos();
        TextureViewScrollPosition = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle("smithbox/worldmap/world_map_vanilla");

        if (IsSoteMapOpen)
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

        ImGui.End();

        // Properties
        ImGui.Begin("Properties##WorldMapProperties");
        Editor.FocusManager.SwitchWindowContext(MapEditorContext.WorldMapProperties);

        UIHelper.WrappedText($"Press Left Mouse button to select an area of the map to filter the map object list by.");
        UIHelper.WrappedText($"");
        UIHelper.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        UIHelper.WrappedText($"Press {KeyBindings.Current.TEXTURE_ResetZoomLevel.HintText} to reset zoom level to 100%.");
        UIHelper.WrappedText($"");

        //ImGui.Text($"Relative Position: {relativePos}");
        //ImGui.Text($"Relative (Sans Scroll) Position: {relativePosWindowPosition}");
        //ImGui.Text($"mousePos: {mousePos}");
        //ImGui.Text($"windowHeight: {windowHeight}");
        //ImGui.Text($"windowWidth: {windowWidth}");
        /*
        ImGui.Text($"MouseDelta: {InputTracker.MouseDelta}");
        ImGui.Text($"scrollPosX: {WorldMapScrollX}");
        ImGui.Text($"scrollPosXMax: {WorldMapScrollXMax}");
        ImGui.Text($"scrollPosY: {WorldMapScrollY}");
        ImGui.Text($"scrollPosYMax: {WorldMapScrollYMax}");

        ImGui.InputInt("X Offset", ref SOTE_xOffset);
        ImGui.InputInt("Y Offset", ref SOTE_yOffset);

        if (ImGui.Button("Update SOTE Layout"))
        {
            GenerateWorldMapLayout_SOTE();
        }
        */

        currentHoverMaps = GetMatchingMaps(MapCurseRelativePosition);

        ImGui.Separator();
        ImGui.Text($"Selection:");
        UIHelper.ShowHoverTooltip("These are the maps that the map object list will be filtered to.");
        ImGui.Separator();

        // Stored Click Maps
        if (SelectedMapTiles.Count > 0)
        {
            foreach (var match in SelectedMapTiles)
            {
                if (ImGui.Button($"Load##load{match}"))
                {
                    Editor.Universe.LoadMap(match, false);
                    Editor.MapListView.SignalLoad(match);
                }
                ImGui.SameLine();
                UIHelper.WrappedText($"{match}");
                UIHelper.DisplayAlias(AliasUtils.GetMapNameAlias(Editor.Project, match));
            }
        }

        ImGui.Separator();
        ImGui.Text($"Maps in Tile:");
        UIHelper.ShowHoverTooltip("These are the maps that are within the tile you are currently hovering over within the world map.");
        ImGui.Separator();

        // Hover Maps
        if (currentHoverMaps != null && currentHoverMaps.Count > 0)
        {
            foreach (var match in currentHoverMaps)
            {
                UIHelper.WrappedText($"{match}");
                UIHelper.DisplayAlias(AliasUtils.GetMapNameAlias(Editor.Project, match));
            }
        }

        ImGui.End();

        if (InputTracker.GetMouseButtonDown(MouseButton.Left))
        {
            if (MapCurseRelativePositionInWindow.X > 0 && MapCurseRelativePositionInWindow.X < windowWidth && MapCurseRelativePositionInWindow.Y > 0 && MapCurseRelativePositionInWindow.Y < windowHeight)
            {
                if (currentHoverMaps != null && currentHoverMaps.Count > 0)
                {
                    SelectedMapTiles = currentHoverMaps;

                    if (CFG.Current.WorldMap_EnableFilterOnClick)
                    {
                        Editor.MapListView.SearchBarText = string.Join("|", currentHoverMaps);
                    }
                }
            }
        }
        if (InputTracker.GetMouseButtonDown(MouseButton.Right))
        {
            if (MapCurseRelativePositionInWindow.X > 0 && MapCurseRelativePositionInWindow.X < windowWidth && MapCurseRelativePositionInWindow.Y > 0 && MapCurseRelativePositionInWindow.Y < windowHeight)
            {
                if (currentHoverMaps != null && currentHoverMaps.Count > 0)
                {
                    SelectedMapTiles = currentHoverMaps;

                    if (CFG.Current.WorldMap_EnableLoadOnClick)
                    {
                        foreach (var mapId in currentHoverMaps)
                        {
                            LoadMapsOnClick(new List<string>() { mapId });
                        }
                    }
                }
            }
        }
    }

    private List<string> SelectedMapTiles = new List<string>();


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


        VanillaLayout = new WorldMapLayout("60", 480, 55);
        VanillaLayout.GenerateTiles(smallRows, smallCols, "00", 124);
        VanillaLayout.GenerateTiles(mediumRows, mediumCols, "01", 248);
        VanillaLayout.GenerateTiles(largeRows, largeCols, "02", 496);
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

        SoteLayout = new WorldMapLayout("61", SOTE_xOffset, SOTE_yOffset);
        SoteLayout.GenerateTiles(smallRows, smallCols, "00", 256);
        SoteLayout.GenerateTiles(mediumRows, mediumCols, "01", 528);
        SoteLayout.GenerateTiles(largeRows, largeCols, "02", 1056);
    }

    private List<string> GetMatchingMaps(Vector2 pos)
    {
        List<string> matches = new List<string>();

        var tiles = VanillaLayout.Tiles;

        if (IsSoteMapOpen)
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
