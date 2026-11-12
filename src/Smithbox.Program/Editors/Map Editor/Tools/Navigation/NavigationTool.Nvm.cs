using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Navigation tool's NVM mode, for DeS, DS1 and DS1R (Navigation Editor Other Games Plan, Phase N6). These games
/// keep their navigation in NVM files (one per MSB navmesh part, in the map's NVM binder), an MCP of rooms (one per part)
/// and an MCG graph whose nodes sit on gate triangles. The mode draws them over the map, picks triangles and nodes,
/// paints triangle flags, edits the NVM entities (triangle sets an event can disable), moves vertices with the Edit
/// tab's brush, edits the MCG's nodes and edges, keeps the MCP room boxes in step, and checks it all. Every edit is one
/// undoable action (NavigationNvmEditAction); the map's save writes the files (NvmBank).
/// </summary>
public partial class NavigationTool
{
    private NvmBank Nvm => View.NvmBank;

    /// <summary>The project's game keeps NVM navigation (DeS, DS1, DS1R).</summary>
    private bool IsNvmMode => View.NvmBank?.CanUse() == true;

    private enum NvmTab { Overlay, Models, Flags, Entities, Graph, Edit, Check }

    /// <summary>What a click in the viewport does in the NVM mode.</summary>
    private enum NvmClick { None, Pick, Paint, Entity, Node, Connect }

    private NvmTab? NvmOpenTab;
    private NvmClick NvmClickMode;

    // Overlay
    private bool NvmShowOverlay;
    private bool NvmOverlayDirty;
    private float NvmOpacity = 45;
    private bool NvmOnlyPickedRoom;
    private bool NvmShowLegend = true;
    private MapContainer NvmContainer;
    private NvmMap NvmData;
    private int NvmOverlayRevision = -1;

    private readonly HashSet<NavOverlayLayer> NvmLayers = new()
    {
        NavOverlayLayer.Faces,
        NavOverlayLayer.OpenEdges,
        NavOverlayLayer.NvmGates,
        NavOverlayLayer.NvmEntities,
        NavOverlayLayer.McgNodes,
        NavOverlayLayer.McgEdges,
    };

    private static readonly NavOverlayLayer[] NvmToggleLayers =
    {
        NavOverlayLayer.Faces, NavOverlayLayer.InnerEdges, NavOverlayLayer.OpenEdges, NavOverlayLayer.NvmGates,
        NavOverlayLayer.NvmEntities, NavOverlayLayer.McpRooms, NavOverlayLayer.McgNodes, NavOverlayLayer.McgEdges,
    };

    // Selection, by index so it outlives the NvmMap being built again after an edit
    private int NvmPickedRoom = -1;
    private int NvmPickedTriangle = -1;
    private Vector3 NvmPickedPoint;
    private int NvmSelectedEntity = -1;
    private int NvmSelectedNode = -1;
    private int NvmSelectedEdge = -1;

    // Flag painting
    private PaintOp NvmPaintOp = PaintOp.AddBits;
    private NVM.TriangleFlags NvmPaintFlags = NVM.TriangleFlags.GATE;
    private readonly Dictionary<(int Room, int Triangle), NVM.TriangleFlags> NvmStroke = new();

    // Entities and the graph, as typed
    private int NvmNewEntityId;
    private string NvmEntityIdText = "";
    private int NvmEntityIdFor = -1;
    private Vector3 NvmNodePositionText;
    private int NvmNodePositionFor = -1;
    private bool NvmEntityRemove;

    private string NvmMessage;
    private bool NvmMessageIsError;

    private static readonly NVM.TriangleFlags[] NvmFlagBits = Enum.GetValues<NVM.TriangleFlags>().Where(f => f != NVM.TriangleFlags.NONE).ToArray();

    // Check
    private sealed class NvmCheckResult
    {
        public string MapId;
        public int Run;
        public List<NavIssue> Issues;
        public bool BaselineDone;
        public string BaselineError;
        public bool Stale;
    }

    private NvmCheckResult NvmCurrent;
    private int NvmRunCounter;
    private CancellationTokenSource NvmCancel;
    private readonly ConcurrentQueue<(int Run, List<NavIssue> Vanilla, string Error)> NvmBaselineResults = new();

    #region Tool window

    private void DisplayNvm(MapContainer map)
    {
        while (NvmBaselineResults.TryDequeue(out var baseline))
        {
            if (NvmCurrent == null || baseline.Run != NvmCurrent.Run)
                continue;

            if (baseline.Error != null)
                NvmCurrent.BaselineError = baseline.Error;
            else
                NvmCurrent.Issues = NavigationValidator.MarkInherited(NvmCurrent.Issues, baseline.Vanilla);

            NvmCurrent.BaselineDone = true;
        }

        if (!Nvm.HasBinder(map))
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_No_Binder", map.Name));
            return;
        }

        DisplayNvmHeader(map);

        GUI.Spacer();

        // The overlay follows the map chosen above; edits and undo redraw it from the viewport too
        if (NvmShowOverlay && (NvmContainer != map || NvmOverlayDirty) && NvmStroke.Count == 0)
            RebuildNvmOverlay(map);

