using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.MsbEditor;

public class Selection
{
    private readonly HashSet<ISelectable> _selected = new();

    // State for SceneTree auto-scroll, as these are set at the same time as selections or using selections.
    // This is processed by SceneTree and cleared as soon as the goto is complete, or no goto target was found.
    //
    // More advanced functionality could be added to expand TreeNodes to show the entity, but this requires
    // tracking even more state in SceneTree, as well as path-from-root metadata for an entity. This should
    // probably be split out of Selection at that point (IGotoTarget, perhaps).
    public ISelectable GotoTreeTarget { get; set; }

    public bool IsSelection()
    {
        return _selected.Count > 0;
    }

    public bool IsFilteredSelection<T>() where T : ISelectable
    {
        return GetFilteredSelection<T>().Count > 0;
    }

    public bool IsFilteredSelection<T>(Func<T, bool> filt) where T : ISelectable
    {
        return GetFilteredSelection(filt).Count > 0;
    }

    public bool IsSingleSelection()
    {
        return _selected.Count == 1;
    }

    public bool IsMultiSelection()
    {
        return _selected.Count > 1;
    }

    public bool IsSingleFilteredSelection<T>() where T : ISelectable
    {
        return GetFilteredSelection<T>().Count == 1;
    }

    public bool IsSingleFilteredSelection<T>(Func<T, bool> filt) where T : ISelectable
    {
        return GetFilteredSelection(filt).Count == 1;
    }

    public ISelectable GetSingleSelection()
    {
        if (IsSingleSelection())
        {
            return _selected.First();
        }

        return null;
    }

    public T GetSingleFilteredSelection<T>() where T : ISelectable
    {
        HashSet<T> filt = GetFilteredSelection<T>();
        if (filt.Count() == 1)
        {
            return filt.First();
        }

        return default;
    }

    public T GetSingleFilteredSelection<T>(Func<T, bool> filt) where T : ISelectable
    {
        HashSet<T> f = GetFilteredSelection(filt);
        if (f.Count() == 1)
        {
            return f.First();
        }

        return default;
    }

    public HashSet<ISelectable> GetSelection()
    {
        return _selected;
    }

    public HashSet<T> GetFilteredSelection<T>() where T : ISelectable
    {
        HashSet<T> filtered = new();
        foreach (ISelectable sel in _selected)
        {
            if (sel is T filsel)
            {
                filtered.Add(filsel);
            }
        }

        return filtered;
    }

    public HashSet<T> GetFilteredSelection<T>(Func<T, bool> filt) where T : ISelectable
    {
        HashSet<T> filtered = new();
        foreach (ISelectable sel in _selected)
        {
            if (sel is T filsel && filt.Invoke(filsel))
            {
                filtered.Add(filsel);
            }
        }

        return filtered;
    }

    public void ClearSelection()
    {
        foreach (ISelectable sel in _selected)
        {
            sel.OnDeselected();
        }

        _selected.Clear();
    }

    public void AddSelection(ISelectable selected)
    {
        if (selected != null)
        {
            selected.OnSelected();
            _selected.Add(selected);
        }
    }

    public void AddSelection(List<ISelectable> selected)
    {
        foreach (ISelectable sel in selected)
        {
            if (sel != null)
            {
                sel.OnSelected();
                _selected.Add(sel);
            }
        }
    }

    public void RemoveSelection(ISelectable selected)
    {
        if (selected != null)
        {
            selected.OnDeselected();
            _selected.Remove(selected);
        }
    }

    public bool IsSelected(ISelectable selected)
    {
        foreach (ISelectable sel in _selected)
        {
            if (sel == selected)
            {
                return true;
            }
        }

        return false;
    }

    public bool ShouldGoto(ISelectable selected)
    {
        return selected != null && selected.Equals(GotoTreeTarget);
    }

    public void ClearGotoTarget()
    {
        GotoTreeTarget = null;
    }
}
