using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using SoulsFormats;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Formats;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Resource;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.MapEditor.WorldMap;

public class WorldMapScreen : IResourceEventListener
{
    private Task _loadingTask;
    private bool LoadedWorldMapTexture { get; set; }
    private bool WorldMapOpen { get; set; }

    private static List<TPF.Texture> WorldMapTextures;
    private ShoeboxLayout WorldMapLayout = null;

    private Vector2 zoomFactor;

    private float zoomFactorStep = 0.1f;

    private Vector2 TextureViewWindowPosition = new Vector2(0, 0);
    private Vector2 TextureViewScrollPosition = new Vector2(0, 0);

    private Vector2 trueSize = new Vector2();
    private Vector2 size = new Vector2();
    private Vector2 relativePos = new Vector2();
    private Vector2 relativePosWindowPosition = new Vector2();

    List<string> currentHoverMaps = new List<string>();

    public WorldMapScreen()
    {
        WorldMapOpen = false;
        LoadedWorldMapTexture = false;
        zoomFactor = GetDefaultZoomLevel();
        Smithbox.UIScaleChanged += (_, _) =>
        {
            zoomFactor = GetDefaultZoomLevel();
        };
    }

    public void OnProjectChanged()
    {
        if (Project.Type is ProjectType.ER)
        {
            LoadWorldMapTexture();
            LoadWorldMapLayout();
        }
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKey(Key.LControl))
        {
            HandleZoom();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.TextureViewer_ZoomReset))
        {
            zoomFactor = GetDefaultZoomLevel();
        }
    }

    public void DisplayWorldMapButton()
    {
        if (Project.Type != ProjectType.ER)
            return;

        var scale = Smithbox.GetUIScale();

        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        var widthUnit = windowWidth / 100;

        if (LoadedWorldMapTexture)
        {
            if (ImGui.Button("Open World Map", new Vector2(widthUnit * 60, 20 * scale)))
            {
                WorldMapOpen = !WorldMapOpen;
            }
            ImguiUtils.ShowHoverTooltip("Open a world map for Elden Ring. Allows you to easily select open-world tiles.");

            ImGui.SameLine();
            if (ImGui.Button("Clear", new Vector2(widthUnit * 34, 20 * scale)))
            {
                EditorContainer.MsbEditor.WorldMap_ClickedMapZone = null;
            }
            ImguiUtils.ShowHoverTooltip("Clear the current world map selection (if any).");
        }
    }

    public void DisplayWorldMap()
    {
        if (Project.Type != ProjectType.ER)
            return;

        if (!WorldMapOpen)
            return;

        ImGui.Begin("World Map##WorldMapImage", ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        var windowHeight = ImGui.GetWindowHeight();
        var windowWidth = ImGui.GetWindowWidth();
        var mousePos = ImGui.GetMousePos();

        TextureViewWindowPosition = ImGui.GetWindowPos();
        TextureViewScrollPosition = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle("smithbox/worldmap/world_map");

        if (resHandle != null)
        {
            TextureResource texRes = resHandle.Get();

            if (texRes != null)
            {
                trueSize = GetImageSize(texRes, false);
                size = GetImageSize(texRes, true);
                relativePos = GetRelativePosition(TextureViewWindowPosition, TextureViewScrollPosition);
                relativePosWindowPosition = GetRelativePositionWindowOnly(TextureViewWindowPosition);

                IntPtr handle = (nint)texRes.GPUTexture.TexHandle;

                ImGui.Image(handle, size);
            }
        }

        ImGui.End();

        ImGui.Begin("Properties##WorldMapProperties");

        ImguiUtils.WrappedText($"Press Left Mouse button to select an area of the map to filter the map object list by.");
        ImguiUtils.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        ImguiUtils.WrappedText($"Press {KeyBindings.Current.TextureViewer_ZoomReset.HintText} to reset zoom level to 100%.");
        ImguiUtils.WrappedText($"");

        //ImGui.Text($"Relative Position: {relativePos}");
        //ImGui.Text($"Relative (Sans Scroll) Position: {relativePosWindowPosition}");
        //ImGui.Text($"mousePos: {mousePos}");
        //ImGui.Text($"windowHeight: {windowHeight}");
        //ImGui.Text($"windowWidth: {windowWidth}");

        currentHoverMaps = GetMatchingMaps(relativePos);

        ImGui.Separator();
        ImGui.Text($"Preview Maps:");
        ImGui.Separator();

        // Hover Maps
        if (currentHoverMaps != null && currentHoverMaps.Count > 0)
        {
            foreach(var match in currentHoverMaps)
            {
                ImGui.Text($"{match}");
                AliasUtils.DisplayAlias(MapAliasBank.GetMapName(match));
            }
        }

        ImGui.Separator();
        ImGui.Text($"Displayed Maps:");
        ImGui.Separator();

        // Stored Click Maps
        if (EditorContainer.MsbEditor.WorldMap_ClickedMapZone != null && EditorContainer.MsbEditor.WorldMap_ClickedMapZone.Count > 0)
        {
            foreach (var match in EditorContainer.MsbEditor.WorldMap_ClickedMapZone)
            {
                ImGui.Text($"{match}");
                AliasUtils.DisplayAlias(MapAliasBank.GetMapName(match));
            }
        }

        ImGui.End();

        if (InputTracker.GetMouseButtonDown(MouseButton.Left))
        {
            if (relativePosWindowPosition.X > 0 && relativePosWindowPosition.X < windowWidth && relativePosWindowPosition.Y > 0 && relativePosWindowPosition.Y < windowHeight)
            {
                if (currentHoverMaps != null && currentHoverMaps.Count > 0)
                {
                    EditorContainer.MsbEditor.WorldMap_ClickedMapZone = currentHoverMaps;
                }
            }
        }
    }

    private void LoadWorldMapTexture() 
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob($@"Loading World Map texture");
        ResourceDescriptor ad = new ResourceDescriptor();
        ad.AssetVirtualPath = "smithbox/worldmap";

        if (!ResourceManager.IsResourceLoadedOrInFlight(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
        {
            if (ad.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly, true);
            }

            _loadingTask = job.Complete();
        }

        ResourceManager.AddResourceListener<TextureResource>(ad.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);

        LoadedWorldMapTexture = true;
    }

    private void LoadWorldMapLayout()
    {
        string sourcePath = $@"{AppContext.BaseDirectory}\Assets\WorldMap\ER_WorldMap.layout";

        if (File.Exists(sourcePath))
        {
            WorldMapLayout = new ShoeboxLayout(sourcePath);
        }
        else
        {
            TaskLogs.AddLog($"Failed to load Shoebox Layout: {sourcePath}");
        }
    }

    private List<string> GetMatchingMaps(Vector2 pos)
    {
        List<string> matches =  new List<string>();

        foreach(var entry in WorldMapLayout.TextureAtlases)
        {
            foreach(var sub in entry.SubTextures)
            {
                var subTex = "";
                var match = false;

                (subTex, match) = MatchMousePosToIcon(sub, pos);

                if(match && !matches.Contains(subTex))
                {
                    matches.Add(subTex);
                }
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
        var scale = Smithbox.GetUIScale();

        Vector2 relativePos = new Vector2(0, 0);

        var fixedX = 3 * scale;
        var fixedY = 24 * scale;
        var cursorPos = ImGui.GetMousePos();

        // Account for window position
        relativePos.X = cursorPos.X - ((windowPos.X + fixedX));
        relativePos.Y = cursorPos.Y - ((windowPos.Y + fixedY));

        return relativePos;
    }

    private (string, bool) MatchMousePosToIcon(SubTexture entry, Vector2 relativePos)
    {
        var cursorPos = relativePos;

        var SubTexName = entry.Name.Replace(".png", "");

        var success = false;

        float Xmin = float.Parse(entry.X);
        float Xmax = Xmin + float.Parse(entry.Width);
        float Ymin = float.Parse(entry.Y);
        float Ymax = Ymin + float.Parse(entry.Height);

        if (cursorPos.X > Xmin && cursorPos.X < Xmax && cursorPos.Y > Ymin && cursorPos.Y < Ymax)
        {
            success = true;
        }

        return (SubTexName, success);
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
                    size = new Vector2((Width * zoomFactor.X), (Height * zoomFactor.Y));
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
        zoomFactor.X = zoomFactor.X + zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y + zoomFactorStep;

        if (zoomFactor.X > 10.0f)
        {
            zoomFactor.X = 10.0f;
        }
        if (zoomFactor.Y > 10.0f)
        {
            zoomFactor.Y = 10.0f;
        }
    }
    private void ZoomOut()
    {
        zoomFactor.X = zoomFactor.X - zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y - zoomFactorStep;

        if (zoomFactor.X < 0.1f)
        {
            zoomFactor.X = 0.1f;
        }
        if (zoomFactor.Y < 0.1f)
        {
            zoomFactor.Y = 0.1f;
        }
    }
    private Vector2 GetDefaultZoomLevel()
    {
        var scale = Smithbox.GetUIScale();
        return new Vector2(float.Round(0.2f * scale, 1), float.Round(0.2f * scale, 1));
    }

    private Vector2 GetRelativePosition(Vector2 windowPos, Vector2 scrollPos)
    {
        var scale = Smithbox.GetUIScale();

        Vector2 relativePos = new Vector2(0, 0);

        // Offsets to account for imgui spacing between window and image texture
        var fixedX = 8 * scale;
        var fixedY = 29 * scale;
        var cursorPos = ImGui.GetMousePos();

        // Account for window position and scroll
        relativePos.X = cursorPos.X - ((windowPos.X + fixedX) - scrollPos.X);
        relativePos.Y = cursorPos.Y - ((windowPos.Y + fixedY) - scrollPos.Y);

        // Account for zoom
        relativePos.X = relativePos.X / zoomFactor.X;
        relativePos.Y = relativePos.Y / zoomFactor.Y;

        return relativePos;
    }
}
