using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using static SoulsFormats.FFXDLSE;

namespace StudioCore.Editors.FxEditor.Model;

// DLSE adapters for the FX model (DES, DS2, DS2S; Claude/Documentation/FFX Older Games Plan.md §14). They wrap
// FFXDLSE's own objects and read names through FfxSchema / FfxActionView; nothing is copied, so edits through an
// adapter edit the file object, and an unchanged value set back leaves the file byte-identical.
//
// How the DLSE structure maps onto the model:
//   effect               → IFxEffect (wrapper effects: IFxVariants, one IFxEffect per variant)
//   effect root          → IFxNode Root, whose actions are the two effect param lists (IFxEffectParams)
//   state                → IFxState with its actions; its triggers → IFxTransition, evaluators → IFxExpression
//   action / action call → IFxAction (Calls: the action and effect calls among its params)
//   effect call          → IFxAction with IFxProxy (the called effect), its params as values
//   param                → IFxValue (constants, sequences as curves, int arrays, argument refs, calls, wrappers)

/// <summary>Builds the model view of an FFXDLSE file.</summary>
public static class FfxModel
{
    public static FfxEffectAdapter Adapt(FFXDLSE ffx, FfxSchema schema, FfxGame game) =>
        new(ffx, ffx.Effect, schema, game, new Dictionary<object, object>(ReferenceEqualityComparer.Instance));

    /// <summary>Type id of the effect param list actions: -1 for ParamList1, -2 for ParamList2.</summary>
    public static int EffectParamsType(int index) => -index;

    /// <summary>How a sequence param interpolates, from its DS2-scheme value type id (§10.3 of the plan).</summary>
    public static (FxrPropertyFunction Function, bool Loop) SequenceKind(int schemeType) => schemeType switch
    {
        3 or 9 or 17 => (FxrPropertyFunction.Stepped, false),
        4 or 10 or 18 => (FxrPropertyFunction.Stepped, true),
        5 or 11 or 19 => (FxrPropertyFunction.Linear, false),
        6 or 12 or 20 => (FxrPropertyFunction.Linear, true),
        13 or 21 => (FxrPropertyFunction.Hermite, false),
        14 or 22 => (FxrPropertyFunction.Hermite, true),
        _ => (FxrPropertyFunction.Linear, false),
    };
}

public sealed class FfxEffectAdapter : IFxEffect, IFxVariants
{
    public FFXDLSE File { get; }
    public FXEffect Effect { get; }
    public FfxSchema Schema { get; }
    public FfxGame Game { get; }

    // Shared by every variant of a wrapper effect, so any object of the file adapts to the same instance
    private readonly Dictionary<object, object> adapters;
    private readonly List<IFxEffect> variants;

    internal FfxEffectAdapter(FFXDLSE file, FXEffect effect, FfxSchema schema, FfxGame game, Dictionary<object, object> cache)
    {
        File = file;
        Effect = effect;
        Schema = schema;
        Game = game;
        adapters = cache;
        variants = effect.Variants?.Select(v => (IFxEffect)new FfxEffectAdapter(file, v.Effect, schema, game, cache)).ToList()
            ?? new List<IFxEffect>();
    }

    /// <summary>The effect whose states and lists are shown: the first variant of a wrapper, else this one.</summary>
    private FfxEffectAdapter Shown => variants.Count > 0 ? (FfxEffectAdapter)variants[0] : this;

    public FxFormat Format => FxFormat.Dlse;
    public int Id => Effect.ID;

    /// <summary>The file for the top-level effect (what the entry holds), the FXEffect for a variant.</summary>
    public object Source => ReferenceEquals(Effect, File.Effect) ? File : Effect;

    public IReadOnlyList<IFxEffect> Variants => variants;

    public IFxNode Root => Shown.RootNode();

    public IReadOnlyList<IFxState> States => Shown.Effect.StateMap.States.Select((s, i) => (IFxState)Shown.State(s, i)).ToList();

    public byte[] Write() => File.Write();

    public object Adapt(object source)
    {
        if (source == null)
            return null;
        if (adapters.TryGetValue(source, out var a))
            return a;

