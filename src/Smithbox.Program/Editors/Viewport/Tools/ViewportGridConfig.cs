using StudioCore.Application;
using System;

namespace StudioCore.Editors.Viewport;

public enum ViewportGridEditor
{
    Map,
    Model,
    TimeAct
}

public enum ViewportGridType
{
    Primary,
    Secondary,
    Tertiary
}

/// <summary>
/// Finds the settings of a viewport grid in <see cref="CFG" />.
/// </summary>
public static class ViewportGridConfig
{
    // The grid shown in each editor's grid tool, see ViewportGridTool
    private static readonly ViewportGridType[] CurrentGrids = new ViewportGridType[3];

    /// <summary>
    /// The grid shown in the editor's grid tool. The gizmo snaps to this grid.
    /// </summary>
    public static ViewportGridType GetCurrentGrid(ViewportGridEditor editor)
    {
        return CurrentGrids[(int)editor];
    }

    public static void SetCurrentGrid(ViewportGridEditor editor, ViewportGridType type)
    {
        CurrentGrids[(int)editor] = type;
    }

    public static ViewportGridSet GetSet(ViewportGridEditor editor)
    {
        var cfg = CFG.Current;

        // A config saved before a grid set existed leaves it null
        return editor switch
        {
            ViewportGridEditor.Map => cfg.MapEditor_Grids ??= new ViewportGridSet(),
            ViewportGridEditor.Model => cfg.ModelEditor_Grids ??= new ViewportGridSet(),
            ViewportGridEditor.TimeAct => cfg.TimeActEditor_Grids ??= new ViewportGridSet(),
            _ => throw new ArgumentOutOfRangeException(nameof(editor))
        };
    }

    public static ViewportGridSettings Get(ViewportGridEditor editor, ViewportGridType type)
    {
        return GetSet(editor).Get(type);
    }
}
