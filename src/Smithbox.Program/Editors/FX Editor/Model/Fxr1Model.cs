using SoulsFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static SoulsFormats.FXR1;
using static SoulsFormats.FXR1.FXNode;

namespace StudioCore.Editors.FxEditor.Model;

// FXR1 adapters for the FX model (DS1, DS1R; Claude/Documentation/FFX Older Games Plan.md §18). They wrap the FXR1
// port's own objects; nothing is copied, so edits through an adapter edit the file object, and an unchanged value set
// back leaves the file byte-identical (the port's writer reproduces every vanilla file).
//
// How the FXR1 node tree maps onto the model (decision §8.8: the file's own tree):
//   node 134 (several roots)     → IFxNode, Kind Roots, children = its 133 roots
//   node 133 (effect root)       → IFxNode, Kind Root, IFxStatesNode (its states); actions = its two containers (IFxEffectParams)
//   state / transition           → IFxState / IFxTransition; the condition's nodes (111–140) → IFxExpression
//   state action (FXAction)      → IFxAction: its container's param nodes are the values; Calls = the container's call
//                                  nodes (38 action, 37 effect: IFxProxy), then its ActionData and modifier (IFxPacked)
//   ActionData / modifier        → IFxAction + IFxPacked: the record's fields are the values
//
// Condition node types, matched to the DLSE evaluators by FfxSurvey --fxr1eval over 1,654 DES ↔ DS1 effect pairs:
//   111 constant, 112 current tick, 113 total tick, 114 / 115 argument (Evaluatable2 / 3: type, index),
//   120 >=, 121 >, 122 <=, 123 <, 124 ==, 125 != (not in vanilla), 126 &&, 127 ||, 128 !,
//   129 child exists, 130 parent exists, 131 distance from camera, 132 emitters stopped,
//   and five packed forms: 136 = child exists == 0 (3,886 of DS1's 4,047 exits);
//   137 [u, a, b] = (total tick >= argB[u,a] && argB[u,a] >= 0) || (argA[u,b] && !parent exists);
//   138 [a] = distance < argA[1,a]; 139 [a] = distance > argA[1,a]; 140 [a, b] = distance < argA[1,a] || distance > argA[1,b].

/// <summary>Builds the model view of an FXR1 file.</summary>
public static class Fxr1Model
{
    public static Fxr1EffectAdapter Adapt(FXR1 fx, FfxSchema schema, FfxGame game) => new(fx, schema, game);

    /// <summary>The FXR1 node type id of a node object (the id the file stores), or -1.</summary>
    public static int TypeId(FXNode node) => FXR1.NodeTypeId(node);

    /// <summary>
    /// How a param node sequence interpolates, by node type. 3–22 share DLSE's (DS2-scheme) ids; 27–29 are taken as
    /// 19–21 and 89 / 91 / 95 as 5 / 11 / 11. Provisional until the preview (F5) or an in-game test shows otherwise.
    /// </summary>
    public static (FxrPropertyFunction Function, bool Loop) NodeSequenceKind(int type) => FfxModel.SequenceKind(type switch
    {
        27 => 19,
        28 => 20,
        29 => 21,
        89 => 5,
        91 or 95 => 11,
        // FXR v2: 4 / 10 / 17 by DLSE's numbering; 30 as 22; the ranged 149 / 150 as 13 / 14 and 151 / 152 as 21 / 22
        30 => 22,
        149 => 13,
        150 => 14,
        151 => 21,
        152 => 22,
        _ => type,
    });

    /// <summary>How an ActionData field sequence interpolates, by field type (provisional, as above).</summary>
    public static (FxrPropertyFunction Function, bool Loop) FieldSequenceKind(int type) => type switch
    {
        0 => (FxrPropertyFunction.Stepped, false),
        12 => (FxrPropertyFunction.Stepped, true),
        16 => (FxrPropertyFunction.Linear, true),
        8 or 9 or 20 => (FxrPropertyFunction.Hermite, false),
        _ => (FxrPropertyFunction.Linear, false),
    };

    /// <summary>The effect roots (node 133) of a file: the root itself, or each root of a node 134.</summary>
    public static List<EffectCreateNode133> Roots(FXR1 fx) => fx.RootFXNode switch
    {
        EffectCreateNode133 r => new List<EffectCreateNode133> { r },
        EffectCreateNode134 m => m.Nodes.OfType<EffectCreateNode133>().ToList(),
        _ => new List<EffectCreateNode133>(),
    };
}

public sealed class Fxr1EffectAdapter : IFxEffect
{
    public FXR1 File { get; }
    public FfxSchema Schema { get; }
    public FfxGame Game { get; }

    private readonly Dictionary<object, object> adapters = new(ReferenceEqualityComparer.Instance);

    internal Fxr1EffectAdapter(FXR1 file, FfxSchema schema, FfxGame game)
    {
        File = file;
        Schema = schema;
        Game = game;
    }

    public FxFormat Format => FxFormat.Fxr1;

    public int Id => File.RootFXNode switch
    {
        EffectCreateNode133 r => r.EffectID,
        EffectCreateNode134 m => m.EffectID,
        _ => -1,
    };

    public object Source => File;

    /// <summary>The first root's states (each root's own are on its node, <see cref="IFxStatesNode"/>).</summary>
    public IReadOnlyList<IFxState> States
    {
        get
        {
            var root = Fxr1Model.Roots(File).FirstOrDefault();
            return root == null ? Array.Empty<IFxState>() : RootNode(root).States;
        }
    }

    public IFxNode Root => Node(File.RootFXNode);

    public byte[] Write() => File.Write();

    public object Adapt(object source)
    {
        if (source == null)
            return null;
        if (adapters.TryGetValue(source, out var a))
            return a;

        // Not drawn yet: found by searching the roots
        switch (source)
        {
            case EffectCreateNode133 or EffectCreateNode134 or ContainerNode:
                return Node((FXNode)source);
            case FXState s:
                foreach (var r in Fxr1Model.Roots(File))
                    if (r.States.Contains(s))
                        return State(r, s);
                return null;
            case FXTransition t:
                foreach (var r in Fxr1Model.Roots(File))
                    foreach (var s in r.States)
                        if (s.Transitions.Contains(t))
                            return Transition(r, t);
                return null;
            case FXContainer c:
                foreach (var r in Fxr1Model.Roots(File))
                {
                    if (ReferenceEquals(c, r.Container1)) return RootContainer(r, 1);
                    if (ReferenceEquals(c, r.Container2)) return RootContainer(r, 2);
                }
                return null;
            case FXAction action: return Action(action);
            case ActionCallNode call: return ActionCall(call);
            case EffectCallNode call: return EffectCall(call);
            case FXActionData data: return Packed(data);
            case FXModifier modifier: return Packed(modifier);
            default: return null;
        }
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

    internal IFxNode Node(FXNode node) => node switch
    {
        EffectCreateNode133 r => RootNode(r),
        EffectCreateNode134 m => Get(m, () => new Fxr1RootsAdapter(this, m)),
        null => null,
        ContainerNode c => Get(c, () => new Fxr1ContainerNodeAdapter(this, c)),
        _ => Get(node, () => new Fxr1OtherNodeAdapter(node)),
    };

