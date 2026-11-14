using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Model Editor's EDGE Editor: the grapple points, hangable ledges and huggable walls of a Sekiro object's EDGE
/// file (in its objbnd, in the object's own space), with the feature set of the Map Editor's EDGE Editor (see
/// Claude/Documentation/EDGE Viewport Editing.md §15): live markers for the whole file, the entry list by chain, the
/// selected entry's fields, Duplicate/Delete, picking and moving points in the viewport, adding grapple points and
/// drawing ledges and walls, split/join/delete, creating a file for an object without one, and the checks with fixes.
/// It uses the same pure rules (<see cref="EdgeHandles"/>, <see cref="EdgeAuthoring"/>, <see cref="EdgeChecks"/>).
/// <para>
/// Every change is one <see cref="ModelEdgeEditAction"/> on the view's history, and the model's save writes the file.
/// A map EDGE file shown with a Sekiro map piece belongs to a collision and is edited in the Map Editor, so here it is
/// read-only: listed, checked and drawn, but not changed.
/// </para>
/// </summary>
public partial class ModelEdgeEditorTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    // The model shown and its selected entry
    private ModelWrapper SelectedWrapper;
    private int SelectedIndex = -1;

    // The fields of the selected entry as typed: taken from the file whenever no input is being edited, and written
    // back as one action when an input is left after a change
    private EDGE.Edge Buffer;

    // Asks the Tools window to bring the tool into view on its next draw (from the property grid's entry buttons)
    private bool FocusOnNextDraw;

    public ModelEdgeEditorTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        GizmoTarget = new ModelEdgeGizmoTarget(this);

        // Mesh edits change the surface points are placed on; this tool's own edits don't
        view.ViewportActionManager.ActionPerformed += () =>
        {
            if (!ExecutingEdgeAction)
                SurfaceStale = true;
        };
    }

    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.SDT;

    /// <summary>The loaded model of this view, if any.</summary>
    private ModelWrapper GetWrapper()
    {
        var wrapper = View.Selection.SelectedModelWrapper;

        return wrapper?.Container != null && wrapper.Container.View == View ? wrapper : null;
    }

    /// <summary>
    /// Carries the EDGE file's points into the viewport: the EDGE's own space (the collision's, for map EDGEs) and the
    /// model's display rotation.
    /// </summary>
    private static Matrix4x4 GetSpace(ModelWrapper wrapper)
    {
        var container = wrapper.Container;

        return container == null ? Matrix4x4.Identity : container.EdgeSpace * container.DisplayMatrix;
    }

    public void OnToolWindow()
    {
        if (FocusOnNextDraw)
        {
            FocusOnNextDraw = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("MODEL_EdgeEditor_Header")}##modelEdgeEditorHeader"))
            return;

        ImGui.BeginChild("modelEdgeEditorSection", ImGuiChildFlags.Borders);

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

        GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_Hint"));

        DisplayVisibility();
        DisplayAppearance();

        var wrapper = GetWrapper();

        if (wrapper != SelectedWrapper)
        {
            SelectedWrapper = wrapper;
            SelectedIndex = -1;
            Buffer = null;
            PickedKind = null;
            EndDrawing();
        }

        // File
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_EdgeEditor_File_Header"), LOC.Get("MODEL_EdgeEditor_File_Header_TT"));

        if (wrapper == null)
        {
            LastDrawnFrame = ImGui.GetFrameCount();
            GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_No_Model"));
            return;
        }

        var edge = wrapper.EDGE;
        var readOnly = !wrapper.CanEditEdge;

        if (edge == null)
        {
            LastDrawnFrame = ImGui.GetFrameCount();

            if (readOnly)
            {
                GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_Not_Object", wrapper.Name));
                return;
            }

            GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_No_File", wrapper.Name));
            DisplayCreateFile(wrapper);

            // Placing a point makes the file too
            DisplayViewportEditing(wrapper, null);
            return;
        }

        if (readOnly)
        {
            GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_Map_File", wrapper.EdgeCollisionModelName));
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_EdgeEditor_Read_Only"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_File_Info", wrapper.EdgeFileName, edge.ID));
        }

        var chains = edge.Edges.Where(e => e.Type != EDGE.EdgeType.Grapple).Select(e => (e.Type, e.Unk30)).Distinct().Count();
        var grapples = edge.Edges.Count(e => e.Type == EDGE.EdgeType.Grapple);

        GUI.WrappedText(LOC.Get("MAP_EdgeEditor_File_Info", edge.Edges.Count, chains, grapples));

        if (wrapper.IsEdgeFileNew)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MODEL_EdgeEditor_New_File"));
        }

        if (SelectedIndex >= edge.Edges.Count)
        {
            SelectedIndex = edge.Edges.Count - 1;
            Buffer = null;
        }

        if (!readOnly)
        {
            DisplayViewportEditing(wrapper, edge);
        }
        else
        {
            LastDrawnFrame = ImGui.GetFrameCount();
            Editing = false;
        }

        DisplayEntries(wrapper, edge);
        DisplayFields(wrapper, edge, readOnly);

        if (!readOnly)
        {
            DisplayActions(wrapper, edge);
        }

        DisplayChecks(wrapper, edge, readOnly);
    }

    private void DisplayVisibility()
    {
        if (CFG.Current.ModelEditor_DisplayEdgePoints)
            return;

        // Visibility
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Visibility_Header"), LOC.Get("MAP_EdgeEditor_Visibility_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Points_Hidden"));

        GUI.MultiButtonInput("modelEdgeEditorVisibility",
            "modelEdgeEditorShowPoints", LOC.Get("MAP_EdgeEditor_Show_Points"), LOC.Get("MODEL_EdgeEditor_Show_Points_TT"),
            () => CFG.Current.ModelEditor_DisplayEdgePoints = true);
    }

    // Whether the Display group is open; closed by default so the entries stay near the top
    private bool ShowAppearance = false;

    /// <summary>
    /// The colours of the markers, which double as the legend, and their sizes: the same settings as the Map Editor's
    /// EDGE Editor. The markers follow a change by themselves.
    /// </summary>
    private void DisplayAppearance()
    {
        // Display
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_EdgeEditor_Display_Header"), LOC.Get("MAP_EdgeEditor_Display_Header_TT"), ref ShowAppearance);

        if (!ShowAppearance)
            return;

        ColorEntry("modelEdgeColorGrapple", "MAP_EdgeEditor_Color_Grapple", ref CFG.Current.Viewport_Edge_Grapple_Color);
        ColorEntry("modelEdgeColorPull", "MAP_EdgeEditor_Color_Pull", ref CFG.Current.Viewport_Edge_Pull_Color);
        ColorEntry("modelEdgeColorHang", "MAP_EdgeEditor_Color_Hang", ref CFG.Current.Viewport_Edge_Hang_Color);
        ColorEntry("modelEdgeColorHug", "MAP_EdgeEditor_Color_Hug", ref CFG.Current.Viewport_Edge_Hug_Color);
        ColorEntry("modelEdgeColorSelected", "MAP_EdgeEditor_Color_Selected", ref CFG.Current.Viewport_Edge_Selected_Color);
        ColorEntry("modelEdgeColorHover", "MAP_EdgeEditor_Color_Hover", ref CFG.Current.Viewport_Edge_Hover_Color);

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##modelEdgePointSize", ref CFG.Current.Viewport_Edge_Point_Size, 0.02f, 1.0f,
            LOC.Get("MAP_EdgeEditor_Point_Size_Format"));
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Point_Size_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##modelEdgeSegmentRadius", ref CFG.Current.Viewport_Edge_Segment_Radius, 0.01f, 0.5f,
            LOC.Get("MAP_EdgeEditor_Segment_Radius_Format"));
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Segment_Radius_TT"));
    }

    /// <summary>A colour of the legend (0-255 per channel), editable.</summary>
    private static void ColorEntry(string id, string key, ref Vector3 color)
    {
        var value = color / 255.0f;

        ImGui.ColorEdit3($"{LOC.Get(key)}##{id}", ref value, ImGuiColorEditFlags.NoInputs);
        GUI.Tooltip(LOC.Get($"{key}_TT"));

        color = value * 255.0f;
    }

    #region Selection from elsewhere

    /// <summary>
    /// Selects an entry of the loaded model's EDGE file and frames it, and brings the tool into view (the property
    /// grid's button on an EDGE entry).
    /// </summary>
    public void SelectAndFrame(int index)
    {
        var wrapper = GetWrapper();

        if (wrapper?.EDGE == null || index < 0 || index >= wrapper.EDGE.Edges.Count)
            return;

        CFG.Current.Interface_ModelEditor_Tool_EdgeEditor = true;
        CFG.Current.ModelEditor_DisplayEdgePoints = true;
        FocusOnNextDraw = true;

        SelectedWrapper = wrapper;
        SelectEntry(index);
        FrameEntry(wrapper, wrapper.EDGE.Edges[index]);
    }

    /// <summary>
    /// Frames an entry's points in the viewport: both ends of a segment, or a grapple point with its pull point.
    /// The box is padded by the marker size so a lone grapple point isn't framed as a zero-size box.
    /// </summary>
    private void FrameEntry(ModelWrapper wrapper, EDGE.Edge entry)
    {
        var viewport = View.ViewportWindow?.Viewport;
        if (viewport == null)
            return;

        var space = GetSpace(wrapper);
        var points = new List<Vector3> { entry.V1, entry.V2 };
        if (entry.Type == EDGE.EdgeType.Grapple && entry.V3 != Vector3.Zero)
            points.Add(entry.V3);

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var p in points)
        {
            var w = Vector3.Transform(p, space);
            min = Vector3.Min(min, w);
            max = Vector3.Max(max, w);
        }

        var pad = new Vector3(Math.Max(CFG.Current.Viewport_Edge_Point_Size, 0.25f));
        viewport.FrameBox(new BoundingBox(min - pad, max + pad), CFG.Current.Viewport_Frame_Offset, CFG.Current.Viewport_Frame_Distance);
    }

    #endregion

    #region Entries

    private void DisplayEntries(ModelWrapper wrapper, EDGE edge)
    {
        // Entries
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Entries_Header"), LOC.Get("MAP_EdgeEditor_Entries_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_EdgeEditor_Group_By_Chain")}##modelEdgeEditorGroup", ref GroupByChain);
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Group_By_Chain_TT"));

        var flagged = GetIssues(edge).Select(x => x.Entry).ToHashSet();
        var height = Math.Min(edge.Edges.Count + (GroupByChain ? 4 : 0), 12) * ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;

        ImGui.BeginChild("modelEdgeEditorEntries", new Vector2(-1, Math.Max(height, ImGui.GetTextLineHeightWithSpacing() * 2)), ImGuiChildFlags.Borders);

        if (GroupByChain)
        {
            DisplayEntryGroups(wrapper, edge, flagged);
        }
        else
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(edge.Edges.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    EntryRow(wrapper, edge, i, flagged);
                }
            }

            clipper.End();
        }

        ImGui.EndChild();
    }

    // Whether the entries are listed under their chains (and the grapples together) rather than in file order
    private bool GroupByChain = true;

    private int LastOpenedFor = -1;

    /// <summary>
    /// The entries under a node per chain, by type then chain ID, in file order within each, and the grapples under one.
    /// The selected entry's node opens by itself when the selection changes.
    /// </summary>
    private void DisplayEntryGroups(ModelWrapper wrapper, EDGE edge, HashSet<int> flagged)
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

            if (ImGui.TreeNodeEx($"{label}##modelEdgeGroup{group.Key.Type}{group.Key.Chain}", ImGuiTreeNodeFlags.SpanAvailWidth))
            {
                foreach (var i in indices)
                {
                    EntryRow(wrapper, edge, i, flagged);
                }

                ImGui.TreePop();
            }
        }

        LastOpenedFor = SelectedIndex;
    }

    private void EntryRow(ModelWrapper wrapper, EDGE edge, int i, HashSet<int> flagged)
    {
        var entry = edge.Edges[i];
        var label = entry.Type == EDGE.EdgeType.Grapple
            ? LOC.Get("MAP_EdgeEditor_Entry_Grapple", i, entry.VariationID)
            : LOC.Get("MAP_EdgeEditor_Entry_Segment", i, entry.Type, entry.Unk30, entry.VariationID);

        if (flagged.Contains(i))
            label += LOC.Get("MAP_EdgeEditor_Flag");

        if (ImGui.Selectable($"{label}##modelEdgeEntry{i}", i == SelectedIndex))
        {
            SelectEntry(i);
        }

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            FrameEntry(wrapper, entry);
        }
    }

    private void SelectEntry(int index)
    {
        if (SelectedIndex != index)
        {
            SelectedIndex = index;
            Buffer = null;
        }
    }

    #endregion

    #region Fields

    private void DisplayFields(ModelWrapper wrapper, EDGE edge, bool readOnly)
    {
        if (SelectedIndex < 0 || SelectedIndex >= edge.Edges.Count)
            return;

        // Taken from the file again unless an input is being typed in, so undo, redo and grid edits show
        if (Buffer == null || !ImGui.IsAnyItemActive())
        {
            Buffer = edge.Edges[SelectedIndex].Clone();
        }

        // Entry
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Entry_Header", SelectedIndex), LOC.Get("MAP_EdgeEditor_Entry_Header_TT"));

        void Commit(string field)
        {
            var after = EdgeAuthoring.Copy(edge);
            after.Edges[SelectedIndex] = Buffer.Clone();

            Execute(wrapper, after, LOC.Get("MAP_History_Edge_Field", field, wrapper.Name));
        }

        ImGui.BeginDisabled(readOnly);

        // Type
        var types = Enum.GetValues<EDGE.EdgeType>();

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##modelEdgeEditorType", LOC.Get($"MAP_EdgeEditor_Type_{Buffer.Type}")))
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

        VectorField(isGrapple ? "MAP_EdgeEditor_Field_Grapple_Start" : "MAP_EdgeEditor_Field_Start",
            isGrapple ? "MODEL_EdgeEditor_Field_Grapple_Start_TT" : "MODEL_EdgeEditor_Field_Start_TT", "modelEdgeV1",
            () => Buffer.V1, v => Buffer.V1 = v, Commit);

        VectorField(isGrapple ? "MAP_EdgeEditor_Field_Grapple_End" : "MAP_EdgeEditor_Field_End",
            isGrapple ? "MODEL_EdgeEditor_Field_Grapple_End_TT" : "MODEL_EdgeEditor_Field_End_TT", "modelEdgeV2",
            () => Buffer.V2, v => Buffer.V2 = v, Commit);

        if (isGrapple)
        {
            VectorField("MAP_EdgeEditor_Field_Pull", "MODEL_EdgeEditor_Field_Pull_TT", "modelEdgeV3",
                () => Buffer.V3, v => Buffer.V3 = v, Commit);

            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("MAP_EdgeEditor_Field_HasPull"));

            var hasPull = Buffer.Unk2C;
            ImGui.SetNextItemWidth(-1);
            ImGui.InputFloat($"##modelEdgeUnk2C", ref hasPull);
            Buffer.Unk2C = hasPull;
            GUI.Tooltip(LOC.Get("MODEL_EdgeEditor_Field_HasPull_TT"));
            if (ImGui.IsItemDeactivatedAfterEdit())
                Commit(LOC.Get("MAP_EdgeEditor_Field_HasPull"));
        }

        ByteOrIntField(isGrapple ? "MAP_EdgeEditor_Field_Group" : "MAP_EdgeEditor_Field_Chain", "modelEdgeUnk30",
            () => Buffer.Unk30, v => Buffer.Unk30 = v, int.MinValue, int.MaxValue, Commit);

        ByteOrIntField("MAP_EdgeEditor_Field_Variation", "modelEdgeVariation",
            () => Buffer.VariationID, v => Buffer.VariationID = (byte)v, 0, 255, Commit);

        if (isGrapple)
        {
            DisplayWireVariation(Buffer.VariationID);
        }

        ByteOrIntField("MAP_EdgeEditor_Field_Unk36", "modelEdgeUnk36",
            () => Buffer.Unk36, v => Buffer.Unk36 = (byte)v, 0, 255, Commit);

        ImGui.EndDisabled();
    }

    /// <summary>
    /// The WireVariationParam row a grapple's Variation ID names. Object grapples mostly use the base rows (1 in 171
    /// of 255); the character rows (10700 and up) are for grapples on enemies.
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

    private static void VectorField(string key, string tooltipKey, string id, Func<Vector3> get, Action<Vector3> set, Action<string> commit)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get(key));

        var value = get();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3($"##{id}", ref value);
        set(value);
        GUI.Tooltip(LOC.Get(tooltipKey));

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

    #endregion

    #region Actions

    private void DisplayActions(ModelWrapper wrapper, EDGE edge)
    {
        var hasSelection = SelectedIndex >= 0 && SelectedIndex < edge.Edges.Count;
        var index = SelectedIndex;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Actions_Header"), LOC.Get("MAP_EdgeEditor_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("modelEdgeEditorActions",
            "modelEdgeEditorDuplicate", LOC.Get("MAP_EdgeEditor_Duplicate"), LOC.Get("MAP_EdgeEditor_Duplicate_TT"),
            () =>
            {
                var after = EdgeAuthoring.Copy(edge);
                after.Edges.Insert(index + 1, edge.Edges[index].Clone());

                Execute(wrapper, after, LOC.Get("MAP_History_Edge_Duplicate", index, wrapper.Name));
                SelectEntry(index + 1);
            }, hasSelection,

            "modelEdgeEditorDelete", LOC.Get("MAP_EdgeEditor_Delete"), LOC.Get("MAP_EdgeEditor_Delete_TT"),
            () =>
            {
                var after = EdgeAuthoring.Copy(edge);
                after.Edges.RemoveAt(index);

                Execute(wrapper, after, LOC.Get("MAP_History_Edge_Delete", index, wrapper.Name));
                SelectEntry(Math.Min(index, after.Edges.Count - 1));
                PickedKind = null;
            }, hasSelection);

        DisplayAuthoringActions(wrapper, edge);

        GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_Save_Note"));
    }

    /// <summary>An object without an EDGE file: make an empty one, with the object's number as its ID.</summary>
    private void DisplayCreateFile(ModelWrapper wrapper)
    {
        var file = EdgeAuthoring.CreateFile(wrapper.Name);

        GUI.ConditionalMultiButtonInput("modelEdgeEditorCreateFile",
            "modelEdgeEditorCreate", LOC.Get("MAP_EdgeEditor_Create_File"),
            LOC.Get("MODEL_EdgeEditor_Create_File_TT", EdgeAuthoring.ObjectDefaults.FileName(wrapper.Name)),
            () =>
            {
                Execute(wrapper, EdgeAuthoring.CreateFile(wrapper.Name), LOC.Get("MAP_History_Edge_Create_File", wrapper.Name));
            }, file != null);
    }

    // Set while this tool's own action runs, so it doesn't count as a mesh edit
    private bool ExecutingEdgeAction;

    private void Execute(ModelWrapper wrapper, EDGE after, string description)
    {
        ExecutingEdgeAction = true;

        try
        {
            View.ViewportActionManager.ExecuteAction(new ModelEdgeEditAction(wrapper, after, description));
        }
        finally
        {
            ExecutingEdgeAction = false;
        }
    }

    #endregion

    #region Checks

    // The checks of the file, kept until its contents change
    private (int Key, List<EdgeIssue> Issues) IssueCache = (0, null);

    private List<EdgeIssue> GetIssues(EDGE edge)
    {
        var key = ModelEdgeMarkers.ContentFingerprint(edge);

        if (IssueCache.Issues != null && IssueCache.Key == key)
            return IssueCache.Issues;

        var issues = EdgeChecks.Check(edge);
        IssueCache = (key, issues);

        return issues;
    }

    /// <summary>
    /// The checks of the file (EdgeChecks): each issue selects its entry when clicked and frames it when double-clicked,
    /// and can be fixed as one action; Fix All fixes every one as one action. A read-only file lists them only.
    /// </summary>
    private void DisplayChecks(ModelWrapper wrapper, EDGE edge, bool readOnly)
    {
        var issues = GetIssues(edge);

        // Checks
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Checks_Header"), LOC.Get("MODEL_EdgeEditor_Checks_Header_TT"));

        if (issues.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_EdgeEditor_Checks_None"));
            return;
        }

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_EdgeEditor_Checks_Count", issues.Count));

        var up = EdgeUp(wrapper);
        var direction = ObjectGrappleDirection(up, null);

        for (var n = 0; n < issues.Count; n++)
        {
            var issue = issues[n];

            if (!readOnly)
            {
                if (ImGui.Button($"{LOC.Get("MAP_EdgeEditor_Fix")}##modelEdgeFix{n}"))
                {
                    // A grapple's missing direction is put sideways, as object grapples point
                    var after = EdgeChecks.Fix(edge, issue, issue.Kind == EdgeIssueKind.GrappleNoDirection ? direction : up);

                    if (after != null)
                    {
                        Execute(wrapper, after, LOC.Get("MAP_History_Edge_Fix", LOC.Get($"MAP_EdgeEditor_Issue_{issue.Kind}"), wrapper.Name));
                        SelectEntry(Math.Min(issue.Entry, after.Edges.Count - 1));
                        PickedKind = null;
                    }
                }
                GUI.Tooltip(LOC.Get(issue.Kind == EdgeIssueKind.GrappleNoDirection
                    ? "MODEL_EdgeEditor_Fix_GrappleNoDirection_TT"
                    : $"MAP_EdgeEditor_Fix_{issue.Kind}_TT"));

                ImGui.SameLine();
            }

            var text = LOC.Get($"MAP_EdgeEditor_Issue_{issue.Kind}", issue.Entry, issue.Detail);

            if (ImGui.Selectable($"{text}##modelEdgeIssue{n}", issue.Entry == SelectedIndex))
            {
                SelectEntry(issue.Entry);
            }
            GUI.Tooltip(LOC.Get($"MAP_EdgeEditor_Issue_{issue.Kind}_TT"));

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && issue.Entry < edge.Edges.Count)
            {
                FrameEntry(wrapper, edge.Edges[issue.Entry]);
            }
        }

        if (readOnly)
            return;

        GUI.MultiButtonInput("modelEdgeEditorFixAll",
            "modelEdgeEditorFixAllButton", LOC.Get("MAP_EdgeEditor_Fix_All"), LOC.Get("MAP_EdgeEditor_Fix_All_TT"),
            () =>
            {
                // Grapples without a direction get the sideways one first, then the rest as in the Map Editor
                var start = edge;
                var count = 0;

                foreach (var issue in issues.Where(i => i.Kind == EdgeIssueKind.GrappleNoDirection))
                {
                    var fixedOne = EdgeChecks.Fix(start, issue, direction);
                    if (fixedOne != null)
                    {
                        start = fixedOne;
                        count++;
                    }
                }

                var after = EdgeChecks.FixAll(start, up, out var rest);
                count += rest;

                if (count > 0)
                {
                    Execute(wrapper, after, LOC.Get("MAP_History_Edge_Fix_All", count, wrapper.Name));
                    SelectedIndex = Math.Min(SelectedIndex, after.Edges.Count - 1);
                    Buffer = null;
                    PickedKind = null;
                }
            });
    }

    #endregion
}
