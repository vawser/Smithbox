using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// One reversible change to an ESD model. Pure: the editor wraps these in undo actions (Actions/EsdEditAction), and
/// Claude/Tools/EsdSurvey editcheck runs them over every vanilla machine.
/// </summary>
public interface IEsdOp
{
    void Do();
    void Undo();
}

/// <summary>Sets one value (an expression's bytes, a transition's target)</summary>
public class EsdSetOp<T> : IEsdOp
{
    private readonly Func<T> Get;
    private readonly Action<T> Set;
    private readonly T NewValue;
    private T OldValue;

    public EsdSetOp(Func<T> get, Action<T> set, T newValue)
    {
        Get = get;
        Set = set;
        NewValue = newValue;
    }

    public void Do() { OldValue = Get(); Set(NewValue); }
    public void Undo() => Set(OldValue);
}

/// <summary>Inserts an item into a list (commands, conditions)</summary>
public class EsdInsertOp<T> : IEsdOp
{
    private readonly List<T> List;
    private readonly int Index;
    private readonly T Item;

    public EsdInsertOp(List<T> list, int index, T item)
    {
        List = list;
        Index = Math.Clamp(index, 0, list.Count);
        Item = item;
    }

    public void Do() => List.Insert(Index, Item);
    public void Undo() => List.RemoveAt(Index);
}

/// <summary>Removes the item at an index of a list</summary>
public class EsdRemoveOp<T> : IEsdOp
{
    private readonly List<T> List;
    private readonly int Index;
    private T Item;

    public EsdRemoveOp(List<T> list, int index)
    {
        List = list;
        Index = index;
    }

    public void Do() { Item = List[Index]; List.RemoveAt(Index); }
    public void Undo() => List.Insert(Index, Item);
}

/// <summary>Moves an item of a list from one index to another</summary>
public class EsdMoveOp<T> : IEsdOp
{
    private readonly List<T> List;
    private readonly int From;
    private readonly int To;

    public EsdMoveOp(List<T> list, int from, int to)
    {
        List = list;
        From = from;
        To = to;
    }

    public void Do() { var item = List[From]; List.RemoveAt(From); List.Insert(To, item); }
    public void Undo() { var item = List[To]; List.RemoveAt(To); List.Insert(From, item); }
}

/// <summary>Replaces the item at an index (used to make a shared condition this place's own copy)</summary>
public class EsdReplaceOp<T> : IEsdOp
{
    private readonly List<T> List;
    private readonly int Index;
    private readonly T Item;
    private T Old;

    public EsdReplaceOp(List<T> list, int index, T item)
    {
        List = list;
        Index = index;
        Item = item;
    }

    public void Do() { Old = List[Index]; List[Index] = Item; }
    public void Undo() => List[Index] = Old;
}

/// <summary>
/// Adds or removes a state of a machine. The machine's dictionary is rebuilt in place in its old order (plus or minus
/// the state), because the writer writes states in dictionary order and a .NET Dictionary can reuse a removed entry's
/// slot: an undo must give back the exact order, or the file would no longer save byte-identical.
/// </summary>
public class EsdStateOp : IEsdOp
{
    private readonly Dictionary<long, ESD.State> Machine;
    private readonly long Id;
    private readonly ESD.State State;
    private readonly bool Add;
    private readonly long? AfterId;
    private List<KeyValuePair<long, ESD.State>> Before;

    /// <param name="afterId">For an add: the state the new one goes after in write order; null puts it last</param>
    private EsdStateOp(Dictionary<long, ESD.State> machine, long id, ESD.State state, bool add, long? afterId)
    {
        Machine = machine;
        Id = id;
        State = state;
        Add = add;
        AfterId = afterId;
    }

    public static EsdStateOp AddState(Dictionary<long, ESD.State> machine, long id, ESD.State state, long? afterId = null) => new(machine, id, state, true, afterId);
    public static EsdStateOp RemoveState(Dictionary<long, ESD.State> machine, long id) => new(machine, id, null, false, null);

    public void Do()
    {
        Before = Machine.ToList();
        var after = new List<KeyValuePair<long, ESD.State>>();

        if (Add)
        {
            var inserted = false;
            foreach (var kv in Before)
            {
                after.Add(kv);
                if (AfterId == kv.Key)
                {
                    after.Add(new(Id, State));
                    inserted = true;
                }
            }
            if (!inserted)
                after.Add(new(Id, State));
        }
        else
        {
            after.AddRange(Before.Where(kv => kv.Key != Id));
        }

        Rebuild(after);
    }

    public void Undo() => Rebuild(Before);

    private void Rebuild(List<KeyValuePair<long, ESD.State>> entries)
    {
        Machine.Clear();
        Machine.TrimExcess();
        foreach (var kv in entries)
            Machine.Add(kv.Key, kv.Value);
    }
}

/// <summary>Several ops as one: done in order, undone in reverse</summary>
public class EsdCompoundOp : IEsdOp
{
    private readonly List<IEsdOp> Ops;

    public EsdCompoundOp(List<IEsdOp> ops)
    {
        Ops = ops;
    }

    public void Do() { foreach (var op in Ops) op.Do(); }
    public void Undo() { for (var i = Ops.Count - 1; i >= 0; i--) Ops[i].Undo(); }
}

/// <summary>
/// Building blocks for edits: copies, new commands and conditions, literal changes in expressions.
/// </summary>
public static class EsdEdits
{
    public static byte[] Copy(byte[] b) => b == null ? Array.Empty<byte>() : (byte[])b.Clone();

