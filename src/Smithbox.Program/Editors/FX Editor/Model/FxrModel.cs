using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FxEditor.Model;

// FXR3 adapters for the FX model (Claude/Documentation/FFX Older Games Plan.md §12). They wrap FXR's own objects and
// FxrActionView; nothing is copied, so edits through an adapter edit the FXR, and edits made directly show up.

/// <summary>Builds the model view of an FXR.</summary>
public static class FxrModel
{
    public static FxrEffectAdapter Adapt(FXR fxr, FxrSchema schema, FxrGame game) => new(fxr, schema, game);

    public const short RootType = 2000, ProxyType = 2001, LodType = 2002, BasicType = 2200, NodeEmitterType = 2202;
    public const short StateMapAction = 199, ProxyAction = 132;

    public static FxNodeKind NodeKind(short type) => type switch
    {
        RootType => FxNodeKind.Root,
        ProxyType => FxNodeKind.Proxy,
        LodType => FxNodeKind.LevelsOfDetail,
        BasicType => FxNodeKind.Basic,
        NodeEmitterType => FxNodeKind.NodeEmitter,
        _ => FxNodeKind.Unknown,
    };

    public static FxConfigKind ConfigKind(short type) => type switch
    {
        1002 => FxConfigKind.LevelsOfDetail,
        1004 => FxConfigKind.Basic,
        1005 => FxConfigKind.NodeEmitter,
        _ => FxConfigKind.Unknown,
    };

    /// <summary>
    /// A config type's slots in a game: the schema's table without the trailing slots none of whose actions exist in
    /// the game. Null for config types without fixed slots. (Same rule as Framework/FxSlotOps.SlotsFor.)
    /// </summary>
    public static List<int[]> SlotsFor(FxrSchema schema, int configType, FxrGame game)
    {
        if (!schema.ConfigSlots.TryGetValue(configType, out var slots))
            return null;
        int count = slots.Count;
        while (count > 0 && !slots[count - 1].Any(t => schema.GetLayout(t, game) != null))
            count--;
        return slots.Take(count).ToList();
    }

    /// <summary>Deep value equality of two curves (function, shape, keys, constants, bounds).</summary>
    public static bool CurveEquals(FxrCurve a, FxrCurve b)
    {
        if (a == null || b == null) return a == b;
        if (a.Function != b.Function || a.ValueType != b.ValueType || a.Loop != b.Loop || a.Duration != b.Duration) return false;
        if (!Same(a.Constant, b.Constant) || !Same(a.Min, b.Min) || !Same(a.Max, b.Max)) return false;
        if (!Keys(a.Keyframes, b.Keyframes)) return false;
        if ((a.ComponentKeyframes == null) != (b.ComponentKeyframes == null)) return false;
        if (a.ComponentKeyframes != null)
        {
            if (a.ComponentKeyframes.Length != b.ComponentKeyframes.Length) return false;
            for (int i = 0; i < a.ComponentKeyframes.Length; i++)
                if (!Keys(a.ComponentKeyframes[i], b.ComponentKeyframes[i])) return false;
        }
        return true;

        static bool Same(float[] x, float[] y) => x == null ? y == null : y != null && x.AsSpan().SequenceEqual(y);
        static bool Keys(List<FxrKeyframe> x, List<FxrKeyframe> y)
        {
            if (x == null || y == null) return x == y;
            if (x.Count != y.Count) return false;
            for (int i = 0; i < x.Count; i++)
                if (x[i].Position != y[i].Position || !Same(x[i].Value, y[i].Value) || !Same(x[i].In, y[i].In) || !Same(x[i].Out, y[i].Out))
                    return false;
            return true;
        }
    }
}

public sealed class FxrEffectAdapter : IFxEffect
{
    public FXR Fxr { get; }
    public FxrSchema Schema { get; }
    public FxrGame Game { get; }

    private readonly Dictionary<object, object> adapters = new(ReferenceEqualityComparer.Instance);

    public FxrEffectAdapter(FXR fxr, FxrSchema schema, FxrGame game)
    {
        Fxr = fxr;
        Schema = schema;
        Game = game;
    }

    public FxFormat Format => FxFormat.Fxr3;
    public int Id => Fxr.Id;
    public object Source => Fxr;
    public IFxNode Root => Node(Fxr.Root);
    public IReadOnlyList<IFxState> States => Fxr.States.Select((s, i) => (IFxState)State(s, i)).ToList();
    public byte[] Write() => Fxr.Write();

    public object Adapt(object source) => source switch
    {
        FxrNode n => Node(n),
        FxrConfig c => Config(c),
        FxrAction a => Action(a),
        FxrState s => Fxr.States.IndexOf(s) is int i and >= 0 ? State(s, i) : null,
        FxrStateCondition c => Transition(c),
        _ => null,
    };

