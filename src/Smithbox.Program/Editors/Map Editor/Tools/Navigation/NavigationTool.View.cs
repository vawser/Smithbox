using Havok.Shared;
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
    private readonly NavigationOverlayRenderer BlockerRenderer = new();

    /// <summary>Set when the blocker tags or the overlay change: the red blocker fill is drawn again.</summary>
    private bool BlockerOverlayDirty;

    private bool ShowOverlay;
    private NavmeshSet OverlaySet = NavmeshSet.N;
    private float FaceOpacity = 45;
    private bool OverlayDirty;
    private bool ShowLegend = true;

    /// <summary>Draw only the model selected in the Models tab, to tell it from the others.</summary>
    private bool OverlayOnlySelectedModel;

    private readonly HashSet<NavOverlayLayer> Layers = new()
    {
        NavOverlayLayer.Faces,
        NavOverlayLayer.OpenEdges,
        NavOverlayLayer.ConnectedInMap,
        NavOverlayLayer.ConnectedCrossMap,
        NavOverlayLayer.UserEdges,
        NavOverlayLayer.BlockedUserEdges,
        NavOverlayLayer.GateNodes,
        NavOverlayLayer.GateNodesUnpaired,
        NavOverlayLayer.GateNodesBroken,
        NavOverlayLayer.GateCosts,
        NavOverlayLayer.OnavAreas,
        NavOverlayLayer.OnavLinks,
    };

    /// <summary>The map and data the overlay was built from.</summary>
    private string OverlayMap;
    private NavigationMap OverlayNavigation;
    private MapContainer OverlayContainer;
    private NavOverlay OverlayData;

    private bool PickMode;
    private bool ShowFaceEdges = true;
    private NavFaceHit Picked;

    /// <summary>How painting changes a face's data.</summary>
    private enum PaintOp { Set, AddBits, RemoveBits }

    private bool PaintMode;
    private PaintOp Paint = PaintOp.Set;
    private uint PaintValue;
    private string PaintValueText = "00000000";

    /// <summary>The faces changed in the stroke being painted, already applied; recorded as one action on release.</summary>
    private readonly Dictionary<(NavmeshFile File, int Face), NavFaceDataChange> Stroke = new();
    private readonly List<NavFaceDataChange> StrokeOrder = new();

    /// <summary>The picked face's data and its user edges' data, as typed.</summary>
    private NavFaceHit TextFor;
    private string FaceDataText = "";
    private readonly Dictionary<int, string> EdgeDataText = new();

    /// <summary>The layers the user can toggle, in the order they are listed. Highlight is the tool's own.</summary>
    private static readonly NavOverlayLayer[] ToggleLayers = Enum.GetValues<NavOverlayLayer>()
        .Where(l => l is not (NavOverlayLayer.Highlight or NavOverlayLayer.Blockers or NavOverlayLayer.GateSelected)).ToArray();

    /// <summary>
    /// Keeps the overlay on the map picked above, and draws it again after a change. Called before the tabs, so it works
    /// whichever tab is open.
    /// </summary>
    private void UpdateOverlay(MapContainer map)
    {
        if (ShowOverlay && OverlayMap != map.Name)
            OverlayDirty = true;

        if (ShowOverlay && OverlayDirty)
            RebuildOverlay(map);
    }

    private void DisplayOverlayTab()
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

        // The set toggle only where the game has an o set (not AC6)
        if (Bank.Profile.HasOSet)
        {
            int set = (int)OverlaySet;
            if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Overlay_Set_N")}##navigationSetN", ref set, (int)NavmeshSet.N))
                OverlayDirty = true;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Set_N_TT"));
            ImGui.SameLine();
            if (ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Overlay_Set_O")}##navigationSetO", ref set, (int)NavmeshSet.O))
                OverlayDirty = true;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Set_O_TT"));
            OverlaySet = (NavmeshSet)set;
        }
        else if (OverlaySet != NavmeshSet.N)
        {
            OverlaySet = NavmeshSet.N;
            OverlayDirty = true;
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##navigationFaceOpacity", ref FaceOpacity, 5, 100, $"{LOC.Get("MAP_Navigation_Overlay_Opacity")} %.0f"))
            OverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Opacity_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Overlay_Only_Selected")}##navigationOverlayOnlySelected", ref OverlayOnlySelectedModel))
            OverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Only_Selected_TT"));

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

        if (ShowOverlay && OverlayData?.UserEdgesWithoutTarget > 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Overlay_User_Edges_Without_Target", OverlayData.UserEdgesWithoutTarget));
    }

    /// <summary>The Face Data tab: picking, the picked face, painting, and the legend of the drawn values.</summary>
    private void DisplayFaceDataTab()
    {
        GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Data_Description"));

        if (!ShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Pick_Needs_Overlay"));

        DisplayPick();
        DisplayInspector();
        DisplayPaint();

        if (ShowOverlay && OverlayData != null)
            DisplayLegend();
    }

    /// <summary>The Links tab: adding links, and the picked face's links.</summary>
    private void DisplayLinksTab()
    {
        GUI.WrappedText(LOC.Get("MAP_Navigation_Links_Description"));

        if (!ShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Pick_Needs_Overlay"));

        DisplayPick();
        DisplayLinks();
        DisplayPickedLinks();
    }

    /// <summary>The Stitch tab.</summary>
    private void DisplayStitchTab()
    {
        DisplayStitch();
    }

    private void DisplayPick()
    {
        // Pick
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Pick_Header"), LOC.Get("MAP_Navigation_Pick_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Pick_Mode")}##navigationPickMode", ref PickMode))
        {
            if (PickMode)
            {
                PaintMode = false;
                LinkMode = false;
                StopGateMode();
                StopVertexEdit();
            }
            else
                SetPicked(null);
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Pick_Mode_TT"));
    }

    private void DisplayPaint()
    {
        // Paint
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Paint_Header"), LOC.Get("MAP_Navigation_Paint_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Paint_Mode")}##navigationPaintMode", ref PaintMode) && PaintMode)
        {
            PickMode = false;
            LinkMode = false;
            StopGateMode();
            StopVertexEdit();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Paint_Mode_TT"));

        int op = (int)Paint;
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Paint_Set")}##navigationPaintSet", ref op, (int)PaintOp.Set);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Paint_Set_TT"));
        ImGui.SameLine();
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Paint_Add")}##navigationPaintAdd", ref op, (int)PaintOp.AddBits);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Paint_Add_TT"));
        ImGui.SameLine();
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Paint_Remove")}##navigationPaintRemove", ref op, (int)PaintOp.RemoveBits);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Paint_Remove_TT"));
        Paint = (PaintOp)op;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##navigationPaintValue", ref PaintValueText, 16);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Paint_Value_TT"));
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (TryParseHex(PaintValueText, out var parsed))
                PaintValue = parsed;
            PaintValueText = PaintValue.ToString("X8");
        }

        // Every bit the game's code reads, by name, and any other bit the drawn faces use, to build the value from
        var bits = OverlayRenderer.FaceLegend.Aggregate(0u, (a, l) => a | l.Value) | NavFaceDataBits.Face.Keys.Aggregate(0u, (a, b) => a | b);
        for (int i = 0; i < 32; i++)
        {
            uint bit = 1u << i;
            if ((bits & bit) == 0)
                continue;

            bool on = (PaintValue & bit) != 0;
            var (label, tooltip) = FaceBitText(bit);
            if (ImGui.Checkbox($"{label}##navigationPaintBit_{i}", ref on))
            {
                PaintValue = on ? PaintValue | bit : PaintValue & ~bit;
                PaintValueText = PaintValue.ToString("X8");
            }
            GUI.Tooltip(tooltip);
        }
    }

    private static bool TryParseHex(string text, out uint value)
    {
        text = text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            text = text.Substring(2);
        return uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out value);
    }

    private uint Painted(uint before) => Paint switch
    {
        PaintOp.AddBits => before | PaintValue,
        PaintOp.RemoveBits => before & ~PaintValue,
        _ => PaintValue,
    };

    /// <summary>Paints the face under the cursor, once a stroke.</summary>
    private void PaintAt(NavFaceHit hit)
    {
        if (hit == null || Stroke.ContainsKey((hit.File, hit.Face)))
            return;

        var mesh = hit.File.Mesh;
        int index = hit.Face * mesh.m_faceDataStriding;

        if (mesh.m_faceDataStriding <= 0 || index >= mesh.m_faceData.Count)
            return;

        int before = mesh.m_faceData[index];
        int after = (int)Painted((uint)before);

        // Each face is only recorded once a stroke, so dragging back over it doesn't record it again
        var change = new NavFaceDataChange(hit.File, hit.Face, before, after);
        Stroke[(hit.File, hit.Face)] = change;

        if (before == after)
            return;

        NavigationEditAction.ApplyFace(change, true);
        StrokeOrder.Add(change);
        SetPicked(hit);
    }

    private void EndStroke()
    {
        Stroke.Clear();

        if (StrokeOrder.Count == 0 || OverlayContainer == null)
            return;

        var action = new NavigationEditAction(OverlayContainer, new List<NavFaceDataChange>(StrokeOrder), null, OnNavigationEdited);
        StrokeOrder.Clear();

        // Already applied, but executing it again marks the map as modified and leaves it on the undo stack
        View.ViewportActionManager.ExecuteAction(action);
    }

    /// <summary>After an edit, or its undo: the overlay is drawn again, and the inspector reads the values again.</summary>
    private void OnNavigationEdited()
    {
        OverlayDirty = true;
        TextFor = null;
        StitchPlan = null;
        ModelsNavigation = null;
        GateOverlayDirty = true;
        GateCostTextFor = null;
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
            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Legend_Use")}##navigationLegendUse_{value:X8}"))
            {
                PaintValue = value;
                PaintValueText = value.ToString("X8");
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Legend_Use_TT"));
            ImGui.SameLine();
            ImGui.Text(LOC.Get("MAP_Navigation_Legend_Entry", value.ToString("X8"), DescribeBits(value), faces));
        }
    }

    /// <summary>The set bits of a face data value, e.g. "0x1000 | 0x1000000", as their meanings are unknown.</summary>
    private static string DescribeBits(uint value)
    {
        if (value == 0)
            return LOC.Get("MAP_Navigation_Legend_No_Flags");

        return DescribeBits(value, NavFaceDataBits.Face);
    }

    /// <summary>The set bits of a value with the names of those the game's code reads, e.g. "0x1 Impassable | 0x40".</summary>
    private static string DescribeBits(uint value, IReadOnlyDictionary<uint, string> names)
    {
        var bits = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            uint bit = 1u << i;
            if ((value & bit) != 0)
                bits.Add(names.TryGetValue(bit, out var name) ? $"0x{bit:X} {LOC.Get($"MAP_Navigation_Bit_{name}")}" : $"0x{bit:X}");
        }
        return string.Join(" | ", bits);
    }

    /// <summary>The name and description of a face bit, for its paint checkbox.</summary>
    private static (string Label, string Tooltip) FaceBitText(uint bit)
    {
        if (NavFaceDataBits.Face.TryGetValue(bit, out var name))
            return ($"0x{bit:X} {LOC.Get($"MAP_Navigation_Bit_{name}")}", LOC.Get($"MAP_Navigation_Bit_{name}_TT"));
        return ($"0x{bit:X} {LOC.Get("MAP_Navigation_Bit_Unknown")}", LOC.Get("MAP_Navigation_Paint_Bit_TT"));
    }

    private void RebuildOverlay(MapContainer map)
    {
        OverlayDirty = false;

        // The picked face stays picked when the same map and set are drawn again, e.g. after an edit
        var keep = OverlayMap == map.Name && OverlayNavigation != null ? Picked : null;
        var keepSet = OverlaySet;
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
        int onlyModel = SelectedModelID;
        bool onlySelected = OverlayOnlySelectedModel && onlyModel >= 0;

        // Navmeshes with vertex edits are drawn by their previews instead (Edit tab), once those have been drawn
        var without = map == VertexMap
            ? new HashSet<string>(DrawnVertexPreviews, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        OverlayWithout = without;

        OverlayData = NavigationOverlayBuilder.Build(navigation, OverlaySet, e => EntryWorld(map, e), MapWorld(map), Layers,
            onlySelected || without.Count > 0 ? e => (!onlySelected || e.ModelID == onlyModel) && !without.Contains(navigation.FileNameOf(e)) : null);
        OverlayRenderer.Show(scene, OverlayData, FaceOpacity);
        OverlayBuildCount++;
        BlockerOverlayDirty = true;
        GateOverlayDirty = true;

        if (keep != null && keepSet == OverlaySet && navigation.Files.Values.Contains(keep.File) && NavigationMap.SetOf(keep.Entry) == OverlaySet)
            SetPicked(keep);
    }

    private void ClearOverlay()
    {
        OverlayRenderer.Clear();
        BlockerRenderer.Clear();
        HighlightRenderer.Clear();
        GateRenderer.Clear();
        GateDragging = false;
        GateDragBefore = null;
        OverlayMap = null;
        OverlayContainer = null;
        OverlayNavigation = null;
        OverlayData = null;
        Picked = null;
    }

    /// <summary>Where a navmesh entry is drawn: its entity's place in the viewport, as the Map Editor draws the navmesh.</summary>
    private static Matrix4x4 EntryWorld(MapContainer map, NVA.Navmesh entry)
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
            Stroke.Clear();
            StrokeOrder.Clear();
            ClearOverlay();
            ShowOverlay = false;
        }

        DrainGenerateResults();
        UpdateNavmeshDrawing();

        // A redrawn overlay replaces the old one once it has been drawn
        OverlayRenderer.Update();
        BlockerRenderer.Update();
        HighlightRenderer.Update();
        GateRenderer.Update();

        bool mouseDown = InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action);

        // A finished stroke is recorded, and an edit or undo redraws the overlay, even while the tool window is closed
        if (!mouseDown)
            EndStroke();

        if (ShowOverlay && OverlayDirty && OverlayContainer != null && StrokeOrder.Count == 0)
            RebuildOverlay(OverlayContainer);

        CheckBlockerMoves();
        if (BlockerOverlayDirty)
            UpdateBlockerOverlay();

        // The chosen navmesh's gates, drawn after the overlay they sit on
        if (GateOverlayDirty && !OverlayDirty)
            DrawGateOverlay();

        // Vertex editing works by keys; it only takes the clicks while a vertex is dragged
        if (UpdateVertexEdit(ray, canInteract))
            return true;

        // Gate mode picks and drags gates
        if (UpdateGateMode(ray, canInteract))
            return true;

        if (OverlayNavigation == null || (!PickMode && !PaintMode && !LinkMode))
            return false;

        if (LinkMode)
        {
            if (canInteract && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
                LinkClick(PickAt(ray));

            return true;
        }

        if (PaintMode)
        {
            if (canInteract && mouseDown)
                PaintAt(PickAt(ray));

            return true;
        }

        if (canInteract && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            SetPicked(PickAt(ray));

        return true;
    }

    private NavFaceHit PickAt(Ray ray)
        => NavigationOverlayBuilder.Pick(OverlayNavigation, OverlaySet, ray.Origin, ray.Direction, e => EntryWorld(OverlayContainer, e));

    private void DisplayInspector()
    {
        // Face: the toggle hides its edge list; the face's details and face data input stay outside it
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Face_Header"), LOC.Get("MAP_Navigation_Face_Header_TT"), ref ShowFaceEdges);

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

        // The typed values start from the face's own whenever another face is picked, or after an edit
        if (!ReferenceEquals(TextFor, hit))
        {
            TextFor = hit;
            EdgeDataText.Clear();
            FaceDataText = FaceData(mesh, hit.Face) is uint value ? value.ToString("X8") : "";
        }

        if (FaceData(mesh, hit.Face) is uint data)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Data", data.ToString("X8"), DescribeBits(data)));

            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##navigationFaceData", ref FaceDataText, 16);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Face_Data_Edit_TT"));

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                if (TryParseHex(FaceDataText, out var typed) && typed != data)
                    Execute(new[] { new NavFaceDataChange(hit.File, hit.Face, (int)data, (int)typed) }, null);
                else
                    FaceDataText = data.ToString("X8");
            }
        }

        if (!ShowFaceEdges)
            return;

        // Its edges, and what each leads to
        for (int k = face.m_startEdgeIndex; k < face.m_startEdgeIndex + face.m_numEdges; k++)
            GUI.WrappedText(DescribeEdge(mesh, entry, k, false));

        // Its user edges; the Links tab edits them
        for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
            GUI.WrappedText(DescribeEdge(mesh, entry, k, true));
    }

    /// <summary>The picked face's user edges, each with its blocked flag, its edge data and Remove link.</summary>
    private void DisplayPickedLinks()
    {
        // Picked Face Links
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Picked_Links_Header"), LOC.Get("MAP_Navigation_Picked_Links_Header_TT"));

        if (Picked == null || OverlayNavigation == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_None"));
            return;
        }

        var hit = Picked;
        var mesh = hit.File.Mesh;
        var face = mesh.m_faces[hit.Face];

        GUI.WrappedText(LOC.Get("MAP_Navigation_Picked_Links_Face", hit.Face, hit.File.Name, face.m_numUserEdges));

        if (!ReferenceEquals(TextFor, hit))
        {
            TextFor = hit;
            EdgeDataText.Clear();
            FaceDataText = FaceData(mesh, hit.Face) is uint value ? value.ToString("X8") : "";
        }

        // User edges can be blocked and their data changed; that renumbers nothing
        for (int k = face.m_startUserEdgeIndex; k < face.m_startUserEdgeIndex + face.m_numUserEdges; k++)
        {
            GUI.WrappedText(DescribeEdge(mesh, hit.Entry, k, true));
            DisplayUserEdgeEdit(hit, mesh, k);

            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Link_Remove")}##navigationLinkRemove_{k}"))
            {
                RemoveLink(hit, k);
                break;
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Link_Remove_TT"));
        }
    }

    private static uint? FaceData(IHavokNavMeshEdit mesh, int face)
    {
        int index = face * mesh.m_faceDataStriding;
        return mesh.m_faceDataStriding > 0 && index < mesh.m_faceData.Count ? (uint)mesh.m_faceData[index] : null;
    }

    private void DisplayUserEdgeEdit(NavFaceHit hit, IHavokNavMeshEdit mesh, int k)
    {
        var edge = mesh.m_edges[k];
        int flags = (int)edge.m_flags;
        int blockedBit = (int)HavokNavEdgeFlags.EDGE_BLOCKED;
        bool hasData = mesh.m_edgeDataStriding > 0 && k * mesh.m_edgeDataStriding < mesh.m_edgeData.Count;
        int data = hasData ? mesh.m_edgeData[k * mesh.m_edgeDataStriding] : 0;

        bool blocked = (flags & blockedBit) != 0;
        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Edge_Blocked")}##navigationEdgeBlocked_{k}", ref blocked))
        {
            int after = blocked ? flags | blockedBit : flags & ~blockedBit;
            Execute(null, new[] { new NavEdgeChange(hit.File, k, flags, after, data, data) });
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edge_Blocked_TT"));

        if (!hasData)
            return;

        ImGui.SameLine();

        if (!EdgeDataText.TryGetValue(k, out var text))
            EdgeDataText[k] = text = ((uint)data).ToString("X");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText($"##navigationEdgeData_{k}", ref text, 16);
        EdgeDataText[k] = text;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edge_Data_Edit_TT"));

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (TryParseHex(text, out var typed) && (int)typed != data)
                Execute(null, new[] { new NavEdgeChange(hit.File, k, flags, flags, data, (int)typed) });
            else
                EdgeDataText[k] = ((uint)data).ToString("X");
        }
    }

    /// <summary>Applies an edit of the picked map as an undoable action.</summary>
    private void Execute(IReadOnlyList<NavFaceDataChange> faces, IReadOnlyList<NavEdgeChange> edges)
    {
        if (OverlayContainer == null)
            return;

        View.ViewportActionManager.ExecuteAction(new NavigationEditAction(OverlayContainer, faces, edges, OnNavigationEdited));
    }

    private string DescribeEdge(IHavokNavMeshEdit mesh, NVA.Navmesh entry, int k, bool user)
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

        // User edge data says what kind of link it is (jump, fall); the game's cost reads those bits
        var dataText = user && data != 0 ? $"{data:X} ({DescribeBits(data, NavFaceDataBits.Edge)})" : data.ToString("X");
        return LOC.Get(user ? "MAP_Navigation_Edge_User" : "MAP_Navigation_Edge", k, (int)edge.m_flags, dataText, target);
    }
}
