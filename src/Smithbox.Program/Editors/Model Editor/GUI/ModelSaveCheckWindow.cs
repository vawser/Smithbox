using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Save Check window: what <see cref="ModelCheck" /> found in the models a save was about to write. It isn't
/// modal, so problems can be fixed in the editor while it is open, then checked again. Opened by File › Save and
/// Save All when an issue stops the save, and by File › Check Model Now. Like the Map Editor's Save Check.
/// </summary>
public class ModelSaveCheckWindow
{
    private readonly ModelEditorScreen Editor;
    private readonly ProjectEntry Project;

    private bool IsOpen;
    private bool FocusOnOpen;

    private List<ModelCheckIssue> Issues = new();

    // The models that were checked, so Check Again looks at the same ones
    private List<ModelWrapper> Models = new();

    // The save that was stopped, run again without the check by Save Anyway
    private Action SaveWithoutCheck;

    // Warnings and information can be hidden to see the errors. Vanilla models share dummy reference IDs (over
    // 15,000 times in 150 ER characters), so information starts hidden.
    private bool ShowWarnings = true;
    private bool ShowInfo = false;

    // The rows shown, rebuilt when the issues or the toggles change
    private List<ModelCheckIssue> Shown = new();
    private (List<ModelCheckIssue> Issues, bool ShowWarnings, bool ShowInfo) ShownFor;