    internal Fxr1RootAdapter RootNode(EffectCreateNode133 r) => Get(r, () => new Fxr1RootAdapter(this, r));

    internal Fxr1StateAdapter State(EffectCreateNode133 root, FXState s) => Get(s, () => new Fxr1StateAdapter(this, root, s));

    internal Fxr1TransitionAdapter Transition(EffectCreateNode133 root, FXTransition t) => Get(t, () => new Fxr1TransitionAdapter(this, root, t));

    internal Fxr1ActionAdapter RootContainer(EffectCreateNode133 root, int index)
    {
        var c = index == 1 ? root.Container1 : root.Container2;
        Schema.EffectParams.TryGetValue($"effect{index}", out var def);
        return Get(c, () => new Fxr1EffectParamsAdapter(this, index, c, def));
    }

    internal Fxr1ActionAdapter Action(FXAction a) =>
        Get(a, () => new Fxr1ActionAdapter(this, a.ActionType, a.Container, a, Schema.Actions.GetValueOrDefault(a.ActionType)));

    internal Fxr1ActionAdapter ActionCall(ActionCallNode call) =>
        Get(call, () => new Fxr1ActionAdapter(this, call.ActionType, call.Container, call, Schema.Actions.GetValueOrDefault(call.ActionType)));

    /// <summary>A v2 container node's container, as an action: its node type and int, no schema layout.</summary>
    internal Fxr1ActionAdapter ContainerNodeAction(ContainerNode n) =>
        Get(n.Container, () => new Fxr1ActionAdapter(this, n.NodeType, n.Container, n.Container, null));

    internal Fxr1EffectCallAdapter EffectCall(EffectCallNode call) => Get(call, () => new Fxr1EffectCallAdapter(this, call));

    internal Fxr1PackedAdapter Packed(object record) => Get(record, () => new Fxr1PackedAdapter(this, record));

    // Values have their own cache: a v2 container node is both a tree node (Node) and a value of its parent's container
    private readonly Dictionary<object, Fxr1ValueAdapter> values = new(ReferenceEqualityComparer.Instance);

    internal Fxr1ValueAdapter Value(Fxr1ActionAdapter owner, FXNode node, int index)
    {
        if (!values.TryGetValue(node, out var v))
            values[node] = v = new Fxr1ValueAdapter(this, node);
        return v.At(owner, index);
    }
}

/// <summary>Node 134: several effect roots.</summary>
public sealed class Fxr1RootsAdapter : IFxNode
{
    private readonly Fxr1EffectAdapter owner;
    private readonly EffectCreateNode134 node;

    internal Fxr1RootsAdapter(Fxr1EffectAdapter owner, EffectCreateNode134 node)
    {
        this.owner = owner;
        this.node = node;
    }

    public FxNodeKind Kind => FxNodeKind.Roots;
    public int TypeId => 134;
    public IReadOnlyList<IFxNode> Children => node.Nodes.Select(owner.Node).Where(n => n != null).ToList();
    public IReadOnlyList<IFxAction> Actions => Array.Empty<IFxAction>();
    public object Source => node;
}

/// <summary>Node 133: an effect root with its two containers and its own states.</summary>
public sealed class Fxr1RootAdapter : IFxNode, IFxStatesNode
{
    private readonly Fxr1EffectAdapter owner;
    private readonly EffectCreateNode133 node;

    internal Fxr1RootAdapter(Fxr1EffectAdapter owner, EffectCreateNode133 node)
    {
        this.owner = owner;
        this.node = node;
    }

    public FxNodeKind Kind => FxNodeKind.Root;
    public int TypeId => 133;
    /// <summary>FXR v2: the root's node 141 tree; v1 roots have no child nodes.</summary>
    public IReadOnlyList<IFxNode> Children => node.V2Node != null ? new[] { owner.Node(node.V2Node) } : Array.Empty<IFxNode>();

    /// <summary>The root's containers (v2 roots have only the first).</summary>
    public IReadOnlyList<IFxAction> Actions => node.Container2 != null
        ? new IFxAction[] { owner.RootContainer(node, 1), owner.RootContainer(node, 2) }
        : new IFxAction[] { owner.RootContainer(node, 1) };
    public IReadOnlyList<IFxState> States => node.States.Select(s => (IFxState)owner.State(node, s)).ToList();
    public object Source => node;
}

/// <summary>
/// FXR v2 container nodes 141–148 (each root → 141 → 142 → 143 → 146 → 147 …; meaning unknown, FFX plan §19): the
/// container nodes among its container's nodes are its children, and the container itself is its one action (its
/// values are all the container's nodes, the child container nodes shown as references).
/// </summary>
public sealed class Fxr1ContainerNodeAdapter : IFxNode
{
    private readonly Fxr1EffectAdapter owner;
    private readonly ContainerNode node;

    internal Fxr1ContainerNodeAdapter(Fxr1EffectAdapter owner, ContainerNode node)
    {
        this.owner = owner;
        this.node = node;
    }

    public FxNodeKind Kind => FxNodeKind.Container;
    public int TypeId => node.NodeType;
    public IReadOnlyList<IFxNode> Children =>
        node.Container?.FXNodes.OfType<ContainerNode>().Select(c => owner.Node(c)).ToList() ?? (IReadOnlyList<IFxNode>)Array.Empty<IFxNode>();
    public IReadOnlyList<IFxAction> Actions => node.Container != null ? new IFxAction[] { owner.ContainerNodeAction(node) } : Array.Empty<IFxAction>();
    public object Source => node;
}

/// <summary>A node of a root list that isn't a root (none in vanilla): shown as an unknown leaf.</summary>
public sealed class Fxr1OtherNodeAdapter : IFxNode
{
    private readonly FXNode node;

    internal Fxr1OtherNodeAdapter(FXNode node) { this.node = node; }

    public FxNodeKind Kind => FxNodeKind.Unknown;
    public int TypeId => Fxr1Model.TypeId(node);
    public IReadOnlyList<IFxNode> Children => Array.Empty<IFxNode>();
    public IReadOnlyList<IFxAction> Actions => Array.Empty<IFxAction>();
    public object Source => node;
}

public sealed class Fxr1StateAdapter : IFxState
{
    private readonly Fxr1EffectAdapter owner;
    private readonly EffectCreateNode133 root;
    private readonly FXState state;

    internal Fxr1StateAdapter(Fxr1EffectAdapter owner, EffectCreateNode133 root, FXState state)
    {
        this.owner = owner;
        this.root = root;
        this.state = state;
    }

    public int Index => root.States.IndexOf(state);
    public object Source => state;
    public IReadOnlyList<IFxTransition> Transitions => state.Transitions.Select(t => (IFxTransition)owner.Transition(root, t)).ToList();
    public IReadOnlyList<IFxAction> Actions => state.Actions.Select(a => (IFxAction)owner.Action(a)).ToList();
}

/// <summary>A transition: its target is a state of the same root; its condition is a tree of evaluator nodes.</summary>
public sealed class Fxr1TransitionAdapter : IFxTransition, IFxEditableTransition
{
    private readonly EffectCreateNode133 root;
    private readonly FXTransition transition;

    internal Fxr1TransitionAdapter(Fxr1EffectAdapter owner, EffectCreateNode133 root, FXTransition transition)
    {
        this.root = root;
        this.transition = transition;
    }

