using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The delete half of the mass edit: every match is removed, as one action that a single undo puts back.
/// Deleting animations asks first, as it takes their events with them.
/// </summary>
public partial class TimeActMassEditTool
{
    private record DeleteOutcome(TimeActMassEditTarget Target, int Deleted, int Stale);

    // Set once the matches have been deleted, so they can't be deleted a second time
    private DeleteOutcome DeleteResult;

    private const string DeleteConfirmId = "###massEditConfirmDelete";

    private bool HasApplied => AppliedResult != null || DeleteResult != null || CreateResult != null || DuplicateResult != null;

    /// <summary>
    /// Shows what the last apply did. Returns true if anything has been applied since Find, as the matches no
    /// longer show what is there.
    /// </summary>
    private bool DisplayAppliedResult()
    {
        if (AppliedResult != null)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Applied", AppliedResult.Applied));

            if (AppliedResult.Stale > 0)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Applied_Stale", AppliedResult.Stale));
            }
        }
        else if (DeleteResult != null)
        {
            GUI.WrappedText(DeleteResult.Target is TimeActMassEditTarget.Events
                ? LOC.Get("TAE_MassEditTool_Deleted_Events", DeleteResult.Deleted)
                : LOC.Get("TAE_MassEditTool_Deleted_Animations", DeleteResult.Deleted));

            if (DeleteResult.Stale > 0)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Deleted_Stale", DeleteResult.Stale));
            }
        }
        else if (CreateResult != null)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Created", CreateResult.Created, CreateResult.Animations));

            if (CreateResult.Stale > 0)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Created_Stale", CreateResult.Stale));
            }
        }
        else if (DuplicateResult != null)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Duplicated", DuplicateResult.Created));

            if (DuplicateResult.Stale > 0)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Created_Stale", DuplicateResult.Stale));
            }
        }
        else
        {
            return false;
        }

        // Save only writes the selected source, so an edit across sources needs the menu's Save All
        if (ResultsGlobal)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("TAE_MassEditTool_Applied_Save_All", LOC.Get("EDITOR_Menubar_Action_SaveAll")));
        }

        GUI.WrappedText(LOC.Get("TAE_MassEditTool_Applied_Find_Again"));

        return true;
    }

    private void DisplayDelete()
    {
        // Deletions
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("TAE_MassEditTool_Deletions_Header"),
            LOC.Get("TAE_MassEditTool_Deletions_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (Results.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Delete_Nothing"));
            return;
        }

        var isEvents = ResultTarget is TimeActMassEditTarget.Events;
        var eventsInAnimations = isEvents ? 0 : Results.Sum(e => e.Match.Animation.Events.Count);

        GUI.WrappedText(isEvents
            ? LOC.Get("TAE_MassEditTool_Delete_Summary_Events", Results.Count, ResultAnimations.Count)
            : LOC.Get("TAE_MassEditTool_Delete_Summary_Animations", Results.Count, eventsInAnimations));

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Changes_Stale"));
        }

        // Delete
        var delete = false;

        GUI.ConditionalMultiButtonInput("massEditDeleteActions",
            "massEditDelete",
            LOC.Get("TAE_MassEditTool_Delete"),
            LOC.Get(isEvents ? "TAE_MassEditTool_Delete_Events_TT" : "TAE_MassEditTool_Delete_Animations_TT"),
            () => delete = true,
            !stale);

        // The popup is opened here rather than in the button, so it is opened and drawn under the same ID
        if (delete)
        {
            if (isEvents)
            {
                ApplyDelete();
                return;
            }

            ImGui.OpenPopup($"{LOC.Get("TAE_MassEditTool_Delete_Confirm_Title")}{DeleteConfirmId}");
        }

        if (DisplayDeleteConfirm(eventsInAnimations))
            return;

        if (!ShowChangesList)
            return;

        // The matches are what is deleted, so they are listed with the same filter as on the Find tab
        EditorFilters.DisplayFramedListFilter("massEditDeleteFilter", ref ResultFilter, ref ExactResultFilter);

        var filtered = GetFilteredResults();

        ImGui.BeginChild("##massEditDeletions", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        TimeActQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (match, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##massEditDeleteJump{index}"))
                {
                    jumpTarget = match;
                }
                GUI.Tooltip(LOC.Get("TAE_MassEditTool_Jump_TT"));

                ImGui.SameLine();

                var selected = View.Selection.SelectedAnimation == match.Animation &&
                    (match.Event == null || View.Selection.IsEventSelected(match.Event));

                if (ImGui.Selectable($"{label}##massEditDeletion{index}", selected))
                {
                    jumpTarget = match;
                }
            }
        }

        clipper.End();

        ImGui.EndChild();

        if (jumpTarget != null)
        {
            Runner.Jump(jumpTarget);
        }
    }

    /// <summary>
    /// Asks before deleting animations. Returns true once they have been deleted, as the rest of the section
    /// draws the matches that are now gone.
    /// </summary>
    private bool DisplayDeleteConfirm(int eventsInAnimations)
    {
        var open = true;
        var confirmed = false;

        if (ImGui.BeginPopupModal($"{LOC.Get("TAE_MassEditTool_Delete_Confirm_Title")}{DeleteConfirmId}",
            ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            GUI.WrappedText(LOC.Get("TAE_MassEditTool_Delete_Confirm_Text", Results.Count, eventsInAnimations));

            var cancelled = false;

            GUI.MultiButtonInput("massEditDeleteConfirmActions",
                "massEditDeleteConfirm",
                LOC.Get("TAE_MassEditTool_Delete"),
                LOC.Get("TAE_MassEditTool_Delete_Animations_TT"),
                () => confirmed = true,

                "massEditDeleteCancel",
                LOC.Get("TAE_MassEditTool_Delete_Cancel"),
                LOC.Get("TAE_MassEditTool_Delete_Cancel_TT"),
                () => cancelled = true);

            if (confirmed || cancelled)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (confirmed)
        {
            ApplyDelete();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Each match is looked up again first. One that has gone since Find, by hand or by another edit, is left out
    /// and counted as stale.
    /// </summary>
    private void ApplyDelete()
    {
        if (Results == null || IsStale())
            return;

        AdoptReadSources();

        var stale = 0;
        var deleted = 0;

        if (ResultTarget is TimeActMassEditTarget.Events)
        {
            var groups = new Dictionary<(TimeActEntry, TAE.Animation), List<TAE.Event>>();

            foreach (var (match, _) in Results)
            {
                if (!TimeActQueryRunner.TryResolve(match, out var entry, out var animation, out var ev) || ev == null)
                {
                    stale++;
                    continue;
                }

                if (!groups.TryGetValue((entry, animation), out var events))
                {
                    events = new();
                    groups[(entry, animation)] = events;
                }

                events.Add(ev);
            }

            var action = new TimeActMassDeleteEventsAction(View.Selection,
                groups.Select(e => (e.Key.Item1, e.Key.Item2, (IEnumerable<TAE.Event>)e.Value)));

            deleted = action.EventCount;

            if (deleted > 0)
            {
                View.ViewportActionManager.ExecuteAction(action);
            }
        }
        else
        {
            var groups = new Dictionary<TimeActEntry, List<TAE.Animation>>();

            foreach (var (match, _) in Results)
            {
                if (!TimeActQueryRunner.TryResolve(match, out var entry, out var animation, out _))
                {
                    stale++;
                    continue;
                }

                if (!groups.TryGetValue(entry, out var animations))
                {
                    animations = new();
                    groups[entry] = animations;
                }

                animations.Add(animation);
            }

            var action = new TimeActMassDeleteAnimationsAction(View,
                groups.Select(e => (e.Key, (IEnumerable<TAE.Animation>)e.Value)));

            deleted = action.AnimationCount;

            if (deleted > 0)
            {
                View.ViewportActionManager.ExecuteAction(action);
            }
        }

        Smithbox.Log(this, LOC.Get(ResultTarget is TimeActMassEditTarget.Events
            ? "TAE_MassEditTool_Deleted_Events_Log"
            : "TAE_MassEditTool_Deleted_Animations_Log", deleted, stale));

        DeleteResult = new DeleteOutcome(ResultTarget, deleted, stale);
    }
}
