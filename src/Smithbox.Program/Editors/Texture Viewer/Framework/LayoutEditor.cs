using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The shoebox layout editor of a Texture Editor view (plan, T5): finds the layout atlas that describes the selected
/// sheet, keeps the cell selection, and does the edits (move, resize, nudge, add, duplicate, delete, grid, draw) as
/// undoable actions. The overlay (LayoutEditor.Overlay.cs) draws and edits cells on the viewport; the Layout window
/// (LayoutEditor.Window.cs) has the fields, lists and checks.
/// </summary>
public partial class LayoutEditor
{
    private readonly TexEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>The layout container that describes the selected sheet (or is paired with its TPF), its layout and atlas.</summary>
    public ShoeboxLayoutContainer Container { get; private set; }
    public ShoeboxLayout Layout { get; private set; }
    public TextureAtlas Atlas { get; private set; }

    /// <summary>The key of the layout in Container.Layouts.</summary>
    public string LayoutKey { get; private set; }

    public int SheetWidth { get; private set; }
    public int SheetHeight { get; private set; }

    public HashSet<SubTexture> Selected { get; } = new();

    /// <summary>The cell under the mouse in the viewport (smallest first), or null.</summary>
    public SubTexture Hovered { get; set; }

    /// <summary>The mouse position over the sheet in sheet pixels, when it is over the image.</summary>
    public System.Numerics.Vector2? MouseSheetPosition { get; set; }

    /// <summary>A cell the viewport should scroll to (centre), set from the cell list and the checks.</summary>
    public SubTexture ScrollTo;

    private TPF.Texture ResolvedFor;
    private Dictionary<ShoeboxLayoutContainer, string> PairedTpfPaths = new();

