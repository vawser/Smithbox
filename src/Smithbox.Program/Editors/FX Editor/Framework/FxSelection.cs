using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>What the FX Editor has selected: an effect, and an item in its tree.</summary>
public class FxSelection
{
    public FxEditorScreen Editor;

    /// <summary>The selected effect, loaded.</summary>
    public FxEntry Entry;

    /// <summary>The selected tree item: an FxrState, FxrNode, FxrConfig or FxrAction.</summary>
    public object Item;

    /// <summary>Set when a selection is made from outside the list, so the list scrolls to it.</summary>
    public bool FocusEntry;

    public FxSelection(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public FxrAction Action => Item as FxrAction;

    public void SelectEntry(FxEntry entry)
    {
        if (entry == Entry)
            return;

        if (entry != null && !Editor.Data.Bank.Load(entry))
            return;

        Entry = entry;
        Item = null;
    }

    public void SelectItem(object item)
    {
        Item = item;
    }

    public bool IsSelected(FxEntry entry) => entry == Entry;

    public bool IsSelected(object item) => item != null && ReferenceEquals(item, Item);

    /// <summary>After a reload the old objects are gone, so the tree selection is cleared.</summary>
    public void ClearItem() => Item = null;
}
