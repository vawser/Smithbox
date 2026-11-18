#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Linq;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The Face Check tool: runs <see cref="FaceCheck"/> when asked, and lists what it found by kind, each a link to the
/// row. A missing face can be made at the ID characters point at, as a copy of the shown face. The list is kept until
/// the next run; rows deleted since are skipped.
/// </summary>
public class FaceGenCheckTool
{
    private readonly FaceGenEditorView View;

    private FaceCheck.Result? Result;

    public FaceGenCheckTool(FaceGenEditorView view)
    {
        View = view;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FACEGEN_Tool_Face_Check")}##faceGenCheckToolHeader"))
            return;

        ImGui.BeginChild("FaceGenCheckToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("FACEGEN_Check_Hint"));

        if (ImGui.Button($"{LOC.Get("FACEGEN_Check_Run")}##faceGenCheckRun", new System.Numerics.Vector2(-1, 0)))
            Result = FaceCheck.Run(View.Project);

        if (Result is not { } result)
            return;

        GUI.Spacer();

        if (result.Issues.Count == 0)
            ImGui.TextDisabled(LOC.Get("FACEGEN_Check_Clean"));

        Section(result, FaceIssueKind.MissingFace, "FACEGEN_Check_Missing", "FACEGEN_Check_Missing_TT", true);
        Section(result, FaceIssueKind.UnknownPart, "FACEGEN_Check_Unknown_Part", "FACEGEN_Check_Unknown_Part_TT", true);
        Section(result, FaceIssueKind.HiddenSlider, "FACEGEN_Check_Hidden_Slider", "FACEGEN_Check_Hidden_Slider_TT", false);

        // Faces no one wears: a count, and the list's filter
        GUI.Spacer();
        ImGui.TextDisabled(LOC.Get("FACEGEN_Check_Unused", result.UnusedFaces, result.Faces));

        if (result.UnusedFaces > 0)
        {
            ImGui.SameLine();

            if (ImGui.SmallButton($"{LOC.Get("FACEGEN_Check_Show_Unused")}##faceGenCheckShowUnused"))
            {
                if (FaceGenParams.GetReferencedFaceParam(View.Project) is { } referenced)
                    View.ShowParam(referenced);

                View.ListView.ShowUnused();
            }
        }
    }

    private void Section(FaceCheck.Result result, FaceIssueKind kind, string titleKey, string tooltipKey, bool isError)
    {
        var issues = result.Issues.Where(i => i.Kind == kind).ToList();

        if (issues.Count == 0)
            return;

        // Errors in the warning colour; notes as plain headers
        if (isError)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Warning_Text_Color);

        var open = ImGui.CollapsingHeader($"{LOC.Get(titleKey, issues.Count)}##faceGenCheck{kind}");

        if (isError)
            ImGui.PopStyleColor();

        GUI.Tooltip(LOC.Get(tooltipKey));

        if (!open)
            return;

        var index = 0;

        foreach (var issue in issues)
        {
            ImGui.PushID($"faceGenIssue{kind}{index++}");

            var param = FaceGenParams.GetParam(View.Project, issue.ParamName);

            // Deleted since the check ran
            if (param == null || param.IndexOfRow(issue.Row) < 0)
            {
                ImGui.PopID();
                continue;
            }

            var label = $"{issue.ParamName}: {FaceGenParams.RowLabel(issue.Row.ID, issue.Row.Name)}  {issue.Detail}";

            // A missing face made since the check ran
            if (kind == FaceIssueKind.MissingFace && FaceGenParams.GetReferencedFaceParam(View.Project) is { } faceParamName &&
                FaceGenParams.GetParam(View.Project, faceParamName)?[issue.FaceId] != null)
            {
                label += $"  {LOC.Get("FACEGEN_Check_Fixed")}";
            }

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
            var clicked = ImGui.Selectable(label);
            ImGui.PopStyleColor();

            if (clicked)
            {
                // A face is shown here; a character row in the Param Editor
                if (kind == FaceIssueKind.MissingFace)
                    EditorCommandQueue.AddCommand($"param/select/-1/{issue.ParamName}/{issue.Row.ID}");
                else
                    View.Select(issue.ParamName, issue.Row);
            }

            GUI.Tooltip(LOC.Get(kind == FaceIssueKind.MissingFace ? "FACEGEN_Check_Missing_Link_TT" : "FACEGEN_Check_Show_Face_TT"));

            if (kind == FaceIssueKind.MissingFace && ImGui.BeginPopupContextItem("faceGenIssueContext"))
            {
                CreateFaceItem(issue);
                ImGui.EndPopup();
            }

            ImGui.PopID();
        }
    }

    /// <summary>"Create face N from the shown face": a copy at the ID the character points at, then shown</summary>
    private void CreateFaceItem(FaceIssue issue)
    {
        var editor = View.Editor.ParamEditor;
        var referenced = FaceGenParams.GetReferencedFaceParam(View.Project);
        var faceParam = referenced != null ? FaceGenParams.GetParam(View.Project, referenced) : null;
        var source = View.ParamName == referenced ? View.ActiveRow : null;

        var canCreate = editor != null && faceParam != null && source != null && faceParam[issue.FaceId] == null;

        var label = source != null
            ? LOC.Get("FACEGEN_Check_Create_Face", issue.FaceId, FaceGenParams.RowLabel(source.ID, source.Name))
            : LOC.Get("FACEGEN_Check_Create_Face_No_Source", issue.FaceId);

        if (ImGui.Selectable($"{label}##faceGenCreateFace", false, canCreate ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            var created = FaceGenEdits.AddCopies(editor!, faceParam!, source!, 1, new[] { issue.FaceId }, issue.Row.Name);

            if (created.Count > 0)
                View.Select(referenced!, created[0]);
        }

        GUI.Tooltip(LOC.Get("FACEGEN_Check_Create_Face_TT"));

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
            EditorCommandQueue.AddCommand($"param/select/-1/{issue.ParamName}/{issue.Row.ID}");
    }
}
