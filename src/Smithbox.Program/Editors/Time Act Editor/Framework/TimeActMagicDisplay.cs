#nullable enable
using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Flies the bullets of the magic picked in the Simulation tool from each cast event (CastHighlightedMagic) the clock has
/// passed, with the Projectile Editor's simulation. A cast fires from where its dummy poly is as the event starts, the
/// way the character faces then, and its bullets fly on from there in the world, whatever the character does after.
/// The scene (the ground's collision, the wall and the target) is the Projectile Editor's, set in its settings; the
/// ground is the floor the character stands on.
/// </summary>
public class TimeActMagicDisplay
{
    private static readonly TimeSpan BuildInterval = TimeSpan.FromMilliseconds(33);

    // From the simulation's forward (+Z) to the character's (-Z)
    private static readonly Matrix4x4 SimulationToCharacter = Matrix4x4.CreateRotationY(MathF.PI);

    // How long a hit effect is let run before it is told to stop, for the effects that would otherwise loop
    private const float HitEffectLength = 5.0f;

    // The shortest a flight effect runs, so a bullet that ends at once still shows it
    private const float MinFlightLength = 1f / 30f;

    // The bullets' effects, drawn by the shared FX display as the animation's own effects are
    private readonly FxDefCache _defs;
    private readonly FxCuePlayer _player;
    private FxDisplay? _fxDisplay;
    private (int Version, bool Shown)? _cuesFor;

    private readonly TimeActEditorView _view;
    private readonly ProjectEntry _project;

    private readonly ProjectileDebugLines _lines = new() { Overlay = true };
    private bool _shown;

    /// <summary>
    /// A cast the clock has passed: where it fired from, and the flight of its bullets
    /// </summary>
    private sealed class Cast
    {
        /// <summary>From the space of the simulation (bullets fly along +Z from above the origin) to the world</summary>
        public Matrix4x4 Anchor;

        /// <summary>How far above the floor the dummy poly was as the cast fired</summary>
        public float LaunchHeight;

        public SimulationSettings? Settings;
        public SimulationResult? Result;
        public object? ResultKey;
    }

    private readonly Dictionary<TAE.Event, Cast> _casts = new();
    private readonly HashSet<TAE.Event> _passed = new();
    private TAE.Animation? _animation;

    // Bumped whenever a cast or its flight changes, so the lines are built again
    private int _version;
    private (int Version, float Time) _builtFor = (-1, -1);
    private DateTime _lastBuild;

    public TimeActMagicDisplay(TimeActEditorView view, ProjectEntry project)
    {
        _view = view;
        _project = project;

        _defs = new FxDefCache(() => project.Handler.FxData);
        _player = new FxCuePlayer(_defs);
    }

    /// <summary>Whether the bullets' effects are still loading: the clock waits, so their first showing doesn't stall</summary>
    public bool IsLoading => _player.IsLoading;

    /// <summary>Effects the casts' bullets have, effects running now, and effects the bank hasn't got</summary>
    public int EffectCount => _player.CueCount;
    public int RunningEffectCount => _player.Instances.Count;
    public int MissingEffectCount => _player.MissingCount;

    /// <summary>
    /// The flight of the latest cast the clock has passed, or null if none has been
    /// </summary>
    public SimulationResult? LatestResult { get; private set; }

    /// <summary>
    /// The magic row a cast uses, from the CFG, or null if there is none
    /// </summary>
    public Param.Row? GetMagic()
    {
        var id = CFG.Current.TimeActEditor_Cast_Magic_ID;

        if (id < 0 || ProjectileParams.GetSpellParamName(_project) is not { } paramName)
            return null;

        return ProjectileParams.GetParam(_project, paramName)?[id];
    }

