using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What Create makes, when the matches are animations
/// </summary>
public enum TimeActMassEditCreateKind
{
    Events,
    AnimationCopies
}

/// <summary>
/// Create for animations can also copy them: each matched animation is copied under its ID plus an offset, with
/// its events, as the animation list's Duplicate does for one.
/// </summary>
public partial class TimeActMassEditTool
{
    private record DuplicateOutcome(int Created, int Stale);

    private TimeActMassEditCreateKind CreateKind = TimeActMassEditCreateKind.Events;
    private string DuplicateOffsetText = "1";
    private bool DuplicateNextFree = false;

    private TimeActMassDuplicatePlan DuplicatePlan;
    private int DuplicatePlanVersion = -1;
    private List<ResultRow> DuplicatePlanFor;
    private int DuplicatePlanForCount;

    private string[] DuplicateLabels;

    private string DuplicateFilter = "";
    private bool ExactDuplicateFilter = false;
    private List<int> FilteredDuplicates;
    private TimeActMassDuplicatePlan FilteredDuplicatesFor;
    private string CachedDuplicateFilter = "";
    private bool CachedExactDuplicateFilter = false;

    // Set once the copies have been made, so they can't be made a second time
    private DuplicateOutcome DuplicateResult;

    private void ClearDuplicatePlan()
    {
        DuplicatePlan = null;
        DuplicatePlanFor = null;
        DuplicatePlanVersion = -1;
        DuplicateLabels = null;
        FilteredDuplicates = null;
        DuplicateResult = null;
    }

    /// <summary>
    /// For animations, the choice of adding events to them or copying them. Returns true if the copies are shown.
    /// </summary>
    private bool DisplayCreateKind()
    {
        if (ResultTarget is not TimeActMassEditTarget.Animations)
            return false;

        // What
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Create_Kind_Header"),
            LOC.Get("TAE_MassEditTool_Create_Kind_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##massEditCreateKind", GetCreateKindName(CreateKind)))
        {
            foreach (var kind in Enum.GetValues<TimeActMassEditCreateKind>())
            {
                if (ImGui.Selectable($"{GetCreateKindName(kind)}##massEditCreateKind{kind}", CreateKind == kind))
                {
                    CreateKind = kind;
                    StepsChanged();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Create_Kind_Header_TT"));

        if (CreateKind is not TimeActMassEditCreateKind.AnimationCopies)
            return false;

        DisplayDuplicate();

        return true;
    }

    private static string GetCreateKindName(TimeActMassEditCreateKind kind) => kind switch
    {
        TimeActMassEditCreateKind.Events => LOC.Get("TAE_MassEditTool_Create_Kind_Events"),
        TimeActMassEditCreateKind.AnimationCopies => LOC.Get("TAE_MassEditTool_Create_Kind_Copies"),
        _ => $"{kind}"
    };

    private void DisplayDuplicate()
    {
        // ID Offset
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Duplicate_Offset_Header"),
            LOC.Get("TAE_MassEditTool_Duplicate_Offset_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputTextWithHint("##massEditDuplicateOffset", LOC.Get("TAE_MassEditTool_Duplicate_Offset_Hint"),
            ref DuplicateOffsetText, 32))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Duplicate_Offset_Header_TT"));

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_MassEditTool_Options_Header"),
            LOC.Get("TAE_MassEditTool_Duplicate_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("TAE_MassEditTool_Duplicate_Next_Free")}##massEditDuplicateNextFree", ref DuplicateNextFree))
        {
            StepsChanged();
        }
        GUI.Tooltip(LOC.Get("TAE_MassEditTool_Duplicate_Next_Free_TT"));

        UpdateDuplicatePlan();

        DisplayDuplicates();
    }

    private void UpdateDuplicatePlan()
    {
        if (HasApplied)
            return;

        if (DuplicatePlan != null && DuplicatePlanVersion == StepsVersion && DuplicatePlanFor == Results &&
            DuplicatePlanForCount == Results.Count)
        {
            return;
        }

        DuplicatePlanVersion = StepsVersion;
        DuplicatePlanFor = Results;
        DuplicatePlanForCount = Results.Count;
        DuplicateLabels = null;
        FilteredDuplicates = null;

        if (!long.TryParse(DuplicateOffsetText.Trim(), out var offset))
        {
            DuplicatePlan = null;
            return;
        }

        DuplicatePlan = Planner.PlanDuplicates(Results.Select(e => e.Match).ToList(), offset, DuplicateNextFree);
        DuplicateLabels = new string[DuplicatePlan.Duplicates.Count];
    }

    private void DisplayDuplicates()
    {
        // Copies
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("TAE_MassEditTool_Duplicates_Header"),
            LOC.Get("TAE_MassEditTool_Duplicates_Header_TT"),
            ref ShowChangesList);

        if (DisplayAppliedResult())
            return;

        if (DuplicatePlan == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Duplicate_Invalid_Offset"));
            return;
        }

        GUI.WrappedText(LOC.Get("TAE_MassEditTool_Duplicate_Summary", DuplicatePlan.Duplicates.Count));

        DisplaySkips(DuplicatePlan.Skips, null);

