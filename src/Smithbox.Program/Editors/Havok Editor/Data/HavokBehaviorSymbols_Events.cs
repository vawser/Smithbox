using System.Collections;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// How a field names an event
/// </summary>
public enum HavokEventUseKind
{
    /// <summary>A transition out of a state (TransitionInfo.m_eventId in a state's m_transitions)</summary>
    Transition,
    /// <summary>A transition from any state of the machine (in its m_wildcardTransitions)</summary>
    Wildcard,
    /// <summary>One of a state machine's own event fields (return to previous, random, next higher/lower)</summary>
    StateMachineField,
    /// <summary>Anything else: sent or listened for (hkbEvent / hkbEventProperty, intervals, layer, end of clip)</summary>
    Notify
}

/// <summary>
/// One field that names an event: the object it's on and, for a transition, the machine and the states it joins
/// (FromState is null for a wildcard; ToStateId is -1 when it isn't a transition)
/// </summary>
public record HavokEventUse(object Owner, FieldInfo Field, HavokEventUseKind Kind, object StateMachine, object FromState, int ToStateId)
{
    public bool IsTransition => Kind is HavokEventUseKind.Transition or HavokEventUseKind.Wildcard;
}

public partial class HavokBehaviorSymbols
{
    /// <summary>
    /// For each event index, the fields that name it. Built in one walk over the file, from the fields tagged EventID
    /// in the meta JSON (Tools/BehaviorSurvey eventsurvey: no other field names an event in ER, NR or DS3).
    /// Values of -1 (none) and out-of-range values are left out; the validator reports the latter.
    /// Plain field edits don't rebuild the symbols, so callers cache this and rebuild it after any edit.
    /// </summary>
    public List<HavokEventUse>[] BuildEventUses(ProjectEntry project)
    {
        var count = EventNames?.Count ?? 0;
        var result = new List<HavokEventUse>[count];

        for (var i = 0; i < count; i++)
            result[i] = new List<HavokEventUse>();

        if (count == 0)
            return result;

        foreach (var obj in AllObjects)
        {
            var type = obj.GetType();
            var meta = HavokMetaHelper.GetMeta(project, type);

            if (meta == null)
                continue;

            foreach (var fieldMeta in meta.Fields)
            {
                if (HavokMetaHelper.GetIndexRole(meta, fieldMeta.Field) is not HavokIndexRole.Event)
                    continue;

                var field = GetField(type, fieldMeta.Field);

                if (field == null)
                    continue;

                int index;

                try
                {
                    index = Convert.ToInt32(field.GetValue(obj));
                }
                catch
                {
                    continue;
                }

                if (index < 0 || index >= count)
                    continue;

                result[index].Add(DescribeEventUse(obj, field));
            }
        }

        return result;
    }

    private HavokEventUse DescribeEventUse(object obj, FieldInfo field)
    {
        if (IsStateMachine(obj))
            return new(obj, field, HavokEventUseKind.StateMachineField, obj, null, -1);

        // A transition: its array is held by a state (m_transitions) or by a machine (m_wildcardTransitions)
        if (field.Name == "m_eventId" && GetField(obj.GetType(), "m_toStateId") != null)
        {
            var toStateId = GetInt(obj, "m_toStateId");

            foreach (var array in GetParents(obj))
            {
                foreach (var holder in GetParents(array))
                {
                    if (IsStateMachine(holder) && ReferenceEquals(GetValue(holder, "m_wildcardTransitions"), array))
                        return new(obj, field, HavokEventUseKind.Wildcard, holder, null, toStateId);

                    if (ReferenceEquals(GetValue(holder, "m_transitions"), array) && GetField(holder.GetType(), "m_stateId") != null)
                        return new(obj, field, HavokEventUseKind.Transition, FindOwningStateMachine(holder), holder, toStateId);
                }
            }

            return new(obj, field, HavokEventUseKind.Transition, FindOwningStateMachine(obj), null, toStateId);
        }

        return new(obj, field, HavokEventUseKind.Notify, FindOwningStateMachine(obj), null, -1);
    }

    /// <summary>
    /// The state of a state machine with this ID, or null
    /// </summary>
    public static object FindState(object stateMachine, int stateId)
    {
        if (GetValue(stateMachine, "m_states") is not IList states)
            return null;

        foreach (var state in states)
        {
            if (state != null && GetInt(state, "m_stateId") == stateId)
                return state;
        }

        return null;
    }

    /// <summary>
    /// "Machine › From → To" for a transition ("Machine › any → To" for a wildcard), else "Machine · field"
    /// </summary>
    public static string DescribeEventUse(HavokEventUse use)
    {
        var machine = use.StateMachine == null ? "?" : GetValue(use.StateMachine, "m_name") as string ?? "?";

        if (use.IsTransition)
        {
            var to = GetStateName(use.StateMachine, use.ToStateId) ?? $"#{use.ToStateId}";
            var from = use.FromState == null ? "*" : GetValue(use.FromState, "m_name") as string ?? "?";

            return $"{machine} › {from} → {to}";
        }

        return $"{machine} · {GetLabel(use.Owner)}.{use.Field.Name}";
    }
}
