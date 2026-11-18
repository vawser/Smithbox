#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// "Assign to Characters": gives a face to character rows (their <see cref="FaceGenParams.FaceReferenceField"/>), so a
/// new face can be worn without leaving for the Param Editor. Rows are ticked in a filtered list and written only by
/// Apply, as one undo step.
/// </summary>
public class FaceAssignDialog
{
    private const string PopupId = "faceGenAssignPopup";

    private readonly FaceGenEditorView View;

    private bool OpenRequested;
    private int FaceId;
    private string? SourceParam;
    private string Filter = "";
    private bool OnlyWithFace = true;
    private readonly HashSet<int> Ticked = new();

    public FaceAssignDialog(FaceGenEditorView view)
    {
        View = view;
    }

    /// <summary>Opens the dialog for a face, on the next draw</summary>
    public void Open(int faceId)
    {
        FaceId = faceId;
        Ticked.Clear();
        Filter = "";
        OpenRequested = true;
    }

    /// <summary>The params whose rows can wear a face: every param with the reference field</summary>
    private List<string> SourceParams()
    {
        var paramList = FaceGenParams.GetParams(View.Project);

        if (paramList == null)
            return new List<string>();

        return paramList
            .Where(kv => kv.Value.AppliedParamdef?.Fields.Any(f => f.InternalName == FaceGenParams.FaceReferenceField) == true)
            .Select(kv => kv.Key)
            .OrderBy(k => k == "CharaInitParam" ? 0 : 1)
            .ThenBy(k => k)
            .ToList();
    }

    /// <summary>Whether the dialog was open on the last draw (the list's shortcuts stand aside while it is)</summary>
    public bool IsOpen { get; private set; }

    public void Display()
    {
        if (OpenRequested)
        {
            ImGui.OpenPopup(PopupId);
            OpenRequested = false;
        }

        ImGui.SetNextWindowSize(new Vector2(520, 560) * DPI.UIScale(), ImGuiCond.Appearing);

        var open = true;

        IsOpen = ImGui.BeginPopupModal($"{LOC.Get("FACEGEN_Assign_Title", FaceId)}###{PopupId}", ref open);

        if (!IsOpen)
            return;

        var editor = View.Editor.ParamEditor;
        var sources = SourceParams();

        if (SourceParam == null || !sources.Contains(SourceParam))
            SourceParam = sources.FirstOrDefault();

        if (editor == null || SourceParam == null || FaceGenParams.GetParam(View.Project, SourceParam) is not { } param)
        {
            GUI.WrappedText(LOC.Get("FACEGEN_Assign_No_Params"));

            if (ImGui.Button($"{LOC.Get("FACEGEN_Assign_Cancel")}##faceGenAssignCancelEmpty"))
                ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("FACEGEN_Assign_Hint", FaceId));

        if (sources.Count > 1)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo("##faceGenAssignParam", SourceParam))
            {
                foreach (var name in sources)
                {
                    if (ImGui.Selectable($"{name}##faceGenAssignParam{name}", name == SourceParam))
                    {
                        SourceParam = name;
                        Ticked.Clear();
                    }
                }

                ImGui.EndCombo();
            }
        }

        // A live filter: it only narrows the list, so nothing is applied by it
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##faceGenAssignFilter", LOC.Get("FACEGEN_List_Filter_Hint"), ref Filter, 256);

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Assign_Only_With_Face")}##faceGenAssignOnlyWithFace", ref OnlyWithFace);
        GUI.Tooltip(LOC.Get("FACEGEN_Assign_Only_With_Face_TT"));

        var rows = param.Rows.Where(r => Matches(r)).ToList();

        if (ImGui.SmallButton($"{LOC.Get("FACEGEN_Assign_Tick_Shown", rows.Count)}##faceGenAssignTickShown"))
            Ticked.UnionWith(rows.Select(r => r.ID));

        ImGui.SameLine();

        if (ImGui.SmallButton($"{LOC.Get("FACEGEN_Assign_Untick_All")}##faceGenAssignUntick"))
            Ticked.Clear();

        var footer = ImGui.GetFrameHeightWithSpacing() * 2;

        if (ImGui.BeginChild("faceGenAssignRows", new Vector2(0, -footer), ImGuiChildFlags.Borders))
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(rows.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var row = rows[i];
                    var ticked = Ticked.Contains(row.ID);
                    var current = FaceOf(row);

                    if (ImGui.Checkbox($"{FaceGenParams.RowLabel(row.ID, row.Name)}##faceGenAssignRow{i}", ref ticked))
                    {
                        if (ticked)
                            Ticked.Add(row.ID);
                        else
                            Ticked.Remove(row.ID);
                    }

                    // The face the row wears now
                    var text = current == FaceId ? LOC.Get("FACEGEN_Assign_Wears_This") : LOC.Get("FACEGEN_Assign_Wears", current);
                    ImGui.SameLine();
                    var room = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0f, room));
                    ImGui.TextDisabled(text);
                }
            }

            clipper.End();
        }

        ImGui.EndChild();

        // Apply: the ticked rows that exist (row IDs can repeat; every row of a ticked ID is set)
        var targets = param.Rows.Where(r => Ticked.Contains(r.ID)).ToList();

        ImGui.BeginDisabled(targets.Count == 0);

        if (ImGui.Button($"{LOC.Get("FACEGEN_Assign_Apply", targets.Count)}##faceGenAssignApply"))
        {
            FaceGenEdits.SetFieldsOnRows(editor, SourceParam, targets,
                new (string, object)[] { (FaceGenParams.FaceReferenceField, FaceId) },
                LOC.Get("FACEGEN_Action_Assign", FaceId, targets.Count));

            ImGui.CloseCurrentPopup();
        }

        ImGui.EndDisabled();

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("FACEGEN_Assign_Cancel")}##faceGenAssignCancel"))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private int FaceOf(Param.Row row) =>
        row[FaceGenParams.FaceReferenceField]?.Value is IConvertible v ? v.ToInt32(null) : -1;

    private bool Matches(Param.Row row)
    {
        if (OnlyWithFace && FaceOf(row) < 0)
            return false;

        var filter = Filter.Trim();

        return filter.Length == 0 ||
               row.ID.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase) ||
               (row.Name ?? "").Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}
