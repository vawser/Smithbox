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
    SpeedFrames
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

    public void Update(TAE.Animation? animation, float time, float playbackEnd)
    {
        SpeedMultiplier = 1;
        SpeedEvent = null;

        if (animation == null || !CFG.Current.TimeActEditor_Sim_Speed_Events)
            return;

        foreach (var ev in animation.Events)
        {
            if (time < ev.StartTime || time >= ev.MemeEndTime || !IsEventEnabled(ev))
                continue;

            if (GetSpeed(ev, time, playbackEnd) is { } speed)
            {
                // A later event in the list takes over from an earlier one
                SpeedMultiplier = Math.Max(speed, MinSpeed);
                SpeedEvent = ev;
            }
        }
    }

    private float? GetSpeed(TAE.Event ev, float time, float playbackEnd)
    {
        var role = _project.Handler.TimeActData?.GetSimulationRole(ev) ?? TimeActSimulationRole.None;

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