    public int TargetState => transition.TargetState == null ? -1 : root.States.IndexOf(transition.TargetState);
    public IFxExpression Condition => Fxr1Expression.Of(transition.EvaluatorNode, n => transition.EvaluatorNode = n);
    public object Source => transition;

    public int StateCount => root.States.Count;

    public void SetTarget(int state)
    {
        if (state >= 0 && state < StateCount)
            transition.TargetState = root.States[state];
    }

    // The target, the condition's root and every condition node's fields (children and ints). Operator edits swap
    // nodes for new ones, so restoring puts the original objects back where they were: the file's sharing and bytes
    // come back exactly, and a snapshot can be restored more than once (undo, redo).
    private sealed record ExitState(FXState Target, FXNode Condition, List<Fxr1State.Slot> Nodes);

    public object Snapshot() =>
        new ExitState(transition.TargetState, transition.EvaluatorNode, Fxr1State.Capture(Fxr1State.ConditionNodes(transition.EvaluatorNode)));

    public void Restore(object state)
    {
        if (state is not ExitState s)
            throw new ArgumentException("Not a snapshot of an FXR1 transition.");
        transition.TargetState = s.Target;
        transition.EvaluatorNode = s.Condition;
        Fxr1State.Restore(s.Nodes);
    }
}

/// <summary>
/// A condition node as an expression, rebuilt on each read. Real nodes keep their node and how to replace it, so
/// operators swap the node for another of the same arity (operands kept), and constants and arguments change their
/// ints. The packed forms (136–140) are shown as the expressions they stand for: their structure is fixed, and only
/// their argument indices can be edited.
/// </summary>
public sealed class Fxr1Expression : IFxExpression, IFxEditableExpression
{
    public FxExpressionKind Kind { get; private init; }
    public string Operator { get; private init; }
    public double? Value { get; private init; }
    public int Index { get; private init; }
    public IReadOnlyList<IFxExpression> Operands { get; private init; } = Array.Empty<IFxExpression>();

    private FXNode node;
    private Action<FXNode> replace;
    private Action<int> setValue;
    private Action<int, int> setArgument;

    public static readonly string[] CompareOperators = FfxExpression.CompareOperators;
    public static readonly string[] LogicOperators = FfxExpression.LogicOperators;

    private static readonly Dictionary<int, string> OperatorOf = new()
    {
        [120] = ">=", [121] = ">", [122] = "<=", [123] = "<", [124] = "==", [125] = "!=", [126] = "&&", [127] = "||",
    };

    public static IFxExpression Of(FXNode n, Action<FXNode> replace)
    {
        Fxr1Expression Leaf(FxExpressionKind kind) => new() { Kind = kind, node = n, replace = replace };
        static IFxExpression Fixed(FxExpressionKind kind, string op = null, params IFxExpression[] operands) =>
            new Fxr1FixedExpression(kind, op, null, operands);
        static IFxExpression Const(int v) => new Fxr1FixedExpression(FxExpressionKind.Constant, null, v);
        static Fxr1Expression Arg(FxExpressionKind kind, int index, int unk, Action<int, int> set) =>
            new() { Kind = kind, Index = index, Value = unk, setArgument = set };

        int type = Fxr1Model.TypeId(n);
        switch (n)
        {
            case null:
                return new Fxr1Expression { Kind = FxExpressionKind.Unknown, Index = -1 };
            case IntNode111 c:
                return new Fxr1Expression { Kind = FxExpressionKind.Constant, Value = c.Value, node = n, replace = replace, setValue = v => c.Value = v };
            case EmptyNode112: return Leaf(FxExpressionKind.CurrentTick);
            case EmptyNode113: return Leaf(FxExpressionKind.TotalTick);
            case UnkIndexedValueNode114 a:
                return new Fxr1Expression { Kind = FxExpressionKind.ArgumentA, Index = a.Index, Value = a.Type, node = n, replace = replace,
                    setArgument = (i, u) => { a.Index = (short)i; a.Type = (short)u; } };
            case UnkIndexedValueNode115 a:
                return new Fxr1Expression { Kind = FxExpressionKind.ArgumentB, Index = a.Index, Value = a.Type, node = n, replace = replace,
                    setArgument = (i, u) => { a.Index = (short)i; a.Type = (short)u; } };
            case NodeNode128 u:
                return new Fxr1Expression { Kind = FxExpressionKind.Not, node = n, replace = replace, Operands = new[] { Of(u.Node, x => u.Node = x) } };
            case EmptyNode129: return Leaf(FxExpressionKind.ChildExists);
            case EmptyNode130: return Leaf(FxExpressionKind.ParentExists);
            case EmptyNode131: return Leaf(FxExpressionKind.DistanceFromCamera);
            case EmptyNode132: return Leaf(FxExpressionKind.EmittersStopped);

            // Packed forms, shown as what they stand for
            case EmptyNode136:
                return Fixed(FxExpressionKind.Compare, "==", Fixed(FxExpressionKind.ChildExists), Const(0));
            case FXNode137 p:
            {
                Action<int, int> setB = (i, u) => { p.Unk2 = i; p.Unk1 = u; };
                Action<int, int> setA = (i, u) => { p.Unk3 = i; p.Unk1 = u; };
                var time = Fixed(FxExpressionKind.And, "&&",
                    Fixed(FxExpressionKind.Compare, ">=", Fixed(FxExpressionKind.TotalTick), Arg(FxExpressionKind.ArgumentB, p.Unk2, p.Unk1, setB)),
                    Fixed(FxExpressionKind.Compare, ">=", Arg(FxExpressionKind.ArgumentB, p.Unk2, p.Unk1, setB), Const(0)));
                var orphan = Fixed(FxExpressionKind.And, "&&",
                    Arg(FxExpressionKind.ArgumentA, p.Unk3, p.Unk1, setA),
                    Fixed(FxExpressionKind.Not, null, Fixed(FxExpressionKind.ParentExists)));
                return Fixed(FxExpressionKind.Or, "||", time, orphan);
            }
            case FXNode138 p:
                return Fixed(FxExpressionKind.Compare, "<", Fixed(FxExpressionKind.DistanceFromCamera),
                    Arg(FxExpressionKind.ArgumentA, p.Unk1, 1, (i, _) => p.Unk1 = i));
            case FXNode139 p:
                return Fixed(FxExpressionKind.Compare, ">", Fixed(FxExpressionKind.DistanceFromCamera),
                    Arg(FxExpressionKind.ArgumentA, p.Unk1, 1, (i, _) => p.Unk1 = i));
            case FXNode140 p:
                return Fixed(FxExpressionKind.Or, "||",
                    Fixed(FxExpressionKind.Compare, "<", Fixed(FxExpressionKind.DistanceFromCamera), Arg(FxExpressionKind.ArgumentA, p.Unk1, 1, (i, _) => p.Unk1 = i)),
                    Fixed(FxExpressionKind.Compare, ">", Fixed(FxExpressionKind.DistanceFromCamera), Arg(FxExpressionKind.ArgumentA, p.Unk2, 1, (i, _) => p.Unk2 = i)));
        }

        if (OperatorOf.TryGetValue(type, out var op))
        {
            var (left, right) = Children(n);
            var kind = type == 126 ? FxExpressionKind.And : type == 127 ? FxExpressionKind.Or : FxExpressionKind.Compare;
            return new Fxr1Expression
            {
                Kind = kind,
                Operator = op,
                node = n,
                replace = replace,
                Operands = new[] { Of(left.Get(), left.Set), Of(right.Get(), right.Set) },
            };
        }
        return new Fxr1Expression { Kind = FxExpressionKind.Unknown, Index = type, node = n, replace = replace };
    }


