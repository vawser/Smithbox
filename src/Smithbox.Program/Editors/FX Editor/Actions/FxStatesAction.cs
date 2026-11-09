using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Any change to an effect's states: adding, duplicating, deleting or moving a state, or editing its conditions.
/// States are referenced by index from conditions and from every node's StateConfigMap, so the states, their
/// conditions and every map in the effect are stored before and after; undo and redo restore them exactly. State
/// objects keep their identity, so a selected state stays selected.
/// </summary>
public class FxStatesAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly Action<FXR> Apply;
    private readonly Snapshot Old;
    private Snapshot New;

    private record StateState(FxrState State, List<FxrStateCondition> Conditions);
    private record MapState(List<int> List, List<int> Values);
    private record Snapshot(List<StateState> States, List<MapState> Maps);

    public FxStatesAction(FxBank bank, FxEntry entry, Action<FXR> apply)
    {
        Bank = bank;
        Entry = entry;
        Apply = apply;
        Old = Take(entry.Fxr);
    }

    private static Snapshot Take(FXR fxr)
    {
        var states = fxr.States.Select(s => new StateState(s, FxClone.State(s).Conditions)).ToList();
        var maps = FxStateOps.Maps(fxr).Select(map => new MapState(map, map.ToList())).ToList();
        return new Snapshot(states, maps);
    }

    private void Restore(Snapshot s)
    {
        var fxr = Entry.Fxr;
        fxr.States.Clear();
        foreach (var st in s.States)
        {
            // Fresh copies, so the stored snapshot is never edited through the file
            st.State.Conditions = FxClone.State(new FxrState { Conditions = st.Conditions }).Conditions;
            fxr.States.Add(st.State);
        }
        foreach (var m in s.Maps)
        {
            m.List.Clear();
            m.List.AddRange(m.Values);
        }
    }

    public override ActionEvent Execute()
    {
        if (New == null)
        {
            Apply(Entry.Fxr);
            New = Take(Entry.Fxr);
        }
        else
        {
            Restore(New);
        }
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Restore(Old);
        Bank.MarkDirty(Entry);
        return ActionEvent.NoEvent;
    }
}