    internal FxrNodeAdapter Node(FxrNode n)
    {
        bool proxy = n.Type == FxrModel.ProxyType;
        if (adapters.TryGetValue(n, out var a) && a is FxrNodeAdapter na && (na is FxrProxyNodeAdapter) == proxy)
            return na;
        var created = proxy ? new FxrProxyNodeAdapter(this, n) : new FxrNodeAdapter(this, n);
        adapters[n] = created;
        return created;
    }

    internal FxrConfigAdapter Config(FxrConfig c) => Get(c, () => new FxrConfigAdapter(this, c));
    internal FxrActionAdapter Action(FxrAction a) => Get(a, () => new FxrActionAdapter(this, a));
    internal FxrTransitionAdapter Transition(FxrStateCondition c) => Get(c, () => new FxrTransitionAdapter(c));

    internal FxrStateAdapter State(FxrState s, int index)
    {
        var st = Get(s, () => new FxrStateAdapter(s));
        st.Index = index;
        st.Owner = this;
        return st;
    }

    private T Get<T>(object key, Func<T> make) where T : class
    {
        if (adapters.TryGetValue(key, out var a) && a is T t)
            return t;
        t = make();
        adapters[key] = t;
        return t;
    }

    internal FxrActionView View(FxrAction a) => new(Schema, a, Game, Fxr);
}

public sealed class FxrStateAdapter : IFxState
{
    private readonly FxrState state;
    internal FxrEffectAdapter Owner;

    internal FxrStateAdapter(FxrState state) { this.state = state; }

    public int Index { get; internal set; }
    public IReadOnlyList<IFxTransition> Transitions => state.Conditions.Select(c => (IFxTransition)Owner.Transition(c)).ToList();
    public IReadOnlyList<IFxAction> Actions => Array.Empty<IFxAction>();
    public object Source => state;
}

public sealed class FxrTransitionAdapter : IFxTransition
{
    private readonly FxrStateCondition condition;

    internal FxrTransitionAdapter(FxrStateCondition condition) { this.condition = condition; }

    public int TargetState => condition.TargetState;
    public object Source => condition;

    public IFxExpression Condition => new FxExpression
    {
        Kind = FxExpressionKind.Compare,
        Operator = condition.Operator switch { 0 => "==", 1 => "!=", 2 => "<", 3 => "<=", _ => "?" },
        Index = condition.Operator,
        Operands = [Operand(condition.Left), Operand(condition.Right)],
    };

    private static FxExpression Operand(FxrOperand o) => o.Type switch
    {
        -4 => new FxExpression { Kind = FxExpressionKind.Constant, Value = o.HasValue ? o.FloatValue : null, Index = -4 },
        -3 => new FxExpression { Kind = FxExpressionKind.External, Index = o.HasValue ? o.IntValue : 0 },
        -1 => new FxExpression { Kind = FxExpressionKind.StateTime, Index = -1 },
        _ => new FxExpression { Kind = FxExpressionKind.Unknown, Index = o.Type },
    };
}

/// <summary>A plain expression node (built on each read; expressions carry no identity).</summary>
public sealed class FxExpression : IFxExpression
{
    public FxExpressionKind Kind { get; init; }
    public string Operator { get; init; }
    public double? Value { get; init; }
    public int Index { get; init; }
    public IReadOnlyList<IFxExpression> Operands { get; init; } = Array.Empty<IFxExpression>();
}

public class FxrNodeAdapter : IFxNode, IFxConfigured
{
    protected readonly FxrEffectAdapter Owner;
    public FxrNode Node { get; }

    internal FxrNodeAdapter(FxrEffectAdapter owner, FxrNode node)
    {
        Owner = owner;
        Node = node;
    }

    public FxNodeKind Kind => FxrModel.NodeKind(Node.Type);
    public int TypeId => Node.Type;
    public object Source => Node;
    public IReadOnlyList<IFxNode> Children => Node.Children.Select(c => (IFxNode)Owner.Node(c)).ToList();
    public IReadOnlyList<IFxAction> Actions => Node.Actions.Select(a => (IFxAction)Owner.Action(a)).ToList();
    public IReadOnlyList<IFxConfig> Configs => Node.Configs.Select(c => (IFxConfig)Owner.Config(c)).ToList();

    public IReadOnlyList<int> StateConfigMap
    {
        get
        {
            foreach (var a in Node.Actions)
                if (a.Type == FxrModel.StateMapAction)
                    return a.Section10s.Count > 0 ? a.Section10s[0] : new List<int>();
            return null;
        }
    }
}

