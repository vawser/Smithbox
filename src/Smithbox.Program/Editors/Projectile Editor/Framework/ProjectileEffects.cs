#nullable enable
using Andre.Formats;
using StudioCore.Editors.FxEditor;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The bullets' effects in the viewport, drawn by the shared FX display (<see cref="FxDisplay"/>, as the FX, Cutscene
/// and Time Act Editors draw theirs):
/// <list type="bullet">
/// <item>A bullet's flight effect (sfxId_Bullet) runs from its spawn to its end, following it and facing the way it
/// flies. At its end the effect is told to stop, and its particles die out as its Termination says.</item>
/// <item>Its hit effect (sfxId_Hit) starts where it hit the ground, the wall or the target, and stays there.</item>
/// <item>Scrubbing or looping back replays the effects with the same seeds, so a time always shows the same particles.</item>
/// <item>The effects come from the project's FX bank (with any unsaved FX Editor edits), so the FX Editor has to be
/// enabled for the project. The bank is indexed in the background the first time a bullet with effects is shown.</item>
/// </list>
/// </summary>
public sealed class ProjectileEffects : IDisposable
{
    // How long a hit effect is let run before it is told to stop, for the effects that would otherwise loop
    private const float HitEffectLength = 5.0f;

    // How far either side of a time the direction of flight is measured over
    private const float DirectionSpan = 1f / 30f;

    private readonly ProjectileEditorView View;
    private readonly FxDefCache Defs;
    private readonly FxCuePlayer Player;
    private FxDisplay? Display;

    // What the cues were made for: another simulation, or a display setting
    private (int Version, bool Flights, bool Hits)? CuesFor;

    public ProjectileEffects(ProjectileEditorView view)
    {
        View = view;
        var project = view.Project;

        Defs = new FxDefCache(() => project.Handler.FxData);
        Player = new FxCuePlayer(Defs);
    }

    /// <summary>Whether the project has effects to show (the FX Editor is enabled for it)</summary>
    public bool IsAvailable => View.Project.Handler.FxData is { Bank: not null, Schema: not null } && View.RenderScene != null;

    /// <summary>Whether any effects are to be shown: FX on (Simulation Filters), with the flight or hit effects on</summary>
    public static bool IsShowing => CFG.Current.ProjectileEditor.ProjectileEditor_Display_Fx &&
                                    (CFG.Current.ProjectileEditor.ProjectileEditor_Display_Effects || CFG.Current.ProjectileEditor.ProjectileEditor_Display_Hit_Effects);

    public FxBank? Bank => View.Project.Handler.FxData?.Bank;

    /// <summary>Effects the bullets have, effects running now, effects the bank hasn't got, particles</summary>
    public int EffectCount => Player.CueCount;
    public int RunningCount => Player.Instances.Count;
    public int MissingCount => Player.MissingCount;
    public int ParticleCount => Player.ParticleCount;

    /// <summary>Whether the bullets' effects are still loading: the simulation waits, so their first showing doesn't stall</summary>
    public bool IsLoading => Player.IsLoading;
    public int LoadingCount => Player.Preload.Count;
    public int LoadedCount => Player.Preload.ReadyCount;

    public void Update(Vector3 eye, Vector3 forward)
    {
        var simulator = View.Simulator;

        if (!IsShowing || simulator.Result == null || !IsAvailable)
        {
            if (CuesFor != null || Player.CueCount > 0)
                Clear();

            return;
        }

        var key = (simulator.Version, CFG.Current.ProjectileEditor.ProjectileEditor_Display_Effects, CFG.Current.ProjectileEditor.ProjectileEditor_Display_Hit_Effects);

        if (key != CuesFor)
        {
            CuesFor = key;
            Player.SetCues(BuildCues(simulator.Result, View.GetBulletParam()));
        }

        // The bank is read only once something has an effect to show from it (indexing reads every sfx binder)
        if (Player.CueCount > 0 && Bank is { } bank)
        {
            bank.StartIndexing();
            bank.Update();
        }

        Display ??= new FxDisplay(new FxDisplayHost(View.Project, () => View.RenderScene!,
            () => FxDisplayHost.PipelineOf(View.ViewportWindow.Viewport)));

        Player.Prepare(Display);
        Player.Update(simulator.Time, eye);
        Display.Update(Player.Instances, eye, forward);
    }

    private static List<FxCue> BuildCues(SimulationResult result, Param? bulletParam)
    {
        var cues = new List<FxCue>();

        if (bulletParam == null)
            return cues;

        var flights = CFG.Current.ProjectileEditor.ProjectileEditor_Display_Effects;
        var hits = CFG.Current.ProjectileEditor.ProjectileEditor_Display_Hit_Effects;

        for (var i = 0; i < result.Tracks.Count; i++)
        {
            var track = result.Tracks[i];

            if (bulletParam[track.RowId] is not { } row)
                continue;

            var flight = ProjectileParams.GetFlightEffect(row);

            if (flights && flight > 0)
            {
                cues.Add(new FxCue
                {
                    EffectId = flight,
                    Start = track.SpawnTime,
                    End = Math.Max(track.EndTime, track.SpawnTime + DirectionSpan),
                    WorldAt = t => FlightWorld(track, t),
                    Tag = track,
                    // Two bullets of one row don't show the same particles
                    Seed = (uint)HashCode.Combine(flight, i) | 1,
                });
            }

            var hit = ProjectileParams.GetHitEffect(row);

            if (hits && hit > 0 && track.End is ProjectileEnd.HitGround or ProjectileEnd.HitWall or ProjectileEnd.HitTarget)
            {
                // Where it landed, facing the way it was flying
                var landed = FlightWorld(track, track.EndTime);

                cues.Add(new FxCue
                {
                    EffectId = hit,
                    Start = track.EndTime,
                    End = track.EndTime + HitEffectLength,
                    WorldAt = _ => landed,
                    Tag = track,
                    Seed = (uint)HashCode.Combine(hit, i, 1) | 1,
                });
            }
        }

        return cues;
    }

    /// <summary>Where a bullet is at a time, facing the way it flies (+Z along its flight, +Y as near to up as it can be)</summary>
    public static Matrix4x4 FlightWorld(ProjectileTrack track, float time)
    {
        time = Math.Clamp(time, track.SpawnTime, track.EndTime);

        var position = track.GetPosition(time);

        var before = track.GetPosition(Math.Max(track.SpawnTime, time - DirectionSpan));
        var after = track.GetPosition(Math.Min(track.EndTime, time + DirectionSpan));
        var direction = after - before;

        if (direction.LengthSquared() < 1e-8f)
            direction = Vector3.UnitZ;

        var forward = Vector3.Normalize(direction);
        var up = MathF.Abs(Vector3.Dot(forward, Vector3.UnitY)) > 0.99f ? Vector3.UnitX : Vector3.UnitY;
        var right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Cross(forward, right);

        return new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            position.X, position.Y, position.Z, 1);
    }

    /// <summary>Drops what is drawn (another row, or effects turned off)</summary>
    public void Clear()
    {
        Player.Clear();
        Display?.Clear();
        CuesFor = null;
    }

    public void Dispose()
    {
        Player.Clear();
        Display?.Dispose();
        Display = null;
    }
}