    /// <summary>
    /// Called after the event display has found the dummy polys for the current pose
    /// </summary>
    public void Update(ModelAnimator? animator, float time, float playbackEnd)
    {
        _lines.Update();

        var animation = _view.Selection.SelectedAnimation;
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;
        var data = _project.Handler.TimeActData;

        if (animation != _animation)
        {
            _animation = animation;
            ClearCasts();
        }

        var magic = GetMagic();
        var bulletParam = ProjectileParams.GetSpellBulletParam(_project);

        if (!CFG.Current.TimeActEditor_Sim_Magic || animation == null || container == null || data == null ||
            _view.RenderScene == null || magic == null || bulletParam == null)
        {
            ClearCasts();
            Hide();
            ClearEffects();
            return;
        }

        var charged = CFG.Current.TimeActEditor_Cast_Magic_Charged;
        var bullets = ProjectileParams.GetCastBullets(magic, charged);

        // Any edit, undo or redo in the Param Editor may have changed the magic or its bullets, as may a reload
        var paramKey = (_project.Handler.ParamEditor?.ActionManager.PeekUndoAction(), magic, bulletParam, charged,
            string.Join(",", bullets));

        // The cast gives the way the first bullets point: the way the character faces
        var baseSettings = ProjectileSimulator.CurrentSettings with { Orientation = CastOrientation.Forward };

        _passed.Clear();
        LatestResult = null;

        foreach (var ev in animation.Events)
        {
            if (time < ev.StartTime || data.GetSimulationRole(ev) != TimeActSimulationRole.CastMagic ||
                !_view.Playback.Simulation.IsEventEnabled(ev))
            {
                continue;
            }

            _passed.Add(ev);

            if (!_casts.TryGetValue(ev, out var cast))
            {
                // Fired as the clock passes the start of the event, from the pose then
                cast = Capture(ev, container, animator, baseSettings.LaunchHeight);
                _casts[ev] = cast;
                _version++;
            }

            // The emitter lasts as long as the event, for the bullets that fire again every shootInterval
            var end = Math.Min(ev.MemeEndTime, Math.Max(playbackEnd, ev.StartTime));
            var settings = baseSettings with
            {
                LaunchHeight = cast.LaunchHeight,
                EmitterDuration = Math.Max(end - ev.StartTime, 0)
            };

            var key = (paramKey, settings);

            if (!Equals(key, cast.ResultKey))
            {
                cast.ResultKey = key;
                cast.Settings = settings;
                cast.Result = bullets.Count > 0 ? Run(bullets, bulletParam, settings) : null;
                _version++;
            }

            LatestResult = cast.Result ?? LatestResult;
        }

        // The clock has gone back to before them, as when the animation loops
        foreach (var ev in _casts.Keys.Where(ev => !_passed.Contains(ev)).ToList())
        {
            _casts.Remove(ev);
            _version++;
        }

        Draw(time);
        UpdateEffects(time, bulletParam);
    }

    /// <summary>
    /// Forgets the casts, for when the source changes
    /// </summary>
    public void Clear()
    {
        ClearCasts();
        Hide();
        ClearEffects();
    }

    private void ClearEffects()
    {
        if (_cuesFor == null && _player.CueCount == 0)
            return;

        _player.Clear();
        _fxDisplay?.Clear();
        _cuesFor = null;
    }

    /// <summary>
    /// Plays the flight effect (sfxId_Bullet) of each bullet of the casts along its flight, and its hit effect
    /// (sfxId_Hit) where it hit, on the animation's clock. They come from the project's FX bank, so the FX Editor has
    /// to be enabled for the project, and they show while the Time Act Editor's effects do.
    /// </summary>
    private void UpdateEffects(float time, Param bulletParam)
    {
        if (!CFG.Current.TimeActEditor_Display_Effects || _project.Handler.FxData is not { Bank: not null, Schema: not null } ||
            _view.RenderScene == null)
        {
            ClearEffects();
            return;
        }

        // The cues follow the casts: one passed, gone back before, or flown again
        var key = (_version, true);

        if (key != _cuesFor)
        {
            _cuesFor = key;
            _player.SetCues(BuildCues(bulletParam));
        }

        // The bank is read only once something has an effect to show from it (indexing reads every sfx binder)
        if (_player.CueCount > 0 && _project.Handler.FxData?.Bank is { } bank)
        {
            bank.StartIndexing();
            bank.Update();
        }

        _fxDisplay ??= new FxDisplay(new FxDisplayHost(_project, () => _view.RenderScene,
            () => FxDisplayHost.PipelineOf(_view.ViewportWindow?.Viewport)));

        var (eye, forward) = GetCamera();

        _player.Prepare(_fxDisplay);
        _player.Update(time, eye);
        _fxDisplay.Update(_player.Instances, eye, forward);
    }

    private List<FxCue> BuildCues(Param bulletParam)
    {
        var cues = new List<FxCue>();

        foreach (var (ev, cast) in _casts)
        {
            if (cast.Result is not { } result)
                continue;

            var start = ev.StartTime;
            var anchor = cast.Anchor;

            for (var i = 0; i < result.Tracks.Count; i++)
            {
                var track = result.Tracks[i];

                if (bulletParam[track.RowId] is not { } row)
                    continue;

                // The tracks keep the simulation's clock, which starts with the event
                var flight = ProjectileParams.GetFlightEffect(row);

                if (flight > 0)
                {
                    cues.Add(new FxCue
                    {
                        EffectId = flight,
                        Start = start + track.SpawnTime,
                        End = start + Math.Max(track.EndTime, track.SpawnTime + MinFlightLength),
                        WorldAt = t => ProjectileEffects.FlightWorld(track, t - start) * anchor,
                        Tag = track,
                        // Two bullets of one row don't show the same particles
                        Seed = (uint)HashCode.Combine(flight, i, start) | 1
                    });
                }

                var hit = ProjectileParams.GetHitEffect(row);

                if (hit > 0 && track.End is ProjectileEnd.HitGround or ProjectileEnd.HitWall or ProjectileEnd.HitTarget)
                {
                    // Where it landed, facing the way it was flying
                    var landed = ProjectileEffects.FlightWorld(track, track.EndTime) * anchor;

                    cues.Add(new FxCue
                    {
                        EffectId = hit,
                        Start = start + track.EndTime,
                        End = start + track.EndTime + HitEffectLength,
                        WorldAt = _ => landed,
                        Tag = track,
                        Seed = (uint)HashCode.Combine(hit, i, start, 1) | 1
                    });
                }
            }
        }

        return cues;
    }

