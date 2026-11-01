using Andre.Formats;
using System;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     An amount for each type of damage.
/// </summary>
public readonly record struct AttackDamage(float Physical, float Magic, float Fire, float Lightning, float Dark, float Stamina)
{
    public static AttackDamage operator +(AttackDamage a, AttackDamage b)
    {
        return new AttackDamage(a.Physical + b.Physical, a.Magic + b.Magic, a.Fire + b.Fire,
            a.Lightning + b.Lightning, a.Dark + b.Dark, a.Stamina + b.Stamina);
    }
}

/// <summary>
///     What a bullet deals, taken from its attack row.
/// </summary>
/// <param name="RowId">The attack row.</param>
/// <param name="Flat">The attack power of the row.</param>
/// <param name="Correction">
///     Percentages of the attack power of whatever fired the bullet, which isn't known here. These are 100 on most
///     rows, including for types of damage that the row doesn't deal, so only other values say anything.
/// </param>
/// <param name="Radius">The reach of the attack, which is 0 if it is left to the bullet's own radius.</param>
public sealed record AttackDefinition(int RowId, AttackDamage Flat, AttackDamage Correction, float Radius)
{
    private const int HitCount = 16;

    public static AttackDefinition FromRow(Param.Row row)
    {
        var radius = 0.0f;

        for (var i = 0; i < HitCount; i++)
        {
            radius = MathF.Max(radius, Read(row, $"hit{i}_Radius"));
        }

        return new AttackDefinition(row.ID, ReadDamage(row, ""), ReadDamage(row, "Correction"), radius);
    }

    private static AttackDamage ReadDamage(Param.Row row, string suffix)
    {
        return new AttackDamage(Read(row, $"atkPhys{suffix}"), Read(row, $"atkMag{suffix}"), Read(row, $"atkFire{suffix}"),
            Read(row, $"atkThun{suffix}"), Read(row, $"atkDark{suffix}"), Read(row, $"atkStam{suffix}"));
    }

    // Dark Souls III capitalizes most of these fields
    private static float Read(Param.Row row, string field)
    {
        Param.Cell? cell = row[field] ?? row[$"{char.ToUpperInvariant(field[0])}{field[1..]}"];

        return cell != null ? Convert.ToSingle(cell.Value.Value) : 0;
    }
}
