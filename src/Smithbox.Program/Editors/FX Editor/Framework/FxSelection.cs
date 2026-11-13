using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>What the FX Editor has selected: an effect, an item in its tree, and a property of the action.</summary>
public class FxSelection
{
    public FxEditorScreen Editor;

    /// <summary>The selected effect, loaded.</summary>
    public FxEntry Entry;

    /// <summary>The selected tree item: an FxrState, FxrNode, FxrConfig or FxrAction.</summary>
    public object Item;

    /// <summary>The selected action's property shown in the Curve Editor, by schema name; null for none.</summary>
    public string Property;

    /// <summary>Set when a selection is made from outside the list, so the list scrolls to it.</summary>
    public bool FocusEntry;

    public FxSelection(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public FxrAction Action => Item as FxrAction;

    /// <summary>
    /// The selected item through the format-neutral model (Model/): an IFxNode, IFxConfig, IFxAction or IFxState
    /// adapter, or null. <see cref="Item"/> stays the file's own object, so reference checks and format-specific
    /// operations keep working.
    /// </summary>
    public object ModelItem => Editor.Data.Adapt(Entry, Item);

    /// <summary>The selected action through the model, or null.</summary>
    public Model.IFxAction ModelAction => ModelItem as Model.IFxAction;

    public void SelectEntry(FxEntry entry)
    {
        if (entry == Entry)
            return;

        // A value mid-edit belongs to the effect it was typed for: committed before the Inspector stops showing it
        Editor.FlushEdits();

        if (entry != null && !Editor.Data.Bank.Load(entry))
            return;

        Entry = entry;
        Item = null;
        Property = null;
    }

    public void SelectItem(object item)
    {
        if (!ReferenceEquals(item, Item))
        {
            Editor.FlushEdits();
            Property = null;
        }
        Item = item;
    }

    public void SelectProperty(string name) => Property = name;

    public bool IsSelected(FxEntry entry) => entry == Entry;

    public bool IsSelected(object item) => item != null && ReferenceEquals(item, Item);

    /// <summary>After a reload the old objects are gone, so the tree selection is cleared.</summary>
    public void ClearItem()
    {
        Item = null;
        Property = null;
    }
}
