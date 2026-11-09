using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Sets one named field of an action through the schema view. The action's two field lists are stored
/// before and after, so undo restores the exact words, including any list padding the edit added.
/// </summary>
public class FxEditFieldAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrAction Target;
    private readonly List<uint> OldFields1, OldFields2;
    private List<uint> NewFields1, NewFields2;
    private readonly Action<FxrAction> Apply;

    /// <param name="apply">Makes the change, e.g. <c>a => view.SetValue(name, values)</c>.</param>
    public FxEditFieldAction(FxBank bank, FxEntry entry, FxrAction target, Action<FxrAction> apply)
    {
        Bank = bank;
        Entry = entry;
        Target = target;
        Apply = apply;
        OldFields1 = target.Fields1.ToList();
        OldFields2 = target.Fields2.ToList();
    }

    public override ActionEvent Execute()
    {
        if (NewFields1 == null)
        {
            Apply(Target);
            NewFields1 = Target.Fields1.ToList();
            NewFields2 = Target.Fields2.ToList();
        }
        else
        {
            Set(NewFields1, NewFields2);
        }
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Set(OldFields1, OldFields2);
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    private void Set(List<uint> f1, List<uint> f2)
    {
        Target.Fields1.Clear();
        Target.Fields1.AddRange(f1);
        Target.Fields2.Clear();
        Target.Fields2.AddRange(f2);
    }
}
