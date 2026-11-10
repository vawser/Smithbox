using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Gates tab (Navigation Editor Plan, Phase 3: Gate nodes): the NVA's gate nodes of the drawn set, picked and moved
/// on the overlay, with the chosen navmesh's gates, their cost table, adding and removing gates, and rebuilding them
/// from the connections. All editing goes through the overlay; see NavGates for the rules.
/// </summary>
public partial class NavigationTool
{
    private readonly NavigationOverlayRenderer GateRenderer = new();

    /// <summary>Picking and moving gates in the viewport.</summary>
    private bool GateMode;

    /// <summary>The navmesh whose gates the tab shows, and its chosen gate (-1 for none).</summary>
    private NVA.Navmesh GatesEntry;
    private int GateSelected = -1;

    private bool GateOverlayDirty;
    private bool ShowGateList = true;
    private bool ShowGateCosts = true;
    private float GateJoin = NavGates.DefaultRunJoin;
    private int GateAddTarget;

    /// <summary>What the last gate edit did, or why it was refused.</summary>
    private string GateMessage;
    private bool GateMessageIsError;

    /// <summary>The cost cells as typed, in metres, for the navmesh they were read from.</summary>
    private readonly Dictionary<(int, int), string> GateCostText = new();
    private NVA.Navmesh GateCostTextFor;

    /// <summary>A gate being dragged: its gates before the drag, and the height of the plane it moves across.</summary>
    private NavGateState GateDragBefore;
    private bool GateDragging;
    private float GateDragHeight;

    /// <summary>Re-stitching also rebuilds the gates of the navmesh pairs whose connections it changes.</summary>
    private bool StitchUpdateGates = true;

    private void StopGateMode()
    {
        GateMode = false;
        GateDragging = false;
        GateDragBefore = null;
    }

    /// <summary>Places a navmesh's mesh in map space, where its gates are: its place in the viewport, out of the map's.</summary>
    private static Func<NVA.Navmesh, Matrix4x4> GateToMap(MapContainer map)
    {
        var mapWorld = MapWorld(map);
        Matrix4x4.Invert(mapWorld, out var inverse);
        return e => EntryWorld(map, e) * inverse;
    }

    /// <summary>The chosen navmesh, if it is still in the drawn map and set; else nothing is chosen.</summary>
    private NVA.Navmesh CurrentGatesEntry()
    {
        if (GatesEntry != null && (OverlayNavigation == null || !OverlayNavigation.Nva.Navmeshes.Contains(GatesEntry)
            || NavigationMap.SetOf(GatesEntry) != OverlaySet))
        {
            GatesEntry = null;
            GateSelected = -1;
            GateOverlayDirty = true;
        }

        if (GatesEntry != null && GateSelected >= GatesEntry.GateNodes.Count)
        {
            GateSelected = -1;
            GateOverlayDirty = true;
        }

        return GatesEntry;
    }

    private void ChooseGates(NVA.Navmesh entry, int gate)
    {
        if (GatesEntry != entry || GateSelected != gate)
            GateOverlayDirty = true;

        GatesEntry = entry;
        GateSelected = gate;
    }

    private void DisplayGatesTab()
    {
        GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_Description"));

        if (!ShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Pick_Needs_Overlay"));

        // Pick
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Gates_Pick_Header"), LOC.Get("MAP_Navigation_Gates_Pick_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Gates_Mode")}##navigationGateMode", ref GateMode))
        {
            if (GateMode)
            {
                PickMode = false;
                PaintMode = false;
                LinkMode = false;
                StopVertexEdit();
            }
            else
                StopGateMode();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Gates_Mode_TT"));

        if (OverlayNavigation == null || OverlayContainer == null)
            return;

        var map = OverlayNavigation;
        var entry = CurrentGatesEntry();

        // Navmesh
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Gates_Navmesh_Header"), LOC.Get("MAP_Navigation_Gates_Navmesh_Header_TT"));

        if (entry == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_None"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_Navmesh", (uint)entry.NameID, entry.ModelID, map.FileNameOf(entry), entry.GateNodes.Count, NavGates.MaxGates));

