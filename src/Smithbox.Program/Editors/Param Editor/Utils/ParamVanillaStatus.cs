#nullable enable annotations
using Andre.Formats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Vanilla status of param rows and fields for editors built on params (FaceGen, Projectile), using the same test
/// as the Param Editor's row list: a row is Added when the vanilla param has no row of its ID, Modified when it is
/// in the primary bank's vanilla diff cache. Plan: Claude/Documentation/Vanilla Modified-Added Parity.md.
/// </summary>
public static class ParamVanillaStatus
{
    // Row IDs per vanilla param; the vanilla bank is never edited, so a param's set is built once
    private static readonly ConditionalWeakTable<Param, HashSet<int>> VanillaIds = new();

    /// <summary>The vanilla param of a name, or null when vanilla has none</summary>
    public static Param? VanillaParam(ProjectEntry project, string? paramName)
    {
        var vanillaParams = project.Handler.ParamData?.VanillaBank?.Params;

        if (paramName == null || vanillaParams == null || !vanillaParams.TryGetValue(paramName, out var vanilla))
            return null;

        return vanilla;
    }

    /// <summary>
    /// Added / Modified / Unchanged for a primary-bank row; None when vanilla has no such param (then nothing can be
    /// called added).
    /// </summary>
    public static VanillaStatus RowStatus(ProjectEntry project, string? paramName, Param.Row row)
    {
        var vanilla = VanillaParam(project, paramName);
        if (vanilla == null || row == null)
            return VanillaStatus.None;

        var ids = VanillaIds.GetValue(vanilla, p => p.Rows.Select(r => r.ID).ToHashSet());
        if (!ids.Contains(row.ID))
            return VanillaStatus.Added;

        var diff = project.Handler.ParamData.PrimaryBank.GetVanillaDiffRows(paramName!);
        return diff.Contains(row) ? VanillaStatus.Modified : VanillaStatus.Unchanged;
    }

    /// <summary>
    /// A field of a primary-bank row against the vanilla row of the same ID (the first, if vanilla repeats it):
    /// Added when vanilla has no such row, Modified when the value differs, else Unchanged.
    /// </summary>
    public static VanillaStatus FieldStatus(ProjectEntry project, string? paramName, Param.Row row, string field)
    {
        var vanilla = VanillaParam(project, paramName);
        if (vanilla == null || row == null)
            return VanillaStatus.None;

        var vanillaRow = vanilla[row.ID];
        if (vanillaRow == null)
            return VanillaStatus.Added;

        var a = row[field]?.Value;
        var b = vanillaRow[field]?.Value;

        if (a == null || b == null)
            return VanillaStatus.None;

        return ParamUtils.IsValueDiff(ref a, ref b, a.GetType()) ? VanillaStatus.Modified : VanillaStatus.Unchanged;
    }
}
