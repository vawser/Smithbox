#nullable enable
using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The Projectile Editor's simulation of the selected row: a bullet row, or a spell row and the bullets in its slots.
/// It runs again when the row or the settings change, and after any param edit, undo or redo (all of which go through
/// the Param Editor's action manager). It then plays back on its own clock.
/// </summary>
public sealed class ProjectileSimulator
{
    private readonly ProjectEntry Project;

    /// <summary>The param of the selected row, and the row</summary>
    public string? ParamName { get; private set; }
    public Param.Row? Row { get; private set; }

    /// <summary>The flight of every bullet the row fires, or null when it can't be simulated</summary>
    public SimulationResult? Result { get; private set; }

    /// <summary>The bullets of the selected spell that are turned off</summary>
    public readonly HashSet<int> DisabledBullets = new();

    /// <summary>Bumped every time the simulation runs again, for what is built from it</summary>
    public int Version { get; private set; }

    public bool Playing;
    public float Time;

    private EditorAction? LastAction;
    private Param? LastParam;
    private bool RebuildPending;

    public ProjectileSimulator(ProjectEntry project)
    {
        Project = project;
    }

    public SimulationSettings Settings
    {
        get
        {
            var cfg = CFG.Current;

            return new SimulationSettings(cfg.ProjectileEditor_LaunchHeight,
                cfg.ProjectileEditor_EmitHeight,
                cfg.ProjectileEditor_FixedEmitRadius,
                cfg.ProjectileEditor_BehindTargetHeight,
                cfg.ProjectileEditor_BehindTargetDistance,
                cfg.ProjectileEditor_Orientation,
                cfg.ProjectileEditor_GroundCollision,
                cfg.ProjectileEditor_SimulateWall,
                cfg.ProjectileEditor_WallDistance,
                cfg.ProjectileEditor_WallWidth,
                cfg.ProjectileEditor_WallHeight,
                cfg.ProjectileEditor_Surface,
                cfg.ProjectileEditor_SimulateTarget,
                cfg.ProjectileEditor_TargetPosition,
                cfg.ProjectileEditor_TargetRadius,
                cfg.ProjectileEditor_TargetMotion,
                cfg.ProjectileEditor_TargetSpeed,
                cfg.ProjectileEditor_TargetRange,
                cfg.ProjectileEditor_Seed,
                cfg.ProjectileEditor_EmitterDuration,
                cfg.ProjectileEditor_DistanceScale,
                cfg.ProjectileEditor_DistanceUpEffect);
        }
    }

    public bool IsSpell => ParamName != null && ProjectileParams.IsSpellParam(Project, ParamName);

    /// <summary>Shows another row, starting its playback from the beginning</summary>
    public void Select(string paramName, Param.Row row)
    {
        if (ParamName == paramName && Row == row)
            return;

        ParamName = paramName;
        Row = row;
        LastParam = ProjectileParams.GetParam(Project, paramName);

        ResetSpellBullets();
        Rebuild();

        Time = 0;
        Playing = Result != null;
    }

    public void Clear()
    {
        ParamName = null;
        Row = null;
        Result = null;
        Playing = false;
        Time = 0;
        Version++;
    }

    /// <summary>Runs the simulation again on the next update (a setting or the spell's bullets changed)</summary>
    public void Invalidate() => RebuildPending = true;

    public void Update(float dt)
    {
        var lastAction = Project.Handler.ParamEditor?.ActionManager.PeekUndoAction();
        var param = ParamName != null ? ProjectileParams.GetParam(Project, ParamName) : null;

        // Only after an edit, undo or redo, or the params being reloaded, can the row have gone from its param
        if (lastAction != LastAction || param != LastParam)
        {
            LastAction = lastAction;
            LastParam = param;
            RebuildPending = true;

            if (Row != null && !KeepRow(param))
                return;
        }

        if (RebuildPending)
            Rebuild();

        UpdatePlayback(dt);
    }

    /// <summary>
    /// Keeps the selected row if its param still holds it; otherwise takes the row with its ID (the params were
    /// reloaded), or shows nothing if there is none (it was deleted). False when nothing is shown any more.
    /// </summary>
    private bool KeepRow(Param? param)
    {
        if (param != null && param.Rows.Contains(Row!))
            return true;

        if (param?[Row!.ID] is { } replacement)
        {
            Row = replacement;
            return true;
        }

        Clear();
        return false;
    }

    /// <summary>
    /// Starts on the normal cast of a spell. Charged and unused slots are there to be turned on, but a spell made up
    /// of nothing else starts with all of them so that it doesn't look empty.
    /// </summary>
    private void ResetSpellBullets()
    {
        DisabledBullets.Clear();

        if (!IsSpell || Row == null)
            return;

        var bullets = BulletDefinition.GetSpellBullets(Row);

        if (bullets.Any(bullet => bullet.Slot is SpellSlotType.Default))
        {
            DisabledBullets.UnionWith(bullets.Where(bullet => bullet.Slot is not SpellSlotType.Default)
                .Select(bullet => bullet.Id));
        }
    }

    private void Rebuild()
    {
        RebuildPending = false;
        Result = null;
        Version++;

        if (ParamName == null || Row == null)
            return;

        var isSpell = IsSpell;

        var param = isSpell ? ProjectileParams.GetSpellBulletParam(Project) : ProjectileParams.GetParam(Project, ParamName);
        IReadOnlyList<int> roots = isSpell
            ? BulletDefinition.GetSpellBullets(Row).Select(bullet => bullet.Id).Where(id => !DisabledBullets.Contains(id)).ToArray()
            : new[] { Row.ID };

        if (param == null || roots.Count == 0 || !isSpell && !BulletDefinition.IsReadable(Row))
        {
            Playing = false;
            return;
        }

        Result = ProjectileSimulation.Run(roots, id =>
        {
            var row = param[id];

            return row != null ? BulletDefinition.FromRow(row, FindAttack) : null;
        }, Settings);

        Time = Math.Min(Time, Result.Duration);
    }

    /// <summary>The attack the simulation uses for an id: the player's or the enemies', as the settings prefer</summary>
    public AttackDefinition? FindAttack(int id)
    {
        var row = ProjectileParams.FindAttackRow(Project, id, out _);

        return row != null ? AttackDefinition.FromRow(row) : null;
    }

    private void UpdatePlayback(float dt)
    {
        if (!Playing || Result == null)
            return;

        Time += dt * CFG.Current.ProjectileEditor_Speed;

        if (Time < Result.Duration)
            return;

        Time = CFG.Current.ProjectileEditor_Loop ? 0 : Result.Duration;
        Playing = CFG.Current.ProjectileEditor_Loop;
    }

    /// <summary>Play, or pause; a finished simulation starts over</summary>
    public void TogglePlay()
    {
        if (Result == null)
            return;

        if (!Playing && Time >= Result.Duration)
            Time = 0;

        Playing = !Playing;
    }

    public void Stop()
    {
        Playing = false;
        Time = 0;
    }
}
