using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Lists the undo history of the view, oldest first, with what each step did and where. Clicking a step goes back
/// to just before it (undoing it and everything after), or, for a step that was undone, redoes up to it.
/// The view goes to what is undone or redone, as Ctrl+Z does.
/// </summary>
public class TimeActHistoryTool
{
    private readonly TimeActEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>
    /// Brings the tool forward the next time the Tools window is drawn, as the Edit menu's History does
    /// </summary>
    public bool FocusOnNextDraw;

    // The labels are worked out once for each step, as finding where a step is looks through the sources
    private sealed class Label
    {
        public string What;
        public string Where;
    }

    private readonly ConditionalWeakTable<ViewportAction, Label> Labels = new();

    // The list is rebuilt when the history changes
    private int CachedVersion = -1;
    private List<(ViewportAction Action, bool IsUndone)> CachedSteps = new();

    // Brings the latest step into view once the history changes
    private bool ScrollToCurrent;

    public TimeActHistoryTool(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (FocusOnNextDraw)
        {
            FocusOnNextDraw = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("TAE_HistoryTool_Header")}##timeActHistoryToolHeader"))
            return;

        ImGui.BeginChild("TimeActHistoryToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("TAE_HistoryTool_Hint"));

        var steps = GetSteps();

        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_HistoryTool_Steps_Header", steps.Count(e => !e.IsUndone), steps.Count),
            LOC.Get("TAE_HistoryTool_Steps_Header_TT"));

        if (steps.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TAE_HistoryTool_Empty"));
        }
        else
        {
            DisplaySteps(steps);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_HistoryTool_Actions_Header"), LOC.Get("TAE_HistoryTool_Actions_Header_TT"));

        var manager = View.ViewportActionManager;

        GUI.ConditionalMultiButtonInput("timeActHistoryActions",
            "timeActHistoryUndo", LOC.Get("TAE_HistoryTool_Undo"), LOC.Get("TAE_HistoryTool_Undo_TT"),
            View.History.Undo, manager.CanUndo(),

            "timeActHistoryRedo", LOC.Get("TAE_HistoryTool_Redo"), LOC.Get("TAE_HistoryTool_Redo_TT"),
            View.History.Redo, manager.CanRedo());
    }

    /// <summary>
    /// The steps in the order they were made: those that can be undone, then those that were undone
    /// </summary>
    private List<(ViewportAction Action, bool IsUndone)> GetSteps()
    {
        var manager = View.ViewportActionManager;

        if (CachedVersion == manager.Version)
            return CachedSteps;

        CachedVersion = manager.Version;
        ScrollToCurrent = true;

        var steps = new List<(ViewportAction, bool)>();

        var undo = manager.GetUndoActions();

        for (var i = undo.Count - 1; i >= 0; i--)
        {
            steps.Add((undo[i], false));
        }

        foreach (var action in manager.GetRedoActions())
        {
            steps.Add((action, true));
        }

        CachedSteps = steps;

        return steps;
    }

    private void DisplaySteps(List<(ViewportAction Action, bool IsUndone)> steps)
    {
        var height = 320 * DPI.UIScale();

        ImGui.BeginChild("##timeActHistorySteps", new Vector2(0, height), ImGuiChildFlags.Borders);

        // The step the history is at: the last one not undone
        var current = steps.FindLastIndex(e => !e.IsUndone);

        ViewportAction undoTo = null;
        ViewportAction redoTo = null;

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
                var (action, isUndone) = steps[i];
                var label = GetLabel(action);

                ImGui.PushID(i);

                if (isUndone)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                }

                if (ImGui.Selectable($"{i + 1}. {label.What}##timeActHistoryStep", i == current))
                {
                    if (isUndone)
                    {
                        redoTo = action;
                    }
                    else
                    {
                        undoTo = action;
                    }
                }

                if (isUndone)
                {
                    ImGui.PopStyleColor();
                }

                GUI.Tooltip(LOC.Get(isUndone ? "TAE_HistoryTool_Step_Redo_TT" : "TAE_HistoryTool_Step_Undo_TT"));

                if (label.Where != "")
                {
                    ImGui.Indent();
                    ImGui.TextDisabled(label.Where);
                    ImGui.Unindent();
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
            View.History.UndoTo(undoTo);
        }
        else if (redoTo != null)
        {
            View.History.RedoTo(redoTo);
        }
    }

    private Label GetLabel(ViewportAction action)
    {
        if (Labels.TryGetValue(action, out var label))
            return label;

        label = new Label();

        if (action is ITimeActHistoryItem item)
        {
            label.What = item.Describe();
            label.Where = DescribeWhere(item);
        }
        else
        {
            // The viewport's own actions, such as a gizmo move of the model
            label.What = LOC.Get("TAE_HistoryTool_Viewport_Step");
            label.Where = "";
        }

        Labels.AddOrUpdate(action, label);

        return label;
    }

    /// <summary>
    /// The source, time act and animation a step changes, as far as it names them
    /// </summary>
    private string DescribeWhere(ITimeActHistoryItem item)
    {
        var source = View.History.FindSource(item);

        if (source == null)
            return item.TargetEntry != null ? item.TargetEntry.Name : LOC.Get("TAE_HistoryTool_Several");

        var parts = new List<string> { source.Name };

        if (item.TargetEntry is { } entry)
        {
            parts.Add(entry.Name);

            if (TimeActHistory.FindAnimation(item) is { } animation)
            {
                parts.Add(source.GetAnimationName(entry, animation));
            }
        }

        return string.Join(" / ", parts);
    }
}
