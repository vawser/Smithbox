using Microsoft.AspNetCore.Components.Forms;
using Silk.NET.SDL;
using StudioCore.Editor;
using StudioCore.Scene.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors;

/// <summary>
/// Handles selection of entities within the Viewport and SceneTree.
/// </summary>
public class ViewportSelection
{
    /// <summary>
    /// Hashset of current Selection.
    /// </summary>
    private readonly HashSet<ISelectable> _selected = new();

    private HashSet<ISelectable> _storedSelection = new();

    // State for SceneTree auto-scroll, as these are set at the same time as selections or using selections.
    // This is processed by SceneTree and cleared as soon as the goto is complete, or no goto target was found.
    //
    // More advanced functionality could be added to expand TreeNodes to show the entity, but this requires
    // tracking even more state in SceneTree, as well as path-from-root metadata for an entity. This should
    // probably be split out of Selection at that point (IGotoTarget, perhaps).
    public ISelectable GotoTreeTarget { get; set; }

    public void StoreSelection()
    {
        _storedSelection = _selected;
    }

    public void ResetSelection(EditorScreen editor)
    {
        foreach (var entry in _selected)
        {
            AddSelection(editor, entry);
        }
    }

    /// <summary>
    /// Return true if any entity been selected.
    /// </summary>
    public bool IsSelection()
    {
        return _selected.Count > 0;
    }

    /// <summary>
    /// Return true if any entity of the passed Type been selected.
    /// </summary>
    public bool IsFilteredSelection<T>() where T : ISelectable
    {
        return GetFilteredSelection<T>().Count > 0;
    }

    /// <summary>
    /// Return true if any entity of the passed Type been selected, and is the passed bool true.
    /// </summary>
    public bool IsFilteredSelection<T>(Func<T, bool> filter) where T : ISelectable
    {
        return GetFilteredSelection(filter).Count > 0;
    }

    /// <summary>
    /// Return true if a single entity has been selected.
    /// </summary>
    public bool IsSingleSelection()
    {
        return _selected.Count == 1;
    }

    /// <summary>
    /// Return true if multiple entities have been selected.
    /// </summary>
    public bool IsMultiSelection()
    {
        return _selected.Count > 1;
    }

    /// <summary>
    /// Return true if the current Selection of the passed Type and is the selection count only 1.
    /// </summary>
    public bool IsSingleFilteredSelection<T>() where T : ISelectable
    {
        return GetFilteredSelection<T>().Count == 1;
    }

    /// <summary>
    /// Return true if the current Selection of the passed Type, is the passed bool true, and is the selection count only 1.
    /// </summary>
    public bool IsSingleFilteredSelection<T>(Func<T, bool> filt) where T : ISelectable
    {
        return GetFilteredSelection(filt).Count == 1;
    }

    /// <summary>
    /// Return the first Selection if a single entity has been selected.
    /// </summary>
    public ISelectable GetSingleSelection()
    {
        if (IsSingleSelection())
            return _selected.First();

        return null;
    }

    /// <summary>
    /// Return the first Selection if a single entity has been selected of the passed Type.
    /// </summary>
    public T GetSingleFilteredSelection<T>() where T : ISelectable
    {
        HashSet<T> filt = GetFilteredSelection<T>();
        if (filt.Count() == 1)
            return filt.First();

        return default;
    }

    /// <summary>
    /// Return the first Selection if a single entity has been selected of the passed Type, and the passed bool is true.
    /// </summary>
    public T GetSingleFilteredSelection<T>(Func<T, bool> filt) where T : ISelectable
    {
        HashSet<T> f = GetFilteredSelection(filt);
        if (f.Count() == 1)
            return f.First();

        return default;
    }

    /// <summary>
    /// Return the current Selection.
    /// </summary>
    public HashSet<ISelectable> GetSelection()
    {
        return _selected;
    }

    /// <summary>
    /// Return the current Selection where the selected entities are of the passed Type.
    /// </summary>
    public HashSet<T> GetFilteredSelection<T>() where T : ISelectable
    {
        HashSet<T> filteredSelectionSet = new();

        foreach (ISelectable selected in _selected)
            if (selected is T filteredSelection)
                filteredSelectionSet.Add(filteredSelection);

        return filteredSelectionSet;
    }

    /// <summary>
    /// Return the current Selection where the selected entities are of the passed Type and the passed bool is true.
    /// </summary>
    public HashSet<T> GetFilteredSelection<T>(Func<T, bool> filter) where T : ISelectable
    {
        HashSet<T> filteredSelectionSet = new();

        foreach (ISelectable selected in _selected)
            if (selected is T filteredSelection && filter.Invoke(filteredSelection))
                filteredSelectionSet.Add(filteredSelection);

        return filteredSelectionSet;
    }

    /// <summary>
    /// Clear the current Selection.
    /// </summary>
    public void ClearSelection(EditorScreen editor)
    {
        foreach (ISelectable sel in _selected)
            sel.OnDeselected(editor);

        _selected.Clear();
    }

    /// <summary>
    /// Add the passed Selectable to the current Selection.
    /// </summary>
    public void AddSelection(EditorScreen editor, ISelectable selected)
    {
        if (selected != null)
        {
            selected.OnSelected(editor);
            _selected.Add(selected);
        }
    }

    /// <summary>
    /// Add the passed list of Selectables to the current Selection.
    /// </summary>
    public void AddSelection(EditorScreen editor, List<ISelectable> selected)
    {
        foreach (ISelectable sel in selected)
            if (sel != null)
            {
                sel.OnSelected(editor);
                _selected.Add(sel);
            }
    }

    /// <summary>
    /// Remove the passed Selectable from the current selection.
    /// </summary>
    public void RemoveSelection(EditorScreen editor, ISelectable selected)
    {
        if (selected != null)
        {
            selected.OnDeselected(editor);
            _selected.Remove(selected);
        }
    }

    /// <summary>
    /// Return true if the passed Selectable is already selected.
    /// </summary>
    public bool IsSelected(ISelectable selected)
    {
        foreach (ISelectable sel in _selected)
            if (sel == selected)
                return true;

        return false;
    }

    /// <summary>
    /// Return true if the passed Selectable can be used by GotoTreeTarget.
    /// </summary>
    public bool ShouldGoto(ISelectable selected)
    {
        return selected != null && selected.Equals(GotoTreeTarget);
    }

    /// <summary>
    /// Clear the current GotoTreeTarget.
    /// </summary>
    public void ClearGotoTarget()
    {
        GotoTreeTarget = null;
    }
}