        var stale = IsStale();

        if (stale)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TAE_MassEditTool_Changes_Stale"));
        }

        // Create
        var create = false;

        GUI.ConditionalMultiButtonInput("massEditDuplicateActions",
            "massEditDuplicate",
            LOC.Get("TAE_MassEditTool_Create"),
            LOC.Get("TAE_MassEditTool_Duplicate_TT"),
            () => create = true,
            !stale && DuplicatePlan.Duplicates.Count > 0);

        // Creating drops the plan, so the rest of the section waits for the next frame
        if (create)
        {
            ApplyDuplicates();
            return;
        }

        if (DuplicatePlan.Duplicates.Count == 0 || !ShowChangesList)
            return;

        EditorFilters.DisplayFramedListFilter("massEditDuplicateFilter", ref DuplicateFilter, ref ExactDuplicateFilter);

        var filtered = GetFilteredDuplicates();

        ImGui.BeginChild("##massEditDuplicates", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        TimeActQueryMatch jumpTarget = null;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var planned = DuplicatePlan.Duplicates[index];

                if (ImGui.Button($"{Icons.LocationArrow}##massEditDuplicateJump{index}"))
                {
                    jumpTarget = planned.Match;
                }
                GUI.Tooltip(LOC.Get("TAE_MassEditTool_Jump_TT"));

                ImGui.SameLine();

                if (ImGui.Selectable($"{GetDuplicateLabel(index)}##massEditDuplicateRow{index}",
                    View.Selection.SelectedAnimation == planned.Match.Animation))
                {
                    jumpTarget = planned.Match;
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

    private string GetDuplicateLabel(int index)
    {
        if (DuplicateLabels[index] != null)
            return DuplicateLabels[index];

        var planned = DuplicatePlan.Duplicates[index];
        var match = planned.Match;
        var source = match.Source;

        var oldName = source.GetAnimationName(match.Entry, match.Animation);
        var newName = source.FormatAnimationName(match.Entry.GetFullAnimationID(planned.NewID, source.AnimationCategorySize));

        var label = PrefixLabel(match, $"{oldName} -> {newName}");

        DuplicateLabels[index] = label;

        return label;
    }

    private List<int> GetFilteredDuplicates()
    {
        if (FilteredDuplicates != null && FilteredDuplicatesFor == DuplicatePlan &&
            CachedDuplicateFilter == DuplicateFilter && CachedExactDuplicateFilter == ExactDuplicateFilter)
        {
            return FilteredDuplicates;
        }

        var filtered = new List<int>();

        for (int i = 0; i < DuplicatePlan.Duplicates.Count; i++)
        {
            if (DuplicateFilter == "" || EditorFilters.IsMatch(DuplicateFilter, GetDuplicateLabel(i), ExactDuplicateFilter))
            {
                filtered.Add(i);
            }
        }

        FilteredDuplicates = filtered;
        FilteredDuplicatesFor = DuplicatePlan;
        CachedDuplicateFilter = DuplicateFilter;
        CachedExactDuplicateFilter = ExactDuplicateFilter;

        return filtered;
    }

    /// <summary>
    /// Each animation is looked up again, and its new ID checked again, as animations can have been added since.
    /// The copies are made here, so they hold the events the animations have now.
    /// </summary>
    private void ApplyDuplicates()
    {
        if (DuplicatePlan == null || IsStale())
            return;

        AdoptReadSources();

        var groups = new Dictionary<TimeActEntry, List<TAE.Animation>>();
        var used = new Dictionary<TimeActEntry, HashSet<long>>();
        var stale = 0;

        foreach (var planned in DuplicatePlan.Duplicates)
        {
            if (!TimeActQueryRunner.TryResolve(planned.Match, out var entry, out var animation, out _))
            {
                stale++;
                continue;
            }

            if (!used.TryGetValue(entry, out var ids))
            {
                ids = entry.TimeAct.Animations.Select(e => e.ID).ToHashSet();
                used[entry] = ids;
            }

            var id = planned.NewID;
            var fileName = planned.FileName;

            if (ids.Contains(id))
            {
                if (!DuplicateNextFree)
                {
                    stale++;
                    continue;
                }

                while (ids.Contains(id))
                {
                    id++;
                }

                fileName = TimeActAnimationSelectionWindow.GetAnimFileName(planned.Match.Source, entry, animation, id,
                    animation.AnimFileName);
            }

            ids.Add(id);

            if (!groups.TryGetValue(entry, out var copies))
            {
                copies = new();
                groups[entry] = copies;
            }

            copies.Add(entry.CloneAnimation(animation, id, fileName));
        }

        var action = new TimeActMassAddAnimationsAction(View, groups.Select(e => (e.Key, e.Value)));

        var created = action.AnimationCount;

        if (created > 0)
        {
            View.ViewportActionManager.ExecuteAction(action);
        }

        Smithbox.Log(this, LOC.Get("TAE_MassEditTool_Duplicated_Log", created, stale));

        DuplicatePlan = null;
        DuplicateLabels = null;
        FilteredDuplicates = null;
        DuplicateResult = new DuplicateOutcome(created, stale);
    }
}
