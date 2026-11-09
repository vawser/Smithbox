using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Changes a property of an action (one SetCurve/SetProperty call). Setting a property past the end of a
/// short list pads it with default properties, so the action's property lists and the state of every
/// property in them are stored before and after; undo restores both exactly. Modifiers are left alone.
/// </summary>
public class FxEditPropertyAction : EditorAction
{
    private readonly FxBank Bank;
    private readonly FxEntry Entry;
    private readonly FxrAction Target;
    private readonly Action<FxrAction> Apply;
    private Snapshot Old, New;

    private record PropState(FxrProperty Prop, FxrValueType ValueType, FxrPropertyFunction Function, bool Loop, int TypeEnumB, List<uint> Fields);

    private record Snapshot(List<FxrProperty> List1, List<FxrProperty> List2, List<PropState> States);

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
        a.Properties1.Concat(a.Properties2)
            .Select(p => new PropState(p, p.ValueType, p.Function, p.Loop, p.TypeEnumB, p.Fields.ToList()))
            .ToList());

    private void Restore(Snapshot s)
    {
        Target.Properties1.Clear();
        Target.Properties1.AddRange(s.List1);
        Target.Properties2.Clear();
        Target.Properties2.AddRange(s.List2);
        foreach (var st in s.States)
        {
            st.Prop.ValueType = st.ValueType;
            st.Prop.Function = st.Function;
            st.Prop.Loop = st.Loop;
            st.Prop.TypeEnumB = st.TypeEnumB;
            st.Prop.Fields.Clear();
            st.Prop.Fields.AddRange(st.Fields);
        }
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
