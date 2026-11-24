using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// State templates (Claude/Documentation/Havok Behavior Templates Plan.md): a new state copied from a template state of
/// the same machine, playing one new animation, with a copy of each wildcard into the template state, on events named
/// as the template's with its ID replaced (W_Attack3000 and W_Event3000 for an NPC attack slot).
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region State templates
    /// <summary>The machine's wildcard transitions into a state</summary>
    public static List<object> FindIncomingWildcards(object stateMachine, int stateId)
    {
        var wildcards = GetList(HavokBehaviorSymbols.GetValue(stateMachine, "m_wildcardTransitions"), "m_transitions");

        return wildcards?.Cast<object>().Where(t => t != null &&
            ToInt(HavokBehaviorSymbols.GetValue(t, "m_toStateId")) == stateId).ToList() ?? new List<object>();
    }

    /// <summary>A name with the template's ID replaced by the new one (or null when the name doesn't hold it)</summary>
    public static string ReplaceTemplateId(string name, string templateId, long newId)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(templateId) || !name.Contains(templateId, StringComparison.Ordinal))
            return null;

        return name.Replace(templateId, newId.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// The edits that add one templated state, for <see cref="Sequence"/>: each is built after the ones before it are
    /// applied, so a second new event takes the next index and the wildcards point at the state the first made.
    /// Wildcards on an event the file already has reuse it.
    /// </summary>
    public static List<Func<(EditorAction Action, string Error)>> NewTemplateStateSteps(HavokBehaviorContext context,
        object templateState, string templateId, long newId, long category, string stateName)
    {
        var symbols = context.Symbols;
        var newStateId = -1;
        var builders = new List<Func<(EditorAction, string)>>();

        builders.Add(() =>
        {
            var selector = FindStateSelector(symbols, templateState, out var placement, out var error);

            if (selector == null)
                return (null, error);

            var selectorName = ReplaceTemplateId(HavokBehaviorSymbols.GetValue(selector, "m_name") as string, templateId, newId)
                ?? $"{stateName}_CMSG";

            // The ID DuplicateGenerator gives the state (the machine's next free one), for the wildcards
            newStateId = GetFreeStateID(placement.StateMachine);

            var action = DuplicateGenerator(context, selector, placement, selectorName, stateName, copyShared: false,
                newId, out _, out error, category);

            if (action == null)
                newStateId = -1;

            return (action, error);
        });

        var templateStateId = ToInt(HavokBehaviorSymbols.GetValue(templateState, "m_stateId"));
        var machine = FindStateSelector(symbols, templateState, out var templatePlacement, out _) != null
            ? templatePlacement.StateMachine
            : null;

        foreach (var wildcard in machine != null ? FindIncomingWildcards(machine, templateStateId) : new List<object>())
        {
            var template = wildcard;

            builders.Add(() =>
            {
                // The state couldn't be made: its wildcards are left out with it (its error is already reported)
                if (newStateId < 0)
                    return (null, null);

                var templateEvent = HavokBehaviorSymbols.GetName(symbols.EventNames, ToInt(HavokBehaviorSymbols.GetValue(template, "m_eventId")));
                var eventName = ReplaceTemplateId(templateEvent, templateId, newId);

                if (eventName == null)
                    return (null, LOC.Get("HAVOK_Templates_Error_Event_Name", templateEvent ?? "-", templateId));

                var steps = new List<EditorAction>();
                var names = symbols.EventNames;
                var eventIndex = names?.Cast<object>().ToList().FindIndex(n => string.Equals(n as string, eventName, StringComparison.Ordinal)) ?? -1;

                if (eventIndex < 0)
                {
                    var eventSteps = AddEventSteps(context, eventName, out eventIndex, out var eventError);

                    if (eventSteps == null)
                        return (null, eventError);

                    steps.AddRange(eventSteps);
                }

                var transitions = GetList(HavokBehaviorSymbols.GetValue(machine, "m_wildcardTransitions"), "m_transitions");
                var transition = HavokBehaviorCloner.Clone(template, symbols, keepShared: true);

                if (transitions == null || transition == null)
                    return (null, LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "TransitionInfo"));

                HavokBehaviorSymbols.GetField(transition.GetType(), "m_eventId").SetValue(transition, eventIndex);
                HavokBehaviorSymbols.GetField(transition.GetType(), "m_toStateId").SetValue(transition, newStateId);

                steps.Add(AddToList(context, transitions, transition));

                return (Wrap(context, steps), null);
            });
        }

        return builders;
    }
    #endregion
}
