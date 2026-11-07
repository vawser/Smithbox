using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The View tab (Navigation Editor Plan, Phase 1): an overlay of a map's navmesh set in the viewport, coloured by face
/// data, with its connections, user edges, gate nodes and ONAV, and a face inspector picked in the viewport.
/// </summary>
public partial class NavigationTool
{
    private readonly NavigationOverlayRenderer OverlayRenderer = new();
    private readonly NavigationOverlayRenderer HighlightRenderer = new();

    private bool ShowOverlay;
    private NavmeshSet OverlaySet = NavmeshSet.N;
    private float FaceOpacity = 45;
    private bool OverlayDirty;
    private bool ShowLegend = true;

    private readonly HashSet<NavOverlayLayer> Layers = new()
    {
        NavOverlayLayer.Faces,
        NavOverlayLayer.OpenEdges,
        NavOverlayLayer.ConnectedInMap,
        NavOverlayLayer.ConnectedCrossMap,
        NavOverlayLayer.UserEdges,
        NavOverlayLayer.BlockedUserEdges,
        NavOverlayLayer.GateNodes,
        NavOverlayLayer.OnavAreas,
        NavOverlayLayer.OnavLinks,
    };

    /// <summary>The map and data the overlay was built from.</summary>
    private string OverlayMap;
    private NavigationMap OverlayNavigation;
    private MapContainer OverlayContainer;
    private NavOverlay OverlayData;

    private bool PickMode;
    private NavFaceHit Picked;

    /// <summary>The layers the user can toggle, in the order they are listed. Highlight is the tool's own.</summary>
    private static readonly NavOverlayLayer[] ToggleLayers = Enum.GetValues<NavOverlayLayer>().Where(l => l != NavOverlayLayer.Highlight).ToArray();

