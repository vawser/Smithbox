using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor.Model;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Changes values of an action through the format-neutral model (a field, a curve, a list). The action's whole stored
/// state is captured before and after through <see cref="IFxSnapshot"/>, so undo and redo restore it exactly,
/// including list padding and curve modifiers. Replaces FxEditFieldAction/FxEditPropertyAction for value edits;
/// those stay for FXR3-only edits (modifiers, proxy targets).
/// </summary>
public class FxEditValueAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly IFxAction Target;
    private readonly IFxSnapshot Snap;
    private readonly Action<IFxAction> Apply;
    private readonly object Old;
    private object New;

    /// <param name="apply">Makes the change, e.g. <c>a => FxEditValueAction.Value(a, name).Set(values)</c>.</param>
    public FxEditValueAction(FxBank bank, FxEntry entry, IFxAction target, Action<IFxAction> apply)
    {
        Bank = bank;
        Entry = entry;
        Target = target;
        Snap = target as IFxSnapshot ?? throw new ArgumentException($"{target.GetType().Name} can't be snapshotted.");
        Apply = apply;
        Old = Snap.Snapshot();
    }

    /// <summary>The action's value with this name, read fresh (its layout can depend on the action's state).</summary>
    public static IFxValue Value(IFxAction action, string name) => action.Values.FirstOrDefault(v => v.Name == name)
        ?? throw new KeyNotFoundException($"'{name}' is not a value of action {action.TypeId}.");

    public override ActionEvent Execute()
    {
        if (New == null)
        {
            Apply(Target);
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