        // Not drawn yet: found by searching the effect (and its variants)
        foreach (var fx in variants.Count > 0 ? variants.Cast<FfxEffectAdapter>() : new[] { this })
        {
            var e = fx.Effect;
            switch (source)
            {
                case FXEffect x when ReferenceEquals(x, e): return fx.RootNode();
                case ParamList l when ReferenceEquals(l, e.ParamList1): return fx.EffectParams(1);
                case ParamList l when ReferenceEquals(l, e.ParamList2): return fx.EffectParams(2);
                case State s when e.StateMap.States.IndexOf(s) is int i and >= 0: return fx.State(s, i);
            }
        }
        return source switch
        {
            FFXDLSE.Action action => Action(action),
            Param38 call => ActionCall(call),
            Param37 call => EffectCall(call),
            Trigger t => Transition(t),
            _ => null,
        };
    }

    // ---------------------------------------------------------------- adapter cache

    private T Get<T>(object key, Func<T> make) where T : class
    {
        if (adapters.TryGetValue(key, out var a) && a is T t)
            return t;
        t = make();
        adapters[key] = t;
        return t;
    }

    internal FfxRootAdapter RootNode() => Get(Effect, () => new FfxRootAdapter(this));

    internal FfxStateAdapter State(State s, int index)
    {
        var st = Get(s, () => new FfxStateAdapter(this, s));
        st.Index = index;
        return st;
    }

    internal FfxTransitionAdapter Transition(Trigger t) => Get(t, () => new FfxTransitionAdapter(t));

    internal FfxActionAdapter EffectParams(int index)
    {
        var list = index == 1 ? Effect.ParamList1 : Effect.ParamList2;
        return Get(list, () => new FfxEffectParamsAdapter(this, index, list, FfxActionView.ForEffect(Schema, Game, Effect, index)));
    }

    internal FfxActionAdapter Action(FFXDLSE.Action a) =>
        Get(a, () => new FfxActionAdapter(this, a.ID, a, () => FfxActionView.ForAction(Schema, Game, a)));

    internal FfxActionAdapter ActionCall(Param38 call) =>
        Get(call, () => new FfxActionAdapter(this, call.ActionID, call, () => FfxActionView.ForCall(Schema, Game, call)));

    internal FfxEffectCallAdapter EffectCall(Param37 call) => Get(call, () => new FfxEffectCallAdapter(this, call));

    internal FfxValueAdapter Value(FfxActionAdapter owner, Param p, int index) =>
        Get(p, () => new FfxValueAdapter(this, p)).At(owner, index);
}

/// <summary>The effect's root: no node tree in DLSE, so it only holds the two effect param lists.</summary>
public sealed class FfxRootAdapter : IFxNode
{
    private readonly FfxEffectAdapter owner;

    internal FfxRootAdapter(FfxEffectAdapter owner) { this.owner = owner; }

    public FxNodeKind Kind => FxNodeKind.Root;
    public int TypeId => 0;
    public IReadOnlyList<IFxNode> Children => Array.Empty<IFxNode>();
    public IReadOnlyList<IFxAction> Actions => new IFxAction[] { owner.EffectParams(1), owner.EffectParams(2) };
    public object Source => owner.Effect;
}

public sealed class FfxStateAdapter : IFxState
{
    private readonly FfxEffectAdapter owner;
    private readonly State state;

    internal FfxStateAdapter(FfxEffectAdapter owner, State state)
    {
        this.owner = owner;
        this.state = state;
    }

    public int Index { get; internal set; }
    public object Source => state;
    public IReadOnlyList<IFxTransition> Transitions => state.Triggers.Select(t => (IFxTransition)owner.Transition(t)).ToList();
    public IReadOnlyList<IFxAction> Actions => state.Actions.Select(a => (IFxAction)owner.Action(a)).ToList();
}

public sealed class FfxTransitionAdapter : IFxTransition
{
    private readonly Trigger trigger;

    internal FfxTransitionAdapter(Trigger trigger) { this.trigger = trigger; }