    /// <summary>The two operand fields of a binary condition node (120–127).</summary>
    private static ((Func<FXNode> Get, Action<FXNode> Set) Left, (Func<FXNode> Get, Action<FXNode> Set) Right) Children(FXNode n)
    {
        var t = n.GetType();
        var f1 = t.GetField("Node1");
        var f2 = t.GetField("Node2");
        return ((() => (FXNode)f1.GetValue(n), x => f1.SetValue(n, x)), (() => (FXNode)f2.GetValue(n), x => f2.SetValue(n, x)));
    }

    // ---------------------------------------------------------------- editing in place

    public IReadOnlyList<string> Operators => node == null ? Array.Empty<string>() : Kind switch
    {
        FxExpressionKind.Compare => CompareOperators,
        FxExpressionKind.And or FxExpressionKind.Or => LogicOperators,
        _ => Array.Empty<string>(),
    };

    public void SetOperator(string op)
    {
        if (node == null || op == Operator || !Operators.Contains(op))
            return;
        FXNode swapped = op switch
        {
            ">=" => new Node2Node120(),
            ">" => new Node2Node121(),
            "<=" => new Node2Node122(),
            "<" => new Node2Node123(),
            "==" => new Node2Node124(),
            "!=" => new Node2Node125(),
            "&&" => new Node2Node126(),
            "||" => new Node2Node127(),
            _ => null,
        };
        if (swapped == null)
            return;
        var from = Children(node);
        var to = Children(swapped);
        to.Left.Set(from.Left.Get());
        to.Right.Set(from.Right.Get());
        replace(swapped);
    }

    public void SetValue(int value) => setValue?.Invoke(value);

    public void SetArgument(int index, int unk) => setArgument?.Invoke(index, unk);
}

/// <summary>
/// A part of a packed condition form (136–140) with nothing of its own in the file: its operator, operands and
/// constants are fixed by the node type, so it isn't editable (only the argument leaves inside it are).
/// </summary>
public sealed class Fxr1FixedExpression : IFxExpression
{
    public Fxr1FixedExpression(FxExpressionKind kind, string op, double? value, params IFxExpression[] operands)
    {
        Kind = kind;
        Operator = op;
        Value = value;
        Operands = operands ?? Array.Empty<IFxExpression>();
    }

    public FxExpressionKind Kind { get; }
    public string Operator { get; }
    public double? Value { get; }
    public int Index => 0;
    public IReadOnlyList<IFxExpression> Operands { get; }
}

/// <summary>
/// A state action, an action call, or a root's container: an action id and a container whose param nodes are the
/// values, named by the schema's DS1 / DS1R layout.
/// </summary>
public class Fxr1ActionAdapter : IFxAction, IFxSnapshot
{
    protected readonly Fxr1EffectAdapter Owner;
    private readonly object source;
    private readonly FfxActionDef def;

    public FXContainer Container { get; }

    internal Fxr1ActionAdapter(Fxr1EffectAdapter owner, int typeId, FXContainer container, object source, FfxActionDef def)
    {
        Owner = owner;
        TypeId = typeId;
        Container = container;
        this.source = source;
        this.def = def;
    }

    public int TypeId { get; }
    public object Source => source;

    /// <summary>The layout for this action's current node count (its per-count tail when it has one, FFX Schema Gaps Plan S7c).</summary>
    public FfxActionLayout Layout => def != null && def.Games.TryGetValue(Owner.Game, out var l) ? l.ForCount(Container?.FXNodes?.Count ?? 0) : null;
    public FfxParamDef ParamDef(int index) => Layout is { } l && index < l.Params.Count ? l.Params[index] : null;

    public virtual string Name => def?.Name;
    public virtual string Slot => string.IsNullOrEmpty(def?.Category) ? null : def.Category;
    public bool IsAppearance => Slot == "Appearance";
    public virtual string Description => def?.Desc;
    public bool HasLayout => Layout != null;

    private IReadOnlyList<FXNode> Nodes => (IReadOnlyList<FXNode>)Container?.FXNodes ?? Array.Empty<FXNode>();

    /// <summary>The schema has this layout's node count, and every node's type is one seen in its slot.</summary>
    public bool IsTyped
    {
        get
        {
            var l = Layout;
            var nodes = Nodes;
            if (l == null || Array.IndexOf(l.ParamCounts, nodes.Count) < 0)
                return false;
            for (int i = 0; i < nodes.Count; i++)
                if (i >= l.Params.Count || Array.IndexOf(l.Params[i].Types, Fxr1Model.TypeId(nodes[i])) < 0)
                    return false;
            return true;
        }
    }

    public IReadOnlyList<IFxValue> Values
    {
        get
        {
            var nodes = Nodes;
            var result = new List<IFxValue>(nodes.Count);
            for (int i = 0; i < nodes.Count; i++)
                result.Add(Owner.Value(this, nodes[i], i));
            return result;
        }
    }

    /// <summary>The container's action and effect call nodes, in node order, then its ActionData and modifier.</summary>
    public IReadOnlyList<IFxAction> Calls
    {
        get
        {
            var result = new List<IFxAction>();
            foreach (var n in Nodes)
            {
                if (n is ActionCallNode a) result.Add(Owner.ActionCall(a));
                else if (n is EffectCallNode e) result.Add(Owner.EffectCall(e));
            }
            if (Container?.ActionData != null)
                result.Add(Owner.Packed(Container.ActionData));
            if (Container?.Modifier != null)
                result.Add(Owner.Packed(Container.Modifier));
            return result;
        }
    }

    // ---------------------------------------------------------------- IFxSnapshot
    // Value edits never change which nodes the container holds, so its state is each node's fields (call nodes keep
    // their container, which is its own action). Restoring puts them back exactly.

    private sealed record ContainerState(FXContainer Container, List<FXNode> Nodes, List<Fxr1State.Slot> Fields);

    public object Snapshot() => new ContainerState(Container, Container?.FXNodes.ToList(), Fxr1State.Capture(Nodes));

    public void Restore(object state)
    {
        if (state is not ContainerState s || !ReferenceEquals(s.Container, Container))
            throw new ArgumentException("Not a snapshot of this FXR1 action.");
        if (s.Nodes != null && !s.Nodes.SequenceEqual(Container.FXNodes))
            throw new InvalidOperationException("The action's nodes changed since the snapshot.");
        Fxr1State.Restore(s.Fields);
    }
}

/// <summary>Container1 or Container2 of an effect root.</summary>
public sealed class Fxr1EffectParamsAdapter : Fxr1ActionAdapter, IFxEffectParams
{
    internal Fxr1EffectParamsAdapter(Fxr1EffectAdapter owner, int index, FXContainer container, FfxActionDef def)
        : base(owner, FfxModel.EffectParamsType(index), container, container, def)
    {
        ListIndex = index;
    }

