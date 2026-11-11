using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Map Information Editor mode of the Map Information window: paints the four index layers of the ER
/// mapinfotexbnd tiles (map piece, map name, weather, mimicry) over the world map.
/// See Claude/Documentation/MapInfoTex Editor.md.
/// </summary>
public class MapInfoEditor
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    public MapInfoTexBank Bank { get; }

    public MapInfoLayer Layer = MapInfoLayer.Weather;

    private enum PaintTool { Brush, Fill, Pick }
    private PaintTool Tool = PaintTool.Brush;

    private byte SelectedValue = MapInfoLayers.None;

    private string PaletteFilter = "";
    private bool PaletteFilterExact = false;

    // Stroke state
    private bool Painting;
    private Vector2 LastPaintPoint;

    // Hover state, set by the canvas each frame
    private bool HoverValid;
    private int HoverGx;
    private int HoverGy;

    #region Palette

    public class PaletteEntry
    {
        public byte Id;
        public string Name;
        public Vector4 Color;
        public List<string> Fields = new();
    }

    private readonly Dictionary<(MapInfoLayer, bool), List<PaletteEntry>> PaletteCache = new();
    private readonly Dictionary<(MapInfoLayer, bool), int> PaletteRowCounts = new();

    /// <summary>
    /// The rows of a layer's param a pixel can hold (IDs 0-255), each with its srcR/G/B colour (the authoring
    /// colour, which the game doesn't read) and the fields it resolves to
    /// </summary>
    public List<PaletteEntry> GetPalette(MapInfoLayer layer, bool dlc)
    {
        var key = (layer, dlc);
        Param param = null;

        if (Project.Handler.ParamData?.PrimaryBank?.Params is { } bank)
            bank.TryGetValue(MapInfoLayers.ParamName(layer, dlc), out param);

        var rowCount = param?.Rows.Count() ?? -1;

        if (PaletteCache.TryGetValue(key, out var cached) && PaletteRowCounts.GetValueOrDefault(key) == rowCount)
            return cached;

        var list = new List<PaletteEntry>();

        if (param != null)
        {
            foreach (var row in param.Rows.Where(r => r.ID is >= 0 and <= 255).OrderBy(r => r.ID))
            {
                var entry = new PaletteEntry
                {
                    Id = (byte)row.ID,
                    Name = row.Name ?? "",
                    Color = new Vector4(
                        CharacterComposition.GetValue(row, "srcR") / 255f,
                        CharacterComposition.GetValue(row, "srcG") / 255f,
                        CharacterComposition.GetValue(row, "srcB") / 255f, 1f)
                };

                foreach (var cell in row.Cells)
                {
                    var field = cell.Def.InternalName;

                    if (field.StartsWith("disableParam", StringComparison.OrdinalIgnoreCase)
                        || field.StartsWith("src", StringComparison.OrdinalIgnoreCase)
                        || field.StartsWith("pad", StringComparison.OrdinalIgnoreCase)
                        || cell.Def.DisplayType.ToString().StartsWith("dummy", StringComparison.OrdinalIgnoreCase))
                        continue;

                    entry.Fields.Add($"{field} = {cell.Value}");
                }

                list.Add(entry);
            }
        }

        PaletteCache[key] = list;
        PaletteRowCounts[key] = rowCount;
        LutVersion++;
        return list;
    }

    private PaletteEntry FindEntry(MapInfoLayer layer, bool dlc, byte id)
    {
        return GetPalette(layer, dlc).FirstOrDefault(e => e.Id == id);
    }

    private string Describe(MapInfoLayer layer, bool dlc, byte id)
    {
        if (id == MapInfoLayers.None)
            return LOC.Get("MAP_MapInfo_Value_None", id);

        var entry = FindEntry(layer, dlc, id);

        if (entry == null)
            return LOC.Get("MAP_MapInfo_Value_No_Row", id);

        return string.IsNullOrWhiteSpace(entry.Name) ? $"{id}" : $"{id} {entry.Name}";
    }

    #endregion

    public MapInfoEditor(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Bank = new MapInfoTexBank(project);
    }

    public static bool IsSupported(ProjectEntry project) => project.Descriptor.ProjectType is ProjectType.ER;

    /// <summary>The area the current map image shows, or null when it has no map information</summary>
    public static string AreaOf(WorldMapImageSource source) => source switch
    {
        WorldMapImageSource.LandsBetween => "m60",
        WorldMapImageSource.ShadowOfTheErdtree => "m61",
        _ => null
    };

    public bool IsPainting => Painting;

    #region Side Panel

    public void DisplayPanel(string area)
    {
        GUI.WrappedText(LOC.Get("MAP_MapInfo_Description"));

        if (!IsSupported(Project))
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_MapInfo_Not_Supported"));
            return;
        }

        if (!Bank.Loaded)
        {
            if (!Bank.Loading)
                Bank.Load();

            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_MapInfo_Loading"));
            return;
        }

        if (area == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_MapInfo_No_Area"));
            return;
        }

        var dlc = area == "m61";
        var areaTiles = Bank.Tiles.Values.Where(t => t.Area == area).ToList();

        // Status
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Status_Header"), LOC.Get("MAP_MapInfo_Status_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_MapInfo_Status_Tiles", areaTiles.Count, area));

        var modified = Bank.Tiles.Values.Count(t => t.Modified);

        if (modified > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_ModifiedFilter_Text, LOC.Get("MAP_MapInfo_Status_Modified", modified));

        if (Bank.LoadErrors > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MapInfo_Status_Load_Errors", Bank.LoadErrors));

        var badSlots = areaTiles.Count(t => t.SlotErrors.Any(e => e != null));

        if (badSlots > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_MapInfo_Status_Bad_Slots", badSlots));

        // Layer
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Layer_Header"), LOC.Get("MAP_MapInfo_Layer_Header_TT"));

        foreach (var layer in MapInfoLayers.All)
        {
            if (ImGui.RadioButton($"{LOC.Get(MapInfoLayers.NameKey(layer))}##mapInfoLayer_{layer}", Layer == layer))
            {
                Layer = layer;
                SelectedValue = MapInfoLayers.None;
                LutVersion++;
            }
            GUI.Tooltip(LOC.Get($"{MapInfoLayers.NameKey(layer)}_TT", MapInfoLayers.ParamName(layer, dlc)));
        }

        // Tool
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Tool_Header"), LOC.Get("MAP_MapInfo_Tool_Header_TT"));

        if (ImGui.RadioButton($"{LOC.Get("MAP_MapInfo_Tool_Brush")}##mapInfoToolBrush", Tool == PaintTool.Brush))
            Tool = PaintTool.Brush;
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Tool_Brush_TT"));

        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("MAP_MapInfo_Tool_Fill")}##mapInfoToolFill", Tool == PaintTool.Fill))
            Tool = PaintTool.Fill;
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Tool_Fill_TT"));

        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("MAP_MapInfo_Tool_Pick")}##mapInfoToolPick", Tool == PaintTool.Pick))
            Tool = PaintTool.Pick;
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Tool_Pick_TT", InputManager.GetMouseHint(MousebindID.MapEditor_Map_Information_Pick)));

        if (Tool == PaintTool.Brush)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderInt("##mapInfoBrushSize", ref CFG.Current.MapInfo_BrushSize, 1, 64,
                LOC.Get("MAP_MapInfo_Brush_Size_Format", CFG.Current.MapInfo_BrushSize, CFG.Current.MapInfo_BrushSize * 4));
            GUI.Tooltip(LOC.Get("MAP_MapInfo_Brush_Size_TT"));
        }

        // Value
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Value_Header"), LOC.Get("MAP_MapInfo_Value_Header_TT", MapInfoLayers.ParamName(Layer, dlc)));

        DisplayPalette(dlc);

        // Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Display_Header"), LOC.Get("MAP_MapInfo_Display_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##mapInfoOpacity", ref CFG.Current.MapInfo_Opacity, 0.05f, 1.0f, LOC.Get("MAP_MapInfo_Opacity_Format")))
            LutVersion++;
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Opacity_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_MapInfo_Highlight_Selected")}##mapInfoHighlight", ref CFG.Current.MapInfo_HighlightSelected))
            LutVersion++;
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Highlight_Selected_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MapInfo_Show_Outlines")}##mapInfoOutlines", ref CFG.Current.MapInfo_ShowTileOutlines);
        GUI.Tooltip(LOC.Get("MAP_MapInfo_Show_Outlines_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MapInfo_Actions_Header"), LOC.Get("MAP_MapInfo_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("mapInfoActions",
            "mapInfoSave", LOC.Get("MAP_MapInfo_Save"), LOC.Get("MAP_MapInfo_Save_TT", InputManager.GetHint(KeybindID.Save)),
            Bank.SaveModified, modified > 0,

            "mapInfoUndo", LOC.Get("MAP_MapInfo_Undo"), LOC.Get("MAP_MapInfo_Undo_TT", InputManager.GetHint(KeybindID.Undo)),
            Bank.Undo, Bank.CanUndo,

            "mapInfoRedo", LOC.Get("MAP_MapInfo_Redo"), LOC.Get("MAP_MapInfo_Redo_TT", InputManager.GetHint(KeybindID.Redo)),
            Bank.Redo, Bank.CanRedo,

            "mapInfoReload", LOC.Get("MAP_MapInfo_Reload"), LOC.Get("MAP_MapInfo_Reload_TT"),
            Bank.Load, true);
    }

    private void DisplayPalette(bool dlc)
    {
        var palette = GetPalette(Layer, dlc);

        if (palette.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_MapInfo_No_Param", MapInfoLayers.ParamName(Layer, dlc)));
        }

        EditorFilters.DisplayFramedListFilter("mapInfoPaletteFilter", ref PaletteFilter, ref PaletteFilterExact);

        ImGui.BeginChild("##mapInfoPalette", new Vector2(-1, 220 * DPI.UIScale()), ImGuiChildFlags.Borders);

        // None first: it is the value most of a tile holds
        DisplayPaletteRow(MapInfoLayers.None, LOC.Get("MAP_MapInfo_Value_None_Name"), new Vector4(0, 0, 0, 0));

        foreach (var entry in palette)
        {
            if (entry.Id == MapInfoLayers.None)
                continue;

            if (!string.IsNullOrEmpty(PaletteFilter)
                && !EditorFilters.IsMatch(PaletteFilter, $"{entry.Id} {entry.Name}", PaletteFilterExact))
                continue;

            DisplayPaletteRow(entry.Id, entry.Name, entry.Color);
        }

        ImGui.EndChild();

        // Selected row
        var selected = FindEntry(Layer, dlc, SelectedValue);

        GUI.WrappedText(LOC.Get("MAP_MapInfo_Selected", Describe(Layer, dlc, SelectedValue)));

        if (selected != null)
        {
            foreach (var field in selected.Fields)
                GUI.WrappedTextColored(UI.Current.ImGui_ParamRow_Text, field);
        }
    }

    private void DisplayPaletteRow(byte id, string name, Vector4 color)
    {
        ImGui.ColorButton($"##mapInfoSwatch_{id}", color,
            ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.AlphaPreviewHalf, new Vector2(14, 14) * DPI.UIScale());
        ImGui.SameLine();

        if (ImGui.Selectable($"{id}  {name}##mapInfoPaletteRow_{id}", SelectedValue == id))
        {
            SelectedValue = id;
            LutVersion++;
        }
    }

    #endregion

    #region Canvas

    /// <summary>
    /// Converts a point on the world map image (before zoom) to a global pixel position of the area
    /// </summary>
    private static Vector2 ToGlobal(WorldMapGrid grid, Vector2 image)
    {
        return new Vector2(
            (image.X - grid.StartX) / grid.Size * MapInfoTexTile.Size + grid.FirstX * MapInfoTexTile.Size,
            (image.Y - grid.StartY) / grid.Size * MapInfoTexTile.Size - grid.FirstY * MapInfoTexTile.Size);
    }

    /// <summary>
    /// Draws the layer over the map and handles painting. imageMin is the screen position of the image's top
    /// left, zoom its scale, mouseImage the cursor on the image before zoom.
    /// </summary>
    public void DisplayCanvas(ImDrawListPtr drawList, Vector2 imageMin, Vector2 zoom, WorldMapLayout layout, string area,
        bool hovered, Vector2 mouseImage)
    {
        HoverValid = false;

        if (!IsSupported(Project) || !Bank.Loaded || area == null || layout == null
            || !layout.Grids.TryGetValue(WorldMapTileType.Large, out var grid))
        {
            EndPainting();
            return;
        }

        var dlc = area == "m61";

        // Layer textures and tile outlines
        foreach (var tile in Bank.Tiles.Values.Where(t => t.Area == area))
        {
            var origin = grid.TileOrigin(tile.X, tile.Y);
            var min = imageMin + origin * zoom;
            var max = imageMin + (origin + new Vector2(grid.Size)) * zoom;

            var handle = GetTexture(tile, dlc);

            if (handle is { Resident: true })
                drawList.AddImage(new ImTextureID(handle.TexHandle), min, max);

            if (CFG.Current.MapInfo_ShowTileOutlines)
            {
                var color = tile.Modified ? UI.Current.ImGui_ModifiedFilter_Text : new Vector4(1, 1, 1, 0.35f);
                drawList.AddRect(min, max, ImGui.GetColorU32(color), 0, ImDrawFlags.None, 1.0f);
            }
        }

        var global = ToGlobal(grid, mouseImage);

        if (hovered)
        {
            HoverValid = true;
            HoverGx = (int)MathF.Floor(global.X);
            HoverGy = (int)MathF.Floor(global.Y);

            // Brush outline
            if (Tool == PaintTool.Brush)
            {
                var radius = CFG.Current.MapInfo_BrushSize * 0.5f / MapInfoTexTile.Size * grid.Size * zoom.X;
                drawList.AddCircle(ImGui.GetMousePos(), MathF.Max(radius, 1.0f), ImGui.GetColorU32(new Vector4(1, 1, 1, 0.8f)));
            }
        }

        // Pick
        if (hovered && (InputManager.IsMousePressed(MousebindID.MapEditor_Map_Information_Pick)
            || (Tool == PaintTool.Pick && InputManager.IsMousePressed(MousebindID.MapEditor_Map_Information_Paint))))
        {
            if (Bank.TryGet(area, Layer, HoverGx, HoverGy, out var value, out _))
            {
                SelectedValue = value;
                LutVersion++;
            }

            return;
        }

        // Fill
        if (Tool == PaintTool.Fill && hovered && InputManager.IsMousePressed(MousebindID.MapEditor_Map_Information_Paint))
        {
            Bank.BeginStroke();
            Bank.Fill(area, Layer, HoverGx, HoverGy, SelectedValue);
            Bank.EndStroke();
            return;
        }

        // Brush
        if (Tool != PaintTool.Brush)
        {
            EndPainting();
            return;
        }

        if (!Painting && hovered && InputManager.IsMousePressed(MousebindID.MapEditor_Map_Information_Paint))
        {
            Painting = true;
            LastPaintPoint = global;
            Bank.BeginStroke();
        }

        if (Painting)
        {
            if (!InputManager.IsMouseDown(MousebindID.MapEditor_Map_Information_Paint))
            {
                EndPainting();
                return;
            }

            Bank.PaintLine(area, Layer, LastPaintPoint.X, LastPaintPoint.Y, global.X, global.Y,
                CFG.Current.MapInfo_BrushSize * 0.5f, SelectedValue);

            LastPaintPoint = global;
        }
    }

    /// <summary>Ends a stroke in progress, e.g. when the window is hidden mid-stroke</summary>
    public void CancelPainting() => EndPainting();

    private void EndPainting()
    {
        if (!Painting)
            return;

        Painting = false;
        Bank.EndStroke();
    }

    /// <summary>The cursor's tile, pixel and the value of each layer, shown while the map is hovered</summary>
    public void DisplayTooltip(string area)
    {
        if (!HoverValid || area == null)
            return;

        var (x, y, px, py) = MapInfoTexBank.Split(HoverGx, HoverGy);
        var tile = Bank.GetTile(area, x, y);
        var dlc = area == "m61";

        ImGui.BeginTooltip();

        if (tile == null)
        {
            ImGui.Text(LOC.Get("MAP_MapInfo_Tooltip_No_Tile", $"{area}_{x:00}_{y:00}_02"));
        }
        else
        {
            ImGui.Text(LOC.Get("MAP_MapInfo_Tooltip_Tile", tile.Name, px, py));

            foreach (var layer in MapInfoLayers.All)
            {
                var text = tile.HasLayer(layer)
                    ? Describe(layer, dlc, tile.Get(layer, px, py))
                    : LOC.Get(tile.SlotErrors[MapInfoLayers.Slot(layer)] ?? "MAP_MapInfo_Tooltip_No_Layer");

                var line = $"{LOC.Get(MapInfoLayers.NameKey(layer))}: {text}";

                if (layer == Layer)
                    ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, line);
                else
                    ImGui.Text(line);
            }
        }

        ImGui.EndTooltip();
    }

    /// <summary>Undo, redo and save while the window has focus; the map editor's own shortcuts are off then</summary>
    public void HandleShortcuts()
    {
        if (!Bank.Loaded || Painting || ImGui.IsAnyItemActive())
            return;

        if (InputManager.IsPressed(KeybindID.Undo) || InputManager.IsPressedOrRepeated(KeybindID.Undo_Repeat))
            Bank.Undo();

        if (InputManager.IsPressed(KeybindID.Redo) || InputManager.IsPressedOrRepeated(KeybindID.Redo_Repeat))
            Bank.Redo();

        if (InputManager.IsPressed(KeybindID.Save))
            Bank.SaveModified();
    }

    #endregion

    #region Textures

    private class TileTexture
    {
        public TexturePool.TextureHandle Handle;
        public MapInfoTexTile Tile;
        public int Version = -1;
        public int Lut = -1;
        public long LastUpload;
    }

    private readonly Dictionary<string, TileTexture> Textures = new();

    /// <summary>Bumped when the layer, palette, selection or display settings change, to rebuild the textures</summary>
    private int LutVersion;

    private const long UploadIntervalMs = 100;

    private TexturePool.TextureHandle GetTexture(MapInfoTexTile tile, bool dlc)
    {
        if (!Textures.TryGetValue(tile.Name, out var entry))
        {
            var handle = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

            if (handle == null)
                return null;

            entry = new TileTexture { Handle = handle };
            Textures[tile.Name] = entry;
        }

        var stale = !ReferenceEquals(entry.Tile, tile) || entry.Version != tile.Version || entry.Lut != LutVersion;

        // While painting, a tile refreshes at most every UploadIntervalMs; each upload replaces a texture
        if (stale && (!Painting || Stopwatch.GetElapsedTime(entry.LastUpload).TotalMilliseconds >= UploadIntervalMs))
        {
            entry.Tile = tile;
            entry.Version = tile.Version;
            entry.Lut = LutVersion;
            entry.LastUpload = Stopwatch.GetTimestamp();

            Upload(entry.Handle, BuildPixels(tile, dlc), tile.Name);
        }

        return entry.Handle;
    }

    private byte[] BuildPixels(MapInfoTexTile tile, bool dlc)
    {
        var lut = BuildLut(dlc);
        var rgba = new byte[MapInfoTexTile.Size * MapInfoTexTile.Size * 4];

        if (!tile.HasLayer(Layer))
            return rgba;

        for (var py = 0; py < MapInfoTexTile.Size; py++)
        {
            for (var px = 0; px < MapInfoTexTile.Size; px++)
            {
                var color = lut[tile.Get(Layer, px, py)];
                var o = (py * MapInfoTexTile.Size + px) * 4;
                rgba[o] = (byte)(color >> 24);
                rgba[o + 1] = (byte)(color >> 16);
                rgba[o + 2] = (byte)(color >> 8);
                rgba[o + 3] = (byte)color;
            }
        }

        return rgba;
    }

    /// <summary>
    /// The colour of each value as RGBA packed high to low: the row's srcR/G/B, clear for none, magenta for a value
    /// with no row. With "Highlight Selected" on, other values are faded.
    /// </summary>
    private uint[] BuildLut(bool dlc)
    {
        var lut = new uint[256];
        var alpha = (byte)Math.Clamp(CFG.Current.MapInfo_Opacity * 255f, 0, 255);
        var faded = (byte)(alpha / 5);

        for (var i = 0; i < 256; i++)
            lut[i] = Pack(255, 0, 255, alpha);

        foreach (var entry in GetPalette(Layer, dlc))
        {
            lut[entry.Id] = Pack((byte)(entry.Color.X * 255), (byte)(entry.Color.Y * 255), (byte)(entry.Color.Z * 255), alpha);
        }

        lut[MapInfoLayers.None] = 0;

        if (CFG.Current.MapInfo_HighlightSelected)
        {
            for (var i = 0; i < 256; i++)
            {
                if (i != SelectedValue && lut[i] != 0)
                    lut[i] = (lut[i] & 0xFFFFFF00) | faded;
            }

            // Selected "none" shows as white, so it can be seen
            if (SelectedValue == MapInfoLayers.None)
                lut[MapInfoLayers.None] = Pack(255, 255, 255, alpha);
        }

        return lut;
    }

    private static uint Pack(byte r, byte g, byte b, byte a) => (uint)(r << 24 | g << 16 | b << 8 | a);

    private static void Upload(TexturePool.TextureHandle handle, byte[] rgba, string name)
    {
        SceneRenderer.AddBackgroundUploadTask((gd, cl) =>
        {
            handle.FillWithGPUTexture(CreateTexture(gd, rgba, $"Map Information {name}"));
        });
    }

    private static unsafe Texture CreateTexture(GraphicsDevice gd, byte[] rgba, string name)
    {
        var description = TextureDescription.Texture2D(MapInfoTexTile.Size, MapInfoTexTile.Size, 1, 1,
            VkFormat.R8G8B8A8Unorm, VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
            VkImageCreateFlags.None, VkImageTiling.Optimal);

        var texture = gd.ResourceFactory.CreateTexture(ref description);
        texture.Name = name;

        fixed (byte* pointer = rgba)
        {
            gd.UpdateTexture(texture, (IntPtr)pointer, (uint)rgba.Length, 0, 0, 0,
                MapInfoTexTile.Size, MapInfoTexTile.Size, 1, 0, 0);
        }

        return texture;
    }

    #endregion
}