    public int TargetState => trigger.StateIndex;
    public IFxExpression Condition => FfxExpression.Of(trigger.Evaluator);
    public object Source => trigger;
}

/// <summary>An evaluator tree as an expression. Rebuilt on each read (evaluators are small and only drawn as text).</summary>
public sealed class FfxExpression : IFxExpression
{
    public FxExpressionKind Kind { get; private init; }
    public string Operator { get; private init; }
    public double? Value { get; private init; }
    public int Index { get; private init; }
    public IReadOnlyList<IFxExpression> Operands { get; private init; } = Array.Empty<IFxExpression>();

    public static IFxExpression Of(Evaluatable e)
    {
        static IFxExpression[] Two(EvaluatableBinary b) => new[] { Of(b.Left), Of(b.Right) };
        static FfxExpression Compare(EvaluatableBinary b, string op) => new() { Kind = FxExpressionKind.Compare, Operator = op, Operands = Two(b) };

        return e switch
        {
            null => new FfxExpression { Kind = FxExpressionKind.Unknown, Index = -1 },
            EvaluatableAnd b => new FfxExpression { Kind = FxExpressionKind.And, Operands = Two(b) },
            EvaluatableOr b => new FfxExpression { Kind = FxExpressionKind.Or, Operands = Two(b) },
            EvaluatableNot u => new FfxExpression { Kind = FxExpressionKind.Not, Operands = new[] { Of(u.Operand) } },
            EvaluatableEQ b => Compare(b, "=="),
            EvaluatableNE b => Compare(b, "!="),
            EvaluatableLT b => Compare(b, "<"),
            EvaluatableLE b => Compare(b, "<="),
            EvaluatableGT b => Compare(b, ">"),
            EvaluatableGE b => Compare(b, ">="),
            EvaluatableConstant c => new FfxExpression { Kind = FxExpressionKind.Constant, Value = c.Value },
            Evaluatable2 a => new FfxExpression { Kind = FxExpressionKind.ArgumentA, Index = a.ArgIndex, Value = a.Unk00 },
            Evaluatable3 a => new FfxExpression { Kind = FxExpressionKind.ArgumentB, Index = a.ArgIndex, Value = a.Unk00 },
            EvaluatableCurrentTick => new FfxExpression { Kind = FxExpressionKind.CurrentTick },
            EvaluatableTotalTick => new FfxExpression { Kind = FxExpressionKind.TotalTick },
            EvaluatableChildExists => new FfxExpression { Kind = FxExpressionKind.ChildExists },
            EvaluatableParentExists => new FfxExpression { Kind = FxExpressionKind.ParentExists },
            EvaluatableDistanceFromCamera => new FfxExpression { Kind = FxExpressionKind.DistanceFromCamera },
            EvaluatableEmittersStopped => new FfxExpression { Kind = FxExpressionKind.EmittersStopped },
            _ => new FfxExpression { Kind = FxExpressionKind.Unknown, Index = -1 },
        };
    }
}

/// <summary>A state action or a nested action call: an action id and a param list, read through FfxActionView.</summary>
public class FfxActionAdapter : IFxAction
{
    protected readonly FfxEffectAdapter Owner;
    private readonly Func<FfxActionView> makeView;
    private readonly object source;

    internal FfxActionAdapter(FfxEffectAdapter owner, int typeId, object source, Func<FfxActionView> view)
    {
        Owner = owner;
        TypeId = typeId;
        this.source = source;
        makeView = view;
    }

    /// <summary>A fresh view each time: cheap, and always the list as it is now.</summary>
    public FfxActionView View => makeView();

    public int TypeId { get; }
    public object Source => source;

    public virtual string Name => View.Def?.Name;
    public virtual string Slot => string.IsNullOrEmpty(View.Def?.Category) ? null : View.Def.Category;
    public bool IsAppearance => Slot == "Appearance";
    public virtual string Description => View.Def?.Desc;
    public bool HasLayout => View.Layout != null;
    public bool IsTyped => View.Fits;

    public ParamList List => View.List;

