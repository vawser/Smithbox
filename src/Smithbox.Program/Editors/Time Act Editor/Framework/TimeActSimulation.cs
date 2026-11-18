#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What the viewport simulates for an event, as the meta of the game names it (TAE/{Game}/Meta/{Def}.json,
/// "Simulation"). Each role reads the parameters it needs by name, so the same role works for every game whose
/// def has those parameters, whatever the event type is there.
/// </summary>
public enum TimeActSimulationRole
{
    None,

    /// <summary>The playback speed goes from SpeedAtStart to SpeedAtEnd over the event</summary>
    SpeedGradient,

    /// <summary>The event plays over AnimSpeed frames at 30 per second, however long it is</summary>
    SpeedFrames,

    /// <summary>
    /// The event invokes the BehaviorParam row of its BehaviorJudgeID, which hits with an AtkParam row. The meta of
    /// the judge field says whether it is added to the variation of the attacker or used as it is.
    /// </summary>
    Attack,

    /// <summary>As <see cref="Attack" />, for a throw (grab)</summary>
    Throw,

    /// <summary>The event invokes the BehaviorParam row of its BehaviorJudgeID, which fires a Bullet row</summary>
    Bullet,

    /// <summary>From its start until the animation ends, sets the model masks: Mask{n} 0 hides, 1 shows, others keep</summary>
    DrawMask,

    /// <summary>While active, hides the model masks whose Mask{n} is set</summary>
    HideMask,

    /// <summary>While active, shows the model masks whose Mask{n} is set</summary>
    ShowMask
}

/// <summary>
/// Turns the events of the selected animation into what the viewport does at the current time: how fast it
/// plays, and which events are switched off by a StateInfo the user has turned off. Nothing here changes the
/// time act: it is all read from the events each frame.
/// </summary>
public class TimeActSimulation
{
    public const string StateInfoField = "StateInfo";

    // A speed of 0 would stop the clock for good, as the speed is read at the time it stops at
    public const float MinSpeed = 0.01f;

    private const float FramesPerSecond = 30f;

    private readonly TimeActEditorView _view;
    private readonly ProjectEntry _project;

    // The StateInfos turned off, which stay off from one animation to the next. All are on to begin with.
    private readonly HashSet<int> _disabledStates = new();

    public TimeActSimulation(TimeActEditorView view, ProjectEntry project)
    {
        _view = view;
        _project = project;
    }

    /// <summary>
    /// How much the events speed up or slow down the playback at the current time
    /// </summary>
    public float SpeedMultiplier { get; private set; } = 1;

    /// <summary>
    /// The event that sets <see cref="SpeedMultiplier" />, or null if none does
    /// </summary>
    public TAE.Event? SpeedEvent { get; private set; }

    /// <summary>
    /// The StateInfos the events of the selected animation are gated on, lowest first
    /// </summary>
    public IReadOnlyList<int> GetStates()
    {
        // Worked out each time, as the events can be edited at any time and an animation only has so many
        return _view.Selection.SelectedAnimation?.Events
            .Select(GetStateInfo)
            .OfType<int>()
            .Distinct()
            .OrderBy(s => s)
            .ToList() ?? new List<int>();
    }

    /// <summary>
    /// The model masks the events set at the current time, by mask ID: true shows the meshes of the mask, false
    /// hides them, and null leaves them as the character has them
    /// </summary>
    public bool?[] MaskOverrides { get; } = new bool?[MaskCount];

    // More than any game's masks: the events have up to 32, and the meshes name theirs in two digits
    public const int MaskCount = 100;

    public void Update(TAE.Animation? animation, float time, float playbackEnd)
    {
        SpeedMultiplier = 1;
        SpeedEvent = null;
        Array.Clear(MaskOverrides);

        if (animation == null)
            return;

        var data = _project.Handler.TimeActData;
        var speedEvents = CFG.Current.TimeActEditor_Sim_Speed_Events;
        var masks = CFG.Current.TimeActEditor_Sim_Model_Masks;

        foreach (var ev in animation.Events)
        {
            if (time < ev.StartTime || !IsEventEnabled(ev))
                continue;

            var role = data?.GetSimulationRole(ev) ?? TimeActSimulationRole.None;
            var active = time < ev.MemeEndTime;

            // The events are gone through in their order in the list, so a later one takes over from an earlier one
            switch (role)
            {
                case TimeActSimulationRole.SpeedGradient or TimeActSimulationRole.SpeedFrames when speedEvents && active:
                    if (GetSpeed(ev, role, time, playbackEnd) is { } speed)
                    {
                        SpeedMultiplier = Math.Max(speed, MinSpeed);
                        SpeedEvent = ev;
                    }
                    break;

                // Stays in effect after it ends, as the mask it sets is kept until something else changes it
                case TimeActSimulationRole.DrawMask when masks:
                    ApplyMasks(ev, value => value switch { 0 => false, 1 => true, _ => null });
                    break;

                case TimeActSimulationRole.HideMask when masks && active:
                    ApplyMasks(ev, value => value != 0 ? false : null);
                    break;

                case TimeActSimulationRole.ShowMask when masks && active:
                    ApplyMasks(ev, value => value != 0 ? true : null);
                    break;
            }
        }
    }

