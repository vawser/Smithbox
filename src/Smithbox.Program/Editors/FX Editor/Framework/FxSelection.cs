using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>What the FX Editor has selected: an effect, an item in its tree, and a property of the action.</summary>
public class FxSelection
{
    public FxEditorView Editor;

    /// <summary>The selected effect, loaded.</summary>
    public FxEntry Entry;

    /// <summary>The selected tree item: an FxrState, FxrNode, FxrConfig or FxrAction.</summary>
    public object Item;

    /// <summary>The selected action's property shown in the Curve Editor, by schema name; null for none.</summary>
    public string Property;

    /// <summary>Set when a selection is made from outside the list, so the list scrolls to it.</summary>
    public bool FocusEntry;

    public FxSelection(FxEditorView editor)
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
        // Clicking an id's row again keeps the version picked in the Effect Tree
        if (entry == Entry || entry != null && Entry?.Primary == entry)
            return;

        // A value mid-edit belongs to the effect it was typed for: committed before the Inspector stops showing it
        Editor.FlushEdits();

        if (entry != null && !Editor.Data.Bank.Load(entry))
            return;

        Entry = entry;
        Item = null;
        Property = null;
    }

    /// <summary>Switches to another version of the selected id (the Effect Tree's version picker).</summary>
    public void SelectVersion(FxEntry version)
    {
        if (version == null || version == Entry || version.Main != Entry?.Main)
            return;
        Editor.FlushEdits();
        if (!Editor.Data.Bank.Load(version))
            return;
        Entry = version;
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

    /// <summary>The entry, or one of its versions (DES), is the selected one.</summary>
    public bool IsSelected(FxEntry entry) => entry == Entry || entry != null && Entry?.Primary == entry;

    public bool IsSelected(object item) => item != null && ReferenceEquals(item, Item);

    /// <summary>After a reload the old objects are gone, so the tree selection is cleared.</summary>
    public void ClearItem()
    {
        Item = null;
        Property = null;
    }
}