public sealed class FxrProxyNodeAdapter : FxrNodeAdapter, IFxProxy
{
    internal FxrProxyNodeAdapter(FxrEffectAdapter owner, FxrNode node) : base(owner, node) { }

    public int TargetEffect => Node.Actions.FirstOrDefault(a => a.Type == FxrModel.ProxyAction) is { Fields1.Count: > 0 } p
        ? (int)p.Fields1[0] : -1;
}

public sealed class FxrConfigAdapter : IFxConfig
{
    private readonly FxrEffectAdapter owner;
    public FxrConfig Config { get; }

    internal FxrConfigAdapter(FxrEffectAdapter owner, FxrConfig config)
    {
        this.owner = owner;
        Config = config;
    }

    public FxConfigKind Kind => FxrModel.ConfigKind(Config.Type);
    public int TypeId => Config.Type;
    public object Source => Config;
    public IReadOnlyList<IFxAction> Actions => Config.Actions.Select(a => (IFxAction)owner.Action(a)).ToList();
    public IReadOnlyList<int[]> SlotTypes => FxrModel.SlotsFor(owner.Schema, Config.Type, owner.Game);
}

public sealed class FxrActionAdapter : IFxAction
{
    private readonly FxrEffectAdapter owner;
    public FxrAction Action { get; }

    internal FxrActionAdapter(FxrEffectAdapter owner, FxrAction action)
    {
        this.owner = owner;
        Action = action;
    }

    /// <summary>A fresh view over the action (the action's type, and so its layout, can change).</summary>
    public FxrActionView View => owner.View(Action);

    public int TypeId => Action.Type;
    public string Name => View.Definition?.Name;
    public string Slot => View.Definition?.Slot;
    public bool IsAppearance => View.Definition?.IsAppearance ?? false;
    public bool IsTyped => View.IsTyped;
    public IReadOnlyList<IFxAction> Calls => Array.Empty<IFxAction>();
    public object Source => Action;

    public IReadOnlyList<IFxValue> Values
    {
        get
        {
            var view = View;
            if (view.IsTyped)
                return view.Names.Select(n => (IFxValue)new FxrNamedValue(view, n)).ToList();

            var list = new List<IFxValue>();
            for (int i = 0; i < Action.Fields1.Count; i++) list.Add(new FxrRawWord(Action.Fields1, i, $"fields1[{i}]"));
            for (int i = 0; i < Action.Fields2.Count; i++) list.Add(new FxrRawWord(Action.Fields2, i, $"fields2[{i}]"));
            for (int i = 0; i < Action.Properties1.Count; i++) list.Add(new FxrRawProperty(owner, Action.Properties1[i], $"properties1[{i}]"));
            for (int i = 0; i < Action.Properties2.Count; i++) list.Add(new FxrRawProperty(owner, Action.Properties2[i], $"properties2[{i}]"));
            for (int i = 0; i < Action.Section10s.Count; i++) list.Add(new FxrRawList(Action.Section10s, i, $"section10s[{i}]"));
            return list;
        }
    }
}

/// <summary>A schema-named value of a typed action: a field, a property (curve) or a section10 list.</summary>
public sealed class FxrNamedValue : IFxValue, IFxCurveValue, IFxListValue
{
    private readonly FxrActionView view;

    internal FxrNamedValue(FxrActionView view, string name)
    {
        this.view = view;
        Name = name;
    }

    public string Name { get; }

    private FxrPropDef Def => view.Describe(Name);

    public FxValueKind Kind
    {
        get
        {
            switch (view.KindOf(Name))
            {
                case FxrValueKind.Field:
                    view.FindField(Name, out var slot, out _);
                    return slot.Type switch
                    {
                        FxrFieldType.Bool => FxValueKind.Bool,
                        FxrFieldType.Int => FxValueKind.Int,
                        FxrFieldType.Float => FxValueKind.Float,
                        _ => FxValueKind.Vector,
                    };
                case FxrValueKind.Property: return FxValueKind.Curve;
                case FxrValueKind.Section10: return FxValueKind.IntList;
                default: return FxValueKind.Other;
            }
        }
    }

    public int Components => view.KindOf(Name) switch
    {
        FxrValueKind.Field => view.FindField(Name, out var slot, out _) ? slot.Words : 1,
        FxrValueKind.Property => Def?.Components ?? 1,
        _ => 1,
    };

    public bool IsUnknown => Def?.IsUnknown ?? Name.StartsWith("unk", StringComparison.Ordinal);
    public bool IsStored => view.IsStored(Name);
    public string Description => Def?.Description;
    public string Enum => Def?.Enum;
    public string Resource => Def?.Resource;

