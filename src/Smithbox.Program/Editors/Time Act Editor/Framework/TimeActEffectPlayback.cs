#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Plays the effects of the selected animation's FFX events in the viewport, drawn by the shared FX display
/// (<see cref="FxDisplay"/>, as the FX Editor and the Cutscene Editor draw theirs).
/// <list type="bullet">
/// <item>An effect starts at its event's start. A one-shot (SpawnOneShotFFX) runs to its own end; any other is told
/// to stop at the event's end, and its particles then die out as the effect's Termination says. An event that lasts
/// until the animation ends runs until it loops.</item>
/// <item>It is placed on the event's dummy poly (or the center of the body), as its marker is, and follows it as the
/// animation plays. A one-shot that doesn't follow its dummy (IsFollowDummyPoly off) stays where it spawned, and
/// IsIgnoreDummyPolyAngle keeps only the dummy's position.</item>
/// <item>Scrubbing or looping back replays an effect from its start with the same seed. The pose is only known at
/// the frame on show, so a replay runs its whole history on the dummy's current place.</item>
/// <item>The effects come from the project's FX bank (with any unsaved FX Editor edits), so the FX Editor has to be
/// enabled for the project. The bank is indexed in the background the first time an animation with effects shows.</item>
/// </list>
/// </summary>
public sealed class TimeActEffectPlayback : IDisposable
{
    private readonly TimeActEditorView _view;
    private readonly ProjectEntry _project;

    private readonly FxDefCache _defs;
    private readonly FxCuePlayer _player;
    private FxDisplay? _display;

    // What the cues were made for: another animation or model, or an edit, makes them again
    private (TAE.Animation? Animation, TimeActContainer? Container, int Version) _cuesFor;
    private int _version;

    // Read by the cues' placement, which is asked for while the player steps the effects
    private TimeActContainer? _container;
    private ModelAnimator? _animator;

    public TimeActEffectPlayback(TimeActEditorView view, ProjectEntry project)
    {
        _view = view;
        _project = project;

        _defs = new FxDefCache(() => project.Handler.FxData);
        _player = new FxCuePlayer(_defs);
    }

    /// <summary>Whether the project has effects to show (the FX Editor is enabled for it)</summary>
    public bool IsAvailable => _project.Handler.FxData is { Bank: not null, Schema: not null };

    public FxBank? Bank => _project.Handler.FxData?.Bank;

    /// <summary>Effect events in the animation, effects running now, particles, and events whose effect the bank hasn't got</summary>
    public int EventCount => _player.CueCount;
    public int RunningCount => _player.Instances.Count;
    public int ParticleCount => _player.ParticleCount;
    public int MissingCount => _player.MissingCount;

    /// <summary>Whether the animation's effects are still loading: the clock waits, so their first showing doesn't stall</summary>
    public bool IsLoading => _player.IsLoading;
    public int LoadingCount => _player.Preload.Count;
    public int LoadedCount => _player.Preload.ReadyCount;

    /// <summary>
    /// Makes the cues again, for when an event was edited or the character's parts changed
    /// </summary>
    public void Invalidate()
    {
        _version++;
    }

    /// <param name="time">The playback clock, in seconds</param>
    public void Update(ModelAnimator? animator, float time)
    {
        var animation = _view.Selection.SelectedAnimation;
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        if (!CFG.Current.TimeActEditor_Display_Effects || animation == null || container == null || !IsAvailable)
        {
            if (_cuesFor.Animation != null || _player.CueCount > 0)
                Clear();

            return;
        }

        _container = container;
        _animator = animator;

        var key = (animation, container, _version);

        if (key != _cuesFor)
        {
            _cuesFor = key;
            _player.SetCues(BuildCues(animation, container));
        }

        // The bank is read only once an animation has something to show from it (indexing reads every sfx binder)
        if (_player.CueCount > 0 && Bank is { } bank)
        {
            bank.StartIndexing();
            bank.Update();
        }

        var (eye, forward) = GetCamera();

        _display ??= CreateDisplay();

        _player.Prepare(_display);
        _player.Update(time, eye);
        _display?.Update(_player.Instances, eye, forward);
    }

    private FxDisplay? CreateDisplay()
    {
        if (_view.RenderScene == null)
            return null;

        return new FxDisplay(new FxDisplayHost(_project, () => _view.RenderScene,
            () => FxDisplayHost.PipelineOf(_view.ViewportWindow?.Viewport)));
    }

