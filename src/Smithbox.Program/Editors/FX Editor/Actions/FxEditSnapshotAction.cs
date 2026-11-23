using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor.Model;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// An edit of anything that can be snapshotted through the model (<see cref="IFxSnapshot"/>), e.g. a DLSE exit: the
/// state is captured before and after the change, so undo and redo restore it exactly. <see cref="FxEditValueAction"/>
/// is the same for an action's values.
/// </summary>
public class FxEditSnapshotAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly IFxSnapshot Snap;
    private readonly Action Apply;
    private readonly object Old;
    private object New;

    public FxEditSnapshotAction(FxBank bank, FxEntry entry, IFxSnapshot target, Action apply)
    {
        Bank = bank;
        Entry = entry;
        Snap = target;
        Apply = apply;
        Old = Snap.Snapshot();
    }

    public override ActionEvent Execute()
    {
        if (New == null)
        {
            Apply();
            New = Snap.Snapshot();
        }
        else
        {
            Snap.Restore(New);
        }
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Snap.Restore(Old);
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }
}
