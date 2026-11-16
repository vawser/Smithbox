#nullable enable
using StudioCore.Application;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The colours and text the Projectile Editor shows a simulation with, shared by its windows and its viewport.
/// </summary>
public static class ProjectileFormat
{
    private const float DefaultCorrection = 100.0f;

    // A bullet's colour says how many bullets down the chain from the one fired it is
    private static readonly Vector4[] GenerationColors =
    {
        new(1.00f, 0.75f, 0.25f, 1.0f),
        new(0.35f, 0.75f, 1.00f, 1.0f),
        new(0.45f, 1.00f, 0.50f, 1.0f),
        new(1.00f, 0.45f, 0.65f, 1.0f),
        new(0.75f, 0.55f, 1.00f, 1.0f),
        new(1.00f, 0.55f, 0.30f, 1.0f)
    };

    public static int GenerationColorCount => GenerationColors.Length;

    public static readonly Vector4 TargetColor = new(1.00f, 0.25f, 0.25f, 1.0f);
    public static readonly Vector4 ReachColor = new(1.00f, 1.00f, 1.00f, 1.0f);
    public static readonly Vector4 DamageColor = new(1.00f, 0.90f, 0.35f, 1.0f);
    public static readonly Vector4 WallColor = new(0.75f, 0.75f, 0.85f, 1.0f);

    public static Vector4 GetColor(int generation) => GenerationColors[generation % GenerationColors.Length];

    /// <summary>
    /// Attack power is shown as it is. Percentages of the attacker's power are only shown when they aren't the usual
    /// 100, unless they are the only thing that the attack has.
    /// </summary>
    public static string FormatDamage(AttackDamage flat, AttackDamage correction, ProjectType projectType)
    {
        List<string> parts = new();

        var darkDamageLoc = projectType is ProjectType.ER or ProjectType.NR
            ? "PROJECTILE_Damage_Holy"
            : "PROJECTILE_Damage_Dark";

        AddDamage(parts, "PROJECTILE_Damage_Physical", flat.Physical, correction.Physical);
        AddDamage(parts, "PROJECTILE_Damage_Magic", flat.Magic, correction.Magic);
        AddDamage(parts, "PROJECTILE_Damage_Fire", flat.Fire, correction.Fire);
        AddDamage(parts, "PROJECTILE_Damage_Lightning", flat.Lightning, correction.Lightning);
        AddDamage(parts, darkDamageLoc, flat.Dark, correction.Dark);
        AddDamage(parts, "PROJECTILE_Damage_Stamina", flat.Stamina, correction.Stamina);

        if (parts.Count > 0)
            return string.Join("  ", parts);

        return correction.Physical > 0 || correction.Magic > 0
            ? LOC.Get("PROJECTILE_Damage_Attacker")
            : LOC.Get("PROJECTILE_Damage_None");
    }

    private static void AddDamage(List<string> parts, string key, float flat, float correction)
    {
        var corrected = correction > 0 && correction != DefaultCorrection;

        if (flat > 0 && corrected)
            parts.Add($"{LOC.Get(key)} {flat:0} ({correction:0}%)");
        else if (flat > 0)
            parts.Add($"{LOC.Get(key)} {flat:0}");
        else if (corrected)
            parts.Add($"{LOC.Get(key)} {correction:0}%");
    }

    public static string GetMotionName(TargetMotion motion) => motion switch
    {
        TargetMotion.Strafe => LOC.Get("PROJECTILE_Motion_Strafe"),
        TargetMotion.Advance => LOC.Get("PROJECTILE_Motion_Advance"),
        TargetMotion.Circle => LOC.Get("PROJECTILE_Motion_Circle"),
        _ => LOC.Get("PROJECTILE_Motion_Still")
    };

    public static string GetOrientationName(CastOrientation orientation) => orientation switch
    {
        CastOrientation.Down => LOC.Get("PROJECTILE_Orientation_Down"),
        CastOrientation.Up => LOC.Get("PROJECTILE_Orientation_Up"),
        _ => LOC.Get("PROJECTILE_Orientation_Forward")
    };

    public static string GetSurfaceName(ImpactSurface surface) => surface switch
    {
        ImpactSurface.Water => LOC.Get("PROJECTILE_Surface_Water"),
        _ => LOC.Get("PROJECTILE_Surface_Map")
    };

    /// <summary>Where a track came from and how it ended, with its attack, for its tooltip</summary>
    public static string DescribeTrack(SimulationResult result, ProjectileTrack track, ProjectType projectType)
    {
        var origin = track.Origin switch
        {
            ProjectileOrigin.Chain => LOC.Get("PROJECTILE_Origin_Chain", result.Tracks[track.Parent].RowId),
            ProjectileOrigin.Interval => LOC.Get("PROJECTILE_Origin_Interval", result.Tracks[track.Parent].RowId),
            _ => LOC.Get("PROJECTILE_Origin_Root")
        };

        var end = track.End switch
        {
            ProjectileEnd.HitWall => LOC.Get("PROJECTILE_End_Hit_Wall", track.EndTime),
            ProjectileEnd.HitGround => LOC.Get("PROJECTILE_End_Hit_Ground", track.EndTime),
            ProjectileEnd.HitTarget => LOC.Get("PROJECTILE_End_Hit_Target", track.EndTime),
            ProjectileEnd.TimeLimit => LOC.Get("PROJECTILE_End_Time_Limit", track.EndTime),
            _ => LOC.Get("PROJECTILE_End_Expired", track.EndTime)
        };

        var description = $"{origin}\n{end}";

        if (track.Attack != null)
        {
            description += $"\n{LOC.Get("PROJECTILE_Attack", track.Attack.RowId, track.Attack.Radius)}" +
                           $"\n{FormatDamage(track.Attack.Flat, track.Attack.Correction, projectType)}";
        }

        if (track.ChainWithheld)
            description += $"\n{LOC.Get("PROJECTILE_Chain_Withheld")}";

        return description;
    }
}
