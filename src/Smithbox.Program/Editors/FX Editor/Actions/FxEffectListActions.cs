using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Adds effects to the bank (Duplicate to New ID, Import) or deletes them, and the reverse on undo. A deleted effect's
/// files are removed on the next save; one restored after that save is written back (FX Effect List Actions Plan §2).
/// </summary>
public class FxEntryPresenceAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly List<FxEntry> Entries;
    private readonly bool Add;
    private readonly Action Changed;

    public FxEntryPresenceAction(FxBank bank, List<FxEntry> entries, bool add, Action changed = null)
    {
        Bank = bank;
        Entries = entries;
        Add = add;
        Changed = changed;
    }

    public override ActionEvent Execute()
    {
        Apply(Add);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(!Add);
        return ActionEvent.NoEvent;
    }

    private void Apply(bool add)
    {
        foreach (var e in Entries)
        {
            if (!add)
                Bank.Delete(e);
            else if (Add)
                Bank.AddNew(e);
            // Undoing a delete puts back an effect that was in the game's files, not a new one
            else
                Bank.Restore(e);
        }
        Changed?.Invoke();
    }
}

/// <summary>Sets which binders hold an effect. The files follow on save (added, or removed with the ids closed up).</summary>
public class FxMembershipAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly List<FxBinderRef> Old;
    private readonly List<FxBinderRef> New;
    private readonly Action Changed;

    public FxMembershipAction(FxBank bank, FxEntry entry, List<FxBinderRef> binders, Action changed = null)
    {
        Bank = bank;
        Entry = entry;
        Old = entry.Binders.ToList();
        New = binders.OrderBy(b => b.Binder.Path, StringComparer.OrdinalIgnoreCase).ToList();
        Changed = changed;
    }

    public override ActionEvent Execute()
    {
        Set(New);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Set(Old);
        return ActionEvent.NoEvent;
    }

    private void Set(List<FxBinderRef> binders)
    {
        Entry.Binders = binders.ToList();
        // The effect is written to the binders it's in, which brings a newly added one its file
        Bank.MarkDirty(Entry);
        Bank.Touch();
        Changed?.Invoke();
    }
}

/// <summary>
/// Swaps an effect's whole content for another FXR (Revert to Vanilla, Import over an effect). Undo puts the old
/// object back, so earlier undo steps, which point into it, still apply.
/// </summary>
public class FxReplaceEffectAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FXR Old;
    private readonly FXR New;
    private readonly Action Changed;

    public FxReplaceEffectAction(FxBank bank, FxEntry entry, FXR replacement, Action changed = null)
    {
        Bank = bank;
        Entry = entry;
        Old = entry.Fxr;
        New = replacement;
        Changed = changed;
    }

    public override ActionEvent Execute()
    {
        Entry.Fxr = New;
        Bank.MarkDirty(Entry);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Fxr = Old;
        Bank.MarkDirty(Entry);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }
}