    public LayoutEditor(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>The layout can be edited: an atlas from a file the lossless reader could read.</summary>
    public bool IsEditable => Atlas != null && Layout != null && !Layout.IsReadOnly && Atlas.Layout == Layout;

    #region Resolve
    /// <summary>Finds the atlas for the selected texture; called every frame (cheap until the selection changes).</summary>
    public void Resolve()
    {
        var sel = View.Selection;
        var tex = sel.SelectedTexture;

        // Still the same atlas: nothing to do
        if (tex == ResolvedFor && Atlas != null && Container != null
            && Container.Layouts.TryGetValue(LayoutKey ?? "", out var current) && current == Layout)
            return;

        if (tex != ResolvedFor)
        {
            ResolvedFor = tex;
            Selected.Clear();
            Hovered = null;

            // The sheet size, once per texture (console textures are headerized for it)
            if (tex != null && sel.SelectedTpf != null)
            {
                var info = TextureFormats.GetInfo(sel.SelectedTpf, tex);
                SheetWidth = info.Width;
                SheetHeight = info.Height;
            }
        }

        Container = null; Layout = null; Atlas = null; LayoutKey = null;

        if (tex == null || sel.SelectedFileEntry == null || sel.SelectedTpf == null)
            return;

        var containers = Project.Handler.TextureData?.PrimaryBank?.ShoeboxEntries?.Values.Where(c => c != null).ToList();
        if (containers == null)
            return;

        // The layout binder paired with this TPF (same folder and name), then Smithbox's bundled layouts
        var paired = containers.FirstOrDefault(c => !c.IsBundled
            && string.Equals(PairedPath(c), sel.SelectedFileEntry.Path, StringComparison.OrdinalIgnoreCase));

        foreach (var candidate in new[] { paired }.Concat(containers.Where(c => c.IsBundled)))
        {
            if (candidate == null)
                continue;

            foreach (var (key, layout) in candidate.Layouts)
            {
                foreach (var atlas in layout.TextureAtlases)
                {
                    var stem = System.IO.Path.GetFileNameWithoutExtension(atlas.ImagePath.Replace('\\', '/'));
                    if (stem.Equals(tex.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Container = candidate; Layout = layout; Atlas = atlas; LayoutKey = key;
                        return;
                    }
                }
            }
        }

        // No layout yet: the paired binder is where Create Layout adds one
        Container = paired;
    }

    /// <summary>Forget the resolved atlas, e.g. after a layout was created (the next Resolve finds it).</summary>
    public void Invalidate()
    {
        Atlas = null;
    }

    private string PairedPath(ShoeboxLayoutContainer container)
    {
        if (!PairedTpfPaths.TryGetValue(container, out var path))
        {
            path = container.FindPairedTpf()?.Path ?? "";
            PairedTpfPaths[container] = path;
        }
        return path;
    }
    #endregion

    #region Selection
    public void Select(IEnumerable<SubTexture> cells)
    {
        Selected.Clear();
        foreach (var c in cells)
            Selected.Add(c);
    }

    public void Deselect(SubTexture cell) => Selected.Remove(cell);

    /// <summary>The first selected cell in atlas order (the one the fields edit).</summary>
    public SubTexture Primary => Atlas?.SubTextures.FirstOrDefault(Selected.Contains);

    /// <summary>All selected cells are one alias group (same rectangle), so they can be resized together.</summary>
    public bool SelectionIsOneRect => Selected.Count > 0 && Selected.Select(CellRect.Of).Distinct().Count() == 1;
    #endregion

    #region Edits
    private List<(SubTexture Cell, CellState Before)> Pending;

    /// <summary>Starts a live edit (drag): the cells' states before it.</summary>
    public void BeginChange(IEnumerable<SubTexture> cells)
    {
        Pending = cells.Distinct().Select(c => (c, CellState.Capture(c))).ToList();
    }

    /// <summary>Ends a live edit: one undoable action from the states at BeginChange to now.</summary>
    public void EndChange()
    {
        if (Pending == null)
            return;

        var action = new LayoutCellChangeAction(Pending.Select(p => (p.Cell, p.Before, CellState.Capture(p.Cell))).ToList());
        Pending = null;

        if (!action.IsEmpty)
            View.ActionManager.ExecuteAction(action);
    }

    public void CancelChange()
    {
        if (Pending == null)
            return;

        foreach (var (cell, before) in Pending)
            before.Apply(cell);
        Pending = null;
    }

    public bool IsChanging => Pending != null;

    /// <summary>One undoable change of the given cells, made by 'change'.</summary>
    public void Change(IEnumerable<SubTexture> cells, Action<SubTexture> change)
    {
        if (!IsEditable)
            return;

        var list = cells.Distinct().ToList();
        BeginChange(list);
        foreach (var c in list)
            change(c);
        EndChange();
    }

    public void Nudge(int dx, int dy)
    {
        Change(Selected, c =>
        {
            c.X = $"{c.XValue + dx}";
            c.Y = $"{c.YValue + dy}";
        });
    }

    public void DeleteSelected()
    {
        if (!IsEditable || Selected.Count == 0)
            return;

        // Highest index first, so each undo puts its cell back where it was
        var actions = Selected
            .Select(c => (Cell: c, Index: Atlas.SubTextures.IndexOf(c)))
            .Where(p => p.Index >= 0)
            .OrderByDescending(p => p.Index)
            .Select(p => (EditorAction)new LayoutCellInsertRemoveAction(this, Atlas, p.Cell, p.Index, false))
            .ToList();

        View.ActionManager.ExecuteAction(actions.Count == 1 ? actions[0] : new CompoundAction(actions));
    }

    /// <summary>
    /// Adds a cell like the template (the selected cell, else the last one) in the next free slot of its grid,
    /// with the next name. Returns false when there is no free slot.
    /// </summary>
    public bool AddNextSlot()
    {
        if (!IsEditable)
            return false;

        var template = Primary ?? Atlas.SubTextures.LastOrDefault();
        var slot = LayoutGeometry.NextSlot(Atlas, template, SheetWidth, SheetHeight);
        if (slot == null)
            return false;

        AddCell(template, LayoutGeometry.NextName(Atlas, template), slot.Value);
        return true;
    }

    /// <summary>Adds a cell with the given rectangle, formatted like the template, after it.</summary>
    public SubTexture AddCell(SubTexture template, string name, CellRect rect)
    {
        if (!IsEditable)
            return null;

        template ??= Atlas.SubTextures.LastOrDefault();
        var cell = Atlas.CreateLike(template, name, rect.X, rect.Y, rect.W, rect.H);
        var index = template != null ? Atlas.SubTextures.IndexOf(template) + 1 : Atlas.SubTextures.Count;

        View.ActionManager.ExecuteAction(new LayoutCellInsertRemoveAction(this, Atlas, cell, index, true));
        return cell;
    }

    /// <summary>The grid generator: rows × columns of cells, named prefix + number (+ suffix), as one action.</summary>
    public int Generate(GridOptions o)
    {
        if (!IsEditable)
            return 0;

        var template = Primary ?? Atlas.SubTextures.LastOrDefault();
        var taken = new HashSet<string>(Atlas.SubTextures.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        var existing = Atlas.SubTextures.Select(CellRect.Of).ToList();
        var actions = new List<EditorAction>();
        var index = Atlas.SubTextures.Count;
        long id = o.StartId;

        for (int row = 0; row < o.Rows; row++)
        {
            for (int col = 0; col < o.Columns; col++)
            {
                var rect = new CellRect(o.OriginX + col * (o.CellWidth + o.Spacing), o.OriginY + row * (o.CellHeight + o.Spacing), o.CellWidth, o.CellHeight);

                if (o.SkipOccupied && existing.Any(e => e.Overlaps(rect)))
                    continue;

                string name;
                do { name = $"{o.Prefix}{id.ToString().PadLeft(Math.Max(1, o.Digits), '0')}{o.Suffix}"; id++; }
                while (taken.Contains(name));
                taken.Add(name);

                var cell = Atlas.CreateLike(template, name, rect.X, rect.Y, rect.W, rect.H);
                actions.Add(new LayoutCellInsertRemoveAction(this, Atlas, cell, index++, true));
            }
        }

        if (actions.Count > 0)
            View.ActionManager.ExecuteAction(actions.Count == 1 ? actions[0] : new CompoundAction(actions));

        return actions.Count;
    }

    /// <summary>A new layout (one atlas, no cells) for the selected sheet in the paired container, as one action.</summary>
    public bool CreateLayout()
    {
        if (Container == null || Atlas != null || ResolvedFor == null)
            return false;

        var (key, layout) = Container.PrepareNewLayout(ResolvedFor.Name, SheetWidth, SheetHeight);
        if (layout == null)
            return false;

        View.ActionManager.ExecuteAction(new LayoutCreateAction(Container, layout, key));
        Invalidate();
        return true;
    }
    #endregion
}

/// <summary>The grid generator's inputs.</summary>
public class GridOptions
{
    public int OriginX, OriginY;
    public int Columns = 4, Rows = 1;
    public int CellWidth = 64, CellHeight = 64;
    public int Spacing;
    public string Prefix = "MENU_ItemIcon_";
    public int Digits = 5;
    public string Suffix = ".png";
    public long StartId;
    public bool SkipOccupied = true;
}
