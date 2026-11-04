using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Carries out a plan as one action, so a single undo takes the whole edit back
/// </summary>
public static class TimeActMassEditApplier
{
    public record Result(int Applied, int Stale);

    /// <summary>
    /// Each event is checked against the values it had when the plan was made. An event changed since, by hand or
    /// by another edit, is left alone and counted as stale, so the edit never builds on values it didn't show.
    /// </summary>
    public static Result Apply(TimeActEditorView view, TimeActMassEditPlan plan, TAE.Template.BankTemplate template)
    {
        var actions = new List<ViewportAction>();

        var applied = 0;
        var stale = 0;

        foreach (var planned in plan.Events)
        {
            if (!TimeActQueryRunner.TryResolve(planned.Match, out var entry, out var animation, out var ev) || ev == null)
            {
                stale++;
                continue;
            }

            if (ev.StartTime != planned.OldStart || ev.EndTime != planned.OldEnd)
            {
                stale++;
                continue;
            }

            if (planned.Parameters.Count > 0)
            {
                // The parameters only exist once the template is on the event. Templates are only applied where
                // they write the event back exactly, which is what the plan read the values through.
                entry.ApplyTemplate(animation, template);

                if (!HasOldValues(ev, planned))
                {
                    stale++;
                    continue;
                }
            }

            if (planned.TimesChanged)
            {
                actions.Add(new TimeActMoveEventAction(entry, ev, planned.OldStart, planned.OldEnd, planned.NewStart, planned.NewEnd));
            }

            foreach (var parameter in planned.Parameters)
            {
                actions.Add(new TimeActSetParameterAction(entry, ev, parameter.Key, parameter.NewValue));
            }

            applied++;
        }

        // Each action sets values rather than moving events, so they undo correctly in any order
        if (actions.Count == 1)
        {
            view.ViewportActionManager.ExecuteAction(actions[0]);
        }
        else if (actions.Count > 1)
        {
            view.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
        }

        return new Result(applied, stale);
    }

    private static bool HasOldValues(TAE.Event ev, TimeActPlannedEvent planned)
    {
        if (ev.Parameters == null)
            return false;

        foreach (var parameter in planned.Parameters)
        {
            if (!ev.Parameters.Values.ContainsKey(parameter.Key))
                return false;

            if (!Equals(ev.Parameters[parameter.Key], parameter.OldValue))
                return false;
        }

        return true;
    }
}