    public int ListIndex { get; }
    public override string Name => null;
    public override string Slot => null;
}

/// <summary>An effect call node: the called effect id and the param nodes handed to it.</summary>
public sealed class Fxr1EffectCallAdapter : Fxr1ActionAdapter, IFxProxy
{
    private readonly EffectCallNode call;

    internal Fxr1EffectCallAdapter(Fxr1EffectAdapter owner, EffectCallNode call)
        : base(owner, call.EffectID, call.Container, call, null)
    {
        this.call = call;
    }

    public int TargetEffect => call.EffectID;
    public override string Name => null;
    public override string Slot => null;
    public override string Description => null;
}

/// <summary>
/// A param node. Its name and position come from the action it was last read through (<see cref="At"/>): the adapter
/// is cached by the node, and vanilla never shares a param node between containers.
/// </summary>
public sealed class Fxr1ValueAdapter : IFxValue, IFxCurveValue, IFxListValue, IFxCallValue, IFxArgumentValue, IFxTypedValue, IFxFixedCurveShape, IFxWrappedValue
{
    private readonly Fxr1EffectAdapter owner;
    public FXNode Node { get; }

    private Fxr1ActionAdapter action;
    private int index;

    internal Fxr1ValueAdapter(Fxr1EffectAdapter owner, FXNode node)
    {
        this.owner = owner;
        Node = node;
    }

    internal Fxr1ValueAdapter At(Fxr1ActionAdapter action, int index)
    {
        this.action = action;
        this.index = index;
        return this;
    }

    private FfxParamDef Def => action?.ParamDef(index);

    public string Name => Def?.Name ?? $"param{index:00}";

    public FxValueKind Kind => Node switch
    {
        IntNode or IntNode41 or SoundIDNode68 or SoundIDNode69 or IntNode111 or TextureNode40 => FxValueKind.Int,
        FloatNode or TickNode => FxValueKind.Float,
        Int2Node or Float2Node or Tick2Node or ColorNode15 or Color2Node83 => FxValueKind.Vector,
        IntArrayNode => FxValueKind.IntList,
        IntSequenceNode3 or IntSequenceNode5 or IntSequenceNode6 or IntSequenceNode9 or IntSequenceNode89
            or FloatSequenceNode11 or FloatSequenceNode12 or FloatSequenceNodeNode91 or FloatSequenceNodeNode95
            or Float3SequenceNode13 or Float3SequenceNode14
            or ColorSequenceNode19 or ColorSequenceNode20 or ColorSequenceNode27 or ColorSequenceNode28
            or Color3SequenceNode21 or Color3SequenceNode22 or Color3SequenceNode29
            // FXR v2 (Bloodborne)
            or IntSequenceNode4 or FloatSequenceNode10 or ColorSequenceNode17 or Color3SequenceNode30
            or RangedFloat3SequenceNode or RangedColor3SequenceNode => FxValueKind.Curve,
        _ => FxValueKind.Other,
    };

    public bool IsColor => Node is ColorSequenceNode19 or ColorSequenceNode20 or ColorSequenceNode27 or ColorSequenceNode28
        or Color3SequenceNode21 or Color3SequenceNode22 or Color3SequenceNode29
        or ColorSequenceNode17 or Color3SequenceNode30 or RangedColor3SequenceNode or ColorNode15;

    public int Components => Node switch
    {
        Int2Node or Float2Node or Tick2Node => 2,
        Color2Node83 => 8,
        _ when IsColor => 4,
        _ => 1,
    };

    public bool IsUnknown => Name.StartsWith("unk", StringComparison.Ordinal) || Name.StartsWith("param", StringComparison.Ordinal);
    public bool IsStored => true;
    public string Description => Def?.Desc;
    public string Enum => FfxEnums.Name(owner.Schema, Def);
    public string Resource => Node is TextureNode40 ? "texture" : null;
    public string TextureType => null;
    public IReadOnlyList<FxEnumMember> EnumMembers => FfxEnums.Members(owner.Schema, Def);
    public string Argument => null;
    public bool IsConverted => false;

    // ---------------------------------------------------------------- IFxWrappedValue (v2 82 / 84)

    public IFxValue Inner => Node switch
    {
        FloatWrapNode82 w when w.Node != null => owner.Value(action, w.Node, index),
        ColorWrapNode84 w when w.Node != null => owner.Value(action, w.Node, index),
        _ => null,
    };

    public IReadOnlyList<double> Factor => Node switch
    {
        FloatWrapNode82 w => new double[] { w.Value },
        ColorWrapNode84 w => new double[] { w.R, w.G, w.B, w.A },
        _ => Array.Empty<double>(),
    };

    // ---------------------------------------------------------------- IFxTypedValue

    public int FileType => Fxr1Model.TypeId(Node);
    public string TypeName => owner.Schema.ValueType(FileType, owner.Game)?.Name;

    // ---------------------------------------------------------------- constants

    public double[] Get() => Node switch
    {
        IntNode n => new double[] { n.Value },
        IntNode41 n => new double[] { n.Value },
        SoundIDNode68 n => new double[] { n.SoundID },
        SoundIDNode69 n => new double[] { n.SoundID },
        IntNode111 n => new double[] { n.Value },
        FloatNode n => new double[] { n.Value },
        TickNode n => new double[] { n.Tick },
        Int2Node n => new double[] { n.X, n.Y },
        Float2Node n => new double[] { n.X, n.Y },
        Tick2Node n => new double[] { n.Tick1, n.Tick2 },
        TextureNode40 n => new double[] { n.TextureID },
        ColorNode15 n => new double[] { n.R, n.G, n.B, n.A },
        Color2Node83 n => n.Values.Select(v => (double)v).ToArray(),
        _ => null,
    };

    public void Set(double[] display)
    {
        var current = Get();
        if (current == null || display == null || display.Length != current.Length)
            return;
        bool Changed(int i) => display[i] != current[i];
        if (!Enumerable.Range(0, current.Length).Any(Changed))
            return;
        int I(int i, int old) => Changed(i) ? (int)Math.Round(display[i]) : old;
        float F(int i, float old) => Changed(i) ? (float)display[i] : old;
        switch (Node)
        {
            case IntNode n: n.Value = I(0, n.Value); break;
            case IntNode41 n: n.Value = I(0, n.Value); break;
            case SoundIDNode68 n: n.SoundID = I(0, n.SoundID); break;
            case SoundIDNode69 n: n.SoundID = I(0, n.SoundID); break;
            case IntNode111 n: n.Value = I(0, n.Value); break;
            case FloatNode n: n.Value = F(0, n.Value); break;
            case TickNode n: n.Tick = F(0, n.Tick); break;
            case Int2Node n: n.X = I(0, n.X); n.Y = I(1, n.Y); break;
            case Float2Node n: n.X = F(0, n.X); n.Y = F(1, n.Y); break;
            case Tick2Node n: n.Tick1 = F(0, n.Tick1); n.Tick2 = F(1, n.Tick2); break;
            case TextureNode40 n: n.TextureID = I(0, n.TextureID); break;
            case ColorNode15 n: n.R = F(0, n.R); n.G = F(1, n.G); n.B = F(2, n.B); n.A = F(3, n.A); break;
            case Color2Node83 n: for (int i = 0; i < 8; i++) n.Values[i] = F(i, n.Values[i]); break;
        }
    }

