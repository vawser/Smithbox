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
    ShowMask,

    /// <summary>From its start, holds the weapons as its WeaponStyle says: in one hand each, or one in both hands</summary>
    WeaponStyle,

    /// <summary>
    /// While active, moves the weapons to another place their WepAbsorpPosParam row names, by the
    /// RHModel0ChangeType and LHModel0ChangeType of the event
    /// </summary>
    WeaponLocation,

    /// <summary>While active, hides the weapons of the hands it sets (LeftHand, RightHand and their _WithScabbard)</summary>
    HideWeapon,

    /// <summary>
    /// While active, puts a weapon on a dummy poly of the body given outright: WeaponModelType 0 the right weapon, 1 the
    /// left, at Model0DummyPolyID (Sekiro's OverrideWeaponModelLocation)
    /// </summary>
    WeaponDummyLocation,

    /// <summary>
    /// The FFXID is an offset into the effects of the floor material underfoot (SpawnFFX_FloorDetermined), see
    /// <see cref="TimeActFloorEffects" />
    /// </summary>
    FloorEffect,

    /// <summary>While active, the character turns toward its target at TurnSpeed degrees a second</summary>
    TurnSpeed,

    /// <summary>
    /// As it starts, shakes the camera with rumble cam RumbleCamID. With a DummyPolyID and FalloffStart and FalloffEnd,
    /// the shake comes from that dummy poly and fades with the distance of the camera from it.
    /// </summary>
    RumbleCam,

    /// <summary>
    /// While active, plays the additive animation of AnimType (plus 10 times AnimType2 where there is one) over the
    /// animation, at a weight going from WeightAtEventStart to WeightAtEventEnd, or of Weight. The meta's
    /// AnimationBase is added to make the animation ID (40000 in the later games).
    /// </summary>
    AdditiveAnim,

    /// <summary>From its start, plays the animation of AnimID through once as an additive layer at full weight</summary>
    AdditiveOneShot,

    /// <summary>The animation crossfades from the one before it over the length of the event</summary>
    Blend,

    /// <summary>
    /// The model fades from OpacityAtEventStart to OpacityAtEventEnd over the event, and stays at the end value after it
    /// </summary>
    Opacity
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

    /// <summary>
    /// How the weapons are held at the current time, as a SetWeaponStyle event gives it: 1 one-handed, 2 the left
    /// weapon in both hands, 3 the right weapon in both hands, 4 and 5 one-handed with a weapon transformed
    /// </summary>
    public int WeaponStyle { get; private set; } = 1;

    /// <summary>
    /// The weapon location events active at the current time, in their order in the list
    /// </summary>
    public List<TAE.Event> WeaponLocationEvents { get; } = new();

    public bool HideRightWeapon { get; private set; }
    public bool HideLeftWeapon { get; private set; }

    /// <summary>
    /// The body dummy poly a weapon location event puts each weapon on at the current time, or null
    /// </summary>
    public int? RightWeaponDummy { get; private set; }

    public int? LeftWeaponDummy { get; private set; }

    /// <summary>
    /// How far the character has turned toward the target by the current time, in degrees, from the turn speed events
    /// </summary>
    public float Turn { get; private set; }

    /// <summary>
    /// Whether the animation has any turn speed event, so that the target is worth showing
    /// </summary>
    public bool HasTurnEvents { get; private set; }

    /// <summary>
    /// The additive animations the events play at the current time: the animation ID, its weight, and how long it has
    /// played, one for each AnimType. A one-shot plays through once rather than looping
    /// </summary>
    public List<(long AnimationId, float Weight, float Time, bool OneShot)> AdditiveLayers { get; } = new();

    private readonly Dictionary<long, (long AnimationId, float Weight, float Time, bool OneShot)> _additiveByType = new();

    /// <summary>
    /// How opaque the model is at the current time, from the opacity events: 1 is as it is
    /// </summary>
    public float Opacity { get; private set; } = 1;

    public void Update(TAE.Animation? animation, float time, float playbackEnd)
    {
        SpeedMultiplier = 1;
        SpeedEvent = null;
        Array.Clear(MaskOverrides);
        WeaponStyle = CFG.Current.TimeActEditor_Weapon_Style;
        WeaponLocationEvents.Clear();
        HideRightWeapon = false;
        HideLeftWeapon = false;
        RightWeaponDummy = null;
        LeftWeaponDummy = null;
        Turn = 0;
        HasTurnEvents = false;
        AdditiveLayers.Clear();
        _additiveByType.Clear();
        Opacity = 1;

        if (animation == null)
            return;

        var data = _project.Handler.TimeActData;
        var speedEvents = CFG.Current.TimeActEditor_Sim_Speed_Events;
        var masks = CFG.Current.TimeActEditor_Sim_Model_Masks;
        var weapons = CFG.Current.TimeActEditor_Sim_Weapons;
        var additive = CFG.Current.TimeActEditor_Sim_Additive_Anims;
        var opacity = CFG.Current.TimeActEditor_Sim_Opacity;

        if (CFG.Current.TimeActEditor_Sim_Turning)
        {
            UpdateTurn(animation, data, time);
        }

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

                // Stays in effect after it ends, as the style it sets is kept until something else changes it
                case TimeActSimulationRole.WeaponStyle when weapons:
                    if (GetFloat(ev, "WeaponStyle") is { } style)
                    {
                        WeaponStyle = (int)style;
                    }
                    break;

                case TimeActSimulationRole.WeaponLocation when weapons && active:
                    WeaponLocationEvents.Add(ev);
                    break;

                // Stays at the end value after it ends, until the animation starts over
                case TimeActSimulationRole.Opacity when opacity:
                    if (GetFloat(ev, "OpacityAtEventStart") is { } from && GetFloat(ev, "OpacityAtEventEnd") is { } to)
                    {
                        var length = Math.Min(ev.MemeEndTime, Math.Max(playbackEnd, ev.StartTime)) - ev.StartTime;
                        var progress = length > 0 ? Math.Clamp((time - ev.StartTime) / length, 0, 1) : 1;
                        Opacity = Math.Clamp(from + (to - from) * progress, 0, 1);
                    }
                    break;

                case TimeActSimulationRole.AdditiveAnim when additive && active:
                    AddAdditive(ev, data, time, playbackEnd);
                    break;

                // Plays on after its event ends, until its clip has
                case TimeActSimulationRole.AdditiveOneShot when additive:
                    AddOneShot(ev, time);
                    break;

                case TimeActSimulationRole.WeaponDummyLocation when weapons && active:
                    if (GetFloat(ev, "Model0DummyPolyID") is { } dummy && dummy >= 0)
                    {
                        switch ((int?)GetFloat(ev, "WeaponModelType"))
                        {
                            case 0: RightWeaponDummy = (int)dummy; break;
                            case 1: LeftWeaponDummy = (int)dummy; break;
                        }
                    }
                    break;

                case TimeActSimulationRole.HideWeapon when weapons && active:
                    HideRightWeapon |= GetFloat(ev, "RightHand") is > 0 || GetFloat(ev, "RightHand_WithScabbard") is > 0;
                    HideLeftWeapon |= GetFloat(ev, "LeftHand") is > 0 || GetFloat(ev, "LeftHand_WithScabbard") is > 0;
                    break;
            }
        }

        AdditiveLayers.AddRange(_additiveByType.Values);
    }

    private void AddAdditive(TAE.Event ev, TimeActData? data, float time, float playbackEnd)
    {
        // Events that only play while the character has a SpEffect, unless the user takes it as having it
        if (!CFG.Current.TimeActEditor_Assume_SpEffects && NeedsSpEffect(ev))
            return;

        var weight = GetAdditiveWeight(ev, time, playbackEnd);
        var animationBase = data?.GetAnimationBase(ev) ?? 0;
        var step = data?.GetAnimationStep(ev) ?? 1;

        // An animation ID given outright takes the place of the AnimType
        if (GetFloat(ev, "AnimID") is { } direct && direct >= 0)
        {
            _additiveByType[-1 - (long)direct] = ((long)direct, weight, time - ev.StartTime, false);
            return;
        }

        // Up to two animation types (SetAdditiveAnim2IfSpEffect plays both)
        foreach (var field in new[] { "AnimType", "StayAnimType", "StayAnimType1", "StayAnimType2" })
        {
            if (GetFloat(ev, field) is not { } animType || animType < 0)
                continue;

            var id = animationBase + (long)animType * step + 10 * (long)(GetFloat(ev, "AnimType2") ?? 0);

            // The additive animation starts with the event, and each animation type plays one at a time
            _additiveByType[(long)animType] = (id, weight, time - ev.StartTime, false);
        }
    }

    /// <summary>
    /// A one-shot: the animation of AnimID plays through once from the event's start, at full weight, however long
    /// the event lasts
    /// </summary>
    private void AddOneShot(TAE.Event ev, float time)
    {
        if (GetFloat(ev, "AnimID") is not { } id || id < 0)
            return;

        // Each event is its own, so two in a row both play; the playback drops it once its clip has run out
        AdditiveLayers.Add(((long)id, 1, time - ev.StartTime, true));
    }

    // The SpEffectID_Exist (or _IsExist) of the event names a SpEffect the character must have
    private static bool NeedsSpEffect(TAE.Event ev)
    {
        return (GetFloat(ev, "SpEffectID_Exist") ?? GetFloat(ev, "SpEffectID_IsExist")) is > 0;
    }

    private static float GetAdditiveWeight(TAE.Event ev, float time, float playbackEnd)
    {
        float weight;

        if (GetFloat(ev, "WeightAtEventStart") is { } start && GetFloat(ev, "WeightAtEventEnd") is { } end)
        {
            var length = Math.Min(ev.MemeEndTime, Math.Max(playbackEnd, ev.StartTime)) - ev.StartTime;
            weight = length > 0 ? start + (end - start) * Math.Clamp((time - ev.StartTime) / length, 0, 1) : start;
        }
        else
        {
            weight = GetFloat(ev, "Weight") ?? GetFloat(ev, "AnimWeight") ?? 1;
        }

        return weight;
    }

    /// <summary>
    /// How long an animation crossfades from the one before it: the length of its Blend event, or 0 without one
    /// </summary>
    public float GetBlendDuration(TAE.Animation? animation)
    {
        var data = _project.Handler.TimeActData;

        if (animation == null || data == null)
            return 0;

        foreach (var ev in animation.Events)
        {
            if (data.GetSimulationRole(ev) == TimeActSimulationRole.Blend &&
                ev.EndTime < TAE.Event.EldenRingInfiniteLengthEventPlaceholder)
            {
                return Math.Max(ev.EndTime - ev.StartTime, 0);
            }
        }

        return 0;
    }

    /// <summary>
    /// Works out the turn from the start of the animation up to the time, so that it is the same however the time was
    /// got to (played, scrubbed or stepped). Over each stretch between the starts and ends of the turn speed events, the
    /// character turns toward the target at the speed of the last event in the list that is active there.
    /// </summary>
    private void UpdateTurn(TAE.Animation animation, TimeActData? data, float time)
    {
        var turns = new List<(TAE.Event Event, float Speed)>();

        foreach (var ev in animation.Events)
        {
            if (data?.GetSimulationRole(ev) == TimeActSimulationRole.TurnSpeed && IsEventEnabled(ev) &&
                GetFloat(ev, "TurnSpeed") is { } speed)
            {
                turns.Add((ev, speed));
            }
        }

        HasTurnEvents = turns.Count > 0;

        if (turns.Count == 0 || time <= 0)
            return;

        var target = CFG.Current.TimeActEditor_Turn_Target_Angle;

        var times = new List<float> { 0, time };
        foreach (var (ev, _) in turns)
        {
            if (ev.StartTime > 0 && ev.StartTime < time)
                times.Add(ev.StartTime);
            if (ev.MemeEndTime > 0 && ev.MemeEndTime < time)
                times.Add(ev.MemeEndTime);
        }

        times.Sort();

        var turn = 0f;

        for (var i = 1; i < times.Count; i++)
        {
            var start = times[i - 1];
            var length = times[i] - start;

            if (length <= 0)
                continue;

            var middle = start + length / 2;
            float? speed = null;

            foreach (var (ev, eventSpeed) in turns)
            {
                if (middle >= ev.StartTime && middle < ev.MemeEndTime)
                    speed = eventSpeed;
            }

            if (speed is not > 0)
                continue;

            var step = speed.Value * length;
            turn = Math.Abs(target - turn) <= step ? target : turn + Math.Sign(target - turn) * step;
        }

        Turn = turn;
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

    internal static float? GetFloat(TAE.Event ev, string field)
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
