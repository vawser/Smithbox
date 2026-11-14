using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Edits the EDGE files of Sekiro collisions (grapple points, hangable ledges, huggable walls), which the map's
/// edgebhd holds one of per collision. The points are relative to the collision part. Each change swaps the
/// collision's whole file through an <see cref="EdgeEditAction"/>, and the map's save writes the edgebhd.
///
/// Claude/Documentation/EDGE Viewport Editing.md: the entries of the selected collision part, their fields, Duplicate
/// and Delete (M1), and picking and moving their points in the viewport (M3, EdgeEditorTool.Viewport).
/// </summary>
public partial class EdgeEditorTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    // The entry shown, by collision part and index
    private MsbEntity SelectedPart;
    private int SelectedIndex = -1;

    // The fields of the selected entry as typed: taken from the file whenever no input is being edited, and written
    // back as one action when an input is left after a change
    private EDGE.Edge Buffer;

    public EdgeEditorTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        GizmoTarget = new EdgeGizmoTarget(this);
    }

    public bool CanUse() => View.EdgeBank.CanUse();

    public void OnToolWindow()
    {
        ImGui.BeginChild("edgeEditorSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        if (!CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Unsupported_Game"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Hint"));

        DisplayVisibility();
        DisplayAppearance();

        // Shown without a collision too: a click on any displayed point picks its collision
        DisplayViewportEditing();

        var part = GetCurrentPart();

        // Collision
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Collision_Header"), LOC.Get("MAP_EdgeEditor_Collision_Header_TT"));

        if (part == null || part.Container is not MapContainer map)
        {
            GUI.WrappedText(LOC.Get(Editing ? "MAP_EdgeEditor_Click_Point" : "MAP_EdgeEditor_Select_Collision"));
            return;
        }

        if (part != SelectedPart)
        {
            SelectedPart = part;
            SelectedIndex = -1;
            Buffer = null;
        }

        var mapId = map.Name;
        var modelName = part.GetPartModelName();

        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Select_Collision"));
            return;
        }

        if (!View.EdgeBank.EnsureMapLoaded(mapId))
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_No_Binder", mapId));
            return;
        }

        var edge = View.EdgeBank.GetEdge(mapId, modelName);
        var fileName = EdgeBank.GetFileKey(mapId, modelName);

        GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Collision_Info", part.Name, modelName, fileName));

        if (edge == null)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_No_File", modelName));
            DisplayCreateFile(map, modelName);
            return;
        }

        var chains = edge.Edges.Where(e => e.Type != EDGE.EdgeType.Grapple).Select(e => (e.Type, e.Unk30)).Distinct().Count();
        var grapples = edge.Edges.Count(e => e.Type == EDGE.EdgeType.Grapple);

        GUI.WrappedText(LOC.Get("MAP_EdgeEditor_File_Info", edge.Edges.Count, chains, grapples));

        if (View.EdgeBank.IsModified(mapId, modelName))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_EdgeEditor_Modified"));
        }

        if (SelectedIndex >= edge.Edges.Count)
        {
            SelectedIndex = edge.Edges.Count - 1;
            Buffer = null;
        }

        // Drawn as selected for as long as the tool shows it
        View.EdgeBank.SetHighlight(mapId, modelName, SelectedIndex);

        DisplayPicked();
        DisplayEntries(part, edge);
        DisplayFields(map, modelName, edge);
        DisplayActions(part, map, modelName, edge);
        DisplayChecks(part, map, modelName, edge);
    }

    // The collision the tool edits: the last one whose point was clicked while editing in the viewport, or the last
    // collision part selected in the Map Editor, whichever happened last
    private MsbEntity ActivePart;
    private MsbEntity LastSelectedCollision;

    /// <summary>
    /// The collision part the tool edits. Selecting another collision part in the Map Editor makes it the one; so does
    /// clicking one of a collision's points while editing in the viewport (see <see cref="SetActivePart"/>).
    /// </summary>
    private MsbEntity GetCurrentPart()
    {
        var selected = GetSelectedCollision();

        if (selected != LastSelectedCollision)
        {
            LastSelectedCollision = selected;

            if (selected != null)
            {
                ActivePart = selected;
            }
        }

        // A part that has left its map, or whose map was unloaded
        if (ActivePart != null && (ActivePart.Parent == null || ActivePart.Container is not MapContainer))
        {
            ActivePart = null;
        }

        return ActivePart ?? selected;
    }

    /// <summary>Makes a collision part the one the tool edits, keeping the entry chosen for it.</summary>
    private void SetActivePart(MsbEntity part)
    {
        if (part == ActivePart && part == SelectedPart)
            return;

        ActivePart = part;
        SelectedPart = part;
        SelectedIndex = -1;
        Buffer = null;
    }

    /// <summary>
    /// The one collision part selected in the viewport or the map's contents, if any.
    /// </summary>
    private MsbEntity GetSelectedCollision()
    {
        var selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>(e => EntityHelper.IsPartCollision(e));

        return selection.Count == 1 ? selection.First() : null;
    }

    private void DisplayVisibility()
    {
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (scene == null || scene.DrawFilter.HasFlag(RenderFilter.EdgePoint))
            return;

        // Visibility
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Visibility_Header"), LOC.Get("MAP_EdgeEditor_Visibility_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Points_Hidden"));

        GUI.MultiButtonInput("edgeEditorVisibility",
            "edgeEditorShowPoints", LOC.Get("MAP_EdgeEditor_Show_Points"), LOC.Get("MAP_EdgeEditor_Show_Points_TT"),
            () =>
            {
                scene.SetDrawFilter(RenderFilter.EdgePoint, true);
                CFG.Current.MapEditor_BasicFilter_EdgePoint = true;
            });
    }

    // Whether the Display group is open; closed by default so the entries stay near the top
    private bool ShowAppearance = false;

    /// <summary>
    /// The colours of the markers, which double as the legend, and their sizes. A change rebuilds every part's points.
    /// </summary>
    private void DisplayAppearance()
    {
        // Display
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_EdgeEditor_Display_Header"), LOC.Get("MAP_EdgeEditor_Display_Header_TT"), ref ShowAppearance);

        if (!ShowAppearance)
            return;

        var changed = false;

        changed |= ColorEntry("edgeColorGrapple", "MAP_EdgeEditor_Color_Grapple", ref CFG.Current.Viewport_Edge_Grapple_Color);
        changed |= ColorEntry("edgeColorPull", "MAP_EdgeEditor_Color_Pull", ref CFG.Current.Viewport_Edge_Pull_Color);
        changed |= ColorEntry("edgeColorHang", "MAP_EdgeEditor_Color_Hang", ref CFG.Current.Viewport_Edge_Hang_Color);
        changed |= ColorEntry("edgeColorHug", "MAP_EdgeEditor_Color_Hug", ref CFG.Current.Viewport_Edge_Hug_Color);
        changed |= ColorEntry("edgeColorSelected", "MAP_EdgeEditor_Color_Selected", ref CFG.Current.Viewport_Edge_Selected_Color);
        ColorEntry("edgeColorHover", "MAP_EdgeEditor_Color_Hover", ref CFG.Current.Viewport_Edge_Hover_Color);

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##edgePointSize", ref CFG.Current.Viewport_Edge_Point_Size, 0.02f, 1.0f,
            LOC.Get("MAP_EdgeEditor_Point_Size_Format"));
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Point_Size_TT"));
        changed |= ImGui.IsItemDeactivatedAfterEdit();

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##edgeSegmentRadius", ref CFG.Current.Viewport_Edge_Segment_Radius, 0.01f, 0.5f,
            LOC.Get("MAP_EdgeEditor_Segment_Radius_Format"));
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Segment_Radius_TT"));
        changed |= ImGui.IsItemDeactivatedAfterEdit();

        if (changed)
        {
            View.EdgeBank.RebuildAll();
        }
    }

    /// <summary>A colour of the legend (0-255 per channel), editable. True once a change is finished.</summary>
    private static bool ColorEntry(string id, string key, ref Vector3 color)
    {
        var value = color / 255.0f;

        ImGui.ColorEdit3($"{LOC.Get(key)}##{id}", ref value, ImGuiColorEditFlags.NoInputs);
        GUI.Tooltip(LOC.Get($"{key}_TT"));

        color = value * 255.0f;

        return ImGui.IsItemDeactivatedAfterEdit();
    }

    /// <summary>
    /// Frames an entry's points in the viewport: both ends of a segment, or a grapple point with its pull point.
    /// The box is padded by the marker size so a lone grapple point isn't framed as a zero-size box.
    /// </summary>
    private void FrameEntry(MsbEntity part, EDGE.Edge entry)
    {
        var viewport = View.ViewportHandler.ActiveViewport?.Viewport;
        if (viewport == null)
            return;

        var world = part.GetWorldMatrix();
        var points = new List<Vector3> { entry.V1, entry.V2 };
        if (entry.Type == EDGE.EdgeType.Grapple && entry.V3 != Vector3.Zero)
            points.Add(entry.V3);

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var p in points)
        {
            var w = Vector3.Transform(p, world);
            min = Vector3.Min(min, w);
            max = Vector3.Max(max, w);
        }

        var pad = new Vector3(Math.Max(CFG.Current.Viewport_Edge_Point_Size, 0.5f));
        viewport.FrameBox(new BoundingBox(min - pad, max + pad), CFG.Current.Viewport_Frame_Offset, CFG.Current.Viewport_Frame_Distance);
    }

    private void DisplayEntries(MsbEntity part, EDGE edge)
    {
        // Entries
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Entries_Header"), LOC.Get("MAP_EdgeEditor_Entries_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_EdgeEditor_Group_By_Chain")}##edgeEditorGroup", ref GroupByChain);
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Group_By_Chain_TT"));

        var flagged = GetIssues(part, edge).Select(x => x.Entry).ToHashSet();
        var height = Math.Min(edge.Edges.Count + (GroupByChain ? 4 : 0), 12) * ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;

        ImGui.BeginChild("edgeEditorEntries", new Vector2(-1, Math.Max(height, ImGui.GetTextLineHeightWithSpacing() * 2)), ImGuiChildFlags.Borders);

        if (GroupByChain)
        {
            DisplayEntryGroups(part, edge, flagged);
        }
        else
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(edge.Edges.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    EntryRow(part, edge, i, flagged);
                }
            }

            clipper.End();
        }

        ImGui.EndChild();
    }

    // Whether the entries are listed under their chains (and the grapples together) rather than in file order
    private bool GroupByChain = true;

    /// <summary>
    /// The entries under a node per chain, by type then chain ID, in file order within each, and the grapples under one.
    /// The selected entry's node opens by itself when the selection changes.
    /// </summary>
    private void DisplayEntryGroups(MsbEntity part, EDGE edge, HashSet<int> flagged)
    {
        var groups = Enumerable.Range(0, edge.Edges.Count)
            .GroupBy(i => (edge.Edges[i].Type, Chain: edge.Edges[i].Type == EDGE.EdgeType.Grapple ? 0 : edge.Edges[i].Unk30))
            .OrderBy(g => g.Key.Type).ThenBy(g => g.Key.Chain);

        foreach (var group in groups)
        {
            var indices = group.ToList();
            var label = group.Key.Type == EDGE.EdgeType.Grapple
                ? LOC.Get("MAP_EdgeEditor_Group_Grapples", indices.Count)
                : LOC.Get("MAP_EdgeEditor_Group_Chain", LOC.Get($"MAP_EdgeEditor_Type_{group.Key.Type}"), group.Key.Chain, indices.Count);

            if (indices.Any(flagged.Contains))
                label += LOC.Get("MAP_EdgeEditor_Flag");

            if (SelectedIndex != LastOpenedFor && indices.Contains(SelectedIndex))
            {
                ImGui.SetNextItemOpen(true);
            }

            if (ImGui.TreeNodeEx($"{label}##edgeGroup{group.Key.Type}{group.Key.Chain}", ImGuiTreeNodeFlags.SpanAvailWidth))
            {
                foreach (var i in indices)
                {
                    EntryRow(part, edge, i, flagged);
                }

                ImGui.TreePop();
            }
        }

        LastOpenedFor = SelectedIndex;
    }

    private int LastOpenedFor = -1;

    private void EntryRow(MsbEntity part, EDGE edge, int i, HashSet<int> flagged)
    {
        var entry = edge.Edges[i];
        var label = entry.Type == EDGE.EdgeType.Grapple
            ? LOC.Get("MAP_EdgeEditor_Entry_Grapple", i, entry.VariationID)
            : LOC.Get("MAP_EdgeEditor_Entry_Segment", i, entry.Type, entry.Unk30, entry.VariationID);

        if (flagged.Contains(i))
            label += LOC.Get("MAP_EdgeEditor_Flag");

        if (ImGui.Selectable($"{label}##edgeEntry{i}", i == SelectedIndex))
        {
            SelectedIndex = i;
            Buffer = null;
        }

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            FrameEntry(part, entry);
        }
    }

    // The checks of each collision's file, kept until the file's version changes
    private readonly Dictionary<string, (int Version, List<EdgeIssue> Issues)> IssueCache = new(StringComparer.OrdinalIgnoreCase);

    private List<EdgeIssue> GetIssues(MsbEntity part, EDGE edge)
    {
        if (part?.Container is not MapContainer map)
            return EdgeChecks.Check(edge);

        var modelName = part.GetPartModelName();
        var key = EdgeBank.GetFileKey(map.Name, modelName);
        var version = View.EdgeBank.GetVersion(map.Name, modelName);

        if (IssueCache.TryGetValue(key, out var cached) && cached.Version == version)
            return cached.Issues;

        var issues = EdgeChecks.Check(edge);
        IssueCache[key] = (version, issues);

        return issues;
    }

    /// <summary>
    /// The checks of the file (EdgeChecks): each issue selects its entry when clicked and frames it when double-clicked,
    /// and can be fixed as one action; Fix All fixes every one as one action.
    /// </summary>
    private void DisplayChecks(MsbEntity part, MapContainer map, string modelName, EDGE edge)
    {
        var issues = GetIssues(part, edge);

        // Checks
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Checks_Header"), LOC.Get("MAP_EdgeEditor_Checks_Header_TT"));

        if (issues.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_EdgeEditor_Checks_None"));
            return;
        }

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_EdgeEditor_Checks_Count", issues.Count));

        var up = PartUp(part);

        for (var n = 0; n < issues.Count; n++)
        {
            var issue = issues[n];

            if (ImGui.Button($"{LOC.Get("MAP_EdgeEditor_Fix")}##edgeFix{n}"))
            {
                var after = EdgeChecks.Fix(edge, issue, up);

                if (after != null)
                {
                    Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Fix", LOC.Get($"MAP_EdgeEditor_Issue_{issue.Kind}"), modelName));
                    SelectEntry(Math.Min(issue.Entry, after.Edges.Count - 1));
                    PickedKind = null;
                }
            }
            GUI.Tooltip(LOC.Get($"MAP_EdgeEditor_Fix_{issue.Kind}_TT"));

            ImGui.SameLine();

            var text = LOC.Get($"MAP_EdgeEditor_Issue_{issue.Kind}", issue.Entry, issue.Detail);

            if (ImGui.Selectable($"{text}##edgeIssue{n}", issue.Entry == SelectedIndex))
            {
                SelectEntry(issue.Entry);
            }
            GUI.Tooltip(LOC.Get($"MAP_EdgeEditor_Issue_{issue.Kind}_TT"));

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && issue.Entry < edge.Edges.Count)
            {
                FrameEntry(part, edge.Edges[issue.Entry]);
            }
        }

        GUI.MultiButtonInput("edgeEditorFixAll",
            "edgeEditorFixAllButton", LOC.Get("MAP_EdgeEditor_Fix_All"), LOC.Get("MAP_EdgeEditor_Fix_All_TT"),
            () =>
            {
                var after = EdgeChecks.FixAll(edge, up, out var count);

                if (count > 0)
                {
                    Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Fix_All", count, modelName));
                    SelectedIndex = Math.Min(SelectedIndex, after.Edges.Count - 1);
                    Buffer = null;
                    PickedKind = null;
                }
            });
    }

    private void DisplayFields(MapContainer map, string modelName, EDGE edge)
    {
        if (SelectedIndex < 0 || SelectedIndex >= edge.Edges.Count)
            return;

        // Taken from the file again unless an input is being typed in, so undo and redo show
        if (Buffer == null || !ImGui.IsAnyItemActive())
        {
            Buffer = edge.Edges[SelectedIndex].Clone();
        }

        // Entry
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Entry_Header", SelectedIndex), LOC.Get("MAP_EdgeEditor_Entry_Header_TT"));

        void Commit(string field)
        {
            var after = EdgeBank.Copy(edge);
            after.Edges[SelectedIndex] = Buffer.Clone();

            Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Field", field, modelName));
        }

        // Type
        var types = Enum.GetValues<EDGE.EdgeType>();
        var typeIndex = Array.IndexOf(types, Buffer.Type);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##edgeEditorType", LOC.Get($"MAP_EdgeEditor_Type_{Buffer.Type}")))
        {
            foreach (var type in types)
            {
                if (ImGui.Selectable(LOC.Get($"MAP_EdgeEditor_Type_{type}"), type == Buffer.Type) && type != Buffer.Type)
                {
                    Buffer.Type = type;
                    Commit(LOC.Get("MAP_EdgeEditor_Field_Type"));
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Field_Type_TT"));

        var isGrapple = Buffer.Type == EDGE.EdgeType.Grapple;

        VectorField(isGrapple ? "MAP_EdgeEditor_Field_Grapple_Start" : "MAP_EdgeEditor_Field_Start", "edgeV1",
            () => Buffer.V1, v => Buffer.V1 = v, Commit);

        VectorField(isGrapple ? "MAP_EdgeEditor_Field_Grapple_End" : "MAP_EdgeEditor_Field_End", "edgeV2",
            () => Buffer.V2, v => Buffer.V2 = v, Commit);

        if (isGrapple)
        {
            VectorField("MAP_EdgeEditor_Field_Pull", "edgeV3", () => Buffer.V3, v => Buffer.V3 = v, Commit);

            var hasPull = Buffer.Unk2C;
            ImGui.SetNextItemWidth(-1);
            ImGui.InputFloat($"##edgeUnk2C", ref hasPull);
            Buffer.Unk2C = hasPull;
            GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Field_HasPull_TT"));
            if (ImGui.IsItemDeactivatedAfterEdit())
                Commit(LOC.Get("MAP_EdgeEditor_Field_HasPull"));
        }

        ByteOrIntField(isGrapple ? "MAP_EdgeEditor_Field_Group" : "MAP_EdgeEditor_Field_Chain", "edgeUnk30",
            () => Buffer.Unk30, v => Buffer.Unk30 = v, int.MinValue, int.MaxValue, Commit);

        ByteOrIntField("MAP_EdgeEditor_Field_Variation", "edgeVariation",
            () => Buffer.VariationID, v => Buffer.VariationID = (byte)v, 0, 255, Commit);

        if (isGrapple)
        {
            DisplayWireVariation(Buffer.VariationID);
        }

        ByteOrIntField("MAP_EdgeEditor_Field_Unk36", "edgeUnk36",
            () => Buffer.Unk36, v => Buffer.Unk36 = (byte)v, 0, 255, Commit);
    }

    /// <summary>
    /// The WireVariationParam row a grapple's Variation ID names. Map grapples use the base rows directly (0, 1, 20,
    /// 40, 45 in the game's files); the character rows (10700 and up) are for grapples on enemies.
    /// </summary>
    private void DisplayWireVariation(int id)
    {
        if (Project.Handler.ParamData?.PrimaryBank?.Params is not { } bank
            || !bank.TryGetValue("WireVariationParam", out var param))
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Variation_No_Params"));
            return;
        }

        var row = param.Rows.FirstOrDefault(r => r.ID == id);

        if (row == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_EdgeEditor_Variation_Missing", id));
            return;
        }

        GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color,
            LOC.Get("MAP_EdgeEditor_Variation_Row", string.IsNullOrEmpty(row.Name) ? id.ToString() : row.Name));
    }

    private static void VectorField(string key, string id, Func<Vector3> get, Action<Vector3> set, Action<string> commit)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get(key));

        var value = get();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3($"##{id}", ref value);
        set(value);
        GUI.Tooltip(LOC.Get($"{key}_TT"));

        if (ImGui.IsItemDeactivatedAfterEdit())
            commit(LOC.Get(key));
    }

    private static void ByteOrIntField(string key, string id, Func<int> get, Action<int> set, int min, int max, Action<string> commit)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get(key));

        var value = get();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt($"##{id}", ref value);
        set(Math.Clamp(value, min, max));
        GUI.Tooltip(LOC.Get($"{key}_TT"));

        if (ImGui.IsItemDeactivatedAfterEdit())
            commit(LOC.Get(key));
    }

    private void DisplayActions(MsbEntity part, MapContainer map, string modelName, EDGE edge)
    {
        var hasSelection = SelectedIndex >= 0 && SelectedIndex < edge.Edges.Count;
        var index = SelectedIndex;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Actions_Header"), LOC.Get("MAP_EdgeEditor_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("edgeEditorActions",
            "edgeEditorDuplicate", LOC.Get("MAP_EdgeEditor_Duplicate"), LOC.Get("MAP_EdgeEditor_Duplicate_TT"),
            () =>
            {
                var after = EdgeBank.Copy(edge);
                after.Edges.Insert(index + 1, edge.Edges[index].Clone());

                Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Duplicate", index, modelName));
                SelectedIndex = index + 1;
                Buffer = null;
            }, hasSelection,

            "edgeEditorDelete", LOC.Get("MAP_EdgeEditor_Delete"), LOC.Get("MAP_EdgeEditor_Delete_TT"),
            () =>
            {
                var after = EdgeBank.Copy(edge);
                after.Edges.RemoveAt(index);

                Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Delete", index, modelName));
                SelectedIndex = Math.Min(index, after.Edges.Count - 1);
                Buffer = null;
            }, hasSelection);

        DisplayAuthoringActions(part, map, modelName, edge);

        GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Save_Note"));
    }

    private void Execute(MapContainer map, string modelName, EDGE before, EDGE after, string description)
    {
        View.ViewportActionManager.ExecuteAction(
            new EdgeEditAction(View.EdgeBank, map, modelName, before, after, description));
    }
}