    private (Vector3 Eye, Vector3 Forward) GetCamera()
    {
        if (_view.ViewportWindow?.Viewport is not StudioCore.Editors.Viewport.VulkanViewport vk)
            return (Vector3.Zero, Vector3.UnitZ);

        var transform = vk.ViewportCamera.CameraTransform;
        return (transform.Position, Vector3.Transform(Vector3.UnitZ, transform.RotationMatrix));
    }

    private void ClearCasts()
    {
        if (_casts.Count == 0)
            return;

        _casts.Clear();
        LatestResult = null;
        _version++;
    }

    private Cast Capture(TAE.Event ev, TimeActContainer container, ModelAnimator? animator, float fallbackHeight)
    {
        // The root carries the character's facing, from the root motion, the turn and a combo's offset
        Matrix4x4 root = animator?.RootMotion ?? Matrix4x4.Identity;
        Matrix4x4 facing = root with { M41 = 0, M42 = 0, M43 = 0 };
        var floor = root.Translation.Y;

        // Without the dummy poly, from the height the Projectile Editor launches from
        Vector3 from = _view.Playback.EventDisplay.GetEventPosition(ev, container, animator)
                       ?? root.Translation + Vector3.UnitY * fallbackHeight;

        // The simulation flies along +Z, but a character faces along -Z in the viewport, so its space is turned around
        return new Cast
        {
            Anchor = SimulationToCharacter * facing * Matrix4x4.CreateTranslation(from.X, floor, from.Z),
            LaunchHeight = Math.Max(from.Y - floor, 0)
        };
    }

    private SimulationResult Run(IReadOnlyList<int> bullets, Param bulletParam, SimulationSettings settings)
    {
        return ProjectileSimulation.Run(bullets, id =>
        {
            var row = bulletParam[id];

            return row != null ? BulletDefinition.FromRow(row, FindAttack) : null!;
        }, settings);
    }

    private AttackDefinition FindAttack(int id)
    {
        var row = ProjectileParams.FindAttackRow(_project, id, out _);

        return row != null ? AttackDefinition.FromRow(row) : null!;
    }

    /// <summary>
    /// The bullets of each cast at the current time, as the Projectile Editor draws them, moved to where the cast fired
    /// </summary>
    private void Draw(float time)
    {
        if (_lines.IsUpdating)
            return;

        if (_builtFor == (_version, time))
            return;

        // A moving time builds at most every BuildInterval; a change of cast shows at once
        if (_builtFor.Version == _version && DateTime.UtcNow - _lastBuild < BuildInterval)
            return;

        _builtFor = (_version, time);
        _lastBuild = DateTime.UtcNow;

        var world = new Dictionary<Color, List<(Vector3, Vector3)>>();

        foreach (var (ev, cast) in _casts)
        {
            if (cast.Result is not { } result || cast.Settings is not { } settings)
                continue;

            var local = new Dictionary<Color, List<(Vector3, Vector3)>>();
            var flightTime = time - ev.StartTime;

            ProjectileViewportWindow.AddTrails(local, result, flightTime);
            ProjectileViewportWindow.AddProjectiles(local, result, flightTime, true, true);
            ProjectileViewportWindow.AddImpacts(local, result, flightTime);

            if (settings.SimulateTarget)
            {
                var target = ProjectileViewportWindow.Lines(local, ProjectileDebugLines.ToColor(ProjectileFormat.TargetColor));
                var position = settings.GetTargetPosition(flightTime);

                target.Add((position, position with { Y = 0 }));
                ProjectileDebugLines.AddSphere(target, position, settings.TargetRadius, 24);
            }

            if (settings.SimulateWall)
            {
                ProjectileViewportWindow.AddWall(
                    ProjectileViewportWindow.Lines(local, ProjectileDebugLines.ToColor(ProjectileFormat.WallColor)), settings);
            }

            foreach (var (color, list) in local)
            {
                var into = ProjectileViewportWindow.Lines(world, color);

                foreach (var (a, b) in list)
                {
                    into.Add((Vector3.Transform(a, cast.Anchor), Vector3.Transform(b, cast.Anchor)));
                }
            }
        }

        if (world.Count == 0 && !_shown)
            return;

        _lines.Show(_view.RenderScene, world);
        _shown = world.Count > 0;
    }

    private void Hide()
    {
        _builtFor = (-1, -1);

        if (!_shown)
            return;

        _lines.Clear();
        _shown = false;
    }
}