    // ---------------------------------------------------------------- curves (sequences)

    public float[] DisplayFactors => new[] { 1f, 1f, 1f, 1f };
    public IReadOnlyList<int> ModifierTypes => Array.Empty<int>();

    private object Ticks => Node.GetType().GetField("Ticks")?.GetValue(Node);

    public FxrCurve GetCurve() => Kind == FxValueKind.Curve ? Fxr1Curves.Get(Ticks, Fxr1Model.NodeSequenceKind(FileType)) : null;

    public void SetCurve(FxrCurve curve)
    {
        if (Kind != FxValueKind.Curve)
            return;
        Fxr1Curves.Set(Ticks, Fxr1Model.NodeSequenceKind(FileType), curve);
    }

    // ---------------------------------------------------------------- int arrays

    public List<int> GetList() => Node is IntArrayNode n ? new List<int>(n.Values) : null;

    public void SetList(List<int> values)
    {
        if (Node is not IntArrayNode n || values == null || n.Values.SequenceEqual(values))
            return;
        n.Values = new List<int>(values);
    }

    // ---------------------------------------------------------------- calls, argument refs

    public IFxAction Call => Node switch
    {
        ActionCallNode c => owner.ActionCall(c),
        EffectCallNode c => owner.EffectCall(c),
        _ => null,
    };

    private (short Type, short Index)? Indexed => Node switch
    {
        IndexedIntNode n => (n.Type, n.Index),
        IndexedIntArrayNode n => (n.Type, n.Index),
        IndexedIntSequenceNode n => (n.Type, n.Index),
        IndexedFloatNode n => (n.Type, n.Index),
        IndexedEffectNode n => (n.Type, n.Index),
        IndexedActionNode n => (n.Type, n.Index),
        IndexedSoundIDNode n => (n.Type, n.Index),
        IndexedTickNode n => (n.Type, n.Index),
        IndexedTick2Node n => (n.Type, n.Index),
        UnkIndexedValueNode114 n => (n.Type, n.Index),
        UnkIndexedValueNode115 n => (n.Type, n.Index),
        _ => null,
    };

    public int ArgumentIndex => Indexed?.Index ?? -1;
    public IReadOnlyList<int> Extra => Indexed is { } x ? new[] { (int)x.Type } : Array.Empty<int>();

    /// <summary>True when the node is an argument ref (<see cref="ArgumentIndex"/> is meaningful).</summary>
    public bool IsArgument => Indexed != null;
}

/// <summary>Sequences (FXR1 tick lists) as curves, both for param nodes and ActionData fields.</summary>
internal static class Fxr1Curves
{
    public static FxrCurve Get(object ticks, (FxrPropertyFunction Function, bool Loop) kind)
    {
        var curve = new FxrCurve { Function = kind.Function, Loop = kind.Loop, ValueType = FxrValueType.Scalar };
        switch (ticks)
        {
            case List<IntTick> l: foreach (var k in l) curve.Keyframes.Add(Key(k.Time, k.Value)); break;
            case List<FloatTick> l: foreach (var k in l) curve.Keyframes.Add(Key(k.Time, k.Value)); break;
            case List<Float3Tick> l:
                foreach (var k in l)
                    curve.Keyframes.Add(new FxrKeyframe { Position = k.Time, Value = new[] { k.X }, In = new[] { k.Y }, Out = new[] { k.Z } });
                break;
            case List<ColorTick> l:
                curve.ValueType = FxrValueType.Vector4;
                foreach (var k in l)
                    curve.Keyframes.Add(new FxrKeyframe { Position = k.Time, Value = new[] { k.R, k.G, k.B, k.A } });
                break;
            case List<Color3Tick> l:
                curve.ValueType = FxrValueType.Vector4;
                foreach (var k in l)
                    curve.Keyframes.Add(new FxrKeyframe
                    {
                        Position = k.Time,
                        Value = new[] { k.R1, k.G1, k.B1, k.A1 },
                        In = new[] { k.R2, k.G2, k.B2, k.A2 },
                        Out = new[] { k.R3, k.G3, k.B3, k.A3 },
                    });
                break;
            default:
                return null;
        }
        return curve;

        static FxrKeyframe Key(float t, float v) => new() { Position = t, Value = new[] { v } };
    }

    /// <summary>
    /// Writes the curve's keys into the tick list. Only keys are stored (the node or field type fixes interpolation and
    /// loop), so a curve with another function or loop flag is refused. An unchanged curve changes nothing.
    /// </summary>
    public static void Set(object ticks, (FxrPropertyFunction Function, bool Loop) kind, FxrCurve curve)
    {
        var current = Get(ticks, kind);
        if (current == null || curve == null || FxrModel.CurveEquals(current, curve))
            return;
        if (curve.Function != current.Function || curve.Loop != current.Loop || curve.Keyframes == null)
            return;

        var keys = curve.Keyframes;
        float V(int i, int c) => keys[i].Value != null && c < keys[i].Value.Length ? keys[i].Value[c] : 0f;
        float In(int i, int c) => keys[i].In != null && c < keys[i].In.Length ? keys[i].In[c] : 0f;
        float Out(int i, int c) => keys[i].Out != null && c < keys[i].Out.Length ? keys[i].Out[c] : 0f;

        // Keys that exist are updated in place; extra keys are added
        switch (ticks)
        {
            case List<IntTick> l:
                Resize(l, keys.Count);
                for (int i = 0; i < keys.Count; i++) { l[i].Time = keys[i].Position; l[i].Value = (int)Math.Round(V(i, 0)); }
                break;
            case List<FloatTick> l:
                Resize(l, keys.Count);
                for (int i = 0; i < keys.Count; i++) { l[i].Time = keys[i].Position; l[i].Value = V(i, 0); }
                break;
            case List<Float3Tick> l:
                Resize(l, keys.Count);
                for (int i = 0; i < keys.Count; i++) { l[i].Time = keys[i].Position; l[i].X = V(i, 0); l[i].Y = In(i, 0); l[i].Z = Out(i, 0); }
                break;
            case List<ColorTick> l:
                Resize(l, keys.Count);
                for (int i = 0; i < keys.Count; i++)
                {
                    var k = l[i];
                    k.Time = keys[i].Position; k.R = V(i, 0); k.G = V(i, 1); k.B = V(i, 2); k.A = V(i, 3);
                }
                break;
            case List<Color3Tick> l:
                Resize(l, keys.Count);
                for (int i = 0; i < keys.Count; i++)
                {
                    var k = l[i];
                    k.Time = keys[i].Position;
                    k.R1 = V(i, 0); k.G1 = V(i, 1); k.B1 = V(i, 2); k.A1 = V(i, 3);
                    k.R2 = In(i, 0); k.G2 = In(i, 1); k.B2 = In(i, 2); k.A2 = In(i, 3);
                    k.R3 = Out(i, 0); k.G3 = Out(i, 1); k.B3 = Out(i, 2); k.A3 = Out(i, 3);
                }
                break;
        }
    }