    public ModelSaveCheckWindow(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <param name="saveWithoutCheck">The save to run from the window; null when nothing was being saved</param>
    public void Open(List<ModelCheckIssue> issues, List<ModelWrapper> models, Action saveWithoutCheck)
    {
        Issues = issues;
        Models = models;
        SaveWithoutCheck = saveWithoutCheck;
        IsOpen = true;
        FocusOnOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    private void CheckAgain()
    {
        Issues = ModelCheck.Run(Project, Models);
    }

    private bool StopsSave => ModelCheck.StopsSave(Issues, CFG.Current.ModelEditor_SaveCheck_Stop_On_Warnings);

    public void Display()
    {
        if (!IsOpen)
            return;

        ImGui.SetNextWindowSize(new Vector2(900, 500) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (FocusOnOpen)
        {
            ImGui.SetNextWindowFocus();
            FocusOnOpen = false;
        }

        if (ImGui.Begin($"{LOC.Get("MODEL_SaveCheck_Window_Title")}###modelSaveCheckWindow", ref IsOpen, ImGuiWindowFlags.NoDocking))
        {
            GUI.WrappedText(LOC.Get("MODEL_SaveCheck_Hint"));

            DisplaySummary();
            DisplayActions();
            DisplayList();
        }

        ImGui.End();
    }

    private void DisplaySummary()
    {
        var errors = Issues.Count(i => i.Severity is ModelCheckSeverity.Error);
        var warnings = Issues.Count(i => i.Severity is ModelCheckSeverity.Warning);
        var info = Issues.Count(i => i.Severity is ModelCheckSeverity.Info);
        var models = Issues.Select(i => i.Model).Distinct().Count();

        // Summary
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_SaveCheck_Summary_Header"), LOC.Get("MODEL_SaveCheck_Summary_Header_TT"));

        if (Issues.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_SaveCheck_Summary_None", Models.Count));
        }
        else
        {
            var color = errors > 0 ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Default_Text_Color;
            GUI.WrappedTextColored(color, LOC.Get("MODEL_SaveCheck_Summary", errors, warnings, info, models));

            if (!StopsSave)
            {
                GUI.WrappedText(LOC.Get("MODEL_SaveCheck_Summary_Can_Save"));
            }
        }
    }

    private void DisplayActions()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_SaveCheck_Actions_Header"), LOC.Get("MODEL_SaveCheck_Actions_Header_TT"));

        // Opened by Check Model Now: there is no save to continue
        if (SaveWithoutCheck == null)
        {
            GUI.MultiButtonInput("modelSaveCheckActions",
                "modelSaveCheckAgain", LOC.Get("MODEL_SaveCheck_Check_Again"), LOC.Get("MODEL_SaveCheck_Check_Again_TT"),
                CheckAgain,

                "modelSaveCheckClose", LOC.Get("MODEL_SaveCheck_Close"), LOC.Get("MODEL_SaveCheck_Close_TT"),
                () => IsOpen = false);

            return;
        }

        // Save is offered as Save Anyway while something would stop it
        var saveTitle = StopsSave ? LOC.Get("MODEL_SaveCheck_Save_Anyway") : LOC.Get("MODEL_SaveCheck_Save");
        var saveTooltip = StopsSave ? LOC.Get("MODEL_SaveCheck_Save_Anyway_TT") : LOC.Get("MODEL_SaveCheck_Save_TT");

        GUI.MultiButtonInput("modelSaveCheckActions",
            "modelSaveCheckAgain", LOC.Get("MODEL_SaveCheck_Check_Again"), LOC.Get("MODEL_SaveCheck_Check_Again_TT"),
            CheckAgain,

            "modelSaveCheckSave", saveTitle, saveTooltip,
            () =>
            {
                IsOpen = false;
                SaveWithoutCheck?.Invoke();
            },

            "modelSaveCheckClose", LOC.Get("MODEL_SaveCheck_Close"), LOC.Get("MODEL_SaveCheck_Close_TT"),
            () => IsOpen = false);
    }

    private void DisplayList()
    {
        if (Issues.Count == 0)
            return;

        // Problems
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_SaveCheck_List_Header"), LOC.Get("MODEL_SaveCheck_List_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_SaveCheck_Show_Warnings")}##modelSaveCheckShowWarnings", ref ShowWarnings);
        GUI.Tooltip(LOC.Get("MODEL_SaveCheck_Show_Warnings_TT"));

        ImGui.SameLine();

        ImGui.Checkbox($"{LOC.Get("MODEL_SaveCheck_Show_Info")}##modelSaveCheckShowInfo", ref ShowInfo);
        GUI.Tooltip(LOC.Get("MODEL_SaveCheck_Show_Info_TT"));

        if (!ReferenceEquals(ShownFor.Issues, Issues) || ShownFor.ShowWarnings != ShowWarnings || ShownFor.ShowInfo != ShowInfo)
        {
            Shown = Issues.Where(i => i.Severity switch
            {
                ModelCheckSeverity.Warning => ShowWarnings,
                ModelCheckSeverity.Info => ShowInfo,
                _ => true
            }).ToList();

            ShownFor = (Issues, ShowWarnings, ShowInfo);
        }

        ImGui.BeginChild("modelSaveCheckList", ImGuiChildFlags.Borders);

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.ScrollY;

        if (ImGui.BeginTable("modelSaveCheckTable", 4, flags))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MODEL_SaveCheck_Column_Severity"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MODEL_SaveCheck_Column_Model"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MODEL_SaveCheck_Column_Object"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn(LOC.Get("MODEL_SaveCheck_Column_Problem"), ImGuiTableColumnFlags.WidthStretch, 2.0f);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(Shown.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    DisplayRow(Shown[i], i);
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    private void DisplayRow(ModelCheckIssue issue, int index)
    {
        // The object is selectable while its model is loaded and it is still in it
        var entity = issue.FindEntity();

        ImGui.TableNextRow();

        // Severity
        ImGui.TableNextColumn();
        switch (issue.Severity)
        {
            case ModelCheckSeverity.Error:
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_SaveCheck_Severity_Error"));
                break;
            case ModelCheckSeverity.Warning:
                ImGui.Text(LOC.Get("MODEL_SaveCheck_Severity_Warning"));
                break;
            default:
                ImGui.TextDisabled(LOC.Get("MODEL_SaveCheck_Severity_Info"));
                break;
        }

        // Model
        ImGui.TableNextColumn();
        ImGui.Text(issue.Model.Name);

        // Object
        ImGui.TableNextColumn();
        if (ImGui.Selectable($"{issue.DescribeObject()}##modelSaveCheckRow{index}", false,
            entity != null ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            SelectIssue(entity);
        }
        GUI.Tooltip(entity != null ? LOC.Get("MODEL_SaveCheck_Select_TT") : LOC.Get("MODEL_SaveCheck_Not_Loaded_TT"));

        // Problem: one line, as the clipper needs rows of one height; the tooltip has it in full
        ImGui.TableNextColumn();
        var description = issue.Describe();
        ImGui.TextUnformatted(description);
        GUI.Tooltip(description);
    }

    /// <summary>
    /// Selects and frames the object in the view its model is loaded in, which holds its selection
    /// </summary>
    private void SelectIssue(Entity entity)
    {
        var view = (entity.Container as ModelContainer)?.View;

        if (view == null)
            return;

        view.ViewportSelection.ClearSelection();
        view.ViewportSelection.AddSelection(entity);
        view.ViewportSelection.GotoTreeTarget = entity;

        view.FrameAction.ApplyViewportFrame();
    }
}
