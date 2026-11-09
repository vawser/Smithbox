using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Deep copies of FXR objects, by their public data only. The reader's bookkeeping (shared field blocks, reserved
/// words, offsets) isn't copied: the writer lays out a copy as new data, as it does any edited object.
/// </summary>
public static class FxClone
{
    public static FxrNode Node(FxrNode n) => new()
    {
        Type = n.Type,
        Configs = n.Configs.Select(Config).ToList(),
        Actions = n.Actions.Select(Action).ToList(),
        Children = n.Children.Select(Node).ToList(),
    };

    public static FxrState State(FxrState s) => new()
    {
        Conditions = s.Conditions.Select(c => new FxrStateCondition
        {
            Operator = c.Operator,
            Unk1 = c.Unk1,
            TargetState = c.TargetState,
            Left = Operand(c.Left),
            Right = Operand(c.Right),
        }).ToList(),
    };

    public static FxrOperand Operand(FxrOperand o) => new() { Type = o.Type, HasValue = o.HasValue, RawValue = o.RawValue };

    public static FxrConfig Config(FxrConfig c) => new()
    {
        Type = c.Type,
        Actions = c.Actions.Select(Action).ToList(),
    };

    public static FxrAction Action(FxrAction a) => new()
    {
        Type = a.Type,
        Unk02 = a.Unk02,
        Unk03 = a.Unk03,
        Unk04 = a.Unk04,
        Fields1 = a.Fields1.ToList(),
        Fields2 = a.Fields2.ToList(),
        Properties1 = a.Properties1.Select(Property).ToList(),
        Properties2 = a.Properties2.Select(Property).ToList(),
        Section10s = a.Section10s.Select(s => s.ToList()).ToList(),
    };

    public static FxrProperty Property(FxrProperty p) => new()
    {
        ValueType = p.ValueType,
        Function = p.Function,
        Loop = p.Loop,
        TypeEnumB = p.TypeEnumB,
        Fields = p.Fields.ToList(),
        Modifiers = p.Modifiers.Select(Modifier).ToList(),
    };

    public static FxrModifier Modifier(FxrModifier m) => new()
    {
        TypeEnumA = m.TypeEnumA,
        TypeEnumB = m.TypeEnumB,
        Fields = m.Fields.ToList(),
        Properties = m.Properties.Select(Property).ToList(),
    };
}