    public IReadOnlyList<IFxValue> Values
    {
        get
        {
            var list = List;
            var result = new List<IFxValue>(list.Params.Count);
            for (int i = 0; i < list.Params.Count; i++)
                result.Add(Owner.Value(this, list.Params[i], i));
            return result;
        }
    }

    /// <summary>The action and effect calls among the params, in param order (looking inside 82/84 wrappers).</summary>
    public IReadOnlyList<IFxAction> Calls
    {
        get
        {
            var result = new List<IFxAction>();
            foreach (var p in List.Params)
                Collect(p, result);
            return result;
        }
    }

    private void Collect(Param p, List<IFxAction> into)
    {
        switch (p)
        {
            case Param38 call: into.Add(Owner.ActionCall(call)); break;
            case Param37 call: into.Add(Owner.EffectCall(call)); break;
            case Param82 w: Collect(w.Param, into); break;
            case Param84 w: Collect(w.Param, into); break;
        }
    }
}

/// <summary>ParamList1 or ParamList2 of an effect.</summary>
public sealed class FfxEffectParamsAdapter : FfxActionAdapter, IFxEffectParams
{
    internal FfxEffectParamsAdapter(FfxEffectAdapter owner, int index, ParamList list, FfxActionView view)
        : base(owner, FfxModel.EffectParamsType(index), list, () => FfxActionView.ForEffect(owner.Schema, owner.Game, owner.Effect, index))
    {
        ListIndex = index;
    }

    public int ListIndex { get; }

    // The schema's "effect1"/"effect2" keys aren't names; the GUI labels these lists
    public override string Name => null;
    public override string Slot => null;
}

/// <summary>An effect call: the called effect id and the params handed to it (no schema layout).</summary>
public sealed class FfxEffectCallAdapter : FfxActionAdapter, IFxProxy
{
    private readonly Param37 call;

    internal FfxEffectCallAdapter(FfxEffectAdapter owner, Param37 call)
        : base(owner, call.EffectID, call, () => new FfxActionView(owner.Schema, owner.Game, int.MinValue, call.ParamList))
    {
        this.call = call;
    }

    public int TargetEffect => call.EffectID;
    public override string Name => null;
    public override string Slot => null;
    public override string Description => null;
}

/// <summary>
/// One param. Its name and position come from the action it was last read through (<see cref="At"/>), since the
/// adapter is cached by the param object and the same param can only sit in one list.
/// </summary>
public sealed class FfxValueAdapter : IFxValue, IFxCurveValue, IFxListValue, IFxCallValue, IFxArgumentValue, IFxWrappedValue, IFxTypedValue
{
    private readonly FfxEffectAdapter owner;
    public Param Param { get; }

    private FfxActionAdapter action;
    private int index;

    internal FfxValueAdapter(FfxEffectAdapter owner, Param param)
    {
        this.owner = owner;
        Param = param;
    }

    internal FfxValueAdapter At(FfxActionAdapter action, int index)
    {
        this.action = action;
        this.index = index;
        return this;
    }

    private int Scheme => owner.Schema.SchemeType(Param.FileType, owner.Game);
    private FfxParamDef Def => action?.View.ParamDef(index);

    public string Name => action?.View.NameOf(index) ?? $"param{index:00}";

    public FxValueKind Kind => Param switch
    {
        Param1 or Param40 or Param41 or Param68 or Param69 => FxValueKind.Int,
        Param7 or Param70 => FxValueKind.Float,
        Param15 or Param79 or Param81 or Param85 or Param83 => FxValueKind.Vector,
        Param2 => FxValueKind.IntList,
        Param5 or Param6 or Param9 or Param11 or Param12 or Param13 or Param17 or Param18 or Param19 or Param20 or Param21 => FxValueKind.Curve,
        _ => FxValueKind.Other,
    };

    public int Components => Param switch
    {
        Param15 => 4,
        Param83 => 8,
        Param79 or Param81 or Param85 => 2,
        Param17 or Param18 or Param19 or Param20 or Param21 => 4,
        _ => 1,
    };

