using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Lists the undo history of the loaded model, oldest first. Clicking a step goes back to just before it, or, for a
/// step that was undone, redoes up to it. Below that are the models this view keeps with their history after being
/// switched away from, which can be loaded again or forgotten. Plan: Documentation/Model Editor Second Review.md, R5.
/// </summary>
public class ModelHistoryTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>
    /// Brings the tool forward the next time the Tools window is drawn, as Edit › History does
    /// </summary>
    public bool FocusOnNextDraw;

    private sealed class Step
    {
        public ViewportAction Action;
        public bool IsUndone;
        public string What;
    }

    // Rebuilt when the history changes
    private int CachedVersion = -1;
    private List<Step> CachedSteps = new();

    // Brings the latest step into view once the history changes
    private bool ScrollToCurrent;

    // Set while drawing the kept models, applied after, as they change the list
    private ModelWrapper PendingLoad;
    private ModelWrapper PendingForget;

    public ModelHistoryTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private ViewportActionManager Manager => View.ViewportActionManager;

    public void OnToolWindow()
    {
        if (FocusOnNextDraw)
        {
            FocusOnNextDraw = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("MODEL_History_Tool_Header")}##modelHistoryToolHeader"))
            return;

        ImGui.BeginChild("ModelHistoryToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();

        // After the section, as they load or release models
        if (PendingLoad != null)
        {
            var wrapper = PendingLoad;
            PendingLoad = null;

            LoadKeptModel(wrapper);
        }

        if (PendingForget != null)
        {
            var wrapper = PendingForget;
            PendingForget = null;

            View.DiscardParked(wrapper);
        }
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MODEL_History_Tool_Hint"));

        var loaded = View.Selection.SelectedModelWrapper;

        if (loaded?.Container?.View == View)
        {
            DisplayLoadedHistory(loaded);
        }
        else
        {
            // Steps
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MODEL_History_Tool_Steps_Header", 0, 0), LOC.Get("MODEL_History_Tool_Steps_Header_TT"));

            GUI.WrappedText(LOC.Get("MODEL_History_Tool_No_Model"));
        }

        DisplayKeptModels();
    }

    private void DisplayLoadedHistory(ModelWrapper loaded)
    {
        var steps = GetSteps();

        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_History_Tool_Steps_Header", steps.Count(e => !e.IsUndone), steps.Count),
            LOC.Get("MODEL_History_Tool_Steps_Header_TT"));

        if (steps.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_History_Tool_Empty", loaded.Name));
        }
        else
        {
            DisplaySteps(steps);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_History_Tool_Actions_Header"), LOC.Get("MODEL_History_Tool_Actions_Header_TT"));

        var undoNext = Manager.PeekUndoAction();
        var redoNext = Manager.PeekRedoAction();

        GUI.ConditionalMultiButtonInput("modelHistoryActions",
            "modelHistoryUndo", LOC.Get("MODEL_History_Tool_Undo"),
            undoNext != null ? LOC.Get("MODEL_History_Tool_Undo_TT", ModelHistory.Describe(undoNext)) : LOC.Get("MODEL_History_Tool_Nothing_TT"),
            Manager.UndoAction, undoNext != null,

            "modelHistoryRedo", LOC.Get("MODEL_History_Tool_Redo"),
            redoNext != null ? LOC.Get("MODEL_History_Tool_Redo_TT", ModelHistory.Describe(redoNext)) : LOC.Get("MODEL_History_Tool_Nothing_TT"),
            Manager.RedoAction, redoNext != null);
    }

    /// <summary>
    /// The steps in the order they were made: those that can be undone, then those that were undone
    /// </summary>
    private List<Step> GetSteps()
    {
        if (CachedVersion == Manager.Version)
            return CachedSteps;

        CachedVersion = Manager.Version;
        ScrollToCurrent = true;

        var steps = new List<Step>();

        var undo = Manager.GetUndoActions();

        for (var i = undo.Count - 1; i >= 0; i--)
        {
            steps.Add(new Step { Action = undo[i], IsUndone = false, What = ModelHistory.Describe(undo[i]) });
        }

        foreach (var action in Manager.GetRedoActions())
        {
            steps.Add(new Step { Action = action, IsUndone = true, What = ModelHistory.Describe(action) });
        }

        CachedSteps = steps;

        return steps;
    }

    private void DisplaySteps(List<Step> steps)
    {
        var height = 320 * DPI.UIScale();

        ImGui.BeginChild("##modelHistorySteps", new Vector2(0, height), ImGuiChildFlags.Borders);

        // The step the history is at: the last one not undone
        var current = steps.FindLastIndex(e => !e.IsUndone);

        Step undoTo = null;
        Step redoTo = null;

        var clipper = new ImGuiListClipper();
        clipper.Begin(steps.Count);

        if (ScrollToCurrent && current >= 0)
        {
            clipper.IncludeItemByIndex(current);
        }

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var step = steps[i];

                ImGui.PushID(i);

                if (step.IsUndone)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                }

                if (ImGui.Selectable($"{i + 1}. {step.What}##modelHistoryStep", i == current))
                {
                    if (step.IsUndone)
                    {
                        redoTo = step;
                    }
                    else
                    {
                        undoTo = step;
                    }
                }

                if (step.IsUndone)
                {
                    ImGui.PopStyleColor();
                }

                if (ImGui.IsItemHovered())
                {
                    // Undo runs from the current step back to this one; redo from the step after it to this one
                    var count = step.IsUndone ? i - current : current - i + 1;

                    GUI.Tooltip(LOC.Get(step.IsUndone ? "MODEL_History_Tool_Step_Redo_TT" : "MODEL_History_Tool_Step_Undo_TT", count));
                }

                if (i == current && ScrollToCurrent)
                {
                    ImGui.SetScrollHereY();
                    ScrollToCurrent = false;
                }

                ImGui.PopID();
            }
        }

        clipper.End();

        ImGui.EndChild();

        // After the list, as they change it
        if (undoTo != null)
        {
            ModelHistory.UndoTo(Manager, undoTo.Action);
        }
        else if (redoTo != null)
        {
            ModelHistory.RedoTo(Manager, redoTo.Action);
        }
    }

    private void DisplayKeptModels()
    {
        var kept = View.GetParkedModels();

        // Kept Models
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_History_Tool_Kept_Header", kept.Count, CFG.Current.ModelEditor_History_Parked_Limit),
            LOC.Get("MODEL_History_Tool_Kept_Header_TT"));

        var limit = CFG.Current.ModelEditor_History_Parked_Limit;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderInt("##modelHistoryKeptLimit", ref limit, 0, 16))
        {
            CFG.Current.ModelEditor_History_Parked_Limit = limit;
        }
        GUI.Tooltip(LOC.Get("MODEL_History_Tool_Kept_Limit_TT"));

        if (kept.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_History_Tool_Kept_None"));
            return;
        }

        if (ImGui.BeginTable("modelHistoryKeptTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("##name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##load", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##forget", ImGuiTableColumnFlags.WidthFixed);

            for (var i = 0; i < kept.Count; i++)
            {
                var wrapper = kept[i];

                ImGui.TableNextRow();

                // Name, steps and unsaved marker
                ImGui.TableNextColumn();
                var unsaved = wrapper.IsDirty ? "*" : "";
                ImGui.Text($"{wrapper.Name}{unsaved}");
                GUI.Tooltip(LOC.Get("MODEL_History_Tool_Kept_Row_TT", wrapper.Parked.History.Count));

                ImGui.SameLine();
                ImGui.TextDisabled(LOC.Get("MODEL_History_Tool_Kept_Steps", wrapper.Parked.History.Count));

                // Load
                ImGui.TableNextColumn();
                if (ImGui.Button($"{LOC.Get("MODEL_History_Tool_Kept_Load")}##modelHistoryKeptLoad{i}"))
                {
                    PendingLoad = wrapper;
                }
                GUI.Tooltip(LOC.Get("MODEL_History_Tool_Kept_Load_TT"));

                // Forget
                ImGui.TableNextColumn();
                if (ImGui.Button($"{LOC.Get("MODEL_History_Tool_Kept_Forget")}##modelHistoryKeptForget{i}"))
                {
                    PendingForget = wrapper;
                }
                GUI.Tooltip(wrapper.IsDirty
                    ? LOC.Get("MODEL_History_Tool_Kept_Forget_Unsaved_TT")
                    : LOC.Get("MODEL_History_Tool_Kept_Forget_TT"));
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    /// Loads a kept model in this view as the File List would, showing its container there
    /// </summary>
    private void LoadKeptModel(ModelWrapper wrapper)
    {
        var container = wrapper.Parent;

        View.Selection.SelectedModelContainerWrapper = container;

        // Keeps the parked wrapper, so the list holds the same object
        container.PopulateModelList();

        View.LoadModel(wrapper);
    }
}
