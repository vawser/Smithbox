using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// A cell's attributes the layout editor changes, as written in the file (null = absent).
/// </summary>
public sealed class CellState
{
    public static readonly string[] Attributes = ["name", "x", "y", "width", "height", "half", "originalWidth", "originalHeight"];

    public Dictionary<string, string> Values = new();

    public static CellState Capture(SubTexture cell)
    {
        var state = new CellState();
        foreach (var a in Attributes)
            state.Values[a] = cell.Element.Get(a);
        return state;
    }

    public CellState Clone() => new() { Values = new Dictionary<string, string>(Values) };

    public void Apply(SubTexture cell)
    {
        foreach (var (name, value) in Values)
            cell.Set(name, value);
    }

    public bool SameAs(CellState other) => Attributes.All(a => Values.GetValueOrDefault(a) == other.Values.GetValueOrDefault(a));
}

/// <summary>
/// Changes the attributes of one or more cells at once (move, resize, field edits, nudge).
/// </summary>
public class LayoutCellChangeAction : EditorAction
{
    private readonly List<(SubTexture Cell, CellState Before, CellState After)> Changes;

    public LayoutCellChangeAction(List<(SubTexture Cell, CellState Before, CellState After)> changes)
    {
        Changes = changes;
    }

    public bool IsEmpty => Changes.All(c => c.Before.SameAs(c.After));

    public override ActionEvent Execute()
    {
        foreach (var (cell, _, after) in Changes)
            after.Apply(cell);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var (cell, before, _) in Changes)
            before.Apply(cell);
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Adds a cell to an atlas at an index, or removes it (add, duplicate, draw, grid, delete).
/// </summary>
public class LayoutCellInsertRemoveAction : EditorAction
{
    private readonly LayoutEditor Editor;
    private readonly TextureAtlas Atlas;
    private readonly SubTexture Cell;
    private readonly int Index;
    private readonly bool Insert;

    public LayoutCellInsertRemoveAction(LayoutEditor editor, TextureAtlas atlas, SubTexture cell, int index, bool insert)
    {
        Editor = editor;
        Atlas = atlas;
        Cell = cell;
        Index = index;
        Insert = insert;
    }

    public override ActionEvent Execute()
    {
        Apply(Insert);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(!Insert);
        return ActionEvent.NoEvent;
    }

    private void Apply(bool insert)
    {
        if (insert)
        {
            Atlas.Insert(Cell, Index);
            Editor.Select([Cell]);
        }
        else
        {
            Atlas.Remove(Cell);
            Editor.Deselect(Cell);
        }
    }
}

/// <summary>
/// Adds a new layout file (one atlas for a sheet that has none) to a layout container, or takes it out.
/// </summary>
public class LayoutCreateAction : EditorAction
{
    private readonly ShoeboxLayoutContainer Container;
    private readonly ShoeboxLayout Layout;
    private readonly string Key;

    public LayoutCreateAction(ShoeboxLayoutContainer container, ShoeboxLayout layout, string key)
    {
        Container = container;
        Layout = layout;
        Key = key;
    }

    public override ActionEvent Execute()
    {
        Container.AddLayout(Key, Layout);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Container.RemoveLayout(Key);
        return ActionEvent.NoEvent;
    }
}
