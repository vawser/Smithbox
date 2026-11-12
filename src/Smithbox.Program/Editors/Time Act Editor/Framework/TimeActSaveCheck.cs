using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// A problem in a time act that is about to be written, and how to fix it if it can be fixed for the user
/// </summary>
public record TimeActSaveIssue(TimeActContainerWrapper Source, TimeActEntry Entry, string Message, ViewportAction Fix);

/// <summary>
/// Checks the time acts of a source before they are written. Only what the game's own files never contain is
/// flagged, going by Tools/TaeTemplateSurvey savecheck over every vanilla ER, DS3 and SDT time act (2026-11-12):
/// no time act has its animations out of ID order or two animations with one ID, and no animation that has event
/// groups has an event outside of them. Ends before starts, negative starts and empty groups are all found in
/// vanilla files, so they aren't flagged.
/// </summary>
public static class TimeActSaveCheck
{
    /// <summary>
    /// Only the time acts that will be written again are checked: the rest go back to the binder as they were read
    /// </summary>
    public static List<TimeActSaveIssue> Check(TimeActContainerWrapper source)
    {
        var issues = new List<TimeActSaveIssue>();

        foreach (var entry in source.TimeActs)
        {
            if (!entry.IsDirty || entry.TimeAct == null)
                continue;

            CheckEntry(source, entry, issues);
        }

        return issues;
    }

    private static void CheckEntry(TimeActContainerWrapper source, TimeActEntry entry, List<TimeActSaveIssue> issues)
    {
        var animations = entry.TimeAct.Animations;

        var sort = new TimeActSortAnimationsAction(entry);

        if (sort.ChangesOrder)
        {
            issues.Add(new TimeActSaveIssue(source, entry,
                LOC.Get("TAE_SaveCheck_Unsorted_Animations", source.Name, entry.Name), sort));
        }

        foreach (var duplicate in animations.GroupBy(e => e.ID).Where(e => e.Count() > 1))
        {
            var name = source.FormatAnimationName(entry.GetFullAnimationID(duplicate.Key, source.AnimationCategorySize));

            issues.Add(new TimeActSaveIssue(source, entry,
                LOC.Get("TAE_SaveCheck_Duplicate_Animation_ID", source.Name, entry.Name, name, duplicate.Count()), null));
        }

        foreach (var animation in animations)
        {
            if (animation.EventGroups.Count == 0)
                continue;

            var ungrouped = animation.Events.Count(e => e.Group == null || !animation.EventGroups.Contains(e.Group));

            if (ungrouped > 0)
            {
                issues.Add(new TimeActSaveIssue(source, entry,
                    LOC.Get("TAE_SaveCheck_Ungrouped_Events", source.Name, source.GetAnimationName(entry, animation), ungrouped),
                    null));
            }
        }
    }
}