    private void DisplayViewTab(MapContainer map)
    {
        GUI.WrappedText(LOC.Get("MAP_Navigation_View_Description"));

        // Overlay
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Overlay_Header"), LOC.Get("MAP_Navigation_Overlay_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Overlay_Show")}##navigationOverlayShow", ref ShowOverlay))
        {
            if (ShowOverlay)
                OverlayDirty = true;
            else
                ClearOverlay();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Show_TT"));

        // The overlay follows the map picked above
        if (ShowOverlay && OverlayMap != map.Name)
            OverlayDirty = true;

        int set = (int)OverlaySet;
        if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Overlay_Set_N")}##navigationSetN", ref set, (int)NavmeshSet.N))
            OverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Set_N_TT"));
        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Overlay_Set_O")}##navigationSetO", ref set, (int)NavmeshSet.O))
            OverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Set_O_TT"));
        OverlaySet = (NavmeshSet)set;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##navigationFaceOpacity", ref FaceOpacity, 5, 100, $"{LOC.Get("MAP_Navigation_Overlay_Opacity")} %.0f"))
            OverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Opacity_TT"));

        // Layers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Layers_Header"), LOC.Get("MAP_Navigation_Layers_Header_TT"));

        foreach (var layer in ToggleLayers)
        {
            bool on = Layers.Contains(layer);

            // The line layers show their colour, so the viewport can be read without the legend
            if (layer != NavOverlayLayer.Faces)
            {
                var c = NavigationOverlayRenderer.LayerColor(layer);
                ImGui.ColorButton($"##navigationLayerColor_{layer}", new Vector4(c, 1), ImGuiColorEditFlags.NoTooltip, new Vector2(14, 14) * DPI.UIScale());
                ImGui.SameLine();
            }

            if (ImGui.Checkbox($"{LOC.Get($"MAP_Navigation_Layer_{layer}")}##navigationLayer_{layer}", ref on))
            {
                if (on) Layers.Add(layer); else Layers.Remove(layer);
                OverlayDirty = true;
            }
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Layer_{layer}_TT"));
        }

        if (ShowOverlay && OverlayDirty)
            RebuildOverlay(map);

        if (ShowOverlay && OverlayData != null)
        {
            DisplayLegend();

            if (OverlayData.UserEdgesWithoutTarget > 0)
                GUI.WrappedText(LOC.Get("MAP_Navigation_Overlay_User_Edges_Without_Target", OverlayData.UserEdgesWithoutTarget));
        }

        // Pick
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Pick_Header"), LOC.Get("MAP_Navigation_Pick_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Pick_Mode")}##navigationPickMode", ref PickMode) && !PickMode)
            SetPicked(null);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Pick_Mode_TT"));

        if (PickMode && !ShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Pick_Needs_Overlay"));

        DisplayInspector();
    }

    private void DisplayLegend()
    {
        var legend = OverlayRenderer.FaceLegend;

        if (!Layers.Contains(NavOverlayLayer.Faces) || legend.Count == 0)
            return;

        // Face Data
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Legend_Header"), LOC.Get("MAP_Navigation_Legend_Header_TT"), ref ShowLegend);

        if (!ShowLegend)
            return;

        foreach (var (value, color, faces) in legend)
        {
            ImGui.ColorButton($"##navigationLegend_{value:X8}", new Vector4(color, 1), ImGuiColorEditFlags.NoTooltip, new Vector2(14, 14) * DPI.UIScale());
            ImGui.SameLine();
            ImGui.Text(LOC.Get("MAP_Navigation_Legend_Entry", value.ToString("X8"), DescribeBits(value), faces));
        }
    }

    /// <summary>The set bits of a face data value, e.g. "0x1000 | 0x1000000", as their meanings are unknown.</summary>
    private static string DescribeBits(uint value)
    {
        if (value == 0)
            return LOC.Get("MAP_Navigation_Legend_No_Flags");

        var bits = new List<string>();
        for (int i = 0; i < 32; i++)
            if ((value & (1u << i)) != 0)
                bits.Add($"0x{1u << i:X}");
        return string.Join(" | ", bits);
    }

    private void RebuildOverlay(MapContainer map)
    {
        OverlayDirty = false;
        SetPicked(null);

        var navigation = Bank.GetMap(map);
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (navigation == null || scene == null)
        {
            ClearOverlay();
            return;
        }

        OverlayMap = map.Name;
        OverlayContainer = map;
        OverlayNavigation = navigation;
        OverlayData = NavigationOverlayBuilder.Build(navigation, OverlaySet, e => EntryWorld(map, e), MapWorld(map), Layers);
        OverlayRenderer.Show(scene, OverlayData, FaceOpacity);
    }

    private void ClearOverlay()
    {
        OverlayRenderer.Clear();
        HighlightRenderer.Clear();
        OverlayMap = null;
        OverlayContainer = null;
        OverlayNavigation = null;
        OverlayData = null;
        Picked = null;
    }

    /// <summary>Where a navmesh entry is drawn: its entity's place in the viewport, as the Map Editor draws the navmesh.</summary>
    private static Matrix4x4 EntryWorld(MapContainer map, NVA_ER.Navmesh entry)
    {
        var entity = map.Objects.FirstOrDefault(o => ReferenceEquals(o.WrappedObject, entry));
        return entity?.GetWorldMatrix() ?? MapWorld(map);
    }

    /// <summary>Where the map's own navigation data (gate nodes, ONAV) is drawn.</summary>
    private static Matrix4x4 MapWorld(MapContainer map) => map.NavmeshParent?.GetWorldMatrix() ?? Matrix4x4.Identity;

    private void SetPicked(NavFaceHit hit)
    {
        Picked = hit;
        HighlightRenderer.Clear();

        if (hit == null || OverlayNavigation == null || OverlayContainer == null)
            return;

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;
        var outline = NavigationOverlayBuilder.Highlight(OverlayNavigation, OverlaySet, hit, EntryWorld(OverlayContainer, hit.Entry));
        HighlightRenderer.Show(scene, outline, FaceOpacity);
    }

    /// <summary>
    /// Picks a face on a click while picking is on. Called each frame by the active viewport; returns true while the
    /// viewport's clicks are taken, so they don't select. Also drops the overlay of a map that was unloaded.
    /// </summary>
    public bool HandleViewportInput(Ray ray, bool canInteract)
    {
        if (!CanUse())
            return false;

        if (OverlayContainer != null && OverlayContainer.LoadState != MapContentLoadState.Loaded)
        {
            ClearOverlay();
            ShowOverlay = false;
        }

        if (!PickMode || OverlayNavigation == null)
            return false;

        if (canInteract && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            var hit = NavigationOverlayBuilder.Pick(OverlayNavigation, OverlaySet, ray.Origin, ray.Direction,
                e => EntryWorld(OverlayContainer, e));
            SetPicked(hit);
        }

        return true;
    }

    private void DisplayInspector()
    {
        // Face
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Face_Header"), LOC.Get("MAP_Navigation_Face_Header_TT"));

        if (Picked == null || OverlayNavigation == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_None"));
            return;
        }

        var hit = Picked;
        var mesh = hit.File.Mesh;
        var face = mesh.m_faces[hit.Face];
        var entry = hit.Entry;

        GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Navmesh", (uint)entry.NameID, entry.ModelID, hit.File.Name));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Index", hit.Face, mesh.m_faces.Count, face.m_numEdges, face.m_startEdgeIndex));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Point", $"{hit.Point.X:0.##}, {hit.Point.Y:0.##}, {hit.Point.Z:0.##}"));

        if (mesh.m_faceDataStriding > 0 && mesh.m_faceData.Count > hit.Face * mesh.m_faceDataStriding)
        {
            uint data = (uint)mesh.m_faceData[hit.Face * mesh.m_faceDataStriding];
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Data", data.ToString("X8"), DescribeBits(data)));
        }

        // Its edges, and what each leads to
        for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
            GUI.WrappedText(DescribeEdge(mesh, entry, k, false));

        for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
            GUI.WrappedText(DescribeEdge(mesh, entry, k, true));
    }

    private string DescribeEdge(Havok.hk2018.hkaiNavMesh mesh, NVA_ER.Navmesh entry, int k, bool user)
    {
        var edge = mesh.m_edges[k];
        uint data = mesh.m_edgeDataStriding > 0 && mesh.m_edgeData.Count > k * mesh.m_edgeDataStriding
            ? (uint)mesh.m_edgeData[k * mesh.m_edgeDataStriding] : 0;

        string target;
        if (NavigationOverlayBuilder.OppositeFace(mesh, edge.m_oppositeFace) is int other)
        {
            target = LOC.Get("MAP_Navigation_Edge_To_Face", other);
        }
        else
        {
            // A boundary edge: what connection, if any, joins it to another navmesh
            var connection = NavigationRemap.Connections(OverlayNavigation).FirstOrDefault(c =>
                (c.MainNameID == entry.NameID && c.Connection.EdgeIndex == k && (!c.IsCrossMap || c.Nvc.MapA == OverlayNavigation.MapId))
                || (c.TargetNameID == entry.NameID && c.Connection.OppositeEdgeIndex == k && (!c.IsCrossMap || c.Nvc.MapB == OverlayNavigation.MapId)));

            if (connection.Connection == null)
            {
                target = LOC.Get("MAP_Navigation_Edge_Open");
            }
            else
            {
                bool fromA = connection.MainNameID == entry.NameID && connection.Connection.EdgeIndex == k;
                uint otherId = (uint)(fromA ? connection.TargetNameID : connection.MainNameID);
                int otherFace = fromA ? connection.Connection.OppositeFaceIndex : connection.Connection.FaceIndex;

                target = connection.IsCrossMap
                    ? LOC.Get("MAP_Navigation_Edge_To_Nvc", otherId, otherFace, connection.Nvc.Opposite(OverlayNavigation.MapId))
                    : LOC.Get("MAP_Navigation_Edge_To_Navmesh", otherId, otherFace);
            }
        }

        return LOC.Get(user ? "MAP_Navigation_Edge_User" : "MAP_Navigation_Edge", k, (int)edge.m_flags, data.ToString("X"), target);
    }
}