    private (Vector3 Eye, Vector3 Forward) GetCamera()
    {
        if (_view.ViewportWindow?.Viewport is not StudioCore.Editors.Viewport.VulkanViewport vk)
            return (Vector3.Zero, Vector3.UnitZ);

        var transform = vk.ViewportCamera.CameraTransform;
        return (transform.Position, Vector3.Transform(Vector3.UnitZ, transform.RotationMatrix));
    }

    private List<FxCue> BuildCues(TAE.Animation animation, TimeActContainer container)
    {
        var cues = new List<FxCue>();
        var display = _view.Playback.EventDisplay;

        for (var i = 0; i < animation.Events.Count; i++)
        {
            var ev = animation.Events[i];

            if (!_view.Playback.Simulation.IsEventEnabled(ev) || !display.IsEffectEvent(ev, container) || GetEffectId(ev) is not { } effectId)
                continue;

            var oneShot = (ev.TypeName ?? "").Contains("OneShot", StringComparison.OrdinalIgnoreCase);
            var ignoreAngle = GetBool(ev, "IsIgnoreDummyPolyAngle") == true;

            // Only one-shots say whether they follow their dummy; the rest are held on it while they last
            var follows = GetBool(ev, "IsFollowDummyPoly") ?? true;

            var start = ev.StartTime;
            float? end = oneShot || ev.EndTime >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder
                ? null
                : Math.Max(ev.EndTime, start + 1f / 30f);

            // Where the effect was last placed, for a frame the anchor can't be found, and where a non-following one spawned
            var last = Matrix4x4.Identity;
            var spawned = false;

            Func<float, Matrix4x4> worldAt = t =>
            {
                // A replay begins again at the start, and spawns again
                if (!follows && spawned && t > start + 1e-3f)
                    return last;

                if (_container != null && display.GetEffectAnchor(ev, _container, _animator, ignoreAngle) is { } anchor)
                    last = anchor;

                spawned = true;
                return last;
            };

            cues.Add(new FxCue
            {
                EffectId = effectId,
                Start = start,
                End = end,
                WorldAt = worldAt,
                Tag = ev,
                // Two events of one effect don't show the same particles
                Seed = (uint)HashCode.Combine(effectId, i, start) | 1,
            });
        }

        return cues;
    }

    /// <summary>
    /// The effect an event spawns: the parameter the meta tags with the Particles alias, else FFXID, else the first
    /// FFXID parameter (such as FFXID_ForAll). Null if it has none, or none above 0.
    /// </summary>
    private int? GetEffectId(TAE.Event ev)
    {
        if (ev.Parameters == null)
            return null;

        var values = ev.Parameters.Values;

        var name = _project.Handler.TimeActData?.GetAliasParameter(ev, ProjectAliasType.Particles);

        if (name == null && values.ContainsKey("FFXID"))
            name = "FFXID";

        name ??= values.Keys.FirstOrDefault(k => k.StartsWith("FFXID", StringComparison.OrdinalIgnoreCase) &&
                                                !k.Contains("Deprecated", StringComparison.OrdinalIgnoreCase));

        if (name == null || !values.TryGetValue(name, out var value) || value is not IConvertible number)
            return null;

        try
        {
            var id = number.ToInt32(null);

            // The ID of a floor-determined effect is an offset into the effects of the floor material underfoot
            if (_project.Handler.TimeActData?.GetSimulationRole(ev) == TimeActSimulationRole.FloorEffect)
                return TimeActFloorEffects.Resolve(_project, id, CFG.Current.TimeActEditor_Floor_Material);

            return id > 0 ? id : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool? GetBool(TAE.Event ev, string name)
    {
        if (ev.Parameters?.Values is not { } values || !values.TryGetValue(name, out var value))
            return null;

        return value switch
        {
            bool b => b,
            IConvertible c => c.ToInt32(null) != 0,
            _ => null
        };
    }

    /// <summary>Drops what is drawn (another source, or effects turned off)</summary>
    public void Clear()
    {
        _player.Clear();
        _display?.Clear();
        _cuesFor = default;
        _container = null;
        _animator = null;
    }

    public void Dispose()
    {
        _player.Clear();
        _display?.Dispose();
        _display = null;
    }
}
