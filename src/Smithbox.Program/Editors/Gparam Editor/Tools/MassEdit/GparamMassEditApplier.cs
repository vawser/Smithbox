using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Carries out a mass edit as one action, so a single undo takes the whole of it back. A file that wasn't open when
/// the matches were found was read through a copy, so it is opened here, and each match is looked up again by its
/// keys and ID.
/// </summary>
public static class GparamMassEditApplier
{
    public record Result(int Applied, int Stale);

    /// <summary>
    /// The GPARAM of a file as the bank has it, opening the file if it isn't
    /// </summary>
    private static GPARAM GetGparam(ProjectEntry project, FileDictionaryEntry fileEntry)
    {
        var bank = project.Handler.GparamData.PrimaryBank;

        bank.LoadGraphicsParamNow(fileEntry);

        return bank.Entries.TryGetValue(fileEntry, out var wrapper) ? wrapper.Gparam : null;
    }

    private static void Execute(GparamEditorView view, List<EditorAction> actions)
    {
        if (actions.Count == 1)
        {
            view.ActionManager.ExecuteAction(actions[0]);
        }
        else if (actions.Count > 1)
        {
            view.ActionManager.ExecuteAction(new CompoundAction(actions));
        }
    }

    #region Edit
    /// <summary>
    /// Each row is checked against the values it had when the plan was made. A row changed since, by hand or by
    /// another edit, is left alone and counted as stale, so the edit never builds on values it didn't show.
    /// </summary>
    public static Result ApplyEdit(GparamEditorView view, ProjectEntry project, GparamMassEditPlan plan)
    {
        var entries = new List<MassSetValuesAction.Entry>();
        var stale = 0;

        foreach (var file in plan.Values.GroupBy(e => e.Match.FileEntry))
        {
            var gparam = GetGparam(project, file.Key);

            foreach (var planned in file)
            {
                if (gparam == null ||
                    !GparamQueryRunner.TryResolve(gparam, planned.Match, out _, out _, out _, out _, out _, out var value) ||
                    value == null)
                {
                    stale++;
                    continue;
                }

                if (!Equals(value.Value, planned.OldValue) || value.ID != planned.OldID || value.TimeOfDay != planned.OldTimeOfDay)
                {
                    stale++;
                    continue;
                }

                entries.Add(new MassSetValuesAction.Entry(gparam, value, planned.NewValue, planned.NewID, planned.NewTimeOfDay));
            }
        }

        if (entries.Count > 0)
        {
            view.ActionManager.ExecuteAction(new MassSetValuesAction(project, entries));
        }

        return new Result(entries.Count, stale);
    }
    #endregion

    #region Delete
    /// <summary>
    /// Removes every match: value rows from their fields, fields from their groups, or groups from their files.
    /// One delete per field, group or file, all in one action.
    /// </summary>
    public static Result ApplyDelete(GparamEditorView view, ProjectEntry project, IReadOnlyList<GparamQueryMatch> matches,
        GparamMassEditTarget target)
    {
        var actions = new List<EditorAction>();
        var applied = 0;
        var stale = 0;

        foreach (var file in matches.GroupBy(e => e.FileEntry))
        {
            var gparam = GetGparam(project, file.Key);

            // What each match points to now
            var resolved = new List<(GPARAM.Param Group, GPARAM.IField Field, GPARAM.IFieldValue Value)>();

            foreach (var match in file)
            {
                if (gparam == null ||
                    !GparamQueryRunner.TryResolve(gparam, match, out _, out var group, out _, out var field, out _, out var value))
                {
                    stale++;
                    continue;
                }

                resolved.Add((group, field, value));
            }

            switch (target)
            {
                case GparamMassEditTarget.Values:
                    foreach (var byField in resolved.Where(e => e.Value != null).GroupBy(e => (e.Group, e.Field)))
                    {
                        var values = byField.Select(e => e.Value).Distinct().ToList();

                        actions.Add(new DeleteValueAction(project, gparam, byField.Key.Group, byField.Key.Field, values));
                        applied += values.Count;
                    }
                    break;

                case GparamMassEditTarget.Fields:
                    foreach (var byGroup in resolved.Where(e => e.Field != null).GroupBy(e => e.Group))
                    {
                        var fields = byGroup.Select(e => e.Field).Distinct().ToList();

                        actions.Add(new DeleteFieldAction(project, gparam, byGroup.Key, fields));
                        applied += fields.Count;
                    }
                    break;

                case GparamMassEditTarget.Groups:
                    var groups = resolved.Select(e => e.Group).Where(e => e != null).Distinct().ToList();

                    if (groups.Count > 0)
                    {
                        actions.Add(new DeleteGroupAction(project, gparam, groups));
                        applied += groups.Count;
                    }
                    break;
            }
        }

        Execute(view, actions);

        return new Result(applied, stale);
    }
    #endregion

    #region Duplicate
    /// <summary>
    /// Adds a copy of every matched value to its field, with its ID moved by the offset. The add moves an ID that is
    /// taken up to the next free one, as the plan showed.
    /// </summary>
    public static Result ApplyDuplicate(GparamEditorView view, ProjectEntry project, GparamMassDuplicatePlan plan)
    {
        var actions = new List<EditorAction>();
        var applied = 0;
        var stale = 0;

        foreach (var file in plan.Duplicates.GroupBy(e => e.Match.FileEntry))
        {
            var gparam = GetGparam(project, file.Key);

            var resolved = new List<(GPARAM.Param Group, GPARAM.IField Field, GPARAM.IFieldValue Value)>();

            foreach (var duplicate in file)
            {
                if (gparam == null ||
                    !GparamQueryRunner.TryResolve(gparam, duplicate.Match, out _, out var group, out _, out var field, out _, out var value) ||
                    value == null)
                {
                    stale++;
                    continue;
                }

                resolved.Add((group, field, value));
            }

            foreach (var byField in resolved.GroupBy(e => (e.Group, e.Field)))
            {
                var values = byField.Select(e => e.Value).Distinct().ToList();

                actions.Add(new AddValueAction(project, gparam, byField.Key.Group, byField.Key.Field, values,
                    0, plan.Offset, true));
                applied += values.Count;
            }
        }

        Execute(view, actions);

        return new Result(applied, stale);
    }
    #endregion
}
