using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// New Action: a new state with a copy of a template state's selector retargeted to a new animation, an event, and a
/// wildcard transition on it into the new state, as one undo step (Claude/Documentation/Behavior.md (Havok behavior edits))
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region New Action
    /// <summary>
    /// The state an object is in: the object if it's a state, otherwise the nearest state above it, or null
    /// </summary>
    public static object FindTemplateState(HavokBehaviorSymbols symbols, object obj)
    {
        if (obj == null)
            return null;

        if (IsStateInfo(obj))
            return obj;

        var queue = new Queue<(object Obj, int Depth)>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance) { obj };
        queue.Enqueue((obj, 0));

        while (queue.Count > 0)
        {
            var (current, depth) = queue.Dequeue();

            if (depth > 6)
                continue;

            foreach (var parent in symbols.GetParents(current))
            {
                if (IsStateInfo(parent))
                    return parent;

                if (seen.Add(parent))
                    queue.Enqueue((parent, depth + 1));
            }
        }

        return null;
    }

    /// <summary>
    /// The selector (CustomManualSelectorGenerator) a state plays, through the single-field wrappers Duplicate looks
    /// through, and where it's held; null with the reason if the state's generator isn't one
    /// </summary>
    public static object FindStateSelector(HavokBehaviorSymbols symbols, object state, out HavokPlacement placement, out string error)
    {
        placement = null;
        error = null;

        var generator = HavokBehaviorSymbols.GetValue(state, "m_generator");

        for (var depth = 0; generator != null && depth < 8; depth++)
        {
            var typeName = generator.GetType().Name;

            if (typeName == "CustomManualSelectorGenerator")
                break;

            if (!WrapperFields.TryGetValue(typeName, out var field))
            {
                error = LOC.Get("HAVOK_NewAction_Error_Not_Selector", typeName);
                return null;
            }

            generator = HavokBehaviorSymbols.GetValue(generator, field);
        }

        if (generator?.GetType().Name != "CustomManualSelectorGenerator")
        {
            error = LOC.Get("HAVOK_NewAction_Error_Not_Selector", generator?.GetType().Name ?? "-");
            return null;
        }

        placement = FindPlacements(symbols, generator)
            .FirstOrDefault(p => p.Kind is PlacementKind.State && ReferenceEquals(p.Holder, state));

        if (placement == null)
        {
            error = LOC.Get("HAVOK_NewAction_Error_Not_Selector", generator.GetType().Name);
            return null;
        }

        return generator;
    }

    /// <summary>
    /// The machine's wildcard transition into a state, or null
    /// </summary>
    public static object FindIncomingWildcard(object stateMachine, int stateId)
    {
        var wildcards = GetList(HavokBehaviorSymbols.GetValue(stateMachine, "m_wildcardTransitions"), "m_transitions");

        return wildcards?.Cast<object>().FirstOrDefault(t => t != null &&
            ToInt(HavokBehaviorSymbols.GetValue(t, "m_toStateId")) == stateId);
    }

    /// <summary>
    /// Builds the new action. With addWildcard, newEventName adds an event (or eventIndex names an existing one) and a
    /// wildcard on it leads into the new state, copied from the template's own wildcard so it behaves alike.
    /// </summary>
    public static EditorAction NewAction(HavokBehaviorContext context, object templateState, string stateName, string selectorName,
        long animId, bool addWildcard, string newEventName, int eventIndex, out object newSelector, out string error)
    {
        newSelector = null;

        var symbols = context.Symbols;
        var selector = FindStateSelector(symbols, templateState, out var placement, out error);

        if (selector == null)
            return null;

        var machine = placement.StateMachine;
        var newStateId = GetFreeStateID(machine);

        // The state, its selector and the selector's clips, retargeted
        var duplicate = DuplicateGenerator(context, selector, placement, selectorName?.Trim(), stateName?.Trim(),
            copyShared: false, animId, out newSelector, out error);

        if (duplicate == null)
            return null;

        var steps = new List<EditorAction> { duplicate };

        if (addWildcard)
        {
            // The event
            if (newEventName != null)
            {
                var eventSteps = AddEventSteps(context, newEventName.Trim(), out eventIndex, out error);

                if (eventSteps == null)
                    return null;

                steps.AddRange(eventSteps);
            }
            else if (HavokBehaviorSymbols.GetName(symbols.EventNames, eventIndex) == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_No_Event");
                return null;
            }

            // The wildcard: a copy of the one into the template state, else of the machine's last one
            var templateId = ToInt(HavokBehaviorSymbols.GetValue(templateState, "m_stateId"));
            var array = HavokBehaviorSymbols.GetValue(machine, "m_wildcardTransitions");

            if (array == null)
            {
                array = NewFieldValue(machine, "m_wildcardTransitions");

                if (array == null)
                {
                    error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "TransitionInfoArray");
                    return null;
                }

                steps.Add(SetField(context, machine, "m_wildcardTransitions", array));
            }

            var transitions = GetList(array, "m_transitions");
            var template = FindIncomingWildcard(machine, templateId) ?? transitions?.Cast<object>().LastOrDefault(t => t != null);

            var transition = template != null
                ? HavokBehaviorCloner.Clone(template, symbols, keepShared: true)
                : NewEntry(array, "m_transitions");

            if (transitions == null || transition == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "TransitionInfo");
                return null;
            }

            HavokBehaviorSymbols.GetField(transition.GetType(), "m_eventId").SetValue(transition, eventIndex);
            HavokBehaviorSymbols.GetField(transition.GetType(), "m_toStateId").SetValue(transition, newStateId);

            // A new wildcard without a template takes the usual flags (see AddTransition)
            if (template == null)
            {
                var flagsField = HavokBehaviorSymbols.GetField(transition.GetType(), "m_flags");
                flagsField.SetValue(transition, ToFieldValue(flagsField.FieldType, 512 | 1024 | 2048));
            }

            steps.Add(AddToList(context, transitions, transition));
        }

        return Wrap(context, steps);
    }

    /// <summary>
    /// The other selectors of the file with this animation ID
    /// </summary>
    public static int CountSelectorsWithAnimId(HavokBehaviorSymbols symbols, long animId)
    {
        return symbols.Selectors.Count(s => HavokBehaviorSymbols.GetValue(s, "m_animId") is { } v && Convert.ToInt64(v) == animId);
    }
    #endregion
}
