#nullable enable
using Andre.Formats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The params the Projectile Editor reads, all from the project's primary param bank: bullets, the spells that fire
/// them, and the attacks they deal.
/// </summary>
public static class ProjectileParams
{
    public const string PlayerAttackParam = "AtkParam_Pc";
    public const string EnemyAttackParam = "AtkParam_Npc";

    public static IReadOnlyDictionary<string, Param>? GetParams(ProjectEntry project) =>
        project.Handler?.ParamData?.PrimaryBank?.Params;

    public static Param? GetParam(ProjectEntry project, string paramName)
    {
        var paramList = GetParams(project);

        return paramList != null && paramList.TryGetValue(paramName, out var param) ? param : null;
    }

    public static bool IsBulletParam(ProjectEntry project, string paramName) =>
        GetParam(project, paramName)?.ParamType is "BULLET_PARAM_ST" or "BULLET_PARAM";

    public static bool IsSpellParam(ProjectEntry project, string paramName) =>
        GetParam(project, paramName)?.ParamType is "MAGIC_PARAM_ST" or "SPELL_PARAM";

    /// <summary>A param the editor can show: a bullet param, or a spell param</summary>
    public static bool IsProjectileParam(ProjectEntry project, string paramName) =>
        IsBulletParam(project, paramName) || IsSpellParam(project, paramName);

    /// <summary>The params the editor can show, bullets first</summary>
    public static List<string> GetProjectileParams(ProjectEntry project)
    {
        var paramList = GetParams(project);

        if (paramList == null)
            return new List<string>();

        return paramList.Keys
            .Where(name => IsProjectileParam(project, name))
            .OrderBy(name => IsSpellParam(project, name))
            .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// The param that a spell's bullets are in, as games can have several: the rest are for enemies and the like.
    /// </summary>
    public static Param? GetSpellBulletParam(ProjectEntry project) =>
        GetParam(project, "Bullet") ?? GetParam(project, "BulletParam");

    /// <summary>
    /// The attack row an id refers to, from the player's or the enemies' attacks as the settings prefer, falling back on
    /// the other
    /// </summary>
    public static Param.Row? FindAttackRow(ProjectEntry project, int id, out string? paramName)
    {
        var preferEnemy = CFG.Current.ProjectileEditor_UseEnemyAttacks;
        var first = preferEnemy ? EnemyAttackParam : PlayerAttackParam;
        var second = preferEnemy ? PlayerAttackParam : EnemyAttackParam;

        if (GetParam(project, first)?[id] is { } row)
        {
            paramName = first;
            return row;
        }

        if (GetParam(project, second)?[id] is { } fallback)
        {
            paramName = second;
            return fallback;
        }

        paramName = null;
        return null;
    }

    /// <summary>The attack a bullet row deals, or -1 for none</summary>
    public static int GetAttackId(Param.Row row) => Int(row, -1, "atkId_Bullet", "atkBullet_Id");

    /// <summary>The effect on a bullet while it flies, or -1 for none</summary>
    public static int GetFlightEffect(Param.Row row) => Int(row, -1, "sfxId_Bullet", "bulletSfxId");

    /// <summary>The effect a bullet leaves where it hits, or -1 for none</summary>
    public static int GetHitEffect(Param.Row row) => Int(row, -1, "sfxId_Hit", "explosionSfxId");

    /// <summary>The bullets a bullet row creates: when it ends (chained) and as it flies (at intervals)</summary>
    public static IEnumerable<(string Field, int Id)> GetChildBullets(Param.Row row)
    {
        foreach (var field in new[] { "HitBulletID", "hitBulletId", "intervalCreateBulletId", "childeBulletId", "childeBulletId02", "childeBulletId03" })
        {
            var id = Int(row, 0, field);

            if (id > 0)
                yield return (field, id);
        }
    }

    public static int Int(Param.Row row, int fallback, params string[] names)
    {
        foreach (var name in names)
        {
            if (row[name] is { } cell)
            {
                try
                {
                    return (int)Convert.ToInt64(cell.Value);
                }
                catch
                {
                    return fallback;
                }
            }
        }

        return fallback;
    }

    public static string RowLabel(int id, string? name) => string.IsNullOrEmpty(name) ? $"{id}" : $"{id} {name}";
}
