using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Sets one of a config's action slots (FX Editor Plan §23): its action takes the given contents (an example action of
/// the chosen type, or an empty one to clear the slot). The action object stays in place, so the tree's selection holds;
/// a config shorter than the slot is padded with empty actions first, and undo takes the padding off again.
/// </summary>
public class FxSetSlotAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrConfig Config;
    private readonly int Slot;
    private readonly FxrAction Contents;
    private FxReplaceActionAction Replace;

    public FxSetSlotAction(FxBank bank, FxEntry entry, FxrConfig config, int slot, FxrAction contents)
    {
        Bank = bank;
        Entry = entry;
        Config = config;
        Slot = slot;
        Contents = FxClone.Action(contents);
    }

    /// <summary>The slot's action object, once the action has run.</summary>
    public FxrAction Target => Slot < Config.Actions.Count ? Config.Actions[Slot] : null;

    public override ActionEvent Execute()
    {
        if (Padding == null)
        {
            // First run: pad as needed, keeping the padding so a redo puts back the same objects
            int before = Config.Actions.Count;
            FxSlotOps.PadTo(Config, Slot);
            Padding = Config.Actions.Skip(before).ToList();
        }
        else
        {
            Config.Actions.AddRange(Padding);
        }
        Replace ??= new FxReplaceActionAction(Bank, Entry, Config.Actions[Slot], Contents);
        Replace.Execute();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Replace?.Undo();
        if (Padding is { Count: > 0 })
            Config.Actions.RemoveRange(Config.Actions.Count - Padding.Count, Padding.Count);
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    private List<FxrAction> Padding;
}
