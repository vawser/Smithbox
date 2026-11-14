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
/// where the game won't save the player's position (SaveLimit) or limits multiplayer (MultiplayLimit). The boxes
/// aren't MSB entries, so they can't be picked or edited here; this is display only.
/// See Claude/Documentation/MFR Format.md.
/// </summary>
public class MapForbiddenRegionTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private RenderScene Scene;

    private class MapBoxes
    {
        public MapContainer Map;
        public List<(MFR.EntryType Type, Matrix4x4 Local, DebugPrimitiveRenderableProxy Proxy)> Boxes = new();

        // What the boxes were last placed and coloured with: maps hold over a thousand boxes, so they are only
        // touched when one of these changes
        public Matrix4x4? Root;
        public (bool, bool, Color, Color) State;
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

    private bool IsEnabled => IsSupported && CFG.Current.MapEditor_Display_Forbidden_Regions;

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

        foreach (var wrapper in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            var map = wrapper.MapContainer;

            if (map == null || map.LoadState is not MapContentLoadState.Loaded || map.RootObject == null)
                continue;

            // A map is drawn in the scene of the view that loaded it
            if (map.OwnerView != null && map.OwnerView != View)
                continue;

            seen.Add(map.Name);

            if (!Maps.TryGetValue(map.Name, out var boxes) || boxes.Map != map)
            {
                boxes?.Boxes.ForEach(b => b.Proxy.Dispose());
                boxes = Load(map);
                Maps[map.Name] = boxes;
            }

            // The map's root carries the offset that places an overworld tile beside the others; the boxes are in
            // map space, like the MSB
            var root = map.RootObject.GetLocalTransform().WorldMatrix;
            var state = (CFG.Current.MapEditor_Forbidden_Region_Show_Save_Limit,
                CFG.Current.MapEditor_Forbidden_Region_Show_Multiplay_Limit,
                GetColor(MFR.EntryType.SaveLimit), GetColor(MFR.EntryType.MultiplayLimit));

            if (boxes.Root == root && boxes.State == state)
                continue;

            boxes.Root = root;
            boxes.State = state;

            foreach (var (type, local, proxy) in boxes.Boxes)
            {
                var show = type switch
                {
                    MFR.EntryType.SaveLimit => state.Item1,
                    MFR.EntryType.MultiplayLimit => state.Item2,
                    _ => true
                };

                proxy.Visible = show;

                if (!show)
                    continue;

                proxy.World = local * root;
                SetColor(proxy, GetColor(type));
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

    /// <summary>
    /// Reads the map's .mfr (project copy first, then the game's) and makes a wireframe box for each entry.
    /// A map without one (or one that fails to read) just gets no boxes.
    /// </summary>
    private MapBoxes Load(MapContainer map)
    {
        var result = new MapBoxes { Map = map };

        if (map.Name.Length < 3)
            return result;

        var path = $"/map/{map.Name[..3]}/{map.Name}/{map.Name}.mfr.dcx";
        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        try
        {
            if (!fs.FileExists(path))
                return result;

            var data = fs.ReadFile(path);

            if (data == null)
                return result;

            var mfr = MFR.Read(data.Value);

            foreach (var entry in mfr.Entries)
            {
                // The box region primitive spans -0.5..0.5 in X and Z and 0..1 in Y: size it to the box, centre it
                // on Y, then place it with the box's axes and centre
                var h = entry.HalfExtents;
                var local = Matrix4x4.CreateScale(h * 2)
                    * Matrix4x4.CreateTranslation(0, -h.Y, 0)
                    * entry.Transform;

                var proxy = RenderableHelper.GetBoxRegionProxy(Scene);
                proxy.Pickable = false;
                SetColor(proxy, GetColor(entry.Type));

                result.Boxes.Add((entry.Type, local, proxy));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError<MapForbiddenRegionTool>(LOC.Get("MAP_Forbidden_Region_Read_Failed", path), e);
        }

        return result;
    }

    private static void SetColor(DebugPrimitiveRenderableProxy proxy, Color color)
    {
        if (proxy.BaseColor != color)
        {
            proxy.BaseColor = color;
            proxy.HighlightedColor = color;
        }
    }

    private static Color GetColor(MFR.EntryType type)
    {
        var c = type == MFR.EntryType.MultiplayLimit
            ? CFG.Current.MapEditor_Forbidden_Region_Multiplay_Limit_Color
            : CFG.Current.MapEditor_Forbidden_Region_Save_Limit_Color;

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
            ImGui.TextUnformatted(LOC.Get("MAP_Forbidden_Region_Options_Header"));
            ImGui.Separator();

            ImGui.Checkbox($"##showSave", ref CFG.Current.MapEditor_Forbidden_Region_Show_Save_Limit);
            ImGui.SameLine();
            ColorEdit("MAP_Forbidden_Region_Save_Limit", ref CFG.Current.MapEditor_Forbidden_Region_Save_Limit_Color);

            ImGui.Checkbox($"##showMultiplay", ref CFG.Current.MapEditor_Forbidden_Region_Show_Multiplay_Limit);
            ImGui.SameLine();
            ColorEdit("MAP_Forbidden_Region_Multiplay_Limit", ref CFG.Current.MapEditor_Forbidden_Region_Multiplay_Limit_Color);

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