    public bool IsUnknown => Name.StartsWith("unk", StringComparison.Ordinal) || Name.StartsWith("param", StringComparison.Ordinal);
    public bool IsStored => true;
    public string Description => Def?.Desc;
    public string Enum => null;
    public string Resource => Param is Param40 ? "texture" : null;
    public string TextureType => null;
    public IReadOnlyList<FxEnumMember> EnumMembers => Array.Empty<FxEnumMember>();
    public string Argument => null;
    public bool IsConverted => false;
    public bool IsColor => Param is Param15 or Param17 or Param18 or Param19 or Param20 or Param21;

    // ---------------------------------------------------------------- IFxTypedValue

    public int FileType => Param.FileType;
    public string TypeName => owner.Schema.ValueType(Param.FileType, owner.Game)?.Name;

    // ---------------------------------------------------------------- constants

    public double[] Get() => Param switch
    {
        Param1 p => new double[] { p.Int },
        Param40 p => new double[] { p.TextureID },
        Param41 p => new double[] { p.Unk04 },
        Param68 p => new double[] { p.SoundID },
        Param69 p => new double[] { p.Unk04 },
        Param7 p => new double[] { p.Float },
        Param70 p => new double[] { p.Tick },
        Param15 p => Color(p.Color),
        Param79 p => new double[] { p.Int1, p.Int2 },
        Param81 p => new double[] { p.Float1, p.Float2 },
        Param85 p => new double[] { p.Tick1, p.Tick2 },
        Param83 p => Color(p.Color1).Concat(Color(p.Color2)).ToArray(),
        _ => null,
    };

    private static double[] Color(PrimitiveColor c) => new double[] { c.R, c.G, c.B, c.A };

    public void Set(double[] display)
    {
        var current = Get();
        if (current == null || display == null || display.Length != current.Length)
            return;
        bool Changed(int i) => display[i] != current[i];
        if (!Enumerable.Range(0, current.Length).Any(Changed))
            return;

        // Each component given back unchanged keeps its stored value
        int I(int i, int old) => Changed(i) ? (int)Math.Round(display[i]) : old;
        float F(int i, float old) => Changed(i) ? (float)display[i] : old;
        void SetColor(PrimitiveColor c, int at)
        {
            c.R = F(at, c.R); c.G = F(at + 1, c.G); c.B = F(at + 2, c.B); c.A = F(at + 3, c.A);
        }

        switch (Param)
        {
            case Param1 p: p.Int = I(0, p.Int); break;
            case Param40 p: p.TextureID = I(0, p.TextureID); break;
            case Param41 p: p.Unk04 = I(0, p.Unk04); break;
            case Param68 p: p.SoundID = I(0, p.SoundID); break;
            case Param69 p: p.Unk04 = I(0, p.Unk04); break;
            case Param7 p: p.Float = F(0, p.Float); break;
            case Param70 p: p.Tick = F(0, p.Tick); break;
            case Param15 p: SetColor(p.Color, 0); break;
            case Param79 p: p.Int1 = I(0, p.Int1); p.Int2 = I(1, p.Int2); break;
            case Param81 p: p.Float1 = F(0, p.Float1); p.Float2 = F(1, p.Float2); break;
            case Param85 p: p.Tick1 = F(0, p.Tick1); p.Tick2 = F(1, p.Tick2); break;
            case Param83 p: SetColor(p.Color1, 0); SetColor(p.Color2, 4); break;
        }
    }

    // ---------------------------------------------------------------- curves (sequences)

    public float[] DisplayFactors => new[] { 1f, 1f, 1f, 1f };
    public IReadOnlyList<int> ModifierTypes => Array.Empty<int>();