    private static void Resize<T>(List<T> list, int count) where T : new()
    {
        while (list.Count > count) list.RemoveAt(list.Count - 1);
        while (list.Count < count) list.Add(new T());
    }
}

/// <summary>
/// An ActionData or a container modifier: a fixed-layout record whose fields are its values, read by reflection over
/// the port's public fields in declaration order. FXField fields become a value per part (a constant, a curve and its
/// range / resource index, or a vector); DS1R-only fields show in DS1R files only.
/// </summary>
public sealed class Fxr1PackedAdapter : IFxAction, IFxPacked, IFxSnapshot
{
    private readonly Fxr1EffectAdapter owner;
    private readonly object record;
    private List<IFxValue> values;

    internal Fxr1PackedAdapter(Fxr1EffectAdapter owner, object record)
    {
        this.owner = owner;
        this.record = record;
    }

    public FxPackedKind Kind => record is FXModifier ? FxPackedKind.Modifier : FxPackedKind.ActionData;

    public int TypeId => record switch
    {
        FXActionData d => d.Type,
        FXModifier m => m.ModifierType,
        _ => -1,
    };

    /// <summary>The port's class name without its type suffix (ClusterEmitterCone, SetLifetime), or null when it has none.</summary>
    public string Name
    {
        get
        {
            var n = record.GetType().Name;
            if (record is FXModifier)
            {
                int us = n.IndexOf('_');
                return us >= 0 ? n[(us + 1)..] : null;
            }
            int at = n.IndexOf("ActionData", StringComparison.Ordinal);
            return at > 0 ? n[..at] : null;
        }
    }

    public string Slot => Name switch
    {
        null => null,
        var n when n.Contains("Emitter") => "Emitter",
        var n when n.Contains("Movement") => "Movement",
        var n when n.Contains("Appearance") || n.StartsWith("Particle3D", StringComparison.Ordinal) => "Appearance",
        _ => null,
    };

    public bool IsAppearance => Slot == "Appearance";
    public string Description => null;
    public bool HasLayout => true;
    public bool IsTyped => true;
    public IReadOnlyList<IFxAction> Calls => Array.Empty<IFxAction>();
    public object Source => record;

    public IReadOnlyList<IFxValue> Values => values ??= Fxr1Fields.Build(record, owner.File.Wide);

    // Value edits change field contents only, never which FXField objects are held, so the record's state is every
    // field of it, its FXFields, its DS1R block and its resources; restoring puts each back in place.
    private sealed record RecordState(List<Fxr1State.Slot> Fields);

    public object Snapshot() => new RecordState(Fxr1State.Capture(Fxr1Fields.Parts(record)));

    public void Restore(object state)
    {
        if (state is not RecordState s)
            throw new ArgumentException("Not a snapshot of an FXR1 record.");
        Fxr1State.Restore(s.Fields);
    }
}

/// <summary>Builds the values of an ActionData or modifier.</summary>
internal static class Fxr1Fields
{
    private static readonly HashSet<string> Skipped = new() { "ParentContainer", "Resources", "XID" };

    /// <summary>The record's own public fields, in declaration order.</summary>
    public static IEnumerable<FieldInfo> FieldsOf(Type t) =>
        t.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(f => f.MetadataToken);

    /// <summary>Every object whose fields make up the record's state: itself, its FXFields, DS1R block, resources.</summary>
    public static List<object> Parts(object record)
    {
        var parts = new List<object> { record };
        void Add(object o)
        {
            foreach (var f in FieldsOf(o.GetType()))
            {
                var v = f.GetValue(o);
                if (v is FXField or DS1RExtraNodes)
                {
                    parts.Add(v);
                    if (v is DS1RExtraNodes) Add(v);
                }
            }
        }
        Add(record);
        if (record is FXActionData d && d.Resources != null)
        {
            parts.Add(d.Resources);
            foreach (var r in d.Resources)
            {
                parts.Add(r);
                if (r.Data != null) parts.Add(r.Data);
            }
        }
        return parts;
    }

    public static List<IFxValue> Build(object record, bool wide)
    {
        var result = new List<IFxValue>();
        void Add(object o, string prefix)
        {
            foreach (var f in FieldsOf(o.GetType()))
            {
                if (Skipped.Contains(f.Name) || f.Name.StartsWith("DEBUG", StringComparison.Ordinal))
                    continue;
                if (!wide && f.Name.StartsWith("DS1R", StringComparison.Ordinal))
                    continue;
                var name = prefix + f.Name;
                if (f.FieldType == typeof(int) || f.FieldType == typeof(short) || f.FieldType == typeof(byte) || f.FieldType == typeof(float))
                    result.Add(Fxr1FieldValue.Scalar(name, o, f));
                else if (f.FieldType == typeof(FXField))
                    AddField(name, () => (FXField)f.GetValue(o));
                else if (f.FieldType == typeof(DS1RExtraNodes) && f.GetValue(o) is DS1RExtraNodes extra)
                    Add(extra, "DS1R ");
            }
        }
        void AddField(string name, Func<FXField> get)
        {
            var field = get();
            if (field == null)
                return;
            result.AddRange(Fxr1FieldValue.Of(name, field));
        }

        Add(record, "");
        if (record is FXActionData d && d.Resources != null)
        {
            for (int i = 0; i < d.Resources.Count; i++)
            {
                var r = d.Resources[i];
                result.Add(Fxr1FieldValue.Scalar($"Resource{i} Unk", r, typeof(ResourceEntry).GetField(nameof(ResourceEntry.Unk))));
                if (r.Data != null)
                    result.AddRange(Fxr1FieldValue.Of($"Resource{i}", r.Data));
            }
        }
        return result;
    }
}

/// <summary>One value of an ActionData or modifier: a scalar field, or one part of an FXField.</summary>
public sealed class Fxr1FieldValue : IFxValue, IFxCurveValue, IFxTypedValue, IFxFixedCurveShape
{
    public string Name { get; private init; }
    public FxValueKind Kind { get; private init; }
    public int Components { get; private init; } = 1;
    public string Resource { get; private init; }

    /// <summary>The FXField type id this value is part of, or -1 for a plain field.</summary>
    public int FileType { get; private init; } = -1;

    private Func<double[]> get;
    private Action<double[]> set;
    private Func<object> getTicks;
    private (FxrPropertyFunction, bool) kind;

    private object ticks => getTicks?.Invoke();

    public bool IsUnknown => Name.Contains("Unk", StringComparison.Ordinal);
    public bool IsStored => true;
    public string Description => null;
    public string Enum => null;
    public string TextureType => null;
    public IReadOnlyList<FxEnumMember> EnumMembers => Array.Empty<FxEnumMember>();
    public string Argument => null;
    public bool IsConverted => false;
    public bool IsColor => false;
    public string TypeName => FileType >= 0 ? $"FXField {FileType}" : null;

    public double[] Get() => get?.Invoke();

    public void Set(double[] display)
    {
        var current = Get();
        if (current == null || display == null || display.Length != current.Length || current.SequenceEqual(display))
            return;
        set(display);
    }

    public float[] DisplayFactors => new[] { 1f, 1f, 1f, 1f };
    public IReadOnlyList<int> ModifierTypes => Array.Empty<int>();
    public FxrCurve GetCurve() => ticks == null ? null : Fxr1Curves.Get(ticks, kind);
    public void SetCurve(FxrCurve curve) { if (ticks != null) Fxr1Curves.Set(ticks, kind, curve); }

