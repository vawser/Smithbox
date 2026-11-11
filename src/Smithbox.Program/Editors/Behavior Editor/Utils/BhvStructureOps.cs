using SoulsFormats;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The structure edits themselves, on a BHV with no editor state, so Tools/BhvSurvey `structcheck` runs the same
/// code as the editor. States and animations are only appended; deletes renumber the known references
/// (<see cref="BhvIndexRemap"/>).
/// </summary>
public static class BhvStructureOps
{
    /// <summary>A free state ID: one past the largest (IDs are unique, not == index; P0)</summary>
    public static short NextStateId(BHV b) => (short)(b.States.Count == 0 ? 0 : b.States.Max(s => s.ID) + 1);

    #region States
    /// <summary>Appends an empty state that plays no animation</summary>
    public static void AddState(BHV b)
    {
        var state = new BHV.State(b.StateSize) { ID = NextStateId(b) };

        foreach (var slot in BhvLayout.BodyAnimationSlots(b))
            BitConverter.TryWriteBytes(state.Body.AsSpan(slot), (short)-1);

        b.States.Add(state);
    }

    /// <summary>Appends a deep copy with a new ID; its transitions lead where the original's do</summary>
    public static void DuplicateState(BHV b, int index)
    {
        var copy = CloneState(b.States[index]);
        copy.ID = NextStateId(b);
        b.States.Add(copy);
    }

    /// <summary>Removes a state, drops transitions into it and renumbers. Returns preamble slots set to -1.</summary>
    public static int DeleteState(BHV b, int index)
    {
        b.States.RemoveAt(index);
        return BhvIndexRemap.RemapStates(b, BhvIndexRemap.DeleteMap(index), true);
    }

    public static BHV.State CloneState(BHV.State s) => new(0x10)
    {
        ID = s.ID,
        Kind = s.Kind,
        Body = (byte[])s.Body.Clone(),
        Data = (byte[])s.Data.Clone(),
        Transitions = s.Transitions.Select(CloneTransition).ToList(),
    };
    #endregion

    #region Animations
    /// <summary>Appends a record with no TAE animation and no extra animations</summary>
    public static void AddAnimation(BHV b)
    {
        var record = new byte[b.AnimationSize];

        BitConverter.TryWriteBytes(record.AsSpan(BHV.V12.AnimationId), -1);
        if (b.Version >= 10)
            for (int i = 0; i < 3; i++)
                BitConverter.TryWriteBytes(record.AsSpan(BHV.V12.AnimationExtraIds + 4 * i), -1);

        b.Animations.Add(record);
    }

    public static void DuplicateAnimation(BHV b, int index) => b.Animations.Add((byte[])b.Animations[index].Clone());

    /// <summary>Removes a record and renumbers. Returns state body slots set to -1.</summary>
    public static int DeleteAnimation(BHV b, int index)
    {
        b.Animations.RemoveAt(index);
        return BhvIndexRemap.RemapAnimations(b, BhvIndexRemap.DeleteMap(index));
    }
    #endregion

    #region Transitions
    /// <summary>
    /// Appends a transition to the state itself (skipped by the game until retargeted) with empty params of the
    /// version's plain type: type 3 (0x20) for v10/v12, type 1 (0x1C) before
    /// </summary>
    public static void AddTransition(BHV b, int state, int target = -1)
    {
        var t = new BHV.Transition { TargetStateIndex = target < 0 ? state : target };
        if (b.Version < 10)
        {
            t.Params = new byte[0x1C];
            t.Params[1] = 1;
        }

        b.States[state].Transitions.Add(t);
    }

    /// <summary>Inserts a copy right after the original</summary>
    public static void DuplicateTransition(BHV b, int state, int index)
    {
        var list = b.States[state].Transitions;
        list.Insert(index + 1, CloneTransition(list[index]));
    }

    public static void DeleteTransition(BHV b, int state, int index) => b.States[state].Transitions.RemoveAt(index);

    public static void MoveTransition(BHV b, int state, int from, int to) => Move(b.States[state].Transitions, from, to);

    public static BHV.Transition CloneTransition(BHV.Transition t) => new()
    {
        TargetStateIndex = t.TargetStateIndex,
        Params = (byte[])t.Params.Clone(),
        Conditions = t.Conditions.Select(CloneCondition).ToList(),
    };
    #endregion

    #region Conditions
    /// <summary>Appends a type 0 condition with no data, optional (Unk02 = 1) so it can't fail the transition</summary>
    public static void AddCondition(BHV.Transition t) => t.Conditions.Add(new BHV.Condition { Unk02 = 1 });

    public static void DuplicateCondition(BHV.Transition t, int index) => t.Conditions.Insert(index + 1, CloneCondition(t.Conditions[index]));

    public static void DeleteCondition(BHV.Transition t, int index) => t.Conditions.RemoveAt(index);

    public static void MoveCondition(BHV.Transition t, int from, int to) => Move(t.Conditions, from, to);

    public static BHV.Condition CloneCondition(BHV.Condition c) => new()
    {
        Type = c.Type,
        Unk02 = c.Unk02,
        Unk03 = c.Unk03,
        Unk08 = c.Unk08,
        Unk0B = c.Unk0B,
        Data = (byte[])c.Data.Clone(),
    };
    #endregion

    /// <summary>A copy of a variable-length blob at a new length: the start is kept, new bytes are 0</summary>
    public static byte[] Resized(byte[] current, int length)
    {
        var resized = new byte[length];
        Array.Copy(current, resized, Math.Min(current.Length, length));
        return resized;
    }

    public static void Move<T>(List<T> list, int from, int to)
    {
        var item = list[from];
        list.RemoveAt(from);
        list.Insert(to, item);
    }
}