    public FxrCurve GetCurve()
    {
        if (Kind != FxValueKind.Curve)
            return null;
        var (function, loop) = FfxModel.SequenceKind(Scheme);
        bool color = IsColor;
        var curve = new FxrCurve
        {
            Function = function,
            Loop = loop,
            ValueType = color ? FxrValueType.Vector4 : FxrValueType.Scalar,
        };
        static float[] C(PrimitiveColor c) => new[] { c.R, c.G, c.B, c.A };
        switch (Param)
        {
            case Param5 p: foreach (var k in p.TickInts) curve.Keyframes.Add(Key(k.Tick, k.Int)); break;
            case Param6 p: foreach (var k in p.TickInts) curve.Keyframes.Add(Key(k.Tick, k.Int)); break;
            case Param9 p: foreach (var k in p.TickFloats) curve.Keyframes.Add(Key(k.Tick, k.Float)); break;
            case Param11 p: foreach (var k in p.TickFloats) curve.Keyframes.Add(Key(k.Tick, k.Float)); break;
            case Param12 p: foreach (var k in p.TickFloats) curve.Keyframes.Add(Key(k.Tick, k.Float)); break;
            case Param13 p:
                foreach (var k in p.TickFloat3s)
                    curve.Keyframes.Add(new FxrKeyframe { Position = k.Tick, Value = new[] { k.Float1 }, In = new[] { k.Float2 }, Out = new[] { k.Float3 } });
                break;
            case Param17 p: foreach (var k in p.TickColors) curve.Keyframes.Add(new FxrKeyframe { Position = k.Tick, Value = C(k.Color) }); break;
            case Param18 p: foreach (var k in p.TickColors) curve.Keyframes.Add(new FxrKeyframe { Position = k.Tick, Value = C(k.Color) }); break;
            case Param19 p: foreach (var k in p.TickColors) curve.Keyframes.Add(new FxrKeyframe { Position = k.Tick, Value = C(k.Color) }); break;
            case Param20 p: foreach (var k in p.TickColors) curve.Keyframes.Add(new FxrKeyframe { Position = k.Tick, Value = C(k.Color) }); break;
            case Param21 p:
                foreach (var k in p.TickColor3s)
                    curve.Keyframes.Add(new FxrKeyframe { Position = k.Tick, Value = C(k.Color1), In = C(k.Color2), Out = C(k.Color3) });
                break;
        }
        return curve;

        static FxrKeyframe Key(float t, float v) => new() { Position = t, Value = new[] { v } };
    }

    /// <summary>
    /// Writes the curve's keys back into the sequence. Only the keys are stored: the interpolation and loop come from
    /// the param type, so a curve with another function or loop flag is refused. An unchanged curve changes nothing.
    /// </summary>
    public void SetCurve(FxrCurve curve)
    {
        var current = GetCurve();
        if (current == null || curve == null || FxrModel.CurveEquals(current, curve))
            return;
        if (curve.Function != current.Function || curve.Loop != current.Loop || curve.Keyframes == null)
            return;

        var keys = curve.Keyframes;
        float V(FxrKeyframe k, int c) => k.Value != null && c < k.Value.Length ? k.Value[c] : 0f;
        float In(FxrKeyframe k, int c) => k.In != null && c < k.In.Length ? k.In[c] : 0f;
        float Out(FxrKeyframe k, int c) => k.Out != null && c < k.Out.Length ? k.Out[c] : 0f;

        // Keys that exist are updated in place (their stored objects keep their versions); extra keys are added
        void Ints(List<TickInt> list)
        {
            Resize(list, keys.Count, () => new TickInt());
            for (int i = 0; i < keys.Count; i++) { list[i].Tick = keys[i].Position; list[i].Int = (int)Math.Round(V(keys[i], 0)); }
        }
        void Floats(List<TickFloat> list)
        {
            Resize(list, keys.Count, () => new TickFloat());
            for (int i = 0; i < keys.Count; i++) { list[i].Tick = keys[i].Position; list[i].Float = V(keys[i], 0); }
        }
        void Colors(List<TickColor> list)
        {
            Resize(list, keys.Count, () => new TickColor());
            for (int i = 0; i < keys.Count; i++) { list[i].Tick = keys[i].Position; SetColor(list[i].Color, c => V(keys[i], c)); }
        }

        switch (Param)
        {
            case Param5 p: Ints(p.TickInts); break;
            case Param6 p: Ints(p.TickInts); break;
            case Param9 p: Floats(p.TickFloats); break;
            case Param11 p: Floats(p.TickFloats); break;
            case Param12 p: Floats(p.TickFloats); break;
            case Param13 p:
                Resize(p.TickFloat3s, keys.Count, () => new TickFloat3());
                for (int i = 0; i < keys.Count; i++)
                {
                    var k = p.TickFloat3s[i];
                    k.Tick = keys[i].Position; k.Float1 = V(keys[i], 0); k.Float2 = In(keys[i], 0); k.Float3 = Out(keys[i], 0);
                }
                break;
            case Param17 p: Colors(p.TickColors); break;
            case Param18 p: Colors(p.TickColors); break;
            case Param19 p: Colors(p.TickColors); break;
            case Param20 p: Colors(p.TickColors); break;
            case Param21 p:
                Resize(p.TickColor3s, keys.Count, () => new TickColor3());
                for (int i = 0; i < keys.Count; i++)
                {
                    var k = p.TickColor3s[i];
                    int at = i;
                    k.Tick = keys[at].Position;
                    SetColor(k.Color1, c => V(keys[at], c));
                    SetColor(k.Color2, c => In(keys[at], c));
                    SetColor(k.Color3, c => Out(keys[at], c));
                }
                break;
        }

        static void SetColor(PrimitiveColor color, Func<int, float> c)
        {
            color.R = c(0); color.G = c(1); color.B = c(2); color.A = c(3);
        }
    }

