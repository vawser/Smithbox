using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Edit History, in the Tools window's General category: the Param Editor's undo history, oldest first, as in the
/// Map, Model and Time Act editors. Clicking a step goes back to just before it (undoing it and every step after),
/// or, for a greyed (undone) step, redoes up to it. Undone steps stay redoable until a new edit is made.
/// <para>
/// The history belongs to the editor, so every view's Tools window shows the same one.
/// </para>
/// </summary>
public class ParamEditHistoryTool
{
    private readonly ParamEditorView View;

    /// <summary>
    /// Brings the tool forward the next time the Tools window is drawn, as Edit › History does
    /// </summary>
    public bool FocusOnNextDraw;

    private sealed class Step
    {
        public EditorAction Action;
        public bool IsUndone;
        public string What;
    }

    // Rebuilt when the history changes (judged by its size and its ends)
    private (int, int, EditorAction, EditorAction) CachedKey;
    private List<Step> CachedSteps = new();

    // Brings the current step into view once the history changes
    private bool ScrollToCurrent;

    public ParamEditHistoryTool(ParamEditorView view)
    {
        View = view;
    }

    private ActionManager Manager => View.Editor.ActionManager;

    public void OnToolWindow()
    {
        if (FocusOnNextDraw)
        {
            FocusOnNextDraw = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("PARAM_EditHistory_Header")}##paramEditHistoryHeader"))
            return;

        ImGui.BeginChild("ParamEditHistorySection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("PARAM_EditHistory_Hint"));

        var steps = GetSteps();

        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_EditHistory_Steps_Header", steps.Count(e => !e.IsUndone), steps.Count),
            LOC.Get("PARAM_EditHistory_Steps_Header_TT"));

        if (steps.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_EditHistory_Empty"));
        }
        else
        {
            DisplaySteps(steps);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_EditHistory_Actions_Header"), LOC.Get("PARAM_EditHistory_Actions_Header_TT"));

        var undoNext = Manager.PeekUndoAction();
        var redoNext = Manager.PeekRedoAction();

        GUI.ConditionalMultiButtonInput("paramEditHistoryActions",
            "paramEditHistoryUndo", LOC.Get("PARAM_EditHistory_Undo"),
            undoNext != null
                ? LOC.Get("PARAM_EditHistory_Undo_TT", ParamEditorScreen.GetActionDisplayName(undoNext))
                : LOC.Get("PARAM_EditHistory_Nothing_TT"),
            View.Editor.Undo, undoNext != null,

            "paramEditHistoryRedo", LOC.Get("PARAM_EditHistory_Redo"),
            redoNext != null
                ? LOC.Get("PARAM_EditHistory_Redo_TT", ParamEditorScreen.GetActionDisplayName(redoNext))
                : LOC.Get("PARAM_EditHistory_Nothing_TT"),
            View.Editor.Redo, redoNext != null);
    }

    /// <summary>
    /// The steps in the order they were made: those that can be undone, then those that were undone
    /// </summary>
    private List<Step> GetSteps()
    {
        var key = (Manager.UndoCount, Manager.RedoCount, Manager.PeekUndoAction(), Manager.PeekRedoAction());

        if (key.Equals(CachedKey))
            return CachedSteps;

        CachedKey = key;
        ScrollToCurrent = true;

        var steps = new List<Step>();

        // Most recent first, so walk it backwards for oldest first
        var undo = Manager.GetUndoHistory();

        for (var i = undo.Count - 1; i >= 0; i--)
        {
            steps.Add(new Step { Action = undo[i], IsUndone = false, What = ParamEditorScreen.GetActionDisplayName(undo[i]) });
        }

        // Next to redo first, which is also the order they were made
        foreach (var action in Manager.GetRedoHistory())
        {
            steps.Add(new Step { Action = action, IsUndone = true, What = ParamEditorScreen.GetActionDisplayName(action) });
        }

        CachedSteps = steps;

        return steps;
    }

    private void DisplaySteps(List<Step> steps)
    {
        var height = 320 * DPI.UIScale();

        ImGui.BeginChild("##paramEditHistorySteps", new Vector2(0, height), ImGuiChildFlags.Borders);

        // The step the history is at: the last one not undone
        var current = steps.FindLastIndex(e => !e.IsUndone);

        var undoCount = 0;
        var redoCount = 0;

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

                // Undo runs from the current step back through this one; redo from the step after the current one
                // up to this one
                var count = step.IsUndone ? i - current : current - i + 1;

                if (ImGui.Selectable($"{i + 1}. {step.What}##paramEditHistoryStep", i == current))
                {
                    if (step.IsUndone)
                        redoCount = count;
                    else
                        undoCount = count;
                }

                if (step.IsUndone)
                {
                    ImGui.PopStyleColor();
                }

                if (ImGui.IsItemHovered())
                {
                    var tooltip = LOC.Get(step.IsUndone ? "PARAM_EditHistory_Step_Redo_TT" : "PARAM_EditHistory_Step_Undo_TT", count);

                    // A mass edit's full script, for example
                    if (!string.IsNullOrEmpty(step.Action.Details))
                    {
                        var details = step.Action.Details;

                        if (details.Length > 1000)
                        {
                            details = details[..1000] + "...";
                        }

                        tooltip += $"\n\n{details}";
                    }

                    GUI.Tooltip(tooltip);
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
        if (undoCount > 0)
        {
            StepUndo(undoCount);
        }
        else if (redoCount > 0)
        {
            StepRedo(redoCount);
        }
    }

    /// <summary>
    /// Undo <paramref name="steps"/> steps, stopping early if one is refused (e.g. during a save)
    /// </summary>
    private void StepUndo(int steps)
    {
        for (var i = 0; i < steps && Manager.CanUndo(); i++)
        {
            var before = Manager.UndoCount;
            Manager.UndoAction();

            if (Manager.UndoCount == before)
                break;
        }
    }

    /// <summary>
    /// Redo <paramref name="steps"/> steps, stopping early if one is refused
    /// </summary>
    private void StepRedo(int steps)
    {
        for (var i = 0; i < steps && Manager.CanRedo(); i++)
        {
            var before = Manager.RedoCount;
            Manager.RedoAction();

            if (Manager.RedoCount == before)
                break;
        }
    }
}
