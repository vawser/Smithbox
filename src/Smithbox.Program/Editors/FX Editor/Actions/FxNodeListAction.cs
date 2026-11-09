using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Adds a node to, or removes one from, a parent node's children (the Effect Tree's Duplicate and Delete). The
/// node object itself is kept, so undoing a delete puts back the same node, with its visibility and anything
/// else that refers to it.
/// </summary>
public class FxNodeListAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrNode Parent;
    private readonly FxrNode Node;
    private readonly int Index;
    private readonly bool Insert;

    private FxNodeListAction(FxBank bank, FxEntry entry, FxrNode parent, FxrNode node, int index, bool insert)
    {
        Bank = bank;
        Entry = entry;
        Parent = parent;
        Node = node;
        Index = index;
        Insert = insert;
    }

    /// <summary>Puts <paramref name="node"/> into the parent's children at <paramref name="index"/>.</summary>
    public static FxNodeListAction Add(FxBank bank, FxEntry entry, FxrNode parent, FxrNode node, int index)
        => new(bank, entry, parent, node, Math.Clamp(index, 0, parent.Children.Count), true);

    /// <summary>Takes <paramref name="node"/> out of the parent's children.</summary>
    public static FxNodeListAction Remove(FxBank bank, FxEntry entry, FxrNode parent, FxrNode node)
        => new(bank, entry, parent, node, parent.Children.IndexOf(node), false);

    public override ActionEvent Execute()
    {
        if (Insert) DoInsert(); else DoRemove();
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Insert) DoRemove(); else DoInsert();
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    private void DoInsert()
    {
        if (!Parent.Children.Contains(Node))
            Parent.Children.Insert(Math.Clamp(Index, 0, Parent.Children.Count), Node);
    }

    private void DoRemove() => Parent.Children.Remove(Node);
}
