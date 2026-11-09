using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Changes a property of an action (one SetCurve/SetProperty call, or a modifier edit). Setting a property past the
/// end of a short list pads it with default properties, so the action's property lists and the state of every
/// property in them are stored before and after, modifiers included (the list, and each modifier's type, fields
/// and factor properties); undo restores them exactly.
/// </summary>
public class FxEditPropertyAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrAction Target;
    private readonly Action<FxrAction> Apply;
    private Snapshot Old, New;

    private record PropState(FxrProperty Prop, FxrValueType ValueType, FxrPropertyFunction Function, bool Loop, int TypeEnumB,
        List<uint> Fields, List<FxrModifier> Modifiers, List<ModState> ModStates);

    private record ModState(FxrModifier Mod, ushort TypeEnumA, int TypeEnumB, List<uint> Fields, List<FxrProperty> Properties, List<PropState> PropStates);

    private record Snapshot(List<FxrProperty> List1, List<FxrProperty> List2, List<PropState> States);

    private static PropState TakeProp(FxrProperty p) => new(p, p.ValueType, p.Function, p.Loop, p.TypeEnumB, p.Fields.ToList(),
        p.Modifiers.ToList(),
        p.Modifiers.Select(m => new ModState(m, m.TypeEnumA, m.TypeEnumB, m.Fields.ToList(), m.Properties.ToList(),
            m.Properties.Select(TakeProp).ToList())).ToList());

    private static void RestoreProp(PropState st)
    {
        var p = st.Prop;
        p.ValueType = st.ValueType;
        p.Function = st.Function;
        p.Loop = st.Loop;
        p.TypeEnumB = st.TypeEnumB;
        p.Fields.Clear();
        p.Fields.AddRange(st.Fields);
        p.Modifiers.Clear();
        p.Modifiers.AddRange(st.Modifiers);
        foreach (var ms in st.ModStates)
        {
            ms.Mod.TypeEnumA = ms.TypeEnumA;
            ms.Mod.TypeEnumB = ms.TypeEnumB;
            ms.Mod.Fields.Clear();
            ms.Mod.Fields.AddRange(ms.Fields);
            ms.Mod.Properties.Clear();
            ms.Mod.Properties.AddRange(ms.Properties);
            foreach (var ps in ms.PropStates)
                RestoreProp(ps);
        }
    }

    public FxEditPropertyAction(FxBank bank, FxEntry entry, FxrAction target, Action<FxrAction> apply)
    {
        Bank = bank;
        Entry = entry;
        Target = target;
        Apply = apply;
        Old = Take(target);
    }

    private static Snapshot Take(FxrAction a) => new(
        a.Properties1.ToList(),
        a.Properties2.ToList(),
        a.Properties1.Concat(a.Properties2).Select(TakeProp).ToList());

    private void Restore(Snapshot s)
    {
        Target.Properties1.Clear();
        Target.Properties1.AddRange(s.List1);
        Target.Properties2.Clear();
        Target.Properties2.AddRange(s.List2);
        foreach (var st in s.States)
            RestoreProp(st);
    }

    public override ActionEvent Execute()
    {
        if (New == null)
        {
            Apply(Target);
            New = Take(Target);
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
