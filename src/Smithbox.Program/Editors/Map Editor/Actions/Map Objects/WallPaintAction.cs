using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// How a triangle of a WALL file was marked before and after it was painted. Null if the file didn't have it.
/// </summary>
public record WallTriangleChange(MsbEntity Entity, string MapId, string ModelName, uint ShapeKey,
    WALL.Triangle Before, WALL.Triangle After);

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
            Bank.SetTriangle(change.MapId, change.ModelName, change.ShapeKey, change.After);
        }

        Finish();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Changes.Count - 1; i >= 0; i--)
        {
            var change = Changes[i];
            Bank.SetTriangle(change.MapId, change.ModelName, change.ShapeKey, change.Before);
        }

        Finish();

        return ActionEvent.NoEvent;
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