    private static string ResourceOf(string name) =>
        name.StartsWith("TextureID", StringComparison.Ordinal) ? "texture" : name == "ModelID" ? "model" : null;

    /// <summary>An int / short / byte / float field. A changed component is stored as the field's type.</summary>
    public static Fxr1FieldValue Scalar(string name, object owner, FieldInfo f)
    {
        bool isFloat = f.FieldType == typeof(float);
        return new Fxr1FieldValue
        {
            Name = name,
            Kind = isFloat ? FxValueKind.Float : FxValueKind.Int,
            Resource = ResourceOf(name),
            get = () => new[] { Convert.ToDouble(f.GetValue(owner)) },
            set = d =>
            {
                object v = f.FieldType == typeof(float) ? (float)d[0]
                    : f.FieldType == typeof(short) ? (short)Math.Round(d[0])
                    : f.FieldType == typeof(byte) ? (byte)Math.Round(d[0])
                    : (object)(int)Math.Round(d[0]);
                f.SetValue(owner, v);
            },
        };
    }

    /// <summary>The values of one FXField: by its type, a constant, a curve (plus range / resource), or a vector.</summary>
    public static IEnumerable<Fxr1FieldValue> Of(string name, FXField field)
    {
        var t = field.GetType();
        int type = field.Type;
        var fTicks = t.GetField("Ticks");
        if (fTicks != null)
        {
            yield return new Fxr1FieldValue
            {
                Name = name,
                Kind = FxValueKind.Curve,
                FileType = type,
                getTicks = () => fTicks.GetValue(field),
                kind = Fxr1Model.FieldSequenceKind(type),
            };
            var min = t.GetField("Min");
            var max = t.GetField("Max");
            if (min != null && max != null)
                yield return Floats($"{name} Range", type, field, min, max);
            if (t.GetField("ResourceIndex") is FieldInfo ri)
                yield return Part($"{name} Resource", type, field, ri);
            yield break;
        }
        switch (field)
        {
            case FXField.ConstFloat:
                yield return Part(name, type, field, t.GetField(nameof(FXField.ConstFloat.Value)));
                break;
            case FXField.FieldType25:
                yield return Floats(name, type, field, t.GetField("Base"), t.GetField("Min"), t.GetField("Max"));
                break;
            case FXField.FieldType26:
                yield return Part(name, type, field, t.GetField("Unk1"));
                yield return Part($"{name} Unk", type, field, t.GetField("Unk2"));
                break;
            case FXField.FieldType27:
                yield return Floats(name, type, field, t.GetField("Unk1"), t.GetField("Unk2"), t.GetField("Unk3"));
                yield return Part($"{name} Resource", type, field, t.GetField("ResourceIndex"));
                break;
            default:
                // Empty (28): nothing stored
                yield return new Fxr1FieldValue { Name = name, Kind = FxValueKind.Other, FileType = type };
                break;
        }
    }

    private static Fxr1FieldValue Part(string name, int type, object owner, FieldInfo f)
    {
        var v = Scalar(name, owner, f);
        return new Fxr1FieldValue { Name = v.Name, Kind = v.Kind, Resource = v.Resource, FileType = type, get = v.get, set = v.set };
    }

    /// <summary>Several float fields as one vector; unchanged components keep their stored value.</summary>
    private static Fxr1FieldValue Floats(string name, int type, object owner, params FieldInfo[] fields) => new()
    {
        Name = name,
        Kind = FxValueKind.Vector,
        Components = fields.Length,
        FileType = type,
        get = () => fields.Select(f => (double)(float)f.GetValue(owner)).ToArray(),
        set = d =>
        {
            for (int i = 0; i < fields.Length; i++)
                if ((double)(float)fields[i].GetValue(owner) != d[i])
                    fields[i].SetValue(owner, (float)d[i]);
        },
    };
}

/// <summary>
/// Snapshots of FXR1 objects for undo: every public field of each object (references kept, int/float lists and tick
/// lists copied deeply), put back in place on restore.
/// </summary>
public static class Fxr1State
{
    public sealed record Slot(object Target, FieldInfo Field, object Value);

    public static List<Slot> Capture(IEnumerable<object> targets)
    {
        var slots = new List<Slot>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        foreach (var o in targets)
        {
            if (o == null || !seen.Add(o))
                continue;
            if (o is IList list)
            {
                // A list's own entries (resources): kept by reference
                slots.Add(new Slot(o, null, list.Cast<object>().ToList()));
                continue;
            }
            foreach (var f in Fxr1Fields.FieldsOf(o.GetType()))
                slots.Add(new Slot(o, f, Copy(f.GetValue(o))));
        }
        return slots;
    }

    public static void Restore(List<Slot> slots)
    {
        foreach (var s in slots)
        {
            if (s.Field == null)
            {
                var list = (IList)s.Target;
                list.Clear();
                foreach (var x in (List<object>)s.Value)
                    list.Add(x);
                continue;
            }
            // A list is refilled in place, so whatever holds it keeps seeing the field's list
            var copy = Copy(s.Value);
            if (copy is IList saved && !ReferenceEquals(copy, s.Value) && s.Field.GetValue(s.Target) is IList current && current.GetType() == saved.GetType())
            {
                current.Clear();
                foreach (var x in saved)
                    current.Add(x);
                continue;
            }
            s.Field.SetValue(s.Target, copy);
        }
    }

    /// <summary>Every node of a condition tree (the root first), following FXNode-typed fields.</summary>
    public static IEnumerable<object> ConditionNodes(FXNode root)
    {
        var stack = new Stack<FXNode>();
        if (root != null)
            stack.Push(root);
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (!seen.Add(n))
                continue;
            yield return n;
            foreach (var f in Fxr1Fields.FieldsOf(n.GetType()))
                if (f.GetValue(n) is FXNode child)
                    stack.Push(child);
        }
    }

    // Lists of values or ticks are copied (ticks one by one); everything else is a value or a kept reference
    private static object Copy(object v)
    {
        switch (v)
        {
            case List<int> l: return new List<int>(l);
            case List<float> l: return new List<float>(l);
            case List<FloatTick> l: return l.Select(k => new FloatTick { Time = k.Time, Value = k.Value }).ToList();
            case List<IntTick> l: return l.Select(k => new IntTick { Time = k.Time, Value = k.Value }).ToList();
            case List<Float3Tick> l: return l.Select(k => new Float3Tick { Time = k.Time, X = k.X, Y = k.Y, Z = k.Z }).ToList();
            case List<ColorTick> l: return l.Select(k => new ColorTick { Time = k.Time, R = k.R, G = k.G, B = k.B, A = k.A }).ToList();
            case List<Color3Tick> l:
                return l.Select(k => new Color3Tick
                {
                    Time = k.Time,
                    R1 = k.R1, G1 = k.G1, B1 = k.B1, A1 = k.A1,
                    R2 = k.R2, G2 = k.G2, B2 = k.B2, A2 = k.A2,
                    R3 = k.R3, G3 = k.G3, B3 = k.B3, A3 = k.A3,
                }).ToList();
            default: return v;
        }
    }
}