        if (ImGui.BeginTabBar("navigationNvmTabs"))
        {
            if (BeginNvmTab(NvmTab.Overlay, "MAP_Navigation_Tab_Overlay", "navigationNvmTabOverlay"))
            {
                DisplayNvmOverlayTab();
                ImGui.EndTabItem();
            }

            if (BeginNvmTab(NvmTab.Models, "MAP_Navigation_Tab_Models", "navigationNvmTabModels"))
            {
                DisplayNvmModelsTab(map);
                ImGui.EndTabItem();
            }

            if (BeginNvmTab(NvmTab.Flags, "MAP_Navigation_Nvm_Tab_Flags", "navigationNvmTabFlags"))
            {
                DisplayNvmFlagsTab();
                ImGui.EndTabItem();
            }

            if (BeginNvmTab(NvmTab.Entities, "MAP_Navigation_Nvm_Tab_Entities", "navigationNvmTabEntities"))
            {
                DisplayNvmEntitiesTab(map);
                ImGui.EndTabItem();
            }

            if (BeginNvmTab(NvmTab.Graph, "MAP_Navigation_Nvm_Tab_Graph", "navigationNvmTabGraph"))
            {
                DisplayNvmGraphTab(map);
                ImGui.EndTabItem();
            }

            if (BeginNvmTab(NvmTab.Edit, "MAP_Navigation_Tab_Edit", "navigationNvmTabEdit"))
            {
                DisplayNvmEditTab();
                ImGui.EndTabItem();
            }

            if (BeginNvmTab(NvmTab.Check, "MAP_Navigation_Tab_Check", "navigationNvmTabCheck"))
            {
                DisplayNvmCheckTab(map);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private bool BeginNvmTab(NvmTab tab, string key, string id)
    {
        if (!ImGui.BeginTabItem($"{LOC.Get(key)}##{id}"))
            return false;

        if (NvmOpenTab != tab)
        {
            NvmOpenTab = tab;

            // The modes of other tabs stop, so a mode never takes the viewport's clicks out of sight; picking stays
            if (NvmClickMode is NvmClick.Paint && tab != NvmTab.Flags)
                SetNvmClick(NvmClick.None);
            if (NvmClickMode is NvmClick.Entity && tab != NvmTab.Entities)
                SetNvmClick(NvmClick.None);
            if (NvmClickMode is NvmClick.Node or NvmClick.Connect && tab != NvmTab.Graph)
                SetNvmClick(NvmClick.None);
            if (tab != NvmTab.Edit)
                StopNvmVertexEdit();
        }

        GUI.WrappedText(LOC.Get($"MAP_Navigation_Nvm_Tab_{tab}_Help"));
        return true;
    }

    private void SetNvmClick(NvmClick mode)
    {
        NvmClickMode = mode;
        NvmStroke.Clear();

        if (mode != NvmClick.None)
            StopNvmVertexEdit();
    }

    /// <summary>Above the tabs: the overlay toggle, what a click does, the selection and pending edits, the last check.</summary>
    private void DisplayNvmHeader(MapContainer map)
    {
        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Viewport_Header"), LOC.Get("MAP_Navigation_Viewport_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Overlay_Show")}##navigationNvmOverlayShow", ref NvmShowOverlay))
        {
            if (NvmShowOverlay)
                NvmOverlayDirty = true;
            else
                ClearNvmOverlay();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Overlay_Show_TT"));

        // The mode that takes the clicks
        string mode = NvmVertexActive ? EditModeKey : NvmClickMode switch
        {
            NvmClick.Pick => "MAP_Navigation_Mode_Pick",
            NvmClick.Paint => "MAP_Navigation_Mode_Paint",
            NvmClick.Entity => "MAP_Navigation_Nvm_Mode_Entity",
            NvmClick.Node => "MAP_Navigation_Nvm_Mode_Node",
            NvmClick.Connect => "MAP_Navigation_Nvm_Mode_Connect",
            _ => null,
        };

        if (mode == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Mode_None"));
        }
        else
        {
            if (ImGui.Button($"{Icons.Stop}##navigationNvmModeStop"))
            {
                SetNvmClick(NvmClick.None);
                StopNvmVertexEdit();
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Mode_Stop_TT"));
            ImGui.SameLine();

            if (!NvmShowOverlay)
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Mode_No_Overlay", LOC.Get(mode)));
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get(mode));
        }

        // The picked triangle and the chosen node
        var data = NvmData != null && NvmContainer == map ? NvmData : null;

        if (data != null && PickedNvmRoom(data) is NvmRoom room)
        {
            if (ImGui.Button($"{Icons.Times}##navigationNvmPickedClear"))
                SetNvmPicked(-1, -1, default);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Picked_Clear_TT"));
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_Status", NvmPickedTriangle, room.FileName, room.PartName));
        }

        if (data?.Mcg != null && NvmSelectedNode >= 0 && NvmSelectedNode < data.Mcg.Nodes.Count)
        {
            if (ImGui.Button($"{Icons.Times}##navigationNvmNodeClear"))
                SelectNvmNode(-1);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Node_Clear_TT"));
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Node_Status", NvmSelectedNode, data.Mcg.Nodes[NvmSelectedNode].ConnectedEdgeIndices.Count));
        }

        if (HasNvmVertexEdits)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_Navigation_Pending_Status", NvmVertexCopies.Values.Count(c => c.IsEdited), NvmVertexMap?.Name ?? "-"));

        if (NvmCurrent != null && NvmCurrent.MapId == map.Name)
        {
            int errors = NvmCurrent.Issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Error);
            int warnings = NvmCurrent.Issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Warning);
            var text = errors == 0 && warnings == 0 ? LOC.Get("MAP_Navigation_Check_Status_Clean") : LOC.Get("MAP_Navigation_Check_Status", errors, warnings);
            if (NvmCurrent.Stale)
                text += " " + LOC.Get("MAP_Navigation_Check_Status_Stale");

            GUI.WrappedTextColored(NvmCurrent.Stale ? UI.Current.ImGui_Invalid_Text_Color
                : errors > 0 ? UI.Current.ImGui_Warning_Text_Color
                : warnings > 0 ? UI.Current.ImGui_Invalid_Text_Color
                : UI.Current.ImGui_Benefit_Text_Color, text);
        }