    /// <summary>
    /// Sets the masks from the Mask0, Mask1 and so on of the event, as far as the event has them
    /// </summary>
    private void ApplyMasks(TAE.Event ev, Func<int, bool?> effect)
    {
        for (var i = 0; i < MaskCount; i++)
        {
            if (GetFloat(ev, $"Mask{i}") is not { } value)
                break;

            if (effect((int)value) is { } visible)
            {
                MaskOverrides[i] = visible;
            }
        }
    }

    private static float? GetSpeed(TAE.Event ev, TimeActSimulationRole role, float time, float playbackEnd)
    {
        switch (role)
        {
            case TimeActSimulationRole.SpeedGradient:
            {
                if (GetFloat(ev, "SpeedAtStart") is not { } start || GetFloat(ev, "SpeedAtEnd") is not { } end)
                    return null;

                // An event that lasts until the animation ends fades over what plays of it
                var length = Math.Min(ev.MemeEndTime, Math.Max(playbackEnd, ev.StartTime)) - ev.StartTime;

                return length > 0 ? start + (end - start) * Math.Clamp((time - ev.StartTime) / length, 0, 1) : start;
            }

            case TimeActSimulationRole.SpeedFrames:
            {
                if (GetFloat(ev, "AnimSpeed") is not { } frames || frames <= 0 ||
                    ev.EndTime >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder)
                    return null;

                return (ev.EndTime - ev.StartTime) * FramesPerSecond / frames;
            }

            default:
                return null;
        }
    }

    /// <summary>
    /// Whether the event plays, or is skipped because its StateInfo is turned off
    /// </summary>
    public bool IsEventEnabled(TAE.Event ev)
    {
        if (!CFG.Current.TimeActEditor_Sim_State_Info || _disabledStates.Count == 0)
            return true;

        return GetStateInfo(ev) is not { } state || !_disabledStates.Contains(state);
    }

    public bool IsStateEnabled(int state) => !_disabledStates.Contains(state);

    public void SetStateEnabled(int state, bool enabled)
    {
        var changed = enabled ? _disabledStates.Remove(state) : _disabledStates.Add(state);

        if (changed)
        {
            OnStatesChanged();
        }
    }

    public void EnableAllStates()
    {
        if (_disabledStates.Count == 0)
            return;

        _disabledStates.Clear();
        OnStatesChanged();
    }

    /// <summary>
    /// Turning StateInfo off as a whole changes which events play, as turning one state off does
    /// </summary>
    public void OnStateInfoToggled()
    {
        if (_disabledStates.Count > 0)
        {
            OnStatesChanged();
        }
    }

    private void OnStatesChanged()
    {
        // The effects are worked out once for the animation, from the events that play
        _view.Playback.Effects.Invalidate();
    }

    /// <summary>
    /// The StateInfo the event is gated on, or null if it isn't. Values of 0 and below are "always".
    /// </summary>
    public static int? GetStateInfo(TAE.Event ev)
    {
        if (ev.Parameters?.Values.TryGetValue(StateInfoField, out var value) != true || value == null)
            return null;

        try
        {
            var state = Convert.ToInt32(value);
            return state > 0 ? state : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// The name of a StateInfo from the project enum its field names in the meta (SP_EFFECT_TYPE in Elden Ring),
    /// or just its number
    /// </summary>
    public string GetStateName(int state)
    {
        var ev = _view.Selection.SelectedAnimation?.Events.FirstOrDefault(e => GetStateInfo(e) == state);

        if (ev != null && _project.Handler.TimeActData?.GetFieldMeta(ev, StateInfoField) is { ProjectEnum: { Length: > 0 } enumKey } &&
            ProjectEnumHelper.GetEnum(_project, enumKey)?.Options.FirstOrDefault(o => o.Key == state.ToString()) is { } option &&
            option.GetName() is { Length: > 0 } name)
        {
            return $"{state}: {name}";
        }

        return state.ToString();
    }

    private static float? GetFloat(TAE.Event ev, string field)
    {
        if (ev.Parameters?.Values.TryGetValue(field, out var value) != true || value == null)
            return null;

        try
        {
            return Convert.ToSingle(value);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