    public double[] Get() => view.KindOf(Name) == FxrValueKind.Field ? view.GetValue(Name) : null;

    public void Set(double[] display)
    {
        if (view.KindOf(Name) != FxrValueKind.Field)
            throw new InvalidOperationException($"{Name} is not a field.");
        var old = view.GetValue(Name);
        if (old.AsSpan().SequenceEqual(display))
            return;
        var oldWords = view.GetWords(Name);
        view.SetValue(Name, display);
        // components given back unchanged keep their stored words (display conversions may not invert exactly)
        var words = view.GetWords(Name);
        for (int i = 0; i < words.Length && i < display.Length && i < old.Length; i++)
            if (display[i] == old[i])
                words[i] = oldWords[i];
        view.SetWords(Name, words);
    }

    public FxrCurve GetCurve() => view.GetCurve(Name);

    public void SetCurve(FxrCurve curve)
    {
        if (FxrModel.CurveEquals(curve, view.GetCurve(Name)))
            return;
        view.SetCurve(Name, curve);
    }

    public List<int> GetList() => view.GetSection10(Name);

    public void SetList(List<int> values)
    {
        var old = view.GetSection10(Name);
        if (old != null && values != null && old.SequenceEqual(values) || old == null && values == null)
            return;
        view.SetSection10(Name, values);
    }
}

/// <summary>A raw field word of an untyped action, shown as an int.</summary>
public sealed class FxrRawWord : IFxValue
{
    private readonly List<uint> words;
    private readonly int index;

    internal FxrRawWord(List<uint> words, int index, string name)
    {
        this.words = words;
        this.index = index;
        Name = name;
    }

    public string Name { get; }
    public FxValueKind Kind => FxValueKind.Raw;
    public int Components => 1;
    public bool IsUnknown => true;
    public bool IsStored => true;
    public string Description => null;
    public string Enum => null;
    public string Resource => null;
    public double[] Get() => [(int)words[index]];

    public void Set(double[] display)
    {
        uint word = (uint)(int)Math.Round(display[0]);
        if (words[index] != word)
            words[index] = word;
    }
}

/// <summary>A raw property (curve) of an untyped action, in raw units.</summary>
public sealed class FxrRawProperty : IFxValue, IFxCurveValue
{
    private readonly FxrEffectAdapter owner;
    private readonly FxrProperty property;

    internal FxrRawProperty(FxrEffectAdapter owner, FxrProperty property, string name)
    {
        this.owner = owner;
        this.property = property;
        Name = name;
    }

    public string Name { get; }
    public FxValueKind Kind => FxValueKind.Curve;
    public int Components => (int)property.ValueType + 1;
    public bool IsUnknown => true;
    public bool IsStored => true;
    public string Description => null;
    public string Enum => null;
    public string Resource => null;
    public double[] Get() => null;
    public void Set(double[] display) => throw new InvalidOperationException($"{Name} is a curve.");

    public FxrCurve GetCurve()
    {
        var c = FxrCurve.Decode(property);
        c.ComponentBoundsFromLast = owner.Fxr.ComponentBoundsFromLast;
        return c;
    }

    public void SetCurve(FxrCurve curve)
    {
        var old = GetCurve();
        if (FxrModel.CurveEquals(curve, old))
            return;
        curve.ComponentBoundsFromLast = owner.Fxr.ComponentBoundsFromLast;
        curve.EncodeInto(property, !curve.SameShape(old));
        bool packs = owner.Game is FxrGame.DS3 or FxrGame.SDT or FxrGame.ER || owner.Game == FxrGame.AC6 && property.TypeEnumB != 0;
        property.TypeEnumB = packs ? property.PackTypeEnumB() : 0;
    }
}

/// <summary>A raw section10 list of an untyped action.</summary>
public sealed class FxrRawList : IFxValue, IFxListValue
{
    private readonly List<List<int>> lists;
    private readonly int index;

    internal FxrRawList(List<List<int>> lists, int index, string name)
    {
        this.lists = lists;
        this.index = index;
        Name = name;
    }

    public string Name { get; }
    public FxValueKind Kind => FxValueKind.IntList;
    public int Components => 1;
    public bool IsUnknown => true;
    public bool IsStored => true;
    public string Description => null;
    public string Enum => null;
    public string Resource => null;
    public double[] Get() => null;
    public void Set(double[] display) => throw new InvalidOperationException($"{Name} is a list.");
    public List<int> GetList() => lists[index];

    public void SetList(List<int> values)
    {
        if (values != null && lists[index].SequenceEqual(values))
            return;
        lists[index] = values ?? new List<int>();
    }
}
