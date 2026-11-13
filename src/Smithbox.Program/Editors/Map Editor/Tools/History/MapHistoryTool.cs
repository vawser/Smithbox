using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Lists the undo history of the view, oldest first, with what each step did and which maps it changed. Clicking a
/// step goes back to just before it (undoing it and everything after, in every map), or, for a step that was
/// undone, redoes up to it. A map filter shows the steps of one map, and how many steps unloading it would drop.
/// Plan: Documentation/Map Editor Review - Third Pass.md, R5.
/// </summary>
public class MapHistoryTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>
    /// Brings the tool forward the next time the Tools window is drawn, as the Edit menu's History does
    /// </summary>
    public bool FocusOnNextDraw;

    private sealed class Step
    {
        public ViewportAction Action;
        public bool IsUndone;
        public string What;
        public string Where;
        public HashSet<string> Maps;
    }

    // Rebuilt when the history changes: names can change after a step runs (a deferred step is built then)
    private int CachedVersion = -1;
    private List<Step> CachedSteps = new();
    private List<string> CachedMaps = new();

    // The map whose steps are shown, or null for all
    private string MapFilter;

    // Brings the latest step into view once the history or the filter changes
    private bool ScrollToCurrent;

    public MapHistoryTool(MapEditorView view, ProjectEntry project)
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

        if (!ToolSections.Header($"{LOC.Get("MAP_ToolWindow_History")}##mapHistoryToolHeader"))
            return;

        ImGui.BeginChild("MapHistoryToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAP_History_Tool_Hint"));

        var steps = GetSteps();

        // The filter's map may have left the history (unloaded, or its steps dropped)
        if (MapFilter != null && !CachedMaps.Contains(MapFilter))
        {
            MapFilter = null;
        }

        // Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_History_Tool_Map_Header"), LOC.Get("MAP_History_Tool_Map_Header_TT"));

        DisplayMapFilter();

        if (MapFilter != null)
        {
            var dropped = Manager.CountActionsTouching(MapFilter);

            GUI.WrappedText(LOC.Get("MAP_History_Tool_Unload_Count", MapFilter, dropped));
        }

        // Steps
        var shown = Enumerable.Range(0, steps.Count)
            .Where(i => MapFilter == null || steps[i].Maps.Contains(MapFilter))
            .ToList();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_History_Tool_Steps_Header", steps.Count(e => !e.IsUndone), steps.Count),
            LOC.Get("MAP_History_Tool_Steps_Header_TT"));

        if (shown.Count == 0)
        {
            GUI.WrappedText(LOC.Get(steps.Count == 0 ? "MAP_History_Tool_Empty" : "MAP_History_Tool_Empty_Filtered"));
        }
        else
        {
            DisplaySteps(steps, shown);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_History_Tool_Actions_Header"), LOC.Get("MAP_History_Tool_Actions_Header_TT"));

        var undoNext = Manager.PeekUndoAction();
        var redoNext = Manager.PeekRedoAction();

        GUI.ConditionalMultiButtonInput("mapHistoryActions",
            "mapHistoryUndo", LOC.Get("MAP_History_Tool_Undo"),
            undoNext != null ? LOC.Get("MAP_History_Tool_Undo_TT", MapHistory.Describe(undoNext)) : LOC.Get("MAP_History_Tool_Nothing_TT"),
            Manager.UndoAction, undoNext != null,

            "mapHistoryRedo", LOC.Get("MAP_History_Tool_Redo"),
            redoNext != null ? LOC.Get("MAP_History_Tool_Redo_TT", MapHistory.Describe(redoNext)) : LOC.Get("MAP_History_Tool_Nothing_TT"),
            Manager.RedoAction, redoNext != null);
    }

    private void DisplayMapFilter()
    {
        var preview = MapFilter ?? LOC.Get("MAP_History_Tool_All_Maps");

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##mapHistoryMapFilter", preview))
        {
            if (ImGui.Selectable($"{LOC.Get("MAP_History_Tool_All_Maps")}##mapHistoryAllMaps", MapFilter == null))
            {
                MapFilter = null;
                ScrollToCurrent = true;
            }

            foreach (var map in CachedMaps)
            {
                if (ImGui.Selectable($"{map}##mapHistoryMap_{map}", MapFilter == map))
                {
                    MapFilter = map;
                    ScrollToCurrent = true;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_History_Tool_Map_Filter_TT"));
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
            steps.Add(MakeStep(undo[i], false));
        }

        foreach (var action in Manager.GetRedoActions())
        {
            steps.Add(MakeStep(action, true));
        }

        CachedSteps = steps;
        CachedMaps = steps.SelectMany(e => e.Maps).Distinct().OrderBy(e => e).ToList();

        return steps;
    }

    private static Step MakeStep(ViewportAction action, bool isUndone)
    {
        var maps = new HashSet<string>(action.TouchedScopes);

        return new Step
        {
            Action = action,
            IsUndone = isUndone,
            What = MapHistory.Describe(action),
            Where = maps.Count > 0
                ? string.Join(", ", maps.OrderBy(e => e))
                : LOC.Get("MAP_History_Tool_No_Map"),
            Maps = maps
        };
    }

    private void DisplaySteps(List<Step> steps, List<int> shown)
    {
        var height = 320 * DPI.UIScale();

        ImGui.BeginChild("##mapHistorySteps", new Vector2(0, height), ImGuiChildFlags.Borders);

        // The step the history is at: the last one not undone
        var current = steps.FindLastIndex(e => !e.IsUndone);

        // Where the current step is in the shown list: the last shown step at or before it
        var currentRow = shown.FindLastIndex(i => i <= current);

        Step undoTo = null;
        Step redoTo = null;

        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);

        if (ScrollToCurrent && currentRow >= 0)
        {
            clipper.IncludeItemByIndex(currentRow);
        }

        while (clipper.Step())
        {
            for (var row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var i = shown[row];
                var step = steps[i];

                ImGui.PushID(i);

                if (step.IsUndone)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                }

                if (ImGui.Selectable($"{i + 1}. {step.What}##mapHistoryStep", i == current))
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
                    GUI.Tooltip(DescribeClick(steps, i, current));
                }

                ImGui.Indent();
                ImGui.TextDisabled(step.Where);
                ImGui.Unindent();

                if (row == currentRow && ScrollToCurrent)
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
            MapHistory.UndoTo(Manager, undoTo.Action);
        }
        else if (redoTo != null)
        {
            MapHistory.RedoTo(Manager, redoTo.Action);
        }
    }

    /// <summary>
    /// What clicking the step does: how many steps it undoes or redoes, and how many of those are in other maps
    /// than the one filtered to, as the history is one list across the maps
    /// </summary>
    private string DescribeClick(List<Step> steps, int index, int current)
    {
        // Undo runs from the current step back to this one; redo from the step after the current one to this one
        var range = steps[index].IsUndone
            ? steps.Skip(current + 1).Take(index - current).ToList()
            : steps.Skip(index).Take(current - index + 1).ToList();

        var text = LOC.Get(steps[index].IsUndone ? "MAP_History_Tool_Step_Redo_TT" : "MAP_History_Tool_Step_Undo_TT", range.Count);

        if (MapFilter != null)
        {
            var others = range.Count(e => !e.Maps.Contains(MapFilter));

            if (others > 0)
            {
                text += "\n" + LOC.Get("MAP_History_Tool_Step_Other_Maps_TT", others, MapFilter);
            }
        }

        return text;
    }
}
