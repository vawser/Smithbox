using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Outlines the area of each loaded overworld tile map (m60/m61 in ER and NR) with a box region wireframe, coloured by
/// the tile's size, with its map ID and name along the top edge. A tile's MSB origin is the centre of its area, which
/// is 256 m across for a small tile (_00), 512 m for a medium tile (_01) and 1024 m for a large tile (_02): the same
/// rule MapConnections_ER/NR use to place the maps. Tiles have no height, so the box spans the map's drawn objects.
/// </summary>
public class MapTileOutlineTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    // Used while a map has nothing drawn yet to take its height from
    private const float FallbackHeight = 100f;

    // How often a map's height is taken again from its objects, whose models load after the map does
    private const double BoundsRefreshSeconds = 2.0;

    private RenderScene Scene;

    private class TileOutline
    {
        public MapContainer Map;
        public int Scale;
        public float Size;
        public string Label;
        public DebugPrimitiveRenderableProxy Proxy;

        // The box in world space, kept for the label
        public Matrix4x4 World;

        public float MinY;
        public float MaxY;
        public double BoundsTime = double.MinValue;
    }

    private readonly Dictionary<string, TileOutline> Outlines = new();

    public MapTileOutlineTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// The games whose overworld is made of tiles. The toggle is hidden for the others.
    /// </summary>
    public bool IsSupported => Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR;

    private bool IsEnabled => IsSupported && CFG.Current.MapEditor_Display_Tile_Outlines;

    /// <summary>
    /// The tile's size step (0 small, 1 medium, 2 large), or -1 if the map isn't an overworld tile
    /// </summary>
    public static int GetTileScale(string mapID)
    {
        if (!MapConnectionsUtil.TryParseMap(mapID, out var parts))
            return -1;

        if ((parts[0] != 60 && parts[0] != 61) || parts[1] == 0 || parts[2] == 0)
            return -1;

        // The tens digit is the variant (ER's _10 alternate tiles, NR's shifting earths); _99 is a skybox
        if (parts[3] == 99)
            return -1;

        var scale = parts[3] % 10;

        return scale <= 2 ? scale : -1;
    }

    #region Viewport

    /// <summary>
    /// Keeps a box in the viewport's scene for each loaded tile map. Called each frame by the active viewport.
    /// </summary>
    public void Update(VulkanViewport viewport)
    {
        if (!IsEnabled)
        {
            Release();
            return;
        }

        if (viewport.RenderScene != Scene)
        {
            Release();
            Scene = viewport.RenderScene;
        }

        if (Scene == null)
            return;

        var now = ImGui.GetTime();
        var seen = new HashSet<string>();

        foreach (var wrapper in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            var map = wrapper.MapContainer;

            if (map == null || map.LoadState is not MapContentLoadState.Loaded || map.RootObject == null)
                continue;

            // A map is drawn in the scene of the view that loaded it
            if (map.OwnerView != null && map.OwnerView != View)
                continue;

            var scale = GetTileScale(map.Name);

            if (scale < 0)
                continue;

            seen.Add(map.Name);

            if (!Outlines.TryGetValue(map.Name, out var outline) || outline.Map != map)
            {
                outline?.Proxy?.Dispose();

                outline = new TileOutline
                {
                    Map = map,
                    Scale = scale,
                    Size = 256f * (1 << scale),
                    Label = MakeLabel(map.Name),
                    Proxy = MakeProxy(scale)
                };

                Outlines[map.Name] = outline;
            }

            if (now - outline.BoundsTime > BoundsRefreshSeconds)
            {
                UpdateHeight(outline);
                outline.BoundsTime = now;
            }

            // The map's root carries the offset (and any rotation) that places the tile beside the others, and its
            // origin is the tile's centre. The box region primitive spans -0.5..0.5 in X and Z and 0..1 in Y.
            var root = map.RootObject.GetLocalTransform().WorldMatrix;
            var height = Math.Max(outline.MaxY - outline.MinY, 1f);

            outline.World = Matrix4x4.CreateScale(outline.Size, height, outline.Size)
                * Matrix4x4.CreateTranslation(0, outline.MinY - root.Translation.Y, 0)
                * root;

            outline.Proxy.World = outline.World;
            SetColor(outline.Proxy, GetColor(outline.Scale));
            outline.Proxy.Visible = true;
        }

        // Maps since unloaded
        if (seen.Count != Outlines.Count)
        {
            foreach (var name in new List<string>(Outlines.Keys))
            {
                if (seen.Contains(name))
                    continue;

                Outlines[name].Proxy?.Dispose();
                Outlines.Remove(name);
            }
        }
    }

    /// <summary>
    /// The lowest and highest point of the map's drawn objects, in world space
    /// </summary>
    private static void UpdateHeight(TileOutline outline)
    {
        BoundingBox? bounds = null;

        foreach (var ent in outline.Map.Objects)
        {
            if (ent.RenderSceneMesh == null)
                continue;

            var box = ent.RenderSceneMesh.GetBounds();

            if (!float.IsFinite(box.Min.Y) || !float.IsFinite(box.Max.Y))
                continue;

            bounds = bounds is BoundingBox b ? BoundingBox.Combine(b, box) : box;
        }

        if (bounds is BoundingBox all)
        {
            outline.MinY = all.Min.Y;
            outline.MaxY = all.Max.Y;
        }
        else
        {
            var y = outline.Map.RootObject.GetLocalTransform().Position.Y;

            outline.MinY = y;
            outline.MaxY = y + FallbackHeight;
        }
    }

    private string MakeLabel(string mapID)
    {
        var alias = AliasHelper.GetMapNameAlias(Project, mapID);

        return string.IsNullOrEmpty(alias) ? mapID : $"{mapID}: {alias}";
    }

    private DebugPrimitiveRenderableProxy MakeProxy(int scale)
    {
        // The standard box region wireframe, without the region colours, the region draw filter or picking
        var proxy = RenderableHelper.GetBoxRegionProxy(Scene);
        proxy.Pickable = false;

        SetColor(proxy, GetColor(scale));

        return proxy;
    }

    private static void SetColor(DebugPrimitiveRenderableProxy proxy, Color color)
    {
        if (proxy.BaseColor != color)
        {
            proxy.BaseColor = color;
            proxy.HighlightedColor = color;
        }
    }

    private static Vector3 GetColorValue(int scale) => scale switch
    {
        0 => CFG.Current.MapEditor_Tile_Outline_Small_Color,
        1 => CFG.Current.MapEditor_Tile_Outline_Medium_Color,
        _ => CFG.Current.MapEditor_Tile_Outline_Large_Color
    };

    private static Color GetColor(int scale)
    {
        var c = GetColorValue(scale);

        return Color.FromArgb(255, (int)Math.Clamp(c.X, 0, 255), (int)Math.Clamp(c.Y, 0, 255), (int)Math.Clamp(c.Z, 0, 255));
    }

    private void Release()
    {
        foreach (var outline in Outlines.Values)
        {
            outline.Proxy?.Dispose();
        }

        Outlines.Clear();
        Scene = null;
    }

    // The middle of the box's north top edge, in the primitive's unit space. North is +Z: the tile grid's Z index
    // (the third part of the map ID) grows northward, and MapConnections_ER/NR place tiles at +256 m Z per step.
    private static readonly Vector3 NorthEdgeMidpoint = new(0, 1, 0.5f);

    /// <summary>
    /// The map ID and name above the middle of the north top edge of each box, so the label stays put as the
    /// camera moves (and isn't drawn while that point is off screen). Called by the viewport's overlay.
    /// </summary>
    public void DrawLabels(VulkanViewport viewport)
    {
        if (!IsEnabled || viewport.RenderScene != Scene || Outlines.Count == 0)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var origin = new Vector2(viewport.X, viewport.Y);
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
        var lift = 4f * DPI.UIScale();

        foreach (var outline in Outlines.Values)
        {
            var world = Vector3.Transform(NorthEdgeMidpoint, outline.World);
            var screen = viewport.WorldToScreen(world, out bool inFront);

            if (!inFront || screen.X < 0 || screen.Y < 0 || screen.X > viewport.Width || screen.Y > viewport.Height)
                continue;

            var color = ImGui.GetColorU32(new Vector4(GetColorValue(outline.Scale) / 255f, 1));
            var textSize = ImGui.CalcTextSize(outline.Label);
            var at = origin + screen - new Vector2(textSize.X / 2, textSize.Y + lift);

            drawList.AddText(at + Vector2.One, shadow, outline.Label);
            drawList.AddText(at, color, outline.Label);
        }
    }

    #endregion

    #region Map List Header

    /// <summary>
    /// The toggle in the Map List header; right-click it for the colours. Hidden for games without tiles.
    /// </summary>
    public void DisplayHeaderButton()
    {
        if (!IsSupported)
            return;

        ImGui.SameLine();

        var enabled = CFG.Current.MapEditor_Display_Tile_Outlines;

        if (enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);

        if (ImGui.Button($"{Icons.Th}##tileOutlineToggle", DPI.IconButtonSize))
        {
            CFG.Current.MapEditor_Display_Tile_Outlines = !enabled;
        }

        if (enabled)
            ImGui.PopStyleColor();

        GUI.Tooltip(LOC.Get(enabled ? "MAP_Tile_Outline_Hide_TT" : "MAP_Tile_Outline_Show_TT"));

        if (ImGui.BeginPopupContextItem("tileOutlineColors"))
        {
            ImGui.TextUnformatted(LOC.Get("MAP_Tile_Outline_Colors_Header"));
            ImGui.Separator();

            ColorEdit("MAP_Tile_Outline_Small_Color", ref CFG.Current.MapEditor_Tile_Outline_Small_Color);
            ColorEdit("MAP_Tile_Outline_Medium_Color", ref CFG.Current.MapEditor_Tile_Outline_Medium_Color);
            ColorEdit("MAP_Tile_Outline_Large_Color", ref CFG.Current.MapEditor_Tile_Outline_Large_Color);

            ImGui.EndPopup();
        }
    }

    private static void ColorEdit(string key, ref Vector3 value)
    {
        var color = value / 255f;

        if (ImGui.ColorEdit3($"{LOC.Get(key)}##{key}", ref color, ImGuiColorEditFlags.NoInputs))
        {
            value = color * 255f;
        }
    }

    #endregion
}