    public static ESD.CommandCall CloneCommand(ESD.CommandCall c) =>
        new(c.CommandBank, c.CommandID, c.Arguments.Select(Copy).ToArray());

    /// <summary>A deep copy: its subconditions are copied too, so the copy shares nothing with the original</summary>
    public static ESD.Condition CloneCondition(ESD.Condition c)
    {
        var map = new Dictionary<ESD.Condition, ESD.Condition>(ReferenceEqualityComparer.Instance);
        return CloneCondition(c, map);
    }

    private static ESD.Condition CloneCondition(ESD.Condition c, Dictionary<ESD.Condition, ESD.Condition> map)
    {
        // Within one copy, a condition shared by two parents stays shared (the copy has the original's shape)
        if (map.TryGetValue(c, out var done))
            return done;

        var copy = new ESD.Condition
        {
            TargetState = c.TargetState,
            Evaluator = Copy(c.Evaluator),
        };
        map[c] = copy;
        copy.PassCommands.AddRange(c.PassCommands.Select(CloneCommand));
        foreach (var s in c.Subconditions)
            copy.Subconditions.Add(CloneCondition(s, map));
        return copy;
    }

    public static ESD.State CloneState(ESD.State s)
    {
        var map = new Dictionary<ESD.Condition, ESD.Condition>(ReferenceEqualityComparer.Instance);
        var copy = new ESD.State();
        copy.EntryCommands.AddRange(s.EntryCommands.Select(CloneCommand));
        copy.WhileCommands.AddRange(s.WhileCommands.Select(CloneCommand));
        copy.ExitCommands.AddRange(s.ExitCommands.Select(CloneCommand));
        foreach (var c in s.Conditions)
            copy.Conditions.Add(CloneCondition(c, map));
        return copy;
    }

    /// <summary>"never": the constant 0, a transition that is never taken (a safe default for a new one)</summary>
    public static byte[] Never => EsdExpr.Encode(EsdExprNode.Integer(0));

    /// <summary>"always": the constant 1</summary>
    public static byte[] Always => EsdExpr.Encode(EsdExprNode.Integer(1));

    /// <summary>A literal for a documented arg type: 0, 0.0f, "" (the defaults a new command gets)</summary>
    public static byte[] DefaultArg(string type)
    {
        var node = type switch
        {
            "float" => new EsdExprNode { Kind = EsdExprKind.Float, Op = 0x80, FloatValue = 0 },
            "string" => new EsdExprNode { Kind = EsdExprKind.String, Op = 0xA5, StringValue = "" },
            _ => EsdExprNode.Integer(0)
        };
        return EsdExpr.Encode(node);
    }

    /// <summary>The next unused state id of a machine (one past the largest)</summary>
    public static long NextStateId(Dictionary<long, ESD.State> machine) => machine.Count == 0 ? 0 : machine.Keys.Max() + 1;

    /// <summary>The node at a path of child indexes from the root, or null</summary>
    public static EsdExprNode At(EsdExprNode root, IReadOnlyList<int> path)
    {
        var n = root;
        foreach (var i in path)
        {
            if (n == null || i < 0 || i >= n.Children.Count)
                return null;
            n = n.Children[i];
        }
        return n;
    }

    /// <summary>
    /// The expression with the literal at a path replaced. An int keeps its encoding when the new value fits it (a
    /// one-byte literal holds -64..63), else uses the four-byte form. Null if the path isn't a literal of that kind.
    /// </summary>
    public static byte[] SetLiteral(byte[] code, IReadOnlyList<int> path, long? intValue = null, double? floatValue = null, string stringValue = null)
    {
        var root = EsdExpr.Decode(code);
        var n = root != null ? At(root, path) : null;
        if (n == null)
            return null;

        switch (n.Kind)
        {
            case EsdExprKind.Int when intValue is long v:
                if (v < int.MinValue || v > int.MaxValue)
                    return null;
                n.IntValue = v;
                if (n.Op < 0x80 && (v < -64 || v > 63))
                    n.Op = 0x82;
                else if (n.Op < 0x80)
                    n.Op = (byte)(v + 64);
                break;
            case EsdExprKind.Float when floatValue is double f:
                n.FloatValue = (float)f;
                break;
            case EsdExprKind.Double when floatValue is double d:
                n.FloatValue = d;
                break;
            case EsdExprKind.String when stringValue != null:
                n.StringValue = stringValue;
                break;
            default:
                return null;
        }

        return EsdExpr.Encode(root);
    }

    /// <summary>
    /// Every editable literal of an expression, with its path. A call's function id and a machine-arg index aren't
    /// values (changing them changes what is called or read), so they're left out.
    /// </summary>
    public static IEnumerable<(EsdExprNode Node, List<int> Path)> Literals(EsdExprNode root)
    {
        var stack = new Stack<(EsdExprNode, List<int>)>();
        stack.Push((root, new List<int>()));
        while (stack.Count > 0)
        {
            var (n, p) = stack.Pop();
            if (n.IsLiteral)
                yield return (n, p);
            if (n.Kind == EsdExprKind.Unary && n.Op == 0xB8)
                continue;
            var first = n.Kind == EsdExprKind.Call ? 1 : 0;
            for (var i = n.Children.Count - 1; i >= first; i--)
                stack.Push((n.Children[i], new List<int>(p) { i }));
        }
    }
}
