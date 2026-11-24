using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Event edits: add, delete, and deleting one transition (Claude/Documentation/Havok Behavior Events Category Plan.md).
/// A state or machine with no transitions holds null, not an empty array (every ER and DS3 file, eventsurvey),
/// so removing the last transition of a list sets its holder's field to null.
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region Events
    /// <summary>
    /// Adds an event (its name and an hkbEventInfo with flags 0) as one undo step
    /// </summary>
    public static EditorAction AddEvent(HavokBehaviorContext context, string name, out int index, out string error)
    {
        var steps = AddEventSteps(context, name, out index, out error);

        return steps == null ? null : Wrap(context, steps);
    }

    /// <summary>
    /// What deleting an event removes: the transitions on it, and the other fields that name it (set to None)
    /// </summary>
    public record HavokEventDeletion(List<HavokEventUse> Transitions, List<HavokEventUse> Fields);

    public static HavokEventDeletion DescribeDeleteEvent(HavokBehaviorContext context, int index)
    {
        var uses = context.Symbols.BuildEventUses(context.Project);
        var mine = index >= 0 && index < uses.Length ? uses[index] : new List<HavokEventUse>();

        return new(mine.Where(u => u.IsTransition && FindTransitionList(context.Symbols, u.Owner).List != null).ToList(),
            mine.Where(u => !u.IsTransition).ToList());
    }

    /// <summary>
    /// The list a transition is in, the object holding that list's array and the holder's field,
    /// or nulls if the transition is no longer in a list (removed by an earlier step)
    /// </summary>
    private static (IList List, object Holder, string HolderField) FindTransitionList(HavokBehaviorSymbols symbols, object transition)
    {
        foreach (var array in symbols.GetParents(transition))
        {
            if (GetList(array, "m_transitions") is not IList list || !list.Cast<object>().Any(t => ReferenceEquals(t, transition)))
                continue;

            foreach (var holder in symbols.GetParents(array))
            {
                if (ReferenceEquals(HavokBehaviorSymbols.GetValue(holder, "m_wildcardTransitions"), array))
                    return (list, holder, "m_wildcardTransitions");

                if (ReferenceEquals(HavokBehaviorSymbols.GetValue(holder, "m_transitions"), array))
                    return (list, holder, "m_transitions");
            }

            return (list, null, null);
        }

        return (null, null, null);
    }

    /// <summary>
    /// The steps that remove transitions from their lists: each list's entries removed from the last to the first
    /// (so the positions stay right), and the holder's field set to null if a list is left empty
    /// </summary>
    private static List<EditorAction> RemoveTransitionSteps(HavokBehaviorContext context, IEnumerable<object> transitions)
    {
        var steps = new List<EditorAction>();
        var byList = new Dictionary<IList, (object Holder, string Field, List<int> Positions)>(ReferenceEqualityComparer.Instance);

        foreach (var transition in transitions)
        {
            var (list, holder, field) = FindTransitionList(context.Symbols, transition);

            if (list == null)
                continue;

            if (!byList.TryGetValue(list, out var entry))
                byList[list] = entry = (holder, field, new List<int>());

            for (var i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], transition) && !entry.Positions.Contains(i))
                    entry.Positions.Add(i);
            }
        }

        foreach (var (list, (holder, field, positions)) in byList)
        {
            var descending = positions.OrderByDescending(p => p).ToList();
            var removed = descending.Select(p => list[p]).ToList();

            steps.Add(Step(context,
                () => { foreach (var p in descending) list.RemoveAt(p); },
                () => { for (var i = descending.Count - 1; i >= 0; i--) list.Insert(descending[i], removed[i]); }));

            if (holder != null && list.Count == descending.Count)
                steps.Add(SetField(context, holder, field, null));
        }

        return steps;
    }

    /// <summary>
    /// Removes one transition (a state's or a wildcard)
    /// </summary>
    public static EditorAction DeleteTransition(HavokBehaviorContext context, object transition, out string error)
    {
        error = null;

        var steps = RemoveTransitionSteps(context, new[] { transition });

        if (steps.Count == 0)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Not_Referenced");
            return null;
        }

        return Wrap(context, steps);
    }

    /// <summary>
    /// Deletes an event: the transitions on it are removed, the other fields that name it become None (-1),
    /// every field naming a later event moves down one, then its name and info are removed
    /// </summary>
    public static EditorAction DeleteEvent(HavokBehaviorContext context, int index, out string error)
    {
        error = null;

        var symbols = context.Symbols;
        var names = symbols.EventNames;
        var infos = GetList(symbols.GraphData, "m_eventInfos");

        if (names == null || infos == null || index < 0 || index >= names.Count || index >= infos.Count)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
            return null;
        }

        var uses = symbols.BuildEventUses(context.Project);
        var steps = new List<EditorAction>();

        // Its transitions go; its other fields become None
        steps.AddRange(RemoveTransitionSteps(context, uses[index].Where(u => u.IsTransition).Select(u => u.Owner)));

        foreach (var use in uses[index].Where(u => !u.IsTransition))
            steps.Add(SetValue(context, use.Owner, use.Field, Convert.ChangeType(-1, use.Field.FieldType)));

        // The later events move down one
        for (var i = index + 1; i < uses.Length; i++)
        {
            foreach (var use in uses[i])
                steps.Add(SetValue(context, use.Owner, use.Field, Convert.ChangeType(i - 1, use.Field.FieldType)));
        }

        // The parallel lists
        var name = names[index];
        var info = infos[index];

        steps.Add(Step(context, () => names.RemoveAt(index), () => names.Insert(index, name)));
        steps.Add(Step(context, () => infos.RemoveAt(index), () => infos.Insert(index, info)));

        return Wrap(context, steps);
    }

    /// <summary>
    /// Deletes several events as one undo step, the highest first so each index still names its event
    /// </summary>
    public static EditorAction DeleteEvents(HavokBehaviorContext context, IEnumerable<int> indices, out string error)
    {
        var builders = indices.Distinct().OrderByDescending(i => i)
            .Select<int, Func<(EditorAction, string)>>(i => () => (DeleteEvent(context, i, out var e), e))
            .ToList();

        var action = Sequence(context, builders, out var errors);
        error = errors.FirstOrDefault();

        return action;
    }
    #endregion
}
