using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Replaces an action's contents with another's (the Effect Tree's Paste Replace) or changes one Section10 list
/// (a node's StateConfigMap). The target object stays in place; its contents before and after are stored.
/// </summary>
public class FxReplaceActionAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrAction Target;
    private readonly FxrAction Old;
    private readonly FxrAction New;

    /// <param name="replacement">The contents to put in; copied, so the same source can be pasted again.</param>
    public FxReplaceActionAction(FxBank bank, FxEntry entry, FxrAction target, FxrAction replacement)
    {
        Bank = bank;
        Entry = entry;
        Target = target;
        Old = FxClone.Action(target);
        New = FxClone.Action(replacement);
    }

    /// <summary>Sets the node's StateConfigMap entry for a state to a config index (padding the map if short).</summary>
    public static FxReplaceActionAction SetStateConfig(FxBank bank, FxEntry entry, FxrAction stateMap, int state, int config)
    {
        var copy = FxClone.Action(stateMap);
        if (copy.Section10s.Count == 0)
            copy.Section10s.Add(new List<int>());
        var list = copy.Section10s[0];
        int pad = list.Count > 0 ? list[0] : 0;
        while (list.Count <= state)
            list.Add(pad);
        list[state] = config;
        return new FxReplaceActionAction(bank, entry, stateMap, copy);
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

    private void Set(FxrAction from)
    {
        // Copied again, so the stored states are never edited through the target
        var c = FxClone.Action(from);
        Target.Type = c.Type;
        Target.Unk02 = c.Unk02;
        Target.Unk03 = c.Unk03;
        Target.Unk04 = c.Unk04;
        Target.Fields1 = c.Fields1;
        Target.Fields2 = c.Fields2;
        Target.Properties1 = c.Properties1;
        Target.Properties2 = c.Properties2;
        Target.Section10s = c.Section10s;
        Bank.MarkDirty(Entry);
    }
}
