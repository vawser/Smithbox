using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Live editing (FX Editor Plan, phase V6): a value being typed, dragged or picked shows in the preview before it's
/// committed. The change is applied to the effect for real, by the same snapshot action a commit makes but outside
/// the undo stack, and taken back before the commit (or when the edit is abandoned), so the undo stack only ever holds
/// the one committed step. One live change exists at a time; showing another takes the first back.
/// </summary>
public class FxLiveEdit
{
    private EditorAction Applied;
    private FxEntry Entry;
    private bool WasDirty;

    /// <summary>Who showed the live change (an Inspector input's id, or the Curve Editor).</summary>
    public object Owner { get; private set; }

    /// <summary>Bumped whenever the live change is shown or taken back, so the preview rebuilds.</summary>
    public int Version { get; private set; }

    public static bool Enabled => CFG.Current.FxEditor_Live_Preview;

    public bool IsShowing => Applied != null;

    /// <summary>
    /// Shows the change of the action <paramref name="make"/> builds (for <paramref name="owner"/>, e.g. an input's id).
    /// The action is built after the previous live change is taken back, so its snapshot holds the real value.
    /// </summary>
    public void Show(object owner, FxEntry entry, Func<EditorAction> make)
    {
        if (!Enabled || entry == null || make == null)
            return;
        Revert();
        var action = make();
        if (action == null)
            return;
        WasDirty = entry.IsDirty;
        action.Execute();
        Applied = action;
        Entry = entry;
        Owner = owner;
        Version++;
    }

    /// <summary>Takes the live change back: any, or only <paramref name="owner"/>'s when given.</summary>
    public void Revert(object owner = null)
    {
        if (Applied == null || owner != null && !Equals(owner, Owner))
            return;
        Applied.Undo();
        // Showing and taking back a value isn't an edit
        Entry.IsDirty = WasDirty;
        Applied = null;
        Entry = null;
        Owner = null;
        Version++;
    }
}
