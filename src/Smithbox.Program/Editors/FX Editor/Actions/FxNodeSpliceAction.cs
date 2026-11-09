using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Replaces one child of a node with other nodes, in its place (Inline Proxy: a Proxy node becomes the nodes of the
/// effect it starts). Both the removed node and the added ones keep their identity across undo and redo.
/// </summary>
public class FxNodeSpliceAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrNode Parent;
    private readonly FxrNode Removed;
    private readonly List<FxrNode> Added;
    private readonly int Index;

    public FxNodeSpliceAction(FxBank bank, FxEntry entry, FxrNode parent, FxrNode removed, List<FxrNode> added)
    {
        Bank = bank;
        Entry = entry;
        Parent = parent;
        Removed = removed;
        Added = added;
        Index = parent.Children.IndexOf(removed);
    }

    public override ActionEvent Execute()
    {
        Parent.Children.Remove(Removed);
        Parent.Children.InsertRange(Math.Clamp(Index, 0, Parent.Children.Count), Added);
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var n in Added)
            Parent.Children.Remove(n);
        Parent.Children.Insert(Math.Clamp(Index, 0, Parent.Children.Count), Removed);
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }
}