            GUI.MultiButtonInput("navigationGateNavmeshActions",
                "navigationGateRebuildOne", LOC.Get("MAP_Navigation_Gates_Rebuild_One"), LOC.Get("MAP_Navigation_Gates_Rebuild_One_TT"),
                () => RebuildGates(new[] { entry }),

                "navigationGateRecompute", LOC.Get("MAP_Navigation_Gates_Recompute"), LOC.Get("MAP_Navigation_Gates_Recompute_TT"),
                () => EditGates(m => { NavGates.RecomputeCosts(m, entry, GateToMap(OverlayContainer)); return new NavGateEditResult(); },
                    "MAP_Navigation_Gates_Recomputed"));

            DisplayGateList(map, entry);
            DisplayGateCosts(entry);
            DisplayGateAdd(map, entry);
        }

        // Rebuild
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Gates_Rebuild_Header"), LOC.Get("MAP_Navigation_Gates_Rebuild_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationGateJoin", ref GateJoin, 0.5f, 20f, $"{LOC.Get("MAP_Navigation_Gates_Join")} %.1f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Gates_Join_TT"));

        GUI.MultiButtonInput("navigationGateRebuildActions",
            "navigationGateRebuildAll", LOC.Get("MAP_Navigation_Gates_Rebuild_All"), LOC.Get("MAP_Navigation_Gates_Rebuild_All_TT"),
            () => RebuildGates(NavigationOverlayBuilder.EntriesOf(map, OverlaySet).ToList()));

        if (GateMessage != null)
            GUI.WrappedTextColored(GateMessageIsError ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Benefit_Text_Color, GateMessage);

        // Gates the game wouldn't have: the counts stay outside any list
        int unpaired = 0, broken = 0;
        foreach (var e in NavigationOverlayBuilder.EntriesOf(map, OverlaySet))
            for (int g = 0; g < e.GateNodes.Count; g++)
                switch (NavGates.Status(map, e, g))
                {
                    case NavGateStatus.Unpaired: unpaired++; break;
                    case NavGateStatus.Broken: broken++; break;
                }

        if (unpaired > 0 || broken > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Navigation_Gates_Problems", unpaired, broken));
    }

    private void DisplayGateList(NavigationMap map, NVA.Navmesh entry)
    {
        // Gates
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Gates_List_Header"), LOC.Get("MAP_Navigation_Gates_List_Header_TT"), ref ShowGateList);

        if (!ShowGateList || entry.GateNodes.Count == 0)
        {
            if (entry.GateNodes.Count == 0)
                GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_List_Empty"));
            return;
        }

        var list = map.Nva.Navmeshes;
        int remove = -1;

        if (ImGui.BeginTable("##navigationGateList", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Gates_Column_Gate"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Gates_Column_Leads_To"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Gates_Column_Status"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            for (int g = 0; g < entry.GateNodes.Count; g++)
            {
                var gate = entry.GateNodes[g];
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{g}##navigationGateSelect_{g}", g == GateSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowOverlap))
                    ChooseGates(entry, g == GateSelected ? -1 : g);
                GUI.Tooltip(LOC.Get("MAP_Navigation_Gates_Select_TT"));

                ImGui.TableNextColumn();
                if (gate.ConnectedNavmeshIndex >= 0 && gate.ConnectedNavmeshIndex < list.Count)
                {
                    var target = list[gate.ConnectedNavmeshIndex];
                    GUI.WrappedText(gate.NodeSubID >= 0
                        ? LOC.Get("MAP_Navigation_Gates_Leads_To", gate.ConnectedNavmeshIndex, map.FileNameOf(target), gate.NodeSubID)
                        : LOC.Get("MAP_Navigation_Gates_Leads_To_Unpaired", gate.ConnectedNavmeshIndex, map.FileNameOf(target)));
                }
                else
                {
                    GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_Leads_Nowhere", gate.ConnectedNavmeshIndex));
                }

                ImGui.TableNextColumn();
                var status = NavGates.Status(map, entry, g);
                var text = LOC.Get($"MAP_Navigation_Gates_Status_{status}");
                if (status is NavGateStatus.Paired or NavGateStatus.UnpairedFull)
                    ImGui.Text(text);
                else
                    GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, text);
                GUI.Tooltip(LOC.Get($"MAP_Navigation_Gates_Status_{status}_TT"));

                ImGui.TableNextColumn();
                if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Gates_Remove")}##navigationGateRemove_{g}"))
                    remove = g;
                GUI.Tooltip(LOC.Get("MAP_Navigation_Gates_Remove_TT"));
            }

            ImGui.EndTable();
        }

        if (remove >= 0)
        {
            int index = remove;
            EditGates(m => NavGates.Remove(m, entry, index), "MAP_Navigation_Gates_Removed");
            GateSelected = -1;
        }

        if (GateSelected >= 0 && GateSelected < entry.GateNodes.Count)
        {
            var p = entry.GateNodes[GateSelected].Position;
            GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_Selected", GateSelected, $"{p.X:0.##}, {p.Y:0.##}, {p.Z:0.##}"));
        }
    }

    private void DisplayGateCosts(NVA.Navmesh entry)
    {
        var gates = entry.GateNodes;

        // Costs
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Gates_Costs_Header"), LOC.Get("MAP_Navigation_Gates_Costs_Header_TT"), ref ShowGateCosts);

        if (!ShowGateCosts || gates.Count < 2)
            return;

        // AC6 ships no costs between gates, and edits keep it so; a cost typed here is the only kind its files get
        if (!Bank.Profile.StoresGateCosts)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_Costs_Not_Stored"));

        if (GateCostTextFor != entry)
        {
            GateCostTextFor = entry;
            GateCostText.Clear();
        }

        // A scrolling table takes the rest of the window unless it is given a height: one row per gate, plus the header
        // and the scroll bar
        var style = ImGui.GetStyle();
        float height = (gates.Count + 1) * (ImGui.GetFrameHeight() + style.CellPadding.Y * 2) + style.ScrollbarSize + style.FramePadding.Y * 2;

        if (!ImGui.BeginTable("##navigationGateCosts", gates.Count + 1, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX | ImGuiTableFlags.SizingFixedFit,
            new Vector2(0, height)))
            return;

        float cell = 64 * DPI.UIScale();
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        for (int j = 0; j < gates.Count; j++)
            ImGui.TableSetupColumn($"{j}", ImGuiTableColumnFlags.WidthFixed, cell);
        ImGui.TableHeadersRow();

        (int, int, short)? commit = null;

        for (int i = 0; i < gates.Count; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text($"{i}");

            for (int j = 0; j < gates.Count; j++)
            {
                ImGui.TableNextColumn();

                if (i == j)
                {
                    ImGui.Text("0");
                    continue;
                }

                short cost = j < gates[i].NeighbourGateNodeCosts.Count ? gates[i].NeighbourGateNodeCosts[j] : NavGates.Unreachable;
                if (!GateCostText.TryGetValue((i, j), out var text))
                    GateCostText[(i, j)] = text = CostText(cost);

                ImGui.SetNextItemWidth(-1);
                ImGui.InputText($"##navigationGateCost_{i}_{j}", ref text, 16);
                GateCostText[(i, j)] = text;
                GUI.Tooltip(LOC.Get("MAP_Navigation_Gates_Cost_TT", i, j));

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (TryParseCost(text, out var typed) && typed != cost)
                        commit = (i, j, typed);
                    else
                        GateCostText[(i, j)] = CostText(cost);
                }
            }
        }

        ImGui.EndTable();

        if (commit is (int a, int b, short value))
            EditGates(m => { NavGates.SetCost(entry, a, b, value, m.Profile); return new NavGateEditResult(); }, "MAP_Navigation_Gates_Cost_Set");
    }

    /// <summary>A cost as metres, or "-" for unreachable.</summary>
    private static string CostText(short cost) => NavGates.CostMetres(cost) is float m ? m.ToString("0.##", CultureInfo.InvariantCulture) : "-";

    private static bool TryParseCost(string text, out short cost)
    {
        text = text.Trim();
        cost = NavGates.Unreachable;

        if (text is "-" or "")
            return true;

        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var metres) || metres < 0)
            return false;

        cost = NavGates.EncodeCost(metres);
        return true;
    }

    private void DisplayGateAdd(NavigationMap map, NVA.Navmesh entry)
    {
        var neighbours = NavGates.Neighbours(map, entry);

        // Add Gate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Gates_Add_Header"), LOC.Get("MAP_Navigation_Gates_Add_Header_TT"));

        if (neighbours.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Gates_Add_No_Neighbours"));
            return;
        }

        GateAddTarget = Math.Clamp(GateAddTarget, 0, neighbours.Count - 1);
        var list = map.Nva.Navmeshes;
        string Name(NVA.Navmesh e) => LOC.Get("MAP_Navigation_Gates_Neighbour", list.IndexOf(e), map.FileNameOf(e), e.GateNodes.Count, NavGates.MaxGates);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##navigationGateAddTarget", Name(neighbours[GateAddTarget])))
        {
            for (int i = 0; i < neighbours.Count; i++)
                if (ImGui.Selectable($"{Name(neighbours[i])}##navigationGateAddTarget_{i}", i == GateAddTarget))
                    GateAddTarget = i;
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Gates_Add_Target_TT"));

        var target = neighbours[GateAddTarget];
        GUI.ConditionalMultiButtonInput("navigationGateAddActions",
            "navigationGateAdd", LOC.Get("MAP_Navigation_Gates_Add"), LOC.Get("MAP_Navigation_Gates_Add_TT"),
            () => EditGates(m => NavGates.Add(m, entry, target, null, GateToMap(OverlayContainer)), "MAP_Navigation_Gates_Added"),
            entry.GateNodes.Count < NavGates.MaxGates);
    }

    private void RebuildGates(IReadOnlyList<NVA.Navmesh> owners)
    {
        float join = GateJoin;
        EditGates(m => NavGates.Rebuild(m, owners, GateToMap(OverlayContainer), join), "MAP_Navigation_Gates_Rebuilt");
    }

    /// <summary>Makes a gate edit to the drawn map and records it as one undoable action.</summary>
    private void EditGates(Func<NavigationMap, NavGateEditResult> edit, string message)
    {
        if (OverlayContainer == null || OverlayNavigation == null)
            return;

        var before = new NavGateState(OverlayNavigation.Nva);
        var result = edit(OverlayNavigation);

        if (!result.Ok)
        {
            before.Restore();
            GateMessage = LOC.Get(result.Error);
            GateMessageIsError = true;
            return;
        }

        var after = new NavGateState(OverlayNavigation.Nva);
        View.ViewportActionManager.ExecuteAction(new NavigationGateEditAction(OverlayContainer, before, after, OnNavigationEdited));
        GateMessage = LOC.Get(message, result.Added, result.Removed, result.Touched.Count);
        GateMessageIsError = false;
    }

    /// <summary>
    /// Called each frame by HandleViewportInput: draws the chosen navmesh's gates, and while gate mode is on, picks a
    /// gate (or the navmesh of a face) on a click and drags a gate along its connection edges. Returns true while the
    /// clicks are taken.
    /// </summary>
    private bool UpdateGateMode(Ray ray, bool canInteract)
    {
        if (!GateMode || OverlayNavigation == null || OverlayContainer == null)
            return false;

        var map = OverlayNavigation;
        var mapWorld = MapWorld(OverlayContainer);
        bool down = InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action);

        if (GateDragging)
        {
            var entry = CurrentGatesEntry();

            if (entry == null || GateSelected < 0)
            {
                GateDragging = false;
                GateDragBefore = null;
                return true;
            }

            // Across the plane at the gate's height, then onto its connection edges
            var dir = Vector3.Normalize(ray.Direction);
            if (MathF.Abs(dir.Y) > 1e-4f)
            {
                float t = (GateDragHeight - ray.Origin.Y) / dir.Y;
                if (t > 0 && Matrix4x4.Invert(mapWorld, out var toMap))
                {
                    var onPlane = Vector3.Transform(ray.Origin + dir * t, toMap);
                    if (NavGates.DragTo(map, entry, GateSelected, onPlane, GateToMap(OverlayContainer)) != null)
                        DrawGateOverlay();
                }
            }

            if (!down)
            {
                GateDragging = false;
                var before = GateDragBefore;
                GateDragBefore = null;

                // The costs are walked once, at the end
                var at = entry.GateNodes[GateSelected].Position;
                var moved = NavGates.Move(map, entry, GateSelected, at, GateToMap(OverlayContainer));
                if (moved.Ok && before != null)
                {
                    View.ViewportActionManager.ExecuteAction(new NavigationGateEditAction(OverlayContainer, before, new NavGateState(map.Nva), OnNavigationEdited));
                    GateMessage = LOC.Get("MAP_Navigation_Gates_Moved");
                    GateMessageIsError = false;
                }
            }

            return true;
        }

        if (canInteract && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            if (NavigationOverlayBuilder.PickGate(map, OverlaySet, ray.Origin, ray.Direction, mapWorld) is (NVA.Navmesh owner, int gate))
            {
                // A second click on the chosen gate starts a drag
                if (owner == GatesEntry && gate == GateSelected)
                {
                    GateDragBefore = new NavGateState(map.Nva);
                    GateDragging = true;
                    GateDragHeight = Vector3.Transform(owner.GateNodes[gate].Position, mapWorld).Y;
                }

                ChooseGates(owner, gate);
            }
            else if (PickAt(ray) is NavFaceHit hit)
            {
                ChooseGates(hit.Entry, -1);
            }
        }

        return true;
    }

    /// <summary>Draws the chosen navmesh's cost lines and its chosen gate.</summary>
    private void DrawGateOverlay()
    {
        GateOverlayDirty = false;
        var entry = CurrentGatesEntry();
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (!ShowOverlay || entry == null || OverlayNavigation == null || OverlayContainer == null || scene == null)
        {
            GateRenderer.Clear();
            return;
        }

        var overlay = NavigationOverlayBuilder.GateHighlight(OverlayNavigation, entry, GateSelected, MapWorld(OverlayContainer),
            Layers.Contains(NavOverlayLayer.GateCosts));
        GateRenderer.Show(scene, overlay, FaceOpacity);
    }

    /// <summary>
    /// The labels of the chosen navmesh's gates in the viewport: each gate's index, and each reachable pair's cost in
    /// metres at the middle of its line. Called by the viewport's overlay.
    /// </summary>
    public unsafe void DrawGateLabels(VulkanViewport viewport)
    {
        if (!CanUse() || !ShowOverlay || OverlayNavigation == null || OverlayContainer == null || viewport.Owner is not MapUniverse)
            return;

        var entry = CurrentGatesEntry();
        if (entry == null || entry.GateNodes.Count == 0)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var origin = new Vector2(viewport.X, viewport.Y);
        var mapWorld = MapWorld(OverlayContainer);
        var gates = entry.GateNodes;
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
        var gateColor = ImGui.GetColorU32(new Vector4(NavigationOverlayRenderer.LayerColor(NavOverlayLayer.GateNodes), 1));
        var costColor = ImGui.GetColorU32(new Vector4(NavigationOverlayRenderer.LayerColor(NavOverlayLayer.GateCosts), 1));

        void Label(Vector3 world, string text, uint color)
        {
            var screen = viewport.WorldToScreen(world, out bool inFront);
            if (!inFront || screen.X < 0 || screen.Y < 0 || screen.X > viewport.Width || screen.Y > viewport.Height)
                return;

            var at = origin + screen + new Vector2(6, -6) * DPI.UIScale();
            drawList.AddText(at + Vector2.One, shadow, text);
            drawList.AddText(at, color, text);
        }

        var points = gates.Select(g => Vector3.Transform(g.Position, mapWorld)).ToList();

        for (int i = 0; i < gates.Count; i++)
            Label(points[i], $"{i}", gateColor);

        if (!Layers.Contains(NavOverlayLayer.GateCosts))
            return;

        for (int i = 0; i < gates.Count; i++)
            for (int j = i + 1; j < gates.Count; j++)
                if (j < gates[i].NeighbourGateNodeCosts.Count && NavGates.CostMetres(gates[i].NeighbourGateNodeCosts[j]) is float m)
                    Label((points[i] + points[j]) / 2, $"{m:0.#} m", costColor);
    }
}
