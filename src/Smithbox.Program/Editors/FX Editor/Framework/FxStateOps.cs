using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// State edits that keep every reference to a state pointing at the same state: condition targets
/// (<see cref="FxrStateCondition.TargetState"/>, -1 = end the effect) and each node's StateConfigMap (one config
/// index per state; a missing entry reads as entry 0). Run inside an <see cref="FxStatesAction"/> for undo.
/// </summary>
public static class FxStateOps
{
    /// <summary>Every StateConfigMap list in the effect (action 199's first Section10 list).</summary>
    public static IEnumerable<List<int>> Maps(FXR fxr)
    {
        // Only SoulsFormats here, so the check tool can compile this file on its own
        foreach (var node in FXR.Walk(fxr.Root))
            foreach (var a in node.Actions)
                if (a.Type == 199 && a.Section10s.Count > 0)
                {
                    yield return a.Section10s[0];
                    break;
                }
    }

    public static IEnumerable<FxrStateCondition> Conditions(FXR fxr) => fxr.States.SelectMany(s => s.Conditions);

    /// <summary>Adds an empty state at the end. Each map gets the entry a missing one would read as (entry 0).</summary>
    public static int Add(FXR fxr)
    {
        int index = fxr.States.Count;
        fxr.States.Add(new FxrState());
        foreach (var map in Maps(fxr))
            if (map.Count == index)
                map.Add(map.Count > 0 ? map[0] : 0);
        return index;
    }

    /// <summary>Puts a copy of state <paramref name="index"/> just after it, using the same configs.</summary>
    public static int Duplicate(FXR fxr, int index)
    {
        int at = index + 1;
        Shift(fxr, at, +1);
        fxr.States.Insert(at, FxClone.State(fxr.States[index]));
        foreach (var map in Maps(fxr))
            if (map.Count > index)
                map.Insert(at, map[index]);
        return at;
    }

    /// <summary>How many conditions move to the state, and how many from it; for the delete confirmation.</summary>
    public static int ConditionsInto(FXR fxr, int index) => Conditions(fxr).Count(c => c.TargetState == index);

    /// <summary>
    /// Removes state <paramref name="index"/>. Conditions that moved to it end the effect instead; later states move
    /// down by one, as do their map entries. The last remaining state can't be removed.
    /// </summary>
    public static bool Delete(FXR fxr, int index)
    {
        if (fxr.States.Count <= 1 || index < 0 || index >= fxr.States.Count)
            return false;

        fxr.States.RemoveAt(index);
        foreach (var c in Conditions(fxr))
        {
            if (c.TargetState == index)
                c.TargetState = -1;
            else if (c.TargetState > index)
                c.TargetState--;
        }
        foreach (var map in Maps(fxr))
            if (map.Count > index)
                map.RemoveAt(index);
        return true;
    }

    /// <summary>Swaps states <paramref name="a"/> and <paramref name="b"/> (state 0 is where the effect starts).</summary>
    public static void Swap(FXR fxr, int a, int b)
    {
        if (a == b || a < 0 || b < 0 || a >= fxr.States.Count || b >= fxr.States.Count)
            return;

        (fxr.States[a], fxr.States[b]) = (fxr.States[b], fxr.States[a]);
        foreach (var c in Conditions(fxr))
        {
            if (c.TargetState == a) c.TargetState = b;
            else if (c.TargetState == b) c.TargetState = a;
        }
        foreach (var map in Maps(fxr))
        {
            // A missing entry reads as entry 0, so fill up to the larger index before swapping
            int need = Math.Max(a, b) + 1;
            if (map.Count == 0)
                continue;
            while (map.Count < need)
                map.Add(map[0]);
            (map[a], map[b]) = (map[b], map[a]);
        }
    }

    /// <summary>Moves every condition target at or after <paramref name="from"/> by <paramref name="by"/>.</summary>
    private static void Shift(FXR fxr, int from, int by)
    {
        foreach (var c in Conditions(fxr))
            if (c.TargetState >= from)
                c.TargetState += by;
    }

    /// <summary>A new condition: after one second in the state, end the effect.</summary>
    public static FxrStateCondition NewCondition() => new()
    {
        Operator = 3, // <=
        Unk1 = 2,
        TargetState = -1,
        Left = new FxrOperand { Type = -4, HasValue = true, FloatValue = 1f },
        Right = new FxrOperand { Type = -1, HasValue = false },
    };

    /// <summary>Sets an operand's kind, with a sensible value for the new kind.</summary>
    public static void SetOperandType(FxrOperand o, short type)
    {
        if (o.Type == type)
            return;
        o.Type = type;
        switch (type)
        {
            case -4: o.HasValue = true; o.FloatValue = 0; break;
            case -3: o.HasValue = true; o.IntValue = 0; break;
            default: o.HasValue = false; o.RawValue = 0; break;
        }
    }
}
