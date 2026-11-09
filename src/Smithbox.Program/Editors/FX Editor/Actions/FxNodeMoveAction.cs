using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Moves a node within its parent's children, or to another parent (the Effect Tree's Move Up/Down and drag and
/// drop). The node object is kept, so its visibility and note go with it.
/// </summary>
public class FxNodeMoveAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrNode Node;
    private readonly FxrNode OldParent, NewParent;
    private readonly int OldIndex, NewIndex;

    /// <param name="newIndex">The node's index in the new parent's children once moved.</param>
    public FxNodeMoveAction(FxBank bank, FxEntry entry, FxrNode node, FxrNode oldParent, FxrNode newParent, int newIndex)
    {
        Bank = bank;
        Entry = entry;
        Node = node;
        OldParent = oldParent;
        NewParent = newParent;
        OldIndex = oldParent.Children.IndexOf(node);
        NewIndex = newIndex;
    }

    public override ActionEvent Execute()
    {
        Move(OldParent, NewParent, NewIndex);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Move(NewParent, OldParent, OldIndex);
        return ActionEvent.NoEvent;
    }

    private void Move(FxrNode from, FxrNode to, int index)
    {
        from.Children.Remove(Node);
        to.Children.Insert(Math.Clamp(index, 0, to.Children.Count), Node);
        Bank.MarkDirty(Entry);
    }
}
