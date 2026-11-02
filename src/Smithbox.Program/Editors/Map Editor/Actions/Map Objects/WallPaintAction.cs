using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The file of a collision a painted triangle was changed in.
/// </summary>
public enum WallLayer
{
    /// <summary>
    /// The WALL file, which marks the triangle with cells.
    /// </summary>
    Wall,

    /// <summary>
    /// The NOJUMPWALL file, which lists the triangle or not.
    /// </summary>
    NoJump
}

/// <summary>
/// How a triangle was marked before and after it was painted. For the WALL file, the triangle is null if the
/// file didn't have it; for the NOJUMPWALL file, the flags say whether it was listed.
/// </summary>
public record WallTriangleChange(MsbEntity Entity, string MapId, string ModelName, uint ShapeKey, WallLayer Layer,
    WALL.Triangle Before = null, WALL.Triangle After = null, bool NoJumpBefore = false, bool NoJumpAfter = false);

/// <summary>
/// The triangles painted in one stroke of the Wall Painting tool.
/// </summary>
public class WallPaintAction : ViewportAction
{
    private readonly WallBank Bank;
    private readonly List<WallTriangleChange> Changes;

    public WallPaintAction(WallBank bank, List<WallTriangleChange> changes)
    {
        Bank = bank;
        Changes = changes;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var change in Changes)
        {
            Apply(Bank, change, true);
        }

        Finish();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Changes.Count - 1; i >= 0; i--)
        {
            Apply(Bank, Changes[i], false);
        }

        Finish();

        return ActionEvent.NoEvent;
    }

    /// <summary>
    /// Sets a triangle to how it was after, or before, it was painted.
    /// </summary>
    public static void Apply(WallBank bank, WallTriangleChange change, bool after)
    {
        if (change.Layer is WallLayer.NoJump)
        {
            bank.SetNoJump(change.MapId, change.ModelName, change.ShapeKey, after ? change.NoJumpAfter : change.NoJumpBefore);
        }
        else
        {
            bank.SetTriangle(change.MapId, change.ModelName, change.ShapeKey, after ? change.After : change.Before);
        }
    }

    private void Finish()
    {
        foreach (var entity in Changes.Select(e => e.Entity).Distinct())
        {
            (entity.Container as MapContainer)?.MarkDirty(MapFileType.Wall);

            if (Bank.Visible && entity.Parent != null)
            {
                Bank.Rebuild(entity);
            }
        }
    }
}