        if (NvmMessage != null)
            GUI.WrappedTextColored(NvmMessageIsError ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Benefit_Text_Color, NvmMessage);
    }

    private void SetNvmMessage(string message, bool error = false)
    {
        NvmMessage = message;
        NvmMessageIsError = error;
    }

    #endregion

    #region Overlay tab

    private void DisplayNvmOverlayTab()
    {
        // Overlay
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Overlay_Header"), LOC.Get("MAP_Navigation_Overlay_Header_TT"));

        if (!NvmShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Overlay_Off"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##navigationNvmFaceOpacity", ref NvmOpacity, 5, 100, $"{LOC.Get("MAP_Navigation_Overlay_Opacity")} %.0f"))
            NvmOverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Overlay_Opacity_TT"));

        if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Nvm_Only_Picked_Room")}##navigationNvmOnlyPicked", ref NvmOnlyPickedRoom))
            NvmOverlayDirty = true;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Only_Picked_Room_TT"));

        // Layers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Layers_Header"), LOC.Get("MAP_Navigation_Layers_Header_TT"));

        foreach (var layer in NvmToggleLayers)
        {
            bool on = NvmLayers.Contains(layer);

            if (layer != NavOverlayLayer.Faces)
            {
                var c = NavigationOverlayRenderer.LayerColor(layer);
                ImGui.ColorButton($"##navigationNvmLayerColor_{layer}", new Vector4(c, 1), ImGuiColorEditFlags.NoTooltip, new Vector2(14, 14) * DPI.UIScale());
                ImGui.SameLine();
            }

            if (ImGui.Checkbox($"{LOC.Get($"MAP_Navigation_Nvm_Layer_{layer}")}##navigationNvmLayer_{layer}", ref on))
            {
                if (on) NvmLayers.Add(layer); else NvmLayers.Remove(layer);
                NvmOverlayDirty = true;
            }
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Nvm_Layer_{layer}_TT"));
        }

        // Summary
        if (NvmData != null)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Summary_Header"), LOC.Get("MAP_Navigation_Summary_Header_TT"));

            var d = NvmData;
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Summary_Rooms", d.Rooms.Count, d.Files.Count, d.Rooms.Count(r => r.Nvm == null)));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Summary_Triangles", d.Rooms.Where(r => r.Nvm != null).Sum(r => r.Nvm.Triangles.Count),
                d.Files.Values.Sum(f => f.Entities.Count)));
            GUI.WrappedText(d.Mcp != null
                ? LOC.Get("MAP_Navigation_Nvm_Summary_Mcp", d.Mcp.Rooms.Count, d.Mcp.Rooms.Count(r => r.MapID == d.MapUid))
                : LOC.Get("MAP_Navigation_Nvm_Summary_No_Mcp"));
            GUI.WrappedText(d.Mcg != null
                ? LOC.Get("MAP_Navigation_Nvm_Summary_Mcg", d.Mcg.Nodes.Count, d.Mcg.Edges.Count, d.Mcg.Edges.Count(e => e.MapID == d.MapUid))
                : LOC.Get("MAP_Navigation_Nvm_Summary_No_Mcg"));
        }

        DisplayNvmLegend();
    }

    /// <summary>The flag combinations drawn, in their colours, with a button to paint with one.</summary>
    private void DisplayNvmLegend()
    {
        var legend = OverlayRenderer.FaceLegend;

        if (!NvmLayers.Contains(NavOverlayLayer.Faces) || legend.Count == 0 || !NvmShowOverlay)
            return;

        // Triangle Flags
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Nvm_Legend_Header"), LOC.Get("MAP_Navigation_Nvm_Legend_Header_TT"), ref NvmShowLegend);

        if (!NvmShowLegend)
            return;

        foreach (var (value, color, faces) in legend)
        {
            ImGui.ColorButton($"##navigationNvmLegend_{value:X4}", new Vector4(color, 1), ImGuiColorEditFlags.NoTooltip, new Vector2(14, 14) * DPI.UIScale());
            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Legend_Use")}##navigationNvmLegendUse_{value:X4}"))
            {
                NvmPaintFlags = (NVM.TriangleFlags)value;
                NvmPaintOp = PaintOp.Set;
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Legend_Use_TT"));
            ImGui.SameLine();
            ImGui.Text(LOC.Get("MAP_Navigation_Nvm_Legend_Entry", DescribeNvmFlags((NVM.TriangleFlags)value), faces));
        }
    }

    private static string DescribeNvmFlags(NVM.TriangleFlags flags)
        => flags == NVM.TriangleFlags.NONE ? LOC.Get("MAP_Navigation_Nvm_Flags_None") : string.Join(" | ", NvmFlagBits.Where(b => (flags & b) != 0));

    #endregion

    #region Flags tab

    private void DisplayNvmFlagsTab()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Flags_Actions_Header"), LOC.Get("MAP_Navigation_Nvm_Flags_Actions_Header_TT"));

        GUI.MultiButtonInput("navigationNvmFlagModes",
            "navigationNvmPick", NvmClickMode == NvmClick.Pick ? LOC.Get("MAP_Navigation_Nvm_Pick_Stop") : LOC.Get("MAP_Navigation_Nvm_Pick_Start"),
            LOC.Get("MAP_Navigation_Nvm_Pick_TT"), () => SetNvmClick(NvmClickMode == NvmClick.Pick ? NvmClick.None : NvmClick.Pick),

            "navigationNvmPaint", NvmClickMode == NvmClick.Paint ? LOC.Get("MAP_Navigation_Nvm_Paint_Stop") : LOC.Get("MAP_Navigation_Nvm_Paint_Start"),
            LOC.Get("MAP_Navigation_Nvm_Paint_TT"), () => SetNvmClick(NvmClickMode == NvmClick.Paint ? NvmClick.None : NvmClick.Paint));

        // Paint
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Paint_Header"), LOC.Get("MAP_Navigation_Nvm_Paint_Header_TT"));

        int op = (int)NvmPaintOp;
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Paint_Set")}##navigationNvmPaintSet", ref op, (int)PaintOp.Set);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Paint_Op_Set_TT"));
        ImGui.SameLine();
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Paint_Add")}##navigationNvmPaintAdd", ref op, (int)PaintOp.AddBits);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Paint_Op_Add_TT"));
        ImGui.SameLine();
        ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Paint_Remove")}##navigationNvmPaintRemove", ref op, (int)PaintOp.RemoveBits);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Paint_Op_Remove_TT"));
        NvmPaintOp = (PaintOp)op;

        foreach (var bit in NvmFlagBits)
        {
            bool on = (NvmPaintFlags & bit) != 0;
            if (ImGui.Checkbox($"{bit}##navigationNvmPaintBit_{bit}", ref on))
                NvmPaintFlags = on ? NvmPaintFlags | bit : NvmPaintFlags & ~bit;
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Nvm_Flag_{bit}_TT"));
        }

        DisplayNvmPickedTriangle();
        DisplayNvmLegend();
    }

    /// <summary>The picked triangle: its room, flags (editable), vertices, neighbours and entities.</summary>
    private void DisplayNvmPickedTriangle()
    {
        // Picked Triangle
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Picked_Header"), LOC.Get("MAP_Navigation_Nvm_Picked_Header_TT"));

        if (NvmData == null || PickedNvmRoom(NvmData) is not NvmRoom room)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_None"));
            return;
        }

        var nvm = room.Nvm;
        var tri = nvm.Triangles[NvmPickedTriangle];

        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_Room", room.Index, room.PartName, room.FileName));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_Vertices", tri.VertexIndex1, tri.VertexIndex2, tri.VertexIndex3));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_Neighbours", tri.EdgeIndex1, tri.EdgeIndex2, tri.EdgeIndex3));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_Obstacles", tri.ObstacleCount));

        var entities = nvm.Entities.Where(e => e.TriangleIndices.Contains(NvmPickedTriangle)).Select(e => e.EntityID.ToString()).ToList();
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_Entities", entities.Count == 0 ? "-" : string.Join(", ", entities)));

        // Its flags, each a checkbox that edits it (one undoable action)
        foreach (var bit in NvmFlagBits)
        {
            bool on = (tri.Flags & bit) != 0;
            if (ImGui.Checkbox($"{bit}##navigationNvmPickedBit_{bit}", ref on))
            {
                var flags = on ? tri.Flags | bit : tri.Flags & ~bit;
                ExecuteNvmFlags(new Dictionary<(int, int), NVM.TriangleFlags> { [(room.Index, NvmPickedTriangle)] = flags });
            }
            GUI.Tooltip(LOC.Get($"MAP_Navigation_Nvm_Flag_{bit}_TT"));
        }
    }

    private NVM.TriangleFlags NvmPainted(NVM.TriangleFlags before) => NvmPaintOp switch
    {
        PaintOp.Set => NvmPaintFlags,
        PaintOp.AddBits => before | NvmPaintFlags,
        PaintOp.RemoveBits => before & ~NvmPaintFlags,
        _ => before,
    };

    /// <summary>Records the painted flags as one action: the edited NVMs swapped for copies with the new flags.</summary>
    private void ExecuteNvmFlags(IReadOnlyDictionary<(int Room, int Triangle), NVM.TriangleFlags> changes)
    {
        var data = NvmData;
        if (data == null || NvmContainer == null || changes.Count == 0)
            return;

        var files = new List<NavigationNvmEditAction.FileChange>();

        foreach (var group in changes.GroupBy(c => data.Rooms[c.Key.Room].FileName))
        {
            var nvm = data.Files[group.Key];
            var flags = group.Where(c => nvm.Triangles[c.Key.Triangle].Flags != c.Value).ToDictionary(c => c.Key.Triangle, c => c.Value);

            if (flags.Count > 0)
                files.Add(new(group.Key, nvm, NvmEdit.WithFlags(nvm, flags)));
        }

        if (files.Count == 0)
            return;

        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(NvmContainer, Nvm, files, data.Mcp, data.Mcp, data.Mcg, data.Mcg, OnNvmEdited));
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Painted", changes.Count, files.Count));
    }

    #endregion

    #region Entities tab

    private void DisplayNvmEntitiesTab(MapContainer map)
    {
        var data = NvmData;
        var room = data != null ? PickedNvmRoom(data) : null;

        // Navmesh
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Entities_Room_Header"), LOC.Get("MAP_Navigation_Nvm_Entities_Room_Header_TT"));

        GUI.MultiButtonInput("navigationNvmEntityPick",
            "navigationNvmEntityPickButton", NvmClickMode == NvmClick.Pick ? LOC.Get("MAP_Navigation_Nvm_Pick_Stop") : LOC.Get("MAP_Navigation_Nvm_Pick_Start"),
            LOC.Get("MAP_Navigation_Nvm_Pick_TT"), () => SetNvmClick(NvmClickMode == NvmClick.Pick ? NvmClick.None : NvmClick.Pick));

        if (room == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Entities_Pick_First"));
            return;
        }

        var nvm = room.Nvm;
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Picked_Room", room.Index, room.PartName, room.FileName));

        if (NvmSelectedEntity >= nvm.Entities.Count)
            NvmSelectedEntity = -1;

        // Entities
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Entities_Header"), LOC.Get("MAP_Navigation_Nvm_Entities_Header_TT"));

        if (nvm.Entities.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Entities_None"));

        for (int e = 0; e < nvm.Entities.Count; e++)
        {
            var entity = nvm.Entities[e];
            if (ImGui.Selectable($"{LOC.Get("MAP_Navigation_Nvm_Entity_Row", entity.EntityID, entity.TriangleIndices.Count)}##navigationNvmEntity_{e}", e == NvmSelectedEntity))
            {
                NvmSelectedEntity = e == NvmSelectedEntity ? -1 : e;
                NvmOverlayDirty = true;
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Entity_Row_TT"));
        }

        // New Entity
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Entity_New_Header"), LOC.Get("MAP_Navigation_Nvm_Entity_New_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##navigationNvmNewEntityId", ref NvmNewEntityId);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Entity_New_Id_TT"));

        GUI.MultiButtonInput("navigationNvmEntityNew",
            "navigationNvmEntityNewButton", LOC.Get("MAP_Navigation_Nvm_Entity_New"), LOC.Get("MAP_Navigation_Nvm_Entity_New_TT"), () =>
            {
                ExecuteNvmEntity(room, nvm.Entities.Count, NvmNewEntityId, new[] { NvmPickedTriangle });
                NvmSelectedEntity = nvm.Entities.Count;
            });

        if (NvmSelectedEntity < 0)
            return;

        var selected = nvm.Entities[NvmSelectedEntity];

        // Selected Entity
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Entity_Selected_Header"), LOC.Get("MAP_Navigation_Nvm_Entity_Selected_Header_TT"));

        if (NvmEntityIdFor != NvmSelectedEntity)
        {
            NvmEntityIdText = selected.EntityID.ToString();
            NvmEntityIdFor = NvmSelectedEntity;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##navigationNvmEntityId", ref NvmEntityIdText, 16);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Entity_Id_TT"));

        if (ImGui.IsItemDeactivatedAfterEdit() && int.TryParse(NvmEntityIdText, out var id) && id != selected.EntityID)
            ExecuteNvmEntity(room, NvmSelectedEntity, id, selected.TriangleIndices);

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Nvm_Entity_Remove_Clicked")}##navigationNvmEntityRemoveClicked", ref NvmEntityRemove);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Entity_Remove_Clicked_TT"));

        bool editing = NvmClickMode == NvmClick.Entity;
        int index = NvmSelectedEntity;

        GUI.MultiButtonInput("navigationNvmEntityActions",
            "navigationNvmEntityEdit", editing ? LOC.Get("MAP_Navigation_Nvm_Entity_Edit_Stop") : LOC.Get("MAP_Navigation_Nvm_Entity_Edit_Start"),
            LOC.Get("MAP_Navigation_Nvm_Entity_Edit_TT"), () => SetNvmClick(editing ? NvmClick.None : NvmClick.Entity),

            "navigationNvmEntityDelete", LOC.Get("MAP_Navigation_Nvm_Entity_Delete"), LOC.Get("MAP_Navigation_Nvm_Entity_Delete_TT"), () =>
            {
                ExecuteNvmEntity(room, index, 0, null);
                NvmSelectedEntity = -1;
            });
    }

    /// <summary>Sets an entity's ID and triangles (null removes it, an index past the end adds one) as one action.</summary>
    private void ExecuteNvmEntity(NvmRoom room, int entity, int entityId, IReadOnlyCollection<int> triangles)
    {
        if (NvmContainer == null || room?.Nvm == null)
            return;

        var data = NvmData;
        var after = NvmEdit.WithEntity(room.Nvm, entity, entityId, triangles?.Where(t => t >= 0 && t < room.Nvm.Triangles.Count).ToList());
        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(NvmContainer, Nvm,
            new[] { new NavigationNvmEditAction.FileChange(room.FileName, room.Nvm, after) }, data.Mcp, data.Mcp, data.Mcg, data.Mcg, OnNvmEdited));
        NvmEntityIdFor = -1;
    }

    #endregion

    #region Graph tab

    private void DisplayNvmGraphTab(MapContainer map)
    {
        var data = NvmData;

        if (data == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Needs_Overlay"));
            return;
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Graph_Actions_Header"), LOC.Get("MAP_Navigation_Nvm_Graph_Actions_Header_TT"));

        bool hasNode = data.Mcg != null && NvmSelectedNode >= 0 && NvmSelectedNode < data.Mcg.Nodes.Count;

        GUI.ConditionalMultiButtonInput("navigationNvmGraphModes",
            "navigationNvmNodePick", NvmClickMode == NvmClick.Node ? LOC.Get("MAP_Navigation_Nvm_Node_Pick_Stop") : LOC.Get("MAP_Navigation_Nvm_Node_Pick_Start"),
            LOC.Get("MAP_Navigation_Nvm_Node_Pick_TT"), () => SetNvmClick(NvmClickMode == NvmClick.Node ? NvmClick.None : NvmClick.Node), data.Mcg != null,

            "navigationNvmConnect", NvmClickMode == NvmClick.Connect ? LOC.Get("MAP_Navigation_Nvm_Connect_Stop") : LOC.Get("MAP_Navigation_Nvm_Connect_Start"),
            LOC.Get("MAP_Navigation_Nvm_Connect_TT"), () => SetNvmClick(NvmClickMode == NvmClick.Connect ? NvmClick.None : NvmClick.Connect), hasNode,

            "navigationNvmRoomBoxes", LOC.Get("MAP_Navigation_Nvm_Refresh_Rooms"), LOC.Get("MAP_Navigation_Nvm_Refresh_Rooms_TT"),
            () => RefreshNvmRoomBoxes(data), data.Mcp != null);

        if (data.Mcg == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Summary_No_Mcg"));
            return;
        }

        DisplayNvmNodeActions(data);

        var mcg = data.Mcg;

        if (!hasNode)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Node_None"));
        }
        else
        {
            var node = mcg.Nodes[NvmSelectedNode];

            // Node
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Node_Header"), LOC.Get("MAP_Navigation_Nvm_Node_Header_TT"));

            if (NvmNodePositionFor != NvmSelectedNode)
            {
                NvmNodePositionText = node.Position;
                NvmNodePositionFor = NvmSelectedNode;
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputFloat3("##navigationNvmNodePosition", ref NvmNodePositionText);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Node_Position_TT"));

            if (ImGui.IsItemDeactivatedAfterEdit() && NvmNodePositionText != node.Position)
                ExecuteNvmGraph(NvmEdit.WithNodePosition(mcg, NvmSelectedNode, NvmNodePositionText));

            int nodeIndex = NvmSelectedNode;
            GUI.ConditionalMultiButtonInput("navigationNvmNodeActions",
                "navigationNvmNodeToPicked", LOC.Get("MAP_Navigation_Nvm_Node_To_Picked"), LOC.Get("MAP_Navigation_Nvm_Node_To_Picked_TT"),
                () => ExecuteNvmGraph(NvmEdit.WithNodePosition(mcg, nodeIndex, NvmPickedPoint)), PickedNvmRoom(data) != null,

                "navigationNvmNodeFrame", LOC.Get("MAP_Navigation_Nvm_Node_Frame"), LOC.Get("MAP_Navigation_Nvm_Node_Frame_TT"),
                () => Frame(new BoundingBox(node.Position, node.Position), 4f), true);

            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Node_Unknowns", node.Unk18, node.Unk1C));

            var on = NvmEdit.NodeTriangles(data, NvmSelectedNode);
            GUI.WrappedText(on.Count == 0 ? LOC.Get("MAP_Navigation_Nvm_Node_No_Gate")
                : LOC.Get("MAP_Navigation_Nvm_Node_Gates", string.Join(", ", on.Select(t => $"{t.Room.FileName}:{t.Triangle}"))));

            // Edges
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Edges_Header"), LOC.Get("MAP_Navigation_Nvm_Edges_Header_TT"));

            DisplayNvmEdges(data, node);
        }

        // Rooms
        if (data.Mcp != null)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Rooms_Header"), LOC.Get("MAP_Navigation_Nvm_Rooms_Header_TT"));

            if (PickedNvmRoom(data) is NvmRoom room && data.McpIndexOf(room) is int r && r >= 0)
            {
                var mcpRoom = data.Mcp.Rooms[r];
                GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Room_Box", r, room.PartName,
                    $"{mcpRoom.BoundingBoxMin.X:0.##}, {mcpRoom.BoundingBoxMin.Y:0.##}, {mcpRoom.BoundingBoxMin.Z:0.##}",
                    $"{mcpRoom.BoundingBoxMax.X:0.##}, {mcpRoom.BoundingBoxMax.Y:0.##}, {mcpRoom.BoundingBoxMax.Z:0.##}"));
                GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Room_Connections", mcpRoom.ConnectedRoomIndices.Count == 0 ? "-"
                    : string.Join(", ", mcpRoom.ConnectedRoomIndices.Select(c => data.RoomOfMcp(c)?.PartName ?? c.ToString()))));
            }
            else
            {
                GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Room_Pick"));
            }
        }
    }

    private void DisplayNvmEdges(NvmMap data, MCG.Node node)
    {
        var mcg = data.Mcg;

        if (node.ConnectedEdgeIndices.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Edges_None"));
            return;
        }

        if (!ImGui.BeginTable("##navigationNvmEdges", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
            return;

        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Edge_Column_Edge"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Edge_Column_Node"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Edge_Column_Room"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Edge_Column_Cost"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableHeadersRow();

        foreach (var e in node.ConnectedEdgeIndices.ToList())
        {
            if (e < 0 || e >= mcg.Edges.Count)
                continue;

            var edge = mcg.Edges[e];
            int other = edge.NodeIndexA == NvmSelectedNode ? edge.NodeIndexB : edge.NodeIndexA;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.Selectable($"{e}##navigationNvmEdgeSelect_{e}", e == NvmSelectedEdge))
            {
                NvmSelectedEdge = e == NvmSelectedEdge ? -1 : e;
                NvmOverlayDirty = true;
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Edge_Select_TT"));

            ImGui.TableNextColumn();
            if (ImGui.SmallButton($"{other}##navigationNvmEdgeGo_{e}"))
                SelectNvmNode(other);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Edge_Go_TT"));

            ImGui.TableNextColumn();
            ImGui.Text(data.RoomOfMcp(edge.MCPRoomIndex)?.PartName ?? LOC.Get("MAP_Navigation_Nvm_Edge_Other_Map", edge.MCPRoomIndex, edge.MapID.ToString("X8")));

            ImGui.TableNextColumn();
            float cost = edge.Unk20;
            ImGui.SetNextItemWidth(-1);
            ImGui.InputFloat($"##navigationNvmEdgeCost_{e}", ref cost);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Edge_Cost_TT", Vector3.Distance(mcg.Nodes[edge.NodeIndexA].Position, mcg.Nodes[edge.NodeIndexB].Position).ToString("0.##")));
            if (ImGui.IsItemDeactivatedAfterEdit() && cost != edge.Unk20)
                ExecuteNvmGraph(NvmEdit.WithEdgeCost(mcg, e, cost));

            ImGui.TableNextColumn();
            if (ImGui.Button($"{Icons.Times}##navigationNvmEdgeRemove_{e}"))
            {
                ExecuteNvmGraph(NvmEdit.WithoutEdge(mcg, e));
                NvmSelectedEdge = -1;
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Edge_Remove_TT"));
        }

        ImGui.EndTable();
    }

    private void SelectNvmNode(int node)
    {
        NvmSelectedNode = node;
        NvmSelectedEdge = -1;
        NvmNodePositionFor = -1;
        NvmOverlayDirty = true;
    }

    /// <summary>Puts a new MCG in place of the map's as one action.</summary>
    private void ExecuteNvmGraph(MCG after)
    {
        var data = NvmData;
        if (data == null || NvmContainer == null || after == null || ReferenceEquals(after, data.Mcg))
            return;

        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(NvmContainer, Nvm, null, data.Mcp, data.Mcp, data.Mcg, after, OnNvmEdited));
        NvmNodePositionFor = -1;
    }

    /// <summary>Connects the chosen node to a clicked one, in the room both nodes' gates are in (else the picked room).</summary>
    private void ConnectNvmNodes(NvmMap data, int other)
    {
        if (data.Mcg == null || other < 0 || other == NvmSelectedNode)
            return;

        var node = NvmSelectedNode;
        if (data.Mcg.Edges.Any(e => (e.NodeIndexA == node && e.NodeIndexB == other) || (e.NodeIndexA == other && e.NodeIndexB == node)))
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Connect_Exists", node, other), true);
            return;
        }

        var roomsA = NvmEdit.NodeTriangles(data, node).Select(t => t.Room.Index).ToHashSet();
        var shared = NvmEdit.NodeTriangles(data, other).Select(t => t.Room).FirstOrDefault(r => roomsA.Contains(r.Index)) ?? PickedNvmRoom(data);
        int mcpRoom = shared != null ? data.McpIndexOf(shared) : -1;

        if (mcpRoom < 0)
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Connect_No_Room"), true);
            return;
        }

        float cost = Vector3.Distance(data.Mcg.Nodes[node].Position, data.Mcg.Nodes[other].Position);
        var mcg = NvmEdit.WithEdge(data, data.Mcg, node, other, mcpRoom, cost);

        // The room is connected to every room its nodes' other edges are in, as the game's MCPs have it
        var mcp = NvmModelEdit.WithConnectionsOf(data, data.Mcp, mcg, mcpRoom);
        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(NvmContainer, Nvm, null, data.Mcp, mcp, data.Mcg, mcg, OnNvmEdited));
        NvmNodePositionFor = -1;
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Connected", node, other, shared.PartName));
    }

    /// <summary>Sets every own MCP room box from its NVM, as the game has them.</summary>
    private void RefreshNvmRoomBoxes(NvmMap data)
    {
        var after = NvmEdit.WithRoomBoxes(data, data.Mcp, data.Rooms.Where(r => r.Nvm != null).Select(r => (r, r.Nvm, r.Nvm)));

        if (ReferenceEquals(after, data.Mcp))
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Refresh_Rooms_None"));
            return;
        }

        int changed = data.Mcp.Rooms.Zip(after.Rooms).Count(p => p.First.BoundingBoxMin != p.Second.BoundingBoxMin || p.First.BoundingBoxMax != p.Second.BoundingBoxMax);
        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(NvmContainer, Nvm, null, data.Mcp, after, data.Mcg, data.Mcg, OnNvmEdited));
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Refresh_Rooms_Done", changed));
    }

    #endregion

    #region Check tab

    private void DisplayNvmCheckTab(MapContainer map)
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Actions_Header"), LOC.Get("MAP_Navigation_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("navigationNvmActions",
            "navigationNvmValidate", LOC.Get("MAP_Navigation_Validate"), LOC.Get("MAP_Navigation_Nvm_Validate_TT"),
            () => ValidateNvm(map), true,

            "navigationNvmClear", LOC.Get("MAP_Navigation_Clear"), LOC.Get("MAP_Navigation_Clear_TT"),
            ClearNvmResult, NvmCurrent != null);

        if (NvmCurrent == null || NvmCurrent.MapId != map.Name)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Not_Checked"));
            return;
        }

        if (NvmCurrent.Stale)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Check_Stale"));

        var issues = NvmCurrent.Issues;
        int newErrors = issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Error);
        int newWarnings = issues.Count(i => !i.Inherited && i.Severity == NavIssueSeverity.Warning);

        // Issues
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Issues_Header"), LOC.Get("MAP_Navigation_Issues_Header_TT"), ref ShowIssues);

        if (newErrors == 0 && newWarnings == 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_Navigation_Issues_None_New"));
        else
            GUI.WrappedTextColored(newErrors > 0 ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_Navigation_Issues_Counts", newErrors, newWarnings));

        if (!NvmCurrent.BaselineDone)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Baseline_Running"));
        else if (NvmCurrent.BaselineError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Baseline_Failed", NvmCurrent.BaselineError));
        else
            GUI.WrappedText(LOC.Get("MAP_Navigation_Baseline_Inherited", issues.Count(i => i.Inherited)));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Show_Inherited")}##navigationNvmShowInherited", ref ShowInherited);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Show_Inherited_TT"));

        if (!ShowIssues)
            return;

        var shown = issues.Where(i => ShowInherited || !i.Inherited).OrderBy(i => i.Inherited).ThenBy(i => i.Severity).ThenBy(i => i.File).ToList();

        if (shown.Count == 0 || !ImGui.BeginTable("##navigationNvmIssues", 3,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY,
            new Vector2(0, 300 * DPI.UIScale())))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Column_Severity"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Column_File"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Column_Issue"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var issue = shown[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                // A row on a room of this map goes to it
                if (issue.Room != null)
                {
                    ImGui.PushID(i);
                    if (ImGui.Selectable("##navigationNvmIssueGoTo", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowOverlap))
                        GoToNvmIssue(map, issue);
                    ImGui.PopID();
                    GUI.Tooltip(LOC.Get("MAP_Navigation_Issue_Go_To_TT"));
                    ImGui.SameLine();
                }

                var severity = LOC.Get(issue.Severity == NavIssueSeverity.Error ? "MAP_Navigation_Severity_Error" : "MAP_Navigation_Severity_Warning");
                if (issue.Inherited)
                    GUI.WrappedText($"{severity} ({LOC.Get("MAP_Navigation_Severity_Inherited")})");
                else
                    GUI.WrappedTextColored(issue.Severity == NavIssueSeverity.Error ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Invalid_Text_Color, severity);

                ImGui.TableNextColumn();
                ImGui.Text(issue.File);

                ImGui.TableNextColumn();
                GUI.WrappedText(LOC.Get($"MAP_Navigation_Issue_{issue.Code}", issue.Args));
            }
        }

        clipper.End();
        ImGui.EndTable();
    }

    private void ValidateNvm(MapContainer map)
    {
        ClearNvmResult();

        var data = Nvm.GetMap(map);
        if (data == null)
            return;

        var run = ++NvmRunCounter;
        NvmCurrent = new NvmCheckResult { MapId = map.Name, Run = run, Issues = NvmValidator.Validate(data) };

        NvmCancel = new CancellationTokenSource();
        var token = NvmCancel.Token;
        var mapId = map.Name;

        Task.Run(() =>
        {
            try
            {
                var vanilla = Nvm.GetVanillaIssues(mapId);
                if (!token.IsCancellationRequested)
                    NvmBaselineResults.Enqueue((run, vanilla, null));
            }
            catch (Exception e)
            {
                NvmBaselineResults.Enqueue((run, null, e.Message));
            }
        }, token);
    }

    private void ClearNvmResult()
    {
        NvmCancel?.Cancel();
        NvmCancel = null;
        NvmCurrent = null;
    }

    /// <summary>Picks the issue's triangle (or the room's first) and frames it, with the overlay on.</summary>
    private void GoToNvmIssue(MapContainer map, NavIssue issue)
    {
        if (!NvmShowOverlay || NvmContainer != map)
        {
            NvmShowOverlay = true;
            RebuildNvmOverlay(map);
        }

        var data = NvmData;
        if (data == null || issue.Room is not int r || r < 0 || r >= data.Rooms.Count || data.Rooms[r].Nvm is not NVM nvm || nvm.Triangles.Count == 0)
            return;

        var room = data.Rooms[r];
        int t = issue.Face is int f && f >= 0 && f < nvm.Triangles.Count ? f : 0;
        var (a, b, c) = NvmGeometry.Corners(nvm, t);
        var pa = Vector3.Transform(a, room.World);
        var pb = Vector3.Transform(b, room.World);
        var pc = Vector3.Transform(c, room.World);

        SetNvmPicked(r, t, (pa + pb + pc) / 3);

        if (issue.Face != null)
            Frame(new BoundingBox(Vector3.Min(pa, Vector3.Min(pb, pc)), Vector3.Max(pa, Vector3.Max(pb, pc))), 2f);
        else
        {
            var (min, max) = NvmGeometry.Placed(nvm.RootBox.MinValueCorner, nvm.RootBox.MaxValueCorner, room.World);
            Frame(new BoundingBox(min, max), 5f);
        }
    }

    #endregion

    #region Viewport

    private NvmRoom PickedNvmRoom(NvmMap data)
    {
        if (data == null || NvmPickedRoom < 0 || NvmPickedRoom >= data.Rooms.Count)
            return null;

        var room = data.Rooms[NvmPickedRoom];
        return room.Nvm != null && NvmPickedTriangle >= 0 && NvmPickedTriangle < room.Nvm.Triangles.Count ? room : null;
    }

    private void SetNvmPicked(int room, int triangle, Vector3 point)
    {
        bool roomChanged = room != NvmPickedRoom;
        NvmPickedRoom = room;
        NvmPickedTriangle = triangle;
        NvmPickedPoint = point;

        if (roomChanged)
        {
            NvmSelectedEntity = -1;
            if (NvmOnlyPickedRoom)
                NvmOverlayDirty = true;
        }

        DrawNvmHighlight();
    }

    private void DrawNvmHighlight()
    {
        HighlightRenderer.Clear();

        var data = NvmData;
        var room = data != null ? PickedNvmRoom(data) : null;
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (room == null || scene == null || !NvmShowOverlay)
            return;

        var overlay = NvmOverlayBuilder.Highlight(data, new NvmHit(room, NvmPickedTriangle, 0, NvmPickedPoint), NvmEditedVertices(room));

        // The triangles painted in the stroke under way, outlined with the picked one
        foreach (var ((r, t), _) in NvmStroke)
        {
            if (r < 0 || r >= data.Rooms.Count || data.Rooms[r].Nvm == null)
                continue;

            var stroke = NvmOverlayBuilder.Highlight(data, new NvmHit(data.Rooms[r], t, 0, default), NvmEditedVertices(data.Rooms[r]));
            overlay.Batch(NavOverlayLayer.Highlight).Lines.AddRange(stroke.Batch(NavOverlayLayer.Highlight).Lines);
        }

        HighlightRenderer.Show(scene, overlay, NvmOpacity);
    }

    private void ClearNvmOverlay()
    {
        OverlayRenderer.Clear();
        HighlightRenderer.Clear();
        NvmData = null;
        NvmContainer = null;
    }

    private void RebuildNvmOverlay(MapContainer map)
    {
        NvmOverlayDirty = false;
        NvmOverlayRevision = NvmVertexRevision;

        var data = Nvm.GetMap(map);
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (data == null || scene == null)
        {
            ClearNvmOverlay();
            return;
        }

        if (NvmContainer != map)
        {
            NvmPickedRoom = -1;
            NvmSelectedNode = -1;
            NvmSelectedEdge = -1;
            NvmSelectedEntity = -1;
        }

        NvmContainer = map;
        NvmData = data;

        var picked = PickedNvmRoom(data);
        Func<NvmRoom, bool> include = NvmOnlyPickedRoom && picked != null ? r => r.Index == picked.Index : null;
        var selection = new NvmOverlayBuilder.Selection(picked, NvmSelectedEntity, NvmSelectedNode, NvmSelectedEdge);

        var overlay = NvmOverlayBuilder.Build(data, NvmLayers, NvmEditedVertices, include, selection);
        OverlayRenderer.Show(scene, overlay, NvmOpacity);
        DrawNvmHighlight();
    }

    /// <summary>After an NVM edit or its undo: the overlay is drawn again, and the last check may be out of date.</summary>
    private void OnNvmEdited()
    {
        NvmOverlayDirty = true;
        NvmEntityIdFor = -1;
        NvmNodePositionFor = -1;

        if (NvmCurrent != null)
            NvmCurrent.Stale = true;
    }

    /// <summary>
    /// The NVM mode's part of HandleViewportInput: keeps the overlay up to date and takes the viewport's clicks while a
    /// mode is on. Returns true while the clicks are taken.
    /// </summary>
    private bool HandleNvmViewportInput(Ray ray, bool canInteract)
    {
        if (NvmContainer != null && NvmContainer.LoadState != MapContentLoadState.Loaded)
        {
            NvmStroke.Clear();
            ClearNvmOverlay();
            NvmShowOverlay = false;
        }

        OverlayRenderer.Update();
        HighlightRenderer.Update();

        // Generations finish in the background; their edits are made here, even while the tool window is closed
        DrainNvmGenerateResults();

        bool mouseDown = InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action);

        // A finished paint stroke is recorded as one action
        if (!mouseDown && NvmStroke.Count > 0)
        {
            var stroke = new Dictionary<(int, int), NVM.TriangleFlags>(NvmStroke);
            NvmStroke.Clear();
            ExecuteNvmFlags(stroke);
        }

        // Drawn again after an edit, and while vertices are brushed (once the last drawing has been shown)
        if (NvmShowOverlay && NvmContainer == null && LoadedMaps().FirstOrDefault(m => m.Name == SelectedMap) is MapContainer selected)
            RebuildNvmOverlay(selected);
        else if (NvmShowOverlay && NvmContainer != null && NvmStroke.Count == 0
            && (NvmOverlayDirty || (NvmOverlayRevision != NvmVertexRevision && !OverlayRenderer.IsUpdating)))
            RebuildNvmOverlay(NvmContainer);

        if (UpdateNvmVertexEdit(ray, canInteract))
            return true;

        if (NvmData == null || NvmClickMode == NvmClick.None)
            return false;

        bool pressed = canInteract && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action);

        switch (NvmClickMode)
        {
            case NvmClick.Pick:
                if (pressed && NvmOverlayBuilder.Pick(NvmData, ray.Origin, ray.Direction, NvmEditedVertices) is NvmHit pick)
                    SetNvmPicked(pick.Room.Index, pick.Triangle, pick.Point);
                break;

            case NvmClick.Paint:
                if (canInteract && mouseDown && NvmOverlayBuilder.Pick(NvmData, ray.Origin, ray.Direction, NvmEditedVertices) is NvmHit paint)
                {
                    var key = (paint.Room.Index, paint.Triangle);
                    if (!NvmStroke.ContainsKey(key))
                    {
                        NvmStroke[key] = NvmPainted(paint.Room.Nvm.Triangles[paint.Triangle].Flags);
                        SetNvmPicked(paint.Room.Index, paint.Triangle, paint.Point);
                    }
                }
                break;

            case NvmClick.Entity:
                if (pressed && NvmOverlayBuilder.Pick(NvmData, ray.Origin, ray.Direction, NvmEditedVertices) is NvmHit hit)
                {
                    var room = PickedNvmRoom(NvmData);
                    if (room == null || hit.Room.Index != room.Index || NvmSelectedEntity < 0 || NvmSelectedEntity >= room.Nvm.Entities.Count)
                    {
                        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Entity_Other_Room"), true);
                        break;
                    }

                    var entity = room.Nvm.Entities[NvmSelectedEntity];
                    var triangles = entity.TriangleIndices.ToList();
                    bool had = triangles.Contains(hit.Triangle);

                    if (NvmEntityRemove == had)
                    {
                        if (had) triangles.Remove(hit.Triangle); else triangles.Add(hit.Triangle);
                        ExecuteNvmEntity(room, NvmSelectedEntity, entity.EntityID, triangles);
                    }

                    NvmPickedTriangle = hit.Triangle;
                    NvmPickedPoint = hit.Point;
                }
                break;

            case NvmClick.Node:
                if (pressed)
                {
                    int node = NvmOverlayBuilder.PickNode(NvmData, ray.Origin, ray.Direction, NavigationOverlayBuilder.GateSize);
                    if (node >= 0)
                        SelectNvmNode(node);
                }
                break;

            case NvmClick.Connect:
                if (pressed)
                {
                    int node = NvmOverlayBuilder.PickNode(NvmData, ray.Origin, ray.Direction, NavigationOverlayBuilder.GateSize);
                    if (node >= 0)
                        ConnectNvmNodes(NvmData, node);
                }
                break;
        }

        return true;
    }

    #endregion
}
