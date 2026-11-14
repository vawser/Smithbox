using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Draws the boxes of each loaded map's .mfr ("Multi Forbidden Region") file in the viewport (ER only): the places
/// where the game won't save the player's position (SaveLimit) or limits multiplayer (MultiplayLimit). The boxes come
/// from <see cref="MfrBank"/>, so the Forbidden Region Editor's edits show straight away; the editor's selected and
/// hovered boxes are drawn in their own colours. The boxes aren't MSB entries and aren't picked by the viewport's
/// selection. See Claude/Documentation/MFR Format.md.
/// </summary>
public class MapForbiddenRegionTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private RenderScene Scene;

    private class MapBoxes
    {
        public MapContainer Map;
        public int Version = -1;
        public List<(MFR.EntryType Type, Matrix4x4 Local, DebugPrimitiveRenderableProxy Proxy)> Boxes = new();

        // What the boxes were last placed and coloured with: maps hold over a thousand boxes, so they are only
        // touched when one of these changes
        public Matrix4x4? Root;
        public (bool, bool, Color, Color, int, int) State;
    }

    private readonly Dictionary<string, MapBoxes> Maps = new();

    public MapForbiddenRegionTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Only Elden Ring has .mfr files. The toggle is hidden for the other games.
    /// </summary>
    public bool IsSupported => Project.Descriptor.ProjectType is ProjectType.ER;

    // Drawn while the toggle is on, and while the editor edits in the viewport (for its selected box)
    private bool IsEnabled => IsSupported &&
        (CFG.Current.MapEditor_Display_Forbidden_Regions || View.MfrEditorTool.IsEditing);

    #region Viewport

    /// <summary>
    /// Keeps the boxes of each loaded map in the viewport's scene. Called each frame by the active viewport.
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

        var seen = new HashSet<string>();

        foreach (var map in View.MfrEditorTool.GetDrawnMaps())
        {
            seen.Add(map.Name);

            if (!Maps.TryGetValue(map.Name, out var boxes) || boxes.Map != map)
            {
                boxes?.Boxes.ForEach(b => b.Proxy.Dispose());
                boxes = new MapBoxes { Map = map };
                Maps[map.Name] = boxes;
            }

            // A new or changed document: one proxy per entry, reused where there already is one
            var version = View.MfrBank.GetVersion(map);

            if (version != boxes.Version)
            {
                Sync(boxes, View.MfrBank.Get(map));
                boxes.Version = version;
                boxes.Root = null;
            }

            // The map's root carries the offset that places an overworld tile beside the others; the boxes are in
            // map space, like the MSB
            var root = map.RootObject.GetLocalTransform().WorldMatrix;
            View.MfrEditorTool.GetHighlight(map, out var selected, out var hovered);

            // Show Boxes governs every box; while editing with it off, only the selected box is drawn
            var showAll = CFG.Current.MapEditor_Display_Forbidden_Regions;
            var state = (showAll && CFG.Current.MapEditor_Forbidden_Region_Show_Save_Limit,
                showAll && CFG.Current.MapEditor_Forbidden_Region_Show_Multiplay_Limit,
                GetColor(MFR.EntryType.SaveLimit), GetColor(MFR.EntryType.MultiplayLimit),
                selected, hovered);

            if (boxes.Root == root && boxes.State == state)
                continue;

            boxes.Root = root;
            boxes.State = state;

            for (var i = 0; i < boxes.Boxes.Count; i++)
            {
                var (type, local, proxy) = boxes.Boxes[i];

                var show = i == selected || type switch
                {
                    MFR.EntryType.SaveLimit => state.Item1,
                    MFR.EntryType.MultiplayLimit => state.Item2,
                    _ => true
                };

                proxy.Visible = show;

                if (!show)
                    continue;

                proxy.World = local * root;

                // Selected over hovered, both drawn over the geometry
                var color = i == selected ? ToColor(CFG.Current.MapEditor_Forbidden_Region_Selected_Color)
                    : i == hovered ? ToColor(CFG.Current.MapEditor_Forbidden_Region_Hovered_Color)
                    : GetColor(type);

                SetColor(proxy, color);
                proxy.RenderOverlay = i == selected || i == hovered;
            }
        }

        // Maps since unloaded
        if (seen.Count != Maps.Count)
        {
            foreach (var name in new List<string>(Maps.Keys))
            {
                if (seen.Contains(name))
                    continue;

                Maps[name].Boxes.ForEach(b => b.Proxy.Dispose());
                Maps.Remove(name);
            }
        }
    }

    /// <summary>One wireframe box per entry of the document (none for a map without one).</summary>
    private void Sync(MapBoxes boxes, MFR document)
    {
        var count = document?.Entries.Count ?? 0;

        while (boxes.Boxes.Count > count)
        {
            boxes.Boxes[^1].Proxy.Dispose();
            boxes.Boxes.RemoveAt(boxes.Boxes.Count - 1);
        }

        for (var i = 0; i < count; i++)
        {
            var entry = document.Entries[i];
            var local = MfrBoxMath.GetDrawMatrix(entry);

            if (i < boxes.Boxes.Count)
            {
                boxes.Boxes[i] = (entry.Type, local, boxes.Boxes[i].Proxy);
                continue;
            }

            var proxy = RenderableHelper.GetBoxRegionProxy(Scene);
            proxy.Pickable = false;
            SetColor(proxy, GetColor(entry.Type));

            boxes.Boxes.Add((entry.Type, local, proxy));
        }
    }

    private static void SetColor(DebugPrimitiveRenderableProxy proxy, Color color)
    {
        if (proxy.BaseColor != color)
        {
            proxy.BaseColor = color;
            proxy.HighlightedColor = color;
        }
    }

    public static Color GetColor(MFR.EntryType type)
    {
        return ToColor(type == MFR.EntryType.MultiplayLimit
            ? CFG.Current.MapEditor_Forbidden_Region_Multiplay_Limit_Color
            : CFG.Current.MapEditor_Forbidden_Region_Save_Limit_Color);
    }

    private static Color ToColor(Vector3 c)
    {
        return Color.FromArgb(255, (int)Math.Clamp(c.X, 0, 255), (int)Math.Clamp(c.Y, 0, 255), (int)Math.Clamp(c.Z, 0, 255));
    }

    private void Release()
    {
        foreach (var boxes in Maps.Values)
            boxes.Boxes.ForEach(b => b.Proxy.Dispose());

        Maps.Clear();
        Scene = null;
    }

    #endregion

    #region Map List Header

    /// <summary>
    /// The toggle in the Map List header; right-click it for which types to show and their colours. ER only.
    /// </summary>
    public void DisplayHeaderButton()
    {
        if (!IsSupported)
            return;

        ImGui.SameLine();

        var enabled = CFG.Current.MapEditor_Display_Forbidden_Regions;

        if (enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);

        if (ImGui.Button($"{Icons.Ban}##forbiddenRegionToggle", DPI.IconButtonSize))
        {
            CFG.Current.MapEditor_Display_Forbidden_Regions = !enabled;
        }

        if (enabled)
            ImGui.PopStyleColor();

        GUI.Tooltip(LOC.Get(enabled ? "MAP_Forbidden_Region_Hide_TT" : "MAP_Forbidden_Region_Show_TT"));

        if (ImGui.BeginPopupContextItem("forbiddenRegionOptions"))
        {
            DisplayOptions();
            ImGui.EndPopup();
        }
    }

    /// <summary>Which types are drawn and the colours: in the toggle's menu and the editor's Display group.</summary>
    public static void DisplayOptions()
    {
        ImGui.TextUnformatted(LOC.Get("MAP_Forbidden_Region_Options_Header"));
        ImGui.Separator();

        ImGui.Checkbox($"##showSave", ref CFG.Current.MapEditor_Forbidden_Region_Show_Save_Limit);
        ImGui.SameLine();
        ColorEdit("MAP_Forbidden_Region_Save_Limit", ref CFG.Current.MapEditor_Forbidden_Region_Save_Limit_Color);

        ImGui.Checkbox($"##showMultiplay", ref CFG.Current.MapEditor_Forbidden_Region_Show_Multiplay_Limit);
        ImGui.SameLine();
        ColorEdit("MAP_Forbidden_Region_Multiplay_Limit", ref CFG.Current.MapEditor_Forbidden_Region_Multiplay_Limit_Color);

        ColorEdit("MAP_Forbidden_Region_Selected", ref CFG.Current.MapEditor_Forbidden_Region_Selected_Color);
        ColorEdit("MAP_Forbidden_Region_Hovered", ref CFG.Current.MapEditor_Forbidden_Region_Hovered_Color);
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