    private static void Resize<T>(List<T> list, int count, Func<T> make)
    {
        while (list.Count > count) list.RemoveAt(list.Count - 1);
        while (list.Count < count) list.Add(make());
    }

    // ---------------------------------------------------------------- int arrays

    public List<int> GetList() => Param is Param2 p ? new List<int>(p.Ints) : null;

    public void SetList(List<int> values)
    {
        if (Param is not Param2 p || values == null || p.Ints.SequenceEqual(values))
            return;
        p.Ints = new List<int>(values);
    }

    // ---------------------------------------------------------------- calls, argument refs, wrappers

    public IFxAction Call => Param switch
    {
        Param38 c => owner.ActionCall(c),
        Param37 c => owner.EffectCall(c),
        _ => null,
    };

    public int ArgumentIndex => Param switch
    {
        Param44 p => p.ArgIndex,
        Param45 p => p.ArgIndex,
        Param46 p => p.ArgIndex,
        Param47 p => p.ArgIndex,
        Param59 p => p.ArgIndex,
        Param60 p => p.ArgIndex,
        Param66 p => p.ArgIndex,
        Param71 p => p.ArgIndex,
        Param72 p => p.ArgIndex,
        Param87 p => p.ArgIndex,
        _ => -1,
    };

    public IReadOnlyList<int> Extra => Param switch
    {
        Param44 p => new[] { p.Unk04 },
        Param45 p => new[] { p.Unk04 },
        Param46 p => new[] { p.Unk04 },
        Param47 p => new[] { p.Unk04 },
        Param59 p => new[] { p.Unk04 },
        Param60 p => new[] { p.Unk04 },
        Param66 p => new[] { p.Unk04 },
        Param71 p => new[] { p.Unk04 },
        Param72 p => new[] { p.Unk04, p.Unk0C },
        Param87 p => new[] { p.Unk04 },
        _ => Array.Empty<int>(),
    };

    /// <summary>True when the param is an argument ref (<see cref="ArgumentIndex"/> is meaningful).</summary>
    public bool IsArgument => ArgumentIndex >= 0 || Param is Param44 or Param45 or Param46 or Param47 or Param59 or Param60 or Param66 or Param71 or Param72 or Param87;

    public IFxValue Inner => Param switch
    {
        Param82 w => owner.Value(action, w.Param, index),
        Param84 w => owner.Value(action, w.Param, index),
        _ => null,
    };

    public IReadOnlyList<double> Factor => Param switch
    {
        Param82 w => new double[] { w.Float },
        Param84 w => Color(w.Color),
        _ => Array.Empty<double>(),
    };
}
